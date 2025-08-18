using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using DomainLayer.Model;
using DomainLayer.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Utility;
using OnionConsumeWebAPI.Models;
using OnionArchitectureAPI.Services.Travelport;
using System.Text;
using OnionConsumeWebAPI.Extensions;
using System.Collections;
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
        string _targetBranch = string.Empty;
        string _userName = string.Empty;
        string _password = string.Empty;
        public GDSTripsellController(IConfiguration configuration)
        {
            _configuration = configuration;
            Task.Run(async () => await LoadCredentialsAsync()).Wait();

        }

        private async Task LoadCredentialsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);

                var url = $"{AppUrlConstant.Getsuppliercred}?flightclass={Uri.EscapeDataString("Corporate")}";
                HttpResponseMessage response = await client.GetAsync(url);
                //HttpResponseMessage response = await client.GetAsync(AppUrlConstant.Getsuppliercred);

                if (response.IsSuccessStatusCode)
                {
                    var results = await response.Content.ReadAsStringAsync();
                    var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);

                    var _CredentialsGDS = jsonObject.FirstOrDefault(cred => cred?.supplierid == 5 && cred.Status == 1);

                    _targetBranch = _CredentialsGDS.organizationId;
                    _userName = _CredentialsGDS.username;
                    _password = _CredentialsGDS.password;
                }
            }
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
                        }


                    }
                }
                catch when (GUID == null)
                {
                }
                catch
                {

                }
                ViewBag.GSTdata = gstList;
            }
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
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoHelper objMongoHelper = new MongoHelper();
            string contobj = objMongoHelper.Zip(JsonConvert.SerializeObject(contactobject));
            _mongoDBHelper.UpdateFlightTokenContact(GUID, "GDS", contobj);
            return RedirectToAction("GDSSaverTripsell", "GDSTripsell", new { Guid = GUID });
        }
        public async Task<PartialViewResult> GDSTravllerDetails(List<passkeytype> passengerdetails, string GUID)
        {
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
            MongoHelper objMongoHelper = new MongoHelper();
            string passobj = objMongoHelper.Zip(JsonConvert.SerializeObject(passengerdetails));
            _mongoDBHelper.UpdateFlightTokenOldPassengerGDS(GUID, "GDS", passobj);
            string passenger = HttpContext.Session.GetString("SGkeypassenger"); //From Itenary Response
            string passengerInfant = HttpContext.Session.GetString("SGkeypassenger");
            string Meals = HttpContext.Session.GetString("Meals");
            string Baggage = HttpContext.Session.GetString("Baggage");
            ViewModel vm = new ViewModel();
            passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));

            string _pricesolution = string.Empty;
            _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_0");
            TravelPort _objAvail = null;
            HttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();
            _objAvail = new TravelPort(httpContextAccessorInstance);
            string _UniversalRecordURL = AppUrlConstant.GDSUniversalRecordURL;
            string _testURL = AppUrlConstant.GDSURL;
            //string _targetBranch = string.Empty;
            //string _userName = string.Empty;
            //string _password = string.Empty;
            //_targetBranch = "P7027135";
            //_userName = "Universal API/uAPI5098257106-beb65aec";
            //_password = "Q!f5-d7A3D";


            //using (HttpClient client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);


            //    HttpResponseMessage response = await client.GetAsync(AppUrlConstant.Getsuppliercred);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var results = await response.Content.ReadAsStringAsync();
            //        var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);

            //        _credentials _CredentialsGDS = new _credentials();
            //        _CredentialsGDS = jsonObject.FirstOrDefault(cred => cred?.supplierid == 5 && cred.Status == 1);

            //        _targetBranch = _CredentialsGDS.organizationId;
            //        _userName = _CredentialsGDS.username;
            //        _password = _CredentialsGDS.password;
            //    }
            //}



            StringBuilder createPNRReq = new StringBuilder();
            StringBuilder createAirmerchandReq = new StringBuilder();
            string passengerNamedetails = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "GDS").Result;
            if (string.IsNullOrEmpty(passengerNamedetails))
            {
                _mongoDBHelper.UpdateFlightTokenOldPassengerGDS(GUID, "GDS", passobj);
                tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "GDS").Result;
                passengerNamedetails = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
            }
            string AdultTraveller = passengerNamedetails;
            string _data = HttpContext.Session.GetString("SGkeypassenger");
            string _Total = HttpContext.Session.GetString("Total");

            string serializedUnitKey = HttpContext.Session.GetString("UnitKey");
            List<string> _unitkey = new List<string>();
            if (!string.IsNullOrEmpty(serializedUnitKey))
            {
                _unitkey = JsonConvert.DeserializeObject<List<string>>(serializedUnitKey);
            }

            string serializedSSRKey = HttpContext.Session.GetString("ssrKey");
            List<string> _SSRkey = new List<string>();
            if (!string.IsNullOrEmpty(serializedSSRKey))
            {
                _SSRkey = JsonConvert.DeserializeObject<List<string>>(serializedSSRKey);
            }
            string newGuid = tokenData.Token;
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
                        htSSr.Add(weight, item.Groups["Price"].Value.Trim() + "@" + item.Value.ToString());
                    }
                    _htpaxwiseBaggage.Add(weight + "_" + BookingTravellerref + "_" + item.Groups["Price"].Value.Trim().Replace("INR", ""), item.Value.ToString());
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
                            SSRAvailabiltyLegssrobj.legKey = "";
                            legDetailsobj = new legDetails();
                            legDetailsobj.destination = "";
                            legDetailsobj.origin = "";
                            legDetailsobj.departureDate = "";
                            legidentifier legidentifierobj = new legidentifier();
                            legidentifierobj.identifier = "";
                            legidentifierobj.carrierCode = "";
                            legDetailsobj.legidentifier = legidentifierobj;
                            childlegssrs legssrs = new childlegssrs();
                            legssrs.ssrCode = (string)entry.Key;
                            legssrs.name = legssrs.ssrCode.ToString();
                            legssrs.available = 0;
                            List<legpassengers> legpassengerslist = new List<legpassengers>();
                            legpassengers passengersdetail = new legpassengers();
                            passengersdetail.passengerKey = "";
                            passengersdetail.ssrKey = "";
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
                vm.passengerNamedetails = passengerNamedetailsdata;
            }
            vm.passeengerlist = passeengerlist;
            vm.Seatmaplist = Seatmaplist;
            vm.Meals = Mealslist;
            vm.Baggage = BaggageList;
            vm.htpaxwiseBaggage = _htpaxwiseBaggage;
            HttpContext.Session.SetString("hashdataBaggage", JsonConvert.SerializeObject(_htpaxwiseBaggage));
            return PartialView("_GDSServiceRequestsPartialView", vm);
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
            HttpContext.Session.SetString("ssrKey", serializedSSRKey);
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
            string segmentblock = HttpContext.Session.GetString("Segmentdetails");
            string stravailibitilityrequest = objMongoHelper.UnZip(tokenData.PassRequest);
            SimpleAvailabilityRequestModel availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(stravailibitilityrequest);
            TravelPort _objAvail = null;
            HttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();
            _objAvail = new TravelPort(httpContextAccessorInstance);
            string _UniversalRecordURL = AppUrlConstant.GDSUniversalRecordURL;
            // string _testURL = AppUrlConstant.GDSURL;
            //string _targetBranch = string.Empty;
            //string _userName = string.Empty;
            //string _password = string.Empty;
            //_targetBranch = "P7027135";
            //_userName = "Universal API/uAPI5098257106-beb65aec";
            //_password = "Q!f5-d7A3D";


            //using (HttpClient client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);
            //    HttpResponseMessage response = await client.GetAsync(AppUrlConstant.Getsuppliercred);
            //    if (response.IsSuccessStatusCode)
            //    {
            //        var results = await response.Content.ReadAsStringAsync();
            //        var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);

            //        _credentials _CredentialsGDS = new _credentials();
            //        _CredentialsGDS = jsonObject.FirstOrDefault(cred => cred?.supplierid == 5 && cred.Status == 1);

            //        _targetBranch = _CredentialsGDS.organizationId;
            //        _userName = _CredentialsGDS.username;
            //        _password = _CredentialsGDS.password;
            //    }
            //}


            StringBuilder createSSRReq = new StringBuilder();
            string _data = HttpContext.Session.GetString("SGkeypassenger");
            string _Total = HttpContext.Session.GetString("Total");
            var jsonDataObject = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(jsonDataObject.ToString(), typeof(List<passkeytype>));
            string hashbaggagedata = HttpContext.Session.GetString("hashdataBaggage");
            Hashtable htbaggagedata = (Hashtable)JsonConvert.DeserializeObject(hashbaggagedata, typeof(Hashtable));
            //PNR
            string _pricesolution = string.Empty;
            _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_0");
            StringBuilder createPNRReq = new StringBuilder();
            string res = _objAvail.CreatePNR(AppUrlConstant.GDSURL, createPNRReq, newGuid.ToString(), _targetBranch, _userName, _password, jsonDataObject, _data, _Total, "GDSOneWay", _unitkey, ssrKey, _pricesolution);
            string RecordLocator = Regex.Match(res, @"universal:UniversalRecord\s*LocatorCode=""(?<LocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["LocatorCode"].Value.Trim();
            string UniversalLocatorCode = string.Empty;
            if (!string.IsNullOrEmpty(RecordLocator))
            {
                string strResponse = _objAvail.RetrivePnr(RecordLocator, _UniversalRecordURL, newGuid.ToString(), _targetBranch, _userName, _password, "GDSOneWay");
                string ProvidelocatorCode = Regex.Match(strResponse, @"universal:ProviderReservationInfo[\s\S]*?LocatorCode=""(?<ProviderLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["ProviderLocatorCode"].Value.Trim();
                string supplierLocatorCode = Regex.Match(strResponse, @"SupplierLocatorCode=""(?<SupplierLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["SupplierLocatorCode"].Value.Trim();
                UniversalLocatorCode = Regex.Match(strResponse, @"UniversalRecord\s*LocatorCode=""(?<UniversalLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["UniversalLocatorCode"].Value.Trim();
                segmentblock += "@" + ProvidelocatorCode + "@" + supplierLocatorCode + "@" + UniversalLocatorCode;
                jsonDataObject = objMongoHelper.UnZip(tokenData.PassengerRequest); //HttpContext.Session.GetString("PassengerModel");
                passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(jsonDataObject.ToString(), typeof(List<passkeytype>));
                string strSeatResponseleft = HttpContext.Session.GetString("SeatResponseleft");
                res = _objAvail.AirMerchandisingFulfillmentReq(AppUrlConstant.GDSURL, createSSRReq, newGuid.ToString(), _targetBranch, _userName, _password, "GDSOneWay", unitKey, ssrKey, BaggageSSrkey, availibiltyRQGDS, passengerdetails, htbaggagedata, strSeatResponseleft, segmentblock);
                UniversalLocatorCode = Regex.Match(res, @"UniversalRecord\s*LocatorCode=""(?<UniversalLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["UniversalLocatorCode"].Value.Trim();
            }
            // HttpContext.Session.SetString("PNR", res + "@@" + UniversalLocatorCode);

            GDSPNRResponse mongoGDS = new GDSPNRResponse
            {
                Guid = GUID,
                Response = res,
                LocatorCode = UniversalLocatorCode,
                RLocatorCode = "",
                RResponse = ""
            };


            _mongoDBHelper.SaveGDSLocatorCode(mongoGDS);

            return RedirectToAction("GDSPayment", "GDSPaymentGateway", new { Guid = GUID });
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
