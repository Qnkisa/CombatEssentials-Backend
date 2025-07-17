using CombatEssentials.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CombatEssentials.API.Areas.UserControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int? categoryId = null, [FromQuery] string? name = null)
        {
            if (page < 1) return BadRequest("Page number must be 1 or greater.");

            var products = await _service.GetAllAsync(page, categoryId, name);
            return Ok(products);
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandom([FromQuery] int count = 9)
        {
            if (count < 1) return BadRequest("Count must be greater than 0.");

            var products = await _service.GetRandomProductsAsync(count);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
    }
}
