using CombatEssentials.Application.DTOs.OrderDtos;
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
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _serviceMock;
        private readonly OrdersController _controller;

        public OrderControllerTests()
        {
            _serviceMock = new Mock<IOrderService>();
            _controller = new OrdersController(_serviceMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }

        [Fact]
        public async Task GetByUser_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetByUserIdAsync("user1", 1)).ReturnsAsync(new PaginatedOrderDto());
            var result = await _controller.GetByUser(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
            var dto = new CreateOrderDto();
            _serviceMock.Setup(s => s.CreateAsync("user1", dto)).ReturnsAsync(new OrderDto { Id = 1 });
            var result = await _controller.Create(dto);
            Assert.IsType<CreatedAtActionResult>(result);
        }
    }
}
