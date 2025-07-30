using CombatEssentials.Application.Interfaces;
using CombatEssentials.API.Areas.UserControllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Controllers.UserControllers
{
    public class WishlistControllerTests
    {
        private readonly Mock<IWishlistService> _serviceMock;
        private readonly WishlistController _controller;

        public WishlistControllerTests()
        {
            _serviceMock = new Mock<IWishlistService>();
            _controller = new WishlistController(_serviceMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetWishlistAsync("user1", 1)).ReturnsAsync(new Application.DTOs.WishlistDtos.PaginatedWishlistDto());
            var result = await _controller.GetAll(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Add_ReturnsOk_OnSuccess()
        {
            _serviceMock.Setup(s => s.AddToWishlistAsync("user1", 1)).ReturnsAsync((true, "Added"));
            var result = await _controller.Add(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Remove_ReturnsOk_OnSuccess()
        {
            _serviceMock.Setup(s => s.RemoveFromWishlistAsync("user1", 1)).ReturnsAsync((true, "Removed"));
            var result = await _controller.Remove(1);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
