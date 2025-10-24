using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers
{
    [Route("price-policy-management")]
    public class PricePolicyManagementController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Price & Policy Management";
            return View();
        }
    }
}
