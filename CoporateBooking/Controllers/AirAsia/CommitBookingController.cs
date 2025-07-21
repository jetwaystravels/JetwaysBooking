using DomainLayer.Model;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using DomainLayer.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System;
using OnionConsumeWebAPI.Extensions;
using OnionConsumeWebAPI.ApiService;
using OnionConsumeWebAPI.Models;
using static DomainLayer.Model.ReturnTicketBooking;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing.Common;
using ZXing;
using ZXing.Windows.Compatibility;
using OnionArchitectureAPI.Services.Barcode;
using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Utility;
using NuGet.Versioning;
using System.Text.RegularExpressions;
using System.Security.Claims;
using CoporateBooking.Models;

namespace OnionConsumeWebAPI.Controllers
{
    public class CommitBookingController : Controller
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
        string flightseatnumber = string.Empty;
        string bookingKey = string.Empty;
        ApiResponseModel responseModel;
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
        private readonly IConfiguration _configuration;
        Logs logs = new Logs();

        public CommitBookingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> booking(string Guid)
        {
            AirLinePNRTicket _AirLinePNRTicket = new AirLinePNRTicket();
            _AirLinePNRTicket.AirlinePNR = new List<ReturnTicketBooking>();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "AirAsia").Result;
            token = tokenData.Token;
            using (HttpClient client = new HttpClient())
            {
                if (tokenData.CommResponse == null)
                {
                    string jsonPayload = string.Empty;
                    string responseContent = string.Empty;
                    _mongoDBHelper.UpdateCommitResponse(Guid, "AirAsia", "1");
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage responceGetBookingSate = await client.GetAsync(AppUrlConstant.URLAirasia + "/api/nsk/v1/booking");
                    if (responceGetBookingSate.IsSuccessStatusCode)
                    {
                        string _responceGetBooking = responceGetBookingSate.Content.ReadAsStringAsync().Result;
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
                        paymentRequest.PaymentFields.ACCTNO = "CRPAPI";
                        paymentRequest.PaymentFields.AMT = Totalpayment;
                        paymentRequest.CurrencyCode = "INR";
                        paymentRequest.Installments = 1;

                        // Serializing the payload to JSON
                        jsonPayload = JsonConvert.SerializeObject(paymentRequest);
                        HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        // Sending the POST request
                        string url = AppUrlConstant.AirasiaPayment;

                        HttpResponseMessage response = await client.PostAsync(url, content);
                        responseContent = await response.Content.ReadAsStringAsync();
                        var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);

                        //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(paymentRequest) + "\nUrl: " + url + "\nResponse: " + responseContent, "CommitPayment", "AirAsiaOneWay");
                        logs.WriteLogs(jsonPayload, "17-AddpaymentRequest", "AirAsiaOneWay", "oneway");
                        logs.WriteLogs(responseContent, "17-AddpaymentResponse", "AirAsiaOneWay", "oneway");
                    }
                    else
                    {
                        logs.WriteLogs(jsonPayload, "17-AddpaymentRequest", "AirAsiaOneWay", "oneway");
                        logs.WriteLogs(responseContent, "17-AddpaymentResponse", "AirAsiaOneWay", "oneway");
                    }
                    #region Commit Booking
                    string[] NotifyContacts = new string[1];
                    NotifyContacts[0] = "P";
                    Commit_BookingModel _Commit_BookingModel = new Commit_BookingModel();

                    _Commit_BookingModel.notifyContacts = true;
                    _Commit_BookingModel.contactTypesToNotify = NotifyContacts;
                    var jsonCommitBookingRequest = JsonConvert.SerializeObject(_Commit_BookingModel, Formatting.Indented);
                    var _responceCommit_Booking = "";
                    ApiRequests apiRequests = new ApiRequests();
                    //   responseModel = await apiRequests.OnPostAsync(AppUrlConstant.AirasiaCommitBooking, _Commit_BookingModel);

                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage responceCommit_Booking = await client.PostAsJsonAsync(AppUrlConstant.AirasiaCommitBooking, _Commit_BookingModel);


                    if (responceCommit_Booking.IsSuccessStatusCode)
                    {
                        _responceCommit_Booking = responceCommit_Booking.Content.ReadAsStringAsync().Result;
                        //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.AirasiaCommitBooking) + "Request:" + jsonCommitBookingRequest + "\n Response: " + _responceCommit_Booking, "CommitBooking", "AirAsiaOneWay", "oneway");
                        logs.WriteLogs(jsonCommitBookingRequest, "18-CommitBookingRequest", "AirAsiaOneWay", "oneway");
                        logs.WriteLogs(_responceCommit_Booking, "18-CommitBookingResponse", "AirAsiaOneWay", "oneway");

                        //var JsonObjCommit_Booking = JsonConvert.DeserializeObject<dynamic>(_responceCommit_Booking);
                    }
                    else
                    {
                        _responceCommit_Booking = responceCommit_Booking.Content.ReadAsStringAsync().Result;
                        logs.WriteLogs(jsonCommitBookingRequest, "18-CommitBookingRequest", "AirAsiaOneWay", "oneway");
                        logs.WriteLogs(_responceCommit_Booking, "18-CommitBookingResponse", "AirAsiaOneWay", "oneway");
                    }
                    var _responceCommit_Booking1 = responceCommit_Booking.Content.ReadAsStringAsync().Result;


                    #endregion
                }
                #region Booking GET
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage responceGetBooking = await client.GetAsync(AppUrlConstant.AirasiaGetBoking);
                if (responceGetBooking.IsSuccessStatusCode)
                {
                    Hashtable htname = new Hashtable();
                    Hashtable htnameempty = new Hashtable();
                    Hashtable htpax = new Hashtable();
                    Hashtable htseatdata = new Hashtable();
                    Hashtable htmealdata = new Hashtable();
                    string sequencenumber = string.Empty;
                    Hashtable htBagdata = new Hashtable();

                    Hashtable TicketSeat = new Hashtable();
                    Hashtable TicketCarryBag = new Hashtable();
                    Hashtable TicketMeal = new Hashtable();
                    Hashtable TicketMealAmount = new Hashtable();
                    Hashtable TicketCarryBagAMount = new Hashtable();
                    DateTime departure;


                    var _responcePNRBooking = responceGetBooking.Content.ReadAsStringAsync().Result;
                    logs.WriteLogs("Request: " + JsonConvert.SerializeObject(AppUrlConstant.AirasiaGetBoking), "18-GetBookingPnrRequest", "AirAsiaOneWay", "oneway");
                    logs.WriteLogs(_responcePNRBooking, "18-GetBookingPnrResponse", "AirAsiaOneWay", "oneway");
                    var JsonObjPNRBooking = JsonConvert.DeserializeObject<dynamic>(_responcePNRBooking);

                    ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                    MongoHelper objMongoHelper = new MongoHelper();
                    string PassengerData = objMongoHelper.UnZip(tokenData.PassengerRequest);
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
                        double totalAmountSum = journeyTotalsobj.totalAmount + infantReturnobj.total;
                        double totaltax = journeyTotalsobj.totalTax + infantReturnobj.taxes;
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
                            returnSeats.adjustments = JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments;
                            returnSeats.totalSeatAmount = returnSeats.total + returnSeats.taxes;
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
                    breakdown.baseTotalAmount = baseTotalAmount;
                    breakdown.ToatalBasePrice = ToatalBasePrice;
                    breakdown.BaseTotalTax = BaseTotalTax;
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
                        ReturnDesignatorobject.origin = JsonObjPNRBooking.data.journeys[0].designator.origin;
                        ReturnDesignatorobject.destination = JsonObjPNRBooking.data.journeys[0].designator.destination;
                        orides = JsonObjPNRBooking.data.journeys[0].designator.origin + JsonObjPNRBooking.data.journeys[0].designator.destination;
                        ReturnDesignatorobject.departure = JsonObjPNRBooking.data.journeys[0].designator.departure;
                        departure = JsonObjPNRBooking.data.journeys[0].designator.departure;
                        ReturnDesignatorobject.arrival = JsonObjPNRBooking.data.journeys[0].designator.arrival;
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
                                    flightseatnumber = item.Value.seats[q].unitDesignator;
                                    if (string.IsNullOrEmpty(flightseatnumber))
                                    {
                                        seatnumber = "0000"; // Set to "0000" if not available
                                    }
                                    else
                                    {
                                        seatnumber = flightseatnumber.PadRight(4, '0'); // Right-pad with zeros if less than 4 characters
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
                                        htmealdata.Add(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, ssrReturn.ssrCode);
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

                        string dateString = JsonObjPNRBooking.data.journeys[0].designator.departure;
                        DateTime date = DateTime.ParseExact(dateString, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        //julian date
                        int year = date.Year;
                        int month = date.Month;
                        int day = date.Day;

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

                    AirLineFlightTicketBooking airLineFlightTicketBooking = new AirLineFlightTicketBooking();
                    airLineFlightTicketBooking.BookingID = JsonObjPNRBooking.data.bookingKey;
                    #region DB Save
                    tb_Booking tb_Booking = new tb_Booking();
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
                    tb_Booking.TripType = "OneWay";
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
                    tb_Booking.BookingStatus = JsonObjPNRBooking.data.info.status;// "2";
                    tb_Booking.PaidStatus = Convert.ToInt32(JsonObjPNRBooking.data.info.paidStatus);// "0";


                    // It  will maintained by manually as Airline Code and description 6E-Indigo
                    tb_Airlines tb_Airlines = new tb_Airlines();
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
                    tb_AirCraft tb_AirCraft = new tb_AirCraft();
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

                    ContactDetail contactDetail = new ContactDetail();
                    contactDetail.BookingID = JsonObjPNRBooking.data.bookingKey;
                    contactDetail.FirstName = JsonObjPNRBooking.data.contacts.P.name.first;
                    contactDetail.LastName = JsonObjPNRBooking.data.contacts.P.name.last;
                    contactDetail.EmailID = JsonObjPNRBooking.data.contacts.P.emailAddress;
                    //contactDetail.MobileNumber = Convert.ToInt32(Regex.Replace(JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString(), @"^\+91", "")); // todo
                    contactDetail.MobileNumber = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[1];
                    contactDetail.CountryCode = JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number.ToString().Split('-')[0];
                    if (JsonObjPNRBooking.data.info.createdDate != null)
                        contactDetail.CreateDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.createdDate); //DateTime.Now;
                    contactDetail.CreateBy = JsonObjPNRBooking.data.info.createdAgentId; //"Admin";
                    if (JsonObjPNRBooking.data.info.modifiedDate != null)
                        contactDetail.ModifyDate = Convert.ToDateTime(JsonObjPNRBooking.data.info.modifiedDate); //DateTime.Now;
                    contactDetail.ModifyBy = JsonObjPNRBooking.data.info.modifiedAgentId; //"Admin";
                    contactDetail.Status = JsonObjPNRBooking.data.info.status;// 0;

                    GSTDetails gSTDetails = new GSTDetails();
                    if (JsonObjPNRBooking.data.contacts.G != null)
                    {
                        gSTDetails.bookingReferenceNumber = JsonObjPNRBooking.data.bookingKey;
                        gSTDetails.GSTEmail = JsonObjPNRBooking.data.contacts.G.emailAddress;
                        gSTDetails.GSTNumber = JsonObjPNRBooking.data.contacts.G.customerNumber;
                        gSTDetails.GSTName = JsonObjPNRBooking.data.contacts.G.companyName;
                        gSTDetails.airLinePNR = JsonObjPNRBooking.data.recordLocator;
                        gSTDetails.status = JsonObjPNRBooking.data.info.status; //0;
                    }

                    tb_PassengerTotal tb_PassengerTotalobj = new tb_PassengerTotal();
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

                    tb_PassengerTotalobj.TotalBookingAmount = JsonObjPNRBooking.data.breakdown.journeyTotals.totalAmount;
                    tb_PassengerTotalobj.totalBookingAmount_Tax = JsonObjPNRBooking.data.breakdown.journeyTotals.totalTax;
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
                                for (int k1 = 0; k1 < JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges.Count; k1++)
                                {
                                    if (JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges[k1].type.ToString() == "0")
                                    {
                                        Amt = Convert.ToDouble(JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges[k1].amount);
                                    }
                                    else
                                    {
                                        tax += Convert.ToDouble(JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].serviceCharges[k1].amount);
                                    }
                                }
                                htPaxAmount.Add(JsonObjPNRBooking.data.journeys[0].segments[ia].designator.origin.ToString() + "_" + JsonObjPNRBooking.data.journeys[0].segments[ia].designator.destination.ToString() + "_" + JsonObjPNRBooking.data.journeys[0].segments[ia].fares[0].passengerFares[k].passengerType.ToString(), Amt + "/" + tax);
                            }
                        }
                    }



                    List<tb_PassengerDetails> tb_PassengerDetailsList = new List<tb_PassengerDetails>();
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
                            tb_Passengerobj.FrequentFlyerNumber = PassengerDataDetailsList.FirstOrDefault(x => x.first == tb_Passengerobj.FirstName && x.last == tb_Passengerobj.LastName).FrequentFlyer;

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
                            if (items.Value.infant != null)
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
                                            TicketCarryBag[tb_Passengerobj.PassengerKey.ToString()] = fee.ssrCode;
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
                                            TicketMeal[tb_Passengerobj.PassengerKey.ToString()] = fee.ssrCode;
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
                                                    TicketSeat[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Seat;
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
                                                    TicketMealAmount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Meals;
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
                                        if (serviceChargeCode != null && isegment==0)
                                        {
                                            if (serviceChargeCode.StartsWith("P") && serviceCharge.type == "6")
                                            {
                                                TotalAmount_Baggage = amount;
                                                TicketCarryBagAMount[tb_Passengerobj.PassengerKey.ToString()] = TotalAmount_Baggage;
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
                    List<tb_journeys> tb_JourneysList = new List<tb_journeys>();
                    List<tb_Segments> segmentReturnsListt = new List<tb_Segments>();
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

                    //Trips tb_Trips = new Trips();
                    //tb_Trips.OutboundFlightID = JsonObjPNRBooking.data.bookingKey;
                    //tb_Trips.TripType = "OneWay";
                    //tb_Trips.TripStatus = "active";
                    //tb_Trips.BookingDate = DateTime.Now;
                    //tb_Trips.UserID = "";
                    //tb_Trips.ReturnFlightID = "";



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
                    #endregion
                }
                #endregion
            }
            return View(_AirLinePNRTicket);

        }
    }
}




