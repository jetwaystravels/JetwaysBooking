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
using Bookingmanager_;
using Utility;
using Sessionmanager;
using OnionArchitectureAPI.Services.Barcode;
using OnionArchitectureAPI.Services.Indigo;
using static DomainLayer.Model.ReturnTicketBooking;
using IndigoBookingManager_;
using IndigoSessionmanager_;
using OnionConsumeWebAPI.Extensions;
using System.Collections;
using static DomainLayer.Model.SeatMapResponceModel;
using static DomainLayer.Model.ReturnAirLineTicketBooking;
using Indigo;
using OnionArchitectureAPI.Services.Travelport;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using OnionConsumeWebAPI.Models;
using SpicejetBookingManager_;
using System.Drawing.Drawing2D;
using Humanizer;
using System.Globalization;
using System.Security.Claims;
using CoporateBooking.Models;

namespace OnionConsumeWebAPI.Controllers.TravelClick
{
    public class GDSCommitBookingController : Controller
    {
        Logs logs = new Logs();
        string BaseURL = "https://dotrezapi.test.I5.navitaire.com";
        string token = string.Empty;
        String BarcodePNR = string.Empty;
        string ssrKey = string.Empty;
        string journeyKey = string.Empty;
        string uniquekey = string.Empty;
        string BarcodeString = string.Empty;
        string BarcodeInfantString = string.Empty;
        string orides = string.Empty;
        string carriercode = string.Empty;
        string flightnumber = string.Empty;
        string seatnumber = string.Empty;
        string sequencenumber = string.Empty;
        decimal TotalMeal = 0;
        decimal TotalBag = 0;
        decimal TotalBagtax = 0;
        decimal Totatamountmb = 0;
        DateTime Journeydatetime = new DateTime();
        string bookingKey = string.Empty;
        private readonly IConfiguration _configuration;
        public GDSCommitBookingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> booking(string Guid)
        {
            AirLinePNRTicket _AirLinePNRTicket = new AirLinePNRTicket();
            _AirLinePNRTicket.AirlinePNR = new List<ReturnTicketBooking>();

            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
            SearchLog searchLog = new SearchLog();
            searchLog = _mongoDBHelper.GetFlightSearchLog(Guid).Result;
            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "GDS").Result;
            string _pricesolution = string.Empty;
            _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_0");
            string newGuid = tokenData.Token;
            if (newGuid == "" || newGuid == null)
            {
                return RedirectToAction("Index");
            }
            if (!string.IsNullOrEmpty(newGuid))
            {
                string passengernamedetails = objMongoHelper.UnZip(tokenData.PassengerRequest);
                List<passkeytype> passeengerlist = (List<passkeytype>)JsonConvert.DeserializeObject(passengernamedetails, typeof(List<passkeytype>));
                string contactdata = objMongoHelper.UnZip(tokenData.ContactRequest);
                ContactModel contactList = (ContactModel)JsonConvert.DeserializeObject(contactdata, typeof(ContactModel));
                using (HttpClient client1 = new HttpClient())
                {

                    #region Commit Booking
                    TravelPort _objAvail = null;
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

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);
                        HttpResponseMessage response = await client.GetAsync(AppUrlConstant.Getsuppliercred);

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
                    }


                    StringBuilder createPNRReq = new StringBuilder();
                    string AdultTraveller = passengernamedetails;

                    GDSPNRResponse gDSPNRResponse = await _mongoDBHelper.GetGDSPNRByGUID(Guid);

