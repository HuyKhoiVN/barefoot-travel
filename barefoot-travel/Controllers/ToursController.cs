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
        /// Display tour details page
        /// Route: /tours/{slug}
        /// </summary>
        /// <param name="slug">Tour slug</param>
        public IActionResult Index(string slug)
        {
            _logger.LogInformation("⚡ ToursController.Index called with slug: {Slug}", slug);
            
            if (string.IsNullOrWhiteSpace(slug))
            {
                _logger.LogWarning("❌ slug is invalid, redirecting to Home");
                return RedirectToAction("Index", "Home");
            }
            
            _logger.LogInformation("✅ Setting ViewData[TourSlug] = {Slug}", slug);
            ViewData["TourSlug"] = slug;
            ViewData["Title"] = "Tour Details";
            
            _logger.LogInformation("✅ Returning view for tour slug: {Slug}", slug);
            return View();
        }
    }
}

