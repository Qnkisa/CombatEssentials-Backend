using CombatEssentials.Application.DTOs.AuthDtos;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.Domain.Entities;
using CombatEssentials.API.Areas.UserControllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Controllers.UserControllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _controller = new AuthController(_authServiceMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk_OnSuccess()
        {
            var dto = new RegisterDto();
            _authServiceMock.Setup(s => s.RegisterAsync(dto)).ReturnsAsync("Registration successful.");
            var result = await _controller.Register(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_OnFailure()
        {
            var dto = new RegisterDto();
            _authServiceMock.Setup(s => s.RegisterAsync(dto)).ReturnsAsync("Email already exists.");
            var result = await _controller.Register(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsOk_OnSuccess()
        {
            var dto = new LoginDto();
            _authServiceMock.Setup(s => s.LoginAsync(dto)).ReturnsAsync("valid.jwt.token");
            var result = await _controller.Login(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_OnFailure()
        {
            var dto = new LoginDto();
            _authServiceMock.Setup(s => s.LoginAsync(dto)).ReturnsAsync("Invalid login credentials.");
            var result = await _controller.Login(dto);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