                    string strResponse = gDSPNRResponse.Response; //HttpContext.Session.GetString("PNR").Split("@@")[0];
                    string _TicketRecordLocator = Regex.Match(strResponse, @"AirReservation[\s\S]*?LocatorCode=""(?<LocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["LocatorCode"].Value.Trim();
                    //GetAirTicket
                    string strAirTicket = _objAvail.GetTicketdata(_TicketRecordLocator, _testURL, newGuid.ToString(), _targetBranch, _userName, _password, "GDSOneWay");
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
                    string RecordLocator = gDSPNRResponse.LocatorCode;// HttpContext.Session.GetString("PNR").Split("@@")[1];
                    string strResponseretriv = _objAvail.RetrivePnr(RecordLocator, _UniversalRecordURL, newGuid.ToString(), _targetBranch, _userName, _password, "GDSOneWay");
                    GDSResModel.PnrResponseDetails pnrResDetail = new GDSResModel.PnrResponseDetails();
                    if (!string.IsNullOrEmpty(strResponse) && !string.IsNullOrEmpty(RecordLocator))
                    {
                        TravelPortParsing _objP = new TravelPortParsing();
                        string stravailibitilityrequest = HttpContext.Session.GetString("GDSAvailibilityRequest");
                        SimpleAvailabilityRequestModel availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(stravailibitilityrequest);
                        List<GDSResModel.Segment> getPnrPriceRes = new List<GDSResModel.Segment>();
                        if (strResponseretriv != null && !strResponseretriv.Contains("Bad Request") && !strResponseretriv.Contains("Internal Server Error"))
                        {
                            pnrResDetail = _objP.ParsePNRRsp(strResponseretriv, "OneWay", availibiltyRQGDS);
                        }
                        if (pnrResDetail != null)
                        {
                            Hashtable htname = new Hashtable();
                            Hashtable htnameempty = new Hashtable();
                            Hashtable htpax = new Hashtable();
                            Hashtable htPaxbag = new Hashtable();
                            Hashtable htpaxdetails = new Hashtable();

                            Hashtable htseatdata = new Hashtable();
                            Hashtable htmealdata = new Hashtable();
                            Hashtable htbagdata = new Hashtable();
                            Hashtable htsegmentdetails = new Hashtable();
                            int adultcount = searchLog.Adults;
                            int childcount = searchLog.Children;
                            int infantcount = searchLog.Infants;
                            int TotalCount = adultcount + childcount;
                            ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();
                            returnTicketBooking.bookingKey = "";
                            ReturnPaxSeats _unitdesinator = new ReturnPaxSeats();
                            _unitdesinator.unitDesignatorPax = "";
                            Contacts _contact = new Contacts();
                            _contact.phoneNumbers = "";
                            if (_unitdesinator.unitDesignatorPax != null)
                                _contact.ReturnPaxSeats = "";
                            returnTicketBooking.airLines = pnrResDetail.Bonds.Legs[0].FlightName;
                            returnTicketBooking.recordLocator = pnrResDetail.UniversalRecordLocator;
                            returnTicketBooking.bookingdate = pnrResDetail.bookingdate;
                            BarcodePNR = pnrResDetail.UniversalRecordLocator;
                            if (BarcodePNR.Length < 7)
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

                                    int fareCount = 1;
                                    List<FareReturn> AAFarelist = new List<FareReturn>();
                                    for (int k = 0; k < fareCount; k++)
                                    {
                                        FareReturn AAFareobj = new FareReturn();
                                        AAFareobj.fareKey = "";
                                        AAFareobj.productClass = "";
                                        int passengerFarescount = pnrResDetail.PaxFareList.Count;
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
                                        if (mitem.Value.Contains("TravelerType=\"INF\""))
                                        {
                                            continue;
                                        }
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
                                    AASegmentobj.unitdesignator = returnSeats.unitDesignator;
                                    AASegmentobj.SSRCode = returnSeats.SSRCode;
                                    AASegmentobj.legs = AALeglist;
                                    AASegmentlist.Add(AASegmentobj);
                                    breakdown.journeyfareTotals = journeyBaseFareobj;
                                }
                                AAJourneyobj.segments = AASegmentlist;
                                AAJourneyList.Add(AAJourneyobj);

                                #endregion


                                foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""Baggage""\s*TotalPrice=""INR(?<BagPrice>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {
                                    passengerTotals.baggage.total += Convert.ToInt32(bagitem.Groups["BagPrice"].Value.Trim());
                                }

                                foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""PreReservedSeatAssignment""\s*TotalPrice=""INR(?<SeatPrice>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {
                                    returnSeats.total += Convert.ToInt32(bagitem.Groups["SeatPrice"].Value.Trim());
                                }

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
                                    for (int i1 = 0; i1 < passeengerlist.Count; i1++)
                                    {
                                        if (passkeytypeobj.passengerTypeCode == passeengerlist[i1].passengertypecode && passkeytypeobj.name.first.ToLower() == passeengerlist[i1].first.ToLower() + " " + passeengerlist[i1].last.ToLower())
                                        {
                                            passkeytypeobj.MobNumber = passeengerlist[i].mobile;
                                            passkeytypeobj.passengerKey = passeengerlist[i].passengerkey;
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
                                    if (i2 == 0)
                                    {
                                        BasefareAmt += breakdown.journeyfareTotals[i].totalAmount;
                                        BasefareTax += breakdown.journeyfareTotals[i].totalTax;
                                    }
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
                                breakdown.totalToCollect = Convert.ToDouble(breakdown.journeyfareTotals[0].totalAmount) + Convert.ToDouble(breakdown.journeyfareTotals[0].totalTax);
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
                                returnTicketBooking.Bagdata = htbagdata;
                                returnTicketBooking.htname = htname;
                                returnTicketBooking.htTicketnumber = htTicketdata;
                                returnTicketBooking.htnameempty = htnameempty;
                                returnTicketBooking.htpax = htpax;
                                returnTicketBooking.TicketNumber = strTicketno;
                                _AirLinePNRTicket.AirlinePNR.Add(returnTicketBooking);


                                #region DB Save
                                AirLineFlightTicketBooking airLineFlightTicketBooking = new AirLineFlightTicketBooking();
                                string Bookingid = Regex.Match(strResponseretriv, "TransactionId=\"(?<Tid>[\\s\\S]*?)\"").Groups["Tid"].Value.Trim();
                                airLineFlightTicketBooking.BookingID = Bookingid;
                                tb_Booking tb_Booking = new tb_Booking();
                                tb_Booking.AirLineID = 5;
                                tb_Booking.BookingType = "Corporate-" + Regex.Match(strResponseretriv, "BrandID=\"[\\s\\S]*?Name=\"(?<fareName>[\\s\\S]*?)\"").Groups["fareName"].Value.Trim();
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
                                tb_Booking.TripType = "OneWay";
                                tb_Booking.BookingRelationId = Guid;
                                tb_Booking.BookingID = Bookingid;
                                tb_Booking.RecordLocator = returnTicketBooking.recordLocator;
                                tb_Booking.CurrencyCode = "INR";
                                segmentscount = pnrResDetail.Bonds.Legs.Count;
                                for (int j = 0; j < segmentscount; j++)
                                {
                                    tb_Booking.Origin = pnrResDetail.Bonds.Legs[j].Origin;
                                    tb_Booking.Destination = pnrResDetail.Bonds.Legs[segmentscount - 1].Destination;
                                    tb_Booking.ArrivalDate = pnrResDetail.Bonds.Legs[j].ArrivalTime;
                                    tb_Booking.DepartureDate = pnrResDetail.Bonds.Legs[j].DepartureTime;
                                    DateTime parsedDate = DateTime.ParseExact(pnrResDetail.Bonds.Legs[j].ArrivalTime, "yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                    tb_Booking.ArrivalDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
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
                                        tb_Booking.SeatAdjustment = 0.0;
                                    }
                                    tb_Booking.SeatTotalAmount -= tb_Booking.SeatTotalAmount_Tax;
                                }
                                tb_Booking.ExpirationDate = DateTime.Now;
                                tb_Booking.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());
                                if (HttpContext.User.Identity.IsAuthenticated)
                                {
                                    var identity = (ClaimsIdentity)User.Identity;
                                    IEnumerable<Claim> claims = identity.Claims;
                                    var userEmail = claims.Where(c => c.Type == ClaimTypes.Email).ToList()[0].Value;
                                    tb_Booking.Createdby = userEmail;
                                }
                                tb_Booking.ModifiedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim());// DateTime.Now;
                                tb_Booking.ModifyBy = "";
                                tb_Booking.BookingDoc = strResponseretriv;
                                tb_Booking.BookingStatus = "2";
                                tb_Booking.PaidStatus = 0;

                                // It  will maintained by manually as Airline Code and description 6E-Indigo
                                tb_Airlines tb_Airlines = new tb_Airlines();
                                tb_Airlines.AirlineID = 5;
                                tb_Airlines.AirlneName = "1G";// "Boing";
                                tb_Airlines.AirlineDescription = "AirIndia";
                                tb_Airlines.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());
                                tb_Airlines.Createdby = "";
                                tb_Airlines.Modifieddate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim());// DateTime.Now;
                                tb_Airlines.Modifyby = "";
                                tb_Airlines.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();

                                //It  will maintained by manually from Getseatmap Api
                                tb_AirCraft tb_AirCraft = new tb_AirCraft();
                                tb_AirCraft.Id = 5;
                                tb_AirCraft.AirlineID = 5;
                                tb_AirCraft.AirCraftName = "";// "Airbus"; to do
                                tb_AirCraft.AirCraftDescription = " ";// " City Squares Worldwide"; to do
                                tb_AirCraft.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim()); //DateTime.Now;
                                tb_AirCraft.Modifieddate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim());// DateTime.Now;
                                tb_AirCraft.Createdby = "";
                                tb_AirCraft.Modifyby = "";
                                tb_AirCraft.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();


                                ContactDetail contactDetail = new ContactDetail();
                                contactDetail.BookingID = Bookingid;
                                contactDetail.FirstName = contactList.first;
                                contactDetail.LastName = contactList.last;
                                contactDetail.EmailID = contactList.emailAddress;
                                contactDetail.MobileNumber = contactList.number;
                                contactDetail.CountryCode = contactList.countrycode;
                                contactDetail.CreateDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());
                                contactDetail.CreateBy = ""; //"Admin";
                                contactDetail.ModifyDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim()); //DateTime.Now;
                                contactDetail.ModifyBy = "";
                                contactDetail.Status = 0;


