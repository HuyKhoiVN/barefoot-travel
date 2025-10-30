using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    public class ToursController : Controller
    {
        private readonly ILogger<ToursController> _logger;

        public ToursController(ILogger<ToursController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Display tours for a specific category
        /// Route: /tours/{categoryId}
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        public IActionResult Index(int categoryId)
        {
            _logger.LogInformation("⚡ ToursController.Index called with categoryId: {CategoryId}", categoryId);
            
            if (categoryId <= 0)
            {
                _logger.LogWarning("❌ categoryId is invalid, redirecting to Home");
                return RedirectToAction("Index", "Home");
            }
            
            _logger.LogInformation("✅ Setting ViewData[CategoryId] = {CategoryId}", categoryId);
            ViewData["CategoryId"] = categoryId;
            
            _logger.LogInformation("✅ Returning view for category ID: {CategoryId}", categoryId);
            return View();
        }
    }
}

