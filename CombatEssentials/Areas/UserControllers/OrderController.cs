using CombatEssentials.Application.DTOs.OrderDtos;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CombatEssentials.API.Areas.UserControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetByUser([FromQuery] int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetByUserIdAsync(userId, page);
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            string? userId = null;

            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var created = await _orderService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetByUser), new { id = created.Id }, created);
        }
    }
}
