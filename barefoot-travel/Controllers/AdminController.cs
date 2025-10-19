using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            return View();
        }

        public IActionResult Demo()
        {
            ViewData["Title"] = "Demo Page";
            return View();
        }
    }
}
