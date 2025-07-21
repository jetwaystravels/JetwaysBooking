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



            //string tokenview = HttpContext.Session.GetString("GDSTraceid");
            string _pricesolution = string.Empty;
            _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_0");
            //if (tokenview == null) { tokenview = ""; }
            //token = string.Empty;
            string newGuid = tokenData.Token;
            if (newGuid == "" || newGuid == null)
            {
                return RedirectToAction("Index");
            }
            if (!string.IsNullOrEmpty(newGuid))
            {

                // token = tokenview.Replace(@"""", string.Empty);
                // string passengernamedetails = HttpContext.Session.GetString("PassengerNameDetails");

                string passengernamedetails = objMongoHelper.UnZip(tokenData.PassengerRequest);

                List<passkeytype> passeengerlist = (List<passkeytype>)JsonConvert.DeserializeObject(passengernamedetails, typeof(List<passkeytype>));
                // string contactdata = HttpContext.Session.GetString("GDSContactdetails");

                string contactdata = objMongoHelper.UnZip(tokenData.ContactRequest);

                ContactModel contactList = (ContactModel)JsonConvert.DeserializeObject(contactdata, typeof(ContactModel));
                using (HttpClient client1 = new HttpClient())
                {

                    //_commit objcommit = new _commit();
                    #region GetState
                    //_sell objsell = new _sell();
                    //IndigoBookingManager_.GetBookingFromStateResponse _GetBookingFromStateRS1 = await objsell.GetBookingFromState(token, "");

                    //string strdata = JsonConvert.SerializeObject(_GetBookingFromStateRS1);
                    //decimal Totalpayment = 0M;
                    //if (_GetBookingFromStateRS1 != null)
                    //{
                    //    Totalpayment = _GetBookingFromStateRS1.BookingData.BookingSum.TotalCost;
                    //}
                    #endregion
                    #region Addpayment Commneted For Api Payment deduction
                    //IndigoBookingManager_.AddPaymentToBookingResponse _BookingPaymentResponse = await objcommit.AddpaymenttoBook(token, Totalpayment, "OneWay");

                    #endregion

                    #region Commit Booking
                    TravelPort _objAvail = null;

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
                    string AdultTraveller = passengernamedetails;
                    string strResponse = HttpContext.Session.GetString("PNR").Split("@@")[0];

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
                    string RecordLocator = HttpContext.Session.GetString("PNR").Split("@@")[1];
                    string strResponseretriv = _objAvail.RetrivePnr(RecordLocator, _UniversalRecordURL, newGuid.ToString(), _targetBranch, _userName, _password, "GDSOneWay");

                    /*
		            string EmdBagResponsedata = string.Empty;
                    string EmdSeatResponsedata = string.Empty;
                    string ProviderLocatorCode = Regex.Match(strResponse, "ProviderReservationInfo\\s*Key=\"[\\s\\S]*?LocatorCode=\"(?<LocatorCode>[\\s\\S]*?)\"").Groups["LocatorCode"].Value.Trim();
                    string UniversalLocatorCode = Regex.Match(strResponse, "UniversalRecord LocatorCode=\"(?<UniLocatorCode>[\\s\\S]*?)\"").Groups["UniLocatorCode"].Value.Trim();
                    string Segmentkey = string.Empty;
                    foreach (Match mitem in Regex.Matches(strResponse, "AirSegment Key=\"(?<Segmentkey>[\\s\\S]*?)\""))
                    {
                        Segmentkey += " " + mitem.Groups["Segmentkey"].Value.Trim();
                    }
                    string Ticketnum = string.Empty;
                    string EndUrl= AppUrlConstant.GDSEmdURL;
                    string strEmdBagResponse = string.Empty;
                    string strEmdSeatResponse = string.Empty;
                    foreach (DictionaryEntry entry in htTicketdata)
                    {
                        Ticketnum = entry.Value.ToString();
                        strEmdBagResponse = _objAvail.RetriveEmdIssuranceBag(EndUrl, newGuid.ToString(), _targetBranch, _userName, _password, ProviderLocatorCode, UniversalLocatorCode, Ticketnum, Segmentkey, "GDSOneWay");
                        EmdBagResponsedata += "\n" + strEmdBagResponse;
                        strEmdSeatResponse = _objAvail.RetriveEmdIssuranceSeat(EndUrl, newGuid.ToString(), _targetBranch, _userName, _password, ProviderLocatorCode, UniversalLocatorCode, Ticketnum, Segmentkey, "GDSOneWay");
                        EmdBagResponsedata += "\n" + strEmdSeatResponse;

                    } */


                    GDSResModel.PnrResponseDetails pnrResDetail = new GDSResModel.PnrResponseDetails();
                    if (!string.IsNullOrEmpty(strResponse) && !string.IsNullOrEmpty(RecordLocator))
                    {
                        //IndigoBookingManager_.GetBookingResponse _getBookingResponse = await objcommit.GetBookingdetails(token, _BookingCommitResponse, "OneWay");
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

                            Hashtable htseatdata = new Hashtable();
                            Hashtable htmealdata = new Hashtable();
                            Hashtable htbagdata = new Hashtable();
                            //int adultcount = Convert.ToInt32(HttpContext.Session.GetString("adultCount"));
                            //int childcount = Convert.ToInt32(HttpContext.Session.GetString("childCount"));
                            //int infantcount = Convert.ToInt32(HttpContext.Session.GetString("infantCount"));
                            int adultcount = searchLog.Adults;
                            int childcount = searchLog.Children;
                            int infantcount = searchLog.Infants;
                            int TotalCount = adultcount + childcount;
                            //string _responceGetBooking = JsonConvert.SerializeObject(_getBookingResponse);
                            ReturnTicketBooking returnTicketBooking = new ReturnTicketBooking();


                            var resultsTripsell = "";// responseTripsell.Content.ReadAsStringAsync().Result;
                            var JsonObjTripsell = "";// JsonConvert.DeserializeObject<dynamic>(resultsTripsell);
                            var totalAmount = "";// _getBookingResponse.Booking.BookingSum.TotalCost;
                            returnTicketBooking.bookingKey = "";// _getBookingResponse.Booking.BookingID.ToString();
                            ReturnPaxSeats _unitdesinator = new ReturnPaxSeats();
                            //if (_getBookingResponse.Booking.Journeys[0].Segments[0].PaxSeats.Length > 0)
                            _unitdesinator.unitDesignatorPax = "";// _getBookingResponse.Booking.Journeys[0].Segments[0].PaxSeats[0].UnitDesignator;

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
                            var totalTax = "";// _getPriceItineraryRS.data.breakdown.journeys[journeyKey].totalTax;

                            #region Itenary segment and legs
                            int journeyscount = 1;// _getBookingResponse.Booking.Journeys.Length;
                            List<JourneysReturn> AAJourneyList = new List<JourneysReturn>();
                            for (int i = 0; i < journeyscount; i++)
                            {

                                JourneysReturn AAJourneyobj = new JourneysReturn();
                                //orides= _getBookingResponse.Booking.Journeys[i].
                                //AAJourneyobj.flightType = JsonObjTripsell.data.journeys[i].flightType;
                                //AAJourneyobj.stops = JsonObjTripsell.data.journeys[i].stops;
                                AAJourneyobj.journeyKey = "";// _getBookingResponse.Booking.Journeys[i].JourneySellKey;

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
                                    //            //AASegmentobj.isStandby = JsonObjTripsell.data.journeys[i].segments[j].isStandby;
                                    //            //AASegmentobj.isHosted = JsonObjTripsell.data.journeys[i].segments[j].isHosted;

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
                                        AAFareobj.fareKey = "";// _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].FareSellKey;
                                                               //To  do;
                                        AAFareobj.productClass = "";// _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].ProductClass;

                                        //var passengerFares = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares;

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
                                                //                }
                                                //            }
                                                //        }
                                                //    }
                                                //}



                                                //var serviceCharges1 = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges;
                                                // int serviceChargescount = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges.Length;
                                                List<ServiceChargeReturn> AAServicechargelist = new List<ServiceChargeReturn>();
                                                //                        for (int m = 0; m < serviceChargescount; m++)
                                                //                        {
                                                ServiceChargeReturn AAServicechargeobj = new ServiceChargeReturn();
                                                AAServicechargeobj.amount = Convert.ToInt32(currentContact.BasicFare);
                                                //                            string data = _getBookingResponse.Booking.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].ChargeType.ToString();
                                                //                            if (data.ToLower() == "fareprice")
                                                //                            {
                                                journeyTotalsobj.totalAmount += Convert.ToInt32(currentContact.BasicFare);
                                                //                            }
                                                //                            else
                                                //                            {
                                                journeyTotalsobj.totalTax += Convert.ToInt32(currentContact.TotalTax);
                                                //                            }


                                                AAServicechargelist.Add(AAServicechargeobj);
                                                //                        }

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
                                    // }
                                    //}
                                    //            //breakdown.journeyTotals = journeyTotalsobj;
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



                                    //var leg = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs;
                                    //          int legcount = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs.Length;
                                    List<LegReturn> AALeglist = new List<LegReturn>();
                                    //            for (int n = 0; n < legcount; n++)
                                    //            {
                                    LegReturn AALeg = new LegReturn();
                                    //                //AALeg.legKey = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legKey;
                                    DesignatorReturn AAlegDesignatorobj = new DesignatorReturn();
                                    AAlegDesignatorobj.origin = pnrResDetail.Bonds.Legs[j].Origin;
                                    AAlegDesignatorobj.destination = pnrResDetail.Bonds.Legs[j].Destination;
                                    //AAlegDesignatorobj.departure = pnrResDetail.Bonds.Legs[segmentscount].
                                    // AAlegDesignatorobj.arrival = _getBookingResponse.Booking.Journeys[i].Segments[j].Legs[n].STA;
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

                                    //baggage

                                    foreach (Match mitem in Regex.Matches(strResponse, @"common_v52_0:BookingTraveler Key=""(?<passengerKey>[\s\S]*?)""[\s\S]*?BookingTravelerName[\s\S]*?First=""(?<First>[\s\S]*?)""\s*Last=""(?<Last>[\s\S]*?)""(?<data>[\s\S]*?)</common_v52_0:BookingTraveler>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                    {
                                        foreach (Match item in Regex.Matches(mitem.Groups["data"].Value, @"SegmentRef=""(?<segmentkey>[\s\S]*?)""[\s\S]*?Type=""XBAG"" FreeText=""TTL(?<BagWeight>[\s\S]*?)KG", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                        {
                                            try
                                            {
                                                if (!htbagdata.Contains(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString()))
                                                {
                                                    htbagdata.Add(mitem.Groups["First"].Value.Trim() + "_" + mitem.Groups["Last"].Value.Trim() + "_" + htsegmentdetails[item.Groups["segmentkey"].Value.Trim()].ToString(), item.Groups["BagWeight"].Value.Trim());

                                                }

                                            }
                                            catch (Exception ex)
                                            {

                                            }
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
                                    BasefareAmt += breakdown.journeyfareTotals[i].totalAmount;
                                    BasefareTax += breakdown.journeyfareTotals[i].totalTax;
                                }
                                breakdown.journeyTotals = new JourneyTotals();
                                breakdown.journeyTotals.totalAmount = Convert.ToDouble(BasefareAmt);
                                breakdown.passengerTotals.seats = new ReturnSeats();
                                //breakdown.passengerTotals.specialServices = new SpecialServices();
                                breakdown.passengerTotals.specialServices.total = passengerTotals.specialServices.total;
                                breakdown.passengerTotals.baggage.total = passengerTotals.baggage.total;
                                breakdown.passengerTotals.seats.total = returnSeats.total;
                                breakdown.passengerTotals.seats.taxes = returnSeats.taxes;
                                breakdown.journeyTotals.totalTax = Convert.ToDouble(BasefareTax);
                                breakdown.totalAmount = breakdown.journeyTotals.totalAmount + breakdown.journeyTotals.totalTax;
                                //if (totalAmount != 0M)
                                //{
                                breakdown.totalToCollect = Convert.ToDouble(breakdown.journeyfareTotals[0].totalAmount) + Convert.ToDouble(breakdown.journeyfareTotals[0].totalTax);
                                //}
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
                                //returnTicketBooking.passengers = passkeylist;
                                returnTicketBooking.passengerscount = passengercount;
                                returnTicketBooking.contacts = _contact;
                                returnTicketBooking.Seatdata = htseatdata;
                                returnTicketBooking.Mealdata = htmealdata;
                                returnTicketBooking.Bagdata = htbagdata;
                                //returnTicketBooking.Bagdata = htPaxbag;

                                returnTicketBooking.htname = htname;
                                returnTicketBooking.htTicketnumber = htTicketdata;
                                returnTicketBooking.htnameempty = htnameempty;
                                returnTicketBooking.htpax = htpax;
                                returnTicketBooking.TicketNumber = strTicketno;
                                //returnTicketBooking.bookingdate = _getBookingResponse.Booking.BookingInfo.BookingDate;
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
                                        //if (JsonObjPNRBooking.data.breakdown.passengerTotals.seats.adjustments != null)
                                        tb_Booking.SeatAdjustment = 0.0;

                                    }
                                    tb_Booking.SeatTotalAmount -= tb_Booking.SeatTotalAmount_Tax;
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
                                tb_Airlines tb_Airlines = new tb_Airlines();
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
                                tb_AirCraft tb_AirCraft = new tb_AirCraft();
                                tb_AirCraft.Id = 5;
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


                                ContactDetail contactDetail = new ContactDetail();
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


                                GSTDetails gSTDetails = new GSTDetails();
                                gSTDetails.bookingReferenceNumber = "";
                                gSTDetails.GSTEmail = contactList.emailAddressgst;
                                gSTDetails.GSTNumber = contactList.customerNumber;
                                gSTDetails.GSTName = contactList.companyName;
                                gSTDetails.airLinePNR = returnTicketBooking.recordLocator;
                                gSTDetails.status = 0;


                                tb_PassengerTotal tb_PassengerTotalobj = new tb_PassengerTotal();
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

                                List<tb_PassengerDetails> tb_PassengerDetailsList = new List<tb_PassengerDetails>();
                                int SegmentCount = segmentscount;
                                string passenger = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
                                //string passenger = objMongoHelper.UnZip(tokenData.PassengerRequest);

                                List<passkeytype> paxList = (List<passkeytype>)JsonConvert.DeserializeObject(passenger, typeof(List<passkeytype>));
                                List<passkeytype> infantList = paxList.Where(p => p.passengertypecode == "INF").ToList();

                                //To do
                                Hashtable htpaxdetails = new Hashtable();
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
                                        if (!htpassenegerdata.Contains(htpaxdetails[itemnew.Groups["BookingTraveller"].Value.Trim()] + "_" + item.Groups["segment"].Value))
                                        {
                                            htpassenegerdata.Add(htpaxdetails[itemnew.Groups["BookingTraveller"].Value.Trim()] + "_" + item.Groups["segment"].Value, item.Groups["Amount"].Value + "/" + item.Groups["Tax"].Value);
                                        }
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

                                        string combinedName = (tb_Passengerobj.FirstName + "_" + tb_Passengerobj.LastName).ToUpper() + "_" + pnrResDetail.Bonds.Legs[isegment].AircraftCode;
                                        string data = htpassenegerdata[combinedName].ToString();
                                        tb_Passengerobj.TotalAmount = Convert.ToDecimal(data.Split('/')[0]);
                                        tb_Passengerobj.TotalAmount_tax = Convert.ToDecimal(data.Split('/')[1]);
                                        //if (JsonObjPNRBooking.data.info.createdDate != null)
                                        tb_Passengerobj.CreatedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "AirReservation[\\s\\S]*?CreateDate=\"(?<CreateDate>[\\s\\S]*?)\"").Groups["CreateDate"].Value.Trim());  //DateTime.Now;
                                        tb_Passengerobj.Createdby = ""; //"Online";
                                                                        //if (JsonObjPNRBooking.data.info.modifiedDate != null)
                                        tb_Passengerobj.ModifiedDate = Convert.ToDateTime(Regex.Match(strResponseretriv, "universal:ProviderReservationInfo[\\s\\S]*?ModifiedDate=\"(?<Modifieddate>[\\s\\S]*?)\"").Groups["Modifieddate"].Value.Trim());  //DateTime.Now;
                                        tb_Passengerobj.ModifyBy = ""; //"Online";
                                        tb_Passengerobj.Status = Regex.Match(strResponseretriv, "UniversalRecord LocatorCode=[\\s\\S]*?Status=\"(?<Status>[\\s\\S]*?)\"").Groups["Status"].Value.Trim();  //"0";

                                        if (infantList.Count > 0 && tb_Passengerobj.TypeCode == "ADT")
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
                                                    data = htpassenegerdata[combinedkey].ToString();
                                                    tb_Passengerobj.InftAmount = Convert.ToDouble(data.Split('/')[0]);
                                                    tb_Passengerobj.InftAmount_Tax = Convert.ToDouble(data.Split('/')[1]);
                                                }
                                            }

                                        }

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

                                        foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""Baggage""\s*TotalPrice=""INR(?<BagPrice>[\s\S]*?)""[\s\S]*?BasePrice=""INR(?<BagBasePrice>[\s\S]*?)""\s*Taxes=""INR(?<BagTaxPrice>[\s\S]*?)""[\s\S]*?BookingTravelerRef=""(?<Paxid>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                        {
                                            if (tb_Passengerobj.FirstName.Trim().ToUpper() + "_" + tb_Passengerobj.LastName.Trim().ToUpper() == htpaxdetails[bagitem.Groups["Paxid"].Value.Trim()].ToString())
                                            {
                                                TotalAmount_Baggage = Convert.ToInt32(bagitem.Groups["BagBasePrice"].Value.Trim());
                                                TotalAmount_Baggage_tax = Convert.ToInt32(bagitem.Groups["BagTaxPrice"].Value.Trim());
                                                break;
                                            }

                                        }

                                        foreach (Match bagitem in Regex.Matches(strResponse, @"OptionalService Type=""PreReservedSeatAssignment""\s*TotalPrice=""INR(?<SeatPrice>[\s\S]*?)""[\s\S]*?BasePrice=""INR(?<seatBasePrice>[\s\S]*?)""[\s\S]*?Taxes=""INR(?<seatTaxPrice>[\s\S]*?)""[\s\S]*?BookingTravelerRef=""(?<Paxid>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                        {
                                            if (tb_Passengerobj.FirstName.Trim().ToUpper() + "_" + tb_Passengerobj.LastName.Trim().ToUpper() == htpaxdetails[bagitem.Groups["Paxid"].Value.Trim()].ToString())
                                            {
                                                TotalAmount_Seat = Convert.ToInt32(bagitem.Groups["seatBasePrice"].Value.Trim());
                                                TotalAmount_Seat_tax = Convert.ToInt32(bagitem.Groups["seatTaxPrice"].Value.Trim());
                                                break;
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

                                //Trips tb_Trips = new Trips();
                                //tb_Trips.OutboundFlightID = Bookingid;
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

                                client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                HttpResponseMessage responsePassengers = await client1.PostAsJsonAsync(AppUrlConstant.BaseURL + "api/AirLineTicketBooking/PostairlineTicketData", airLineFlightTicketBooking);
                                if (responsePassengers.IsSuccessStatusCode)
                                {
                                    var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                }
                                #endregion
                                //}

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
                    }
                }
            }
            return View(_AirLinePNRTicket);
            //return RedirectToAction("GetTicketBooking", "AirLinesTicket");
        }

        public PointOfSale GetPointOfSale()
        {
            PointOfSale SourcePOS = null;
            try
            {
                SourcePOS = new PointOfSale();
                SourcePOS.State = Bookingmanager_.MessageState.New;
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
