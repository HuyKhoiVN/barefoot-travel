using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    [Route("tour-manager")]
    public class TourManagerController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Tour Management";
            return View();
        }
    }
}
