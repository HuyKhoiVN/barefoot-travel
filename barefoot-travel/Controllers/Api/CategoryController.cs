using Microsoft.AspNetCore.Mvc;
using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Category;
using barefoot_travel.Services;

namespace barefoot_travel.Controllers.Api
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

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get category tree structure
        /// </summary>
        [HttpGet("tree")]
        public async Task<IActionResult> GetCategoryTree()
        {
            var result = await _categoryService.GetCategoryTreeAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get categories with pagination
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetCategoriesPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] string? categoryName = null,
            [FromQuery] string? type = null,
            [FromQuery] int? parentCategory = null,
            [FromQuery] bool? active = null)
        {
            try
            {
                var result = await _categoryService.GetCategoriesPagedAsync(page, pageSize, sortBy, sortOrder, categoryName, type, parentCategory, active);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
        }

        /// <summary>
        /// Get categories by type
        /// </summary>
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetCategoriesByType(string type)
        {
            var result = await _categoryService.GetCategoriesByTypeAsync(type);
            return Ok(result);
        }

        /// <summary>
        /// Get categories by parent ID
        /// </summary>
        [HttpGet("parent/{parentId?}")]
        public async Task<IActionResult> GetCategoriesByParentId(int? parentId)
        {
            var result = await _categoryService.GetCategoriesByParentIdAsync(parentId);
            return Ok(result);
        }

        /// <summary>
        /// Create new category
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, "Invalid model state", ModelState));
            }

            var adminUsername = GetUserIdFromClaims.GetUsername(HttpContext.User);
            var result = await _categoryService.CreateCategoryAsync(dto, adminUsername);
            return result.Success ? CreatedAtAction(nameof(GetCategoryById), new { id = ((CategoryDto)result.Data!).Id }, result) : BadRequest(result);
        }

        /// <summary>
        /// Update category
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, "Invalid model state", ModelState));
            }

            var adminUsername = GetUserIdFromClaims.GetUsername(HttpContext.User);
            var result = await _categoryService.UpdateCategoryAsync(id, dto, adminUsername);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete category
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var adminUsername = GetUserIdFromClaims.GetUsername(HttpContext.User);
            var result = await _categoryService.DeleteCategoryAsync(id, adminUsername);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update category status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateCategoryStatus(int id, [FromBody] bool active)
        {
            var adminUsername = GetUserIdFromClaims.GetUsername(HttpContext.User);
            var result = await _categoryService.UpdateCategoryStatusAsync(id, active, adminUsername);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all unique category types
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetAllType()
        {
            var result = await _categoryService.GetAllType();
            return Ok(result);
        }

        /// <summary>
        /// Get categories with pagination for tree view
        /// </summary>
        [HttpGet("tree-paged")]
        public async Task<IActionResult> GetTreePaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] string? categoryName = null,
            [FromQuery] string? type = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] bool? active = null)
        {
            try
            {
                var result = await _categoryService.GetTreePagedAsync(page, pageSize, sortBy, sortOrder, categoryName, type, categoryIds, active);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
        }

        /// <summary>
        /// Get children of a category
        /// </summary>
        [HttpGet("child/{id}")]
        public async Task<IActionResult> GetChildren(int id)
        {
            var result = await _categoryService.GetChildrenAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
