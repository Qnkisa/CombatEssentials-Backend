using CombatEssentials.Application.DTOs.CartDtos;
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
    public class CartControllerTests
    {
        private readonly Mock<ICartService> _serviceMock;
        private readonly CartController _controller;

        public CartControllerTests()
        {
            _serviceMock = new Mock<ICartService>();
            _controller = new CartController(_serviceMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }

        [Fact]
        public async Task GetCartItems_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetCartItemsAsync("user1")).ReturnsAsync(new[] { new GetCartItemDto() });
            var result = await _controller.GetCartItems();
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddToCart_ReturnsOk_OnSuccess()
        {
            var dto = new CartItemDto();
            _serviceMock.Setup(s => s.AddToCartAsync("user1", dto)).ReturnsAsync((true, "Added"));
            var result = await _controller.AddToCart(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task RemoveFromCart_ReturnsOk_OnSuccess()
        {
            _serviceMock.Setup(s => s.RemoveFromCartAsync("user1", 1)).ReturnsAsync((true, "Removed"));
            var result = await _controller.RemoveFromCart(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ClearCart_ReturnsOk_OnSuccess()
        {
            _serviceMock.Setup(s => s.ClearCartAsync("user1")).ReturnsAsync((true, "Cleared"));
            var result = await _controller.ClearCart();
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateQuantity_ReturnsOk_OnSuccess()
        {
            var dto = new UpdateCartQuantityDto() { Quantity = 2 };
            _serviceMock.Setup(s => s.UpdateQuantityAsync("user1", 1, 2)).ReturnsAsync((true, "Updated"));
            var result = await _controller.UpdateQuantity(1, dto);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
