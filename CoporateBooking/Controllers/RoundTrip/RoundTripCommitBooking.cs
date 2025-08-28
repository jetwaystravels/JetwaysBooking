using Bookingmanager_;
using DomainLayer.Model;
using DomainLayer.ViewModel;
using Indigo;
using Microsoft.AspNetCore.Mvc;
using Nancy.Json;
using Newtonsoft.Json;
using NuGet.Common;
using OnionArchitectureAPI.Services.Indigo;
using OnionConsumeWebAPI.Extensions;
using OnionConsumeWebAPI.Models;
using Sessionmanager;
using System.Collections;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using Utility;
using static DomainLayer.Model.ReturnAirLineTicketBooking;
using static DomainLayer.Model.ReturnTicketBooking;
using OnionArchitectureAPI.Services.Barcode;
using OnionArchitectureAPI.Services.Travelport;
using System.Text;
using System.Text.RegularExpressions;
using OnionConsumeWebAPI.Models;
using CoporateBooking.Models;
using System.Security.Claims;
using ServiceLayer.Service.Interface;

namespace OnionConsumeWebAPI.Controllers.RoundTrip
{
    public class RoundTripCommitBooking : Controller
    {
        string token = string.Empty;
        string ssrKey = string.Empty;
        string journeyKey = string.Empty;
        string uniquekey = string.Empty;
        string AirLinePNR = string.Empty;
        string BarcodeString = string.Empty;
        string BarcodeInfantString = string.Empty;
        String BarcodePNR = string.Empty;
        string orides = string.Empty;
        string carriercode = string.Empty;
        string flightnumber = string.Empty;
        string seatnumber = string.Empty;
        string sequencenumber = string.Empty;
        DateTime Journeydatetime = new DateTime();
        string bookingKey = string.Empty;
        ApiResponseModel responseModel;
        private readonly IConfiguration _configuration;
        public RoundTripCommitBooking(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> RoundTripBookingView(string Guid)
        {

            AirLinePNRTicket _AirLinePNRTicket = new AirLinePNRTicket();
            _AirLinePNRTicket.AirlinePNR = new List<ReturnTicketBooking>();
            Logs logs = new Logs();
            bool flagAirAsia = true;
            bool flagSpicejet = true;
            bool flagIndigo = true;
            string json = HttpContext.Session.GetString("AirlineSelectedRT");
            Airlinenameforcommit data = JsonConvert.DeserializeObject<Airlinenameforcommit>(json);



            using (HttpClient client = new HttpClient())
            {
                MongoHelper objMongoHelper = new MongoHelper();
                MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
                MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
                string outboundbookingid = string.Empty;
                string inboundbookingid = string.Empty;
                GSTDetails gSTDetails = new GSTDetails();
                ContactDetail contactDetail = new ContactDetail();
                tb_Booking tb_Booking = new tb_Booking();
                tb_Airlines tb_Airlines = new tb_Airlines();
                tb_AirCraft tb_AirCraft = new tb_AirCraft();
                client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);

                GDSPNRResponse gDSPNRResponse = await _mongoDBHelper.GetGDSPNRByGUID(Guid);
                decimal TotalMeal = 0;
                decimal TotalBag = 0;
                decimal TotalFastFFWD = 0;
                decimal Totatamountmb = 0;
                decimal TotalBagtax = 0;
                List<tb_journeys> tb_JourneysList = new List<tb_journeys>();
                List<tb_PassengerDetails> tb_PassengerDetailsList = new List<tb_PassengerDetails>();
                List<tb_Segments> segmentReturnsListt = new List<tb_Segments>();
                tb_PassengerTotal tb_PassengerTotalobj = new tb_PassengerTotal();
                AirLineFlightTicketBooking airLineFlightTicketBooking = new AirLineFlightTicketBooking();
                for (int k1 = 0; k1 < data.Airline.Count; k1++)
                {
                    string tokenview = string.Empty;
                    string token = string.Empty;
                    if (string.IsNullOrEmpty(tokenview) && flagAirAsia == true && data.Airline[k1].ToLower().Contains("airasia"))
                    {
                        double totalAmount = 0;
                        double totalAmountBaggage = 0;
                        double totalAmounttax = 0;
                        double totalAmounttaxSGST = 0;
                        double totalAmounttaxBag = 0;
                        double totalAmounttaxSGSTBag = 0;
                        double totalMealTax = 0;
                        double totalBaggageTax = 0;
                        double taxMinusMeal = 0;
                        double taxMinusBaggage = 0;
                        double TotalAmountMeal = 0;
                        double TotaAmountBaggage = 0;
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "AirAsia").Result;
                        tokenview = tokenData.Token;

                        if (k1 == 0)
                        {
                            token = tokenData.Token;
                        }
                        else
                        {
                            token = tokenData.RToken;
                        }

                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responceGetBookingSate = await client.GetAsync(AppUrlConstant.AirasiaGetBoking);
                        if (responceGetBookingSate.IsSuccessStatusCode)
                        {
                            string _responceGetBooking = responceGetBookingSate.Content.ReadAsStringAsync().Result;
                            var DataBooking = JsonConvert.DeserializeObject<dynamic>(_responceGetBooking);
                            decimal Totalpayment = 0M;
                            if (_responceGetBooking != null)
                            {
                                Totalpayment = DataBooking.data.breakdown.totalAmount;
                            }

                            if (k1 == 0)
                            {
                                logs.WriteLogsR(AppUrlConstant.URLAirasia + "/api/nsk/v1/booking", "14-GetBookingRequest_Left", "AirAsiaRT");
                                logs.WriteLogsR(_responceGetBooking, "14-GetBookingResponse_Left", "AirAsiaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(AppUrlConstant.URLAirasia + "/api/nsk/v1/booking", "14-GetBookingRequest_Right", "AirAsiaRT");
                                logs.WriteLogsR(_responceGetBooking, "14-GetBookingResponse_Right", "AirAsiaRT");
                            }
                            //ADD Payment
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            PaymentRequest paymentRequest = new PaymentRequest();
                            paymentRequest.PaymentMethodCode = "AG";
                            paymentRequest.Amount = Totalpayment;
                            paymentRequest.PaymentFields = new PaymentFields();
                            paymentRequest.PaymentFields.ACCTNO = "CRPAPI";
                            paymentRequest.PaymentFields.AMT = Totalpayment;
                            paymentRequest.CurrencyCode = "INR";
                            paymentRequest.Installments = 1;
                            string jsonPayload = JsonConvert.SerializeObject(paymentRequest);
                            HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                            string url = AppUrlConstant.AirasiaPayment;
                            HttpResponseMessage response = await client.PostAsync(url, content);
                            string responseContent = await response.Content.ReadAsStringAsync();
                            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                            if (k1 == 0)
                            {
                                logs.WriteLogsR(jsonPayload, "15-AddPaymentRequest_Left", "AirAsiaRT");
                                logs.WriteLogsR(responseContent, "15-AddPaymentResponse_Left", "AirAsiaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(jsonPayload, "15-AddPaymentRequest_Right", "AirAsiaRT");
                                logs.WriteLogsR(responseContent, "15-AddPaymentResponse_Right", "AirAsiaRT");
                            }
                        }



                        #region Commit Booking
                        string[] NotifyContacts = new string[1];
                        NotifyContacts[0] = "P";
                        Commit_BookingModel _Commit_BookingModel = new Commit_BookingModel();

                        _Commit_BookingModel.notifyContacts = true;
                        _Commit_BookingModel.contactTypesToNotify = NotifyContacts;
                        var jsonCommitBookingRequest = JsonConvert.SerializeObject(_Commit_BookingModel, Formatting.Indented);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responceCommit_Booking = await client.PostAsJsonAsync(AppUrlConstant.AirasiaCommitBooking, _Commit_BookingModel);
                        if (responceCommit_Booking.IsSuccessStatusCode)
                        {
                            var _responceCommit_Booking = responceCommit_Booking.Content.ReadAsStringAsync().Result;
                            if (k1 == 0)
                            {
                                logs.WriteLogsR(jsonCommitBookingRequest, "16-CommitRequest_Left", "AirAsiaRT");
                                logs.WriteLogsR(_responceCommit_Booking, "16-CommitResponse_Left", "AirAsiaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(jsonCommitBookingRequest, "16-CommitRequest_Right", "AirAsiaRT");
                                logs.WriteLogsR(_responceCommit_Booking, "16-CommitResponse_Right", "AirAsiaRT");
                            }
                            var JsonObjCommit_Booking = JsonConvert.DeserializeObject<dynamic>(_responceCommit_Booking);
                        }
                        #endregion

                        #region Booking GET
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responceGetBooking = await client.GetAsync(AppUrlConstant.AirasiaGetBoking);
                        if (responceGetBooking.IsSuccessStatusCode)
                        {
                            Hashtable htname = new Hashtable();
                            Hashtable htnameempty = new Hashtable();
                            Hashtable htpax = new Hashtable();
                            string sequencenumber = string.Empty;

                            Hashtable htseatdata = new Hashtable();
                            Hashtable htmealdata = new Hashtable();
                            Hashtable htBagdata = new Hashtable();
                            var _responcePNRBooking = responceGetBooking.Content.ReadAsStringAsync().Result;
                            if (k1 == 0)
                            {
                                logs.WriteLogsR(AppUrlConstant.AirasiaGetBoking.ToString(), "17-GetBookingPNRDetailsRequest_Left", "AirAsiaRT");
                                logs.WriteLogsR(_responcePNRBooking, "17-GetBookingPNRDetailsResponse_Left", "AirAsiaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(AppUrlConstant.AirasiaGetBoking.ToString(), "17-GetBookingPNRDetailsRequest_Right", "AirAsiaRT");
                                logs.WriteLogsR(_responcePNRBooking, "17GetBookingPNRDetailsResponse_Right", "AirAsiaRT");
                            }
                            var JsonObjPNRBooking = JsonConvert.DeserializeObject<dynamic>(_responcePNRBooking);
                            ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                            string PassengerData = HttpContext.Session.GetString("PassengerNameDetails");
                            List<passkeytype> PassengerDataDetailsList = JsonConvert.DeserializeObject<List<passkeytype>>(PassengerData);
                            returnTicketBooking.recordLocator = JsonObjPNRBooking.data.recordLocator;
                            BarcodePNR = JsonObjPNRBooking.data.recordLocator;
                            Info info = new Info();
                            info.bookedDate = JsonObjPNRBooking.data.info.bookedDate;
                            returnTicketBooking.info = info;
                            if (BarcodePNR != null && BarcodePNR.Length < 7)
                            {
                                BarcodePNR = BarcodePNR.PadRight(7);
                            }
                            returnTicketBooking.airLines = "AirAsia";
                            returnTicketBooking.bookingKey = JsonObjPNRBooking.data.bookingKey;
                            Breakdown breakdown = new Breakdown();
                            breakdown.balanceDue = JsonObjPNRBooking.data.breakdown.totalAmount; //TotalAmount
                            JourneyTotals journeyTotalsobj = new JourneyTotals();
                            journeyTotalsobj.totalAmount = JsonObjPNRBooking.data.breakdown.journeyTotals.totalAmount;
                            journeyTotalsobj.totalTax = JsonObjPNRBooking.data.breakdown.journeyTotals.totalTax;

                            var baseTotalAmount = journeyTotalsobj.totalAmount;
                            var BaseTotalTax = journeyTotalsobj.totalTax;

                            var ToatalBasePrice = journeyTotalsobj.totalAmount + journeyTotalsobj.totalTax;

                            //changes for Passeneger name:
                            foreach (var items in JsonObjPNRBooking.data.passengers)
                            {
                                htname.Add(items.Value.passengerKey.ToString(), items.Value.name.last.ToString() + "/" + items.Value.name.first.ToString());
                            }
                            InfantReturn infantReturnobj = new InfantReturn();
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.infant != null)
                            {
                                infantReturnobj.total = JsonObjPNRBooking.data.breakdown.passengerTotals.infant.total;
                                infantReturnobj.taxes = JsonObjPNRBooking.data.breakdown.passengerTotals.infant.taxes;
                                double TotalInfantAmount = infantReturnobj.total + infantReturnobj.taxes;
                                double totalAmountSum = journeyTotalsobj.totalAmount + infantReturnobj.total + infantReturnobj.taxes;
                                double totaltax = journeyTotalsobj.totalTax;

                                double totalplusAmountSumtax = totalAmountSum + totaltax;
                                breakdown.totalAmountSum = totalAmountSum;
                                breakdown.totaltax = totaltax;
                                breakdown.totalplusAmountSumtax = totalplusAmountSumtax;
                            }

                            PassengerTotals passengerTotals = new PassengerTotals();
                            SpecialServices serviceChargeReturn = new SpecialServices();
                            List<ReturnCharge> returnChargeList = new List<ReturnCharge>();
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices != null)
                            {
                                int chargesCount = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.charges.Count;

                                for (int ch = 0; ch < chargesCount; ch++)
                                {
                                    ReturnCharge returnChargeobj = new ReturnCharge();
                                    returnChargeobj.amount = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.charges[ch].amount;
                                    returnChargeobj.code = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.charges[ch].code;

                                    if (returnChargeobj.code.StartsWith("CGST"))
                                    {
                                        continue;
                                    }
                                    if (returnChargeobj.code.StartsWith("SGST") || returnChargeobj.code.Contains("GST"))
                                    {
                                        continue;
                                    }

                                    bool isSpecialCode = returnChargeobj.code.Equals("PBCA", StringComparison.OrdinalIgnoreCase) ||
                                                            returnChargeobj.code.Equals("PBCB", StringComparison.OrdinalIgnoreCase) ||
                                                            returnChargeobj.code.Equals("PBA3", StringComparison.OrdinalIgnoreCase) ||
                                                            returnChargeobj.code.Equals("PBAB", StringComparison.OrdinalIgnoreCase) ||
                                                            returnChargeobj.code.Equals("PBAC", StringComparison.OrdinalIgnoreCase) ||
                                                            returnChargeobj.code.Equals("PBAD", StringComparison.OrdinalIgnoreCase) ||
                                                            returnChargeobj.code.Equals("PBAF", StringComparison.OrdinalIgnoreCase);


                                    if (isSpecialCode == false)
                                    {
                                        TotalAmountMeal += returnChargeobj.amount;
                                    }
                                    else
                                    {
                                        if (returnChargeobj.amount.ToString().Contains("-"))
                                        {
                                            TotaAmountBaggage -= returnChargeobj.amount;
                                        }
                                        else
                                        {
                                            TotaAmountBaggage += returnChargeobj.amount;
                                        }
                                    }

                                    returnChargeList.Add(returnChargeobj);
                                }
                                serviceChargeReturn.charges = returnChargeList;

                            }

                            ReturnSeats returnSeats = new ReturnSeats();
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total > 0 || JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total != null)
                                {
                                    returnSeats.total = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total;
                                    returnSeats.taxes = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.taxes;
                                    returnSeats.totalSeatAmount = returnSeats.total + returnSeats.taxes;
                                    returnSeats.adjustments = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments;
                                    if (returnSeats.adjustments != null && returnSeats.adjustments.ToString() != "")
                                    {
                                        returnSeats.totalSeatAmount += Convert.ToInt32(returnSeats.adjustments);
                                    }
                                }
                            }
                            SpecialServices specialServices = new SpecialServices();
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total != null)
                                {
                                    specialServices.total = (decimal)JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total;
                                }
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes != null)
                                {
                                    specialServices.taxes = (decimal)JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes;
                                }
                            }
                            breakdown.journeyTotals = journeyTotalsobj;
                            breakdown.passengerTotals = passengerTotals;
                            breakdown.baseTotalAmount = baseTotalAmount + infantReturnobj.total;
                            breakdown.ToatalBasePrice = ToatalBasePrice + infantReturnobj.taxes;
                            breakdown.BaseTotalTax = BaseTotalTax + infantReturnobj.taxes;
                            passengerTotals.seats = returnSeats;
                            passengerTotals.infant = infantReturnobj;
                            passengerTotals.specialServices = specialServices;
                            passengerTotals.specialServices = serviceChargeReturn;

                            if (JsonObjPNRBooking.data.contacts.G != null)
                            {
                                returnTicketBooking.customerNumber = JsonObjPNRBooking.data.contacts.G.customerNumber;
                                returnTicketBooking.companyName = JsonObjPNRBooking.data.contacts.G.companyName;
                                returnTicketBooking.emailAddressgst = JsonObjPNRBooking.data.contacts.G.emailAddress;
                            }
                            Contacts _contactobj = new Contacts();
                            int PhoneNumberCount = JsonObjPNRBooking.data.contacts.P.phoneNumbers.Count;
                            List<PhoneNumber> phoneNumberList = new List<PhoneNumber>();
                            for (int p = 0; p < PhoneNumberCount; p++)
                            {
                                PhoneNumber phoneobject = new PhoneNumber();
                                phoneobject.number = JsonObjPNRBooking.data.contacts.P.phoneNumbers[p].number;
                                phoneNumberList.Add(phoneobject);
                            }
                            int JourneysReturnCount = JsonObjPNRBooking.data.journeys.Count;
                            List<JourneysReturn> journeysreturnList = new List<JourneysReturn>();
                            for (int i = 0; i < JourneysReturnCount; i++)
                            {
                                JourneysReturn journeysReturnObj = new JourneysReturn();
                                journeysReturnObj.stops = JsonObjPNRBooking.data.journeys[i].stops;

                                DesignatorReturn ReturnDesignatorobject = new DesignatorReturn();
                                ReturnDesignatorobject.origin = JsonObjPNRBooking.data.journeys[i].designator.origin;
                                ReturnDesignatorobject.destination = JsonObjPNRBooking.data.journeys[i].designator.destination;
                                orides = JsonObjPNRBooking.data.journeys[i].designator.origin + JsonObjPNRBooking.data.journeys[i].designator.destination;
                                ReturnDesignatorobject.departure = JsonObjPNRBooking.data.journeys[i].designator.departure;
                                ReturnDesignatorobject.arrival = JsonObjPNRBooking.data.journeys[i].designator.arrival;

                                journeysReturnObj.designator = ReturnDesignatorobject;
                                int SegmentReturnCount = JsonObjPNRBooking.data.journeys[i].segments.Count;
                                List<SegmentReturn> segmentReturnsList = new List<SegmentReturn>();
                                for (int j = 0; j < SegmentReturnCount; j++)
                                {
                                    returnSeats.unitDesignator = string.Empty;
                                    returnSeats.SSRCode = string.Empty;
                                    SegmentReturn segmentReturnobj = new SegmentReturn();
                                    segmentReturnobj.isStandby = JsonObjPNRBooking.data.journeys[i].segments[j].isStandby;
                                    segmentReturnobj.isHosted = JsonObjPNRBooking.data.journeys[i].segments[j].isHosted;
                                    DesignatorReturn designatorReturn = new DesignatorReturn();
                                    designatorReturn.origin = JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin;
                                    designatorReturn.destination = JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination;
                                    designatorReturn.departure = JsonObjPNRBooking.data.journeys[i].segments[j].designator.departure;
                                    designatorReturn.arrival = JsonObjPNRBooking.data.journeys[i].segments[j].designator.arrival;
                                    segmentReturnobj.designator = designatorReturn;
                                    orides = designatorReturn.origin + designatorReturn.destination;
                                    var passengersegmentCount = JsonObjPNRBooking.data.journeys[i].segments[j].passengerSegment;
                                    int passengerReturnCount = ((Newtonsoft.Json.Linq.JContainer)passengersegmentCount).Count;
                                    string dateString = JsonObjPNRBooking.data.journeys[i].designator.departure;
                                    DateTime date = DateTime.ParseExact(dateString, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                    //julian date
                                    int year = date.Year;
                                    int month = date.Month;
                                    int day = date.Day;

                                    // Calculate the number of days from January 1st to the given date
                                    DateTime currentDate = new DateTime(year, month, day);
                                    DateTime startOfYear = new DateTime(year, 1, 1);
                                    int julianDate = (currentDate - startOfYear).Days + 1;
                                    sequencenumber = SequenceGenerator.GetNextSequenceNumber();

                                    flightnumber = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.identifier;
                                    if (flightnumber.Length < 5)
                                    {
                                        flightnumber = flightnumber.PadRight(5);
                                    }
                                    carriercode = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.carrierCode;
                                    if (carriercode.Length < 3)
                                    {
                                        carriercode = carriercode.PadRight(3);
                                    }

                                    foreach (var items in JsonObjPNRBooking.data.passengers)
                                    {
                                        if (!htnameempty.Contains(items.Value.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination))
                                        {
                                            if (carriercode.Length < 3)
                                                carriercode = carriercode.PadRight(3);
                                            if (flightnumber.Length < 5)
                                            {
                                                flightnumber = flightnumber.PadRight(5);
                                            }
                                            if (sequencenumber.Length < 5)
                                                sequencenumber = sequencenumber.PadRight(5, '0');
                                            seatnumber = "0000";
                                            if (seatnumber.Length < 4)
                                                seatnumber = seatnumber.PadLeft(4, '0');
                                            BarcodeString = "M" + "1" + htname[items.Value.passengerKey.ToString()] + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                            htnameempty.Add(items.Value.passengerKey.ToString() + "_" + htname[items.Value.passengerKey.ToString()] + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, BarcodeString);
                                        }
                                    }



                                    List<PassengerSegment> passengerSegmentsList = new List<PassengerSegment>();
                                    foreach (var item in JsonObjPNRBooking.data.journeys[i].segments[j].passengerSegment)
                                    {
                                        PassengerSegment passengerSegmentobj = new PassengerSegment();
                                        passengerSegmentobj.passengerKey = item.Value.passengerKey;
                                        passengerSegmentsList.Add(passengerSegmentobj);
                                        int seatCount = item.Value.seats.Count;
                                        int ssrCodeCount = item.Value.ssrs.Count;
                                        List<ReturnSeats> returnSeatsList = new List<ReturnSeats>();
                                        for (int q = 0; q < seatCount; q++)
                                        {
                                            ReturnSeats returnSeatsObj = new ReturnSeats();
                                            returnSeatsObj.unitDesignator = item.Value.seats[q].unitDesignator;
                                            seatnumber = item.Value.seats[q].unitDesignator;
                                            if (string.IsNullOrEmpty(seatnumber))
                                            {
                                                seatnumber = "0000"; // Set to "0000" if not available
                                            }
                                            else
                                            {
                                                seatnumber = seatnumber.PadRight(4, '0'); // Right-pad with zeros if less than 4 characters
                                            }
                                            returnSeatsList.Add(returnSeatsObj);
                                            htseatdata.Add(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, returnSeatsObj.unitDesignator);
                                            returnSeats.unitDesignator += returnSeatsObj.unitDesignator + ",";

                                            if (!htpax.Contains(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination))
                                            {
                                                if (carriercode.Length < 3)
                                                    carriercode = carriercode.PadRight(3);
                                                if (flightnumber.Length < 5)
                                                {
                                                    flightnumber = flightnumber.PadRight(5);
                                                }
                                                if (sequencenumber.Length < 5)
                                                    sequencenumber = sequencenumber.PadRight(5, '0');
                                                seatnumber = htseatdata[passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination].ToString();
                                                if (seatnumber.Length < 4)
                                                    seatnumber = seatnumber.PadLeft(4, '0');
                                                BarcodeString = "M" + "1" + htname[passengerSegmentobj.passengerKey] + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                                htpax.Add(passengerSegmentobj.passengerKey.ToString() + "_" + htname[passengerSegmentobj.passengerKey] + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, BarcodeString);
                                            }
                                        }
                                        List<SsrReturn> SrrcodereturnsList = new List<SsrReturn>();
                                        for (int t = 0; t < ssrCodeCount; t++)
                                        {
                                            SsrReturn ssrReturn = new SsrReturn();
                                            ssrReturn.ssrCode = item.Value.ssrs[t].ssrCode;

                                            bool isSpecialCode = ssrReturn.ssrCode.Equals("PBCA", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBCB", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBA3", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBAB", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBAC", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBAD", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBAF", StringComparison.OrdinalIgnoreCase);
                                            if (isSpecialCode)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (!htmealdata.Contains(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination))
                                                {
                                                    htmealdata.Add(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, ssrReturn.ssrCode);
                                                }
                                                returnSeats.SSRCode += ssrReturn.ssrCode + ",";
                                            }


                                        }
                                        for (int t = 0; t < ssrCodeCount; t++)
                                        {
                                            SsrReturn ssrReturn = new SsrReturn();
                                            ssrReturn.ssrCode = item.Value.ssrs[t].ssrCode;
                                            bool isSpecialCode = ssrReturn.ssrCode.Equals("PBCA", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBCB", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBA3", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBAB", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBAC", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBAD", StringComparison.OrdinalIgnoreCase) ||
                                                    ssrReturn.ssrCode.Equals("PBAF", StringComparison.OrdinalIgnoreCase);


                                            if (isSpecialCode)
                                            {
                                                if (!htBagdata.Contains(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination))
                                                {
                                                    htBagdata.Add(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, ssrReturn.ssrCode);

                                                }
                                                returnSeats.SSRCode += ssrReturn.ssrCode + ",";

                                            }
                                            else
                                            {
                                                continue;
                                            }


                                        }

                                    }
                                    segmentReturnobj.passengerSegment = passengerSegmentsList;

                                    int ReturmFareCount = JsonObjPNRBooking.data.journeys[i].segments[j].fares.Count;
                                    List<FareReturn> fareList = new List<FareReturn>();
                                    for (int k = 0; k < ReturmFareCount; k++)
                                    {
                                        FareReturn fareReturnobj = new FareReturn();
                                        fareReturnobj.productClass = JsonObjPNRBooking.data.journeys[i].segments[j].fares[k].productClass;

                                        int PassengerFareReturnCount = JsonObjPNRBooking.data.journeys[i].segments[j].fares[k].passengerFares.Count;
                                        List<PassengerFareReturn> passengerFareReturnList = new List<PassengerFareReturn>();
                                        for (int l = 0; l < PassengerFareReturnCount; l++)
                                        {
                                            PassengerFareReturn passengerFareReturnobj = new PassengerFareReturn();

                                            int ServiceChargeReturnCount = JsonObjPNRBooking.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges.Count;

                                            List<ServiceChargeReturn> serviceChargeReturnList = new List<ServiceChargeReturn>();
                                            for (int m = 0; m < ServiceChargeReturnCount; m++)
                                            {
                                                ServiceChargeReturn serviceChargeReturnobj = new ServiceChargeReturn();

                                                serviceChargeReturnobj.amount = JsonObjPNRBooking.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges[m].amount;
                                                serviceChargeReturnList.Add(serviceChargeReturnobj);


                                            }
                                            passengerFareReturnobj.serviceCharges = serviceChargeReturnList;
                                            passengerFareReturnList.Add(passengerFareReturnobj);

                                        }
                                        fareReturnobj.passengerFares = passengerFareReturnList;
                                        fareList.Add(fareReturnobj);

                                    }
                                    segmentReturnobj.fares = fareList;

                                    IdentifierReturn identifierReturn = new IdentifierReturn();
                                    identifierReturn.identifier = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.identifier;
                                    flightnumber = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.identifier;
                                    if (flightnumber.Length < 5)
                                    {
                                        flightnumber = flightnumber.PadRight(5);
                                    }
                                    carriercode = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.carrierCode;
                                    if (carriercode.Length < 3)
                                    {
                                        carriercode = carriercode.PadRight(3);
                                    }
                                    identifierReturn.carrierCode = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.carrierCode;
                                    segmentReturnobj.identifier = identifierReturn;

                                    var LegReturn = JsonObjPNRBooking.data.journeys[i].segments[j].legs;
                                    int Legcount = ((Newtonsoft.Json.Linq.JContainer)LegReturn).Count;
                                    List<LegReturn> legReturnsList = new List<LegReturn>();
                                    for (int n = 0; n < Legcount; n++)
                                    {
                                        LegReturn LegReturnobj = new LegReturn();
                                        LegReturnobj.legKey = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legKey;

                                        DesignatorReturn ReturnlegDesignatorobj = new DesignatorReturn();
                                        ReturnlegDesignatorobj.origin = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].designator.origin;
                                        ReturnlegDesignatorobj.destination = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].designator.destination;
                                        ReturnlegDesignatorobj.departure = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].designator.departure;
                                        ReturnlegDesignatorobj.arrival = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].designator.arrival;
                                        LegReturnobj.designator = ReturnlegDesignatorobj;

                                        LegInfoReturn legInfoReturn = new LegInfoReturn();
                                        legInfoReturn.arrivalTerminal = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal;
                                        legInfoReturn.arrivalTime = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.arrivalTime;
                                        legInfoReturn.departureTerminal = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal;
                                        legInfoReturn.departureTime = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.departureTime;
                                        LegReturnobj.legInfo = legInfoReturn;
                                        legReturnsList.Add(LegReturnobj);

                                    }
                                    segmentReturnobj.unitdesignator = returnSeats.unitDesignator;
                                    segmentReturnobj.SSRCode = returnSeats.SSRCode;
                                    segmentReturnobj.legs = legReturnsList;
                                    segmentReturnsList.Add(segmentReturnobj);

                                }
                                journeysReturnObj.segments = segmentReturnsList;
                                journeysreturnList.Add(journeysReturnObj);
                            }

                            var Returnpassanger = JsonObjPNRBooking.data.passengers;
                            int Returnpassengercount = ((Newtonsoft.Json.Linq.JContainer)Returnpassanger).Count;
                            List<ReturnPassengers> ReturnpassengersList = new List<ReturnPassengers>();
                            foreach (var items in JsonObjPNRBooking.data.passengers)
                            {
                                ReturnPassengers returnPassengersobj = new ReturnPassengers();
                                returnPassengersobj.passengerKey = items.Value.passengerKey;
                                returnPassengersobj.passengerTypeCode = items.Value.passengerTypeCode;
                                returnPassengersobj.name = new Name();
                                returnPassengersobj.name.first = items.Value.name.first;
                                returnPassengersobj.name.last = items.Value.name.last;
                                for (int i = 0; i < PassengerDataDetailsList.Count; i++)
                                {
                                    if (returnPassengersobj.passengerTypeCode == PassengerDataDetailsList[i].passengertypecode && returnPassengersobj.name.first.ToLower() == PassengerDataDetailsList[i].first.ToLower() && returnPassengersobj.name.last.ToLower() == PassengerDataDetailsList[i].last.ToLower())
                                    {
                                        returnPassengersobj.MobNumber = PassengerDataDetailsList[i].mobile;
                                        returnPassengersobj.passengerKey = PassengerDataDetailsList[i].passengerkey;

                                        break;
                                    }

                                }
                                ReturnpassengersList.Add(returnPassengersobj);

                                //julian date
                                int year = 2024;
                                int month = 07;
                                int day = 02;

                                // Calculate the number of days from January 1st to the given date
                                DateTime currentDate = new DateTime(year, month, day);
                                DateTime startOfYear = new DateTime(year, 1, 1);
                                int julianDate = (currentDate - startOfYear).Days + 1;

                                if (items.Value.infant != null)
                                {
                                    returnPassengersobj = new ReturnPassengers();
                                    returnPassengersobj.name = new Name();
                                    returnPassengersobj.passengerTypeCode = "INFT";
                                    returnPassengersobj.name.first = items.Value.infant.name.first;
                                    returnPassengersobj.name.last = items.Value.infant.name.last;
                                    for (int i = 0; i < PassengerDataDetailsList.Count; i++)
                                    {
                                        if (returnPassengersobj.passengerTypeCode == PassengerDataDetailsList[i].passengertypecode && returnPassengersobj.name.first.ToLower() == PassengerDataDetailsList[i].first.ToLower() && returnPassengersobj.name.last.ToLower() == PassengerDataDetailsList[i].last.ToLower())
                                        {
                                            returnPassengersobj.passengerKey = PassengerDataDetailsList[i].passengerkey;
                                            break;
                                        }

                                    }
                                    ReturnpassengersList.Add(returnPassengersobj);

                                }

                            }

                            returnTicketBooking.breakdown = breakdown;
                            returnTicketBooking.journeys = journeysreturnList;
                            returnTicketBooking.passengers = ReturnpassengersList;
                            returnTicketBooking.passengerscount = Returnpassengercount;
                            returnTicketBooking.PhoneNumbers = phoneNumberList;
                            returnTicketBooking.totalAmount = totalAmount;
                            returnTicketBooking.taxMinusMeal = taxMinusMeal;
                            returnTicketBooking.taxMinusBaggage = taxMinusBaggage;
                            returnTicketBooking.totalMealTax = totalMealTax;
                            returnTicketBooking.totalAmountBaggage = totalAmountBaggage;
                            returnTicketBooking.TotalAmountMeal = TotalAmountMeal;
                            returnTicketBooking.TotaAmountBaggage = TotaAmountBaggage;
                            returnTicketBooking.Seatdata = htseatdata;
                            returnTicketBooking.Mealdata = htmealdata;
                            returnTicketBooking.Bagdata = htBagdata;
                            returnTicketBooking.htname = htname;
                            returnTicketBooking.htnameempty = htnameempty;
                            returnTicketBooking.htpax = htpax;
                            _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);

                            airLineFlightTicketBooking.BookingID = JsonObjPNRBooking.data.bookingKey;
                            #region DB Save
                            tb_Booking = new tb_Booking();
                            tb_Booking.AirLineID = 1;
                            string productcode = JsonObjPNRBooking.data.journeys[0].segments[0].fares[0].productClass;
                            var fareName = FareList.GetAllfare().Where(x => ((string)productcode).Equals(x.ProductCode)).FirstOrDefault();
                            tb_Booking.BookingType = "Corporate-" + JsonObjPNRBooking.data.journeys[0].segments[0].fares[0].productClass + " (" + fareName.Faredesc + ")";
                            LegalEntity legal = new LegalEntity();
                            legal = _mongoDBHelper.GetlegalEntityByGUID(Guid).Result;
                            if (legal != null)
                            {
                                tb_Booking.CompanyName = legal.BillingEntityFullName;
                            }
                            else
                                tb_Booking.CompanyName = "";
                            tb_Booking.BookingRelationId = Guid;
                            if (k1 == 0)
                            {
                                tb_Booking.TripType = "RoundTrip";
                            }
                            else
                            {
                                tb_Booking.TripType = "ReturnTrip";
                            }
                            tb_Booking.BookingID = JsonObjPNRBooking.data.bookingKey;
                            tb_Booking.RecordLocator = JsonObjPNRBooking.data.recordLocator;
                            if (JsonObjPNRBooking.data.recordLocator == null)
                            {
                                return View("service-error-msg");
                            }
                            tb_Booking.CurrencyCode = JsonObjPNRBooking.data.currencyCode;
                            tb_Booking.Origin = JsonObjPNRBooking.data.journeys[0].designator.origin;
                            tb_Booking.Destination = JsonObjPNRBooking.data.journeys[0].designator.destination;
                            tb_Booking.BookedDate = JsonObjPNRBooking.data.info.bookedDate;
                            tb_Booking.TotalAmount = JsonObjPNRBooking.data.breakdown.totalAmount;
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total != null)
                                {
                                    tb_Booking.SpecialServicesTotal = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total;
                                }
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes != null)
                                {
                                    tb_Booking.SpecialServicesTotal_Tax = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes;
                                }
                            }
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total > 0 || JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total != null)
                                {
                                    tb_Booking.SeatTotalAmount = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total;
                                    tb_Booking.SeatTotalAmount_Tax = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.taxes;
                                    if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments != null)
                                        tb_Booking.SeatAdjustment = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments;
                                }
                            }
                            tb_Booking.ExpirationDate = JsonObjPNRBooking?.data?.info?.expirationDate != null ? Convert.ToDateTime(JsonObjPNRBooking.data.info.expirationDate) : DateTime.Now;
                            tb_Booking.ArrivalDate = JsonObjPNRBooking.data.journeys[0].designator.arrival;
                            tb_Booking.DepartureDate = JsonObjPNRBooking.data.journeys[0].designator.departure;
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                tb_Booking.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate);
                            if (HttpContext.User.Identity.IsAuthenticated)
                            {
                                var identity = (ClaimsIdentity)User.Identity;
                                IEnumerable<Claim> claims = identity.Claims;
                                var userEmail = claims.Where(c => c.Type == ClaimTypes.Email).ToList()[0].Value;
                                tb_Booking.Createdby = userEmail;// "Online";
                            }
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                tb_Booking.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate);
                            tb_Booking.ModifyBy = JsonObjPNRBooking.data.info.modifiedAgentId;//"Online";
                            tb_Booking.BookingDoc = _responcePNRBooking;
                            tb_Booking.BookingStatus = JsonObjPNRBooking.data.info.status;// "2";
                            tb_Booking.PaidStatus = Convert.ToInt32(JsonObjPNRBooking.data.info.paidStatus);// "0";
                            // It  will maintained by manually as Airline Code and description 6E-Indigo
                            tb_Airlines = new tb_Airlines();
                            tb_Airlines.AirlineID = 1;
                            tb_Airlines.AirlneName = JsonObjPNRBooking.data.info.owningCarrierCode;// "Boing";
                            tb_Airlines.AirlineDescription = "AirIndia Express";
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                tb_Airlines.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                            tb_Airlines.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                tb_Airlines.Modifieddate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate);// DateTime.Now;
                            tb_Airlines.Modifyby = JsonObjPNRBooking.data.info.modifiedAgentId; //"Online";
                            tb_Airlines.Status = JsonObjPNRBooking.data.info.status; //"0";
                            //It  will maintained by manually from Getseatmap Api
                            tb_AirCraft = new tb_AirCraft();
                            tb_AirCraft.Id = 1;
                            tb_AirCraft.AirlineID = 1;
                            tb_AirCraft.AirCraftName = "";// "Airbus"; to do
                            tb_AirCraft.AirCraftDescription = " ";// " City Squares Worldwide"; to do
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                tb_AirCraft.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                tb_AirCraft.Modifieddate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate);// DateTime.Now;
                            tb_AirCraft.Createdby = JsonObjPNRBooking.data.info.createdAgentId;// "Online";
                            tb_AirCraft.Modifyby = JsonObjPNRBooking.data.info.modifiedAgentId;// "Online";
                            tb_AirCraft.Status = JsonObjPNRBooking.data.info.status; //"0";
                            contactDetail = new ContactDetail();
                            contactDetail.BookingID = JsonObjPNRBooking.data.bookingKey;
                            contactDetail.FirstName = JsonObjPNRBooking.data.contacts.P.name.first;
                            contactDetail.LastName = JsonObjPNRBooking.data.contacts.P.name.last;
                            contactDetail.EmailID = JsonObjPNRBooking.data.contacts.P.emailAddress;
                            //contactDetail.MobileNumber = Convert.ToInt32(Regex.Replace(JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString(), @"^\+91", "")); // todo

                            if (JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-').Length > 1)
                            {
                                contactDetail.MobileNumber = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[1];
                                contactDetail.CountryCode = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[0];
                            }
                            else
                            {
                                contactDetail.MobileNumber = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString();
                                contactDetail.CountryCode = "+91";
                            }

                            //contactDetail.MobileNumber = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[1];
                            //contactDetail.CountryCode = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[0];
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                contactDetail.CreateDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                            contactDetail.CreateBy = JsonObjPNRBooking.data.info.createdAgentId; //"Admin";
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                contactDetail.ModifyDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                            contactDetail.ModifyBy = JsonObjPNRBooking.data.info.modifiedAgentId; //"Admin";
                            contactDetail.Status = JsonObjPNRBooking.data.info.status;// 0;
                            gSTDetails = new GSTDetails();
                            if (JsonObjPNRBooking.data.contacts.G != null)
                            {
                                gSTDetails.bookingReferenceNumber = JsonObjPNRBooking.data.bookingKey;
                                gSTDetails.GSTEmail = JsonObjPNRBooking.data.contacts.G.emailAddress;
                                gSTDetails.GSTNumber = JsonObjPNRBooking.data.contacts.G.customerNumber;
                                gSTDetails.GSTName = JsonObjPNRBooking.data.contacts.G.companyName;
                                gSTDetails.airLinePNR = JsonObjPNRBooking.data.recordLocator;
                                gSTDetails.status = JsonObjPNRBooking.data.info.status; //0;
                            }
                            tb_PassengerTotalobj = new tb_PassengerTotal();
                            bookingKey = JsonObjPNRBooking.data.bookingKey;
                            tb_PassengerTotalobj.BookingID = JsonObjPNRBooking.data.bookingKey;
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total != null)
                                {
                                    tb_PassengerTotalobj.SpecialServicesAmount = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total;
                                }
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes != null)
                                {
                                    tb_PassengerTotalobj.SpecialServicesAmount_Tax = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes;
                                }
                            }
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total > 0 || JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total != null)
                                {
                                    tb_PassengerTotalobj.TotalSeatAmount = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total;
                                    tb_PassengerTotalobj.TotalSeatAmount_Tax = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.taxes;
                                    if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments != null)
                                        tb_PassengerTotalobj.SeatAdjustment = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments;
                                }
                            }
                            double inftAmt = 0;
                            double infttax = 0;
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.infant != null)
                            {
                                inftAmt = JsonObjPNRBooking.data.breakdown.passengerTotals.infant.total;
                                infttax = JsonObjPNRBooking.data.breakdown.passengerTotals.infant.taxes;

                            }
                            tb_PassengerTotalobj.TotalBookingAmount = JsonObjPNRBooking.data.breakdown.journeyTotals.totalAmount + inftAmt;
                            tb_PassengerTotalobj.totalBookingAmount_Tax = JsonObjPNRBooking.data.breakdown.journeyTotals.totalTax + infttax;
                            tb_PassengerTotalobj.Modifyby = JsonObjPNRBooking.data.info.createdDate;// "Online";
                            tb_PassengerTotalobj.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                            tb_PassengerTotalobj.Status = JsonObjPNRBooking.data.info.status; //"0";
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                tb_PassengerTotalobj.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate);// DateTime.Now;
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                tb_PassengerTotalobj.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                            var passangerCount = JsonObjPNRBooking.data.passengers;
                            int PassengerDataCount = ((Newtonsoft.Json.Linq.JContainer)passangerCount).Count;
                            int Adult = 0;
                            int child = 0;
                            int Infant = 0;
                            for (int i = 0; i < PassengerDataDetailsList.Count; i++)
                            {
                                if (PassengerDataDetailsList[i].passengertypecode == "ADT")
                                {
                                    Adult++;
                                }
                                else if (PassengerDataDetailsList[i].passengertypecode == "CHD" || PassengerDataDetailsList[i].passengertypecode == "CNN")
                                {
                                    child++;
                                }
                                else if (PassengerDataDetailsList[i].passengertypecode == "INFT" || PassengerDataDetailsList[i].passengertypecode == "INF")
                                {
                                    Infant++;
                                }
                            }
                            tb_PassengerTotalobj.AdultCount = Adult;
                            tb_PassengerTotalobj.ChildCount = child;
                            tb_PassengerTotalobj.InfantCount = Infant;
                            tb_PassengerTotalobj.TotalPax = Adult + child + Infant;
                            Hashtable htPaxAmount = new Hashtable();
                            int JourneysCount = JsonObjPNRBooking.data.journeys.Count;
                            int SegmentCount = JsonObjPNRBooking.data.journeys[0].segments.Count;
                            for (int i = 0; i < JourneysCount; i++)
                            {
                                for (int ia = 0; ia < SegmentCount; ia++)
                                {
                                    for (int k = 0; k < JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares.Count; k++)
                                    {
                                        double Amt = 0.0;
                                        double tax = 0.0;
                                        for (int ka = 0; ka < JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges.Count; ka++)
                                        {
                                            if (JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges[ka].type.ToString() == "0")
                                            {
                                                Amt = Convert.ToDouble(JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges[ka].amount);
                                            }
                                            else
                                            {
                                                tax += Convert.ToDouble(JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges[ka].amount);
                                            }
                                        }
                                        htPaxAmount.Add(JsonObjPNRBooking.data.journeys[0].segments[ia].designator.origin.ToString() + "_" + JsonObjPNRBooking.data.journeys[0].segments[ia].designator.destination.ToString() + "_" + JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].passengerType.ToString(), Amt + "/" + tax);
                                    }
                                }
                            }
                            tb_PassengerDetailsList = new List<tb_PassengerDetails>();
                            SegmentCount = JsonObjPNRBooking.data.journeys[0].segments.Count;
                            for (int isegment = 0; isegment < SegmentCount; isegment++)
                            {
                                foreach (var items in JsonObjPNRBooking.data.passengers)
                                {
                                    tb_PassengerDetails tb_Passengerobj = new tb_PassengerDetails();
                                    tb_Passengerobj.BookingID = bookingKey;
                                    tb_Passengerobj.SegmentsKey = JsonObjPNRBooking.data.journeys[0].segments[isegment].segmentKey;
                                    tb_Passengerobj.PassengerKey = items.Value.passengerKey;
                                    tb_Passengerobj.TypeCode = items.Value.passengerTypeCode;
                                    tb_Passengerobj.FirstName = items.Value.name.first;
                                    tb_Passengerobj.Title = items.Value.name.title;
                                    tb_Passengerobj.LastName = items.Value.name.last;
                                    tb_Passengerobj.contact_Emailid = PassengerDataDetailsList.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).Email;
                                    tb_Passengerobj.contact_Mobileno = PassengerDataDetailsList.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).mobile;
                                    tb_Passengerobj.FastForwardService = 'N';
                                    //tb_Passengerobj.FrequentFlyerNumber = PassengerDataDetailsList.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).FrequentFlyer;
                                    tb_Passengerobj.FrequentFlyerNumber = "";
                                    if (tb_Passengerobj.Title == "MR" || tb_Passengerobj.Title == "Master" || tb_Passengerobj.Title == "MSTR")
                                        tb_Passengerobj.Gender = "Male";
                                    else if (tb_Passengerobj.Title == "MS" || tb_Passengerobj.Title == "MRS" || tb_Passengerobj.Title == "MISS")
                                        tb_Passengerobj.Gender = "Female";
                                    int JourneysReturnCount1 = JsonObjPNRBooking.data.journeys.Count;
                                    if (JsonObjPNRBooking.data.journeys[0].segments[isegment].passengerSegment[tb_Passengerobj.PassengerKey].seats != null && JsonObjPNRBooking.data.journeys[0].segments[isegment].passengerSegment[tb_Passengerobj.PassengerKey].seats.Count > 0)
                                    {
                                        var flightseatnumber1 = JsonObjPNRBooking.data.journeys[0].segments[isegment].passengerSegment[tb_Passengerobj.PassengerKey].seats[0].unitDesignator;
                                        tb_Passengerobj.Seatnumber = flightseatnumber1;
                                    }
                                    string key = JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.origin.ToString() + "_" +
                     JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.destination.ToString() + "_" +
                     tb_Passengerobj.TypeCode.ToString();
                                    if (htPaxAmount.ContainsKey(key))
                                    {
                                        string[] parts = htPaxAmount[key].ToString().Split('/');
                                        tb_Passengerobj.TotalAmount = Convert.ToDecimal(parts[0]);
                                        tb_Passengerobj.TotalAmount_tax = Convert.ToDecimal(parts[1]);
                                    }
                                    else
                                    {
                                        tb_Passengerobj.TotalAmount = 0.0M;
                                        tb_Passengerobj.TotalAmount_tax = 0.0M;
                                    }
                                    if (JsonObjPNRBooking.data.info.createdDate != null)
                                        tb_Passengerobj.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                                    tb_Passengerobj.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                                    if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                        tb_Passengerobj.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                                    tb_Passengerobj.ModifyBy = JsonObjPNRBooking.data.info.modifiedAgentId; //"Online";
                                    tb_Passengerobj.Status = JsonObjPNRBooking.data.info.status; //"0";
                                    if (items.Value.infant != null && isegment == 0)
                                    {
                                        tb_Passengerobj.Inf_TypeCode = "INFT";
                                        tb_Passengerobj.Inf_Firstname = items.Value.infant.name.first;
                                        tb_Passengerobj.Inf_Lastname = items.Value.infant.name.last;
                                        tb_Passengerobj.Inf_Dob = items.Value.infant.dateOfBirth;
                                        if (items.Value.infant.gender == "1")
                                        {
                                            tb_Passengerobj.Inf_Gender = "Master";
                                        }
                                        if (isegment == 0)
                                        {
                                            for (int i = 0; i < items.Value.infant.fees[0].serviceCharges.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    tb_Passengerobj.InftAmount = items.Value.infant.fees[0].serviceCharges[0].amount;
                                                }
                                                else
                                                {
                                                    tb_Passengerobj.InftAmount_Tax += Convert.ToDouble(items.Value.infant.fees[0].serviceCharges[i].amount);
                                                }
                                            }
                                            tb_Passengerobj.InftAmount = Convert.ToDouble(items.Value.infant.fees[0].serviceCharges[0].amount) - tb_Passengerobj.InftAmount_Tax;
                                        }
                                        else
                                        {
                                            tb_Passengerobj.InftAmount = 0.0;// to do
                                            tb_Passengerobj.InftAmount_Tax = 0.0;// to do
                                        }
                                        for (int i = 0; i < PassengerDataDetailsList.Count; i++)
                                        {
                                            if (tb_Passengerobj.Inf_TypeCode == PassengerDataDetailsList[i].passengertypecode && tb_Passengerobj.Inf_Firstname.ToLower() == PassengerDataDetailsList[i].first.ToLower() + " " + PassengerDataDetailsList[i].last.ToLower())
                                            {
                                                tb_Passengerobj.PassengerKey = PassengerDataDetailsList[i].passengerkey;
                                                break;
                                            }
                                        }
                                    }
                                    string oridest = JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.origin + JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.destination;
                                    // Handle carrybages and fees
                                    List<FeeDetails> feeDetails = new List<FeeDetails>();
                                    double TotalAmount_Seat = 0;
                                    decimal TotalAmount_Seat_tax = 0;
                                    decimal TotalAmount_Seat_discount = 0;
                                    double TotalAmount_Meals = 0;
                                    decimal TotalAmount_Meals_tax = 0;
                                    decimal TotalAmount_Meals_discount = 0;
                                    double TotalAmount_Baggage = 0;
                                    decimal TotalAmount_Baggage_tax = 0;
                                    decimal TotalAmount_Baggage_discount = 0;
                                    string carryBagesConcatenation = "";
                                    string MealConcatenation = "";
                                    int feesCount = items.Value.fees.Count;
                                    foreach (var fee in items.Value.fees)
                                    {
                                        string ssrCode = fee.ssrCode?.ToString();
                                        if (!string.IsNullOrEmpty(ssrCode))
                                        {
                                            if (ssrCode.StartsWith("P"))
                                            {
                                                if (fee.flightReference.ToString().Contains(oridest) == true)
                                                {
                                                    //TicketCarryBag[tb_Passengerobj.PassengerKey.ToString()] = fee.ssrCode;
                                                    var BaggageName = MealImageList.GetAllmeal()
                                                                    .Where(x => ((string)fee.ssrCode).Contains(x.MealCode))
                                                                    .Select(x => x.MealImage)
                                                                    .FirstOrDefault();
                                                    carryBagesConcatenation += fee.ssrCode + "-" + BaggageName + ",";
                                                }
                                            }
                                            else
                                            {
                                                if (fee.flightReference.ToString().Contains(oridest) == true)
                                                {
                                                    //TicketMeal[tb_Passengerobj.PassengerKey.ToString()] = fee.ssrCode;
                                                    var MealName = MealImageList.GetAllmeal()
                                                                    .Where(x => ((string)fee.ssrCode).Contains(x.MealCode))
                                                                    .Select(x => x.MealImage)
                                                                    .FirstOrDefault();
                                                    MealConcatenation += fee.ssrCode + "-" + MealName + ",";
                                                }
                                            }
                                        }
                                        Hashtable TicketMealTax = new Hashtable();
                                        Hashtable TicketMealAmountTax = new Hashtable();
                                        Hashtable TicketCarryBagAMountTax = new Hashtable();
                                        // Iterate through service charges
                                        int ServiceCount = fee.serviceCharges.Count;
                                        if (fee.code.ToString().StartsWith("SE"))
                                        {
                                            foreach (var serviceCharge in fee.serviceCharges)
                                            {
                                                string serviceChargeCode = serviceCharge.code?.ToString();
                                                double amount = (serviceCharge.amount != null) ? Convert.ToDouble(serviceCharge.amount) : 0;
                                                if (serviceChargeCode != null)
                                                {
                                                    if (fee.flightReference.ToString().Contains(oridest) == true)
                                                    {
                                                        if (serviceChargeCode.StartsWith("SE") && serviceCharge.type == "6")
                                                        {
                                                            TotalAmount_Seat = amount;
                                                            //TicketSeat[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Seat;
                                                        }
                                                        else if (serviceCharge.type == "3")
                                                        {
                                                            TotalAmount_Seat_tax += Convert.ToDecimal(amount);
                                                        }
                                                        else if (serviceCharge.type == "1")
                                                        {
                                                            TotalAmount_Seat_discount += Convert.ToDecimal(amount);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (!fee.code.ToString().StartsWith("P") && !fee.code.ToString().StartsWith("SE"))
                                        {
                                            foreach (var serviceCharge in fee.serviceCharges)
                                            {
                                                string serviceChargeCode = serviceCharge.code?.ToString();
                                                double amount = (serviceCharge.amount != null) ? Convert.ToDouble(serviceCharge.amount) : 0;
                                                if (serviceChargeCode != null)
                                                {
                                                    if (fee.flightReference.ToString().Contains(oridest) == true)
                                                    {
                                                        if (serviceCharge.type == "6")
                                                        {
                                                            TotalAmount_Meals = amount;
                                                            //TicketMealAmount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Meals;
                                                        }
                                                        else if (serviceCharge.type == "3")
                                                        {
                                                            TotalAmount_Meals_tax += Convert.ToDecimal(amount);
                                                        }
                                                        else if (serviceCharge.type == "1")
                                                        {
                                                            TotalAmount_Meals_discount += Convert.ToDecimal(amount);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (fee.code.ToString().StartsWith("P"))
                                        {
                                            foreach (var serviceCharge in fee.serviceCharges)
                                            {
                                                string serviceChargeCode = serviceCharge.code?.ToString();
                                                double amount = (serviceCharge.amount != null) ? Convert.ToDouble(serviceCharge.amount) : 0;
                                                if (serviceChargeCode != null && isegment == 0)
                                                {
                                                    if (serviceChargeCode.StartsWith("P") && serviceCharge.type == "6")
                                                    {
                                                        TotalAmount_Baggage = amount;
                                                        //TicketCarryBagAMount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Baggage;
                                                    }
                                                    else if (serviceCharge.type == "3")
                                                    {
                                                        TotalAmount_Baggage_tax += Convert.ToDecimal(amount);
                                                    }
                                                    else if (serviceCharge.type == "1")
                                                    {
                                                        TotalAmount_Baggage_discount += Convert.ToDecimal(amount);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    tb_Passengerobj.TotalAmount_Seat = TotalAmount_Seat;
                                    tb_Passengerobj.TotalAmount_Seat_tax = TotalAmount_Seat_tax;
                                    tb_Passengerobj.TotalAmount_Seat_tax_discount = TotalAmount_Seat_discount;
                                    tb_Passengerobj.TotalAmount_Meals = TotalAmount_Meals;
                                    tb_Passengerobj.TotalAmount_Meals_tax = Convert.ToDouble(TotalAmount_Meals_tax);
                                    tb_Passengerobj.TotalAmount_Meals_discount = Convert.ToDouble(TotalAmount_Meals_discount);
                                    tb_Passengerobj.BaggageTotalAmount = TotalAmount_Baggage;
                                    tb_Passengerobj.BaggageTotalAmountTax = TotalAmount_Baggage_tax;
                                    tb_Passengerobj.BaggageTotalAmountTax_discount = TotalAmount_Baggage_discount;
                                    tb_Passengerobj.Carrybages = carryBagesConcatenation.TrimEnd(',');
                                    tb_Passengerobj.MealsCode = MealConcatenation.TrimEnd(',');
                                    tb_PassengerDetailsList.Add(tb_Passengerobj);
                                }
                            }
                            JourneysCount = JsonObjPNRBooking.data.journeys.Count;
                            tb_JourneysList = new List<tb_journeys>();
                            segmentReturnsListt = new List<tb_Segments>();
                            Hashtable seatNumber = new Hashtable();
                            for (int i = 0; i < JourneysCount; i++)
                            {
                                tb_journeys tb_JourneysObj = new tb_journeys();
                                tb_JourneysObj.BookingID = JsonObjPNRBooking.data.bookingKey;
                                tb_JourneysObj.JourneyKey = JsonObjPNRBooking.data.journeys[i].journeyKey;
                                tb_JourneysObj.Stops = JsonObjPNRBooking.data.journeys[i].stops;
                                tb_JourneysObj.JourneyKeyCount = i;
                                tb_JourneysObj.FlightType = JsonObjPNRBooking.data.journeys[i].flightType;
                                tb_JourneysObj.Origin = JsonObjPNRBooking.data.journeys[i].designator.origin;
                                tb_JourneysObj.Destination = JsonObjPNRBooking.data.journeys[i].designator.destination;
                                tb_JourneysObj.DepartureDate = JsonObjPNRBooking.data.journeys[i].designator.departure;
                                tb_JourneysObj.ArrivalDate = JsonObjPNRBooking.data.journeys[i].designator.arrival;
                                if (JsonObjPNRBooking.data.info.createdDate != null)
                                    tb_JourneysObj.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                                tb_JourneysObj.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                                if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                    tb_JourneysObj.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                                tb_JourneysObj.Modifyby = JsonObjPNRBooking.data.info.modifiedAgentId; //"Online";
                                tb_JourneysObj.Status = JsonObjPNRBooking.data.info.status; //"0";
                                tb_JourneysList.Add(tb_JourneysObj);
                                int SegmentReturnCountt = JsonObjPNRBooking.data.journeys[0].segments.Count;
                                for (int j = 0; j < SegmentReturnCountt; j++)
                                {
                                    tb_Segments segmentReturnobj = new tb_Segments();
                                    segmentReturnobj.BookingID = JsonObjPNRBooking.data.bookingKey;
                                    segmentReturnobj.journeyKey = JsonObjPNRBooking.data.journeys[0].journeyKey;
                                    segmentReturnobj.SegmentKey = JsonObjPNRBooking.data.journeys[0].segments[j].segmentKey;
                                    segmentReturnobj.SegmentCount = j;
                                    segmentReturnobj.Origin = JsonObjPNRBooking.data.journeys[0].segments[j].designator.origin;
                                    segmentReturnobj.Destination = JsonObjPNRBooking.data.journeys[0].segments[j].designator.destination;
                                    segmentReturnobj.DepartureDate = JsonObjPNRBooking.data.journeys[0].segments[j].designator.departure;
                                    segmentReturnobj.ArrivalDate = JsonObjPNRBooking.data.journeys[0].segments[j].designator.arrival;
                                    segmentReturnobj.Identifier = JsonObjPNRBooking.data.journeys[0].segments[j].identifier.identifier;
                                    segmentReturnobj.CarrierCode = JsonObjPNRBooking.data.journeys[0].segments[j].identifier.carrierCode;
                                    segmentReturnobj.Seatnumber = ""; // to do
                                    segmentReturnobj.MealCode = ""; // to do
                                    segmentReturnobj.MealDiscription = "";// "it is a coffe"; // to fo
                                    var LegReturn = JsonObjPNRBooking.data.journeys[i].segments[j].legs;
                                    int Legcount = ((Newtonsoft.Json.Linq.JContainer)LegReturn).Count;
                                    List<LegReturn> legReturnsList = new List<LegReturn>();
                                    for (int n = 0; n < Legcount; n++)
                                    {
                                        if (JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal != null)
                                            segmentReturnobj.DepartureTerminal = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal;  // to do
                                        if (JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal != null)
                                            segmentReturnobj.ArrivalTerminal = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal; // to do
                                    }
                                    if (JsonObjPNRBooking.data.info.createdDate != null)
                                        segmentReturnobj.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                                    if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                        segmentReturnobj.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                                    segmentReturnobj.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                                    segmentReturnobj.Modifyby = JsonObjPNRBooking.data.info.modifiedAgentId; //"Online";
                                    segmentReturnobj.Status = JsonObjPNRBooking.data.info.status; //;
                                    segmentReturnsListt.Add(segmentReturnobj);
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                            _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);
                        }
                        #endregion

                    }

                    //Akasa Air Line Commit Booking
                    var BookingKeyAkasa = string.Empty;
                    tokenview = string.Empty;

                    token = string.Empty;
                    if (string.IsNullOrEmpty(tokenview) && flagAirAsia == true && data.Airline[k1].ToLower().Contains("akasaair"))
                    {
                        double totalAmount = 0;
                        double totalAmountBaggage = 0;
                        double totalAmounttax = 0;
                        double totalAmounttaxSGST = 0;
                        double totalAmounttaxBag = 0;
                        double totalAmounttaxSGSTBag = 0;
                        double totalMealTax = 0;
                        double totalBaggageTax = 0;
                        double taxMinusMeal = 0;
                        double taxMinusBaggage = 0;
                        double TotalAmountMeal = 0;
                        double TotaAmountBaggage = 0;

                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;

                        if (k1 == 0)
                        {
                            tokenview = tokenData.Token; // HttpContext.Session.GetString("AkasaTokan");
                        }
                        else
                        {
                            tokenview = tokenData.RToken; // HttpContext.Session.GetString("AkasaTokanR");
                        }
                        token = tokenview;
                        #region Get Booking

                        //GetBOoking FRom State
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responceGetBookingSate = await client.GetAsync(AppUrlConstant.AkasaAirGetBooking);
                        if (responceGetBookingSate.IsSuccessStatusCode)
                        {
                            string _responceGetBooking = responceGetBookingSate.Content.ReadAsStringAsync().Result;
                            if (k1 == 0)
                            {
                                logs.WriteLogsR(AppUrlConstant.AkasaAirGetBooking, "14-GetBookingRequest_Left", "AkasaRT");
                                logs.WriteLogsR(_responceGetBooking, "14-GetBookingResponse_Left", "AkasaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(AppUrlConstant.AkasaAirGetBooking, "14-GetBookingRequest_Right", "AkasaRT");
                                logs.WriteLogsR(_responceGetBooking, "14-GetBookingResponse_Right", "AkasaRT");
                            }

                            var DataBooking = JsonConvert.DeserializeObject<dynamic>(_responceGetBooking);
                            decimal Totalpayment = 0M;
                            if (_responceGetBooking != null)
                            {
                                Totalpayment = DataBooking.data.breakdown.totalAmount;
                            }
                            //ADD Payment
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                            // Payment request payload
                            PaymentRequest paymentRequest = new PaymentRequest();
                            paymentRequest.PaymentMethodCode = "AG";
                            paymentRequest.Amount = Totalpayment;
                            paymentRequest.PaymentFields = new PaymentFields();
                            paymentRequest.PaymentFields.ACCTNO = "QPDEL5019C";
                            paymentRequest.PaymentFields.AMT = Totalpayment;
                            paymentRequest.CurrencyCode = "INR";
                            paymentRequest.Installments = 1;

                            // Serializing the payload to JSON
                            string jsonPayload = JsonConvert.SerializeObject(paymentRequest);
                            HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                            // Sending the POST request
                            string url = AppUrlConstant.AkasaAirPayment;

                            HttpResponseMessage response = await client.PostAsync(url, content);
                            string responseContent = await response.Content.ReadAsStringAsync();
                            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                            if (k1 == 0)
                            {
                                logs.WriteLogsR(jsonPayload, "15-AddPaymentRequest_Left", "AkasaRT");
                                logs.WriteLogsR(responseContent, "15-AddPaymentResponse_Left", "AkasaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(jsonPayload, "15-AddPaymentRequest_Right", "AkasaRT");
                                logs.WriteLogsR(responseContent, "15-AddPaymentResponse_Right", "AkasaRT");
                            }

                        }
                        Commit_BookingModel _Commit_BookingModel = new Commit_BookingModel();
                        _Commit_BookingModel.receivedBy = null;
                        _Commit_BookingModel.restrictionOverride = false;
                        _Commit_BookingModel.hold = null;
                        _Commit_BookingModel.notifyContacts = false;
                        _Commit_BookingModel.comments = null;
                        _Commit_BookingModel.contactTypesToNotify = null;
                        var jsonCommitBookingRequest = JsonConvert.SerializeObject(_Commit_BookingModel, Formatting.Indented);

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responceCommit_Booking = await client.PostAsJsonAsync(AppUrlConstant.AkasaGetBoking, _Commit_BookingModel);


                        if (responceCommit_Booking.IsSuccessStatusCode)
                        {
                            var _responceCommit_Booking = responceCommit_Booking.Content.ReadAsStringAsync().Result;
                            if (k1 == 0)
                            {
                                logs.WriteLogsR(jsonCommitBookingRequest, "16-CommitRequest_Left", "AkasaRT");
                                logs.WriteLogsR(_responceCommit_Booking, "16-CommitResponse_Left", "AkasaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(jsonCommitBookingRequest, "16-CommitRequest_Right", "AkasaRT");
                                logs.WriteLogsR(_responceCommit_Booking, "16-CommitResponse_Right", "AkasaRT");
                            }
                            var JsonObjCommit_Booking = JsonConvert.DeserializeObject<dynamic>(_responceCommit_Booking);

                        }
                        #endregion
                        #region AirLinePNR
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responcepnrBooking = await client.GetAsync(AppUrlConstant.AkasaPNRBooking + BookingKeyAkasa);
                        if (responcepnrBooking.IsSuccessStatusCode)
                        {
                            Hashtable htname = new Hashtable();
                            Hashtable htnameempty = new Hashtable();
                            Hashtable htpax = new Hashtable();
                            string sequencenumber = string.Empty;

                            Hashtable htseatdata = new Hashtable();
                            Hashtable htmealdata = new Hashtable();
                            Hashtable htBagdata = new Hashtable();
                            var _responcePNRBooking = responcepnrBooking.Content.ReadAsStringAsync().Result;
                            if (k1 == 0)
                            {
                                logs.WriteLogsR(AppUrlConstant.AkasaPNRBooking + BookingKeyAkasa.ToString(), "17-GetBookingPNRDeatilsRequest_Left", "AkasaRT");
                                logs.WriteLogsR(_responcePNRBooking, "17-GetBookingPNRDeatilsResponse_Left", "AkasaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(AppUrlConstant.AkasaPNRBooking + BookingKeyAkasa.ToString(), "17-GetBookingPNRDeatilsRequest_Right", "AkasaRT");
                                logs.WriteLogsR(_responcePNRBooking, "17-GetBookingPNRDeatilsResponse_Right", "AkasaRT");
                            }
                            var JsonObjPNRBooking = JsonConvert.DeserializeObject<dynamic>(_responcePNRBooking);
                            ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                            string PassengerData = HttpContext.Session.GetString("PassengerNameDetails");
                            List<passkeytype> PassengerDataDetailsList = JsonConvert.DeserializeObject<List<passkeytype>>(PassengerData);
                            returnTicketBooking.recordLocator = JsonObjPNRBooking.data.recordLocator;
                            BarcodePNR = JsonObjPNRBooking.data.recordLocator;
                            Info info = new Info();
                            info.bookedDate = JsonObjPNRBooking.data.info.bookedDate;
                            returnTicketBooking.info = info;
                            if (BarcodePNR != null && BarcodePNR.Length < 7)
                            {
                                BarcodePNR = BarcodePNR.PadRight(7);
                            }
                            returnTicketBooking.airLines = "AkasaAir";
                            returnTicketBooking.bookingKey = JsonObjPNRBooking.data.bookingKey;
                            Breakdown breakdown = new Breakdown();
                            breakdown.balanceDue = JsonObjPNRBooking.data.breakdown.totalAmount;

                            JourneyTotals journeyTotalsobj = new JourneyTotals();
                            journeyTotalsobj.totalAmount = JsonObjPNRBooking.data.breakdown.journeyTotals.totalAmount;
                            journeyTotalsobj.totalTax = JsonObjPNRBooking.data.breakdown.journeyTotals.totalTax;

                            var baseTotalAmount = journeyTotalsobj.totalAmount;
                            var BaseTotalTax = journeyTotalsobj.totalTax;

                            var ToatalBasePrice = journeyTotalsobj.totalAmount + journeyTotalsobj.totalTax;
                            //changes for Passeneger name:
                            foreach (var items in JsonObjPNRBooking.data.passengers)
                            {
                                htname.Add(items.Value.passengerKey.ToString(), items.Value.name.last.ToString() + "/" + items.Value.name.first.ToString());
                            }
                            InfantReturn infantReturnobj = new InfantReturn();
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.infant != null)
                            {
                                infantReturnobj.total = JsonObjPNRBooking.data.breakdown.passengerTotals.infant.total;
                                infantReturnobj.taxes = JsonObjPNRBooking.data.breakdown.passengerTotals.infant.taxes;
                                double TotalInfantAmount = infantReturnobj.total + infantReturnobj.taxes;
                                double totalAmountSum = journeyTotalsobj.totalAmount + infantReturnobj.total + infantReturnobj.taxes;
                                double totaltax = journeyTotalsobj.totalTax;

                                double totalplusAmountSumtax = totalAmountSum + totaltax;
                                breakdown.totalAmountSum = totalAmountSum;
                                breakdown.totaltax = totaltax;
                                breakdown.totalplusAmountSumtax = totalplusAmountSumtax;
                            }

                            PassengerTotals passengerTotals = new PassengerTotals();
                            SpecialServices serviceChargeReturn = new SpecialServices();
                            List<ReturnCharge> returnChargeList = new List<ReturnCharge>();
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices != null)
                            {
                                int chargesCount = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.charges.Count;

                                for (int ch = 0; ch < chargesCount; ch++)
                                {
                                    ReturnCharge returnChargeobj = new ReturnCharge();
                                    returnChargeobj.amount = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.charges[ch].amount;
                                    returnChargeobj.code = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.charges[ch].code;

                                    if (returnChargeobj.code.StartsWith("P"))
                                    {
                                        totalAmount += returnChargeobj.amount;

                                    }
                                    if (returnChargeobj.code.StartsWith("C"))
                                    {
                                        totalAmounttax += returnChargeobj.amount;
                                    }

                                    if (returnChargeobj.code.StartsWith("U"))
                                    {
                                        totalAmounttaxSGST += returnChargeobj.amount;
                                    }
                                    totalMealTax = totalAmounttax + totalAmounttaxSGST;
                                    taxMinusMeal = totalAmount - totalMealTax;
                                    TotalAmountMeal = totalMealTax + taxMinusMeal;

                                    if (returnChargeobj.code.StartsWith("X"))
                                    {
                                        totalAmountBaggage += returnChargeobj.amount;

                                    }
                                    if (returnChargeobj.code.StartsWith("C"))
                                    {
                                        totalAmounttaxBag += returnChargeobj.amount;
                                    }

                                    if (returnChargeobj.code.StartsWith("U"))
                                    {
                                        totalAmounttaxSGSTBag += returnChargeobj.amount;
                                    }
                                    totalBaggageTax = totalAmounttaxBag + totalAmounttaxSGSTBag;
                                    taxMinusBaggage = totalAmountBaggage - totalBaggageTax;
                                    TotaAmountBaggage = totalBaggageTax + taxMinusBaggage;


                                    returnChargeList.Add(returnChargeobj);
                                }
                                serviceChargeReturn.charges = returnChargeList;

                            }

                            ReturnSeats returnSeats = new ReturnSeats();
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total > 0 || JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total != null)
                                {
                                    returnSeats.total = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total;
                                    returnSeats.taxes = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.taxes;
                                    returnSeats.totalSeatAmount = returnSeats.total + returnSeats.taxes;

                                }
                            }
                            SpecialServices specialServices = new SpecialServices();
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices != null)
                            {
                                specialServices.total = (decimal)JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total;
                                specialServices.taxes = (decimal)JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes;
                            }
                            breakdown.journeyTotals = journeyTotalsobj;
                            breakdown.passengerTotals = passengerTotals;
                            breakdown.baseTotalAmount = baseTotalAmount + infantReturnobj.total;
                            breakdown.ToatalBasePrice = BaseTotalTax + infantReturnobj.taxes;
                            breakdown.BaseTotalTax = BaseTotalTax + infantReturnobj.taxes;
                            passengerTotals.seats = returnSeats;
                            passengerTotals.infant = infantReturnobj;
                            passengerTotals.specialServices = specialServices;
                            passengerTotals.specialServices = serviceChargeReturn;

                            if (JsonObjPNRBooking.data.contacts.G != null)
                            {
                                returnTicketBooking.customerNumber = JsonObjPNRBooking.data.contacts.G.customerNumber;
                                returnTicketBooking.companyName = JsonObjPNRBooking.data.contacts.G.companyName;
                                returnTicketBooking.emailAddressgst = JsonObjPNRBooking.data.contacts.G.emailAddress;
                            }
                            Contacts _contactobj = new Contacts();
                            int PhoneNumberCount = JsonObjPNRBooking.data.contacts.P.phoneNumbers.Count;
                            List<PhoneNumber> phoneNumberList = new List<PhoneNumber>();
                            for (int p = 0; p < PhoneNumberCount; p++)
                            {
                                PhoneNumber phoneobject = new PhoneNumber();
                                phoneobject.number = JsonObjPNRBooking.data.contacts.P.phoneNumbers[p].number;
                                phoneNumberList.Add(phoneobject);
                            }
                            int JourneysReturnCount = JsonObjPNRBooking.data.journeys.Count;
                            List<JourneysReturn> journeysreturnList = new List<JourneysReturn>();
                            for (int i = 0; i < JourneysReturnCount; i++)
                            {
                                JourneysReturn journeysReturnObj = new JourneysReturn();
                                journeysReturnObj.stops = JsonObjPNRBooking.data.journeys[i].stops;

                                DesignatorReturn ReturnDesignatorobject = new DesignatorReturn();
                                ReturnDesignatorobject.origin = JsonObjPNRBooking.data.journeys[i].designator.origin;
                                ReturnDesignatorobject.destination = JsonObjPNRBooking.data.journeys[i].designator.destination;
                                orides = JsonObjPNRBooking.data.journeys[i].designator.origin + JsonObjPNRBooking.data.journeys[i].designator.destination;
                                ReturnDesignatorobject.departure = JsonObjPNRBooking.data.journeys[i].designator.departure;
                                ReturnDesignatorobject.arrival = JsonObjPNRBooking.data.journeys[i].designator.arrival;

                                journeysReturnObj.designator = ReturnDesignatorobject;
                                int SegmentReturnCount = JsonObjPNRBooking.data.journeys[i].segments.Count;
                                List<SegmentReturn> segmentReturnsList = new List<SegmentReturn>();
                                for (int j = 0; j < SegmentReturnCount; j++)
                                {
                                    returnSeats.unitDesignator = string.Empty;
                                    returnSeats.SSRCode = string.Empty;
                                    SegmentReturn segmentReturnobj = new SegmentReturn();
                                    segmentReturnobj.isStandby = JsonObjPNRBooking.data.journeys[i].segments[j].isStandby;
                                    segmentReturnobj.isHosted = JsonObjPNRBooking.data.journeys[i].segments[j].isHosted;
                                    DesignatorReturn designatorReturn = new DesignatorReturn();
                                    //var cityname = Citydata.GetAllcity().Where(x => x.cityCode == "DEL");
                                    designatorReturn.origin = JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin;
                                    designatorReturn.destination = JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination;
                                    designatorReturn.departure = JsonObjPNRBooking.data.journeys[i].segments[j].designator.departure;
                                    designatorReturn.arrival = JsonObjPNRBooking.data.journeys[i].segments[j].designator.arrival;
                                    segmentReturnobj.designator = designatorReturn;
                                    orides = designatorReturn.origin + designatorReturn.destination;

                                    var passengersegmentCount = JsonObjPNRBooking.data.journeys[i].segments[j].passengerSegment;
                                    int passengerReturnCount = ((Newtonsoft.Json.Linq.JContainer)passengersegmentCount).Count;
                                    string dateString = JsonObjPNRBooking.data.journeys[i].designator.departure;
                                    DateTime date = DateTime.ParseExact(dateString, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                    //julian date
                                    int year = date.Year;
                                    int month = date.Month;
                                    int day = date.Day;

                                    // Calculate the number of days from January 1st to the given date
                                    DateTime currentDate = new DateTime(year, month, day);
                                    DateTime startOfYear = new DateTime(year, 1, 1);
                                    int julianDate = (currentDate - startOfYear).Days + 1;
                                    sequencenumber = SequenceGenerator.GetNextSequenceNumber();

                                    flightnumber = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.identifier;
                                    if (flightnumber.Length < 5)
                                    {
                                        flightnumber = flightnumber.PadRight(5);
                                    }
                                    carriercode = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.carrierCode;
                                    if (carriercode.Length < 3)
                                    {
                                        carriercode = carriercode.PadRight(3);
                                    }

                                    foreach (var items in JsonObjPNRBooking.data.passengers)
                                    {
                                        if (!htnameempty.Contains(items.Value.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination))
                                        {
                                            if (carriercode.Length < 3)
                                                carriercode = carriercode.PadRight(3);
                                            if (flightnumber.Length < 5)
                                            {
                                                flightnumber = flightnumber.PadRight(5);
                                            }
                                            if (sequencenumber.Length < 5)
                                                sequencenumber = sequencenumber.PadRight(5, '0');
                                            seatnumber = "0000";
                                            if (seatnumber.Length < 4)
                                                seatnumber = seatnumber.PadLeft(4, '0');
                                            BarcodeString = "M" + "1" + htname[items.Value.passengerKey.ToString()] + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                            htnameempty.Add(items.Value.passengerKey.ToString() + "_" + htname[items.Value.passengerKey.ToString()] + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, BarcodeString);
                                        }
                                    }
                                    List<PassengerSegment> passengerSegmentsList = new List<PassengerSegment>();
                                    foreach (var item in JsonObjPNRBooking.data.journeys[i].segments[j].passengerSegment)
                                    {
                                        PassengerSegment passengerSegmentobj = new PassengerSegment();
                                        passengerSegmentobj.passengerKey = item.Value.passengerKey;
                                        passengerSegmentsList.Add(passengerSegmentobj);
                                        int seatCount = item.Value.seats.Count;
                                        int ssrCodeCount = item.Value.ssrs.Count;
                                        List<ReturnSeats> returnSeatsList = new List<ReturnSeats>();
                                        for (int q = 0; q < seatCount; q++)
                                        {
                                            ReturnSeats returnSeatsObj = new ReturnSeats();
                                            returnSeatsObj.unitDesignator = item.Value.seats[q].unitDesignator;
                                            seatnumber = item.Value.seats[q].unitDesignator;
                                            if (string.IsNullOrEmpty(seatnumber))
                                            {
                                                seatnumber = "0000"; // Set to "0000" if not available
                                            }
                                            else
                                            {
                                                seatnumber = seatnumber.PadRight(4, '0'); // Right-pad with zeros if less than 4 characters
                                            }
                                            returnSeatsList.Add(returnSeatsObj);
                                            htseatdata.Add(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, returnSeatsObj.unitDesignator);
                                            returnSeats.unitDesignator += returnSeatsObj.unitDesignator + ",";
                                            if (!htpax.Contains(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination))
                                            {
                                                if (carriercode.Length < 3)
                                                    carriercode = carriercode.PadRight(3);
                                                if (flightnumber.Length < 5)
                                                {
                                                    flightnumber = flightnumber.PadRight(5);
                                                }
                                                if (sequencenumber.Length < 5)
                                                    sequencenumber = sequencenumber.PadRight(5, '0');
                                                seatnumber = htseatdata[passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination].ToString();
                                                if (seatnumber.Length < 4)
                                                    seatnumber = seatnumber.PadLeft(4, '0');
                                                BarcodeString = "M" + "1" + htname[passengerSegmentobj.passengerKey] + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                                htpax.Add(passengerSegmentobj.passengerKey.ToString() + "_" + htname[passengerSegmentobj.passengerKey] + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, BarcodeString);
                                            }
                                        }
                                        List<SsrReturn> SrrcodereturnsList = new List<SsrReturn>();
                                        for (int t = 0; t < ssrCodeCount; t++)
                                        {
                                            SsrReturn ssrReturn = new SsrReturn();
                                            ssrReturn.ssrCode = item.Value.ssrs[t].ssrCode;
                                            if (!ssrReturn.ssrCode.StartsWith("P") && !ssrReturn.ssrCode.StartsWith("C"))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (!htmealdata.Contains(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination))
                                                {


                                                    htmealdata.Add(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, ssrReturn.ssrCode);
                                                }
                                                returnSeats.SSRCode += ssrReturn.ssrCode + ",";

                                            }


                                        }
                                        for (int t = 0; t < ssrCodeCount; t++)
                                        {
                                            SsrReturn ssrReturn = new SsrReturn();
                                            ssrReturn.ssrCode = item.Value.ssrs[t].ssrCode;
                                            if (!ssrReturn.ssrCode.StartsWith("X"))
                                            {
                                                continue;
                                            }
                                            else
                                            {

                                                if (!htBagdata.Contains(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination))
                                                {
                                                    htBagdata.Add(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, ssrReturn.ssrCode);

                                                }
                                                returnSeats.SSRCode += ssrReturn.ssrCode + ",";
                                            }


                                        }

                                    }
                                    segmentReturnobj.passengerSegment = passengerSegmentsList;

                                    int ReturmFareCount = JsonObjPNRBooking.data.journeys[i].segments[j].fares.Count;
                                    List<FareReturn> fareList = new List<FareReturn>();
                                    for (int k = 0; k < ReturmFareCount; k++)
                                    {
                                        FareReturn fareReturnobj = new FareReturn();
                                        fareReturnobj.productClass = JsonObjPNRBooking.data.journeys[i].segments[j].fares[k].productClass;

                                        int PassengerFareReturnCount = JsonObjPNRBooking.data.journeys[i].segments[j].fares[k].passengerFares.Count;
                                        List<PassengerFareReturn> passengerFareReturnList = new List<PassengerFareReturn>();
                                        for (int l = 0; l < PassengerFareReturnCount; l++)
                                        {
                                            PassengerFareReturn passengerFareReturnobj = new PassengerFareReturn();

                                            int ServiceChargeReturnCount = JsonObjPNRBooking.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges.Count;

                                            List<ServiceChargeReturn> serviceChargeReturnList = new List<ServiceChargeReturn>();
                                            for (int m = 0; m < ServiceChargeReturnCount; m++)
                                            {
                                                ServiceChargeReturn serviceChargeReturnobj = new ServiceChargeReturn();

                                                serviceChargeReturnobj.amount = JsonObjPNRBooking.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges[m].amount;
                                                serviceChargeReturnList.Add(serviceChargeReturnobj);


                                            }
                                            passengerFareReturnobj.serviceCharges = serviceChargeReturnList;
                                            passengerFareReturnList.Add(passengerFareReturnobj);

                                        }
                                        fareReturnobj.passengerFares = passengerFareReturnList;
                                        fareList.Add(fareReturnobj);

                                    }
                                    segmentReturnobj.fares = fareList;

                                    IdentifierReturn identifierReturn = new IdentifierReturn();
                                    identifierReturn.identifier = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.identifier;
                                    flightnumber = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.identifier;
                                    if (flightnumber.Length < 5)
                                    {
                                        flightnumber = flightnumber.PadRight(5);
                                    }
                                    carriercode = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.carrierCode;
                                    if (carriercode.Length < 3)
                                    {
                                        carriercode = carriercode.PadRight(3);
                                    }
                                    identifierReturn.carrierCode = JsonObjPNRBooking.data.journeys[i].segments[j].identifier.carrierCode;
                                    segmentReturnobj.identifier = identifierReturn;

                                    var LegReturn = JsonObjPNRBooking.data.journeys[i].segments[j].legs;
                                    int Legcount = ((Newtonsoft.Json.Linq.JContainer)LegReturn).Count;
                                    List<LegReturn> legReturnsList = new List<LegReturn>();
                                    for (int n = 0; n < Legcount; n++)
                                    {
                                        LegReturn LegReturnobj = new LegReturn();
                                        LegReturnobj.legKey = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legKey;

                                        DesignatorReturn ReturnlegDesignatorobj = new DesignatorReturn();
                                        ReturnlegDesignatorobj.origin = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].designator.origin;
                                        ReturnlegDesignatorobj.destination = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].designator.destination;
                                        ReturnlegDesignatorobj.departure = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].designator.departure;
                                        ReturnlegDesignatorobj.arrival = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].designator.arrival;
                                        LegReturnobj.designator = ReturnlegDesignatorobj;

                                        LegInfoReturn legInfoReturn = new LegInfoReturn();
                                        legInfoReturn.arrivalTerminal = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal;
                                        legInfoReturn.arrivalTime = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.arrivalTime;
                                        legInfoReturn.departureTerminal = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal;
                                        legInfoReturn.departureTime = JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.departureTime;
                                        LegReturnobj.legInfo = legInfoReturn;
                                        legReturnsList.Add(LegReturnobj);

                                    }
                                    segmentReturnobj.unitdesignator = returnSeats.unitDesignator;
                                    segmentReturnobj.SSRCode = returnSeats.SSRCode;
                                    segmentReturnobj.legs = legReturnsList;
                                    segmentReturnsList.Add(segmentReturnobj);

                                }
                                journeysReturnObj.segments = segmentReturnsList;
                                journeysreturnList.Add(journeysReturnObj);
                            }

                            var Returnpassanger = JsonObjPNRBooking.data.passengers;
                            int Returnpassengercount = ((Newtonsoft.Json.Linq.JContainer)Returnpassanger).Count;
                            List<ReturnPassengers> ReturnpassengersList = new List<ReturnPassengers>();
                            foreach (var items in JsonObjPNRBooking.data.passengers)
                            {
                                ReturnPassengers returnPassengersobj = new ReturnPassengers();
                                returnPassengersobj.passengerKey = items.Value.passengerKey;
                                returnPassengersobj.passengerTypeCode = items.Value.passengerTypeCode;
                                returnPassengersobj.name = new Name();
                                returnPassengersobj.name.first = items.Value.name.first;
                                returnPassengersobj.name.last = items.Value.name.last;
                                for (int i = 0; i < PassengerDataDetailsList.Count; i++)
                                {
                                    if (returnPassengersobj.passengerTypeCode == PassengerDataDetailsList[i].passengertypecode && returnPassengersobj.name.first.ToLower() == PassengerDataDetailsList[i].first.ToLower() && returnPassengersobj.name.last.ToLower() == PassengerDataDetailsList[i].last.ToLower())
                                    {
                                        returnPassengersobj.MobNumber = PassengerDataDetailsList[i].mobile;
                                        returnPassengersobj.passengerKey = PassengerDataDetailsList[i].passengerkey;

                                        break;
                                    }

                                }
                                ReturnpassengersList.Add(returnPassengersobj);

                                //julian date
                                int year = 2024;
                                int month = 07;
                                int day = 02;

                                // Calculate the number of days from January 1st to the given date
                                DateTime currentDate = new DateTime(year, month, day);
                                DateTime startOfYear = new DateTime(year, 1, 1);
                                int julianDate = (currentDate - startOfYear).Days + 1;

                                if (items.Value.infant != null)
                                {
                                    returnPassengersobj = new ReturnPassengers();
                                    returnPassengersobj.name = new Name();
                                    returnPassengersobj.passengerTypeCode = "INFT";
                                    returnPassengersobj.name.first = items.Value.infant.name.first;
                                    returnPassengersobj.name.last = items.Value.infant.name.last;
                                    for (int i = 0; i < PassengerDataDetailsList.Count; i++)
                                    {
                                        if (returnPassengersobj.passengerTypeCode == PassengerDataDetailsList[i].passengertypecode && returnPassengersobj.name.first.ToLower() == PassengerDataDetailsList[i].first.ToLower() && returnPassengersobj.name.last.ToLower() == PassengerDataDetailsList[i].last.ToLower())
                                        {
                                            returnPassengersobj.passengerKey = PassengerDataDetailsList[i].passengerkey;
                                            break;
                                        }

                                    }
                                    ReturnpassengersList.Add(returnPassengersobj);

                                }

                            }

                            returnTicketBooking.breakdown = breakdown;
                            returnTicketBooking.journeys = journeysreturnList;
                            returnTicketBooking.passengers = ReturnpassengersList;
                            returnTicketBooking.passengerscount = Returnpassengercount;
                            returnTicketBooking.PhoneNumbers = phoneNumberList;
                            returnTicketBooking.totalAmount = totalAmount;
                            returnTicketBooking.taxMinusMeal = taxMinusMeal;
                            returnTicketBooking.taxMinusBaggage = taxMinusBaggage;
                            returnTicketBooking.totalMealTax = totalMealTax;
                            returnTicketBooking.totalAmountBaggage = totalAmountBaggage;
                            returnTicketBooking.TotalAmountMeal = TotalAmountMeal;
                            returnTicketBooking.TotaAmountBaggage = TotaAmountBaggage;
                            returnTicketBooking.htname = htname;
                            returnTicketBooking.htnameempty = htnameempty;
                            returnTicketBooking.htpax = htpax;
                            returnTicketBooking.Seatdata = htseatdata;
                            returnTicketBooking.Mealdata = htmealdata;
                            returnTicketBooking.Bagdata = htBagdata;
                            returnTicketBooking.bookingdate = returnTicketBooking.info.bookedDate;
                            #endregion
                            _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);

                            airLineFlightTicketBooking.BookingID = JsonObjPNRBooking.data.bookingKey;
                            #region DB Save
                            tb_Booking = new tb_Booking();
                            tb_Booking.AirLineID = 2;
                            string productcode = JsonObjPNRBooking.data.journeys[0].segments[0].fares[0].productClass;
                            var fareName = FareList.GetAllfare().Where(x => ((string)productcode).Equals(x.ProductCode)).FirstOrDefault();
                            tb_Booking.BookingType = "Corporate-" + JsonObjPNRBooking.data.journeys[0].segments[0].fares[0].productClass + " (" + fareName.Faredesc + ")";
                            LegalEntity legal = new LegalEntity();
                            legal = _mongoDBHelper.GetlegalEntityByGUID(Guid).Result;
                            if (legal != null)
                            {
                                tb_Booking.CompanyName = legal.BillingEntityFullName;
                            }
                            else
                            {
                                tb_Booking.CompanyName = "";
                            }
                            if (k1 == 0)
                            {
                                tb_Booking.TripType = "RoundTrip";
                            }
                            else
                            {
                                tb_Booking.TripType = "ReturnTrip";
                            }
                            tb_Booking.BookingRelationId = Guid;
                            tb_Booking.BookingID = JsonObjPNRBooking.data.bookingKey;
                            tb_Booking.RecordLocator = JsonObjPNRBooking.data.recordLocator;
                            tb_Booking.CurrencyCode = JsonObjPNRBooking.data.currencyCode;
                            tb_Booking.Origin = JsonObjPNRBooking.data.journeys[0].designator.origin;
                            tb_Booking.Destination = JsonObjPNRBooking.data.journeys[0].designator.destination;
                            tb_Booking.BookedDate = JsonObjPNRBooking.data.info.bookedDate;
                            tb_Booking.TotalAmount = JsonObjPNRBooking.data.breakdown.totalAmount;
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total != null)
                                {
                                    tb_Booking.SpecialServicesTotal = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total;
                                }
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes != null)
                                {
                                    tb_Booking.SpecialServicesTotal_Tax = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes;
                                }
                            }
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total > 0 || JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total != null)
                                {
                                    tb_Booking.SeatTotalAmount = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total;
                                    tb_Booking.SeatTotalAmount_Tax = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.taxes;
                                    if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments != null)
                                        tb_Booking.SeatAdjustment = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments;

                                }
                            }
                            tb_Booking.ExpirationDate = JsonObjPNRBooking?.data?.info?.expirationDate != null ? Convert.ToDateTime(JsonObjPNRBooking.data.info.expirationDate) : DateTime.Now;
                            tb_Booking.ArrivalDate = JsonObjPNRBooking.data.journeys[0].designator.arrival;
                            tb_Booking.DepartureDate = JsonObjPNRBooking.data.journeys[0].designator.departure;
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                tb_Booking.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate);
                            if (HttpContext.User.Identity.IsAuthenticated)
                            {
                                var identity = (ClaimsIdentity)User.Identity;
                                IEnumerable<Claim> claims = identity.Claims;
                                var userEmail = claims.Where(c => c.Type == ClaimTypes.Email).ToList()[0].Value;
                                tb_Booking.Createdby = userEmail;// "Online";
                            }
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                tb_Booking.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate);
                            tb_Booking.ModifyBy = JsonObjPNRBooking.data.info.modifiedAgentId;//"Online";
                            tb_Booking.BookingDoc = _responcePNRBooking;
                            tb_Booking.BookingStatus = "2";// JsonObjPNRBooking.data.info.status;// "0";
                            tb_Booking.PaidStatus = Convert.ToInt32(JsonObjPNRBooking.data.info.paidStatus);// "0";

                            // It  will maintained by manually as Airline Code and description 6E-Indigo
                            tb_Airlines = new tb_Airlines();
                            tb_Airlines.AirlineID = 2;
                            tb_Airlines.AirlneName = JsonObjPNRBooking.data.info.owningCarrierCode;// "Boing";
                            tb_Airlines.AirlineDescription = "AkashaAir";
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                tb_Airlines.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                            tb_Airlines.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                tb_Airlines.Modifieddate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate);// DateTime.Now;
                            tb_Airlines.Modifyby = JsonObjPNRBooking.data.info.modifiedAgentId; //"Online";
                            tb_Airlines.Status = JsonObjPNRBooking.data.info.status; //"0";

                            //It  will maintained by manually from Getseatmap Api
                            tb_AirCraft = new tb_AirCraft();
                            tb_AirCraft.Id = 1;
                            tb_AirCraft.AirlineID = 2;
                            tb_AirCraft.AirCraftName = "";// "Airbus"; to do
                            tb_AirCraft.AirCraftDescription = " ";// " City Squares Worldwide"; to do
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                tb_AirCraft.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                tb_AirCraft.Modifieddate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate);// DateTime.Now;
                            tb_AirCraft.Createdby = JsonObjPNRBooking.data.info.createdAgentId;// "Online";
                            tb_AirCraft.Modifyby = JsonObjPNRBooking.data.info.modifiedAgentId;// "Online";
                            tb_AirCraft.Status = JsonObjPNRBooking.data.info.status; //"0";

                            contactDetail = new ContactDetail();
                            contactDetail.BookingID = JsonObjPNRBooking.data.bookingKey;
                            contactDetail.FirstName = JsonObjPNRBooking.data.contacts.P.name.first;
                            contactDetail.LastName = JsonObjPNRBooking.data.contacts.P.name.last;
                            contactDetail.EmailID = JsonObjPNRBooking.data.contacts.P.emailAddress;
                            //contactDetail.MobileNumber = Convert.ToInt32(Regex.Replace(JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString(), @"^\+91", "")); // todo
                            if (JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-').Length > 1)
                            {
                                contactDetail.MobileNumber = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[1];
                                contactDetail.CountryCode = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[0];
                            }
                            else
                            {
                                contactDetail.MobileNumber = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString();
                                contactDetail.CountryCode = "+91";
                            }
                            //contactDetail.MobileNumber = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[1];
                            //contactDetail.CountryCode = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[0];
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                contactDetail.CreateDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                            contactDetail.CreateBy = JsonObjPNRBooking.data.info.createdAgentId; //"Admin";
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                contactDetail.ModifyDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                            contactDetail.ModifyBy = JsonObjPNRBooking.data.info.modifiedAgentId; //"Admin";
                            contactDetail.Status = JsonObjPNRBooking.data.info.status;// 0;
                            gSTDetails = new GSTDetails();
                            if (JsonObjPNRBooking.data.contacts.G != null)
                            {
                                gSTDetails.bookingReferenceNumber = JsonObjPNRBooking.data.bookingKey;
                                gSTDetails.GSTEmail = JsonObjPNRBooking.data.contacts.G.emailAddress;
                                gSTDetails.GSTNumber = JsonObjPNRBooking.data.contacts.G.customerNumber;
                                gSTDetails.GSTName = JsonObjPNRBooking.data.contacts.G.companyName;
                                gSTDetails.airLinePNR = JsonObjPNRBooking.data.recordLocator;
                                gSTDetails.status = JsonObjPNRBooking.data.info.status; //0;
                            }

                            tb_PassengerTotalobj = new tb_PassengerTotal();
                            bookingKey = JsonObjPNRBooking.data.bookingKey;
                            tb_PassengerTotalobj.BookingID = JsonObjPNRBooking.data.bookingKey;
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices != null)
                            {
                                tb_PassengerTotalobj.SpecialServicesAmount = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total;
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes != null)
                                {
                                    tb_PassengerTotalobj.SpecialServicesAmount_Tax = JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes;
                                }
                            }
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats != null)
                            {
                                if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total > 0 || JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total != null)
                                {
                                    tb_PassengerTotalobj.TotalSeatAmount = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total;
                                    tb_PassengerTotalobj.TotalSeatAmount_Tax = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.taxes;
                                    if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments != null)
                                        tb_PassengerTotalobj.SeatAdjustment = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments;

                                }
                            }
                            double inftAmt = 0;
                            double infttax = 0;
                            if (JsonObjPNRBooking.data.breakdown.passengerTotals.infant != null)
                            {
                                inftAmt = JsonObjPNRBooking.data.breakdown.passengerTotals.infant.total;
                                infttax = JsonObjPNRBooking.data.breakdown.passengerTotals.infant.taxes;

                            }
                            tb_PassengerTotalobj.TotalBookingAmount = JsonObjPNRBooking.data.breakdown.journeyTotals.totalAmount + inftAmt;
                            tb_PassengerTotalobj.totalBookingAmount_Tax = JsonObjPNRBooking.data.breakdown.journeyTotals.totalTax + infttax;
                            tb_PassengerTotalobj.Modifyby = JsonObjPNRBooking.data.info.createdDate;// "Online";
                            tb_PassengerTotalobj.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                            tb_PassengerTotalobj.Status = JsonObjPNRBooking.data.info.status; //"0";
                            if (JsonObjPNRBooking.data.info.createdDate != null)
                                tb_PassengerTotalobj.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate);// DateTime.Now;
                            if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                tb_PassengerTotalobj.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                            var passangerCount = JsonObjPNRBooking.data.passengers;
                            int PassengerDataCount = ((Newtonsoft.Json.Linq.JContainer)passangerCount).Count;

                            int Adult = 0;
                            int child = 0;
                            int Infant = 0;
                            for (int i = 0; i < PassengerDataDetailsList.Count; i++)
                            {
                                if (PassengerDataDetailsList[i].passengertypecode == "ADT")
                                {
                                    Adult++;
                                }
                                else if (PassengerDataDetailsList[i].passengertypecode == "CHD" || PassengerDataDetailsList[i].passengertypecode == "CNN")
                                {
                                    child++;
                                }
                                else if (PassengerDataDetailsList[i].passengertypecode == "INFT" || PassengerDataDetailsList[i].passengertypecode == "INF")
                                {
                                    Infant++;
                                }

                            }
                            tb_PassengerTotalobj.AdultCount = Adult;
                            tb_PassengerTotalobj.ChildCount = child;
                            tb_PassengerTotalobj.InfantCount = Infant;
                            tb_PassengerTotalobj.TotalPax = Adult + child + Infant;

                            Hashtable htPaxAmount = new Hashtable();
                            int JourneysCount = JsonObjPNRBooking.data.journeys.Count;
                            int SegmentCount = JsonObjPNRBooking.data.journeys[0].segments.Count;
                            for (int i = 0; i < JourneysCount; i++)
                            {
                                for (int ia = 0; ia < SegmentCount; ia++)
                                {
                                    for (int k = 0; k < JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares.Count; k++)
                                    {
                                        double Amt = 0.0;
                                        double tax = 0.0;
                                        for (int ka = 0; ka < JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges.Count; ka++)
                                        {
                                            if (JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges[ka].type.ToString() == "0")
                                            {
                                                Amt = Convert.ToDouble(JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges[ka].amount);
                                            }
                                            else
                                            {
                                                tax += Convert.ToDouble(JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges[ka].amount);
                                            }
                                        }
                                        htPaxAmount.Add(JsonObjPNRBooking.data.journeys[0].segments[ia].designator.origin.ToString() + "_" + JsonObjPNRBooking.data.journeys[0].segments[ia].designator.destination.ToString() + "_" + JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].passengerType.ToString(), Amt + "/" + tax);
                                    }
                                }
                            }



                            tb_PassengerDetailsList = new List<tb_PassengerDetails>();
                            SegmentCount = JsonObjPNRBooking.data.journeys[0].segments.Count;
                            for (int isegment = 0; isegment < SegmentCount; isegment++)
                            {
                                foreach (var items in JsonObjPNRBooking.data.passengers)
                                {
                                    tb_PassengerDetails tb_Passengerobj = new tb_PassengerDetails();
                                    tb_Passengerobj.BookingID = bookingKey;
                                    tb_Passengerobj.SegmentsKey = JsonObjPNRBooking.data.journeys[0].segments[isegment].segmentKey;
                                    tb_Passengerobj.PassengerKey = items.Value.passengerKey;
                                    tb_Passengerobj.TypeCode = items.Value.passengerTypeCode;
                                    tb_Passengerobj.FirstName = items.Value.name.first;
                                    tb_Passengerobj.Title = items.Value.name.title;
                                    tb_Passengerobj.LastName = items.Value.name.last;

                                    tb_Passengerobj.contact_Emailid = PassengerDataDetailsList.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).Email;
                                    tb_Passengerobj.contact_Mobileno = PassengerDataDetailsList.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).mobile;
                                    tb_Passengerobj.FastForwardService = 'N';
                                    //tb_Passengerobj.FrequentFlyerNumber = PassengerDataDetailsList.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).FrequentFlyer;
                                    tb_Passengerobj.FrequentFlyerNumber = "";
                                    if (tb_Passengerobj.Title == "MR" || tb_Passengerobj.Title == "Master" || tb_Passengerobj.Title == "MSTR")
                                        tb_Passengerobj.Gender = "Male";
                                    else if (tb_Passengerobj.Title == "MS" || tb_Passengerobj.Title == "MRS" || tb_Passengerobj.Title == "MISS")
                                        tb_Passengerobj.Gender = "Female";
                                    int JourneysReturnCount1 = JsonObjPNRBooking.data.journeys.Count;
                                    if (JsonObjPNRBooking.data.journeys[0].segments[isegment].passengerSegment[tb_Passengerobj.PassengerKey].seats != null && JsonObjPNRBooking.data.journeys[0].segments[isegment].passengerSegment[tb_Passengerobj.PassengerKey].seats.Count > 0)
                                    {
                                        var flightseatnumber1 = JsonObjPNRBooking.data.journeys[0].segments[isegment].passengerSegment[tb_Passengerobj.PassengerKey].seats[0].unitDesignator;
                                        tb_Passengerobj.Seatnumber = flightseatnumber1;
                                    }
                                    string key = JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.origin.ToString() + "_" +
                     JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.destination.ToString() + "_" +
                     tb_Passengerobj.TypeCode.ToString();

                                    if (htPaxAmount.ContainsKey(key))
                                    {
                                        string[] parts = htPaxAmount[key].ToString().Split('/');

                                        tb_Passengerobj.TotalAmount = Convert.ToDecimal(parts[0]);
                                        tb_Passengerobj.TotalAmount_tax = Convert.ToDecimal(parts[1]);
                                    }
                                    else
                                    {
                                        tb_Passengerobj.TotalAmount = 0.0M;
                                        tb_Passengerobj.TotalAmount_tax = 0.0M;
                                    }




                                    if (JsonObjPNRBooking.data.info.createdDate != null)
                                        tb_Passengerobj.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                                    tb_Passengerobj.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                                    if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                        tb_Passengerobj.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                                    tb_Passengerobj.ModifyBy = JsonObjPNRBooking.data.info.modifiedAgentId; //"Online";
                                    tb_Passengerobj.Status = JsonObjPNRBooking.data.info.status; //"0";
                                    if (items.Value.infant != null & isegment == 0)
                                    {
                                        tb_Passengerobj.Inf_TypeCode = "INFT";
                                        tb_Passengerobj.Inf_Firstname = items.Value.infant.name.first;
                                        tb_Passengerobj.Inf_Lastname = items.Value.infant.name.last;
                                        tb_Passengerobj.Inf_Dob = items.Value.infant.dateOfBirth;
                                        if (items.Value.infant.gender == "1")
                                        {
                                            tb_Passengerobj.Inf_Gender = "Master";
                                        }
                                        if (isegment == 0)
                                        {
                                            for (int i = 0; i < items.Value.infant.fees[0].serviceCharges.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    tb_Passengerobj.InftAmount = items.Value.infant.fees[0].serviceCharges[0].amount;
                                                }
                                                else
                                                {
                                                    tb_Passengerobj.InftAmount_Tax += Convert.ToDouble(items.Value.infant.fees[0].serviceCharges[i].amount);
                                                }

                                            }
                                            tb_Passengerobj.InftAmount = Convert.ToDouble(items.Value.infant.fees[0].serviceCharges[0].amount) - tb_Passengerobj.InftAmount_Tax;

                                        }
                                        else
                                        {
                                            tb_Passengerobj.InftAmount = 0.0;// to do
                                            tb_Passengerobj.InftAmount_Tax = 0.0;// to do
                                        }

                                        for (int i = 0; i < PassengerDataDetailsList.Count; i++)
                                        {
                                            if (tb_Passengerobj.Inf_TypeCode == PassengerDataDetailsList[i].passengertypecode && tb_Passengerobj.Inf_Firstname.ToLower() == PassengerDataDetailsList[i].first.ToLower() + " " + PassengerDataDetailsList[i].last.ToLower())
                                            {
                                                tb_Passengerobj.PassengerKey = PassengerDataDetailsList[i].passengerkey;
                                                break;
                                            }
                                        }
                                    }
                                    string oridest = JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.origin + JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.destination;

                                    // Handle carrybages and fees
                                    List<FeeDetails> feeDetails = new List<FeeDetails>();
                                    double TotalAmount_Seat = 0;
                                    decimal TotalAmount_Seat_tax = 0;
                                    decimal TotalAmount_Seat_discount = 0;
                                    double TotalAmount_Meals = 0;
                                    decimal TotalAmount_Meals_tax = 0;
                                    decimal TotalAmount_Meals_discount = 0;
                                    double TotalAmount_Baggage = 0;
                                    decimal TotalAmount_Baggage_tax = 0;
                                    decimal TotalAmount_Baggage_discount = 0;
                                    string carryBagesConcatenation = "";
                                    string MealConcatenation = "";
                                    int feesCount = items.Value.fees.Count;
                                    foreach (var fee in items.Value.fees)
                                    {
                                        string ssrCode = fee.ssrCode?.ToString();
                                        if (ssrCode != null)
                                        {
                                            if (ssrCode.StartsWith("X"))
                                            {
                                                if (fee.flightReference.ToString().Contains(oridest) == true)
                                                {
                                                    //TicketCarryBag[tb_Passengerobj.PassengerKey.ToString()] = fee.ssrCode;
                                                    var BaggageName = MealImageList.GetAllmeal()
                                                                    .Where(x => ((string)fee.ssrCode).Contains(x.MealCode))
                                                                    .Select(x => x.MealImage)
                                                                    .FirstOrDefault();
                                                    carryBagesConcatenation += fee.ssrCode + "-" + BaggageName + ",";
                                                }
                                            }
                                            else if (ssrCode.StartsWith("P") || ssrCode.StartsWith("C"))
                                            {
                                                if (fee.flightReference.ToString().Contains(oridest) == true)
                                                {
                                                    //TicketMeal[tb_Passengerobj.PassengerKey.ToString()] = fee.ssrCode;
                                                    var MealName = MealImageList.GetAllmeal()
                                                                    .Where(x => ((string)fee.ssrCode).Contains(x.MealCode))
                                                                    .Select(x => x.MealImage)
                                                                    .FirstOrDefault();
                                                    MealConcatenation += fee.ssrCode + "-" + MealName + ",";
                                                }
                                            }
                                        }
                                        Hashtable TicketMealTax = new Hashtable();
                                        Hashtable TicketMealAmountTax = new Hashtable();
                                        Hashtable TicketCarryBagAMountTax = new Hashtable();

                                        // Iterate through service charges
                                        int ServiceCount = fee.serviceCharges.Count;
                                        if (fee.code.ToString().StartsWith("SFE"))
                                        {
                                            foreach (var serviceCharge in fee.serviceCharges)
                                            {
                                                string serviceChargeCode = serviceCharge.code?.ToString();
                                                double amount = (serviceCharge.amount != null) ? Convert.ToDouble(serviceCharge.amount) : 0;
                                                if (serviceChargeCode != null)
                                                {
                                                    if (fee.flightReference.ToString().Contains(oridest) == true)
                                                    {
                                                        if (serviceChargeCode.StartsWith("SFE") && serviceCharge.type == "6")
                                                        {
                                                            TotalAmount_Seat = amount;
                                                            //TicketSeat[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Seat;
                                                        }
                                                        else if (serviceCharge.type == "3" || serviceCharge.type == "5")
                                                        {
                                                            TotalAmount_Seat_tax += Convert.ToDecimal(amount);
                                                        }
                                                        else if (serviceCharge.type == "1")
                                                        {
                                                            TotalAmount_Seat_discount += Convert.ToDecimal(amount);
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                        else if (fee.code.ToString().StartsWith("P") || fee.code.ToString().StartsWith("C"))
                                        {
                                            foreach (var serviceCharge in fee.serviceCharges)
                                            {
                                                string serviceChargeCode = serviceCharge.code?.ToString();
                                                double amount = (serviceCharge.amount != null) ? Convert.ToDouble(serviceCharge.amount) : 0;
                                                if (serviceChargeCode != null)
                                                {
                                                    if (fee.flightReference.ToString().Contains(oridest) == true)
                                                    {

                                                        if ((serviceChargeCode.StartsWith("P") || serviceChargeCode.StartsWith("C")) && serviceCharge.type == "6")
                                                        {
                                                            TotalAmount_Meals = amount;
                                                            //TicketMealAmount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Meals;
                                                        }
                                                        else if (serviceCharge.type == "3")
                                                        {
                                                            TotalAmount_Meals_tax += Convert.ToDecimal(amount);
                                                        }
                                                        else if (serviceCharge.type == "1")
                                                        {
                                                            TotalAmount_Meals_discount += Convert.ToDecimal(amount);
                                                        }
                                                    }

                                                }

                                            }
                                        }
                                        else if (fee.code.ToString().StartsWith("X"))
                                        {
                                            foreach (var serviceCharge in fee.serviceCharges)
                                            {
                                                string serviceChargeCode = serviceCharge.code?.ToString();
                                                double amount = (serviceCharge.amount != null) ? Convert.ToDouble(serviceCharge.amount) : 0;
                                                if (serviceChargeCode != null && isegment == 0)
                                                {
                                                    if (serviceChargeCode.StartsWith("X") && serviceCharge.type == "6")
                                                    {
                                                        TotalAmount_Baggage = amount;
                                                        //TicketCarryBagAMount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Baggage;
                                                    }
                                                    else if (serviceCharge.type == "3" || serviceCharge.type == "5")
                                                    {
                                                        TotalAmount_Baggage_tax += Convert.ToDecimal(amount);
                                                    }
                                                    else if (serviceCharge.type == "1")
                                                    {
                                                        TotalAmount_Baggage_discount += Convert.ToDecimal(amount);
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    tb_Passengerobj.TotalAmount_Seat = TotalAmount_Seat;
                                    tb_Passengerobj.TotalAmount_Seat_tax = TotalAmount_Seat_tax;
                                    tb_Passengerobj.TotalAmount_Seat_tax_discount = TotalAmount_Seat_discount;
                                    tb_Passengerobj.TotalAmount_Meals = TotalAmount_Meals;
                                    tb_Passengerobj.TotalAmount_Meals_tax = Convert.ToDouble(TotalAmount_Meals_tax);
                                    tb_Passengerobj.TotalAmount_Meals_discount = Convert.ToDouble(TotalAmount_Meals_discount);
                                    tb_Passengerobj.BaggageTotalAmount = TotalAmount_Baggage;
                                    tb_Passengerobj.BaggageTotalAmountTax = TotalAmount_Baggage_tax;
                                    tb_Passengerobj.BaggageTotalAmountTax_discount = TotalAmount_Baggage_discount;
                                    tb_Passengerobj.Carrybages = carryBagesConcatenation.TrimEnd(',');
                                    if (string.IsNullOrEmpty(MealConcatenation.TrimEnd(',')) && htmealdata.Count > 0)
                                    {
                                        string data2 = htmealdata[tb_Passengerobj.PassengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.origin + "_" + JsonObjPNRBooking.data.journeys[0].segments[isegment].designator.destination].ToString();
                                        var MealName = MealImageList.GetAllmeal()
                                                        .Where(x => ((string)data2).Contains(x.MealCode))
                                                        .Select(x => x.MealImage)
                                                        .FirstOrDefault();
                                        MealConcatenation += data2 + "-" + MealName + ",";
                                        tb_Passengerobj.MealsCode = MealConcatenation.TrimEnd(',');
                                    }
                                    else
                                    {
                                        tb_Passengerobj.MealsCode = MealConcatenation.TrimEnd(',');
                                    }
                                    tb_PassengerDetailsList.Add(tb_Passengerobj);


                                }
                            }
                            JourneysCount = JsonObjPNRBooking.data.journeys.Count;
                            tb_JourneysList = new List<tb_journeys>();
                            segmentReturnsListt = new List<tb_Segments>();
                            Hashtable seatNumber = new Hashtable();
                            for (int i = 0; i < JourneysCount; i++)
                            {
                                tb_journeys tb_JourneysObj = new tb_journeys();
                                tb_JourneysObj.BookingID = JsonObjPNRBooking.data.bookingKey;
                                tb_JourneysObj.JourneyKey = JsonObjPNRBooking.data.journeys[i].journeyKey;
                                tb_JourneysObj.Stops = JsonObjPNRBooking.data.journeys[i].stops;
                                tb_JourneysObj.JourneyKeyCount = i;
                                tb_JourneysObj.FlightType = JsonObjPNRBooking.data.journeys[i].flightType;
                                tb_JourneysObj.Origin = JsonObjPNRBooking.data.journeys[i].designator.origin;
                                tb_JourneysObj.Destination = JsonObjPNRBooking.data.journeys[i].designator.destination;
                                tb_JourneysObj.DepartureDate = JsonObjPNRBooking.data.journeys[i].designator.departure;
                                tb_JourneysObj.ArrivalDate = JsonObjPNRBooking.data.journeys[i].designator.arrival;
                                if (JsonObjPNRBooking.data.info.createdDate != null)
                                    tb_JourneysObj.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                                tb_JourneysObj.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                                if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                    tb_JourneysObj.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                                tb_JourneysObj.Modifyby = JsonObjPNRBooking.data.info.modifiedAgentId; //"Online";
                                tb_JourneysObj.Status = JsonObjPNRBooking.data.info.status; //"0";
                                tb_JourneysList.Add(tb_JourneysObj);
                                int SegmentReturnCountt = JsonObjPNRBooking.data.journeys[0].segments.Count;
                                for (int j = 0; j < SegmentReturnCountt; j++)
                                {
                                    tb_Segments segmentReturnobj = new tb_Segments();
                                    segmentReturnobj.BookingID = JsonObjPNRBooking.data.bookingKey;
                                    segmentReturnobj.journeyKey = JsonObjPNRBooking.data.journeys[0].journeyKey;
                                    segmentReturnobj.SegmentKey = JsonObjPNRBooking.data.journeys[0].segments[j].segmentKey;
                                    segmentReturnobj.SegmentCount = j;
                                    segmentReturnobj.Origin = JsonObjPNRBooking.data.journeys[0].segments[j].designator.origin;
                                    segmentReturnobj.Destination = JsonObjPNRBooking.data.journeys[0].segments[j].designator.destination;
                                    segmentReturnobj.DepartureDate = JsonObjPNRBooking.data.journeys[0].segments[j].designator.departure;
                                    segmentReturnobj.ArrivalDate = JsonObjPNRBooking.data.journeys[0].segments[j].designator.arrival;
                                    segmentReturnobj.Identifier = JsonObjPNRBooking.data.journeys[0].segments[j].identifier.identifier;
                                    segmentReturnobj.CarrierCode = JsonObjPNRBooking.data.journeys[0].segments[j].identifier.carrierCode;
                                    segmentReturnobj.Seatnumber = ""; // to do
                                    segmentReturnobj.MealCode = ""; // to do
                                    segmentReturnobj.MealDiscription = "";// "it is a coffe"; // to fo
                                    var LegReturn = JsonObjPNRBooking.data.journeys[i].segments[j].legs;
                                    int Legcount = ((Newtonsoft.Json.Linq.JContainer)LegReturn).Count;
                                    List<LegReturn> legReturnsList = new List<LegReturn>();
                                    for (int n = 0; n < Legcount; n++)
                                    {
                                        if (JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal != null)
                                            segmentReturnobj.DepartureTerminal = Convert.ToInt32(Regex.Match(JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal.ToString(), @"\d+").Value);   // to do
                                        if (JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal != null)
                                            segmentReturnobj.ArrivalTerminal = Convert.ToInt32(Regex.Match(JsonObjPNRBooking.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal.ToString(), @"\d+").Value); ; // to do
                                    }
                                    if (JsonObjPNRBooking.data.info.createdDate != null)
                                        segmentReturnobj.CreatedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                                    if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                        segmentReturnobj.ModifiedDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                                    segmentReturnobj.Createdby = JsonObjPNRBooking.data.info.createdAgentId; //"Online";
                                    segmentReturnobj.Modifyby = JsonObjPNRBooking.data.info.modifiedAgentId; //"Online";
                                    segmentReturnobj.Status = JsonObjPNRBooking.data.info.status; //;
                                    segmentReturnsListt.Add(segmentReturnobj);

                                }
                            }

                            //tb_Trips = new Trips();
                            //tb_Trips.OutboundFlightID = JsonObjPNRBooking.data.bookingKey;
                            //tb_Trips.TripType = "OneWay";
                            //tb_Trips.TripStatus = "active";
                            //tb_Trips.BookingDate = DateTime.Now;
                            //tb_Trips.UserID = "";
                            //tb_Trips.ReturnFlightID = "";



                            //airLineFlightTicketBooking.tb_Booking = tb_Booking;
                            //airLineFlightTicketBooking.GSTDetails = gSTDetails;
                            //airLineFlightTicketBooking.tb_Segments = segmentReturnsListt;
                            //airLineFlightTicketBooking.tb_AirCraft = tb_AirCraft;
                            //airLineFlightTicketBooking.tb_journeys = tb_JourneysList;
                            //airLineFlightTicketBooking.tb_PassengerTotal = tb_PassengerTotalobj;
                            //airLineFlightTicketBooking.tb_PassengerDetails = tb_PassengerDetailsList;
                            //airLineFlightTicketBooking.ContactDetail = contactDetail;
                            //airLineFlightTicketBooking.tb_Trips = tb_Trips;
                            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            //HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.BaseURL + "api/AirLineTicketBooking/PostairlineTicketData", airLineFlightTicketBooking);
                            //if (responsePassengers.IsSuccessStatusCode)
                            //{
                            //    var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                            //}
                            #endregion
                        }
                        else
                        {
                            ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                            _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);
                        }

                    }

                    // Spice Jet
                    else if (flagSpicejet == true && data.Airline[k1].ToLower().Contains("spicejet"))
                    {
                        TotalMeal = 0;
                        TotalBag = 0;
                        TotalFastFFWD = 0;
                        Totatamountmb = 0;
                        TotalBagtax = 0;
                        #region Spicejet Commit
                        //Spicejet
                        token = string.Empty;
                        SearchLog searchLog = new SearchLog();
                        searchLog = _mongoDBHelper.GetFlightSearchLog(Guid).Result;
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "SpiceJet").Result;

                        if (k1 == 0)
                        {
                            tokenview = tokenData.Token;
                        }
                        else
                        {
                            tokenview = tokenData.RToken;
                        }
                        if (tokenview == null) { tokenview = ""; }
                        token = tokenview.Replace(@"""", string.Empty);
                        if (!string.IsNullOrEmpty(tokenview))
                        {

                            _commit objcommit = new _commit();
                            #region GetState
                            GetBookingFromStateResponse _GetBookingFromStateRS1 = null;
                            GetBookingFromStateRequest _GetBookingFromStateRQ1 = null;
                            _GetBookingFromStateRQ1 = new GetBookingFromStateRequest();
                            _GetBookingFromStateRQ1.Signature = token;
                            _GetBookingFromStateRQ1.ContractVersion = 420;


                            SpiceJetApiController objSpiceJet = new SpiceJetApiController();
                            _GetBookingFromStateRS1 = await objSpiceJet.GetBookingFromState(_GetBookingFromStateRQ1);

                            string strdata = JsonConvert.SerializeObject(_GetBookingFromStateRS1);
                            decimal Totalpayment = 0M;
                            if (_GetBookingFromStateRS1 != null)
                            {
                                Totalpayment = _GetBookingFromStateRS1.BookingData.BookingSum.TotalCost;
                            }
                            //ADD Payment
                            AddPaymentToBookingRequest _bookingpaymentRequest = new AddPaymentToBookingRequest();
                            AddPaymentToBookingResponse _BookingPaymentResponse = new AddPaymentToBookingResponse();
                            _bookingpaymentRequest.Signature = token;
                            _bookingpaymentRequest.ContractVersion = 420;
                            _bookingpaymentRequest.addPaymentToBookingReqData = new AddPaymentToBookingRequestData();
                            _bookingpaymentRequest.addPaymentToBookingReqData.MessageStateSpecified = true;
                            _bookingpaymentRequest.addPaymentToBookingReqData.MessageState = MessageState.New;
                            _bookingpaymentRequest.addPaymentToBookingReqData.WaiveFeeSpecified = true;
                            _bookingpaymentRequest.addPaymentToBookingReqData.WaiveFee = false;
                            _bookingpaymentRequest.addPaymentToBookingReqData.PaymentMethodTypeSpecified = true;
                            _bookingpaymentRequest.addPaymentToBookingReqData.PaymentMethodType = RequestPaymentMethodType.AgencyAccount;
                            _bookingpaymentRequest.addPaymentToBookingReqData.PaymentMethodCode = "AG";
                            _bookingpaymentRequest.addPaymentToBookingReqData.QuotedCurrencyCode = "INR";
                            _bookingpaymentRequest.addPaymentToBookingReqData.QuotedAmountSpecified = true;
                            _bookingpaymentRequest.addPaymentToBookingReqData.QuotedAmount = Totalpayment;
                            _bookingpaymentRequest.addPaymentToBookingReqData.InstallmentsSpecified = true;
                            _bookingpaymentRequest.addPaymentToBookingReqData.Installments = 1;
                            _bookingpaymentRequest.addPaymentToBookingReqData.ExpirationSpecified = true;
                            _bookingpaymentRequest.addPaymentToBookingReqData.Expiration = Convert.ToDateTime("0001-01-01T00:00:00");
                            _BookingPaymentResponse = await objSpiceJet.Addpayment(_bookingpaymentRequest);
                            string payment = JsonConvert.SerializeObject(_BookingPaymentResponse);
                            logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_bookingpaymentRequest) + "\n\n Response: " + JsonConvert.SerializeObject(_BookingPaymentResponse), "BookingPayment", "SpiceJetRT");


                            #endregion


                            #region Addpayment For Api payment deduction
                            //IndigoBookingManager_.AddPaymentToBookingResponse _BookingPaymentResponse = await objcommit.AddpaymenttoBook(token, Totalpayment);

                            #endregion

                            string passengernamedetails = HttpContext.Session.GetString("PassengerNameDetailsSG");
                            List<passkeytype> passeengerlist = (List<passkeytype>)JsonConvert.DeserializeObject(passengernamedetails, typeof(List<passkeytype>));
                            string contactdata = HttpContext.Session.GetString("ContactDetails");
                            UpdateContactsRequest contactList = (UpdateContactsRequest)JsonConvert.DeserializeObject(contactdata, typeof(UpdateContactsRequest));
                            using (HttpClient client1 = new HttpClient())
                            {
                                #region Commit Booking
                                BookingCommitRequest _bookingCommitRequest = new BookingCommitRequest();
                                BookingCommitResponse _BookingCommitResponse = new BookingCommitResponse();
                                _bookingCommitRequest.Signature = token;
                                _bookingCommitRequest.ContractVersion = 420;
                                _bookingCommitRequest.BookingCommitRequestData = new BookingCommitRequestData();
                                _bookingCommitRequest.BookingCommitRequestData.SourcePOS = GetPointOfSale();
                                _bookingCommitRequest.BookingCommitRequestData.CurrencyCode = "INR";
                                _bookingCommitRequest.BookingCommitRequestData.PaxCount = Convert.ToInt16(passeengerlist.Count);
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts = new BookingContact[1];
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0] = new BookingContact();
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].TypeCode = "P";
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names = new BookingName[1];
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0] = new BookingName();
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].State = MessageState.New;
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].FirstName = passeengerlist[0].first;
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].MiddleName = passeengerlist[0].middle;
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].LastName = passeengerlist[0].last;
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].Title = passeengerlist[0].title;
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].EmailAddress = contactList.updateContactsRequestData.BookingContactList[0].EmailAddress; //"vinay.ks@gmail.com"; //passeengerlist.Email;
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].HomePhone = "9457000000"; //contactList.updateContactsRequestData.BookingContactList[0].HomePhone; //"9457000000"; //passeengerlist.mobile;
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].AddressLine1 = "A";
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].AddressLine2 = "B";
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].City = "Delhi";
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].CountryCode = "IN";
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].CultureCode = "en-GB";
                                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].DistributionOption = DistributionOption.Email;

                                objSpiceJet = new SpiceJetApiController();
                                _BookingCommitResponse = await objSpiceJet.BookingCommit(_bookingCommitRequest);
                                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_bookingCommitRequest) + "\n\n Response: " + JsonConvert.SerializeObject(_BookingCommitResponse), "BookingCommit", "SpiceJetRT");

                                if (_BookingCommitResponse != null && _BookingCommitResponse.BookingUpdateResponseData.Success.RecordLocator != null)
                                {
                                    string Str3 = JsonConvert.SerializeObject(_BookingCommitResponse);

                                    GetBookingRequest getBookingRequest = new GetBookingRequest();
                                    GetBookingResponse _getBookingResponse = new GetBookingResponse();
                                    getBookingRequest.Signature = token;
                                    getBookingRequest.ContractVersion = 420;
                                    getBookingRequest.GetBookingReqData = new GetBookingRequestData();
                                    getBookingRequest.GetBookingReqData.GetBookingBy = GetBookingBy.RecordLocator;
                                    getBookingRequest.GetBookingReqData.GetByRecordLocator = new GetByRecordLocator();
                                    getBookingRequest.GetBookingReqData.GetByRecordLocator.RecordLocator = _BookingCommitResponse.BookingUpdateResponseData.Success.RecordLocator;

                                    _getBookingResponse = await objSpiceJet.GetBookingdetails(getBookingRequest);
                                    string _responceGetBooking = JsonConvert.SerializeObject(_getBookingResponse);

                                    logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_getBookingResponse) + "\n\n Response: " + JsonConvert.SerializeObject(_getBookingResponse), "GetBookingDetails", "SpiceJetRT");

                                    if (_getBookingResponse != null)
                                    {
                                        Hashtable htname = new Hashtable();
                                        Hashtable htnameempty = new Hashtable();
                                        Hashtable htpax = new Hashtable();

                                        Hashtable htseatdata = new Hashtable();
                                        Hashtable htmealdata = new Hashtable();
                                        Hashtable htbagdata = new Hashtable();

                                        int adultcount = searchLog.Adults;
                                        int childcount = searchLog.Children;
                                        int infantcount = searchLog.Infants;

                                        int TotalCount = adultcount + childcount;
                                        string _responceGetBooking1 = JsonConvert.SerializeObject(_getBookingResponse);
                                        ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                                        var totalAmount = _getBookingResponse.Booking.BookingSum.TotalCost;
                                        returnTicketBooking.bookingKey = _getBookingResponse.Booking.BookingID.ToString();
                                        ReturnPaxSeats _unitdesinator = new ReturnPaxSeats();
                                        if (_getBookingResponse.Booking.Journeys[0].Segments[0].PaxSeats.Length > 0)
                                            _unitdesinator.unitDesignatorPax = _getBookingResponse.Booking.Journeys[0].Segments[0].PaxSeats[0].UnitDesignator;
                                        //GST Number
                                        if (_getBookingResponse.Booking.BookingContacts[0].TypeCode == "G")
                                        {
                                            returnTicketBooking.customerNumber = _getBookingResponse.Booking.BookingContacts[0].CustomerNumber;
                                            returnTicketBooking.companyName = _getBookingResponse.Booking.BookingContacts[0].CompanyName;
                                            returnTicketBooking.emailAddressgst = _getBookingResponse.Booking.BookingContacts[0].EmailAddress;
                                        }
                                        Contacts _contact = new Contacts();
                                        _contact.phoneNumbers = _getBookingResponse.Booking.BookingContacts[0].HomePhone.ToString();
                                        if (_unitdesinator.unitDesignatorPax != null)
                                            _contact.ReturnPaxSeats = _unitdesinator.unitDesignatorPax.ToString();
                                        returnTicketBooking.airLines = "SpiceJet";
                                        returnTicketBooking.recordLocator = _getBookingResponse.Booking.RecordLocator;
                                        BarcodePNR = _getBookingResponse.Booking.RecordLocator;

                                        Breakdown breakdown = new Breakdown();
                                        List<JourneyTotals> journeyBaseFareobj = new List<JourneyTotals>();
                                        JourneyTotals journeyTotalsobj = new JourneyTotals();

                                        PassengerTotals passengerTotals = new PassengerTotals();
                                        ReturnSeats returnSeats = new ReturnSeats();
                                        passengerTotals.specialServices = new SpecialServices();
                                        passengerTotals.baggage = new SpecialServices();
                                        var totalTax = "";// _getPriceItineraryRS.data.breakdown.journeys[journeyKey].totalTax;

                                        //changes for Passeneger name:
                                        foreach (var item in _getBookingResponse.Booking.Passengers)
                                        {
                                            htname.Add(item.PassengerNumber, item.Names[0].LastName + "/" + item.Names[0].FirstName);
                                        }

                                        //barcode
                                        BarcodePNR = _getBookingResponse.Booking.RecordLocator;
                                        if (BarcodePNR != null && BarcodePNR.Length < 7)
                                        {
                                            BarcodePNR = BarcodePNR.PadRight(7);
                                        }
                                        List<string> barcodeImage = new List<string>();
                                        #region Itenary segment and legs
                                        int journeyscount = _getBookingResponse.Booking.Journeys.Length;
                                        List<JourneysReturn> AAJourneyList = new List<JourneysReturn>();
                                        for (int i = 0; i < journeyscount; i++)
                                        {

                                            JourneysReturn AAJourneyobj = new JourneysReturn();
                                            AAJourneyobj.journeyKey = _getBookingResponse.Booking.Journeys[i].JourneySellKey;

                                            int segmentscount = _getBookingResponse.Booking.Journeys[i].Segments.Length;
                                            List<SegmentReturn> AASegmentlist = new List<SegmentReturn>();
                                            for (int j = 0; j < segmentscount; j++)
                                            {
                                                returnSeats.unitDesignator = string.Empty;
                                                returnSeats.SSRCode = string.Empty;
                                                DesignatorReturn AADesignatorobj = new DesignatorReturn();
                                                AADesignatorobj.origin = _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation;
                                                AADesignatorobj.destination = _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation;
                                                AADesignatorobj.departure = _getBookingResponse.Booking.Journeys[i].Segments[j].STD;
                                                AADesignatorobj.arrival = _getBookingResponse.Booking.Journeys[i].Segments[j].STA;
                                                AAJourneyobj.designator = AADesignatorobj;

                                                SegmentReturn AASegmentobj = new SegmentReturn();
                                                DesignatorReturn AASegmentDesignatorobj = new DesignatorReturn();

                                                AASegmentDesignatorobj.origin = _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation;
                                                AASegmentDesignatorobj.destination = _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation;
                                                AASegmentDesignatorobj.departure = _getBookingResponse.Booking.Journeys[i].Segments[j].STD;
                                                AASegmentDesignatorobj.arrival = _getBookingResponse.Booking.Journeys[i].Segments[j].STA;
                                                AASegmentobj.designator = AASegmentDesignatorobj;
                                                orides = AASegmentDesignatorobj.origin + AASegmentDesignatorobj.destination;
                                                int fareCount = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares.Length;
                                                List<FareReturn> AAFarelist = new List<FareReturn>();
                                                for (int k = 0; k < fareCount; k++)
                                                {
                                                    FareReturn AAFareobj = new FareReturn();
                                                    AAFareobj.fareKey = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].FareSellKey;
                                                    AAFareobj.productClass = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].ProductClass;

                                                    var passengerFares = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares;

                                                    int passengerFarescount = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares.Length;
                                                    List<PassengerFareReturn> PassengerfarelistRT = new List<PassengerFareReturn>();
                                                    double AdtAmount = 0.0;
                                                    double AdttaxAmount = 0.0;
                                                    double chdAmount = 0.0;
                                                    double chdtaxAmount = 0.0;
                                                    if (passengerFarescount > 0)
                                                    {
                                                        for (int l = 0; l < passengerFarescount; l++)
                                                        {
                                                            journeyTotalsobj = new JourneyTotals();
                                                            PassengerFareReturn AAPassengerfareobject = new PassengerFareReturn();
                                                            AAPassengerfareobject.passengerType = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].PaxType;

                                                            double percentagechd = 0.0;
                                                            var serviceCharges1 = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges;
                                                            int serviceChargescount = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges.Length;
                                                            List<ServiceChargeReturn> AAServicechargelist = new List<ServiceChargeReturn>();
                                                            for (int m = 0; m < serviceChargescount; m++)
                                                            {
                                                                ServiceChargeReturn AAServicechargeobj = new ServiceChargeReturn();
                                                                AAServicechargeobj.amount = Convert.ToInt32(_getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                string _data = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].ChargeType.ToString();
                                                                if (_data.ToLower() == "fareprice")
                                                                {
                                                                    journeyTotalsobj.totalAmount += Convert.ToInt32(_getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                }
                                                                else
                                                                {
                                                                    if (_getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].PaxType.Equals("CHD") && _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].ChargeCode.Contains("PRCT"))
                                                                        percentagechd = Convert.ToInt32(_getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                    else
                                                                        journeyTotalsobj.totalTax += Convert.ToInt32(_getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                }


                                                                AAServicechargelist.Add(AAServicechargeobj);
                                                            }

                                                            if (AAPassengerfareobject.passengerType.Equals("ADT"))
                                                            {
                                                                AdtAmount += journeyTotalsobj.totalAmount * adultcount;
                                                                AdttaxAmount += journeyTotalsobj.totalTax * adultcount;
                                                            }

                                                            if (AAPassengerfareobject.passengerType.Equals("CHD"))
                                                            {
                                                                if (percentagechd > 0)
                                                                    journeyTotalsobj.totalAmount = journeyTotalsobj.totalAmount - percentagechd;
                                                                chdAmount += journeyTotalsobj.totalAmount * childcount;
                                                                chdtaxAmount += journeyTotalsobj.totalTax * childcount;

                                                            }


                                                            AAPassengerfareobject.serviceCharges = AAServicechargelist;
                                                            PassengerfarelistRT.Add(AAPassengerfareobject);

                                                        }
                                                        journeyTotalsobj.totalAmount = AdtAmount + chdAmount;
                                                        journeyTotalsobj.totalTax = AdttaxAmount + chdtaxAmount;
                                                        journeyBaseFareobj.Add(journeyTotalsobj);
                                                        AAFareobj.passengerFares = PassengerfarelistRT;

                                                        AAFarelist.Add(AAFareobj);
                                                    }
                                                }
                                                //breakdown.journeyTotals = journeyTotalsobj;
                                                breakdown.passengerTotals = passengerTotals;
                                                AASegmentobj.fares = AAFarelist;
                                                IdentifierReturn AAIdentifierobj = new IdentifierReturn();

                                                AAIdentifierobj.identifier = _getBookingResponse.Booking.Journeys[i].Segments[j].FlightDesignator.FlightNumber;
                                                AAIdentifierobj.carrierCode = _getBookingResponse.Booking.Journeys[i].Segments[j].FlightDesignator.CarrierCode;

                                                AASegmentobj.identifier = AAIdentifierobj;
                                                //barCode
                                                //julian date
                                                Journeydatetime = DateTime.Parse(_getBookingResponse.Booking.Journeys[i].Segments[j].STD.ToString());
                                                carriercode = AAIdentifierobj.carrierCode;
                                                flightnumber = AAIdentifierobj.identifier;
                                                int year = Journeydatetime.Year;
                                                int month = Journeydatetime.Month;
                                                int day = Journeydatetime.Day;
                                                // Calculate the number of days from January 1st to the given date
                                                DateTime currentDate = new DateTime(year, month, day);
                                                DateTime startOfYear = new DateTime(year, 1, 1);
                                                int julianDate = (currentDate - startOfYear).Days + 1;
                                                if (string.IsNullOrEmpty(sequencenumber))
                                                {
                                                    sequencenumber = "00000";
                                                }
                                                else
                                                {
                                                    sequencenumber = sequencenumber.PadRight(5, '0');
                                                }
                                                var leg = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs;
                                                int legcount = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs.Length;
                                                List<LegReturn> AALeglist = new List<LegReturn>();
                                                for (int n = 0; n < legcount; n++)
                                                {
                                                    LegReturn AALeg = new LegReturn();
                                                    //AALeg.legKey = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legKey;
                                                    DesignatorReturn AAlegDesignatorobj = new DesignatorReturn();
                                                    AAlegDesignatorobj.origin = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].DepartureStation;
                                                    AAlegDesignatorobj.destination = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].ArrivalStation;
                                                    AAlegDesignatorobj.departure = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].STD;
                                                    AAlegDesignatorobj.arrival = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].STA;
                                                    AALeg.designator = AAlegDesignatorobj;

                                                    LegInfoReturn AALeginfoobj = new LegInfoReturn();
                                                    AALeginfoobj.arrivalTerminal = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].LegInfo.ArrivalTerminal;
                                                    AALeginfoobj.arrivalTime = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].LegInfo.PaxSTA;
                                                    AALeginfoobj.departureTerminal = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].LegInfo.DepartureTerminal;
                                                    AALeginfoobj.departureTime = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].LegInfo.PaxSTD;
                                                    AALeg.legInfo = AALeginfoobj;
                                                    AALeglist.Add(AALeg);

                                                }
                                                foreach (var item in _getBookingResponse.Booking.Passengers)
                                                {
                                                    if (!htnameempty.Contains(item.PassengerNumber.ToString() + "_" + htname[item.PassengerNumber] + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation))
                                                    {
                                                        if (carriercode.Length < 3)
                                                            carriercode = carriercode.PadRight(3);
                                                        if (flightnumber.Length < 5)
                                                        {
                                                            flightnumber = flightnumber.PadRight(5);
                                                        }
                                                        if (sequencenumber.Length < 5)
                                                            sequencenumber = sequencenumber.PadRight(5, '0');
                                                        seatnumber = "0000";
                                                        if (seatnumber.Length < 4)
                                                            seatnumber = seatnumber.PadLeft(4, '0');
                                                        BarcodeString = "M" + "1" + htname[item.PassengerNumber] + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                                        htnameempty.Add(item.PassengerNumber.ToString() + "_" + htname[item.PassengerNumber] + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, BarcodeString);
                                                    }
                                                }

                                                // Vinay For Seat 
                                                foreach (var item1 in _getBookingResponse.Booking.Journeys[i].Segments[j].PaxSeats)
                                                {
                                                    barcodeImage = new List<string>();
                                                    try
                                                    {
                                                        if (!htseatdata.Contains(item1.PassengerNumber.ToString() + "_" + htname[item1.PassengerNumber] + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation))
                                                        {
                                                            htseatdata.Add(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, item1.UnitDesignator);
                                                            returnSeats.unitDesignator += item1.PassengerNumber + "_" + item1.UnitDesignator + ",";
                                                        }
                                                        if (!htpax.Contains(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation))
                                                        {
                                                            if (carriercode.Length < 3)
                                                                carriercode = carriercode.PadRight(3);
                                                            if (flightnumber.Length < 5)
                                                            {
                                                                flightnumber = flightnumber.PadRight(5);
                                                            }
                                                            if (sequencenumber.Length < 5)
                                                                sequencenumber = sequencenumber.PadRight(5, '0');
                                                            seatnumber = htseatdata[item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation].ToString();
                                                            if (seatnumber.Length < 4)
                                                                seatnumber = seatnumber.PadLeft(4, '0');
                                                            BarcodeString = "M" + "1" + htname[item1.PassengerNumber] + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                                            htpax.Add(item1.PassengerNumber.ToString() + "_" + htname[item1.PassengerNumber] + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, BarcodeString);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }

                                                // Vinay SSR Meal 

                                                foreach (var item1 in _getBookingResponse.Booking.Journeys[i].Segments[j].PaxSSRs)
                                                {
                                                    try
                                                    {
                                                        if (!htmealdata.Contains(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation))
                                                        {
                                                            if (item1.SSRCode != "INFT" && !item1.SSRCode.StartsWith("E", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                htmealdata.Add(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, item1.SSRCode);
                                                            }
                                                            returnSeats.SSRCode += item1.SSRCode + ",";
                                                        }

                                                        if (!htbagdata.Contains(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation))
                                                        {
                                                            if (item1.SSRCode != "INFT" && item1.SSRCode.StartsWith("E", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                htbagdata.Add(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, item1.SSRCode);
                                                            }
                                                            returnSeats.SSRCode += item1.SSRCode + ",";
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }
                                                AASegmentobj.unitdesignator = returnSeats.unitDesignator;
                                                AASegmentobj.SSRCode = returnSeats.SSRCode;
                                                AASegmentobj.legs = AALeglist;
                                                AASegmentlist.Add(AASegmentobj);
                                                breakdown.journeyfareTotals = journeyBaseFareobj;
                                            }
                                            AAJourneyobj.segments = AASegmentlist;
                                            AAJourneyList.Add(AAJourneyobj);

                                        }
                                        #endregion
                                        // string stravailibitilityrequest = HttpContext.Session.GetString("IndigoAvailibilityRequest");
                                        string stravailibitilityrequest = objMongoHelper.UnZip(tokenData.PassRequest);
                                        GetAvailabilityRequest availibiltyRQ = JsonConvert.DeserializeObject<GetAvailabilityRequest>(stravailibitilityrequest);

                                        var passanger = _getBookingResponse.Booking.Passengers;
                                        int passengercount = availibiltyRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCount;
                                        ReturnPassengers passkeytypeobj = new ReturnPassengers();
                                        List<ReturnPassengers> passkeylist = new List<ReturnPassengers>();
                                        string flightreference = string.Empty;
                                        foreach (var item in _getBookingResponse.Booking.Passengers)
                                        {
                                            barcodeImage = new List<string>();
                                            passkeytypeobj = new ReturnPassengers();
                                            passkeytypeobj.name = new Name();
                                            foreach (var item1 in item.PassengerFees)
                                            {
                                                if (item1.FeeCode.Equals("SeatFee") || item1.FeeType.ToString().ToLower().Equals("seatfee"))
                                                {
                                                    flightreference = item1.FlightReference;
                                                    string[] parts = flightreference.Split(' ');

                                                    if (parts.Length > 3)
                                                    {
                                                        carriercode = parts[1]; // "6E" + "774"
                                                        flightnumber = parts[2];
                                                        int lastSpaceIndex = flightreference.LastIndexOf(' ');
                                                        // Extract substring after the last space
                                                        string result = flightreference.Substring(lastSpaceIndex + 1);
                                                        orides = result;
                                                    }
                                                    else
                                                    {
                                                        // Combine parts for the flight code
                                                        carriercode = parts[1].Substring(0, 2); // "6E" + "774"
                                                        flightnumber = parts[1].Substring(2);
                                                        orides = parts[2];
                                                    }
                                                    if (flightnumber.Length < 5)
                                                    {
                                                        flightnumber = flightnumber.PadRight(5);
                                                    }
                                                    if (carriercode.Length < 3)
                                                    {
                                                        carriercode = carriercode.PadRight(3);
                                                    }

                                                    //barCode
                                                    //julian date
                                                    Journeydatetime = DateTime.Parse(_getBookingResponse.Booking.Journeys[0].Segments[0].STD.ToString());
                                                    int year = Journeydatetime.Year;
                                                    int month = Journeydatetime.Month;
                                                    int day = Journeydatetime.Day;

                                                    // Calculate the number of days from January 1st to the given date
                                                    DateTime currentDate = new DateTime(year, month, day);
                                                    DateTime startOfYear = new DateTime(year, 1, 1);
                                                    int julianDate = (currentDate - startOfYear).Days + 1;
                                                    if (string.IsNullOrEmpty(sequencenumber))
                                                    {
                                                        sequencenumber = "00000";
                                                    }
                                                    else
                                                    {
                                                        sequencenumber = sequencenumber.PadRight(5, '0');
                                                    }

                                                    Hashtable seatassignhashtable = new Hashtable();
                                                    string[] entries = returnSeats.unitDesignator.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                    foreach (string entry in entries)
                                                    {
                                                        // Split each entry by underscore
                                                        string[] keyValue = entry.Split('_');
                                                        if (keyValue.Length == 2)
                                                        {
                                                            string key = keyValue[0];
                                                            string value = keyValue[1];
                                                            // Add to the hashtable
                                                            seatassignhashtable.Add(key, value);
                                                        }
                                                    }
                                                    if (htseatdata.ContainsKey(item.PassengerNumber.ToString() + "_" + orides.Substring(0, 3) + "_" + orides.Substring(3)))
                                                    {
                                                        seatnumber = htseatdata[item.PassengerNumber.ToString() + "_" + orides.Substring(0, 3) + "_" + orides.Substring(3)].ToString();
                                                        if (string.IsNullOrEmpty(seatnumber))
                                                        {
                                                            seatnumber = "0000"; // Set to "0000" if not available
                                                        }
                                                        else
                                                        {
                                                            seatnumber = seatnumber.PadRight(4, '0'); // Right-pad with zeros if less than 4 characters
                                                        }


                                                    }
                                                    foreach (var item2 in item1.ServiceCharges)
                                                    {
                                                        if (item2.ChargeCode.Equals("SeatFee") || item2.ChargeType.ToString().ToLower().Equals("servicecharge"))
                                                        {
                                                            returnSeats.total += Convert.ToInt32(item2.Amount);
                                                        }
                                                        else
                                                        {
                                                            returnSeats.taxes += Convert.ToInt32(item2.Amount);
                                                        }
                                                    }
                                                }
                                                else if (item1.FeeCode.Equals("INFT"))
                                                {
                                                    JourneyTotals InfantfareTotals = new JourneyTotals();
                                                    foreach (var item2 in item1.ServiceCharges)
                                                    {
                                                        if (item2.ChargeCode.Equals("INFT"))
                                                        {
                                                            InfantfareTotals.totalAmount = Convert.ToInt32(item2.Amount);
                                                        }
                                                        else
                                                        {
                                                            InfantfareTotals.totalTax += Convert.ToInt32(item2.Amount);
                                                        }
                                                    }
                                                    journeyBaseFareobj.Add(InfantfareTotals);
                                                    breakdown.journeyfareTotals = journeyBaseFareobj;
                                                }
                                                else
                                                {
                                                    foreach (var item2 in item1.ServiceCharges)
                                                    {
                                                        if ((!item2.ChargeCode.Equals("SeatFee") || !item2.ChargeCode.Equals("INFT")) && !item2.ChargeType.ToString().ToLower().Contains("tax") && item2.ChargeCode.StartsWith("E", StringComparison.OrdinalIgnoreCase) == false)
                                                        {
                                                            passengerTotals.specialServices.total += Convert.ToInt32(item2.Amount);
                                                            TotalMeal = passengerTotals.specialServices.total;
                                                        }
                                                        else if (item2.ChargeCode.StartsWith("E", StringComparison.OrdinalIgnoreCase) == true)
                                                        {
                                                            passengerTotals.baggage.total += Convert.ToInt32(item2.Amount);
                                                            TotalBag = passengerTotals.baggage.total;
                                                        }
                                                        else
                                                        {
                                                            passengerTotals.specialServices.taxes += Convert.ToInt32(item2.Amount);
                                                            TotalBagtax = passengerTotals.specialServices.taxes;
                                                        }
                                                        Totatamountmb = TotalMeal + TotalBag;
                                                    }
                                                }
                                            }
                                            passkeytypeobj.passengerTypeCode = item.PassengerTypeInfo.PaxType;
                                            passkeytypeobj.name.first = item.Names[0].FirstName;
                                            passkeytypeobj.name.last = item.Names[0].LastName;
                                            for (int i = 0; i < passeengerlist.Count; i++)
                                            {
                                                if (passkeytypeobj.passengerTypeCode == passeengerlist[i].passengertypecode && passkeytypeobj.name.first.ToLower().Trim() == passeengerlist[i].first.ToLower().Trim() && passkeytypeobj.name.last.ToLower().Trim() == passeengerlist[i].last.ToLower().Trim())
                                                {
                                                    passkeytypeobj.MobNumber = passeengerlist[i].mobile;
                                                    string[] splitStr = passeengerlist[i].passengercombinedkey.Split('@');
                                                    for (int ia = 0; ia < splitStr.Length; ia++)
                                                    {
                                                        if (splitStr[ia].ToLower().Trim().Contains("spicejet"))
                                                        {
                                                            string[] beforeCaret = splitStr[ia].Split('^');
                                                            passkeytypeobj.passengerKey = beforeCaret[0];
                                                            break;
                                                        }

                                                    }
                                                    //passkeytypeobj.passengerKey = passeengerlist[i].passengerkey;
                                                    //break;
                                                }

                                            }

                                            passkeylist.Add(passkeytypeobj);
                                            if (item.Infant != null)
                                            {
                                                passkeytypeobj = new ReturnPassengers();
                                                passkeytypeobj.name = new Name();
                                                passkeytypeobj.passengerTypeCode = "INFT";
                                                passkeytypeobj.name.first = item.Infant.Names[0].FirstName;
                                                passkeytypeobj.name.last = item.Infant.Names[0].LastName;
                                                for (int i = 0; i < passeengerlist.Count; i++)
                                                {
                                                    if (passkeytypeobj.passengerTypeCode == passeengerlist[i].passengertypecode && passkeytypeobj.name.first.ToLower().Trim() == passeengerlist[i].first.ToLower().Trim() && passkeytypeobj.name.last.ToLower().Trim() == passeengerlist[i].last.ToLower().Trim())
                                                    {
                                                        passkeytypeobj.MobNumber = passeengerlist[i].mobile;
                                                        passkeytypeobj.passengerKey = passeengerlist[i].passengerkey;
                                                        break;
                                                    }

                                                }
                                                passkeylist.Add(passkeytypeobj);

                                            }
                                            returnTicketBooking.passengers = passkeylist;
                                        }

                                        double BasefareAmt = 0.0;
                                        double BasefareTax = 0.0;
                                        for (int i = 0; i < breakdown.journeyfareTotals.Count; i++)
                                        {
                                            BasefareAmt += breakdown.journeyfareTotals[i].totalAmount;
                                            BasefareTax += breakdown.journeyfareTotals[i].totalTax;
                                        }
                                        breakdown.journeyTotals = new JourneyTotals();
                                        breakdown.journeyTotals.totalAmount = Convert.ToDouble(BasefareAmt);
                                        breakdown.passengerTotals.seats = new ReturnSeats();
                                        breakdown.passengerTotals.specialServices.total = passengerTotals.specialServices.total;
                                        breakdown.passengerTotals.baggage.total = passengerTotals.baggage.total;
                                        breakdown.passengerTotals.seats.total = returnSeats.total;
                                        breakdown.passengerTotals.seats.taxes = returnSeats.taxes;
                                        breakdown.journeyTotals.totalTax = Convert.ToDouble(BasefareTax);
                                        breakdown.totalAmount = breakdown.journeyTotals.totalAmount + breakdown.journeyTotals.totalTax;
                                        if (totalAmount != 0M)
                                        {
                                            breakdown.totalToCollect = Convert.ToDouble(totalAmount);
                                        }
                                        returnTicketBooking.breakdown = breakdown;
                                        returnTicketBooking.journeys = AAJourneyList;
                                        returnTicketBooking.passengerscount = passengercount;
                                        returnTicketBooking.contacts = _contact;
                                        returnTicketBooking.Seatdata = htseatdata;
                                        returnTicketBooking.Mealdata = htmealdata;
                                        returnTicketBooking.Bagdata = htbagdata;
                                        returnTicketBooking.htname = htname;
                                        returnTicketBooking.htnameempty = htnameempty;
                                        returnTicketBooking.htpax = htpax;
                                        returnTicketBooking.bookingdate = _getBookingResponse.Booking.BookingInfo.BookingDate;
                                        _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);

                                        #region DB Save

                                        airLineFlightTicketBooking.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                        tb_Booking = new tb_Booking();
                                        tb_Booking.AirLineID = 3;
                                        string productcode = _getBookingResponse.Booking.Journeys[0].Segments[0].Fares[0].ProductClass;
                                        var fareName = FareList.GetAllfare().Where(x => ((string)productcode).Equals(x.ProductCode)).FirstOrDefault();
                                        tb_Booking.BookingType = "Corporate-" + _getBookingResponse.Booking.Journeys[0].Segments[0].Fares[0].ProductClass + " (" + fareName.Faredesc + ")";
                                        LegalEntity legal = new LegalEntity();
                                        legal = _mongoDBHelper.GetlegalEntityByGUID(Guid).Result;
                                        //tb_Booking.CompanyName = legal.BillingEntityFullName;
                                        if (legal != null)
                                        {
                                            tb_Booking.CompanyName = legal.BillingEntityFullName;
                                        }
                                        else
                                            tb_Booking.CompanyName = "";
                                        tb_Booking.BookingRelationId = Guid;
                                        if (k1 == 0)
                                        {
                                            tb_Booking.TripType = "RoundTrip";
                                        }
                                        else
                                        {
                                            tb_Booking.TripType = "ReturnTrip";
                                        }
                                        tb_Booking.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                        tb_Booking.RecordLocator = _getBookingResponse.Booking.RecordLocator;
                                        tb_Booking.CurrencyCode = _getBookingResponse.Booking.CurrencyCode;
                                        tb_Booking.Origin = _getBookingResponse.Booking.Journeys[0].Segments[0].Legs[0].DepartureStation;
                                        int segmentcount = _getBookingResponse.Booking.Journeys[0].Segments.Length;
                                        tb_Booking.Destination = _getBookingResponse.Booking.Journeys[0].Segments[segmentcount - 1].Legs[0].ArrivalStation;
                                        tb_Booking.BookedDate = _getBookingResponse.Booking.BookingInfo.BookingDate;
                                        tb_Booking.TotalAmount = (double)_getBookingResponse.Booking.BookingSum.TotalCost;
                                        tb_Booking.SpecialServicesTotal = (double)Totatamountmb;
                                        tb_Booking.SpecialServicesTotal_Tax = (double)TotalBagtax;
                                        tb_Booking.SeatTotalAmount = returnSeats.total;
                                        tb_Booking.SeatTotalAmount_Tax = returnSeats.taxes;
                                        tb_Booking.SpecialServicesTotal -= tb_Booking.SpecialServicesTotal_Tax;
                                        tb_Booking.SeatTotalAmount -= tb_Booking.SeatTotalAmount_Tax;
                                        tb_Booking.ExpirationDate = _getBookingResponse.Booking.BookingInfo.ExpiredDate;
                                        //tb_Booking.ArrivalDate = _getBookingResponse.Booking.Journeys[0].Segments[segmentcount - 1].STA.ToString().Replace('T',' ');//DateTime.Now;
                                        //tb_Booking.DepartureDate = _getBookingResponse.Booking.Journeys[0].Segments[0].Legs[0].STD.ToString().Replace('T', ' ');//DateTime.Now;
                                        DateTime parsedDate = DateTime.ParseExact(_getBookingResponse.Booking.Journeys[0].Segments[segmentcount - 1].Legs[0].STA.ToString(), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        tb_Booking.ArrivalDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                        parsedDate = DateTime.ParseExact(_getBookingResponse.Booking.Journeys[0].Segments[0].Legs[0].STD.ToString(), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        tb_Booking.DepartureDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");

                                        tb_Booking.CreatedDate = _getBookingResponse.Booking.BookingInfo.CreatedDate;
                                        if (HttpContext.User.Identity.IsAuthenticated)
                                        {
                                            var identity = (ClaimsIdentity)User.Identity;
                                            IEnumerable<Claim> claims = identity.Claims;
                                            var userEmail = claims.Where(c => c.Type == ClaimTypes.Email).ToList()[0].Value;
                                            tb_Booking.Createdby = userEmail;// "Online";
                                        }
                                        tb_Booking.ModifiedDate = _getBookingResponse.Booking.BookingInfo.ModifiedDate;
                                        tb_Booking.ModifyBy = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        tb_Booking.BookingDoc = JsonConvert.SerializeObject(_getBookingResponse);
                                        tb_Booking.BookingStatus = "2";// _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                        tb_Booking.PaidStatus = Convert.ToInt32(_getBookingResponse.Booking.BookingInfo.PaidStatus);

                                        tb_Airlines = new tb_Airlines();
                                        tb_Airlines.AirlineID = 3;
                                        tb_Airlines.AirlneName = "";
                                        tb_Airlines.AirlineDescription = "Spicejet";
                                        tb_Airlines.CreatedDate = _getBookingResponse.Booking.BookingInfo.CreatedDate;
                                        tb_Airlines.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                        tb_Airlines.Modifieddate = _getBookingResponse.Booking.BookingInfo.ModifiedDate;
                                        tb_Airlines.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        tb_Airlines.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();

                                        tb_AirCraft = new tb_AirCraft();
                                        tb_AirCraft.Id = 1;
                                        tb_AirCraft.AirlineID = 3;
                                        tb_AirCraft.AirCraftName = "";
                                        tb_AirCraft.AirCraftDescription = " ";
                                        tb_AirCraft.CreatedDate = _getBookingResponse.Booking.BookingInfo.CreatedDate;
                                        tb_AirCraft.Modifieddate = _getBookingResponse.Booking.BookingInfo.ModifiedDate;
                                        tb_AirCraft.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                        tb_AirCraft.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        tb_AirCraft.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();

                                        contactDetail = new ContactDetail();
                                        contactDetail.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                        contactDetail.FirstName = _getBookingResponse.Booking.BookingContacts[0].Names[0].FirstName;
                                        contactDetail.LastName = _getBookingResponse.Booking.BookingContacts[0].Names[0].LastName;
                                        contactDetail.EmailID = _getBookingResponse.Booking.BookingContacts[0].EmailAddress;
                                        if (_getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-').Length > 1)
                                        {
                                            contactDetail.MobileNumber = _getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-')[1];
                                            contactDetail.CountryCode = _getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-')[0];
                                        }
                                        else
                                        {
                                            contactDetail.MobileNumber = _getBookingResponse.Booking.BookingContacts[0].HomePhone;
                                            contactDetail.CountryCode = "+91";
                                        }
                                        //contactDetail.MobileNumber = _getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-')[1];
                                        //contactDetail.CountryCode = _getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-')[0];
                                        contactDetail.CreateDate = _getBookingResponse.Booking.BookingInfo.CreatedDate;
                                        contactDetail.CreateBy = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                        contactDetail.ModifyDate = _getBookingResponse.Booking.BookingInfo.ModifiedDate;
                                        contactDetail.ModifyBy = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        contactDetail.Status = Convert.ToInt32(_getBookingResponse.Booking.BookingInfo.BookingStatus);

                                        gSTDetails = new GSTDetails();
                                        if (_getBookingResponse.Booking.BookingContacts[0].CustomerNumber != null)
                                        {
                                            gSTDetails.bookingReferenceNumber = _getBookingResponse.Booking.BookingID.ToString();
                                            gSTDetails.GSTEmail = _getBookingResponse.Booking.BookingContacts[0].EmailAddress;
                                            gSTDetails.GSTNumber = _getBookingResponse.Booking.BookingContacts[0].CustomerNumber;
                                            gSTDetails.GSTName = _getBookingResponse.Booking.BookingContacts[0].CompanyName;
                                            gSTDetails.airLinePNR = _getBookingResponse.Booking.RecordLocator;
                                            gSTDetails.status = Convert.ToInt32(_getBookingResponse.Booking.BookingInfo.BookingStatus);
                                        }

                                        tb_PassengerTotalobj = new tb_PassengerTotal();
                                        bookingKey = _getBookingResponse.Booking.BookingID.ToString();
                                        tb_PassengerTotalobj.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                        if (_getBookingResponse.Booking.Passengers.Length > 0 && _getBookingResponse.Booking.Passengers[0].PassengerFees.Length > 0)
                                        {
                                            tb_PassengerTotalobj.SpecialServicesAmount = (double)Totatamountmb; // FFWD + MEAL + BAGGAGE
                                            tb_PassengerTotalobj.SpecialServicesAmount_Tax = (double)TotalBagtax; // FFWD + MEAL + BAGGAGE
                                            tb_PassengerTotalobj.TotalSeatAmount = returnSeats.total;
                                            tb_PassengerTotalobj.TotalSeatAmount_Tax = returnSeats.taxes;
                                            tb_PassengerTotalobj.SpecialServicesAmount -= tb_PassengerTotalobj.SpecialServicesAmount_Tax;
                                            tb_PassengerTotalobj.TotalSeatAmount -= tb_PassengerTotalobj.TotalSeatAmount_Tax;
                                        }
                                        tb_PassengerTotalobj.TotalBookingAmount = (double)breakdown.journeyTotals.totalAmount;
                                        tb_PassengerTotalobj.totalBookingAmount_Tax = (double)breakdown.journeyTotals.totalTax;
                                        tb_PassengerTotalobj.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        tb_PassengerTotalobj.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                        tb_PassengerTotalobj.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                        tb_PassengerTotalobj.CreatedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.CreatedDate);
                                        tb_PassengerTotalobj.ModifiedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.ModifiedDate);


                                        tb_PassengerTotalobj.AdultCount = adultcount;
                                        tb_PassengerTotalobj.ChildCount = childcount;
                                        tb_PassengerTotalobj.InfantCount = infantcount;
                                        tb_PassengerTotalobj.TotalPax = adultcount + childcount + infantcount;

                                        var passengerCount = _getBookingResponse.Booking.Passengers;
                                        int PassengerDataCount = availibiltyRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCount;
                                        tb_PassengerDetailsList = new List<tb_PassengerDetails>();
                                        int SegmentCount = _getBookingResponse.Booking.Journeys[0].Segments.Length;
                                        for (int isegment = 0; isegment < SegmentCount; isegment++)
                                        {
                                            foreach (var items in _getBookingResponse.Booking.Passengers)
                                            {
                                                tb_PassengerDetails tb_Passengerobj = new tb_PassengerDetails();
                                                tb_Passengerobj.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                                tb_Passengerobj.PassengerKey = items.PassengerID.ToString();
                                                tb_Passengerobj.TypeCode = items.PassengerTypeInfo.PaxType;
                                                tb_Passengerobj.FirstName = items.Names[0].FirstName;
                                                tb_Passengerobj.Title = items.Names[0].Title;
                                                tb_Passengerobj.Dob = DateTime.Now;
                                                tb_Passengerobj.LastName = items.Names[0].LastName;
                                                tb_Passengerobj.contact_Emailid = passeengerlist.FirstOrDefault(x => x.first.ToUpper() == tb_Passengerobj.FirstName && x.last.ToUpper() == tb_Passengerobj.LastName).Email;
                                                tb_Passengerobj.contact_Mobileno = passeengerlist.FirstOrDefault(x => x.first.ToUpper() == tb_Passengerobj.FirstName && x.last.ToUpper() == tb_Passengerobj.LastName).mobile;
                                                tb_Passengerobj.FastForwardService = 'N';
                                                tb_Passengerobj.FrequentFlyerNumber = "";// passeengerlist.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).FrequentFlyer;
                                                if (tb_Passengerobj.Title == "MR" || tb_Passengerobj.Title == "Master" || tb_Passengerobj.Title == "MSTR")
                                                    tb_Passengerobj.Gender = "Male";
                                                else if (tb_Passengerobj.Title == "MS" || tb_Passengerobj.Title == "MRS" || tb_Passengerobj.Title == "MISS")
                                                    tb_Passengerobj.Gender = "Female";
                                                tb_Passengerobj.InftAmount = 0.0;// to do
                                                tb_Passengerobj.InftAmount_Tax = 0.0;// to do
                                                double AdtAmount = 0.0;
                                                double AdttaxAmount = 0.0;
                                                double AdtTAmount = 0.0;
                                                double AdtTtaxAmount = 0.0;
                                                //for (int isegment = 0; isegment < SegmentCount; isegment++)
                                                //{
                                                AdtAmount = 0.0;
                                                AdttaxAmount = 0.0;
                                                for (int i = 0; i < _getBookingResponse.Booking.Journeys[0].Segments[isegment].PaxSeats.Length; i++)
                                                {
                                                    if (items.PassengerNumber == _getBookingResponse.Booking.Journeys[0].Segments[isegment].PaxSeats[i].PassengerNumber)
                                                    {
                                                        var flightseatnumber1 = _getBookingResponse.Booking.Journeys[0].Segments[isegment].PaxSeats[i].UnitDesignator;
                                                        tb_Passengerobj.Seatnumber = flightseatnumber1 + ",";
                                                    }
                                                }
                                                double percentagechd = 0.0;
                                                tb_Passengerobj.SegmentsKey = _getBookingResponse.Booking.Journeys[0].Segments[isegment].SegmentSellKey;
                                                int fareCount = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares.Length;
                                                for (int k = 0; k < fareCount; k++)
                                                {
                                                    var passengerFares = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares;
                                                    int passengerFarescount = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares.Length;
                                                    if (passengerFarescount > 0)
                                                    {
                                                        for (int l = 0; l < passengerFarescount; l++)
                                                        {
                                                            if (_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].PaxType == tb_Passengerobj.TypeCode)
                                                            {
                                                                int serviceChargescount = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges.Length;
                                                                for (int m = 0; m < serviceChargescount; m++)
                                                                {
                                                                    ServiceChargeReturn AAServicechargeobj = new ServiceChargeReturn();
                                                                    AAServicechargeobj.amount = Convert.ToInt32(_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                    string data3 = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].ChargeType.ToString();
                                                                    if (data3.ToLower() == "fareprice")
                                                                    {
                                                                        AdtTAmount += Convert.ToInt32(_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].PaxType.Equals("CHD") && _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].ChargeCode.Contains("PRCT"))
                                                                        {
                                                                            percentagechd = Convert.ToInt32(_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                        }
                                                                        else
                                                                            AdtTtaxAmount += Convert.ToInt32(_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].Amount);

                                                                    }
                                                                }

                                                                //AdtAmount += AdtTAmount * adultcount;
                                                                //AdttaxAmount += AdtTtaxAmount * adultcount;
                                                            }

                                                        }
                                                    }
                                                }
                                                //}

                                                if (percentagechd > 0)
                                                    AdtTAmount = AdtTAmount - percentagechd;
                                                tb_Passengerobj.TotalAmount = (decimal)AdtTAmount;
                                                tb_Passengerobj.TotalAmount_tax = (decimal)AdtTtaxAmount;
                                                tb_Passengerobj.CreatedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.CreatedDate);
                                                tb_Passengerobj.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                                tb_Passengerobj.ModifiedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.ModifiedDate);
                                                tb_Passengerobj.ModifyBy = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                                tb_Passengerobj.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                                if (items.Infant != null && isegment == 0)
                                                {
                                                    tb_Passengerobj.Inf_TypeCode = "INFT";
                                                    tb_Passengerobj.Inf_Firstname = items.Infant.Names[0].FirstName;
                                                    tb_Passengerobj.Inf_Lastname = items.Infant.Names[0].LastName;
                                                    tb_Passengerobj.Inf_Dob = Convert.ToDateTime(items.Infant.DOB);
                                                    if (items.Infant.Gender != null)
                                                    {
                                                        tb_Passengerobj.Inf_Gender = "Master";
                                                    }
                                                    for (int i = 0; i < passeengerlist.Count; i++)
                                                    {
                                                        if (tb_Passengerobj.Inf_TypeCode == passeengerlist[i].passengertypecode && tb_Passengerobj.Inf_Firstname.ToLower() == passeengerlist[i].first.ToLower() && tb_Passengerobj.Inf_Lastname.ToLower() == passeengerlist[i].last.ToLower())
                                                        {
                                                            //tb_Passengerobj.PassengerKey = passeengerlist[i].passengerkey;
                                                            break;
                                                        }
                                                    }
                                                }
                                                string oridest = _getBookingResponse.Booking.Journeys[0].Segments[isegment].DepartureStation + _getBookingResponse.Booking.Journeys[0].Segments[isegment].ArrivalStation;

                                                // Handle carrybages and fees
                                                List<FeeDetails> feeDetails = new List<FeeDetails>();
                                                double TotalAmount_Seat = 0;
                                                decimal TotalAmount_Seat_tax = 0;
                                                decimal TotalAmount_Seat_discount = 0;
                                                double TotalAmount_Meals = 0;
                                                decimal TotalAmount_Meals_tax = 0;
                                                decimal TotalAmount_Meals_discount = 0;
                                                double TotalAmount_Baggage = 0;
                                                decimal TotalAmount_Baggage_tax = 0;
                                                decimal TotalAmount_Baggage_discount = 0;
                                                string carryBagesConcatenation = "";
                                                string MealConcatenation = "";
                                                int feesCount = items.PassengerFees.Length;
                                                foreach (var fee in items.PassengerFees)
                                                {
                                                    string ssrCode = fee.SSRCode?.ToString();
                                                    if (ssrCode != null)
                                                    {
                                                        if (ssrCode.StartsWith("E", StringComparison.OrdinalIgnoreCase) == true)
                                                        {
                                                            if (fee.FlightReference.ToString().Contains(oridest) == true)
                                                            {
                                                                var BaggageName = MealImageList.GetAllmeal()
                                                                                .Where(x => ((string)fee.SSRCode).Contains(x.MealCode))
                                                                                .Select(x => x.MealImage)
                                                                                .FirstOrDefault();
                                                                carryBagesConcatenation += fee.SSRCode + "-" + BaggageName + ",";
                                                            }
                                                        }
                                                        else if (!ssrCode.Equals("SFBO") && !ssrCode.Equals("INFT") && ssrCode.StartsWith("E", StringComparison.OrdinalIgnoreCase) == false)
                                                        {       //|| (fee.ServiceCharges.Length > 1 && fee.ServiceCharges[1].ChargeDetail.Trim().ToUpper().ToString().Replace("-", "").Contains(oridest.Trim().ToUpper()))
                                                            if (fee.FlightReference.Trim().ToUpper().ToString().Contains(oridest.Trim().ToUpper()))
                                                            {
                                                                Hashtable htssr = new Hashtable();
                                                                SpicejetMealImageList.GetAllmealSG(htssr);
                                                                var MealName = htssr[ssrCode];
                                                                MealConcatenation += fee.SSRCode + "-" + MealName + ",";
                                                            }
                                                        }
                                                    }
                                                    Hashtable TicketMealTax = new Hashtable();
                                                    Hashtable TicketMealAmountTax = new Hashtable();
                                                    Hashtable TicketCarryBagAMountTax = new Hashtable();

                                                    // Iterate through service charges
                                                    int ServiceCount = fee.ServiceCharges.Length;
                                                    if (fee.FeeType.ToString().StartsWith("SeatFee"))
                                                    {
                                                        foreach (var serviceCharge in fee.ServiceCharges)
                                                        {
                                                            string serviceChargeCode = serviceCharge.ChargeCode?.ToString();
                                                            double amount = (serviceCharge.Amount != null) ? Convert.ToDouble(serviceCharge.Amount) : 0;
                                                            if (serviceChargeCode != null)
                                                            {
                                                                if (fee.FlightReference.ToString().Contains(oridest) == true)
                                                                {
                                                                    if (serviceCharge.ChargeType.ToString() == "ServiceCharge")
                                                                    {
                                                                        TotalAmount_Seat = amount;
                                                                        //TicketSeat[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Seat;
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "IncludedTax")
                                                                    {
                                                                        TotalAmount_Seat_tax += Convert.ToDecimal(amount);
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "Discount")
                                                                    {
                                                                        TotalAmount_Seat_discount += Convert.ToDecimal(amount);
                                                                    }
                                                                }
                                                            }

                                                        }
                                                    }
                                                    else if (!fee.FeeType.ToString().StartsWith("SeatFee") && !ssrCode.Equals("INFT") && !ssrCode.ToString().ToLower().Contains("tax") && ssrCode.StartsWith("E", StringComparison.OrdinalIgnoreCase) == false)
                                                    {
                                                        foreach (var serviceCharge in fee.ServiceCharges)
                                                        {
                                                            string serviceChargeCode = serviceCharge.ChargeCode?.ToString();
                                                            double amount = (serviceCharge.Amount != null) ? Convert.ToDouble(serviceCharge.Amount) : 0;
                                                            if (serviceChargeCode != null)
                                                            {
                                                                //|| (fee.ServiceCharges.Length > 1 && fee.ServiceCharges[1].ChargeDetail.Trim().ToUpper().ToString().Replace("-", "").Contains(oridest.Trim().ToUpper()))
                                                                if (fee.FlightReference.ToString().Contains(oridest) == true)
                                                                {

                                                                    if (serviceCharge.ChargeType.ToString() == "ServiceCharge")
                                                                    {
                                                                        TotalAmount_Meals = amount;
                                                                        //TicketMealAmount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Meals;
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "Tax" || serviceCharge.ChargeType.ToString() == "IncludedTax")
                                                                    {
                                                                        TotalAmount_Meals_tax += Convert.ToDecimal(amount);
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "Discount")
                                                                    {
                                                                        TotalAmount_Meals_discount += Convert.ToDecimal(amount);
                                                                    }
                                                                }

                                                            }

                                                        }
                                                    }
                                                    else if (fee.FeeCode.ToString().StartsWith("E"))
                                                    {
                                                        foreach (var serviceCharge in fee.ServiceCharges)
                                                        {
                                                            string serviceChargeCode = serviceCharge.ChargeCode?.ToString();
                                                            double amount = (serviceCharge.Amount != null) ? Convert.ToDouble(serviceCharge.Amount) : 0;
                                                            if (serviceChargeCode != null && isegment == 0)
                                                            {
                                                                if (serviceChargeCode.StartsWith("E") && serviceCharge.ChargeType.ToString() == "ServiceCharge")
                                                                {
                                                                    TotalAmount_Baggage += amount;
                                                                    //TicketCarryBagAMount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Baggage;
                                                                }
                                                                else if (serviceCharge.ChargeType.ToString() == "IncludedTax")
                                                                {
                                                                    TotalAmount_Baggage_tax += Convert.ToDecimal(amount);
                                                                }
                                                                else if (serviceCharge.ChargeType.ToString() == "Discount")
                                                                {
                                                                    TotalAmount_Baggage_discount += Convert.ToDecimal(amount);
                                                                }
                                                            }

                                                        }
                                                    }
                                                    else if (ssrCode.Equals("FFWD"))
                                                    {
                                                        tb_Passengerobj.FastForwardService = 'Y';
                                                    }
                                                    else if (ssrCode.Equals("INFT"))
                                                    {

                                                        foreach (var serviceCharge in fee.ServiceCharges)
                                                        {
                                                            if (fee.FlightReference.ToString().Contains(oridest) == true)
                                                            {
                                                                string serviceChargeCode = serviceCharge.ChargeCode?.ToString();
                                                                double amount = (serviceCharge.Amount != null) ? Convert.ToDouble(serviceCharge.Amount) : 0;
                                                                if (serviceChargeCode != null && isegment == 0)
                                                                {
                                                                    if (serviceCharge.ChargeType.ToString() == "ServiceCharge")
                                                                    {
                                                                        tb_Passengerobj.InftAmount = amount;
                                                                        //TicketCarryBagAMount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Baggage;
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "Tax")
                                                                    {
                                                                        tb_Passengerobj.InftAmount_Tax += Convert.ToDouble(amount);
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "Discount")
                                                                    {
                                                                        //TotalAmount_Baggage_discount += Convert.ToDecimal(amount);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                tb_Passengerobj.InftAmount = 0.0;
                                                                tb_Passengerobj.InftAmount_Tax = 0.0;
                                                            }

                                                        }

                                                        //tb_Passengerobj.InftAmount -= tb_Passengerobj.InftAmount_Tax;
                                                    }
                                                }

                                                tb_Passengerobj.TotalAmount_Seat = TotalAmount_Seat;
                                                tb_Passengerobj.TotalAmount_Seat_tax = TotalAmount_Seat_tax;
                                                tb_Passengerobj.TotalAmount_Seat_tax_discount = TotalAmount_Seat_discount;
                                                tb_Passengerobj.TotalAmount_Meals = TotalAmount_Meals;
                                                tb_Passengerobj.TotalAmount_Meals_tax = Convert.ToDouble(TotalAmount_Meals_tax);
                                                tb_Passengerobj.TotalAmount_Meals_discount = Convert.ToDouble(TotalAmount_Meals_discount);
                                                tb_Passengerobj.BaggageTotalAmount = TotalAmount_Baggage;
                                                tb_Passengerobj.BaggageTotalAmountTax = TotalAmount_Baggage_tax;
                                                tb_Passengerobj.BaggageTotalAmountTax_discount = TotalAmount_Baggage_discount;
                                                tb_Passengerobj.Carrybages = carryBagesConcatenation.TrimEnd(',');
                                                tb_Passengerobj.MealsCode = MealConcatenation.TrimEnd(',');

                                                tb_PassengerDetailsList.Add(tb_Passengerobj);
                                            }
                                        }



                                        int JourneysCount = _getBookingResponse.Booking.Journeys.Length;
                                        tb_JourneysList = new List<tb_journeys>();
                                        for (int i = 0; i < JourneysCount; i++)
                                        {
                                            tb_journeys tb_JourneysObj = new tb_journeys();
                                            tb_JourneysObj.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                            tb_JourneysObj.JourneyKey = _getBookingResponse.Booking.Journeys[i].JourneySellKey;
                                            tb_JourneysObj.Stops = _getBookingResponse.Booking.Journeys[i].Segments.Length;
                                            tb_JourneysObj.JourneyKeyCount = i;
                                            tb_JourneysObj.FlightType = "";
                                            tb_JourneysObj.Origin = _getBookingResponse.Booking.Journeys[i].Segments[0].DepartureStation;
                                            int len = _getBookingResponse.Booking.Journeys[i].Segments.Length;
                                            tb_JourneysObj.Destination = _getBookingResponse.Booking.Journeys[i].Segments[len - 1].ArrivalStation;
                                            tb_JourneysObj.DepartureDate = Convert.ToDateTime(_getBookingResponse.Booking.Journeys[i].Segments[0].STD);
                                            tb_JourneysObj.ArrivalDate = Convert.ToDateTime(_getBookingResponse.Booking.Journeys[i].Segments[len - 1].STA);
                                            tb_JourneysObj.CreatedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.CreatedDate);
                                            tb_JourneysObj.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                            tb_JourneysObj.ModifiedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.ModifiedDate);
                                            tb_JourneysObj.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                            tb_JourneysObj.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                            tb_JourneysList.Add(tb_JourneysObj);
                                        }
                                        int SegmentReturnCountt = _getBookingResponse.Booking.Journeys[0].Segments.Length;
                                        segmentReturnsListt = new List<tb_Segments>();
                                        for (int j = 0; j < SegmentReturnCountt; j++)
                                        {
                                            tb_Segments segmentReturnobj = new tb_Segments();
                                            segmentReturnobj.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                            segmentReturnobj.journeyKey = _getBookingResponse.Booking.Journeys[0].JourneySellKey;
                                            segmentReturnobj.SegmentKey = _getBookingResponse.Booking.Journeys[0].Segments[j].SegmentSellKey;
                                            segmentReturnobj.SegmentCount = j;
                                            segmentReturnobj.Origin = _getBookingResponse.Booking.Journeys[0].Segments[j].DepartureStation;
                                            segmentReturnobj.Destination = _getBookingResponse.Booking.Journeys[0].Segments[j].ArrivalStation;

                                            parsedDate = DateTime.ParseExact(_getBookingResponse.Booking.Journeys[0].Segments[j].STA.ToString(), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                            segmentReturnobj.ArrivalDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                            parsedDate = DateTime.ParseExact(_getBookingResponse.Booking.Journeys[0].Segments[j].STD.ToString(), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                            segmentReturnobj.DepartureDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                            segmentReturnobj.Identifier = _getBookingResponse.Booking.Journeys[0].Segments[j].FlightDesignator.FlightNumber;
                                            segmentReturnobj.CarrierCode = _getBookingResponse.Booking.Journeys[0].Segments[j].FlightDesignator.CarrierCode;
                                            segmentReturnobj.Seatnumber = "";
                                            segmentReturnobj.MealCode = "";
                                            segmentReturnobj.MealDiscription = "";
                                            if (!string.IsNullOrEmpty(_getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.DepartureTerminal))
                                            {
                                                var match = Regex.Match(_getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.DepartureTerminal, @"^\d+");
                                                if (match.Success)
                                                    segmentReturnobj.DepartureTerminal = Convert.ToInt32(match.Value);
                                            }

                                            if (!string.IsNullOrEmpty(_getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.ArrivalTerminal))
                                            {
                                                var match = Regex.Match(_getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.ArrivalTerminal, @"^\d+");
                                                if (match.Success)
                                                    segmentReturnobj.ArrivalTerminal = Convert.ToInt32(match.Value);
                                            }
                                            segmentReturnobj.CreatedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.CreatedDate);
                                            segmentReturnobj.ModifiedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.ModifiedDate);
                                            segmentReturnobj.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                            segmentReturnobj.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                            segmentReturnobj.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                            segmentReturnsListt.Add(segmentReturnobj);
                                        }

                                        //LogOut 
                                        LogoutRequest _logoutRequestobj = new LogoutRequest();
                                        LogoutResponse _logoutResponse = new LogoutResponse();
                                        _logoutRequestobj.ContractVersion = 420;
                                        _logoutRequestobj.Signature = token;
                                        objSpiceJet = new SpiceJetApiController();
                                        _logoutResponse = await objSpiceJet.Logout(_logoutRequestobj);
                                        logs.WriteLogs(JsonConvert.SerializeObject(_logoutRequestobj), "17-LogoutRequest", "SpicejetOneWay", "oneway");
                                        logs.WriteLogs(JsonConvert.SerializeObject(_logoutResponse), "17-LogoutResponse", "SpicejetOneWay", "oneway");

                                        //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_logoutRequestobj) + "\n Response: " + JsonConvert.SerializeObject(_logoutResponse), "Logout", "SpicejetOneWay", "oneway");
                                        //}
                                        //}
                                        #endregion

                                    }
                                    else
                                    {
                                        ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                                        _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                    }
                    else if (flagIndigo == true && data.Airline[k1].ToLower().Contains("indigo"))
                    {
                        //flagIndigo = false;
                        #region Indigo Commit
                        //Spicejet
                        token = string.Empty;
                        SearchLog searchLog = new SearchLog();
                        searchLog = _mongoDBHelper.GetFlightSearchLog(Guid).Result;
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Indigo").Result;

                        if (k1 == 0)
                        {
                            tokenview = tokenData.Token;
                        }
                        else
                        {
                            tokenview = tokenData.RToken;
                        }
                        if (!string.IsNullOrEmpty(tokenview))
                        {
                            if (tokenview == null) { tokenview = ""; }
                            token = tokenview.Replace(@"""", string.Empty);
                            string passengernamedetails = HttpContext.Session.GetString("PassengerNameDetailsIndigo");
                            List<passkeytype> passeengerlist = (List<passkeytype>)JsonConvert.DeserializeObject(passengernamedetails, typeof(List<passkeytype>));
                            string contactdata = HttpContext.Session.GetString("ContactDetails");
                            IndigoBookingManager_.UpdateContactsRequest contactList = (IndigoBookingManager_.UpdateContactsRequest)JsonConvert.DeserializeObject(contactdata, typeof(IndigoBookingManager_.UpdateContactsRequest));
                            using (HttpClient client1 = new HttpClient())
                            {
                                _commit objcommit = new _commit();
                                #region GetState
                                _sell objsell = new _sell();
                                IndigoBookingManager_.GetBookingFromStateResponse _GetBookingFromStateRS1 = await objsell.GetBookingFromState(token, 0, "");

                                string strdata = JsonConvert.SerializeObject(_GetBookingFromStateRS1);
                                decimal Totalpayment = 0M;
                                if (_GetBookingFromStateRS1 != null)
                                {
                                    Totalpayment = _GetBookingFromStateRS1.BookingData.BookingSum.TotalCost;
                                }
                                #endregion
                                #region Addpayment For Api payment deduction
                                IndigoBookingManager_.AddPaymentToBookingResponse _BookingPaymentResponse = await objcommit.AddpaymenttoBook(token, Totalpayment);

                                #endregion
                                if (_BookingPaymentResponse.BookingPaymentResponse.ValidationPayment.PaymentValidationErrors.Length > 0 && _BookingPaymentResponse.BookingPaymentResponse.ValidationPayment.PaymentValidationErrors[0].ErrorDescription.ToLower().Contains("not enough funds available"))
                                {
                                    _AirLinePNRTicket.ErrorDesc = "Not enough funds available.";
                                }
                                #region Commit Booking
                                IndigoBookingManager_.BookingCommitResponse _BookingCommitResponse = await objcommit.commit(token, contactList, passeengerlist);
                                if (_BookingCommitResponse != null && _BookingCommitResponse.BookingUpdateResponseData.Success.RecordLocator != null)
                                {
                                    IndigoBookingManager_.GetBookingResponse _getBookingResponse = await objcommit.GetBookingdetails(token, _BookingCommitResponse);

                                    if (_getBookingResponse != null)
                                    {
                                        Hashtable htname = new Hashtable();
                                        Hashtable htnameempty = new Hashtable();
                                        Hashtable htpax = new Hashtable();

                                        Hashtable htseatdata = new Hashtable();
                                        Hashtable htmealdata = new Hashtable();
                                        Hashtable htbagdata = new Hashtable();
                                        int adultcount = searchLog.Adults;
                                        int childcount = searchLog.Children;
                                        int infantcount = searchLog.Infants;
                                        int TotalCount = adultcount + childcount;
                                        string _responceGetBooking = JsonConvert.SerializeObject(_getBookingResponse);
                                        ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                                        var totalAmount = _getBookingResponse.Booking.BookingSum.TotalCost;
                                        returnTicketBooking.bookingKey = _getBookingResponse.Booking.BookingID.ToString();

                                        ReturnPaxSeats _unitdesinator = new ReturnPaxSeats();
                                        if (_getBookingResponse.Booking.Journeys[0].Segments[0].PaxSeats != null && _getBookingResponse.Booking.Journeys[0].Segments[0].PaxSeats.Length > 0)
                                            _unitdesinator.unitDesignatorPax = _getBookingResponse.Booking.Journeys[0].Segments[0].PaxSeats[0].UnitDesignator;
                                        //GST Number
                                        if (_getBookingResponse.Booking.BookingContacts[0].TypeCode == "I")
                                        {
                                            returnTicketBooking.customerNumber = _getBookingResponse.Booking.BookingContacts[0].CustomerNumber;
                                            returnTicketBooking.companyName = _getBookingResponse.Booking.BookingContacts[0].CompanyName;
                                            returnTicketBooking.emailAddressgst = _getBookingResponse.Booking.BookingContacts[0].EmailAddress;
                                        }

                                        Contacts _contact = new Contacts();
                                        _contact.phoneNumbers = _getBookingResponse.Booking.BookingContacts[0].HomePhone.ToString();
                                        if (_unitdesinator.unitDesignatorPax != null)
                                            _contact.ReturnPaxSeats = _unitdesinator.unitDesignatorPax.ToString();


                                        returnTicketBooking.airLines = "Indigo";
                                        returnTicketBooking.recordLocator = _getBookingResponse.Booking.RecordLocator;

                                        Breakdown breakdown = new Breakdown();
                                        List<JourneyTotals> journeyBaseFareobj = new List<JourneyTotals>();
                                        JourneyTotals journeyTotalsobj = new JourneyTotals();

                                        PassengerTotals passengerTotals = new PassengerTotals();
                                        ReturnSeats returnSeats = new ReturnSeats();
                                        passengerTotals.specialServices = new SpecialServices();
                                        passengerTotals.baggage = new SpecialServices(); // Vinay Bag
                                        var totalTax = "";
                                        foreach (var item in _getBookingResponse.Booking.Passengers)
                                        {
                                            htname.Add(item.PassengerNumber, item.Names[0].LastName + "/" + item.Names[0].FirstName);
                                        }

                                        //barcode
                                        BarcodePNR = _getBookingResponse.Booking.RecordLocator;
                                        if (BarcodePNR != null && BarcodePNR.Length < 7)
                                        {
                                            BarcodePNR = BarcodePNR.PadRight(7);
                                        }
                                        List<string> barcodeImage = new List<string>();

                                        #region Itenary segment and legs

                                        int journeyscount = _getBookingResponse.Booking.Journeys.Length;
                                        List<JourneysReturn> AAJourneyList = new List<JourneysReturn>();
                                        for (int i = 0; i < journeyscount; i++)
                                        {

                                            JourneysReturn AAJourneyobj = new JourneysReturn();
                                            AAJourneyobj.journeyKey = _getBookingResponse.Booking.Journeys[i].JourneySellKey;

                                            int segmentscount = _getBookingResponse.Booking.Journeys[i].Segments.Length;
                                            List<SegmentReturn> AASegmentlist = new List<SegmentReturn>();
                                            for (int j = 0; j < segmentscount; j++)
                                            {
                                                returnSeats.unitDesignator = string.Empty;
                                                returnSeats.SSRCode = string.Empty;
                                                DesignatorReturn AADesignatorobj = new DesignatorReturn();
                                                AADesignatorobj.origin = _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation;
                                                AADesignatorobj.destination = _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation;
                                                AADesignatorobj.departure = _getBookingResponse.Booking.Journeys[i].Segments[j].STD;
                                                AADesignatorobj.arrival = _getBookingResponse.Booking.Journeys[i].Segments[j].STA;
                                                AAJourneyobj.designator = AADesignatorobj;


                                                SegmentReturn AASegmentobj = new SegmentReturn();
                                                DesignatorReturn AASegmentDesignatorobj = new DesignatorReturn();

                                                AASegmentDesignatorobj.origin = _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation;
                                                AASegmentDesignatorobj.destination = _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation;
                                                AASegmentDesignatorobj.departure = _getBookingResponse.Booking.Journeys[i].Segments[j].STD;
                                                AASegmentDesignatorobj.arrival = _getBookingResponse.Booking.Journeys[i].Segments[j].STA;
                                                AASegmentobj.designator = AASegmentDesignatorobj;
                                                orides = AASegmentDesignatorobj.origin + AASegmentDesignatorobj.destination;
                                                int fareCount = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares.Length;
                                                List<FareReturn> AAFarelist = new List<FareReturn>();
                                                for (int k = 0; k < fareCount; k++)
                                                {
                                                    FareReturn AAFareobj = new FareReturn();
                                                    AAFareobj.fareKey = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].FareSellKey;
                                                    AAFareobj.productClass = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].ProductClass;

                                                    var passengerFares = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares;

                                                    int passengerFarescount = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares.Length;
                                                    List<PassengerFareReturn> PassengerfarelistRT = new List<PassengerFareReturn>();
                                                    double AdtAmount = 0.0;
                                                    double AdttaxAmount = 0.0;
                                                    double chdAmount = 0.0;
                                                    double chdtaxAmount = 0.0;
                                                    if (passengerFarescount > 0)
                                                    {
                                                        for (int l = 0; l < passengerFarescount; l++)
                                                        {
                                                            journeyTotalsobj = new JourneyTotals();
                                                            PassengerFareReturn AAPassengerfareobject = new PassengerFareReturn();
                                                            AAPassengerfareobject.passengerType = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].PaxType;

                                                            var serviceCharges1 = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges;
                                                            int serviceChargescount = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges.Length;
                                                            List<ServiceChargeReturn> AAServicechargelist = new List<ServiceChargeReturn>();
                                                            for (int m = 0; m < serviceChargescount; m++)
                                                            {
                                                                ServiceChargeReturn AAServicechargeobj = new ServiceChargeReturn();
                                                                AAServicechargeobj.amount = Convert.ToInt32(_getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                string _data = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].ChargeType.ToString().ToLower().Trim();
                                                                if (_data == "fareprice")
                                                                {
                                                                    journeyTotalsobj.totalAmount += Convert.ToInt32(_getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                }
                                                                else
                                                                {
                                                                    journeyTotalsobj.totalTax += Convert.ToInt32(_getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                }


                                                                AAServicechargelist.Add(AAServicechargeobj);
                                                            }

                                                            if (AAPassengerfareobject.passengerType.Equals("ADT"))
                                                            {
                                                                AdtAmount += journeyTotalsobj.totalAmount * adultcount;
                                                                AdttaxAmount += journeyTotalsobj.totalTax * adultcount;
                                                            }

                                                            if (AAPassengerfareobject.passengerType.Equals("CHD"))
                                                            {
                                                                chdAmount += journeyTotalsobj.totalAmount * childcount;
                                                                chdtaxAmount += journeyTotalsobj.totalTax * childcount;
                                                            }


                                                            AAPassengerfareobject.serviceCharges = AAServicechargelist;
                                                            PassengerfarelistRT.Add(AAPassengerfareobject);

                                                        }
                                                        journeyTotalsobj.totalAmount = AdtAmount + chdAmount;
                                                        journeyTotalsobj.totalTax = AdttaxAmount + chdtaxAmount;
                                                        journeyBaseFareobj.Add(journeyTotalsobj);
                                                        AAFareobj.passengerFares = PassengerfarelistRT;

                                                        AAFarelist.Add(AAFareobj);
                                                    }
                                                }
                                                //breakdown.journeyTotals = journeyTotalsobj;
                                                breakdown.passengerTotals = passengerTotals;
                                                AASegmentobj.fares = AAFarelist;
                                                IdentifierReturn AAIdentifierobj = new IdentifierReturn();

                                                AAIdentifierobj.identifier = _getBookingResponse.Booking.Journeys[i].Segments[j].FlightDesignator.FlightNumber;
                                                AAIdentifierobj.carrierCode = _getBookingResponse.Booking.Journeys[i].Segments[j].FlightDesignator.CarrierCode;

                                                AASegmentobj.identifier = AAIdentifierobj;
                                                //barCode
                                                //julian date
                                                Journeydatetime = DateTime.Parse(_getBookingResponse.Booking.Journeys[i].Segments[j].STD.ToString());
                                                carriercode = AAIdentifierobj.carrierCode;
                                                flightnumber = AAIdentifierobj.identifier;
                                                int year = Journeydatetime.Year;
                                                int month = Journeydatetime.Month;
                                                int day = Journeydatetime.Day;
                                                // Calculate the number of days from January 1st to the given date
                                                DateTime currentDate = new DateTime(year, month, day);
                                                DateTime startOfYear = new DateTime(year, 1, 1);
                                                int julianDate = (currentDate - startOfYear).Days + 1;
                                                if (string.IsNullOrEmpty(sequencenumber))
                                                {
                                                    sequencenumber = "00000";
                                                }
                                                else
                                                {
                                                    sequencenumber = sequencenumber.PadRight(5, '0');
                                                }
                                                var leg = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs;
                                                int legcount = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs.Length;
                                                List<LegReturn> AALeglist = new List<LegReturn>();
                                                for (int n = 0; n < legcount; n++)
                                                {
                                                    LegReturn AALeg = new LegReturn();
                                                    //AALeg.legKey = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legKey;
                                                    DesignatorReturn AAlegDesignatorobj = new DesignatorReturn();
                                                    AAlegDesignatorobj.origin = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].DepartureStation;
                                                    AAlegDesignatorobj.destination = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].ArrivalStation;
                                                    AAlegDesignatorobj.departure = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].STD;
                                                    AAlegDesignatorobj.arrival = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].STA;
                                                    AALeg.designator = AAlegDesignatorobj;

                                                    LegInfoReturn AALeginfoobj = new LegInfoReturn();
                                                    AALeginfoobj.arrivalTerminal = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].LegInfo.ArrivalTerminal;
                                                    AALeginfoobj.arrivalTime = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].LegInfo.PaxSTA;
                                                    AALeginfoobj.departureTerminal = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].LegInfo.DepartureTerminal;
                                                    AALeginfoobj.departureTime = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].LegInfo.PaxSTD;
                                                    AALeg.legInfo = AALeginfoobj;
                                                    AALeglist.Add(AALeg);

                                                }
                                                foreach (var item in _getBookingResponse.Booking.Passengers)
                                                {
                                                    if (!htnameempty.Contains(item.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation))
                                                    {
                                                        if (carriercode.Length < 3)
                                                            carriercode = carriercode.PadRight(3);
                                                        if (flightnumber.Length < 5)
                                                        {
                                                            flightnumber = flightnumber.PadRight(5);
                                                        }
                                                        if (sequencenumber.Length < 5)
                                                            sequencenumber = sequencenumber.PadRight(5, '0');
                                                        seatnumber = "0000";
                                                        if (seatnumber.Length < 4)
                                                            seatnumber = seatnumber.PadLeft(4, '0');
                                                        BarcodeString = "M" + "1" + htname[item.PassengerNumber] + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                                        htnameempty.Add(item.PassengerNumber.ToString() + "_" + htname[item.PassengerNumber] + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, BarcodeString);
                                                    }
                                                }

                                                //vivek
                                                foreach (var item1 in _getBookingResponse.Booking.Journeys[i].Segments[j].PaxSeats)
                                                {
                                                    barcodeImage = new List<string>();
                                                    try
                                                    {
                                                        if (!htseatdata.Contains(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation))
                                                        {
                                                            htseatdata.Add(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, item1.UnitDesignator);
                                                            returnSeats.unitDesignator += item1.UnitDesignator + ",";
                                                        }
                                                        if (!htpax.Contains(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation))
                                                        {
                                                            if (carriercode.Length < 3)
                                                                carriercode = carriercode.PadRight(3);
                                                            if (flightnumber.Length < 5)
                                                            {
                                                                flightnumber = flightnumber.PadRight(5);
                                                            }
                                                            if (sequencenumber.Length < 5)
                                                                sequencenumber = sequencenumber.PadRight(5, '0');
                                                            seatnumber = htseatdata[item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation].ToString();
                                                            if (seatnumber.Length < 4)
                                                                seatnumber = seatnumber.PadLeft(4, '0');
                                                            BarcodeString = "M" + "1" + htname[item1.PassengerNumber] + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                                            htpax.Add(item1.PassengerNumber.ToString() + "_" + htname[item1.PassengerNumber] + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, BarcodeString);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }
                                                //SSR
                                                foreach (var item1 in _getBookingResponse.Booking.Journeys[i].Segments[j].PaxSSRs)
                                                {
                                                    try
                                                    {
                                                        if (!htmealdata.Contains(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation) && item1.SSRCode != "INFT" && item1.SSRCode != "FFWD" && !item1.SSRCode.StartsWith('X'))
                                                        {
                                                            htmealdata.Add(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, item1.SSRCode);
                                                            returnSeats.SSRCode += item1.SSRCode + ",";
                                                        }

                                                        else if (!htbagdata.Contains(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation) && item1.SSRCode != "INFT" && item1.SSRCode != "FFWD")
                                                        {

                                                            htbagdata.Add(item1.PassengerNumber.ToString() + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].DepartureStation + "_" + _getBookingResponse.Booking.Journeys[i].Segments[j].ArrivalStation, item1.SSRCode);
                                                            returnSeats.SSRCode += item1.SSRCode + ",";


                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }

                                                AASegmentobj.unitdesignator = returnSeats.unitDesignator;
                                                AASegmentobj.SSRCode = returnSeats.SSRCode;
                                                AASegmentobj.legs = AALeglist;
                                                AASegmentlist.Add(AASegmentobj);
                                                breakdown.journeyfareTotals = journeyBaseFareobj;
                                            }

                                            AAJourneyobj.segments = AASegmentlist;
                                            AAJourneyList.Add(AAJourneyobj);

                                        }

                                        #endregion

                                        // string stravailibitilityrequest = HttpContext.Session.GetString("IndigoAvailibilityRequest");


                                        string stravailibitilityrequest = objMongoHelper.UnZip(tokenData.PassRequest);
                                        GetAvailabilityRequest availibiltyRQ = JsonConvert.DeserializeObject<GetAvailabilityRequest>(stravailibitilityrequest);

                                        var passanger = _getBookingResponse.Booking.Passengers;
                                        int passengercount = availibiltyRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCount;
                                        ReturnPassengers passkeytypeobj = new ReturnPassengers();
                                        List<ReturnPassengers> passkeylist = new List<ReturnPassengers>();
                                        foreach (var item in _getBookingResponse.Booking.Passengers)
                                        {
                                            foreach (var item1 in item.PassengerFees)
                                            {
                                                if (item1.FeeCode.Equals("SEAT") || item1.FeeType.ToString().ToLower().Contains("seat"))
                                                {
                                                    foreach (var item2 in item1.ServiceCharges)
                                                    {
                                                        if (item2.ChargeCode.Equals("SEAT") || item2.ChargeCode.Equals("SNXT"))
                                                        {
                                                            returnSeats.total += Convert.ToInt32(item2.Amount);
                                                        }
                                                        else
                                                        {
                                                            returnSeats.taxes += Convert.ToInt32(item2.Amount);
                                                        }
                                                    }
                                                }
                                                else if (item1.FeeCode.Equals("INFT"))
                                                {
                                                    JourneyTotals InfantfareTotals = new JourneyTotals();
                                                    foreach (var item2 in item1.ServiceCharges)
                                                    {
                                                        if (item2.ChargeCode.Equals("INFT"))
                                                        {
                                                            InfantfareTotals.totalAmount = Convert.ToInt32(item2.Amount);
                                                        }
                                                        else
                                                        {
                                                            InfantfareTotals.totalTax += Convert.ToInt32(item2.Amount);
                                                        }
                                                    }
                                                    InfantfareTotals.totalAmount = InfantfareTotals.totalAmount - InfantfareTotals.totalTax;
                                                    journeyBaseFareobj.Add(InfantfareTotals);
                                                    breakdown.journeyfareTotals = journeyBaseFareobj;
                                                }
                                                else
                                                {
                                                    foreach (var item2 in item1.ServiceCharges)
                                                    {
                                                        if ((!item2.ChargeCode.Equals("SEAT") || !item2.ChargeCode.Equals("INFT")) && !item2.ChargeType.ToString().ToLower().Contains("tax") && item2.ChargeCode.StartsWith("X", StringComparison.OrdinalIgnoreCase) == false)
                                                        {
                                                            passengerTotals.specialServices.total += Convert.ToInt32(item2.Amount);
                                                            TotalMeal = passengerTotals.specialServices.total;
                                                        }
                                                        else if (item2.ChargeCode.StartsWith("X", StringComparison.OrdinalIgnoreCase) == true)
                                                        {
                                                            passengerTotals.baggage.total += Convert.ToInt32(item2.Amount);
                                                            TotalBag = passengerTotals.baggage.total;
                                                        }
                                                        else
                                                        {
                                                            if (item2.ChargeCode.Equals("FFWD"))
                                                            {
                                                                passengerTotals.fastForward.total += Convert.ToInt32(item2.Amount);
                                                                TotalFastFFWD = passengerTotals.fastForward.total;
                                                            }
                                                            else
                                                            {
                                                                passengerTotals.specialServices.taxes += Convert.ToInt32(item2.Amount);
                                                            }
                                                            TotalBagtax = passengerTotals.specialServices.taxes;
                                                        }
                                                        Totatamountmb = TotalMeal + TotalBag + TotalFastFFWD;
                                                    }
                                                }
                                            }
                                            passkeytypeobj = new ReturnPassengers();
                                            passkeytypeobj.name = new Name();
                                            passkeytypeobj.passengerTypeCode = item.PassengerTypeInfo.PaxType;
                                            passkeytypeobj.name.first = item.Names[0].FirstName;
                                            passkeytypeobj.name.last = item.Names[0].LastName;
                                            for (int i = 0; i < passeengerlist.Count; i++)
                                            {
                                                if (passkeytypeobj.passengerTypeCode == passeengerlist[i].passengertypecode && passkeytypeobj.name.first.ToLower() == passeengerlist[i].first.ToLower() && passkeytypeobj.name.last.ToLower() == passeengerlist[i].last.ToLower())
                                                {
                                                    passkeytypeobj.MobNumber = passeengerlist[i].mobile;
                                                    string[] splitStr = passeengerlist[i].passengercombinedkey.Split('@');
                                                    for (int ia = 0; ia < splitStr.Length; ia++)
                                                    {
                                                        if (splitStr[ia].ToLower().Trim().Contains("indigo"))
                                                        {
                                                            string[] beforeCaret = splitStr[ia].Split('^');
                                                            passkeytypeobj.passengerKey = beforeCaret[0];
                                                            break;
                                                        }

                                                    }
                                                    //passkeytypeobj.passengerKey = passeengerlist[i].passengerkey;
                                                    //break;
                                                }

                                            }



                                            passkeylist.Add(passkeytypeobj);
                                            if (item.Infant != null)
                                            {
                                                passkeytypeobj = new ReturnPassengers();
                                                passkeytypeobj.name = new Name();
                                                passkeytypeobj.passengerTypeCode = "INFT";
                                                //passkeytypeobj.name.first = item.Infant.Names[0].FirstName + " " + item.Infant.Names[0].LastName;
                                                passkeytypeobj.name.first = item.Infant.Names[0].FirstName;
                                                passkeytypeobj.name.last = item.Infant.Names[0].LastName;
                                                for (int i = 0; i < passeengerlist.Count; i++)
                                                {
                                                    if (passkeytypeobj.passengerTypeCode == passeengerlist[i].passengertypecode && passkeytypeobj.name.first.ToLower() == passeengerlist[i].first.ToLower() && passkeytypeobj.name.last.ToLower() == passeengerlist[i].last.ToLower())
                                                    {
                                                        passkeytypeobj.MobNumber = passeengerlist[i].mobile;
                                                        passkeytypeobj.passengerKey = passeengerlist[i].passengerkey;
                                                        break;
                                                    }

                                                }
                                                passkeylist.Add(passkeytypeobj);

                                            }
                                        }

                                        double BasefareAmt = 0.0;
                                        double BasefareTax = 0.0;
                                        for (int i = 0; i < breakdown.journeyfareTotals.Count; i++)
                                        {
                                            BasefareAmt += breakdown.journeyfareTotals[i].totalAmount;
                                            BasefareTax += breakdown.journeyfareTotals[i].totalTax;
                                        }
                                        breakdown.journeyTotals = new JourneyTotals();
                                        breakdown.journeyTotals.totalAmount = Convert.ToDouble(BasefareAmt);
                                        breakdown.passengerTotals.seats = new ReturnSeats();
                                        breakdown.passengerTotals.specialServices.total = passengerTotals.specialServices.total;
                                        breakdown.passengerTotals.baggage.total = passengerTotals.baggage.total;
                                        breakdown.passengerTotals.seats.total = returnSeats.total;
                                        breakdown.passengerTotals.seats.taxes = returnSeats.taxes;
                                        breakdown.journeyTotals.totalTax = Convert.ToDouble(BasefareTax);
                                        breakdown.totalAmount = breakdown.journeyTotals.totalAmount + breakdown.journeyTotals.totalTax;
                                        if (totalAmount != 0M)
                                        {
                                            breakdown.totalToCollect = Convert.ToDouble(totalAmount);
                                        }
                                        returnTicketBooking.breakdown = breakdown;
                                        returnTicketBooking.journeys = AAJourneyList;
                                        returnTicketBooking.passengers = passkeylist;
                                        returnTicketBooking.passengerscount = passengercount;
                                        returnTicketBooking.contacts = _contact;
                                        returnTicketBooking.Seatdata = htseatdata;
                                        returnTicketBooking.Mealdata = htmealdata;
                                        returnTicketBooking.Bagdata = htbagdata;
                                        returnTicketBooking.htname = htname;
                                        returnTicketBooking.htnameempty = htnameempty;
                                        returnTicketBooking.htpax = htpax;
                                        returnTicketBooking.bookingdate = _getBookingResponse.Booking.BookingInfo.BookingDate;
                                        _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);
                                        #region DB Save

                                        airLineFlightTicketBooking = new AirLineFlightTicketBooking();
                                        airLineFlightTicketBooking.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                        tb_Booking = new tb_Booking();
                                        tb_Booking.AirLineID = 4;
                                        string productcode = _getBookingResponse.Booking.Journeys[0].Segments[0].Fares[0].ProductClass;
                                        var fareName = FareList.GetAllfare().Where(x => ((string)productcode).Equals(x.ProductCode)).FirstOrDefault();
                                        tb_Booking.BookingType = "Corporate-" + _getBookingResponse.Booking.Journeys[0].Segments[0].Fares[0].ProductClass + " (" + fareName.Faredesc + ")";
                                        LegalEntity legal = new LegalEntity();
                                        legal = _mongoDBHelper.GetlegalEntityByGUID(Guid).Result;
                                        if (legal != null)
                                        {
                                            tb_Booking.CompanyName = legal.BillingEntityFullName;
                                        }
                                        else
                                        {
                                            tb_Booking.CompanyName = "";
                                        }
                                        tb_Booking.BookingRelationId = Guid;
                                        if (k1 == 0)
                                        {
                                            tb_Booking.TripType = "RoundTrip";
                                        }
                                        else
                                        {
                                            tb_Booking.TripType = "ReturnTrip";
                                        }
                                        tb_Booking.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                        tb_Booking.RecordLocator = _getBookingResponse.Booking.RecordLocator;
                                        tb_Booking.CurrencyCode = _getBookingResponse.Booking.CurrencyCode;
                                        tb_Booking.Origin = _getBookingResponse.Booking.Journeys[0].Segments[0].Legs[0].DepartureStation;
                                        int segmentcount = _getBookingResponse.Booking.Journeys[0].Segments.Length;
                                        tb_Booking.Destination = _getBookingResponse.Booking.Journeys[0].Segments[segmentcount - 1].Legs[0].ArrivalStation;
                                        tb_Booking.BookedDate = _getBookingResponse.Booking.BookingInfo.BookingDate;
                                        tb_Booking.TotalAmount = (double)_getBookingResponse.Booking.BookingSum.TotalCost;
                                        tb_Booking.SpecialServicesTotal = (double)Totatamountmb;
                                        tb_Booking.SpecialServicesTotal_Tax = (double)TotalBagtax;
                                        tb_Booking.SpecialServicesTotal -= tb_Booking.SpecialServicesTotal_Tax;
                                        tb_Booking.SeatTotalAmount = returnSeats.total;
                                        tb_Booking.SeatTotalAmount_Tax = returnSeats.taxes;
                                        tb_Booking.SeatTotalAmount -= tb_Booking.SeatTotalAmount_Tax;
                                        tb_Booking.ExpirationDate = _getBookingResponse.Booking.BookingInfo.ExpiredDate;
                                        //tb_Booking.ArrivalDate = _getBookingResponse.Booking.Journeys[0].Segments[segmentcount - 1].STA.ToString().Replace('T',' ');//DateTime.Now;
                                        //tb_Booking.DepartureDate = _getBookingResponse.Booking.Journeys[0].Segments[0].Legs[0].STD.ToString().Replace('T', ' ');//DateTime.Now;
                                        DateTime parsedDate = DateTime.ParseExact(_getBookingResponse.Booking.Journeys[0].Segments[segmentcount - 1].Legs[0].STA.ToString(), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        tb_Booking.ArrivalDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                        parsedDate = DateTime.ParseExact(_getBookingResponse.Booking.Journeys[0].Segments[0].Legs[0].STD.ToString(), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        tb_Booking.DepartureDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");

                                        tb_Booking.CreatedDate = _getBookingResponse.Booking.BookingInfo.CreatedDate;
                                        if (HttpContext.User.Identity.IsAuthenticated)
                                        {
                                            var identity = (ClaimsIdentity)User.Identity;
                                            IEnumerable<Claim> claims = identity.Claims;
                                            var userEmail = claims.Where(c => c.Type == ClaimTypes.Email).ToList()[0].Value;
                                            tb_Booking.Createdby = userEmail;// "Online";
                                        }
                                        tb_Booking.ModifiedDate = _getBookingResponse.Booking.BookingInfo.ModifiedDate;
                                        tb_Booking.ModifyBy = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        tb_Booking.BookingDoc = JsonConvert.SerializeObject(_getBookingResponse);
                                        tb_Booking.BookingStatus = "2";// _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                        tb_Booking.PaidStatus = Convert.ToInt32(_getBookingResponse.Booking.BookingInfo.PaidStatus);

                                        tb_Airlines = new tb_Airlines();
                                        tb_Airlines.AirlineID = 4;
                                        tb_Airlines.AirlneName = "";
                                        tb_Airlines.AirlineDescription = "Indigo";
                                        tb_Airlines.CreatedDate = _getBookingResponse.Booking.BookingInfo.CreatedDate;
                                        tb_Airlines.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                        tb_Airlines.Modifieddate = _getBookingResponse.Booking.BookingInfo.ModifiedDate;
                                        tb_Airlines.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        tb_Airlines.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();

                                        tb_AirCraft = new tb_AirCraft();
                                        tb_AirCraft.Id = 1;
                                        tb_AirCraft.AirlineID = 4;
                                        tb_AirCraft.AirCraftName = "";
                                        tb_AirCraft.AirCraftDescription = " ";
                                        tb_AirCraft.CreatedDate = _getBookingResponse.Booking.BookingInfo.CreatedDate;
                                        tb_AirCraft.Modifieddate = _getBookingResponse.Booking.BookingInfo.ModifiedDate;
                                        tb_AirCraft.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                        tb_AirCraft.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        tb_AirCraft.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();

                                        contactDetail = new ContactDetail();
                                        contactDetail.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                        contactDetail.FirstName = _getBookingResponse.Booking.BookingContacts[0].Names[0].FirstName;
                                        contactDetail.LastName = _getBookingResponse.Booking.BookingContacts[0].Names[0].LastName;
                                        contactDetail.EmailID = _getBookingResponse.Booking.BookingContacts[0].EmailAddress;
                                        if (_getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-').Length > 1)
                                        {
                                            contactDetail.MobileNumber = _getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-')[1];
                                            contactDetail.CountryCode = _getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-')[0];
                                        }
                                        else
                                        {
                                            contactDetail.MobileNumber = _getBookingResponse.Booking.BookingContacts[0].HomePhone;
                                            contactDetail.CountryCode = "+91";
                                        }
                                        //contactDetail.MobileNumber = _getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-')[1];
                                        //contactDetail.CountryCode = _getBookingResponse.Booking.BookingContacts[0].HomePhone.Split('-')[0];
                                        contactDetail.CreateDate = _getBookingResponse.Booking.BookingInfo.CreatedDate;
                                        contactDetail.CreateBy = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                        contactDetail.ModifyDate = _getBookingResponse.Booking.BookingInfo.ModifiedDate;
                                        contactDetail.ModifyBy = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        contactDetail.Status = Convert.ToInt32(_getBookingResponse.Booking.BookingInfo.BookingStatus);

                                        gSTDetails = new GSTDetails();
                                        if (_getBookingResponse.Booking.BookingContacts[0].CustomerNumber != null)
                                        {
                                            gSTDetails.bookingReferenceNumber = _getBookingResponse.Booking.BookingID.ToString();
                                            gSTDetails.GSTEmail = _getBookingResponse.Booking.BookingContacts[0].EmailAddress;
                                            gSTDetails.GSTNumber = _getBookingResponse.Booking.BookingContacts[0].CustomerNumber;
                                            gSTDetails.GSTName = _getBookingResponse.Booking.BookingContacts[0].CompanyName;
                                            gSTDetails.airLinePNR = _getBookingResponse.Booking.RecordLocator;
                                            gSTDetails.status = Convert.ToInt32(_getBookingResponse.Booking.BookingInfo.BookingStatus);
                                        }

                                        tb_PassengerTotalobj = new tb_PassengerTotal();
                                        bookingKey = _getBookingResponse.Booking.BookingID.ToString();
                                        tb_PassengerTotalobj.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                        if (_getBookingResponse.Booking.Passengers.Length > 0 && _getBookingResponse.Booking.Passengers[0].PassengerFees.Length > 0)
                                        {
                                            tb_PassengerTotalobj.SpecialServicesAmount = (double)Totatamountmb; // FFWD + MEAL + BAGGAGE
                                            tb_PassengerTotalobj.SpecialServicesAmount_Tax = (double)TotalBagtax; // FFWD + MEAL + BAGGAGE
                                            tb_PassengerTotalobj.TotalSeatAmount = returnSeats.total;
                                            tb_PassengerTotalobj.TotalSeatAmount_Tax = returnSeats.taxes;
                                            tb_PassengerTotalobj.SpecialServicesAmount -= tb_PassengerTotalobj.SpecialServicesAmount_Tax;
                                            tb_PassengerTotalobj.TotalSeatAmount -= tb_PassengerTotalobj.TotalSeatAmount_Tax;
                                        }
                                        tb_PassengerTotalobj.TotalBookingAmount = (double)breakdown.journeyTotals.totalAmount;
                                        tb_PassengerTotalobj.totalBookingAmount_Tax = (double)breakdown.journeyTotals.totalTax;
                                        tb_PassengerTotalobj.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                        tb_PassengerTotalobj.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                        tb_PassengerTotalobj.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                        tb_PassengerTotalobj.CreatedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.CreatedDate);
                                        tb_PassengerTotalobj.ModifiedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.ModifiedDate);


                                        tb_PassengerTotalobj.AdultCount = adultcount;
                                        tb_PassengerTotalobj.ChildCount = childcount;
                                        tb_PassengerTotalobj.InfantCount = infantcount;
                                        tb_PassengerTotalobj.TotalPax = adultcount + childcount + infantcount;

                                        var passengerCount = _getBookingResponse.Booking.Passengers;
                                        int PassengerDataCount = availibiltyRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCount;
                                        tb_PassengerDetailsList = new List<tb_PassengerDetails>();
                                        int SegmentCount = _getBookingResponse.Booking.Journeys[0].Segments.Length;
                                        for (int isegment = 0; isegment < SegmentCount; isegment++)
                                        {
                                            foreach (var items in _getBookingResponse.Booking.Passengers)
                                            {
                                                tb_PassengerDetails tb_Passengerobj = new tb_PassengerDetails();
                                                tb_Passengerobj.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                                tb_Passengerobj.PassengerKey = items.PassengerID.ToString();
                                                tb_Passengerobj.TypeCode = items.PassengerTypeInfo.PaxType;
                                                tb_Passengerobj.FirstName = items.Names[0].FirstName;
                                                tb_Passengerobj.Title = items.Names[0].Title;
                                                tb_Passengerobj.Dob = DateTime.Now;
                                                tb_Passengerobj.LastName = items.Names[0].LastName;
                                                tb_Passengerobj.contact_Emailid = passeengerlist.FirstOrDefault(x => x.first.ToUpper() == tb_Passengerobj.FirstName && x.last.ToUpper() == tb_Passengerobj.LastName).Email;
                                                tb_Passengerobj.contact_Mobileno = passeengerlist.FirstOrDefault(x => x.first.ToUpper() == tb_Passengerobj.FirstName && x.last.ToUpper() == tb_Passengerobj.LastName).mobile;
                                                tb_Passengerobj.FastForwardService = 'N';
                                                tb_Passengerobj.FrequentFlyerNumber = "";// passeengerlist.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).FrequentFlyer;
                                                if (tb_Passengerobj.Title == "MR" || tb_Passengerobj.Title == "Master" || tb_Passengerobj.Title == "MSTR")
                                                    tb_Passengerobj.Gender = "Male";
                                                else if (tb_Passengerobj.Title == "MS" || tb_Passengerobj.Title == "MRS" || tb_Passengerobj.Title == "MISS")
                                                    tb_Passengerobj.Gender = "Female";
                                                tb_Passengerobj.FrequentFlyerNumber = items.PassengerProgram.ProgramNumber;
                                                tb_Passengerobj.InftAmount = 0.0;// to do
                                                tb_Passengerobj.InftAmount_Tax = 0.0;// to do
                                                double AdtAmount = 0.0;
                                                double AdttaxAmount = 0.0;
                                                double AdtTAmount = 0.0;
                                                double AdtTtaxAmount = 0.0;
                                                //for (int isegment = 0; isegment < SegmentCount; isegment++)
                                                //{
                                                AdtAmount = 0.0;
                                                AdttaxAmount = 0.0;
                                                for (int i = 0; i < _getBookingResponse.Booking.Journeys[0].Segments[isegment].PaxSeats.Length; i++)
                                                {
                                                    if (items.PassengerNumber == _getBookingResponse.Booking.Journeys[0].Segments[isegment].PaxSeats[i].PassengerNumber)
                                                    {
                                                        var flightseatnumber1 = _getBookingResponse.Booking.Journeys[0].Segments[isegment].PaxSeats[i].UnitDesignator;
                                                        tb_Passengerobj.Seatnumber = flightseatnumber1 + ",";
                                                    }
                                                }
                                                tb_Passengerobj.SegmentsKey = _getBookingResponse.Booking.Journeys[0].Segments[isegment].SegmentSellKey;
                                                int fareCount = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares.Length;
                                                for (int k = 0; k < fareCount; k++)
                                                {
                                                    var passengerFares = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares;
                                                    int passengerFarescount = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares.Length;
                                                    if (passengerFarescount > 0)
                                                    {
                                                        for (int l = 0; l < passengerFarescount; l++)
                                                        {
                                                            if (_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].PaxType == tb_Passengerobj.TypeCode)
                                                            {
                                                                int serviceChargescount = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges.Length;
                                                                for (int m = 0; m < serviceChargescount; m++)
                                                                {
                                                                    ServiceChargeReturn AAServicechargeobj = new ServiceChargeReturn();
                                                                    AAServicechargeobj.amount = Convert.ToInt32(_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                    string data4 = _getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].ChargeType.ToString();
                                                                    if (data4.ToLower() == "fareprice")
                                                                    {
                                                                        AdtTAmount += Convert.ToInt32(_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                    }
                                                                    else
                                                                    {
                                                                        AdtTtaxAmount += Convert.ToInt32(_getBookingResponse.Booking.Journeys[0].Segments[isegment].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                                    }
                                                                }

                                                                //AdtAmount += AdtTAmount * adultcount;
                                                                //AdttaxAmount += AdtTtaxAmount * adultcount;
                                                            }

                                                        }
                                                    }
                                                }
                                                //}




                                                tb_Passengerobj.TotalAmount = (decimal)AdtTAmount;
                                                tb_Passengerobj.TotalAmount_tax = (decimal)AdtTtaxAmount;
                                                tb_Passengerobj.CreatedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.CreatedDate);
                                                tb_Passengerobj.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                                tb_Passengerobj.ModifiedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.ModifiedDate);
                                                tb_Passengerobj.ModifyBy = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                                tb_Passengerobj.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                                if (items.Infant != null & isegment == 0)
                                                {
                                                    tb_Passengerobj.Inf_TypeCode = "INFT";
                                                    tb_Passengerobj.Inf_Firstname = items.Infant.Names[0].FirstName;
                                                    tb_Passengerobj.Inf_Lastname = items.Infant.Names[0].LastName;
                                                    tb_Passengerobj.Inf_Dob = Convert.ToDateTime(items.Infant.DOB);
                                                    if (items.Infant.Gender != null)
                                                    {
                                                        tb_Passengerobj.Inf_Gender = "Master";
                                                    }
                                                    for (int i = 0; i < passeengerlist.Count; i++)
                                                    {
                                                        if (tb_Passengerobj.Inf_TypeCode == passeengerlist[i].passengertypecode && tb_Passengerobj.Inf_Firstname.ToLower() == passeengerlist[i].first.ToLower() && tb_Passengerobj.Inf_Lastname.ToLower() == passeengerlist[i].last.ToLower())
                                                        {
                                                            //tb_Passengerobj.PassengerKey = passeengerlist[i].passengerkey;
                                                            break;
                                                        }
                                                    }
                                                }
                                                string oridest = _getBookingResponse.Booking.Journeys[0].Segments[isegment].DepartureStation + _getBookingResponse.Booking.Journeys[0].Segments[isegment].ArrivalStation;

                                                // Handle carrybages and fees
                                                List<FeeDetails> feeDetails = new List<FeeDetails>();
                                                double TotalAmount_Seat = 0;
                                                decimal TotalAmount_Seat_tax = 0;
                                                decimal TotalAmount_Seat_discount = 0;
                                                double TotalAmount_Meals = 0;
                                                decimal TotalAmount_Meals_tax = 0;
                                                decimal TotalAmount_Meals_discount = 0;
                                                double TotalAmount_Baggage = 0;
                                                decimal TotalAmount_Baggage_tax = 0;
                                                decimal TotalAmount_Baggage_discount = 0;
                                                string carryBagesConcatenation = "";
                                                string MealConcatenation = "";
                                                int feesCount = items.PassengerFees.Length;
                                                foreach (var fee in items.PassengerFees)
                                                {
                                                    string ssrCode = fee.SSRCode?.ToString();
                                                    if (ssrCode != null)
                                                    {
                                                        if (ssrCode.StartsWith("X", StringComparison.OrdinalIgnoreCase) == true)
                                                        {
                                                            if (fee.FlightReference.ToString().Contains(oridest) == true)
                                                            {
                                                                var BaggageName = MealImageList.GetAllmeal()
                                                                            .Where(x => ((string)fee.SSRCode).Contains(x.MealCode))
                                                                            .Select(x => x.MealImage)
                                                                            .FirstOrDefault();
                                                                carryBagesConcatenation += fee.SSRCode + "-" + BaggageName + ",";
                                                            }
                                                        }
                                                        else if (!ssrCode.Equals("SEAT") && !ssrCode.Equals("INFT") && !ssrCode.Equals("FFWD") && ssrCode.StartsWith("X", StringComparison.OrdinalIgnoreCase) == false)
                                                        {
                                                            if (fee.FlightReference.ToString().Contains(oridest) == true)
                                                            {
                                                                Hashtable htssr = new Hashtable();
                                                                SpicejetMealImageList.GetAllmeal(htssr);
                                                                var MealName = htssr[ssrCode];
                                                                MealConcatenation += fee.SSRCode + "-" + MealName + ",";
                                                            }
                                                        }
                                                    }
                                                    Hashtable TicketMealTax = new Hashtable();
                                                    Hashtable TicketMealAmountTax = new Hashtable();
                                                    Hashtable TicketCarryBagAMountTax = new Hashtable();

                                                    // Iterate through service charges
                                                    int ServiceCount = fee.ServiceCharges.Length;
                                                    if (fee.FeeType.ToString().StartsWith("SeatFee"))
                                                    {
                                                        foreach (var serviceCharge in fee.ServiceCharges)
                                                        {
                                                            string serviceChargeCode = serviceCharge.ChargeCode?.ToString();
                                                            double amount = (serviceCharge.Amount != null) ? Convert.ToDouble(serviceCharge.Amount) : 0;
                                                            if (serviceChargeCode != null)
                                                            {
                                                                if (fee.FlightReference.ToString().Contains(oridest) == true)
                                                                {
                                                                    if (serviceCharge.ChargeType.ToString() == "ServiceCharge")
                                                                    {
                                                                        TotalAmount_Seat = amount;
                                                                        //TicketSeat[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Seat;
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "IncludedTax")
                                                                    {
                                                                        TotalAmount_Seat_tax += Convert.ToDecimal(amount);
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "Discount")
                                                                    {
                                                                        TotalAmount_Seat_discount += Convert.ToDecimal(amount);
                                                                    }
                                                                }
                                                            }

                                                        }
                                                    }
                                                    else if (!fee.FeeType.ToString().StartsWith("SeatFee") && !ssrCode.Equals("INFT") && !ssrCode.Equals("FFWD") && ssrCode.StartsWith("X", StringComparison.OrdinalIgnoreCase) == false)
                                                    {
                                                        foreach (var serviceCharge in fee.ServiceCharges)
                                                        {
                                                            string serviceChargeCode = serviceCharge.ChargeCode?.ToString();
                                                            double amount = (serviceCharge.Amount != null) ? Convert.ToDouble(serviceCharge.Amount) : 0;
                                                            if (serviceChargeCode != null)
                                                            {
                                                                if (fee.FlightReference.ToString().Contains(oridest) == true)
                                                                {

                                                                    if (serviceCharge.ChargeType.ToString() == "ServiceCharge")
                                                                    {
                                                                        TotalAmount_Meals = amount;
                                                                        //TicketMealAmount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Meals;
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "IncludedTax")
                                                                    {
                                                                        TotalAmount_Meals_tax += Convert.ToDecimal(amount);
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "Discount")
                                                                    {
                                                                        TotalAmount_Meals_discount += Convert.ToDecimal(amount);
                                                                    }
                                                                }

                                                            }

                                                        }
                                                    }
                                                    else if (fee.FeeCode.ToString().StartsWith("X"))
                                                    {
                                                        foreach (var serviceCharge in fee.ServiceCharges)
                                                        {
                                                            string serviceChargeCode = serviceCharge.ChargeCode?.ToString();
                                                            double amount = (serviceCharge.Amount != null) ? Convert.ToDouble(serviceCharge.Amount) : 0;
                                                            if (serviceChargeCode != null && isegment == 0)
                                                            {
                                                                if (serviceChargeCode.StartsWith("X") && serviceCharge.ChargeType.ToString() == "ServiceCharge")
                                                                {
                                                                    TotalAmount_Baggage += amount;
                                                                    //TicketCarryBagAMount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Baggage;
                                                                }
                                                                else if (serviceCharge.ChargeType.ToString() == "IncludedTax")
                                                                {
                                                                    TotalAmount_Baggage_tax += Convert.ToDecimal(amount);
                                                                }
                                                                else if (serviceCharge.ChargeType.ToString() == "Discount")
                                                                {
                                                                    TotalAmount_Baggage_discount += Convert.ToDecimal(amount);
                                                                }
                                                            }

                                                        }
                                                    }
                                                    else if (ssrCode.Equals("FFWD"))
                                                    {
                                                        tb_Passengerobj.FastForwardService = 'Y';
                                                    }
                                                    else if (ssrCode.Equals("INFT"))
                                                    {

                                                        foreach (var serviceCharge in fee.ServiceCharges)
                                                        {
                                                            if (fee.FlightReference.ToString().Contains(oridest) == true)
                                                            {
                                                                string serviceChargeCode = serviceCharge.ChargeCode?.ToString();
                                                                double amount = (serviceCharge.Amount != null) ? Convert.ToDouble(serviceCharge.Amount) : 0;
                                                                if (serviceChargeCode != null && isegment == 0)
                                                                {
                                                                    if (serviceCharge.ChargeType.ToString() == "ServiceCharge")
                                                                    {
                                                                        tb_Passengerobj.InftAmount = amount;
                                                                        //TicketCarryBagAMount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Baggage;
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "IncludedTax")
                                                                    {
                                                                        tb_Passengerobj.InftAmount_Tax += Convert.ToDouble(amount);
                                                                    }
                                                                    else if (serviceCharge.ChargeType.ToString() == "Discount")
                                                                    {
                                                                        //TotalAmount_Baggage_discount += Convert.ToDecimal(amount);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                tb_Passengerobj.InftAmount = 0.0;
                                                                tb_Passengerobj.InftAmount_Tax = 0.0;
                                                            }

                                                        }

                                                        tb_Passengerobj.InftAmount -= tb_Passengerobj.InftAmount_Tax;
                                                    }
                                                }

                                                tb_Passengerobj.TotalAmount_Seat = TotalAmount_Seat;
                                                tb_Passengerobj.TotalAmount_Seat_tax = TotalAmount_Seat_tax;
                                                tb_Passengerobj.TotalAmount_Seat_tax_discount = TotalAmount_Seat_discount;
                                                tb_Passengerobj.TotalAmount_Meals = TotalAmount_Meals;
                                                tb_Passengerobj.TotalAmount_Meals_tax = Convert.ToDouble(TotalAmount_Meals_tax);
                                                tb_Passengerobj.TotalAmount_Meals_discount = Convert.ToDouble(TotalAmount_Meals_discount);
                                                tb_Passengerobj.BaggageTotalAmount = TotalAmount_Baggage;
                                                tb_Passengerobj.BaggageTotalAmountTax = TotalAmount_Baggage_tax;
                                                tb_Passengerobj.BaggageTotalAmountTax_discount = TotalAmount_Baggage_discount;
                                                tb_Passengerobj.Carrybages = carryBagesConcatenation.TrimEnd(',');
                                                tb_Passengerobj.MealsCode = MealConcatenation.TrimEnd(',');

                                                tb_PassengerDetailsList.Add(tb_Passengerobj);
                                            }
                                        }



                                        int JourneysCount = _getBookingResponse.Booking.Journeys.Length;
                                        tb_JourneysList = new List<tb_journeys>();
                                        for (int i = 0; i < JourneysCount; i++)
                                        {
                                            tb_journeys tb_JourneysObj = new tb_journeys();
                                            tb_JourneysObj.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                            tb_JourneysObj.JourneyKey = _getBookingResponse.Booking.Journeys[i].JourneySellKey;
                                            tb_JourneysObj.Stops = _getBookingResponse.Booking.Journeys[i].Segments.Length;
                                            tb_JourneysObj.JourneyKeyCount = i;
                                            tb_JourneysObj.FlightType = "";
                                            tb_JourneysObj.Origin = _getBookingResponse.Booking.Journeys[i].Segments[0].DepartureStation;
                                            int len = _getBookingResponse.Booking.Journeys[i].Segments.Length;
                                            tb_JourneysObj.Destination = _getBookingResponse.Booking.Journeys[i].Segments[len - 1].ArrivalStation;
                                            tb_JourneysObj.DepartureDate = Convert.ToDateTime(_getBookingResponse.Booking.Journeys[i].Segments[0].STD);
                                            tb_JourneysObj.ArrivalDate = Convert.ToDateTime(_getBookingResponse.Booking.Journeys[i].Segments[len - 1].STA);
                                            tb_JourneysObj.CreatedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.CreatedDate);
                                            tb_JourneysObj.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                            tb_JourneysObj.ModifiedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.ModifiedDate);
                                            tb_JourneysObj.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                            tb_JourneysObj.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                            tb_JourneysList.Add(tb_JourneysObj);
                                        }
                                        int SegmentReturnCountt = _getBookingResponse.Booking.Journeys[0].Segments.Length;
                                        segmentReturnsListt = new List<tb_Segments>();
                                        for (int j = 0; j < SegmentReturnCountt; j++)
                                        {
                                            tb_Segments segmentReturnobj = new tb_Segments();
                                            segmentReturnobj.BookingID = _getBookingResponse.Booking.BookingID.ToString();
                                            segmentReturnobj.journeyKey = _getBookingResponse.Booking.Journeys[0].JourneySellKey;
                                            segmentReturnobj.SegmentKey = _getBookingResponse.Booking.Journeys[0].Segments[j].SegmentSellKey;
                                            segmentReturnobj.SegmentCount = j;
                                            segmentReturnobj.Origin = _getBookingResponse.Booking.Journeys[0].Segments[j].DepartureStation;
                                            segmentReturnobj.Destination = _getBookingResponse.Booking.Journeys[0].Segments[j].ArrivalStation;

                                            parsedDate = DateTime.ParseExact(_getBookingResponse.Booking.Journeys[0].Segments[j].STA.ToString(), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                            segmentReturnobj.ArrivalDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                            parsedDate = DateTime.ParseExact(_getBookingResponse.Booking.Journeys[0].Segments[j].STD.ToString(), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                            segmentReturnobj.DepartureDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                            segmentReturnobj.Identifier = _getBookingResponse.Booking.Journeys[0].Segments[j].FlightDesignator.FlightNumber;
                                            segmentReturnobj.CarrierCode = _getBookingResponse.Booking.Journeys[0].Segments[j].FlightDesignator.CarrierCode;
                                            segmentReturnobj.Seatnumber = "";
                                            segmentReturnobj.MealCode = "";
                                            segmentReturnobj.MealDiscription = "";
                                            if (!string.IsNullOrEmpty(_getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.DepartureTerminal))
                                            {
                                                //segmentReturnobj.DepartureTerminal = Convert.ToInt32(_getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.DepartureTerminal);
                                                string terminalValue = _getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.DepartureTerminal;
                                                Match match = Regex.Match(terminalValue, @"\d+");
                                                if (match.Success)
                                                {
                                                    segmentReturnobj.DepartureTerminal = Convert.ToInt32(match.Value);
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(_getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.ArrivalTerminal))
                                            {
                                                //segmentReturnobj.ArrivalTerminal = Convert.ToInt32(_getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.ArrivalTerminal);
                                                string terminalValue = _getBookingResponse.Booking.Journeys[0].Segments[j].Legs[0].LegInfo.ArrivalTerminal;
                                                Match match = Regex.Match(terminalValue, @"\d+");
                                                if (match.Success)
                                                {
                                                    segmentReturnobj.ArrivalTerminal = Convert.ToInt32(match.Value);
                                                }
                                            }
                                            segmentReturnobj.CreatedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.CreatedDate);
                                            segmentReturnobj.ModifiedDate = Convert.ToDateTime(_getBookingResponse.Booking.BookingInfo.ModifiedDate);
                                            segmentReturnobj.Createdby = _getBookingResponse.Booking.BookingInfo.CreatedAgentID.ToString();
                                            segmentReturnobj.Modifyby = _getBookingResponse.Booking.BookingInfo.ModifiedAgentID.ToString();
                                            segmentReturnobj.Status = _getBookingResponse.Booking.BookingInfo.BookingStatus.ToString();
                                            segmentReturnsListt.Add(segmentReturnobj);
                                        }


                                        #endregion
                                    }
                                }
                                else
                                {
                                    ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                                    _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);
                                }
                                #endregion
                                //LogOut 
                                IndigoSessionmanager_.LogoutRequest _logoutRequestobj = new IndigoSessionmanager_.LogoutRequest();
                                IndigoSessionmanager_.LogoutResponse _logoutResponse = new IndigoSessionmanager_.LogoutResponse();
                                _logoutRequestobj.ContractVersion = 456;
                                _logoutRequestobj.Signature = token;
                                _getapiIndigo objIndigo = new _getapiIndigo(); ;
                                _logoutResponse = await objIndigo.Logout(_logoutRequestobj);

                                logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_logoutRequestobj) + "\n Response: " + JsonConvert.SerializeObject(_logoutResponse), "Logout", "SpicejetOneWay", "oneway");

                            }
                        }
                        #endregion
                    }
                    else if (flagIndigo == true && (data.Airline[k1].ToLower().Contains("vistara") || data.Airline[k1].ToLower().Contains("airindia") || data.Airline[k1].ToLower().Contains("Hahnair")))
                    {
                        //flagIndigo = false;
                        #region GDS Commit
                        //Spicejet
                        token = string.Empty;

                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "GDS").Result;
                        if (k1 == 0)
                        {
                            tokenview = tokenData.Token;
                        }
                        else
                        {
                            tokenview = tokenData.RToken;
                        }
                        if (!string.IsNullOrEmpty(tokenview))
                        {
                            if (tokenview == null) { tokenview = ""; }
                            string newGuid = token = tokenview.Replace(@"""", string.Empty);
                            //string passengernamedetails = HttpContext.Session.GetString("PassengerNameDetails");
                            string passengernamedetails = objMongoHelper.UnZip(tokenData.PassengerRequest);
                            List<passkeytype> passeengerlist = (List<passkeytype>)JsonConvert.DeserializeObject(passengernamedetails, typeof(List<passkeytype>));
                            //string contactdata = HttpContext.Session.GetString("GDSContactdetails");
                            string contactdata = objMongoHelper.UnZip(tokenData.ContactRequest);
                            ContactModel contactList = (ContactModel)JsonConvert.DeserializeObject(contactdata, typeof(ContactModel));
                            using (HttpClient client1 = new HttpClient())
                            {
                                //_commit objcommit = new _commit();
                                #region GetState
                                #endregion
                                #region Addpayment For Api payment deduction
                                //IndigoBookingManager_.AddPaymentToBookingResponse _BookingPaymentResponse = await objcommit.AddpaymenttoBook(token, Totalpayment);

                                #endregion
                                #region Commit Booking
                                TravelPort _objAvail = null;
                                SearchLog searchLog = new SearchLog();
                                searchLog = _mongoDBHelper.GetFlightSearchLog(Guid).Result;
                                HttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();
                                _objAvail = new TravelPort(httpContextAccessorInstance);
                                string _UniversalRecordURL = AppUrlConstant.GDSUniversalRecordURL;
                                string _testURL = AppUrlConstant.GDSURL;
                                string _targetBranch = string.Empty;
                                string _userName = string.Empty;
                                string _password = string.Empty;
                                //_targetBranch = "P7027135";
                                //_userName = "Universal API/uAPI5098257106-beb65aec";
                                //_password = "Q!f5-d7A3D";

                                //using (HttpClient client = new HttpClient())
                                //{
                                var url = $"{AppUrlConstant.Getsuppliercred}?flightclass={Uri.EscapeDataString("Corporate")}";
                                HttpResponseMessage response = await client.GetAsync(url);
                                //HttpResponseMessage response = await client.GetAsync(AppUrlConstant.Getsuppliercred);

                                if (response.IsSuccessStatusCode)
                                {
                                    var results = await response.Content.ReadAsStringAsync();
                                    var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);

                                    _credentials _CredentialsGDS = new _credentials();
                                    _CredentialsGDS = jsonObject.FirstOrDefault(cred => cred?.supplierid == 5 && cred.Status == 1);

                                    _targetBranch = _CredentialsGDS.organizationId;
                                    _userName = _CredentialsGDS.username;
                                    _password = _CredentialsGDS.password;
                                }
                                // }

                                StringBuilder createPNRReq = new StringBuilder();
                                //string AdultTraveller = HttpContext.Session.GetString("PassengerNameDetails");
                                string AdultTraveller = passengernamedetails;
                                string _data = HttpContext.Session.GetString("SGkeypassengerRT");
                                string _Total = HttpContext.Session.GetString("Total");

                                //retrive PNR
                                string _pricesolution = string.Empty;
                                string Logfolder = string.Empty;

                                if (k1 == 0)
                                {
                                    //Logfolder = "GDSOneWay";
                                    _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_0");
                                }
                                else
                                {
                                    //Logfolder = "GDSRT";
                                    _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_1");
                                }
                                string strAirTicket = string.Empty;
                                string strResponse = string.Empty;
                                string res = string.Empty;
                                string RecordLocator = string.Empty;
                                string _TicketRecordLocator = string.Empty;
                                string serializedUnitKey = HttpContext.Session.GetString("UnitKey");
                                List<string> _unitkey = new List<string>();
                                if (!string.IsNullOrEmpty(serializedUnitKey))
                                {
                                    // Deserialize the JSON string back into a List<string>
                                    _unitkey = JsonConvert.DeserializeObject<List<string>>(serializedUnitKey);
                                }
                                string serializedSSRKey = HttpContext.Session.GetString("ssrKey");
                                List<string> _SSRkey = new List<string>();
                                if (!string.IsNullOrEmpty(serializedSSRKey))
                                {
                                    // Deserialize the JSON string back into a List<string>
                                    _SSRkey = JsonConvert.DeserializeObject<List<string>>(serializedSSRKey);
                                }

                                //res = "";// _objAvail.CreatePNRRoundTrip(_testURL, createPNRReq, newGuid.ToString(), _targetBranch, _userName, _password, AdultTraveller, _data, _Total, Logfolder, k1, _unitkey, _SSRkey, _pricesolution);

                                //RecordLocator = Regex.Match(res, @"universal:UniversalRecord\s*LocatorCode=""(?<LocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["LocatorCode"].Value.Trim();
                                if (k1 == 0 && gDSPNRResponse != null)
                                {
                                    strResponse = gDSPNRResponse.Response; // HttpContext.Session.GetString("PNRL").Split("@@")[0];
                                    RecordLocator = gDSPNRResponse.LocatorCode; // HttpContext.Session.GetString("PNRL").Split("@@")[1];
                                }
                                else if (k1 == 1 && gDSPNRResponse != null)
                                {
                                    strResponse = gDSPNRResponse.RResponse;  //HttpContext.Session.GetString("PNRR").Split("@@")[0];
                                    RecordLocator = gDSPNRResponse.RLocatorCode; // HttpContext.Session.GetString("PNRR").Split("@@")[1];
                                }
                                //string filePath = @"C:\Users\Jet\Downloads\test.xml";

                                //using (StreamReader reader = new StreamReader(filePath))
                                //{
                                //    strResponse = reader.ReadToEnd();
                                //}
                                _TicketRecordLocator = Regex.Match(strResponse, @"AirReservation[\s\S]*?LocatorCode=""(?<LocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["LocatorCode"].Value.Trim();
                                //GetAirTicket

                                strAirTicket = _objAvail.GetTicketdata(_TicketRecordLocator, _testURL, newGuid.ToString(), _targetBranch, _userName, _password, Logfolder);
                                string strTicketno = string.Empty;
                                Hashtable htTicketdata = new Hashtable();
                                foreach (Match mitem in Regex.Matches(strAirTicket, @"BookingTraveler Key=""[\s\S]*?First=""(?<First>[\s\S]*?)""[\s\S]*?Last=""(?<Last>[\s\S]*?)""[\s\S]*?TicketNumber=""(?<TicketNum>[\s\S]*?)""[\s\S]*?Origin=""(?<Origin>[\s\S]*?)""[\s\S]*?Destination=""(?<destination>[\s\S]*?)""[\s\S]*?</air:Ticket>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {
                                    try
                                    {
                                        if (!htTicketdata.Contains(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + mitem.Groups["Origin"].Value.Trim() + "_" + mitem.Groups["destination"].Value.Trim()))
                                        {
                                            htTicketdata.Add(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + mitem.Groups["Origin"].Value.Trim() + "_" + mitem.Groups["destination"].Value.Trim(), mitem.Groups["TicketNum"].Value.Trim());
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                }
                                //getdetails

                                string strResponseretriv = _objAvail.RetrivePnr(RecordLocator, _UniversalRecordURL, newGuid.ToString(), _targetBranch, _userName, _password, Logfolder);

                                //_TicketRecordLocator = Regex.Match(strResponse, @"AirReservation[\s\S]*?LocatorCode=""(?<LocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["LocatorCode"].Value.Trim();



                                GDSResModel.PnrResponseDetails pnrResDetail = new GDSResModel.PnrResponseDetails();
                                if (!string.IsNullOrEmpty(strResponse) && !string.IsNullOrEmpty(RecordLocator))
                                {
                                    TravelPortParsing _objP = new TravelPortParsing();
                                    string stravailibitilityrequest = HttpContext.Session.GetString("GDSAvailibilityRequest");
                                    SimpleAvailabilityRequestModel availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(stravailibitilityrequest);

                                    List<GDSResModel.Segment> getPnrPriceRes = new List<GDSResModel.Segment>();
                                    if (strResponseretriv != null && !strResponseretriv.Contains("Bad Request") && !strResponseretriv.Contains("Internal Server Error"))
                                    {
                                        pnrResDetail = _objP.ParsePNRRsp(strResponseretriv, "oneway", availibiltyRQGDS);
                                    }
                                    if (pnrResDetail != null)
                                    {
                                        Hashtable htname = new Hashtable();
                                        Hashtable htnameempty = new Hashtable();
                                        Hashtable htpax = new Hashtable();
                                        Hashtable htPaxbag = new Hashtable();
                                        Hashtable htpaxdetails = new Hashtable();

                                        Hashtable htsegmentdetails = new Hashtable();
                                        Hashtable htseatdata = new Hashtable();
                                        Hashtable htmealdata = new Hashtable();
                                        Hashtable htbagdata = new Hashtable();

                                        int adultcount = searchLog.Adults;
                                        int childcount = searchLog.Children;
                                        int infantcount = searchLog.Infants;
                                        int TotalCount = adultcount + childcount;
                                        ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();


                                        var resultsTripsell = "";
                                        var JsonObjTripsell = "";
                                        var totalAmount = "";
                                        returnTicketBooking.bookingKey = "";
                                        ReturnPaxSeats _unitdesinator = new ReturnPaxSeats();
                                        _unitdesinator.unitDesignatorPax = "";
                                        //    //GST Number
                                        //    if (_getBookingResponse.Booking.BookingContacts[0].TypeCode == "I")
                                        //    {
                                        //        returnTicketBooking.customerNumber = _getBookingResponse.Booking.BookingContacts[0].CustomerNumber;
                                        //        returnTicketBooking.companyName = _getBookingResponse.Booking.BookingContacts[0].CompanyName;
                                        //    }

                                        Contacts _contact = new Contacts();
                                        _contact.phoneNumbers = "";// _getBookingResponse.Booking.BookingContacts[0].HomePhone.ToString();
                                        if (_unitdesinator.unitDesignatorPax != null)
                                            _contact.ReturnPaxSeats = "";// _unitdesinator.unitDesignatorPax.ToString();
                                        returnTicketBooking.airLines = pnrResDetail.Bonds.Legs[0].FlightName;
                                        returnTicketBooking.recordLocator = pnrResDetail.UniversalRecordLocator;// _getBookingResponse.Booking.RecordLocator;
                                        returnTicketBooking.bookingdate = pnrResDetail.bookingdate;
                                        BarcodePNR = pnrResDetail.UniversalRecordLocator;
                                        if (BarcodePNR != null && BarcodePNR.Length < 7)
                                        {
                                            BarcodePNR = BarcodePNR.PadRight(7);
                                        }
                                        Breakdown breakdown = new Breakdown();
                                        List<JourneyTotals> journeyBaseFareobj = new List<JourneyTotals>();
                                        JourneyTotals journeyTotalsobj = new JourneyTotals();

                                        PassengerTotals passengerTotals = new PassengerTotals();
                                        ReturnSeats returnSeats = new ReturnSeats();
                                        passengerTotals.specialServices = new SpecialServices();
                                        passengerTotals.baggage = new SpecialServices();
                                        var totalTax = "";

                                        #region Itenary segment and legs
                                        int journeyscount = 1;
                                        List<JourneysReturn> AAJourneyList = new List<JourneysReturn>();
                                        for (int i = 0; i < journeyscount; i++)
                                        {

                                            JourneysReturn AAJourneyobj = new JourneysReturn();
                                            AAJourneyobj.journeyKey = "";

                                            int segmentscount = pnrResDetail.Bonds.Legs.Count;
                                            List<SegmentReturn> AASegmentlist = new List<SegmentReturn>();
                                            for (int j = 0; j < segmentscount; j++)
                                            {
                                                returnSeats.unitDesignator = string.Empty;
                                                returnSeats.SSRCode = string.Empty;
                                                DesignatorReturn AADesignatorobj = new DesignatorReturn();
                                                AADesignatorobj.origin = pnrResDetail.Bonds.Legs[j].Origin;
                                                AADesignatorobj.destination = pnrResDetail.Bonds.Legs[j].Destination;
                                                if (!string.IsNullOrEmpty(pnrResDetail.Bonds.Legs[j].DepartureTime))
                                                {
                                                    AADesignatorobj.departure = Convert.ToDateTime(pnrResDetail.Bonds.Legs[j].DepartureTime);
                                                }
                                                if (!string.IsNullOrEmpty(pnrResDetail.Bonds.Legs[j].ArrivalTime))
                                                {
                                                    AADesignatorobj.arrival = Convert.ToDateTime(pnrResDetail.Bonds.Legs[j].ArrivalTime);
                                                }
                                                AAJourneyobj.designator = AADesignatorobj;


                                                SegmentReturn AASegmentobj = new SegmentReturn();
                                                DesignatorReturn AASegmentDesignatorobj = new DesignatorReturn();
                                                AASegmentDesignatorobj.origin = pnrResDetail.Bonds.Legs[j].Origin;
                                                AASegmentDesignatorobj.destination = pnrResDetail.Bonds.Legs[j].Destination;
                                                orides = AASegmentDesignatorobj.origin + AASegmentDesignatorobj.destination;
                                                if (!string.IsNullOrEmpty(pnrResDetail.Bonds.Legs[j].DepartureTime))
                                                {
                                                    AASegmentDesignatorobj.departure = Convert.ToDateTime(pnrResDetail.Bonds.Legs[j].DepartureTime);
                                                }
                                                if (!string.IsNullOrEmpty(pnrResDetail.Bonds.Legs[j].ArrivalTime))
                                                {
                                                    AASegmentDesignatorobj.arrival = Convert.ToDateTime(pnrResDetail.Bonds.Legs[j].ArrivalTime);
                                                }
                                                AASegmentobj.designator = AASegmentDesignatorobj;

                                                int fareCount = 1;// pnrResDetail.PaxFareList.Count;
                                                List<FareReturn> AAFarelist = new List<FareReturn>();
                                                for (int k = 0; k < fareCount; k++)
                                                {
                                                    FareReturn AAFareobj = new FareReturn();
                                                    AAFareobj.fareKey = "";
                                                    //To  do;
                                                    AAFareobj.productClass = "";
                                                    int passengerFarescount = pnrResDetail.PaxFareList.Count; //_getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares.Length;
                                                    List<PassengerFareReturn> PassengerfarelistRT = new List<PassengerFareReturn>();
                                                    double AdtAmount = 0.0;
                                                    double AdttaxAmount = 0.0;
                                                    double chdAmount = 0.0;
                                                    double chdtaxAmount = 0.0;
                                                    double InftAmount = 0.0;
                                                    double infttaxAmount = 0.0;

                                                    if (fareCount > 0)
                                                    {
                                                        for (int l = 0; l < pnrResDetail.PaxFareList.Count; l++)
                                                        {
                                                            journeyTotalsobj = new JourneyTotals();
                                                            PassengerFareReturn AAPassengerfareobject = new PassengerFareReturn();
                                                            GDSResModel.PaxFare currentContact = (GDSResModel.PaxFare)pnrResDetail.PaxFareList[l];
                                                            AAPassengerfareobject.passengerType = currentContact.PaxType.ToString();
                                                            List<ServiceChargeReturn> AAServicechargelist = new List<ServiceChargeReturn>();
                                                            ServiceChargeReturn AAServicechargeobj = new ServiceChargeReturn();
                                                            AAServicechargeobj.amount = Convert.ToInt32(currentContact.BasicFare);
                                                            journeyTotalsobj.totalAmount += Convert.ToInt32(currentContact.BasicFare);
                                                            journeyTotalsobj.totalTax += Convert.ToInt32(currentContact.TotalTax);
                                                            AAServicechargelist.Add(AAServicechargeobj);
                                                            if (AAPassengerfareobject.passengerType.Equals("ADT"))
                                                            {
                                                                AdtAmount += journeyTotalsobj.totalAmount * adultcount;
                                                                AdttaxAmount += journeyTotalsobj.totalTax * adultcount;
                                                            }

                                                            if (AAPassengerfareobject.passengerType.Equals("CHD"))
                                                            {
                                                                chdAmount += journeyTotalsobj.totalAmount * childcount;
                                                                chdtaxAmount += journeyTotalsobj.totalTax * childcount;
                                                            }
                                                            if (AAPassengerfareobject.passengerType.Equals("INF"))
                                                            {
                                                                InftAmount += journeyTotalsobj.totalAmount * infantcount;
                                                                infttaxAmount += journeyTotalsobj.totalTax * infantcount;
                                                            }

                                                            AAPassengerfareobject.serviceCharges = AAServicechargelist;
                                                            PassengerfarelistRT.Add(AAPassengerfareobject);

                                                        }
                                                        journeyTotalsobj.totalAmount = AdtAmount + chdAmount + InftAmount;
                                                        journeyTotalsobj.totalTax = AdttaxAmount + chdtaxAmount + infttaxAmount;
                                                        journeyBaseFareobj.Add(journeyTotalsobj);
                                                        AAFareobj.passengerFares = PassengerfarelistRT;

                                                        AAFarelist.Add(AAFareobj);
                                                    }
                                                }
                                                breakdown.passengerTotals = passengerTotals;
                                                AASegmentobj.fares = AAFarelist;
                                                IdentifierReturn AAIdentifierobj = new IdentifierReturn();

                                                AAIdentifierobj.identifier = pnrResDetail.Bonds.Legs[j].FlightNumber;
                                                AAIdentifierobj.carrierCode = pnrResDetail.Bonds.Legs[j].CarrierCode;

                                                AASegmentobj.identifier = AAIdentifierobj;

                                                //barCode
                                                //julian date
                                                Journeydatetime = DateTime.Parse(AASegmentDesignatorobj.departure.ToString());
                                                carriercode = AAIdentifierobj.carrierCode;
                                                flightnumber = AAIdentifierobj.identifier;
                                                int year = Journeydatetime.Year;
                                                int month = Journeydatetime.Month;
                                                int day = Journeydatetime.Day;
                                                // Calculate the number of days from January 1st to the given date
                                                DateTime currentDate = new DateTime(year, month, day);
                                                DateTime startOfYear = new DateTime(year, 1, 1);
                                                int julianDate = (currentDate - startOfYear).Days + 1;
                                                if (string.IsNullOrEmpty(sequencenumber))
                                                {
                                                    sequencenumber = "00000";
                                                }
                                                else
                                                {
                                                    sequencenumber = sequencenumber.PadRight(5, '0');
                                                }
                                                List<LegReturn> AALeglist = new List<LegReturn>();
                                                LegReturn AALeg = new LegReturn();
                                                DesignatorReturn AAlegDesignatorobj = new DesignatorReturn();
                                                AAlegDesignatorobj.origin = pnrResDetail.Bonds.Legs[j].Origin;
                                                AAlegDesignatorobj.destination = pnrResDetail.Bonds.Legs[j].Destination;
                                                if (!string.IsNullOrEmpty(pnrResDetail.Bonds.Legs[j].DepartureDate))
                                                {
                                                    AAlegDesignatorobj.departure = Convert.ToDateTime(pnrResDetail.Bonds.Legs[j].DepartureDate);
                                                }
                                                if (!string.IsNullOrEmpty(pnrResDetail.Bonds.Legs[j].ArrivalDate))
                                                {
                                                    AAlegDesignatorobj.arrival = Convert.ToDateTime(pnrResDetail.Bonds.Legs[j].ArrivalDate);
                                                }
                                                AALeg.designator = AAlegDesignatorobj;

                                                LegInfoReturn AALeginfoobj = new LegInfoReturn();
                                                AALeginfoobj.arrivalTerminal = pnrResDetail.Bonds.Legs[j].ArrivalTerminal;
                                                AALeginfoobj.arrivalTime = Convert.ToDateTime(pnrResDetail.Bonds.Legs[j].ArrivalDate);
                                                AALeginfoobj.departureTerminal = pnrResDetail.Bonds.Legs[j].DepartureTerminal;
                                                AALeginfoobj.departureTime = Convert.ToDateTime(pnrResDetail.Bonds.Legs[j].DepartureDate);
                                                AALeg.legInfo = AALeginfoobj;
                                                AALeglist.Add(AALeg);


                                                foreach (Match mitem in Regex.Matches(strResponse, @"AirSegment Key=""(?<segmentid>[\s\S]*?)""[\s\S]*?Origin=""(?<origin>[\s\S]*?)""\s*Destination=""(?<Destination>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    try
                                                    {
                                                        if (!htsegmentdetails.Contains(mitem.Groups["segmentid"].Value.Trim()))
                                                        {
                                                            htsegmentdetails.Add(mitem.Groups["segmentid"].Value.Trim(), mitem.Groups["origin"].Value.Trim() + "_" + mitem.Groups["Destination"].Value.Trim());
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }




                                                //Seat
                                                foreach (Match mitem in Regex.Matches(strResponse, @"common_v52_0:BookingTraveler Key=""(?<passengerKey>[\s\S]*?)""[\s\S]*?BookingTravelerName[\s\S]*?First=""(?<First>[\s\S]*?)""\s*Last=""(?<Last>[\s\S]*?)""(?<data>[\s\S]*?)</common_v52_0:BookingTraveler>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    if (mitem.Value.Contains("TravelerType=\"INF\""))
                                                    {
                                                        continue;
                                                    }

                                                    foreach (Match item in Regex.Matches(mitem.Groups["data"].Value, @"AirSeatAssignment Key=""[\s\S]*?Seat=""(?<unitKey>[\s\S]*?)""\s*SegmentRef=""(?<segmentkey>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                    {
                                                        try
                                                        {
                                                            if (!htseatdata.Contains(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString()))
                                                            {
                                                                htseatdata.Add(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString(), item.Groups["unitKey"].Value.Trim());
                                                                returnSeats.unitDesignator += mitem.Groups["passengerKey"].Value.Trim() + "_" + item.Groups["unitKey"].Value.Trim() + ",";
                                                            }
                                                            if (!htpax.Contains(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString()))
                                                            {
                                                                if (carriercode.Length < 3)
                                                                    carriercode = carriercode.PadRight(3);
                                                                if (flightnumber.Length < 5)
                                                                {
                                                                    flightnumber = flightnumber.PadRight(5);
                                                                }
                                                                if (sequencenumber.Length < 5)
                                                                    sequencenumber = sequencenumber.PadRight(5, '0');
                                                                seatnumber = htseatdata[mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString()].ToString();
                                                                if (seatnumber.Length < 4)
                                                                    seatnumber = seatnumber.PadLeft(4, '0');
                                                                BarcodeString = "M" + "1" + mitem.Groups["Last"].Value.Trim() + "/" + mitem.Groups["First"].Value.Trim() + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                                                htpax.Add(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString(), BarcodeString);
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                        if (!htnameempty.Contains(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()]))
                                                        {
                                                            if (carriercode.Length < 3)
                                                                carriercode = carriercode.PadRight(3);
                                                            if (flightnumber.Length < 5)
                                                            {
                                                                flightnumber = flightnumber.PadRight(5);
                                                            }
                                                            if (sequencenumber.Length < 5)
                                                                sequencenumber = sequencenumber.PadRight(5, '0');
                                                            seatnumber = "0000";
                                                            if (seatnumber.Length < 4)
                                                                seatnumber = seatnumber.PadLeft(4, '0');
                                                            BarcodeString = "M" + "1" + mitem.Groups["Last"].Value.Trim() + "/" + mitem.Groups["First"].Value.Trim() + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + "" + sequencenumber + "1" + "00";
                                                            htnameempty.Add(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString(), BarcodeString);
                                                        }

                                                    }

                                                }

                                                //SSR
                                                foreach (Match mitem in Regex.Matches(strResponse, @"common_v52_0:BookingTraveler Key=""(?<passengerKey>[\s\S]*?)""[\s\S]*?BookingTravelerName[\s\S]*?First=""(?<First>[\s\S]*?)""\s*Last=""(?<Last>[\s\S]*?)""(?<data>[\s\S]*?)</common_v52_0:BookingTraveler>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    foreach (Match item in Regex.Matches(mitem.Groups["data"].Value, @"SSR Key=""[\s\S]*?SegmentRef=""(?<segmentkey>[\s\S]*?)""[\s\S]*?Type=""(?<SsrCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                    {
                                                        try
                                                        {
                                                            if (!htmealdata.Contains(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString()))
                                                            {
                                                                if (item.Groups["SsrCode"].Value.Trim() != "INFT" && item.Groups["SsrCode"].Value.Trim() != "FFWD")
                                                                {
                                                                    htmealdata.Add(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString(), item.Groups["SsrCode"].Value.Trim());
                                                                    returnSeats.SSRCode += item.Groups["SsrCode"].Value.Trim() + ",";
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }

                                                    }

                                                }
                                                //To do

                                                foreach (Match item in Regex.Matches(strResponseretriv, @"<air:TicketInfo[\s\S]*?BookingTravelerRef=""(?<paxid>[\s\S]*?)""[\s\S]*?First=""(?<FName>[\s\S]*?)""[\s\S]*?last=""(?<LName>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    if (!htpaxdetails.Contains(item.Groups["paxid"].Value))
                                                    {
                                                        htpaxdetails.Add(item.Groups["paxid"].Value, item.Groups["FName"].Value + "_" + item.Groups["LName"].Value);
                                                    }
                                                }

                                                if (htpaxdetails.Count == 0)
                                                {
                                                    foreach (Match item in Regex.Matches(strResponseretriv, @"BookingTraveler\s*Key=""(?<paxid>[\s\S]*?)""[\s\S]*?First=""(?<FName>[\s\S]*?)""[\s\S]*?last=""(?<LName>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                    {
                                                        if (!htpaxdetails.Contains(item.Groups["paxid"].Value))
                                                        {
                                                            htpaxdetails.Add(item.Groups["paxid"].Value, item.Groups["FName"].Value + "_" + item.Groups["LName"].Value);
                                                        }
                                                    }
                                                }

                                                foreach (Match mitem in Regex.Matches(strResponse, @"common_v52_0:BookingTraveler Key=""(?<passengerKey>[\s\S]*?)""[\s\S]*?BookingTravelerName[\s\S]*?First=""(?<First>[\s\S]*?)""\s*Last=""(?<Last>[\s\S]*?)""(?<data>[\s\S]*?)</common_v52_0:BookingTraveler>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    int segcounter = 0;
                                                    foreach (Match item in Regex.Matches(mitem.Groups["data"].Value, @"SegmentRef=""(?<segmentkey>[\s\S]*?)""[\s\S]*?Type=""XBAG"" FreeText=""TTL(?<BagWeight>[\s\S]*?)KG", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                    {
                                                        try
                                                        {
                                                            if (segcounter == 1) continue;
                                                            if (!htbagdata.Contains(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString()))
                                                            {
                                                                htbagdata.Add(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString(), item.Groups["BagWeight"].Value.Trim());

                                                            }

                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                        segcounter++;
                                                    }
                                                }

                                                //Free seat

                                                foreach (Match mitem in Regex.Matches(strResponse, @"OptionalService Type=""PreReservedSeatAssignment""[\s\S]*?SSRCode=""SEAT""[\s\S]*?ServiceData\s*Data=""(?<Seat>[\s\S]*?)""[\s\S]*?BookingTravelerRef=""(?<Travellerref>[\s\S]*?)""[\s\S]*?AirSegmentRef=""(?<SegmentRef>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    try
                                                    {
                                                        if (!htseatdata.Contains(htpaxdetails[mitem.Groups["Travellerref"].Value.Trim()] + "_" + htsegmentdetails[mitem.Groups["SegmentRef"].Value.Trim()].ToString()))
                                                        {
                                                            htseatdata.Add(htpaxdetails[mitem.Groups["Travellerref"].Value.Trim()] + "_" + htsegmentdetails[mitem.Groups["SegmentRef"].Value.Trim()].ToString(), "0" + mitem.Groups["Seat"].Value.Trim());

                                                        }

                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }

                                                //baggage

                                                foreach (Match mitem in Regex.Matches(strResponse, @"OptionalService Type=""Baggage""[\s\S]*?SSRCode=""XBAG""[\s\S]*?FreeText=""TTL(?<BagWeight>[\s\S]*?)KG[\s\S]*?BookingTravelerRef=""(?<Travellerref>[\s\S]*?)""[\s\S]*?AirSegmentRef=""(?<SegmentRef>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    try
                                                    {
                                                        if (!htbagdata.Contains(htpaxdetails[mitem.Groups["Travellerref"].Value.Trim()] + "_" + htsegmentdetails[mitem.Groups["SegmentRef"].Value.Trim()].ToString()))
                                                        {
                                                            htbagdata.Add(htpaxdetails[mitem.Groups["Travellerref"].Value.Trim()] + "_" + htsegmentdetails[mitem.Groups["SegmentRef"].Value.Trim()].ToString(), mitem.Groups["BagWeight"].Value.Trim());

                                                        }

                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }
                                                AASegmentobj.unitdesignator = returnSeats.unitDesignator;
                                                AASegmentobj.SSRCode = returnSeats.SSRCode;
                                                AASegmentobj.legs = AALeglist;
                                                AASegmentlist.Add(AASegmentobj);
                                                breakdown.journeyfareTotals = journeyBaseFareobj;
                                            }
                                            AAJourneyobj.segments = AASegmentlist;
                                            AAJourneyList.Add(AAJourneyobj);

                                            //}
                                            //baggage

                                            /*foreach (Match mitem in Regex.Matches(strResponse, @"PassengerTypeCode=""(?<PaxType>[\s\S]*?)""[\s\S]*?BaggageAllowance[\s\S]*?MaxWeight Value=""(?<Weight>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                            {
                                                if (!htPaxbag.Contains(mitem.Groups["PaxType"].Value.Trim()))
                                                {
                                                    htPaxbag.Add(mitem.Groups["PaxType"].Value.Trim(), mitem.Groups["Weight"].Value.Trim());
                                                }
                                            }*/

                                            foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""Baggage""\s*TotalPrice=""INR(?<BagPrice>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                            {
                                                passengerTotals.baggage.total += Convert.ToInt32(bagitem.Groups["BagPrice"].Value.Trim());
                                            }

                                            foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""PreReservedSeatAssignment""\s*TotalPrice=""INR(?<SeatPrice>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                            {
                                                returnSeats.total += Convert.ToInt32(bagitem.Groups["SeatPrice"].Value.Trim());
                                            }

                                            #endregion
                                            int passengercount = availibiltyRQGDS.adultcount + availibiltyRQGDS.childcount + availibiltyRQGDS.infantcount;
                                            ReturnPassengers passkeytypeobj = new ReturnPassengers();
                                            List<ReturnPassengers> passkeylist = new List<ReturnPassengers>();
                                            string flightreference = string.Empty;
                                            List<string> barcodeImage = new List<string>();


                                            foreach (var item in pnrResDetail.PaxeDetailList)
                                            {
                                                barcodeImage = new List<string>();
                                                passkeytypeobj = new ReturnPassengers();
                                                passkeytypeobj.name = new Name();
                                                GDSResModel.TravellerDetail currentContact = (GDSResModel.TravellerDetail)item;
                                                passkeytypeobj.barcodestringlst = barcodeImage;
                                                passkeytypeobj.passengerTypeCode = currentContact.PaxType.ToString();
                                                passkeytypeobj.name.first = currentContact.FirstName + " " + currentContact.LastName;
                                                //        //passkeytypeobj.MobNumber = "";
                                                for (int i1 = 0; i1 < passeengerlist.Count; i1++)
                                                {
                                                    if (passkeytypeobj.passengerTypeCode == passeengerlist[i1].passengertypecode && passkeytypeobj.name.first.ToLower() == passeengerlist[i1].first.ToLower() + " " + passeengerlist[i1].last.ToLower())
                                                    {
                                                        passkeytypeobj.MobNumber = passeengerlist[i].mobile;
                                                        passkeytypeobj.passengerKey = passeengerlist[i].passengerkey;
                                                        //                //passkeytypeobj.seats.unitDesignator = htseatdata[passeengerlist[i].passengerkey].ToString();
                                                        break;
                                                    }

                                                }
                                                passkeylist.Add(passkeytypeobj);
                                                returnTicketBooking.passengers = passkeylist;
                                            }

                                            double BasefareAmt = 0.0;
                                            double BasefareTax = 0.0;
                                            for (int i2 = 0; i2 < breakdown.journeyfareTotals.Count; i2++)
                                            {
                                                if (i2 == 1) continue;
                                                BasefareAmt += breakdown.journeyfareTotals[i].totalAmount;
                                                BasefareTax += breakdown.journeyfareTotals[i].totalTax;
                                            }
                                            breakdown.journeyTotals = new JourneyTotals();
                                            breakdown.journeyTotals.totalAmount = Convert.ToDouble(BasefareAmt);
                                            breakdown.passengerTotals.seats = new ReturnSeats();
                                            breakdown.passengerTotals.specialServices.total = passengerTotals.specialServices.total;
                                            breakdown.passengerTotals.baggage.total = passengerTotals.baggage.total;
                                            breakdown.passengerTotals.seats.total = returnSeats.total;
                                            breakdown.passengerTotals.seats.taxes = returnSeats.taxes;
                                            breakdown.journeyTotals.totalTax = Convert.ToDouble(BasefareTax);
                                            breakdown.totalAmount = breakdown.journeyTotals.totalAmount + breakdown.journeyTotals.totalTax;
                                            breakdown.totalToCollect = Convert.ToDouble(breakdown.journeyTotals.totalAmount) + Convert.ToDouble(breakdown.journeyTotals.totalTax);
                                            if (breakdown.passengerTotals.baggage.total != 0)
                                            {
                                                breakdown.totalToCollect += Convert.ToDouble(breakdown.passengerTotals.baggage.total);
                                            }
                                            if (breakdown.passengerTotals.seats.total != 0)
                                            {
                                                breakdown.totalToCollect += Convert.ToDouble(breakdown.passengerTotals.seats.total);
                                            }
                                            returnTicketBooking.breakdown = breakdown;
                                            returnTicketBooking.journeys = AAJourneyList;
                                            returnTicketBooking.passengerscount = passengercount;
                                            returnTicketBooking.contacts = _contact;
                                            returnTicketBooking.Seatdata = htseatdata;
                                            returnTicketBooking.Mealdata = htmealdata;
                                            //returnTicketBooking.Bagdata = htPaxbag;
                                            returnTicketBooking.Bagdata = htbagdata;
                                            returnTicketBooking.htTicketnumber = htTicketdata;
                                            returnTicketBooking.htname = htname;
                                            returnTicketBooking.htnameempty = htnameempty;
                                            returnTicketBooking.htpax = htpax;
                                            returnTicketBooking.TicketNumber = strTicketno;
                                            _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);


                                            #region DB Save
                                            airLineFlightTicketBooking = new AirLineFlightTicketBooking();
                                            string Bookingid = Regex.Match(strResponseretriv, "TransactionId=\"(?<Tid>[\\s\\S]*?)\"").Groups["Tid"].Value.Trim();
                                            airLineFlightTicketBooking.BookingID = Bookingid;
                                            tb_Booking = new tb_Booking();
                                            tb_Booking.AirLineID = 5;
                                            tb_Booking.BookingType = "Corporate-" + Regex.Match(strResponseretriv, "BrandID=\"[\\s\\S]*?Name=\"(?<fareName>[\\s\\S]*?)\"").Groups["fareName"].Value.Trim();
                                            LegalEntity legal = new LegalEntity();
                                            legal = _mongoDBHelper.GetlegalEntityByGUID(Guid).Result;
                                            tb_Booking.CompanyName = legal.BillingEntityFullName;
                                            tb_Booking.BookingRelationId = Guid;
                                            if (k1 == 0)
                                            {
                                                tb_Booking.TripType = "RoundTrip";
                                            }
                                            else
                                            {
                                                tb_Booking.TripType = "ReturnTrip";
                                            }
                                            tb_Booking.BookingID = Bookingid;
                                            tb_Booking.RecordLocator = returnTicketBooking.recordLocator;
                                            tb_Booking.CurrencyCode = "INR";
                                            segmentscount = pnrResDetail.Bonds.Legs.Count;
                                            // AASegmentlist = new List<SegmentReturn>();
                                            for (int j = 0; j < segmentscount; j++)
                                            {
                                                // returnSeats.unitDesignator = string.Empty;
                                                //returnSeats.SSRCode = string.Empty;
                                                //DesignatorReturn AADesignatorobj = new DesignatorReturn();
                                                tb_Booking.Origin = pnrResDetail.Bonds.Legs[j].Origin;
                                                tb_Booking.Destination = pnrResDetail.Bonds.Legs[segmentscount - 1].Destination;
                                                tb_Booking.ArrivalDate = pnrResDetail.Bonds.Legs[j].ArrivalTime;
                                                tb_Booking.DepartureDate = pnrResDetail.Bonds.Legs[j].DepartureTime;
                                                //DateTime parsedDate = DateTime.ParseExact(pnrResDetail.Bonds.Legs[j].ArrivalTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                                DateTime parsedDate = DateTime.ParseExact(pnrResDetail.Bonds.Legs[j].ArrivalTime, "yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                                tb_Booking.ArrivalDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                                //parsedDate = DateTime.ParseExact(pnrResDetail.Bonds.Legs[j].DepartureTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                                parsedDate = DateTime.ParseExact(pnrResDetail.Bonds.Legs[j].DepartureTime, "yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                                tb_Booking.DepartureDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                            }
                                            tb_Booking.BookedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());
                                            tb_Booking.TotalAmount = breakdown.totalToCollect;

                                            Decimal basefareBag = 0.0M;
                                            Decimal taxfareBag = 0.0M;
                                            int basefareSeat = 0;
                                            int taxfareSeat = 0;

                                            foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""Baggage""\s*TotalPrice=""INR(?<BagPrice>[\s\S]*?)""[\s\S]*?BasePrice=""INR(?<BagBasePrice>[\s\S]*?)""\s*Taxes=""INR(?<BagTaxPrice>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                            {
                                                basefareBag += Convert.ToInt32(bagitem.Groups["BagBasePrice"].Value.Trim());
                                                taxfareBag += Convert.ToInt32(bagitem.Groups["BagTaxPrice"].Value.Trim());
                                            }

                                            foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""PreReservedSeatAssignment""\s*TotalPrice=""INR(?<SeatPrice>[\s\S]*?)""[\s\S]*?BasePrice=""INR(?<seatBasePrice>[\s\S]*?)""\s*Taxes=""INR(?<seatTaxPrice>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                            {
                                                basefareSeat += Convert.ToInt32(bagitem.Groups["seatBasePrice"].Value.Trim());
                                                taxfareSeat += Convert.ToInt32(bagitem.Groups["seatTaxPrice"].Value.Trim());
                                            }





                                            if (basefareBag != null)
                                            {
                                                tb_Booking.SpecialServicesTotal = Convert.ToDouble(basefareBag) + Convert.ToDouble(taxfareBag);
                                                if (taxfareBag != null)
                                                {
                                                    tb_Booking.SpecialServicesTotal_Tax = Convert.ToDouble(taxfareBag);
                                                }
                                                tb_Booking.SpecialServicesTotal -= tb_Booking.SpecialServicesTotal_Tax;
                                            }
                                            if (basefareSeat != null)
                                            {
                                                if (basefareSeat > 0 || basefareSeat != 0)
                                                {
                                                    tb_Booking.SeatTotalAmount = basefareSeat + taxfareSeat;
                                                    if (taxfareSeat != null)
                                                    {
                                                        tb_Booking.SeatTotalAmount_Tax = taxfareSeat;
                                                    }
                                                    tb_Booking.SeatTotalAmount -= tb_Booking.SeatTotalAmount_Tax;
                                                    //if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments != null)
                                                    tb_Booking.SeatAdjustment = 0.0;

                                                }
                                            }
                                            tb_Booking.ExpirationDate = DateTime.Now;

                                            //if (JsonObjPNRBooking.data.info.createdDate != null)
                                            tb_Booking.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());
                                            if (HttpContext.User.Identity.IsAuthenticated)
                                            {
                                                var identity = (ClaimsIdentity)User.Identity;
                                                IEnumerable<Claim> claims = identity.Claims;
                                                var userEmail = claims.Where(c => c.Type == ClaimTypes.Email).ToList()[0].Value;
                                                tb_Booking.Createdby = userEmail;// "Online";
                                            }
                                            tb_Booking.ModifiedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim());// DateTime.Now;
                                            tb_Booking.ModifyBy = "";
                                            tb_Booking.BookingDoc = strResponseretriv;
                                            tb_Booking.BookingStatus = "2";// Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();
                                            tb_Booking.PaidStatus = 0;// "0";

                                            // It  will maintained by manually as Airline Code and description 6E-Indigo
                                            tb_Airlines = new tb_Airlines();
                                            tb_Airlines.AirlineID = 5;
                                            tb_Airlines.AirlneName = "1G";// "Boing";
                                            tb_Airlines.AirlineDescription = "AirIndia";
                                            //if (JsonObjPNRBooking.data.info.createdDate != null)
                                            tb_Airlines.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());
                                            tb_Airlines.Createdby = "";
                                            //if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                            tb_Airlines.Modifieddate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim());// DateTime.Now;
                                            tb_Airlines.Modifyby = "";
                                            tb_Airlines.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();

                                            //It  will maintained by manually from Getseatmap Api
                                            tb_AirCraft = new tb_AirCraft();
                                            tb_AirCraft.Id = 1;
                                            tb_AirCraft.AirlineID = 5;
                                            tb_AirCraft.AirCraftName = "";// "Airbus"; to do
                                            tb_AirCraft.AirCraftDescription = " ";// " City Squares Worldwide"; to do
                                                                                  //if (JsonObjPNRBooking.data.info.createdDate != null)
                                            tb_AirCraft.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim()); //DateTime.Now;
                                                                                                                                                                                                                                //if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                            tb_AirCraft.Modifieddate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim());// DateTime.Now;
                                            tb_AirCraft.Createdby = "";
                                            tb_AirCraft.Modifyby = "";
                                            tb_AirCraft.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();


                                            contactDetail = new ContactDetail();
                                            contactDetail.BookingID = Bookingid;
                                            contactDetail.FirstName = contactList.first;
                                            contactDetail.LastName = contactList.last;
                                            contactDetail.EmailID = contactList.emailAddress;
                                            //contactDetail.MobileNumber = Convert.ToInt32(Regex.Replace(JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString(), @"^\+91", "")); // todo
                                            contactDetail.MobileNumber = contactList.number;
                                            contactDetail.CountryCode = contactList.countrycode;
                                            //if (JsonObjPNRBooking.data.info.createdDate != null)
                                            contactDetail.CreateDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());
                                            contactDetail.CreateBy = ""; //"Admin";
                                                                         //if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                            contactDetail.ModifyDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim()); //DateTime.Now;
                                            contactDetail.ModifyBy = "";
                                            contactDetail.Status = 0;


                                            gSTDetails = new GSTDetails();
                                            gSTDetails.bookingReferenceNumber = "";
                                            gSTDetails.GSTEmail = contactList.emailAddressgst;
                                            gSTDetails.GSTNumber = contactList.customerNumber;
                                            gSTDetails.GSTName = contactList.companyName;
                                            gSTDetails.airLinePNR = returnTicketBooking.recordLocator;
                                            gSTDetails.status = 0;


                                            tb_PassengerTotalobj = new tb_PassengerTotal();
                                            tb_PassengerTotalobj.BookingID = Bookingid;
                                            if (breakdown.passengerTotals.specialServices != null)
                                            {
                                                if (breakdown.passengerTotals.specialServices.total != null)
                                                {
                                                    //tb_PassengerTotalobj.TotalMealsAmount = Convert.ToDouble(breakdown.passengerTotals.specialServices.total);
                                                }
                                                if (breakdown.passengerTotals.specialServices.taxes != null)
                                                {
                                                    //tb_PassengerTotalobj.TotalMealsAmount_Tax = Convert.ToDouble(breakdown.passengerTotals.specialServices.taxes);
                                                }
                                            }
                                            if (breakdown.passengerTotals.seats != null)
                                            {
                                                if (breakdown.passengerTotals.seats.total > 0 || breakdown.passengerTotals.seats.total != null)
                                                {
                                                    //tb_PassengerTotalobj.TotalSeatAmount = breakdown.passengerTotals.seats.total;
                                                    //tb_PassengerTotalobj.TotalSeatAmount_Tax = breakdown.passengerTotals.seats.taxes;
                                                    //if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments != null)
                                                    tb_PassengerTotalobj.SeatAdjustment = 0.0;

                                                }
                                            }

                                            tb_PassengerTotalobj.TotalBookingAmount = breakdown.journeyTotals.totalAmount;
                                            tb_PassengerTotalobj.totalBookingAmount_Tax = breakdown.journeyTotals.totalTax;
                                            tb_PassengerTotalobj.Modifyby = "";
                                            tb_PassengerTotalobj.Createdby = ""; //"Online";
                                            tb_PassengerTotalobj.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();
                                            //if (JsonObjPNRBooking.data.info.createdDate != null)
                                            tb_PassengerTotalobj.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());
                                            contactDetail.CreateBy = ""; //"Admin";// DateTime.Now;
                                                                         //if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                            tb_PassengerTotalobj.ModifiedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim()); //DateTime.Now;

                                            tb_PassengerTotalobj.AdultCount = adultcount;
                                            tb_PassengerTotalobj.ChildCount = childcount;
                                            tb_PassengerTotalobj.InfantCount = infantcount;
                                            tb_PassengerTotalobj.TotalPax = adultcount + childcount + infantcount;

                                            tb_PassengerDetailsList = new List<tb_PassengerDetails>();
                                            int SegmentCount = segmentscount;
                                            string passenger = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
                                            //string passenger = objMongoHelper.UnZip(tokenData.PassengerRequest);

                                            List<passkeytype> paxList = (List<passkeytype>)JsonConvert.DeserializeObject(passenger, typeof(List<passkeytype>));
                                            List<passkeytype> infantList = paxList.Where(p => p.passengertypecode == "INFT").ToList();
                                            if (infantList.Count == 0)
                                                infantList = paxList.Where(p => p.passengertypecode == "INF").ToList();

                                            //for frequentFlyer info
                                            Hashtable htpaxFQTVdetails = new Hashtable();
                                            foreach (Match item in Regex.Matches(strResponseretriv, @"FQTV""\s*FreeText=""/AI(?<FQTV>[\s\S]*?)-(?<LastName>[\s\S]*?)/(?<FirstName>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                            {
                                                if (!htpaxFQTVdetails.Contains(item.Groups["FirstName"].Value.ToUpper().Trim() + "_" + item.Groups["LastName"].Value.ToUpper().Trim()))
                                                {
                                                    htpaxFQTVdetails.Add(item.Groups["FirstName"].Value.ToUpper().Trim() + "_" + item.Groups["LastName"].Value.ToUpper().Trim(), item.Groups["FQTV"].Value);
                                                }
                                            }


                                            //To do
                                            htpaxdetails = new Hashtable();
                                            foreach (Match item in Regex.Matches(strResponseretriv, @"<air:TicketInfo[\s\S]*?BookingTravelerRef=""(?<paxid>[\s\S]*?)""[\s\S]*?First=""(?<FName>[\s\S]*?)""[\s\S]*?last=""(?<LName>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                            {
                                                if (!htpaxdetails.Contains(item.Groups["paxid"].Value))
                                                {
                                                    htpaxdetails.Add(item.Groups["paxid"].Value, item.Groups["FName"].Value + "_" + item.Groups["LName"].Value);
                                                }
                                            }

                                            if (htpaxdetails.Count == 0)
                                            {
                                                foreach (Match item in Regex.Matches(strResponseretriv, @"BookingTraveler\s*Key=""(?<paxid>[\s\S]*?)""[\s\S]*?First=""(?<FName>[\s\S]*?)""[\s\S]*?last=""(?<LName>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    if (!htpaxdetails.Contains(item.Groups["paxid"].Value))
                                                    {
                                                        htpaxdetails.Add(item.Groups["paxid"].Value, item.Groups["FName"].Value + "_" + item.Groups["LName"].Value);
                                                    }
                                                }
                                            }

                                            Hashtable htpassenegerdata = new Hashtable();
                                            foreach (Match item in Regex.Matches(strResponseretriv, @"air:AirPricingInfo[\s\S]*?BasePrice=""INR(?<Amount>[\s\S]*?)""[\s\S]*?Taxes=""INR(?<Tax>[\s\S]*?)""[\s\S]*?SegmentRef=""(?<segment>[\s\S]*?)""[\s\S]*?</air:AirPricingInfo>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                            {
                                                foreach (Match itemnew in Regex.Matches(item.Value, @"<air:PassengerType[\s\S]*?BookingTravelerRef=""(?<BookingTraveller>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    htpassenegerdata.Add(htpaxdetails[itemnew.Groups["BookingTraveller"].Value.Trim()] + "_" + item.Groups["segment"].Value, item.Groups["Amount"].Value + "/" + item.Groups["Tax"].Value);
                                                }
                                            }


                                            for (int isegment = 0; isegment < pnrResDetail.Bonds.Legs.Count; isegment++)
                                            {
                                                //foreach (var items in pnrResDetail.PaxeDetailList)
                                                for (int k = 0; k < paxList.Count; k++)
                                                {
                                                    tb_PassengerDetails tb_Passengerobj = new tb_PassengerDetails();
                                                    tb_Passengerobj.BookingID = Bookingid;
                                                    tb_Passengerobj.SegmentsKey = "";
                                                    tb_Passengerobj.PassengerKey = paxList[k].passengerkey;
                                                    tb_Passengerobj.TypeCode = paxList[k].passengertypecode;
                                                    if (tb_Passengerobj.TypeCode == "INF")
                                                        continue;
                                                    tb_Passengerobj.FirstName = paxList[k].first;
                                                    tb_Passengerobj.Title = paxList[k].title;
                                                    tb_Passengerobj.LastName = paxList[k].last;

                                                    //tb_Passengerobj.contact_Emailid = PassengerDataDetailsList.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).Email;
                                                    tb_Passengerobj.contact_Emailid = paxList[k].Email;
                                                    tb_Passengerobj.contact_Mobileno = paxList[k].mobile;
                                                    tb_Passengerobj.FastForwardService = 'N';
                                                    tb_Passengerobj.FrequentFlyerNumber = paxList[k].FrequentFlyer;

                                                    if (tb_Passengerobj.Title.ToUpper() == "MR" || tb_Passengerobj.Title.ToUpper() == "Master" || tb_Passengerobj.Title.ToUpper() == "MSTR")
                                                        tb_Passengerobj.Gender = "Male";
                                                    else if (tb_Passengerobj.Title.ToUpper() == "MS" || tb_Passengerobj.Title.ToUpper() == "MRS" || tb_Passengerobj.Title.ToUpper() == "MISS")
                                                        tb_Passengerobj.Gender = "Female";
                                                    int JourneysReturnCount1 = 1;
                                                    //if (JsonObjPNRBooking.data.journeys[0].segments[isegment].passengerSegment[tb_Passengerobj.PassengerKey].seats != null && JsonObjPNRBooking.data.journeys[0].segments[isegment].passengerSegment[tb_Passengerobj.PassengerKey].seats.Count > 0)
                                                    //{
                                                    //var flightseatnumber1 = JsonObjPNRBooking.data.journeys[0].segments[isegment].passengerSegment[tb_Passengerobj.PassengerKey].seats[0].unitDesignator;
                                                    tb_Passengerobj.Seatnumber = "";
                                                    //}

                                                    string data5 = string.Empty;
                                                    string combinedName = (tb_Passengerobj.FirstName + "_" + tb_Passengerobj.LastName).ToUpper() + "_" + pnrResDetail.Bonds.Legs[isegment].AircraftCode;
                                                    if (htpassenegerdata.Contains(combinedName))
                                                    {
                                                        data5 = htpassenegerdata[combinedName].ToString();
                                                        tb_Passengerobj.TotalAmount = Convert.ToDecimal(data5.Split('/')[0]);
                                                        tb_Passengerobj.TotalAmount_tax = Convert.ToDecimal(data5.Split('/')[1]);
                                                    }
                                                    if (htpaxFQTVdetails.Contains(tb_Passengerobj.FirstName.ToUpper().Trim() + "_" + tb_Passengerobj.LastName.ToUpper().Trim()))
                                                    {
                                                        tb_Passengerobj.FrequentFlyerNumber = htpaxFQTVdetails[tb_Passengerobj.FirstName.ToUpper().Trim() + "_" + tb_Passengerobj.LastName.ToUpper().Trim()].ToString();
                                                    }

                                                    //if (JsonObjPNRBooking.data.info.createdDate != null)
                                                    tb_Passengerobj.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());  //DateTime.Now;
                                                    tb_Passengerobj.Createdby = ""; //"Online";
                                                                                    //if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                                    tb_Passengerobj.ModifiedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim());  //DateTime.Now;
                                                    tb_Passengerobj.ModifyBy = ""; //"Online";
                                                    tb_Passengerobj.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();  //"0";

                                                    if (infantList.Count > 0 && tb_Passengerobj.TypeCode == "ADT" && isegment == 0)
                                                    {
                                                        if (k < infantList.Count)
                                                        {
                                                            for (int inf = 0; inf < infantList.Count; inf++)
                                                            {
                                                                tb_Passengerobj.Inf_TypeCode = "INFT";
                                                                tb_Passengerobj.Inf_Firstname = infantList[k].first;
                                                                tb_Passengerobj.Inf_Lastname = infantList[k].last;
                                                                tb_Passengerobj.Inf_Dob = Convert.ToDateTime(infantList[k].dateOfBirth);
                                                                //if (items.Value.infant.gender == "1")
                                                                //{
                                                                tb_Passengerobj.Inf_Gender = "Master";
                                                                //}
                                                                // for (int i = 0; i < PassengerDataDetailsList.Count; i++)
                                                                //{
                                                                //if (tb_Passengerobj.Inf_TypeCode == PassengerDataDetailsList[i].passengertypecode && tb_Passengerobj.Inf_Firstname.ToLower() == PassengerDataDetailsList[i].first.ToLower() + " " + PassengerDataDetailsList[i].last.ToLower())
                                                                //{
                                                                //tb_Passengerobj.PassengerKey = infantList[k].passengerkey;
                                                                // break;
                                                                //}
                                                                //}
                                                                string combinedkey = (infantList[inf].first + "_" + infantList[inf].last).ToUpper() + "_" + pnrResDetail.Bonds.Legs[isegment].AircraftCode;
                                                                if (htpassenegerdata.Contains(combinedkey))
                                                                {
                                                                    data5 = htpassenegerdata[combinedkey].ToString();
                                                                    tb_Passengerobj.InftAmount = Convert.ToDouble(data5.Split('/')[0]);
                                                                    tb_Passengerobj.InftAmount_Tax = Convert.ToDouble(data5.Split('/')[1]);
                                                                }
                                                            }
                                                        }

                                                    }
                                                    string oridest = pnrResDetail.Bonds.Legs[isegment].Origin + "_" + pnrResDetail.Bonds.Legs[isegment].Destination;
                                                    // Handle carrybages and fees
                                                    List<FeeDetails> feeDetails = new List<FeeDetails>();
                                                    double TotalAmount_Seat = 0;
                                                    decimal TotalAmount_Seat_tax = 0;
                                                    decimal TotalAmount_Seat_discount = 0;
                                                    double TotalAmount_Meals = 0;
                                                    decimal TotalAmount_Meals_tax = 0;
                                                    decimal TotalAmount_Meals_discount = 0;
                                                    double TotalAmount_Baggage = 0;
                                                    decimal TotalAmount_Baggage_tax = 0;
                                                    decimal TotalAmount_Baggage_discount = 0;
                                                    string carryBagesConcatenation = "";
                                                    string MealConcatenation = "";
                                                    string SeatConcatenation = "";

                                                    string hashdata = paxList[k].first.ToString().ToUpper().Replace(" ", "_") + "_" + paxList[k].last.ToString().ToUpper().Replace(" ", "_") + "_" + pnrResDetail.Bonds.Legs[isegment].Origin + "_" + pnrResDetail.Bonds.Legs[isegment].Destination;

                                                    if (htmealdata != null && htmealdata.ContainsKey(hashdata))
                                                    {

                                                        var Mealcode = htmealdata[hashdata].ToString();

                                                        var MealName = MealImageList.GetAllmeal()
                                                        .Where(x => Mealcode.Contains(x.MealCode))
                                                        .Select(x => x.MealImage)
                                                        .FirstOrDefault();


                                                        if (Mealcode != null && MealName != null)
                                                        {

                                                            MealConcatenation += Mealcode + "-" + MealName + ",";
                                                        }
                                                    }

                                                    if (htbagdata != null && htbagdata.ContainsKey(hashdata))
                                                    {

                                                        var bagcode = htbagdata[hashdata].ToString();
                                                        if (bagcode != null && bagcode != null)
                                                        {
                                                            carryBagesConcatenation += "XBAG" + "-" + bagcode + ",";
                                                        }
                                                    }
                                                    if (htseatdata != null && htseatdata.ContainsKey(hashdata))
                                                    {

                                                        var seatcode = htseatdata[hashdata].ToString();
                                                        if (seatcode != null && seatcode != null)
                                                        {
                                                            SeatConcatenation += seatcode + ",";
                                                        }
                                                    }

                                                    //Seat
                                                    if (isegment < 1)
                                                    {
                                                        foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""Baggage""\s*TotalPrice=""INR(?<BagPrice>[\s\S]*?)""[\s\S]*?BasePrice=""INR(?<BagBasePrice>[\s\S]*?)""\s*Taxes=""INR(?<BagTaxPrice>[\s\S]*?)""[\s\S]*?BookingTravelerRef=""(?<Paxid>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                        {
                                                            if (htpaxdetails.Contains(bagitem.Groups["Paxid"].Value.Trim()))
                                                            {
                                                                if (tb_Passengerobj.FirstName.Trim().ToUpper() + "_" + tb_Passengerobj.LastName.Trim().ToUpper() == htpaxdetails[bagitem.Groups["Paxid"].Value.Trim()].ToString())
                                                                {
                                                                    TotalAmount_Baggage = Convert.ToInt32(bagitem.Groups["BagBasePrice"].Value.Trim());
                                                                    TotalAmount_Baggage_tax = Convert.ToInt32(bagitem.Groups["BagTaxPrice"].Value.Trim());
                                                                    break;
                                                                }
                                                            }

                                                        }
                                                    }

                                                    foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""PreReservedSeatAssignment""\s*TotalPrice=""INR(?<SeatPrice>[\s\S]*?)""[\s\S]*?BasePrice=""INR(?<seatBasePrice>[\s\S]*?)""[\s\S]*?Taxes=""INR(?<seatTaxPrice>[\s\S]*?)""[\s\S]*?BookingTravelerRef=""(?<Paxid>[\s\S]*?)""\s*AirSegmentRef=""(?<segid>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                    {
                                                        if (oridest == htsegmentdetails[bagitem.Groups["segid"].Value.Trim()].ToString())
                                                        {
                                                            if (htpaxdetails.Contains(bagitem.Groups["Paxid"].Value.Trim()))
                                                            {
                                                                if (tb_Passengerobj.FirstName.Trim().ToUpper() + "_" + tb_Passengerobj.LastName.Trim().ToUpper() == htpaxdetails[bagitem.Groups["Paxid"].Value.Trim()].ToString())
                                                                {
                                                                    TotalAmount_Seat = Convert.ToInt32(bagitem.Groups["seatBasePrice"].Value.Trim());
                                                                    TotalAmount_Seat_tax = Convert.ToInt32(bagitem.Groups["seatTaxPrice"].Value.Trim());
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    tb_Passengerobj.Seatnumber = SeatConcatenation.TrimEnd(',');
                                                    tb_Passengerobj.TotalAmount_Seat = TotalAmount_Seat + Convert.ToDouble(TotalAmount_Seat_tax);
                                                    tb_Passengerobj.TotalAmount_Seat_tax = TotalAmount_Seat_tax;
                                                    tb_Passengerobj.TotalAmount_Seat_tax_discount = TotalAmount_Seat_discount;
                                                    tb_Passengerobj.TotalAmount_Meals = TotalAmount_Meals;
                                                    tb_Passengerobj.TotalAmount_Meals_tax = Convert.ToDouble(TotalAmount_Meals_tax);
                                                    tb_Passengerobj.TotalAmount_Meals_discount = Convert.ToDouble(TotalAmount_Meals_discount);
                                                    tb_Passengerobj.BaggageTotalAmount = TotalAmount_Baggage + Convert.ToDouble(TotalAmount_Baggage_tax);
                                                    tb_Passengerobj.BaggageTotalAmountTax = TotalAmount_Baggage_tax;
                                                    tb_Passengerobj.BaggageTotalAmountTax_discount = TotalAmount_Baggage_discount;
                                                    tb_Passengerobj.Carrybages = carryBagesConcatenation.TrimEnd(',');
                                                    tb_Passengerobj.MealsCode = MealConcatenation.TrimEnd(',');
                                                    tb_PassengerDetailsList.Add(tb_Passengerobj);

                                                }
                                            }

                                            for (int l = 0; l < tb_PassengerDetailsList.Count; l++)
                                            {
                                                tb_PassengerTotalobj.TotalSeatAmount += tb_PassengerDetailsList[l].TotalAmount_Seat;
                                                tb_PassengerTotalobj.TotalSeatAmount_Tax += Convert.ToDouble(tb_PassengerDetailsList[l].TotalAmount_Seat_tax);
                                                tb_PassengerTotalobj.SpecialServicesAmount += Convert.ToDouble(tb_PassengerDetailsList[l].TotalAmount_Meals);
                                                tb_PassengerTotalobj.SpecialServicesAmount += Convert.ToDouble(tb_PassengerDetailsList[l].BaggageTotalAmount);
                                                tb_PassengerTotalobj.SpecialServicesAmount_Tax += tb_PassengerDetailsList[l].TotalAmount_Meals_tax ?? 0.0;
                                                tb_PassengerTotalobj.SpecialServicesAmount_Tax += Convert.ToDouble(tb_PassengerDetailsList[l].BaggageTotalAmountTax);
                                            }
                                            tb_PassengerTotalobj.TotalSeatAmount -= tb_PassengerTotalobj.TotalSeatAmount_Tax;
                                            tb_PassengerTotalobj.SpecialServicesAmount -= tb_PassengerTotalobj.SpecialServicesAmount_Tax;

                                            int JourneysCount = 1;
                                            tb_JourneysList = new List<tb_journeys>();
                                            segmentReturnsListt = new List<tb_Segments>();
                                            Hashtable seatNumber = new Hashtable();
                                            for (int i1 = 0; i1 < JourneysCount; i1++)
                                            {
                                                tb_journeys tb_JourneysObj = new tb_journeys();
                                                tb_JourneysObj.BookingID = Bookingid;
                                                tb_JourneysObj.JourneyKey = "";
                                                tb_JourneysObj.Stops = segmentscount;
                                                tb_JourneysObj.JourneyKeyCount = i1;
                                                tb_JourneysObj.FlightType = "";
                                                tb_JourneysObj.Origin = tb_Booking.Origin;
                                                tb_JourneysObj.Destination = tb_Booking.Destination;
                                                tb_JourneysObj.DepartureDate = Convert.ToDateTime(tb_Booking.DepartureDate);
                                                tb_JourneysObj.ArrivalDate = Convert.ToDateTime(tb_Booking.ArrivalDate);
                                                //if (JsonObjPNRBooking.data.info.createdDate != null)
                                                tb_JourneysObj.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim()); //DateTime.Now;
                                                tb_JourneysObj.Createdby = ""; //"Online";
                                                                               //if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                                tb_JourneysObj.ModifiedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim()); //DateTime.Now;                                                                                                                                                                                                                                                         //DateTime.Now;
                                                tb_JourneysObj.Modifyby = ""; //"Online";
                                                tb_JourneysObj.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();   //"0";
                                                tb_JourneysList.Add(tb_JourneysObj);
                                                int SegmentReturnCountt = segmentscount;
                                                for (int j = 0; j < SegmentReturnCountt; j++)
                                                {
                                                    tb_Segments segmentReturnobj = new tb_Segments();
                                                    segmentReturnobj.BookingID = Bookingid;
                                                    segmentReturnobj.journeyKey = "";
                                                    segmentReturnobj.SegmentKey = "";
                                                    segmentReturnobj.SegmentCount = j;
                                                    segmentReturnobj.Origin = pnrResDetail.Bonds.Legs[j].Origin;
                                                    segmentReturnobj.Destination = pnrResDetail.Bonds.Legs[j].Destination;
                                                    //segmentReturnobj.DepartureDate = pnrResDetail.Bonds.Legs[j].DepartureTime;
                                                    //segmentReturnobj.ArrivalDate = pnrResDetail.Bonds.Legs[j].ArrivalTime;

                                                    DateTime parsedDate = DateTime.ParseExact(pnrResDetail.Bonds.Legs[j].ArrivalTime, "yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                                    segmentReturnobj.ArrivalDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                                    //parsedDate = DateTime.ParseExact(pnrResDetail.Bonds.Legs[j].DepartureTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                                    parsedDate = DateTime.ParseExact(pnrResDetail.Bonds.Legs[j].DepartureTime, "yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                                    segmentReturnobj.DepartureDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");

                                                    segmentReturnobj.Identifier = pnrResDetail.Bonds.Legs[j].FlightNumber;
                                                    segmentReturnobj.CarrierCode = pnrResDetail.Bonds.Legs[j].CarrierCode;
                                                    segmentReturnobj.Seatnumber = ""; // to do
                                                    segmentReturnobj.MealCode = ""; // to do
                                                    segmentReturnobj.MealDiscription = "";// "it is a coffe"; // to fo
                                                    var LegReturn = pnrResDetail.Bonds.Legs.Count;
                                                    int Legcount = pnrResDetail.Bonds.Legs.Count; //((Newtonsoft.Json.Linq.JContainer)LegReturn).Count;
                                                    List<LegReturn> legReturnsList = new List<LegReturn>();
                                                    for (int n = 0; n < Legcount; n++)
                                                    {
                                                        if (pnrResDetail.Bonds.Legs[j].DepartureTerminal != null)
                                                            segmentReturnobj.DepartureTerminal = Convert.ToInt32(pnrResDetail.Bonds.Legs[j].DepartureTerminal); // to do
                                                        if (pnrResDetail.Bonds.Legs[j].ArrivalTerminal != null)
                                                            segmentReturnobj.ArrivalTerminal = Convert.ToInt32(pnrResDetail.Bonds.Legs[j].ArrivalTerminal);  // to do
                                                    }
                                                    //if (JsonObjPNRBooking.data.info.createdDate != null)
                                                    segmentReturnobj.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim()); //DateTime.Now;
                                                                                                                                                                                                                                             //if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                                    segmentReturnobj.ModifiedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim()); //DateTime.Now;
                                                    segmentReturnobj.Createdby = ""; //"Online";
                                                    segmentReturnobj.Modifyby = ""; //"Online";
                                                    segmentReturnobj.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();
                                                    segmentReturnsListt.Add(segmentReturnobj);

                                                }
                                            }

                                            #endregion
                                            //LogOut 
                                            //IndigoSessionmanager_.LogoutRequest _logoutRequestobj = new IndigoSessionmanager_.LogoutRequest();
                                            //IndigoSessionmanager_.LogoutResponse _logoutResponse = new IndigoSessionmanager_.LogoutResponse();
                                            //_logoutRequestobj.ContractVersion = 456;
                                            //_logoutRequestobj.Signature = token;
                                            //_getapi objIndigo = new _getapi();
                                            //_logoutResponse = await objIndigo.Logout(_logoutRequestobj);

                                            //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_logoutRequestobj) + "\n Response: " + JsonConvert.SerializeObject(_logoutResponse), "Logout", "SpicejetOneWay");

                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                                        _AirLinePNRTicket.ErrorDesc = "";
                                        _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);
                                    }
                                }
                                else
                                {
                                    ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                                    _AirLinePNRTicket.ErrorDesc = "";
                                    _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);
                                }
                            }
                        }
                        #endregion
                    }
                    //Trips tb_Trips = new Trips();
                    //tb_Trips.OutboundFlightID = outboundbookingid;
                    //tb_Trips.TripType = "RoundTrip";
                    //tb_Trips.TripStatus = "active";
                    //tb_Trips.BookingDate = DateTime.Now;
                    //tb_Trips.UserID = "";
                    //tb_Trips.ReturnFlightID = inboundbookingid;

                    airLineFlightTicketBooking.tb_Booking = tb_Booking;
                    airLineFlightTicketBooking.GSTDetails = gSTDetails;
                    airLineFlightTicketBooking.tb_Segments = segmentReturnsListt;
                    airLineFlightTicketBooking.tb_AirCraft = tb_AirCraft;
                    airLineFlightTicketBooking.tb_journeys = tb_JourneysList;
                    airLineFlightTicketBooking.tb_PassengerTotal = tb_PassengerTotalobj;
                    airLineFlightTicketBooking.tb_PassengerDetails = tb_PassengerDetailsList;
                    airLineFlightTicketBooking.ContactDetail = contactDetail;
                    //airLineFlightTicketBooking.tb_Trips = tb_Trips;
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.BaseURL + "api/AirLineTicketBooking/PostairlineTicketData", airLineFlightTicketBooking);
                    if (responsePassengers.IsSuccessStatusCode)
                    {
                        var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                    }


                }
            }

            return View(_AirLinePNRTicket);
        }
        public PointOfSale GetPointOfSale()
        {
            PointOfSale SourcePOS = null;
            try
            {
                SourcePOS = new PointOfSale();
                SourcePOS.State = MessageState.New;
                SourcePOS.OrganizationCode = "APITESTID";
                SourcePOS.AgentCode = "AG";
                SourcePOS.LocationCode = "";
                SourcePOS.DomainCode = "WWW";
            }
            catch (Exception e)
            {
                string exp = e.Message;
                exp = null;
            }

            return SourcePOS;
        }
    }

}
