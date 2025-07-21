using DomainLayer.Model;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Service.Implementation;
using ServiceLayer.Service.Interface;

namespace OnionArchitectureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : Controller
    {
        private readonly IBooking<Booking> _bookingDetail;

        public BookingController(IBooking<Booking> bookingDetail)
        {
            this._bookingDetail = bookingDetail;
        }
        //[HttpGet("GetbookingDetail")]
        //public async Task<IActionResult> GetbookingDetail()
        //{
        //    var data = await _bookingDetail.GetAllAsync();
        //    return Ok(data);

        //    //return View();

        //}

        [HttpGet("GetbookingDetail")]
        public async Task<IActionResult> GetbookingDetail([FromQuery] string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                return BadRequest("userEmail is required.");

            var data = await _bookingDetail.GetAllAsync(userEmail);
            return Ok(data);
        }

        [HttpGet("GetbookingPNRs")]
        public async Task<IActionResult> GetbookingPNRs([FromQuery] string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                return BadRequest("userEmail is required.");

            var data = await _bookingDetail.GetPNRAsync(guid);
            return Ok(data);
        }


        [HttpPost("UpdateCancelStatus")]
       
        public async Task<IActionResult> UpdateCancelStatus([FromBody] CancelStatusRequest req)
        {
            bool ok = await _bookingDetail.UpdateCancelStatusAsync(
                          req.RecordLocator,
                          req.Status,
                          req.UserEmail,
                          req.BalanceDue,
                          req.TotalAmount);

            return ok ? Ok() : StatusCode(500, "DB update failed");
        }

        [HttpGet("GetbookingPNR")]
        public async Task<IActionResult> GetDetails(string recordLocator)
        {
            var result = await _bookingDetail.GetBookingDetailsFromSPAsync(recordLocator);
                      if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        public sealed record CancelStatusRequest(
          string RecordLocator,
          int Status,
          string UserEmail,
          decimal BalanceDue,
         decimal TotalAmount);
    }
}
