using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UMA.Models.Dto.Request;
using UMA.Models.Dto.Response;
using UMA.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public AuthController(IConfiguration config, IUserService userService, IAuthService authService)
    {
        _config = config;
        _userService = userService;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<UserResponse> Login([FromBody] LoginRequest request)
    {

        UserResponse userResponse = await this._userService.ValidateUser(request);

        if (!userResponse.Success)
        {
            return userResponse;
        }

        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = _authService.GenerateRefreshToken();

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return new UserResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = tokenString,
            AccessTokenExpire = DateTime.UtcNow.AddMinutes(60)
        };
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token missing" });
        }

        if (refreshToken == null)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
            signingCredentials: creds
        );

        var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponse { Token = newAccessToken, Expiration = DateTime.Now.AddDays(7) });
    }


    [HttpGet("check-cookie")]
    public IActionResult CheckCookie()
    {
        var token = Request.Cookies["jwt"];
        if (!string.IsNullOrEmpty(token))
            return Ok(new { message = "Cookie received", tokenExists = true, token = token });
        else
            return Ok(new { message = "Cookie missing", tokenExists = false });
    }

}
