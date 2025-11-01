using Microsoft.AspNetCore.Mvc;
using barefoot_travel.Services;

namespace barefoot_travel.Controllers
{
    /// <summary>
    /// Controller for category-based tour listing with slug-based URLs
    /// Route: /categories/{slug}
    /// </summary>
    public class CategoriesController : Controller
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly ICategoryService _categoryService;

        public CategoriesController(
            ILogger<CategoriesController> logger,
            ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        /// <summary>
        /// Display tours for a specific category using slug
        /// Route: /categories/{slug}
        /// Example: /categories/ha-long-bay
        /// </summary>
        /// <param name="slug">Category slug (e.g., "ha-long-bay")</param>
        public async Task<IActionResult> Index(string slug)
        {
            _logger.LogInformation("⚡ CategoriesController.Index called with slug: {Slug}", slug);
            
            if (string.IsNullOrWhiteSpace(slug))
            {
                _logger.LogWarning("❌ Slug is empty, redirecting to Home");
                return RedirectToAction("Index", "Home");
            }
            
            // Fetch category by slug to get ID and details
            var result = await _categoryService.GetCategoryBySlugAsync(slug);
            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("❌ Category not found for slug: {Slug}, redirecting to Home", slug);
                return RedirectToAction("Index", "Home");
            }
            
            var categoryDto = result.Data as DTOs.Category.CategoryDto;
            
            _logger.LogInformation("✅ Category found: {CategoryName} (ID: {CategoryId})", 
                categoryDto.CategoryName, categoryDto.Id);
            
            // Pass both slug and ID to view
            // - Slug: For display in URL and breadcrumbs
            // - ID: For API calls (performance - indexed queries)
            ViewData["CategorySlug"] = slug;
            ViewData["CategoryId"] = categoryDto.Id;
            ViewData["CategoryName"] = categoryDto.CategoryName;
            ViewData["Title"] = $"{categoryDto.CategoryName} Tours";
            
            _logger.LogInformation("✅ Returning view for category: {CategoryName} (slug: {Slug}, id: {Id})", 
                categoryDto.CategoryName, slug, categoryDto.Id);
            
            return View();
        }
    }
}




