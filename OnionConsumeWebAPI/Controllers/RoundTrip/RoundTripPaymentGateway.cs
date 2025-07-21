using Microsoft.AspNetCore.Mvc;

namespace OnionConsumeWebAPI.Controllers.RoundTrip
{
    public class RoundTripPaymentGateway : Controller
    {
        public IActionResult RoundTripPaymentView(string Guid)
        {
            ViewBag.Guid = Guid;
            return View();
        }
    }
}
