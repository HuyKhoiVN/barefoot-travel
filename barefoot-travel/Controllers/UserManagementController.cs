using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    [Route("user-management")]
    public class UserManagementController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "User Management";
            return View();
        }
    }
}
