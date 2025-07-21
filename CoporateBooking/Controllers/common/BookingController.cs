using Microsoft.AspNetCore.Mvc;
using DomainLayer.Model;
using OnionConsumeWebAPI.Extensions;
using CoporateBooking.Comman;
using static DomainLayer.Model.ReturnTicketBooking;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Utility;
using Newtonsoft.Json;
using CoporateBooking.Models;
using static CoporateBooking.Models.CancelBookingResponse;
namespace CoporateBooking.Controllers.common
{
    public class BookingController : Controller
    {
        private _credentials _credentialsAirasia = null;
        public async Task<IActionResult> MyBookingAsync(string Mess = "")
        {
            List<Booking> bookingList = null;
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            var userEmail = claims.Where(c => c.Type == ClaimTypes.Email).ToList()[0].Value;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Make GET call
                    //HttpResponseMessage response = await client.GetAsync(AppUrlConstant.Getflightbooking);
                    var requestUrl = $"{AppUrlConstant.Getflightbooking}?userEmail={Uri.EscapeDataString(userEmail)}";
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        bookingList = JsonConvert.DeserializeObject<List<Booking>>(result);

                        if(Mess != "")
                        {
                            ViewBag.Message = Mess;
                        }

                        return View(bookingList); // Pass to Razor View
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Failed to load booking data.";
                        return View(bookingList);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                return View(bookingList);
            }
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CancelbookingAsync(int airline, string pnr, string brid)
        {
            string result = string.Empty;
            string retrieveUrl = string.Empty;
            string bookingResult = string.Empty;
            HttpResponseMessage retrieveResponse = null;
            var requestUrl = "";

            // List<BookingData> bookingResponses = new List<BookingData>();
            List<FullBookingDetailsDto> bookingResponses = new List<FullBookingDetailsDto>();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(AppUrlConstant.BaseURL);
                requestUrl = $"{AppUrlConstant.GetflightPNR}?guid={Uri.EscapeDataString(brid)}";
                HttpResponseMessage response1 = await client.GetAsync(requestUrl);
                if (response1.IsSuccessStatusCode)
                {
                    result = await response1.Content.ReadAsStringAsync();
                }

                for (int i = 0; i < result.Split(',').Length; i++)
                {
                    pnr = result.Split(',')[i].Split('-')[0];
                    airline = Convert.ToInt32(result.Split(',')[i].Split('-')[1]);
                    retrieveUrl = $"{AppUrlConstant.GetflightPNRByRecordLocator}?recordLocator={Uri.EscapeDataString(pnr)}";
                    retrieveResponse = await client.GetAsync(retrieveUrl);
                    if (!retrieveResponse.IsSuccessStatusCode)
                    {
                        string err = await retrieveResponse.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", $"Booking retrieval failed: {err}");
                        return View("Error");
                    }
                    bookingResult = await retrieveResponse.Content.ReadAsStringAsync();
                    var booking = JsonConvert.DeserializeObject<FullBookingDetailsDto>(bookingResult);
                    ViewBag.AirlineId = airline;
                    bookingResponses.Add(booking);
                    if (i == result.Split(',').Length - 1)
                    {
                        // Last iteration, return the view
                        return View(bookingResponses);
                    }
                }
                return null;
               
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetBookingData(string tab, int page = 1, int pageSize = 10)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userEmail = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
                return BadRequest("User not authenticated");

            try
            {
                using var client = new HttpClient();
                var requestUrl = $"{AppUrlConstant.Getflightbooking}?userEmail={Uri.EscapeDataString(userEmail)}";
                var response = await client.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, "Failed to retrieve data");

                var result = await response.Content.ReadAsStringAsync();
                var bookings = JsonConvert.DeserializeObject<List<Booking>>(result);

                var filtered = tab switch
                {
                    "active" => bookings.Where(x => x.cancelstatus == 0 && x.DepartureDate >= DateTime.Today),
                    "cancelled" => bookings.Where(x => x.cancelstatus == 3),
                    "completed" => bookings.Where(x => x.cancelstatus != 3 && x.DepartureDate < DateTime.Today),
                    _ => Enumerable.Empty<Booking>()
                };

                var paged = filtered
                            .OrderByDescending(x => x.DepartureDate)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

                return Json(paged);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }


    }
}
