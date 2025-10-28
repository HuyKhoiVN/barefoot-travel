using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace barefoot_travel.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        public SettingsController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

