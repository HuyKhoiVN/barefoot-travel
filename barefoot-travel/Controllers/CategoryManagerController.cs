using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    public class CategoryManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
