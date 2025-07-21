using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DomainLayer.Model;
using DomainLayer.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
//using OnionArchitectureAPI.Services.Indigo;
//using OnionArchitectureAPI.Services.Indigo;
using Utility;
using static DomainLayer.Model.ReturnTicketBooking;
using OnionConsumeWebAPI.Models;
using System;
using OnionArchitectureAPI.Services.Travelport;
using System.Text;
using OnionConsumeWebAPI.Extensions;
using System.Collections;
using Microsoft.IdentityModel.Tokens;
using CoporateBooking.Models;

namespace OnionConsumeWebAPI.Controllers.TravelClick
{
    public class GDSTripsellController : Controller
    {
        Logs logs = new Logs();
        string BaseURL = "https://dotrezapi.test.I5.navitaire.com";
        string token = string.Empty;
        string ssrKey = string.Empty;
        string journeyKey = string.Empty;
        string uniquekey = string.Empty;
        AirAsiaTripResponceModel passeengerlist = null;
        IHttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();

        private readonly IConfiguration _configuration;

        public GDSTripsellController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> GDSSaverTripsell(string GUID)
        {

            List<SelectListItem> Title = new()
            {
                new SelectListItem { Text = "Mr", Value = "Mr" },
                new SelectListItem { Text = "Ms" ,Value = "Ms" },
                new SelectListItem { Text = "Mrs", Value = "Mrs"},

            };

            ViewBag.Title = Title;
            var AirCraftName = TempData["AirCraftName"];
            ViewData["name"] = AirCraftName;
            //spicejet
            string passenger = HttpContext.Session.GetString("SGkeypassenger"); //From Itenary Response
            string passengerInfant = HttpContext.Session.GetString("SGkeypassenger");
            string Seatmap = HttpContext.Session.GetString("Seatmap");
            string Meals = HttpContext.Session.GetString("Meals");
            string Baggage = HttpContext.Session.GetString("Baggage");
            SSRAvailabiltyResponceModel Baggagelist = null;

            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "GDS").Result;
            string passengerNamedetails = objMongoHelper.UnZip(tokenData.PassengerRequest);

            int? airlineId = 6;

            _mongoDBHelper.UpdateSuppLegalEntity(GUID, Convert.ToString(airlineId.Value));

            LegalEntity legal = new LegalEntity();
            legal = _mongoDBHelper.GetlegalEntityByGUID(GUID).Result;

            if (legal != null)
            {

                //string apiUrl = $"{AppUrlConstant.CompanyEmployeeGST}?employeeCode={legal.Employee}&legalEntityCode={legal.LegalName}";

                string apiUrl = $"{AppUrlConstant.CompanyEmployeeGST}?employeeCode={legal.Employee}&legalEntityCode={legal.BillingEntityName}";

                if (airlineId.HasValue)
                {
                    apiUrl += $"&airlineId={airlineId.Value}";
                }
                List<CompanyEmployeeGSTDetails> gstList = new();
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(
                            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


                        HttpResponseMessage response = await client.GetAsync(apiUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonData = await response.Content.ReadAsStringAsync();
                            gstList = JsonConvert.DeserializeObject<List<CompanyEmployeeGSTDetails>>(jsonData);
                            // Optional: Deserialize JSON if you have a model
                            // var customers = JsonConvert.DeserializeObject<List<CustomerDetails>>(jsonData);
                            // ViewBag.GSTdata = gstData;
                        }


                    }
                }

                catch when (GUID == null)
                {
                    // Handle the exception when Guid is null
                    // You can log the error or take appropriate action
                }
                catch
                {

                }



                ViewBag.GSTdata = gstList;
            }

