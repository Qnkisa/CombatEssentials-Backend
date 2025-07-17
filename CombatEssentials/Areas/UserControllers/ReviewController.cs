using CombatEssentials.Application.DTOs.ReviewDtos;
using CombatEssentials.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CombatEssentials.API.Areas.UserControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // GET: api/review/product/{productId}
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewsForProduct(int productId, [FromQuery] int page = 1)
        {
            var reviews = await _reviewService.GetReviewsForProductAsync(productId, page);
            return Ok(reviews);
        }

        // GET: api/review/product/{productId}/average
        [HttpGet("product/{productId}/average")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAverageRatingForProduct(int productId)
        {
            var average = await _reviewService.GetAverageRatingForProductAsync(productId);
            return Ok(new { productId, averageRating = average });
        }

        // POST: api/review
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            await _reviewService.AddReviewAsync(userId, dto);
            return Ok(new { message = "Review added successfully." });
        }

        // DELETE: api/review/{reviewId}
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var (success, message) = await _reviewService.DeleteReviewAsync(userId, reviewId);

            if (!success)
            {
                if (message == "Review not found.")
                    return NotFound(new { message });

                if (message == "You are not authorized to delete this review.")
                    return Forbid();

                return BadRequest(new { message });
            }

            return Ok(new { message });
        }
    }
}
