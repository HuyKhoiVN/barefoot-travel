using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    [Route("tour-approval")]
    public class TourApprovalController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Tour Status Approval";
            return View();
        }
    }
}

