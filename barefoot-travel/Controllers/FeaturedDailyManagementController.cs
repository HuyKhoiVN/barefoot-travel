using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    [Route("featured-daily-management")]
    public class FeaturedDailyManagementController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Featured & Daily Tours Management";
            return View();
        }
    }
}
