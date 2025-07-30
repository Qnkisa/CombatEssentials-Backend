using CombatEssentials.Application.DTOs.OrderDtos;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.API.Areas.AdminControllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Controllers.AdminControllers
{
    public class AdminOrderControllerTests
    {
        private readonly Mock<IOrderService> _serviceMock;
        private readonly AdminOrderController _controller;

        public AdminOrderControllerTests()
        {
            _serviceMock = new Mock<IOrderService>();
            _controller = new AdminOrderController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetAllAsync(1)).ReturnsAsync(new PaginatedOrderDto());
            var result = await _controller.GetAll(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsOk_OnSuccess()
        {
            var dto = new UpdateOrderDto();
            _serviceMock.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync((true, "Updated"));
            var result = await _controller.Update(1, dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_OnFailure()
        {
            var dto = new UpdateOrderDto();
            _serviceMock.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync((false, "Not found"));
            var result = await _controller.Update(1, dto);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
