using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UMA.Models;
using UMA.Models.Entity;
using UMA.Services;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly IHttpClientFactory _httpClientFactory;
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
            await this._userService.AddUserAsync(userRequest);

            return Ok("User created successfully");
        }
        catch
        {
            return BadRequest("Error creating user");
        }
    }

    [HttpGet("Profile")]
    public async Task<User> GetUser(string email)
    {
        var user = await this._userService.GetUserByEmail(email);

        return user;
    }

    // POST: UserController/Edit/5
    [HttpPut("Edit")]
    public async Task<IActionResult> Edit(UserRequest userRequest)
    {
        try
        {
            await this._userService.UpdateUserAsync(userRequest);

            return Ok("User created successfully");
        }
        catch
        {
            return BadRequest("Error updating user");
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadProfilePicture([FromForm] IFormFile file, [FromForm] string firstName)
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
        var fileName = $"{firstName}-{DateTimeOffset.UtcNow}-{file.FileName}";

        // Upload to Supabase Storage via REST API
        var supabaseUrl = _configuration.GetValue<string>("Supabase:ProjectUrl") + fileName;
        var supabaseKey = "YOUR_SERVICE_ROLE_KEY"; // service key from Supabase (server side)

        var client = _httpClientFactory.CreateClient();
        var content = new ByteArrayContent(fileBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        var request = new HttpRequestMessage(HttpMethod.Post, supabaseUrl);
        request.Content = content;
        request.Headers.Add("Authorization", $"Bearer {supabaseKey}");
        request.Headers.Add("apikey", supabaseKey);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, "Upload failed");

        // Construct public URL
        var publicUrl = $"https://your-project.supabase.co/storage/v1/object/public/profile-pictures/{fileName}";

        return Ok(new { url = publicUrl });
    }
}
