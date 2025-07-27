using CombatEssentials.Application.DTOs.ProductDtos;
using CombatEssentials.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CombatEssentials.API.Areas.AdminControllers
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")]
    public class AdminProductController : ControllerBase
    {
        private readonly IProductService _service;

        public AdminProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllForAdmin([FromQuery] int page = 1, [FromQuery] int? categoryId = null, [FromQuery] string? name = null)
        {
            if (page < 1) return BadRequest("Page number must be 1 or greater.");
            var products = await _service.GetAllForAdminAsync(page, categoryId, name);
            return Ok(products);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return CreatedAtRoute(routeName: null, routeValues: null, value: result.CreatedProduct);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                // Log errors or return detailed error response
                return BadRequest(new { errors });
            }
            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Message);
        }

        [HttpPut("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Message);
        }

        [HttpPut("undelete/{id}")]
        public async Task<IActionResult> Undelete(int id)
        {
            var result = await _service.UndeleteAsync(id);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Message);
        }
    }
}