                                GSTDetails gSTDetails = new GSTDetails();
                                gSTDetails.bookingReferenceNumber = "";
                                gSTDetails.GSTEmail = contactList.emailAddressgst;
                                gSTDetails.GSTNumber = contactList.customerNumber;
                                gSTDetails.GSTName = contactList.companyName;
                                gSTDetails.airLinePNR = returnTicketBooking.recordLocator;
                                gSTDetails.status = 0;


                                tb_PassengerTotal tb_PassengerTotalobj = new tb_PassengerTotal();
                                tb_PassengerTotalobj.BookingID = Bookingid;
                                if (breakdown.passengerTotals.seats != null)
                                {
                                    if (breakdown.passengerTotals.seats.total > 0 || breakdown.passengerTotals.seats.total != null)
                                    {
                                        tb_PassengerTotalobj.SeatAdjustment = 0.0;
                                    }
                                }

                                tb_PassengerTotalobj.TotalBookingAmount = breakdown.journeyTotals.totalAmount;
                                tb_PassengerTotalobj.totalBookingAmount_Tax = breakdown.journeyTotals.totalTax;
                                tb_PassengerTotalobj.Modifyby = "";
                                tb_PassengerTotalobj.Createdby = ""; //"Online";
                                tb_PassengerTotalobj.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();
                                tb_PassengerTotalobj.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());
                                contactDetail.CreateBy = ""; //"Admin";// DateTime.Now;
                                tb_PassengerTotalobj.ModifiedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim()); //DateTime.Now;

                                tb_PassengerTotalobj.AdultCount = adultcount;
                                tb_PassengerTotalobj.ChildCount = childcount;
                                tb_PassengerTotalobj.InfantCount = infantcount;
                                tb_PassengerTotalobj.TotalPax = adultcount + childcount + infantcount;

                                List<tb_PassengerDetails> tb_PassengerDetailsList = new List<tb_PassengerDetails>();
                                int SegmentCount = segmentscount;
                                string passenger = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
                                List<passkeytype> paxList = (List<passkeytype>)JsonConvert.DeserializeObject(passenger, typeof(List<passkeytype>));
                                List<passkeytype> infantList = paxList.Where(p => p.passengertypecode == "INF").ToList();

                                //for frequentFlyer info
                                Hashtable htpaxFQTVdetails = new Hashtable();
                                foreach (Match item in Regex.Matches(strResponseretriv, @"FQTV""\s*FreeText=""/AI(?<FQTV>[\s\S]*?)-(?<LastName>[\s\S]*?)/(?<FirstName>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {
                                    if (!htpaxFQTVdetails.Contains(item.Groups["FQTV"].Value.ToUpper().Trim()))
                                    {
                                        htpaxFQTVdetails.Add(item.Groups["FirstName"].Value.ToUpper().Trim() + "_" + item.Groups["LastName"].Value.ToUpper().Trim(), item.Groups["FQTV"].Value);
                                    }
                                }
                                Hashtable htpassenegerdata = new Hashtable();
                                foreach (Match item in Regex.Matches(strResponseretriv, @"air:AirPricingInfo[\s\S]*?BasePrice=""INR(?<Amount>[\s\S]*?)""[\s\S]*?Taxes=""INR(?<Tax>[\s\S]*?)""[\s\S]*?SegmentRef=""(?<segment>[\s\S]*?)""[\s\S]*?</air:AirPricingInfo>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {
                                    foreach (Match itemnew in Regex.Matches(item.Value, @"<air:PassengerType[\s\S]*?BookingTravelerRef=""(?<BookingTraveller>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                    {
                                        if (!htpassenegerdata.Contains(htpaxdetails[itemnew.Groups["BookingTraveller"].Value.Trim()] + "_" + item.Groups["segment"].Value))
                                        {
                                            htpassenegerdata.Add(htpaxdetails[itemnew.Groups["BookingTraveller"].Value.Trim()] + "_" + item.Groups["segment"].Value, item.Groups["Amount"].Value + "/" + item.Groups["Tax"].Value);
                                        }
                                    }
                                }


                                for (int isegment = 0; isegment < pnrResDetail.Bonds.Legs.Count; isegment++)
                                {
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

                                        tb_Passengerobj.contact_Emailid = paxList[k].Email;
                                        tb_Passengerobj.contact_Mobileno = paxList[k].mobile;
                                        tb_Passengerobj.FastForwardService = 'N';
                                        tb_Passengerobj.FrequentFlyerNumber = paxList[k].FrequentFlyer;

                                        if (tb_Passengerobj.Title.ToUpper() == "MR" || tb_Passengerobj.Title.ToUpper() == "Master" || tb_Passengerobj.Title.ToUpper() == "MSTR")
                                            tb_Passengerobj.Gender = "Male";
                                        else if (tb_Passengerobj.Title.ToUpper() == "MS" || tb_Passengerobj.Title.ToUpper() == "MRS" || tb_Passengerobj.Title.ToUpper() == "MISS")
                                            tb_Passengerobj.Gender = "Female";
                                        int JourneysReturnCount1 = 1;
                                        tb_Passengerobj.Seatnumber = "";
                                        string combinedName = (tb_Passengerobj.FirstName + "_" + tb_Passengerobj.LastName).ToUpper() + "_" + pnrResDetail.Bonds.Legs[isegment].AircraftCode;
                                        string data = string.Empty;
                                        if (htpassenegerdata.Contains(combinedName))
                                        {
                                            data = htpassenegerdata[combinedName].ToString();
                                            tb_Passengerobj.TotalAmount = Convert.ToDecimal(data.Split('/')[0]);
                                            tb_Passengerobj.TotalAmount_tax = Convert.ToDecimal(data.Split('/')[1]);
                                        }
                                        if (htpaxFQTVdetails.Contains(tb_Passengerobj.FirstName.ToUpper().Trim() + "_" + tb_Passengerobj.LastName.ToUpper().Trim()))
                                        {
                                            tb_Passengerobj.FrequentFlyerNumber = htpaxFQTVdetails[tb_Passengerobj.FirstName.ToUpper().Trim() + "_" + tb_Passengerobj.LastName.ToUpper().Trim()].ToString();
                                        }
                                        tb_Passengerobj.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());  //DateTime.Now;
                                        tb_Passengerobj.Createdby = ""; //"Online";
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
                                                    tb_Passengerobj.Inf_Gender = "Master";
                                                    string combinedkey = (infantList[inf].first + "_" + infantList[inf].last).ToUpper() + "_" + pnrResDetail.Bonds.Legs[isegment].AircraftCode;
                                                    if (htpassenegerdata.Contains(combinedkey))
                                                    {
                                                        data = htpassenegerdata[combinedkey].ToString();
                                                        tb_Passengerobj.InftAmount = Convert.ToDouble(data.Split('/')[0]);
                                                        tb_Passengerobj.InftAmount_Tax = Convert.ToDouble(data.Split('/')[1]);
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
                                                if (tb_Passengerobj.FirstName.Trim().ToUpper() + "_" + tb_Passengerobj.LastName.Trim().ToUpper() == htpaxdetails[bagitem.Groups["Paxid"].Value.Trim()].ToString())
                                                {
                                                    TotalAmount_Baggage = Convert.ToInt32(bagitem.Groups["BagBasePrice"].Value.Trim());
                                                    TotalAmount_Baggage_tax = Convert.ToInt32(bagitem.Groups["BagTaxPrice"].Value.Trim());
                                                    break;
                                                }

                                            }
                                        }

                                        foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""PreReservedSeatAssignment""\s*TotalPrice=""INR(?<SeatPrice>[\s\S]*?)""[\s\S]*?BasePrice=""INR(?<seatBasePrice>[\s\S]*?)""[\s\S]*?Taxes=""INR(?<seatTaxPrice>[\s\S]*?)""[\s\S]*?BookingTravelerRef=""(?<Paxid>[\s\S]*?)""\s*AirSegmentRef=""(?<segid>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                        {
                                            if (oridest == htsegmentdetails[bagitem.Groups["segid"].Value.Trim()].ToString())
                                            {
                                                if (tb_Passengerobj.FirstName.Trim().ToUpper() + "_" + tb_Passengerobj.LastName.Trim().ToUpper() == htpaxdetails[bagitem.Groups["Paxid"].Value.Trim()].ToString())
                                                {
                                                    TotalAmount_Seat = Convert.ToInt32(bagitem.Groups["seatBasePrice"].Value.Trim());
                                                    TotalAmount_Seat_tax = Convert.ToInt32(bagitem.Groups["seatTaxPrice"].Value.Trim());
                                                    break;
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
                                List<tb_journeys> tb_JourneysList = new List<tb_journeys>();
                                List<tb_Segments> segmentReturnsListt = new List<tb_Segments>();
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
                                    tb_JourneysObj.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim()); //DateTime.Now;
                                    tb_JourneysObj.Createdby = ""; //"Online";
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
                                        DateTime parsedDate = DateTime.ParseExact(pnrResDetail.Bonds.Legs[j].ArrivalTime, "yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                        segmentReturnobj.ArrivalDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
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
                                        segmentReturnobj.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim()); //DateTime.Now;
                                        segmentReturnobj.ModifiedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim()); //DateTime.Now;
                                        segmentReturnobj.Createdby = ""; //"Online";
                                        segmentReturnobj.Modifyby = ""; //"Online";
                                        segmentReturnobj.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();
                                        segmentReturnsListt.Add(segmentReturnobj);

                                    }
                                }
                                airLineFlightTicketBooking.tb_Booking = tb_Booking;
                                airLineFlightTicketBooking.GSTDetails = gSTDetails;
                                airLineFlightTicketBooking.tb_Segments = segmentReturnsListt;
                                airLineFlightTicketBooking.tb_AirCraft = tb_AirCraft;
                                airLineFlightTicketBooking.tb_journeys = tb_JourneysList;
                                airLineFlightTicketBooking.tb_PassengerTotal = tb_PassengerTotalobj;
                                airLineFlightTicketBooking.tb_PassengerDetails = tb_PassengerDetailsList;
                                airLineFlightTicketBooking.ContactDetail = contactDetail;
                                client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                HttpResponseMessage responsePassengers = await client1.PostAsJsonAsync(AppUrlConstant.BaseURL + "api/AirLineTicketBooking/PostairlineTicketData", airLineFlightTicketBooking);
                                if (responsePassengers.IsSuccessStatusCode)
                                {
                                    var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                }
                                #endregion
                            }
                            #endregion

                        }
                    }
                }
            }
            return View(_AirLinePNRTicket);
        }

    }
}
