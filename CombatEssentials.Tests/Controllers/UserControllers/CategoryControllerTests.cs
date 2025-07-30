using CombatEssentials.Application.Interfaces;
using CombatEssentials.API.Areas.UserControllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Controllers.UserControllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _serviceMock;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _serviceMock = new Mock<ICategoryService>();
            _controller = new CategoryController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new System.Collections.Generic.List<Application.DTOs.CategoryDtos.CategoryDto>());
            var result = await _controller.GetAll();
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
