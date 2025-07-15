using CombatEssentials.Application.DTOs.ProductDtos;
using CombatEssentials.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CombatEssentials.API.Controllers
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
        public async Task<IActionResult> GetAll([FromQuery] int page = 1)
        {
            if (page < 1) return BadRequest("Page number must be 1 or greater.");

            var products = await _service.GetAllAsync(page);
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

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin([FromQuery] int page = 1)
        {
            if (page < 1) return BadRequest("Page number must be 1 or greater.");
            var products = await _service.GetAllForAdminAsync(page);
            return Ok(products);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return CreatedAtAction(nameof(GetById), new { id = result.CreatedProduct!.Id }, result.CreatedProduct);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Message);
        }

        [HttpPut("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Message);
        }

        [HttpPut("undelete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Undelete(int id)
        {
            var result = await _service.UndeleteAsync(id);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Message);
        }
    }
}
