using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace OnionConsumeWebAPI.Controllers.AirAsia
{
    public class AirAsiaOneWayPaymentController : Controller
    {
        public IActionResult AirAsiaOneWayPaymentView(string GUID)
        {
            ViewBag.Guid = GUID;
            return View();
        }
    }
}

