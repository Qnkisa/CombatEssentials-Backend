using CombatEssentials.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CombatEssentials.API.Areas.UserControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories); // Returns List<CategoryDto>
        }
    }
}
