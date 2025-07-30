using CombatEssentials.Application.Interfaces;
using CombatEssentials.API.Areas.UserControllers;
using CombatEssentials.Application.DTOs.ProductDtos;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Controllers.UserControllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _serviceMock;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _serviceMock = new Mock<IProductService>();
            _controller = new ProductController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetAllAsync(1, null, null)).ReturnsAsync(new PaginatedProductsDto());
            var result = await _controller.GetAll(1, null, null);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRandom_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetRandomProductsAsync(5)).ReturnsAsync(new[] { new ProductDto() });
            var result = await _controller.GetRandom(5);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsOk_IfFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new ProductDto());
            var result = await _controller.GetById(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_IfNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((ProductDto)null);
            var result = await _controller.GetById(1);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
