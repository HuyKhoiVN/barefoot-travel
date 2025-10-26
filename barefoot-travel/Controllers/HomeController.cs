using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            ViewData["Title"] = "Home";
            return View();
        }

        public IActionResult Login()
        {
            ViewData["Title"] = "Login";
            return View();
        }
    }
}
