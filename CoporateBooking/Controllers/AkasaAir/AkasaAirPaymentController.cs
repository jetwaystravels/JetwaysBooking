using Microsoft.AspNetCore.Mvc;

namespace OnionConsumeWebAPI.Controllers.AkasaAir
{
    public class AkasaAirPaymentController : Controller
    {
        public IActionResult AkasaAirPaymentView(string GUID)
        {
			ViewBag.Guid = GUID;
			return View();
        }
    }
}