            // string passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");
            ViewModel vm = new ViewModel();
            if (passengerInfant != null)
            {
                AirAsiaTripResponceModel passeengerlistItanary = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passengerInfant, typeof(AirAsiaTripResponceModel));
                passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));
                if (Baggage != null)
                {
                    Baggagelist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Baggage, typeof(SSRAvailabiltyResponceModel));
                }
                vm.passeengerlist = passeengerlist;
                vm.passeengerlistItanary = passeengerlistItanary;
                vm.Seatmaplist = Seatmaplist;
                vm.Meals = Mealslist;
                vm.Baggage = Baggagelist;
            }
            else
            {
                if (passenger != null)
                {
                    passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                    SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
                    SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));
                    if (Baggage != null)
                    {
                        Baggagelist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Baggage, typeof(SSRAvailabiltyResponceModel));
                    }

                    vm.passeengerlist = passeengerlist;
                    vm.Seatmaplist = Seatmaplist;
                    vm.Meals = Mealslist;
                    vm.Baggage = Baggagelist;
                }
            }
            if (!string.IsNullOrEmpty(passengerNamedetails))
            {
                List<passkeytype> passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                vm.passengerNamedetails = passengerNamedetailsdata;
            }
            return View(vm);

        }

        //Seat map meal Pip Up bind Code 
        public IActionResult PostSeatMapModaldataView(string GUID)
        {

            List<SelectListItem> Title = new()
            {
                new SelectListItem { Text = "Mr", Value = "Mr" },
                new SelectListItem { Text = "Ms" ,Value = "Ms" },
                new SelectListItem { Text = "Mrs", Value = "Mrs"},

            };

            ViewBag.Title = Title;
            var AirlineName = TempData["AirLineName"];
            ViewData["name"] = AirlineName;

            string passenger = HttpContext.Session.GetString("SGkeypassenger"); //From Itenary Response
            string passengerInfant = HttpContext.Session.GetString("SGkeypassenger");
            string Seatmap = HttpContext.Session.GetString("Seatmap");
            string Meals = HttpContext.Session.GetString("Meals");
            //string passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");

            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "GDS").Result;
            string passengerNamedetails = objMongoHelper.UnZip(tokenData.PassengerRequest);

            ViewModel vm = new ViewModel();
            if (passengerInfant != null)
            {
                AirAsiaTripResponceModel passeengerlistItanary = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passengerInfant, typeof(AirAsiaTripResponceModel));
                passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));
                //SeatMapResponceModel Seatmaplist = new SeatMapResponceModel();
                //SSRAvailabiltyResponceModel Mealslist = new SSRAvailabiltyResponceModel();
                if (!string.IsNullOrEmpty(passengerNamedetails))
                {
                    List<passkeytype> passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                    vm.passengerNamedetails = passengerNamedetailsdata;
                }
                vm.passeengerlist = passeengerlist;
                vm.passeengerlistItanary = passeengerlistItanary;
                vm.Seatmaplist = Seatmaplist;
                vm.Meals = Mealslist;
            }
            else
            {
                passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));
                //SeatMapResponceModel Seatmaplist = new SeatMapResponceModel();
                //SSRAvailabiltyResponceModel Mealslist = new SSRAvailabiltyResponceModel();
                if (!string.IsNullOrEmpty(passengerNamedetails))
                {
                    List<passkeytype> passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                    vm.passengerNamedetails = passengerNamedetailsdata;
                }
                vm.passeengerlist = passeengerlist;
                vm.Seatmaplist = Seatmaplist;
                vm.Meals = Mealslist;
            }
            return View(vm);
        }


        public async Task<IActionResult> GDSContactDetails(ContactModel contactobject, string GUID)
        {

            //string Signature = HttpContext.Session.GetString("GDSSignature");
            //if (Signature == null) { Signature = ""; }
            //if (!string.IsNullOrEmpty(Signature))
            //{
            //    Signature = Signature.Replace(@"""", string.Empty);
            //    //_updateContact obj = new _updateContact(httpContextAccessorInstance);
            //    IndigoBookingManager_.UpdateContactsResponse _responseAddContact6E = null;// await obj.GetUpdateContacts(Signature, contactobject.emailAddress, contactobject.number, contactobject.companyName, contactobject.customerNumber, "OneWay");
            //    string Str1 = JsonConvert.SerializeObject(_responseAddContact6E);
            //}

            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);

            MongoHelper objMongoHelper = new MongoHelper();
            string contobj = objMongoHelper.Zip(JsonConvert.SerializeObject(contactobject));
            _mongoDBHelper.UpdateFlightTokenContact(GUID, "GDS", contobj);

            // HttpContext.Session.SetString("GDSContactdetails", JsonConvert.SerializeObject(contactobject));
            return RedirectToAction("GDSSaverTripsell", "GDSTripsell", new { Guid = GUID });
        }

        //Passenger Data on Trip Page

        public async Task<PartialViewResult> GDSTravllerDetails(List<passkeytype> passengerdetails, string GUID)
        {
            //HttpContext.Session.SetString("PassengerNameDetails", JsonConvert.SerializeObject(passengerdetails));

            //string Signature = HttpContext.Session.GetString("PassengerNameDetails");
            //if (Signature == null) { Signature = ""; }
            //if (!string.IsNullOrEmpty(Signature))
            //{
            //    Signature = Signature.Replace(@"""", string.Empty);
            //    //_updateContact obj = new _updateContact(httpContextAccessorInstance);
            //    IndigoBookingManager_.UpdatePassengersResponse updatePaxResp = null;// await obj.UpdatePassengers(Signature, passengerdetails, "OneWay");
            //    string Str2 = JsonConvert.SerializeObject(updatePaxResp);
            //}


            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

            MongoHelper objMongoHelper = new MongoHelper();
            string passobj = objMongoHelper.Zip(JsonConvert.SerializeObject(passengerdetails));

            //List<string> unitKey = new List<string>();
            //string serializedUnitKey = JsonConvert.SerializeObject(unitKey);
            //// Store the serialized string in session
            //HttpContext.Session.SetString("UnitKey", serializedUnitKey);

            //List<string> ssrKey = new List<string>();
            //string serializedSSRKey = JsonConvert.SerializeObject(ssrKey);
            //// Store the serialized string in session
            //HttpContext.Session.SetString("ssrKey", serializedSSRKey);

            _mongoDBHelper.UpdateFlightTokenOldPassengerGDS(GUID, "GDS", passobj);

           

            string passenger = HttpContext.Session.GetString("SGkeypassenger"); //From Itenary Response
            string passengerInfant = HttpContext.Session.GetString("SGkeypassenger");
            //string Seatmap = HttpContext.Session.GetString("Seatmap");
            string Meals = HttpContext.Session.GetString("Meals");
            string Baggage = HttpContext.Session.GetString("Baggage");
            //string passengerNamedetails = JsonConvert.SerializeObject(passengerdetails); // HttpContext.Session.GetString("PassengerNameDetails");
          
            ViewModel vm = new ViewModel();
            passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));

            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "GDS").Result;
            string passengerNamedetails = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
            if(string.IsNullOrEmpty(passengerNamedetails))
            {
                _mongoDBHelper.UpdateFlightTokenOldPassengerGDS(GUID, "GDS", passobj);
                tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "GDS").Result;
                passengerNamedetails = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
            }
            //Addcreate Reservation
            string _pricesolution = string.Empty;
            _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_0");
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
            StringBuilder createAirmerchandReq = new StringBuilder();
            string AdultTraveller = passengerNamedetails;
            string _data = HttpContext.Session.GetString("SGkeypassenger");
            string _Total = HttpContext.Session.GetString("Total");

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
            string newGuid = tokenData.Token;
            //retrive PNR

            string segmentdata = string.Empty;
            foreach (Match item in Regex.Matches(_pricesolution.Replace("\\", ""), "<air:AirSegment Key=\"[\\s\\S]*?</air:AirSegment><air:AirPricingInfo", RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                segmentdata += item.Value.Replace("<air:AirPricingInfo", "");
            }
            HttpContext.Session.SetString("Segmentdetails", segmentdata);


            //Seat Map
            string stravailibitilityrequest = HttpContext.Session.GetString("GDSAvailibilityRequest");
            SimpleAvailabilityRequestModel availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(stravailibitilityrequest);
            Hashtable _htpaxwiseBaggage = new Hashtable();
            string res = _objAvail.GetAirMerchandisingOfferAvailabilityReq(_testURL, createAirmerchandReq, newGuid.ToString(), _targetBranch, _userName, _password, AdultTraveller, _data, "GDSOneWay", segmentdata);
            if (res != null)
            {
                string weight = "";
                string BookingTravellerref = "";
                Hashtable htSSr = new Hashtable();

                foreach (Match item in Regex.Matches(res, @"<air:OptionalService Type=""Baggage""[\s\S]*?TotalPrice=""(?<Price>[\s\S]*?)""[\s\S]*?</air:OptionalService>"))
                {
                    if (!item.Value.Contains("TotalWeight"))
                        continue;
                    else
                    {
                        weight = Regex.Match(item.Value, @"TotalWeight=""(?<Weight>[\s\S]*?)""").Groups["Weight"].Value;
                        BookingTravellerref = Regex.Match(item.Value, @"BookingTravelerRef=""(?<BookingTravelerRef>[\s\S]*?)""").Groups["BookingTravelerRef"].Value;
                    }
                    if (!htSSr.Contains(weight))
                    {
                        //htSSr.Add(weight, item.Groups["Price"].Value.Trim() + "@" + item.Value.ToString() + "@" + UniversalLocatorCode + "@" + supplierLocatorCode + "@" + ProvidelocatorCode + "@" + strAirsegmenttext + "@" + Trvellerrefkey);
                        htSSr.Add(weight, item.Groups["Price"].Value.Trim() + "@" + item.Value.ToString());
                    }
                    _htpaxwiseBaggage.Add(weight + "_" + BookingTravellerref + "_" + item.Groups["Price"].Value.Trim().Replace("INR",""), item.Value.ToString());
                }


                List<legSsrs> SSRAvailabiltyLegssrlist = new List<legSsrs>();
                SSRAvailabiltyResponceModel SSRAvailabiltyResponceobj = new SSRAvailabiltyResponceModel();
                try
                {
                    legSsrs SSRAvailabiltyLegssrobj = new legSsrs();
                    legDetails legDetailsobj = null;
                    List<childlegssrs> legssrslist = new List<childlegssrs>();
                    foreach (DictionaryEntry entry in _htpaxwiseBaggage)
                    {
                        legssrslist = new List<childlegssrs>();
                        try
                        {
                            SSRAvailabiltyLegssrobj = new legSsrs();
                            SSRAvailabiltyLegssrobj.legKey = "";// _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.ToString();
                            legDetailsobj = new legDetails();
                            legDetailsobj.destination = "";// _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.ArrivalStation;
                            legDetailsobj.origin = "";// _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.DepartureStation;
                            legDetailsobj.departureDate = ""; //_res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.DepartureDate.ToString();
                            legidentifier legidentifierobj = new legidentifier();
                            legidentifierobj.identifier = "";//_res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.FlightNumber;
                            legidentifierobj.carrierCode = ""; //_res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.CarrierCode;
                            legDetailsobj.legidentifier = legidentifierobj;
                            childlegssrs legssrs = new childlegssrs();
                            legssrs.ssrCode = (string)entry.Key; // htSSr[i1].
                            legssrs.name = legssrs.ssrCode.ToString();
                            legssrs.available = 0;// _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].Available;
                            List<legpassengers> legpassengerslist = new List<legpassengers>();
                            Decimal Amount = decimal.Zero;
                            legpassengers passengersdetail = new legpassengers();


                            passengersdetail.passengerKey = "";// _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList[0].PassengerNumberList.ToString();
                            passengersdetail.ssrKey = ""; //_res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRCode;
                            passengersdetail.price = _htpaxwiseBaggage[legssrs.ssrCode].ToString();
                            passengersdetail.Airline = Airlines.AirIndia;
                            legpassengerslist.Add(passengersdetail);
                            legssrs.legpassengers = legpassengerslist;
                            legssrslist.Add(legssrs);
                        }
                        catch (Exception ex)
                        {

                        }

                        SSRAvailabiltyLegssrobj.legDetails = legDetailsobj;
                        SSRAvailabiltyLegssrobj.legssrs = legssrslist;
                        SSRAvailabiltyLegssrlist.Add(SSRAvailabiltyLegssrobj);
                    }

                }
                catch (Exception ex)
                {

                }
                SSRAvailabiltyResponceobj.legSsrs = SSRAvailabiltyLegssrlist;
                HttpContext.Session.SetString("Baggage", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));

            }

            SeatMapResponceModel Seatmaplist = new SeatMapResponceModel(); //(SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
            //SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
            SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));

            Baggage = HttpContext.Session.GetString("Baggage");
            SSRAvailabiltyResponceModel BaggageList = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Baggage, typeof(SSRAvailabiltyResponceModel));

            List<passkeytype> passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));

            for (int i = 0; i < passengerNamedetailsdata.Count; i++)
            {
                foreach (Match mitem in Regex.Matches(res, "SearchTraveler\\s*Key=\"(?<Key>[\\s\\S]*?)\"[\\s\\S]*?Code=\"(?<TravellerType>[\\s\\S]*?)\"[\\s\\S]*?First=\"(?<Fname>[\\s\\S]*?)\"[\\s\\S]*?Last=\"(?<Lname>[\\s\\S]*?)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {
                    if (passengerNamedetailsdata[i].first.ToUpper() == mitem.Groups["Fname"].ToString().ToUpper() && passengerNamedetailsdata[i].last.ToUpper() == mitem.Groups["Lname"].ToString().ToUpper())
                    {
                        passengerNamedetailsdata[i].passengerkey = mitem.Groups["Key"].Value;
                    }
                    else
                    {
                        continue;
                    }
                }

            }
            passobj = objMongoHelper.Zip(JsonConvert.SerializeObject(passengerNamedetailsdata));

            _mongoDBHelper.UpdateFlightTokenPassengerGDS(GUID, "GDS", passobj);

            if (!string.IsNullOrEmpty(passengerNamedetails))
            {
                //vm.passengerNamedetails = passengerNamedetailsdata;
                vm.passengerNamedetails = passengerNamedetailsdata;
            }

            vm.passeengerlist = passeengerlist;
            vm.Seatmaplist = Seatmaplist;
            vm.Meals = Mealslist;
            vm.Baggage = BaggageList;
            vm.htpaxwiseBaggage = _htpaxwiseBaggage;
            HttpContext.Session.SetString("hashdataBaggage", JsonConvert.SerializeObject(_htpaxwiseBaggage));
            return PartialView("_GDSServiceRequestsPartialView", vm);

            //return RedirectToAction("IndigoSaverTripsell", "IndigoTripsell", passengerdetails);
        }
        public async Task<IActionResult> PostUnitkey(List<string> unitKey, List<string> ssrKey, List<string> BaggageSSrkey, string GUID)
        {

            List<string> _unitkey = new List<string>();
            for (int i = 0; i < unitKey.Count; i++)
            {
                if (unitKey[i] == null)
                    continue;
                _unitkey.Add(unitKey[i].Trim());
            }
            unitKey = new List<string>();
            unitKey = _unitkey;

            string serializedUnitKey = JsonConvert.SerializeObject(unitKey);
            // Store the serialized string in session
            HttpContext.Session.SetString("UnitKey", serializedUnitKey);

            List<string> _ssrKey = new List<string>();
            for (int i = 0; i < ssrKey.Count; i++)
            {
                if (ssrKey[i] == null)
                    continue;
                _ssrKey.Add(ssrKey[i].Trim());
            }
            ssrKey = new List<string>();
            ssrKey = _ssrKey;

            string serializedSSRKey = JsonConvert.SerializeObject(ssrKey);
            // Store the serialized string in session
            HttpContext.Session.SetString("ssrKey", serializedSSRKey);

            //string serializedssrKey = JsonConvert.SerializeObject(ssrKey);
            //// Store the serialized string in session
            //HttpContext.Session.SetString("SSRKey", serializedssrKey);
            if (BaggageSSrkey.Count > 0 && BaggageSSrkey[0] == null)
            {
                BaggageSSrkey = new List<string>();
            }
            if (ssrKey.Count > 0 && ssrKey[0] == null)
            {
                ssrKey = new List<string>();
            }
            if (unitKey.Count > 0 && unitKey[0] == null)
            {
                unitKey = new List<string>();
            }
            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "GDS").Result;
            string newGuid = tokenData.Token;
            //AirMerchandisngofferfullfilment
            //string stravailibitilityrequest = HttpContext.Session.GetString("GDSAvailibilityRequest");
            string segmentblock = HttpContext.Session.GetString("Segmentdetails");
            string stravailibitilityrequest = objMongoHelper.UnZip(tokenData.PassRequest); //HttpContext.Session.GetString("PassengerModel");
            SimpleAvailabilityRequestModel availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(stravailibitilityrequest);
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
            StringBuilder createSSRReq = new StringBuilder();
            //string AdultTraveller = passengerNamedetails;
            string _data = HttpContext.Session.GetString("SGkeypassenger");
            string _Total = HttpContext.Session.GetString("Total");
            var jsonDataObject = objMongoHelper.UnZip(tokenData.OldPassengerRequest); //HttpContext.Session.GetString("PassengerModel");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(jsonDataObject.ToString(), typeof(List<passkeytype>));
            string hashbaggagedata = HttpContext.Session.GetString("hashdataBaggage");
            Hashtable htbaggagedata = (Hashtable)JsonConvert.DeserializeObject(hashbaggagedata, typeof(Hashtable));

            //PNR
            string _pricesolution = string.Empty;
            _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_0");
            StringBuilder createPNRReq = new StringBuilder();
            string res = _objAvail.CreatePNR(_testURL, createPNRReq, newGuid.ToString(), _targetBranch, _userName, _password, jsonDataObject, _data, _Total, "GDSOneWay", _unitkey, ssrKey, _pricesolution);

            string RecordLocator = Regex.Match(res, @"universal:UniversalRecord\s*LocatorCode=""(?<LocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["LocatorCode"].Value.Trim();
            string UniversalLocatorCode = string.Empty;
            if (!string.IsNullOrEmpty(RecordLocator))
            {
                //getdetails
                string strResponse = _objAvail.RetrivePnr(RecordLocator, _UniversalRecordURL, newGuid.ToString(), _targetBranch, _userName, _password, "GDSOneWay");


                string ProvidelocatorCode = Regex.Match(strResponse, @"universal:ProviderReservationInfo[\s\S]*?LocatorCode=""(?<ProviderLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["ProviderLocatorCode"].Value.Trim();
                string supplierLocatorCode = Regex.Match(strResponse, @"SupplierLocatorCode=""(?<SupplierLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["SupplierLocatorCode"].Value.Trim();
                UniversalLocatorCode = Regex.Match(strResponse, @"UniversalRecord\s*LocatorCode=""(?<UniversalLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["UniversalLocatorCode"].Value.Trim();
                segmentblock += "@" + ProvidelocatorCode + "@" + supplierLocatorCode + "@" + UniversalLocatorCode;

                jsonDataObject = objMongoHelper.UnZip(tokenData.PassengerRequest); //HttpContext.Session.GetString("PassengerModel");
                passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(jsonDataObject.ToString(), typeof(List<passkeytype>));
                string strSeatResponseleft = HttpContext.Session.GetString("SeatResponseleft");
                res = _objAvail.AirMerchandisingFulfillmentReq(_testURL, createSSRReq, newGuid.ToString(), _targetBranch, _userName, _password, "GDSOneWay", unitKey, ssrKey, BaggageSSrkey, availibiltyRQGDS, passengerdetails, htbaggagedata, strSeatResponseleft, segmentblock);

                UniversalLocatorCode = Regex.Match(res, @"UniversalRecord\s*LocatorCode=""(?<UniversalLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["UniversalLocatorCode"].Value.Trim();
            }
            HttpContext.Session.SetString("PNR", res+"@@"+UniversalLocatorCode);

            return RedirectToAction("GDSPayment", "GDSPaymentGateway", new { Guid = GUID });
        }

        public async Task<IActionResult> PostMeal(legpassengers legpassengers)
        {
            using (HttpClient client = new HttpClient())
            {
                #region SellSSR
                 
                 SellSSRModel _sellSSRModel = new SellSSRModel();
                _sellSSRModel.count = 1;
                _sellSSRModel.note = "DevTest";
                _sellSSRModel.forceWaveOnSell = false;
                _sellSSRModel.currencyCode = "INR";


                var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);



                HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(BaseURL + "/api/nsk/v2/booking/ssrs/" + legpassengers.ssrKey, _sellSSRModel);
                if (responseSellSSR.IsSuccessStatusCode)
                {
                    var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                    var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                }
                #endregion
            }
            return View();
        }

        public class ssrsegmentwise
        {
            public List<ssrsKey> SSRcode0 { get; set; }
            public List<ssrsKey> SSRcode1 { get; set; }
            public List<ssrsKey> SSRcodeOneWayI { get; set; }
            public List<ssrsKey> SSRcodeOneWayII { get; set; }
            public List<ssrsKey> SSRcodeRTI { get; set; }
            public List<ssrsKey> SSRcodeRTII { get; set; }
            public List<ssrsKey> SSRbaggagecodeOneWayI { get; set; }
            public List<ssrsKey> SSRbaggagecodeOneWayII { get; set; }
            public List<ssrsKey> SSRbaggagecodeRTI { get; set; }
            public List<ssrsKey> SSRbaggagecodeRTII { get; set; }
            public List<ssrsKey> SSRffwOneWayI { get; set; }
            public List<ssrsKey> SSRffwcodeRTI { get; set; }
            public List<ssrsKey> PPBGOneWayI { get; set; }
            public List<ssrsKey> PPBGcodeRTI { get; set; }
        }

        public class ssrsKey
        {
            public string key { get; set; }
        }

        public class Paxes
        {
            public List<passkeytype> Adults_ { get; set; }
            public List<passkeytype> Childs_ { get; set; }

            public List<passkeytype> Infant_ { get; set; }
        }
        Paxes _paxes = new Paxes();
    }
}
