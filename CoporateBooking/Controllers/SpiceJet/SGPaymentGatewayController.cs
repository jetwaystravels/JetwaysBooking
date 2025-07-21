using DomainLayer.Model;
using Microsoft.AspNetCore.Mvc;

namespace OnionConsumeWebAPI.Controllers.AirAsia
{
    public class SGPaymentGatewayController : Controller
    {
        public IActionResult SpiceJetPayment(string Guid)
        {
            ViewBag.Guid = Guid;
            return View();
        }
    }
}
