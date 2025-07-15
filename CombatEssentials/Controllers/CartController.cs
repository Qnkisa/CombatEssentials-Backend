using CombatEssentials.Application.DTOs.CartDtos;
using CombatEssentials.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CombatEssentials.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService) => _cartService = cartService;

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<List<GetCartItemDto>>> GetCartItems()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var items = await _cartService.GetCartItemsAsync(userId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cartService.AddToCartAsync(GetUserId(), dto);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var result = await _cartService.RemoveFromCartAsync(GetUserId(), cartItemId);
            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var result = await _cartService.ClearCartAsync(GetUserId());
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // PATCH: api/cart/update-quantity/{cartItemId}
        [HttpPatch("update-quantity/{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateCartQuantityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cartService.UpdateQuantityAsync(GetUserId(), cartItemId, dto.Quantity);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
