using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Login()
        {
            ViewData["Title"] = "Login";
            return View();
        }
    }
}
