using CombatEssentials.Application.DTOs.ReviewDtos;
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
    public class ReviewControllerTests
    {
        private readonly Mock<IReviewService> _serviceMock;
        private readonly ReviewController _controller;

        public ReviewControllerTests()
        {
            _serviceMock = new Mock<IReviewService>();
            _controller = new ReviewController(_serviceMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }

        [Fact]
        public async Task GetReviewsForProduct_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetReviewsForProductAsync(1, 1)).ReturnsAsync(new[] { new GetReviewDto() });
            var result = await _controller.GetReviewsForProduct(1, 1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAverageRatingForProduct_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetAverageRatingForProductAsync(1)).ReturnsAsync(4.5);
            var result = await _controller.GetAverageRatingForProduct(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddReview_ReturnsOk()
        {
            var dto = new ReviewDto();
            _serviceMock.Setup(s => s.AddReviewAsync("user1", dto)).Returns(Task.CompletedTask);
            var result = await _controller.AddReview(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteReview_ReturnsOk_OnSuccess()
        {
            _serviceMock.Setup(s => s.DeleteReviewAsync("user1", 1)).ReturnsAsync((true, "Deleted"));
            var result = await _controller.DeleteReview(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteReview_ReturnsNotFound_IfNotFound()
        {
            _serviceMock.Setup(s => s.DeleteReviewAsync("user1", 1)).ReturnsAsync((false, "Review not found."));
            var result = await _controller.DeleteReview(1);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
