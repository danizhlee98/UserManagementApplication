using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Supabase;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UMA.Models;
using UMA.Models.Dto.Response;
using UMA.Models.Entity;
using UMA.Services;

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly IHttpClientFactory _httpClientFactory;
    private string _bucketName = "profile-picture";
    public UserController(IUserService userService, IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _userService = userService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(UserRequest userRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserResponse
                {
                    Success = false,
                    Message = "Invalid input"
                });
            }

            UserResponse userResponse = await this._userService.AddUserAsync(userRequest);

            if (!userResponse.Success)
            {
                return Ok(userResponse);
            }

            return Ok(userResponse);
        }
        catch
        {
            return BadRequest("Error creating user");
        }
    }

    [Authorize]
    [HttpGet("Edit")]
    public async Task<UserRequest> GetUser(string email)
    {
        var user = await this._userService.GetUserByEmail(email);

        return user;
    }

    private async Task GetToken(string email)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
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

        Response.Cookies.Append("jwt", tokenString, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!))
        });
    }

    [Authorize]
    [HttpPut("Edit")]
    public async Task<UserResponse> Edit(UserRequest userRequest)
    {
        try
        {
            await this._userService.UpdateUserAsync(userRequest);

            return new UserResponse { Message = "User update successfully", Success = true };
        }
        catch
        {
            return new UserResponse { Message = "Error updating user", Success = false };
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadProfilePicture([FromForm] IFormFile file, [FromForm] string? email)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        // Generate a unique filename
        var fileName = $"{email}-{file.FileName}";

        // Upload to Supabase Storage via REST API
        var supabaseUrl = _configuration.GetValue<string>("Supabase:ProjectUrl");
        var supabaseKey = _configuration.GetValue<string>("Supabase:ServiceRoleKey");

        var uploadUrl = $"{supabaseUrl}/storage/v1/object/{_bucketName}/{fileName}";

        var client = _httpClientFactory.CreateClient();
        var content = new ByteArrayContent(fileBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Content = content;
        request.Headers.Add("Authorization", $"Bearer {supabaseKey}");
        request.Headers.Add("apikey", supabaseKey);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, "Upload failed");

        var publicUrl = $"https://{supabaseUrl}/storage/v1/object/profile-pictures/{fileName}";

        return Ok(new { url = publicUrl });
    }

    [Authorize]
    [HttpGet("image/{fileName}")]
    public async Task<IActionResult> GetImage(string fileName)
    {
        try
        {
            var url = _configuration.GetValue<string>("Supabase:ProjectUrl");
            var serviceRoleKey = _configuration.GetValue<string>("Supabase:ServiceRoleKey");

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

            var supabase = new Client(url, serviceRoleKey, options);
            await supabase.InitializeAsync();

            var bucket = supabase.Storage.From(_bucketName);

            var signedUrl = await bucket.CreateSignedUrl(fileName, 60 * 60);

            return Ok(new { url = signedUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

}
