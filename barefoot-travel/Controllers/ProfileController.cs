using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace barefoot_travel.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        public ProfileController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

