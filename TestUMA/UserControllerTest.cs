using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net.Http;
using UMA.Models;
using UMA.Models.Dto.Request;
using UMA.Models.Dto.Response;
using UMA.Services;

namespace TestUMA
{
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClient;
        private readonly DefaultHttpContext _httpContext;

        public UserControllerTest()
        {
            _mockUserService = new Mock<IUserService>();

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

            _httpClient = new Mock<IHttpClientFactory>().Object;
        }


        [Fact]
        public async Task CreateUserWithNonExistEmail()
        {
            // Arrange
            var userRequest = new UserRequest { FirstName = "danizh", LastName = "lee", Email = "danizhlee@gmail.com", Password = "abc123", PathUrl = "Testing" };
            UserResponse userResponse = new UserResponse { Success = true, Message = "User created successfully" };

            _mockUserService
                .Setup(s => s.AddUserAsync(userRequest))
                .ReturnsAsync(userResponse);

            var controller = new UserController(_mockUserService.Object, _httpClient, _config);

            // Act
            var result = await controller.Create(userRequest);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Assert

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(userResponse, okResult.Value);
        }

        [Fact]
        public async Task Create_ReturnFailure_WhenEmailEixst()
        {
            // Arrange
            var userRequest = new UserRequest { FirstName = "danizh", LastName = "lee", Email = "abc123@gmail.com", Password = "asdqwezxc", PathUrl = "Testing" };
            UserResponse userResponse = new UserResponse { Success = false, Message = "Email already exist" };
            _mockUserService
                .Setup(s => s.AddUserAsync(userRequest))
                .ReturnsAsync(userResponse);

            var controller = new UserController(_mockUserService.Object, _httpClient, _config);

            // Act
            var result = await controller.Create(userRequest);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Assert

            var conflict = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(userResponse, conflict.Value);
        }

    }
}