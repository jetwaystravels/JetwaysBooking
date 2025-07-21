using DomainLayer.Model;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Service.Interface;

namespace OnionArchitectureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefundRequestsController : ControllerBase
    {
        private readonly IRefundRequest<RefundRequest> _refunddetail;

        public RefundRequestsController(IRefundRequest<RefundRequest> refunddetail)
        {
           
            this._refunddetail = refunddetail;
        }

        [HttpGet("GetRefund")]
      
        public async Task<IActionResult> GetRefund([FromQuery] string bookingId, [FromQuery] string recordLocator)
        {
            var results = await _refunddetail.GetByRefundAsync(bookingId, recordLocator);
            if (!results.Any())
                return NotFound("No refund requests found.");

            return Ok(results);
        }

               
    }
}
