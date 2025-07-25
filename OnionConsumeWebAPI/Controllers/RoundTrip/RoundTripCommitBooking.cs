﻿using Bookingmanager_;
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
                                        returnSeats.totalSeatAmount +=  Convert.ToInt32(returnSeats.adjustments);
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
                                                        orides = parts[3];
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
                                                        }
                                                        if (item2.ChargeCode.StartsWith("E", StringComparison.OrdinalIgnoreCase) == true)
                                                        {
                                                            passengerTotals.baggage.total += Convert.ToInt32(item2.Amount);
                                                        }
                                                        else
                                                        {
                                                            passengerTotals.specialServices.taxes += Convert.ToInt32(item2.Amount);
                                                        }
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
                                                passkeytypeobj.name.first = item.Names[0].FirstName;
                                                passkeytypeobj.name.last = item.Names[0].LastName;
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
                                    }
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
                                                        }
                                                        else if (item2.ChargeCode.StartsWith("X", StringComparison.OrdinalIgnoreCase) == true)
                                                        {
                                                            passengerTotals.baggage.total += Convert.ToInt32(item2.Amount);
                                                        }
                                                        else
                                                        {
                                                            passengerTotals.specialServices.taxes += Convert.ToInt32(item2.Amount);
                                                        }
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
                                                passkeytypeobj.name.first = item.Names[0].FirstName;
                                                passkeytypeobj.name.last = item.Names[0].LastName;
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
                                _targetBranch = "P7027135";
                                _userName = "Universal API/uAPI5098257106-beb65aec";
                                _password = "Q!f5-d7A3D";
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
                                if (k1 == 0)
                                {
                                    strResponse = HttpContext.Session.GetString("PNRL").Split("@@")[0];
                                    RecordLocator = HttpContext.Session.GetString("PNRL").Split("@@")[1];
                                }
                                else
                                {
                                    strResponse = HttpContext.Session.GetString("PNRR").Split("@@")[0];
                                    RecordLocator = HttpContext.Session.GetString("PNRR").Split("@@")[1];
                                }
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

                                                Hashtable htsegmentdetails = new Hashtable();
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
                }
                return View(_AirLinePNRTicket);
            }
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
