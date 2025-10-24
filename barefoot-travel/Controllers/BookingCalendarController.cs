using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    [Route("booking-calendar")]
    public class BookingCalendarController : Controller
    {
        public IActionResult BookingCalendar()
        {
            ViewData["Title"] = "Booking Calendar";
            return View();
        }
    }
}
