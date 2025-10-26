using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    [Route("homepage-content")]
    public class HomePageContentController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Homepage Content Management";
            return View();
        }
    }
}
