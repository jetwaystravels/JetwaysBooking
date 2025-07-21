using DomainLayer.Model;
using Microsoft.AspNetCore.Mvc;
using OnionArchitectureAPI.Services.Print;
using ServiceLayer.Service.Interface;

namespace OnionArchitectureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintTicketController : Controller
    {
        private readonly IPrintTicket<Printticket> _printticket;
        private readonly PdfTicketService _pdfService;


        public PrintTicketController( IPrintTicket<Printticket> printticket,  PdfTicketService pdfService)
        {
            this._printticket = printticket;
            this._pdfService = pdfService;
        }

        [HttpGet("PrintTicket")]
        public async Task<IActionResult> GetTicketPdf(string pnr)
        {
            var json = await _printticket.GetBookingJsonByPNRAsync(pnr);

            if (string.IsNullOrWhiteSpace(json))
                return NotFound("Booking not found or JSON invalid.");

            var pdfBytes = _pdfService.GeneratePdfFromJson(json);
            return File(pdfBytes, "application/pdf", $"Ticket_{pnr}.pdf");
        }
    }
}
