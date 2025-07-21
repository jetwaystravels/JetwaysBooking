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
using Utility;

namespace OnionConsumeWebAPI.Controllers.AkasaAir
{
    public class AkasaAirCommitBookingController : Controller
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
        Logs logs = new Logs();
		private readonly IConfiguration _configuration;

		public AkasaAirCommitBookingController(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public async Task<IActionResult> AkasaAirBookingView(string Guid)
        {
            AirLinePNRTicket _AirLinePNRTicket = new AirLinePNRTicket();
            _AirLinePNRTicket.AirlinePNR = new List<ReturnTicketBooking>();
			//string tokenview = HttpContext.Session.GetString("AkasaTokan");
			//if (tokenview == null) { tokenview = ""; }
			//token = tokenview.Replace(@"""", string.Empty);


			MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
			MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

			tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;

			token = tokenData.Token;

            using (HttpClient client = new HttpClient())
            {

                //GetBOoking FRom State
                // STRAT Get INFO
                if (tokenData.CommResponse == null)
                {
                    _mongoDBHelper.UpdateCommitResponse(Guid, "Akasa","1");

                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage responceGetBookingSate = await client.GetAsync(AppUrlConstant.AkasaAirGetBooking);
                    if (responceGetBookingSate.IsSuccessStatusCode)
                    {
                        string _responceGetBooking = responceGetBookingSate.Content.ReadAsStringAsync().Result;
                        var DataBooking = JsonConvert.DeserializeObject<dynamic>(_responceGetBooking);
                        decimal Totalpayment = 0M;
                        if (_responceGetBooking != null)
                        {
                            Totalpayment = DataBooking.data.breakdown.totalAmount;
                        }

                    //Logs logs = new Logs();
                    //logs.WriteLogs("Request: " + JsonConvert.SerializeObject("GetBookingStateRequest") + "Url: " + AppUrlConstant.URLAirasia + "/api/nsk/v1/booking" + "\n Response: " + JsonConvert.SerializeObject(_responceGetBooking), "GetBookingState", "AirAsiaOneWay");

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

                    //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(paymentRequest) + "\nUrl: " + url + "\nResponse: " + responseContent, "CommitPayment", "AirAsiaOneWay");
                    logs.WriteLogs(jsonPayload, "14-AddpaymentRequest", "AkasaOneWay", "oneway");
                    logs.WriteLogs(responseContent, "14-AddpaymentResponse", "AkasaOneWay", "oneway");
                }


                #region Commit Booking
                string[] NotifyContacts = new string[1];
                NotifyContacts[0] = "P";
                Commit_BookingModel _Commit_BookingModel = new Commit_BookingModel();

                _Commit_BookingModel.notifyContacts = true;
                _Commit_BookingModel.contactTypesToNotify = NotifyContacts;
                var jsonCommitBookingRequest = JsonConvert.SerializeObject(_Commit_BookingModel, Formatting.Indented);
                ApiRequests apiRequests = new ApiRequests();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage AkresponceCommit_Booking = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirCommitBooking, _Commit_BookingModel);
                if (AkresponceCommit_Booking.IsSuccessStatusCode)
                {

                        var _responceCommit_Booking = AkresponceCommit_Booking.Content.ReadAsStringAsync().Result;
                        logs.WriteLogs(jsonCommitBookingRequest, "15-CommitBookingRequest", "AkasaOneWay", "oneway");
                        logs.WriteLogs(_responceCommit_Booking, "15-CommitBookingResponse", "AkasaOneWay", "oneway");

                        //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_Commit_BookingModel) + "Url: " + (AppUrlConstant.AkasaAirCommitBooking) + "\n Response: " + JsonConvert.SerializeObject(_responceCommit_Booking), "Commit", "AkasaOneWay", "oneway");
                        //var JsonObjCommit_Booking = JsonConvert.DeserializeObject<dynamic>(_responceCommit_Booking);
                    }

                    HttpContext.Session.SetString("pnr", "123");

                    #endregion
                }

                #region AKBooking GET
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage AKresponceGetBooking = await client.GetAsync(AppUrlConstant.AkasaAirGetBooking);
                if (AKresponceGetBooking.IsSuccessStatusCode)
                {
                    Hashtable htname = new Hashtable();
                    Hashtable htnameempty = new Hashtable();
                    Hashtable htpax = new Hashtable();
                    string sequencenumber = string.Empty;
                    Hashtable htseatdata = new Hashtable();
                    Hashtable htmealdata = new Hashtable();
                    Hashtable htBagdata = new Hashtable();
                    var _responcePNRBooking = AKresponceGetBooking.Content.ReadAsStringAsync().Result;
                    //logs.WriteLogs("Request: " + JsonConvert.SerializeObject("") + "Url: " + (AppUrlConstant.AkasaAirGetBooking) + "\n Response: " + JsonConvert.SerializeObject(_responcePNRBooking), "GetBooking", "AkasaOneWay", "oneway");
                    logs.WriteLogs("Request: " + JsonConvert.SerializeObject(AppUrlConstant.AkasaAirGetBooking), "16-GetBookingPnrRequest", "AkasaOneWay", "oneway");
                    logs.WriteLogs(_responcePNRBooking, "16-GetBookingPnrResponse", "AkasaOneWay", "oneway");

                    var JsonObjPNRBooking = JsonConvert.DeserializeObject<dynamic>(_responcePNRBooking);
                    ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
					//var PassengerData = HttpContext.Session.GetString("AKPassengerName");
					MongoHelper objMongoHelper = new MongoHelper();
					var PassengerData = objMongoHelper.UnZip(tokenData.PassengerRequest);
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
                    // var zxvx= JsonObjPNRBooking.data.breakdown.journeyTotals.totalAmount;
                    Breakdown breakdown = new Breakdown();
                    breakdown.balanceDue = JsonObjPNRBooking.data.breakdown.balanceDue;
                    breakdown.totalAmount = JsonObjPNRBooking.data.breakdown.totalAmount;
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
                                    seatnumber = item.Value.seats[q].unitDesignator;
                                    if (string.IsNullOrEmpty(seatnumber))
                                    {
                                        seatnumber = "0000";
                                    }
                                    else
                                    {
                                        seatnumber = seatnumber.PadRight(4, '0');
                                    }
                                    returnSeatsList.Add(returnSeatsObj);
                                    if (!htseatdata.Contains(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination))
                                    {
                                        htseatdata.Add(passengerSegmentobj.passengerKey.ToString() + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.origin + "_" + JsonObjPNRBooking.data.journeys[i].segments[j].designator.destination, returnSeatsObj.unitDesignator);
                                    }
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
                                    // if (ssrReturn.ssrCode.StartsWith("P") || ssrReturn.ssrCode.StartsWith("X"))
                                    if (!ssrReturn.ssrCode.StartsWith("P"))
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
                        //returnPassengersobj.name.first = items.Value.name.first + " " + items.Value.name.last;
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
                        //if (string.IsNullOrEmpty(sequencenumber))
                        //{
                        //	sequencenumber = "0000";
                        //}
                        //else
                        //{
                        //	sequencenumber = sequencenumber.PadRight(5, '0');
                        //}
                        //string sequencenumber = SequenceGenerator.GetNextSequenceNumber();
                        //BarcodeString = "M" + "1" + items.Value.name.last + "/" + items.Value.name.first + " " + BarcodePNR + "" + orides + carriercode + "" + flightnumber + "" + julianDate + "Y" + seatnumber + " " + sequencenumber + "1" + "00";
                        //BarcodeUtility BarcodeUtility = new BarcodeUtility();
                        //var barcodeImage = BarcodeUtility.BarcodereadUtility(BarcodeString);
                        //returnPassengersobj.barcodestring = barcodeImage;

                        if (items.Value.infant != null)
                        {
                            returnPassengersobj = new ReturnPassengers();
                            returnPassengersobj.name = new Name();
                            returnPassengersobj.passengerTypeCode = "INFT";
                            //returnPassengersobj.name.first = items.Value.infant.name.first + " " + items.Value.infant.name.last;
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
                    _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);



                    AirLineFlightTicketBooking airLineFlightTicketBooking = new AirLineFlightTicketBooking();
                    airLineFlightTicketBooking.BookingID = JsonObjPNRBooking.data.bookingKey;
                    tb_Booking tb_Booking = new tb_Booking();
                    tb_Booking.AirLineID = 1;
                    tb_Booking.BookingID = JsonObjPNRBooking.data.bookingKey;
                    tb_Booking.RecordLocator = JsonObjPNRBooking.data.recordLocator;
                    tb_Booking.CurrencyCode = JsonObjPNRBooking.data.currencyCode;
                    tb_Booking.Origin = JsonObjPNRBooking.data.journeys[0].designator.origin;
                    tb_Booking.Destination = JsonObjPNRBooking.data.journeys[0].designator.destination;
                    tb_Booking.BookedDate = DateTime.Now;//JsonObjPNRBooking.data.journeys[0].designator.departure;                    
                    tb_Booking.TotalAmount = JsonObjPNRBooking.data.breakdown.journeyTotals.totalAmount;
                    tb_Booking.SpecialServicesTotal = (double)1000.00;//(decimal)JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total;
                    tb_Booking.SpecialServicesTotal_Tax = (double)100.0;//JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes;
                    tb_Booking.SeatTotalAmount = (double)2000.00;//JsonObjPNRBooking.data.breakdown.passengerTotalsls.seats.total;
                    tb_Booking.SeatTotalAmount_Tax = (double)200.00;//JsonObjPNRBooking.data.breakdown.passengerTotalsls.seats.taxes;
                    tb_Booking.ExpirationDate = DateTime.Now;//JsonObjPNRBooking.data.hold.expiration;
                    tb_Booking.ArrivalDate = JsonObjPNRBooking.data.journeys[0].designator.arrival;//DateTime.Now;
                    tb_Booking.DepartureDate = JsonObjPNRBooking.data.journeys[0].designator.departure;//DateTime.Now;
                    tb_Booking.CreatedDate = DateTime.Now;
                    tb_Booking.Createdby = "Online";
                    tb_Booking.ModifiedDate = DateTime.Now;
                    tb_Booking.ModifyBy = "Online";
                    tb_Booking.BookingDoc = _responcePNRBooking;
                    tb_Booking.BookingStatus = "0";
                    tb_Airlines tb_Airlines = new tb_Airlines();
                    tb_Airlines.AirlineID = 1;
                    tb_Airlines.AirlneName = "Boing";
                    tb_Airlines.AirlineDescription = "Indra Gandhi airport";
                    tb_Airlines.CreatedDate = DateTime.Now;
                    tb_Airlines.Createdby = "Online";
                    tb_Airlines.Modifieddate = DateTime.Now;
                    tb_Airlines.Modifyby = "Online";
                    tb_Airlines.Status = "0";



                    tb_AirCraft tb_AirCraft = new tb_AirCraft();
                    tb_AirCraft.Id = 1;
                    tb_AirCraft.AirlineID = 1;
                    tb_AirCraft.AirCraftName = "Airbus";
                    tb_AirCraft.AirCraftDescription = " City Squares Worldwide";
                    tb_AirCraft.CreatedDate = DateTime.Now;
                    tb_AirCraft.Modifieddate = DateTime.Now;
                    tb_AirCraft.Createdby = "Online";
                    tb_AirCraft.Modifyby = "Online";
                    tb_AirCraft.Status = "0";

                    ContactDetail contactDetail = new ContactDetail();
                    contactDetail.FirstName = JsonObjPNRBooking.data.contacts.P.name.first;
                    contactDetail.LastName = JsonObjPNRBooking.data.contacts.P.name.last;
                    contactDetail.EmailID = JsonObjPNRBooking.data.contacts.P.emailAddress;
                    contactDetail.MobileNumber = "789456123";//JsonObjPNRBooking.data.contacts.P.phoneNumbers[0].number;
                    contactDetail.CreateDate = DateTime.Now;
                    contactDetail.CreateBy = "Admin";
                    contactDetail.ModifyDate = DateTime.Now;
                    contactDetail.ModifyBy = "Admin";
                    contactDetail.Status = 0;


                    tb_PassengerTotal tb_PassengerTotalobj = new tb_PassengerTotal();
                    bookingKey = JsonObjPNRBooking.data.bookingKey;
                    tb_PassengerTotalobj.BookingID = JsonObjPNRBooking.data.bookingKey;
                    tb_PassengerTotalobj.SpecialServicesAmount = (double)1000.00;//JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.total;
                    tb_PassengerTotalobj.SpecialServicesAmount_Tax = (double)100.00; //JsonObjPNRBooking.data.breakdown.passengerTotals.specialServices.taxes;
                    tb_PassengerTotalobj.TotalSeatAmount = (double)2000.00;//JsonObjPNRBooking.data.breakdown.passengerTotals.seats.total;
                    tb_PassengerTotalobj.TotalSeatAmount_Tax = (double)200.00;//JsonObjPNRBooking.data.breakdown.passengerTotals.seats.taxes;
                    tb_PassengerTotalobj.TotalBookingAmount = (double)1000.00;//JsonObjPNRBooking.data.breakdown.journeyTotals.totalAmount;
                    tb_PassengerTotalobj.totalBookingAmount_Tax = (double)100.00;// JsonObjPNRBooking.data.breakdown.journeyTotals.totalTax;
                    tb_PassengerTotalobj.Modifyby = "Online";
                    tb_PassengerTotalobj.Createdby = "Online";
                    tb_PassengerTotalobj.Status = "0";
                    tb_PassengerTotalobj.CreatedDate = DateTime.Now;
                    tb_PassengerTotalobj.ModifiedDate = DateTime.Now;
                    var passangerCount = JsonObjPNRBooking.data.passengers;
                    int PassengerDataCount = ((Newtonsoft.Json.Linq.JContainer)passangerCount).Count;
                    List<tb_PassengerDetails> tb_PassengerDetailsList = new List<tb_PassengerDetails>();
                    foreach (var items in JsonObjPNRBooking.data.passengers)
                    {
                        tb_PassengerDetails tb_Passengerobj = new tb_PassengerDetails();
                        tb_Passengerobj.BookingID = bookingKey;
                        tb_Passengerobj.PassengerKey = items.Value.passengerKey;
                        tb_Passengerobj.TypeCode = items.Value.passengerTypeCode;
                        tb_Passengerobj.FirstName = items.Value.name.first;
                        tb_Passengerobj.Title = items.Value.name.title;
                        tb_Passengerobj.LastName = items.Value.name.last;
                        tb_Passengerobj.TotalAmount = JsonObjPNRBooking.data.breakdown.journeyTotals.totalAmount;
                        tb_Passengerobj.TotalAmount_tax = JsonObjPNRBooking.data.breakdown.journeyTotals.totalTax;
                        tb_Passengerobj.CreatedDate = DateTime.Now;
                        tb_Passengerobj.Createdby = "Online";
                        tb_Passengerobj.ModifiedDate = DateTime.Now;
                        tb_Passengerobj.ModifyBy = "Online";
                        tb_Passengerobj.Status = "0";
                        tb_PassengerDetailsList.Add(tb_Passengerobj);
                    }
                    int JourneysCount = JsonObjPNRBooking.data.journeys.Count;
                    List<tb_journeys> tb_JourneysList = new List<tb_journeys>();
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
                        tb_JourneysObj.CreatedDate = DateTime.Now;
                        tb_JourneysObj.Createdby = "Online";
                        tb_JourneysObj.ModifiedDate = DateTime.Now;
                        tb_JourneysObj.Modifyby = "Online";
                        tb_JourneysObj.Status = "0";
                        tb_JourneysList.Add(tb_JourneysObj);
                    }
                    int SegmentReturnCountt = JsonObjPNRBooking.data.journeys[0].segments.Count;
                    List<tb_Segments> segmentReturnsListt = new List<tb_Segments>();
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
                        segmentReturnobj.Seatnumber = "2";
                        segmentReturnobj.MealCode = "VScODE";
                        segmentReturnobj.MealDiscription = "it is a coffe";
                        segmentReturnobj.DepartureTerminal = 2;
                        segmentReturnobj.ArrivalTerminal = 1;
                        segmentReturnobj.CreatedDate = DateTime.Now;
                        segmentReturnobj.ModifiedDate = DateTime.Now;
                        segmentReturnobj.Createdby = "Online";
                        segmentReturnobj.Modifyby = "Online";
                        segmentReturnobj.Status = "0";
                        segmentReturnsListt.Add(segmentReturnobj);
                    }

                    airLineFlightTicketBooking.tb_Booking = tb_Booking;
                    airLineFlightTicketBooking.tb_Segments = segmentReturnsListt;
                    airLineFlightTicketBooking.tb_AirCraft = tb_AirCraft;
                    airLineFlightTicketBooking.tb_journeys = tb_JourneysList;
                    airLineFlightTicketBooking.tb_PassengerTotal = tb_PassengerTotalobj;
                    airLineFlightTicketBooking.tb_PassengerDetails = tb_PassengerDetailsList;
                    airLineFlightTicketBooking.ContactDetail = contactDetail;
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.BaseURL + "api/AirLineTicketBooking/PostairlineTicketData", airLineFlightTicketBooking);
                    if (responsePassengers.IsSuccessStatusCode)
                    {
                        var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                    }
                }
                #endregion
            }
            return View(_AirLinePNRTicket);
        }
    }
}
