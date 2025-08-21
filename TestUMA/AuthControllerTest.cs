using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using UMA.Models.Dto.Request;
using UMA.Models.Dto.Response;
using UMA.Services;

namespace TestUMA
{
    public class AuthControllerTest
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly IConfiguration _config;
        private readonly DefaultHttpContext _httpContext;

        public AuthControllerTest()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAuthService = new Mock<IAuthService>();

            var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Key", "f62365d876bb44c4beddd905546cb8b4"},
            {"Jwt:Issuer", "testIssuer"},
            {"Jwt:Audience", "testAudience"},
            {"Jwt:ExpireMinutes", "30"}
        };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _httpContext = new DefaultHttpContext();
        }


        [Fact]
        public async Task Login_ShouldReturnFailure_WhenEmailInvalid()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "abc123@yahoo.com", Password = "abc123" };

            _mockUserService
                .Setup(s => s.ValidateUser(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new UserResponse { Success = false, Message = "Email not found" });

            var controller = new AuthController(_config, _mockUserService.Object, _mockAuthService.Object)
            {
                ControllerContext = { HttpContext = _httpContext }
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email not found", result.Message);
            Assert.DoesNotContain("jwt", _httpContext.Response.Headers["Set-Cookie"].ToString());
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenPasswordInvalid()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "abc123@gmail.com", Password = "abc1234" };

            _mockUserService
                .Setup(s => s.ValidateUser(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new UserResponse { Success = false, Message = "Wrong Password" });

            var controller = new AuthController(_config, _mockUserService.Object, _mockAuthService.Object)
            {
                ControllerContext = { HttpContext = _httpContext }
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Wrong Password", result.Message);
            Assert.DoesNotContain("jwt", _httpContext.Response.Headers["Set-Cookie"].ToString());
        }

        [Fact]
        public async Task Login_ShouldReturnSuccess_AndSetJwtCookie_WhenValidUser()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "abc123@gmail.com", Password = "abc123" };

            _mockUserService
                .Setup(s => s.ValidateUser(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new UserResponse { Success = true, Message = "Login successful" });

            var controller = new AuthController(_config, _mockUserService.Object, _mockAuthService.Object)
            {
                ControllerContext = { HttpContext = _httpContext }
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Login successful", result.Message);

            // Check JWT cookie exists
            Assert.Contains("jwt", _httpContext.Response.Headers["Set-Cookie"].ToString());
        }
    }
}