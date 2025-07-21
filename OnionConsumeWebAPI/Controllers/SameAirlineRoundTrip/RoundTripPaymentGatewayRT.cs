using Microsoft.AspNetCore.Mvc;

namespace OnionConsumeWebAPI.Controllers.RoundTrip
{
    public class RoundTripPaymentGatewayRT : Controller
    {
        public IActionResult RoundTripPaymentViewRT()
        {
            return View();
        }
    }
}
