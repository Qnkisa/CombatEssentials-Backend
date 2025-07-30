using CombatEssentials.Application.DTOs.ProductDtos;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.API.Areas.AdminControllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Controllers.AdminControllers
{
    public class AdminProductControllerTests
    {
        private readonly Mock<IProductService> _serviceMock;
        private readonly AdminProductController _controller;

        public AdminProductControllerTests()
        {
            _serviceMock = new Mock<IProductService>();
            _controller = new AdminProductController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAllForAdmin_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetAllForAdminAsync(1, null, null)).ReturnsAsync(new PaginatedProductsDto());
            var result = await _controller.GetAllForAdmin(1, null, null);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtRoute_OnSuccess()
        {
            var dto = new CreateProductDto();
            _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync((true, "", new ProductDto()));
            var result = await _controller.Create(dto);
            Assert.IsType<CreatedAtRouteResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsOk_OnSuccess()
        {
            var dto = new UpdateProductDto();
            _serviceMock.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync((true, "Updated"));
            var result = await _controller.Update(1, dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsOk_OnSuccess()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync((true, "Deleted"));
            var result = await _controller.Delete(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Undelete_ReturnsOk_OnSuccess()
        {
            _serviceMock.Setup(s => s.UndeleteAsync(1)).ReturnsAsync((true, "Restored"));
            var result = await _controller.Undelete(1);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
