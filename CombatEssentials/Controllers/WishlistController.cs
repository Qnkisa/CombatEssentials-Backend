using CombatEssentials.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CombatEssentials.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var items = await _wishlistService.GetWishlistAsync(userId);
            return Ok(items);
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var (success, message) = await _wishlistService.AddToWishlistAsync(userId, productId);

            if (!success)
            {
                if (message == "Product not found.")
                    return NotFound(new { message });
                if (message == "Product is already in the wishlist.")
                    return Conflict(new { message });
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var (success, message) = await _wishlistService.RemoveFromWishlistAsync(userId, productId);

            if (!success)
            {
                if (message == "Product not found in wishlist.")
                    return NotFound(new { message });
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }
    }
}
