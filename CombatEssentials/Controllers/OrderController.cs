using CombatEssentials.Application.Interfaces;
using CombatEssentials.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CombatEssentials.API.Controllers
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

        // GET: api/orders
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        // GET: api/orders/user
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetByUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetByUserIdAsync(userId);
            return Ok(orders);
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Order order)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            order.UserId = userId;
            order.OrderDate = DateTime.UtcNow;
            order.TotalAmount = order.OrderItems.Sum(i => i.TotalAmount);

            var created = await _orderService.CreateAsync(order);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/orders/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, Order order)
        {
            if (id != order.Id) return BadRequest();

            var success = await _orderService.UpdateAsync(order);
            if (!success) return NotFound();

            return NoContent();
        }

        // DELETE: api/orders/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _orderService.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
