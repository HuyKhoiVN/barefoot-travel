using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    [Route("booking-management")]
    public class BookingManagementController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Booking Management";
            return View();
        }
    }
}
