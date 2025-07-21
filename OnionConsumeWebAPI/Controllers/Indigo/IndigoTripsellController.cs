using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DomainLayer.Model;
using DomainLayer.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OnionArchitectureAPI.Services.Indigo;
using OnionConsumeWebAPI.Models;
using Utility;
using IndigoBookingManager_;
using static DomainLayer.Model.ReturnTicketBooking;
using Indigo;


namespace OnionConsumeWebAPI.Controllers
{
    public class IndigoTripsellController : Controller
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


        public IndigoTripsellController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult IndigoSaverTripsell(string GUID)
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
            //string passenger = HttpContext.Session.GetString("SGkeypassenger"); //From Itenary Response
            //string passengerInfant = HttpContext.Session.GetString("SGkeypassenger");
            //string Seatmap = HttpContext.Session.GetString("Seatmap");
            //string Meals = HttpContext.Session.GetString("Meals");
            // string passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");

            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "Indigo").Result;
            string passengerNamedetails = objMongoHelper.UnZip(tokenData.PassengerRequest);

			MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
			seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(GUID, "Indigo").Result;


			ViewModel vm = new ViewModel();
            if (seatMealdetail.ResultRequest != null)
            {
                AirAsiaTripResponceModel passeengerlistItanary = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                vm.passeengerlist = passeengerlist;
                vm.passeengerlistItanary = passeengerlistItanary;
                vm.Seatmaplist = Seatmaplist;
                vm.Meals = Mealslist;
            }
            else
            {
                passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                //SeatMapResponceModel Seatmaplist = new SeatMapResponceModel();
                //SSRAvailabiltyResponceModel Mealslist = new SSRAvailabiltyResponceModel();
                vm.passeengerlist = passeengerlist;
                vm.Seatmaplist = Seatmaplist;
                vm.Meals = Mealslist;
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

            //string passenger = HttpContext.Session.GetString("SGkeypassenger"); //From Itenary Response
            //string passengerInfant = HttpContext.Session.GetString("SGkeypassenger");
            //string Seatmap = HttpContext.Session.GetString("Seatmap");
            //string Meals = HttpContext.Session.GetString("Meals");
            //  string passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");

            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "Indigo").Result;
            string passengerNamedetails = objMongoHelper.UnZip(tokenData.PassRequest);

			MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
			seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(GUID, "Indigo").Result;

			ViewModel vm = new ViewModel();
            if (seatMealdetail.ResultRequest != null)
            {
                AirAsiaTripResponceModel passeengerlistItanary = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
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
                passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
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


        public async Task<IActionResult> IndigoContactDetails(ContactModel contactobject, string GUID)
        {

            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "Indigo").Result;

            // string Signature = tokenData.Token;

            if (!string.IsNullOrEmpty(tokenData.Token))
            {
                // Signature = Signature.Replace(@"""", string.Empty);
                _updateContact obj = new _updateContact(httpContextAccessorInstance);
                // Start

                UpdateContactsRequest _ContactModel6E = new UpdateContactsRequest();
                //  _ContactModel.emailAddress = passengerdetails.Email;
                _ContactModel6E.updateContactsRequestData = new IndigoBookingManager_.UpdateContactsRequestData();
                _ContactModel6E.Signature = tokenData.Token;
                _ContactModel6E.ContractVersion = 456;
                _ContactModel6E.updateContactsRequestData.BookingContactList = new BookingContact[1];
                _ContactModel6E.updateContactsRequestData.BookingContactList[0] = new BookingContact();
                _ContactModel6E.updateContactsRequestData.BookingContactList[0].Names = new BookingName[1];
                _ContactModel6E.updateContactsRequestData.BookingContactList[0].Names[0] = new BookingName();
                _ContactModel6E.updateContactsRequestData.BookingContactList[0].Names[0].Title = contactobject.title;
                _ContactModel6E.updateContactsRequestData.BookingContactList[0].Names[0].FirstName = contactobject.first;
                _ContactModel6E.updateContactsRequestData.BookingContactList[0].Names[0].LastName = contactobject.last;

                if (contactobject.customerNumber != null && contactobject.customerNumber != "")
                {
                    _ContactModel6E.updateContactsRequestData.BookingContactList[0].TypeCode = "I";
                    _ContactModel6E.updateContactsRequestData.BookingContactList[0].CompanyName = contactobject.companyName;//"Indigo";
                    _ContactModel6E.updateContactsRequestData.BookingContactList[0].CustomerNumber = contactobject.customerNumber; //GSTNumber Re_ Assistance required for SG API Integration\GST Logs.zip\GST Logs
                    _ContactModel6E.updateContactsRequestData.BookingContactList[0].EmailAddress = contactobject.emailAddressgst;
                }

                else
                {
                    _ContactModel6E.updateContactsRequestData.BookingContactList[0].TypeCode = "P";
                    _ContactModel6E.updateContactsRequestData.BookingContactList[0].EmailAddress = contactobject.emailAddress;
                }
                _ContactModel6E.updateContactsRequestData.BookingContactList[0].HomePhone = contactobject.countrycode + contactobject.number;
                _getapiIndigo objIndigo = new _getapiIndigo();
                UpdateContactsResponse responseAddContact6E = await objIndigo.GetUpdateContacts(_ContactModel6E);
                //SetSessionValue("ContactDetails", JsonConvert.SerializeObject(_ContactModel6E));

                MongoHelper objMongoHelper = new MongoHelper();
                string contobj = objMongoHelper.Zip(JsonConvert.SerializeObject(_ContactModel6E));
                _mongoDBHelper.UpdateFlightTokenContact(GUID, "Indigo", contobj);

                //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_ContactModel6E) + "\n\n Response: " + JsonConvert.SerializeObject(responseAddContact6E), "UpdateContact", "IndigoOneWay", "oneway");
                logs.WriteLogs(JsonConvert.SerializeObject(_ContactModel6E), "9-UpdateContactReq", "IndigoOneWay", "oneway");
                logs.WriteLogs(JsonConvert.SerializeObject(responseAddContact6E), "9-UpdateContactRes", "IndigoOneWay", "oneway");

                //end

                //IndigoBookingManager_.UpdateContactsResponse _responseAddContact6E = await obj.GetUpdateContacts(tokenData.Token, contactobject.emailAddress, contactobject.emailAddressgst, contactobject.number, contactobject.companyName, contactobject.customerNumber, contactobject.countrycode, contactobject.title, contactobject.first, contactobject.last, "OneWay");
                //string Str1 = JsonConvert.SerializeObject(_responseAddContact6E);
            }
            return RedirectToAction("IndigoSaverTripsell", "IndigoTripsell", new { Guid = GUID });
        }

        //Passenger Data on Trip Page

        public async Task<PartialViewResult> IndigoTravllerDetails(List<passkeytype> passengerdetails, string GUID)
        {
            // HttpContext.Session.SetString("PassengerNameDetails", JsonConvert.SerializeObject(passengerdetails));

            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

            MongoHelper objMongoHelper = new MongoHelper();
            string passobj = objMongoHelper.Zip(JsonConvert.SerializeObject(passengerdetails));

            _mongoDBHelper.UpdateFlightTokenPassenger(GUID, "Indigo", passobj);

            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "Indigo").Result;

            string Signature = tokenData.Token;
            if (!string.IsNullOrEmpty(Signature))
            {
                //  Signature = Signature.Replace(@"""", string.Empty);
                _updateContact obj = new _updateContact(httpContextAccessorInstance);
                IndigoBookingManager_.UpdatePassengersResponse updatePaxResp = await obj.UpdatePassengers(Signature, passengerdetails, "OneWay");
                string Str2 = JsonConvert.SerializeObject(updatePaxResp);
            }

			#region GetState
			//_sell objsell = new _sell();
			//IndigoBookingManager_.GetBookingFromStateResponse _GetBookingFromStateRS1 = await objsell.GetBookingFromState(Signature, "");

			//string strdata = JsonConvert.SerializeObject(_GetBookingFromStateRS1);
			#endregion


			//string passenger = HttpContext.Session.GetString("SGkeypassenger"); //From Itenary Response
			//string passengerInfant = HttpContext.Session.GetString("SGkeypassenger");
			//string Seatmap = HttpContext.Session.GetString("Seatmap");
			//string Meals = HttpContext.Session.GetString("Meals");
			// string passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");

			MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
			seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(GUID, "Indigo").Result;

			string passengerNamedetails = objMongoHelper.UnZip(tokenData.PassRequest);

            ViewModel vm = new ViewModel();
            passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
            SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
            SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
			vm.passeengerlist = passeengerlist;
			if (passengerdetails != null)
            {
             //   List<passkeytype> passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                vm.passengerNamedetails = passengerdetails;


				for (int i = 0; i < vm.passeengerlist.passengers.Count; i++)
				{
					vm.passeengerlist.passengers[i].name = vm.passengerNamedetails[i].first + " " + vm.passengerNamedetails[i].last;

				}
			}

           

				
            vm.Seatmaplist = Seatmaplist;
            vm.Meals = Mealslist;

            return PartialView("_IndigoServiceRequestsPartialView", vm);

            //return RedirectToAction("IndigoSaverTripsell", "IndigoTripsell", passengerdetails);
        }
        public async Task<IActionResult> PostUnitkey(List<string> unitKey, List<string> ssrKey, List<string> BaggageSSrkey, List<string> FastfarwardAddon, List<string> PPBGAddon, bool Boolfastforward, string GUID)
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

            if (FastfarwardAddon.Count > 0 && FastfarwardAddon[0] == null)
            {
                FastfarwardAddon = new List<string>();
            }

            if (PPBGAddon.Count > 0 && PPBGAddon[0] == null)
            {
                PPBGAddon = new List<string>();
            }

			MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
			MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

			MongoHelper objMongoHelper = new MongoHelper();
			tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "Indigo").Result;

            string tokenview = tokenData.Token; // HttpContext.Session.GetString("IndigoSignature");
            if (tokenview == null) { tokenview = ""; }
            token = tokenview.Replace(@"""", string.Empty);
            if (token == "" || token == null)
            {
                return RedirectToAction("Index");
            }

			MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
			seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(GUID, "Indigo").Result;

		//	string passenger = HttpContext.Session.GetString("SGkeypassenger");
            AirAsiaTripResponceModel passeengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
            int passengerscount = passeengerKeyList.passengerscount;
            using (HttpClient client = new HttpClient())
            {
                if (ssrKey.Count > 0 || Boolfastforward==true || BaggageSSrkey.Count>0)
                {
                    #region SellSSr
                    _SellSSR obj_ = new _SellSSR(httpContextAccessorInstance);
                    IndigoBookingManager_.SellResponse sellSsrResponse = await obj_.sellssr(token, passeengerKeyList, ssrKey, BaggageSSrkey, FastfarwardAddon, PPBGAddon, Boolfastforward, 0, "OneWay");
                    #endregion

                }
                if (unitKey.Count > 0)
                {
                    try
                    {
                        var unitKey_1 = unitKey;// selectedIds;
                        string[] unitKey2 = null;
                        string[] unitsubKey2 = null;
                        string pas_unitKey = string.Empty;

                        int journeyscount = passeengerKeyList.journeys.Count;

                        _SellSSR obj_ = new _SellSSR(httpContextAccessorInstance);
                        IndigoBookingManager_.AssignSeatsResponse _AssignseatRes = await obj_.AssignSeat(token, passeengerKeyList, unitKey, 0, unitKey.Count, 0, "OneWay");

                        string Str2 = JsonConvert.SerializeObject(_AssignseatRes);

                        if (_AssignseatRes != null)
                        {
                            var JsonObjSeatAssignment = _AssignseatRes;
                            #region GetBookingFromState
                            _sell objsell = new _sell();
                            IndigoBookingManager_.GetBookingFromStateResponse _GetBookingFromStateRS = await objsell.GetBookingFromState(token, 0,"OneWay");

                            string str3 = JsonConvert.SerializeObject(_GetBookingFromStateRS);

                            if (_GetBookingFromStateRS != null)
                            {
                                //var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                            }

                            #endregion
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return RedirectToAction("IndigoPayment", "IndigoPaymentGateway");
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
        //public Passenger[] GetPassenger(List<passkeytype> travellers_)
        //{

        //    _paxes.Adults_ = new List<passkeytype>();
        //    _paxes.Childs_ = new List<passkeytype>();
        //    _paxes.Infant_ = new List<passkeytype>();
        //    for (int i = 0; i < travellers_.Count; i++)
        //    {
        //        if (travellers_[i].passengertypecode == "ADT")
        //            _paxes.Adults_.Add(travellers_[i]);
        //        else if (travellers_[i].passengertypecode == "CHD")
        //            _paxes.Childs_.Add(travellers_[i]);
        //        else if (travellers_[i].passengertypecode == "INFT")
        //            _paxes.Infant_.Add(travellers_[i]);

        //    }

        //    HttpContext.Session.SetString("PaxArray", JsonConvert.SerializeObject(_paxes));

        //    Passenger[] passengers = null;
        //    try
        //    {
        //        #region
        //        //int chdPax = 0;
        //        //int infFax = 0;
        //        //passengers = new Passenger[travellers_.Count]; //Assign Passenger Information 
        //        //Passenger p1 = null;
        //        //int PassCnt = 0;
        //        //for (int cntAdt = 0; cntAdt < travellers_.Count; cntAdt++)
        //        //{
        //        //    if (travellers_[cntAdt].passengertypecode.ToString().ToUpper() == "ADT")
        //        //    {

        //        //        p1 = new Passenger();
        //        //        p1.PassengerNumberSpecified = true;
        //        //        p1.PassengerNumber = Convert.ToInt16(PassCnt);
        //        //        p1.Names = new BookingName[1];
        //        //        p1.Names[0] = new BookingName();
        //        //        if (!string.IsNullOrEmpty(travellers_[cntAdt].first))
        //        //        {
        //        //            p1.Names[0].FirstName = Convert.ToString(travellers_[cntAdt].first.Trim()).ToUpper();
        //        //        }
        //        //        if (!string.IsNullOrEmpty(travellers_[cntAdt].middle))
        //        //        {
        //        //            p1.Names[0].MiddleName = Convert.ToString(travellers_[cntAdt].middle.Trim()).ToUpper();
        //        //        }
        //        //        if (!string.IsNullOrEmpty(travellers_[cntAdt].last))
        //        //        {
        //        //            p1.Names[0].LastName = Convert.ToString(travellers_[cntAdt].last.Trim()).ToUpper();
        //        //        }
        //        //        p1.Names[0].Title = travellers_[cntAdt].title.ToUpper().Replace(".", "");
        //        //        p1.PassengerInfo = new PassengerInfo();
        //        //        p1.PassengerInfo.Gender = Gender.Male;
        //        //        p1.PassengerInfo.WeightCategory = WeightCategory.Male;

        //        //        p1.PassengerTypeInfos = new PassengerTypeInfo[1];
        //        //        p1.PassengerTypeInfos[0] = new PassengerTypeInfo();
        //        //        p1.PassengerTypeInfos[0].PaxType = travellers_[cntAdt].passengertypecode.ToString().ToUpper();
        //        //        p1.PassengerTypeInfos[0].DOBSpecified = true;
        //        //        p1.PassengerTypeInfos[0].DOB = Convert.ToDateTime("0001-01-01T00:00:00");
        //        //    }

        //        //    if(travellers_[cntAdt].passengertypecode.ToString().ToUpper() == "CHD")
        //        //    {

        //        //        p1 = new Passenger();
        //        //        p1.PassengerNumberSpecified = true;
        //        //        p1.PassengerNumber = Convert.ToInt16(PassCnt);
        //        //        p1.Names = new BookingName[1];
        //        //        p1.Names[0] = new BookingName();
        //        //        if (!string.IsNullOrEmpty(travellers_[cntAdt].first))
        //        //        {
        //        //            p1.Names[0].FirstName = Convert.ToString(travellers_[cntAdt].first.Trim()).ToUpper();
        //        //        }
        //        //        if (!string.IsNullOrEmpty(travellers_[cntAdt].middle))
        //        //        {
        //        //            p1.Names[0].MiddleName = Convert.ToString(travellers_[cntAdt].middle.Trim()).ToUpper();
        //        //        }
        //        //        if (!string.IsNullOrEmpty(travellers_[cntAdt].last))
        //        //        {
        //        //            p1.Names[0].LastName = Convert.ToString(travellers_[cntAdt].last.Trim()).ToUpper();
        //        //        }
        //        //        p1.Names[0].Title = travellers_[cntAdt].title.ToUpper().Replace(".", "");
        //        //        p1.PassengerInfo = new PassengerInfo();
        //        //        p1.PassengerInfo.Gender = Gender.Male;
        //        //        p1.PassengerInfo.WeightCategory = WeightCategory.Male;

        //        //        p1.PassengerTypeInfos = new PassengerTypeInfo[1];
        //        //        p1.PassengerTypeInfos[0] = new PassengerTypeInfo();
        //        //        p1.PassengerTypeInfos[0].PaxType = travellers_[cntAdt].passengertypecode.ToString().ToUpper();
        //        //        p1.PassengerTypeInfos[0].DOBSpecified = true;
        //        //        p1.PassengerTypeInfos[0].DOB = Convert.ToDateTime("0001-01-01T00:00:00");


        //        //        //if (cntAdt < travellers_.InfantTraveller.Count)
        //        //        //{
        //        //        //    p1.Infant = new PassengerInfant();
        //        //        //    p1.Infant.DOB = travellers_.InfantTraveller[cntAdt].DOB;
        //        //        //    //p1.Infant.Gender = Gender.Male;
        //        //        //    if (GetGender(travellers_.InfantTraveller[cntAdt].Title.Replace(".", "")) == AirService.GENDER.MALE)
        //        //        //    {
        //        //        //        p1.Infant.Gender = Gender.Male;
        //        //        //    }
        //        //        //    else
        //        //        //    {
        //        //        //        p1.Infant.Gender = Gender.Female;
        //        //        //    }
        //        //        //    p1.Infant.Names = new BookingName[1];
        //        //        //    p1.Infant.Names[0] = new BookingName();
        //        //        //    if (!string.IsNullOrEmpty(travellers_.InfantTraveller[cntAdt].FirstName))
        //        //        //    {
        //        //        //        p1.Infant.Names[0].FirstName = Convert.ToString(travellers_.InfantTraveller[cntAdt].FirstName.Trim());
        //        //        //    }
        //        //        //    if (!string.IsNullOrEmpty(travellers_.InfantTraveller[cntAdt].MiddleName))
        //        //        //    {
        //        //        //        p1.Infant.Names[0].MiddleName = Convert.ToString(travellers_.InfantTraveller[cntAdt].MiddleName.Trim());
        //        //        //    }
        //        //        //    if (!string.IsNullOrEmpty(travellers_.InfantTraveller[cntAdt].LastName))
        //        //        //    {
        //        //        //        p1.Infant.Names[0].LastName = Convert.ToString(travellers_.InfantTraveller[cntAdt].LastName.Trim());
        //        //        //    }
        //        //        //    p1.Infant.Names[0].Title = travellers_.InfantTraveller[cntAdt].Title.Replace(".", "");
        //        //        //    p1.Infant.Nationality = travellers_.InfantTraveller[cntAdt].Nationality;
        //        //        //    p1.Infant.ResidentCountry = travellers_.InfantTraveller[cntAdt].ResidentCountry;
        //        //        //    p1.State = MessageState.New;
        //        //        //}

        //        //    }



        //        //    passengers[PassCnt] = p1;
        //        //    PassCnt++;
        //        //}
        //        #endregion

        //        int chdPax = 0;
        //        int infFax = 0;
        //        if (_paxes.Childs_ != null)
        //        {
        //            chdPax = _paxes.Childs_.Count;
        //        }
        //        if (_paxes.Infant_ != null)
        //        {
        //            infFax = _paxes.Infant_.Count;
        //        }
        //        passengers = new Passenger[_paxes.Adults_.Count + chdPax]; //Assign Passenger Information 
        //        Passenger p1 = null;
        //        int PassCnt = 0;
        //        for (int cntAdt = 0; cntAdt < _paxes.Adults_.Count; cntAdt++)
        //        {
        //            p1 = new Passenger();
        //            p1.PassengerNumberSpecified = true;
        //            p1.PassengerNumber = Convert.ToInt16(PassCnt);
        //            p1.Names = new BookingName[1];
        //            p1.Names[0] = new BookingName();
        //            if (!string.IsNullOrEmpty(_paxes.Adults_[cntAdt].first))
        //            {
        //                p1.Names[0].FirstName = Convert.ToString(_paxes.Adults_[cntAdt].first.Trim()).ToUpper();
        //            }
        //            if (!string.IsNullOrEmpty(_paxes.Adults_[cntAdt].middle))
        //            {
        //                p1.Names[0].MiddleName = Convert.ToString(_paxes.Adults_[cntAdt].middle.Trim()).ToUpper();
        //            }
        //            if (!string.IsNullOrEmpty(_paxes.Adults_[cntAdt].last))
        //            {
        //                p1.Names[0].LastName = Convert.ToString(_paxes.Adults_[cntAdt].last.Trim()).ToUpper();
        //            }
        //            p1.Names[0].Title = _paxes.Adults_[cntAdt].title.ToUpper().Replace(".", "");
        //            p1.PassengerInfo = new PassengerInfo();
        //            if (_paxes.Adults_[cntAdt].title.ToUpper().Replace(".", "") == "MR")
        //            {
        //                p1.PassengerInfo.Gender = Gender.Male;
        //                p1.PassengerInfo.WeightCategory = WeightCategory.Male;
        //            }
        //            else
        //            {
        //                p1.PassengerInfo.Gender = Gender.Female;
        //                p1.PassengerInfo.WeightCategory = WeightCategory.Female;
        //            }
        //            p1.PassengerTypeInfos = new PassengerTypeInfo[1];
        //            p1.PassengerTypeInfos[0] = new PassengerTypeInfo();
        //            p1.PassengerTypeInfos[0].DOBSpecified = true;
        //            p1.PassengerTypeInfos[0].PaxType = _paxes.Adults_[cntAdt].passengertypecode.ToString().ToUpper();
        //            if (_paxes.Infant_ != null && _paxes.Infant_.Count > 0)
        //            {
        //                if (cntAdt < _paxes.Infant_.Count)
        //                {
        //                    p1.Infant = new PassengerInfant();
        //                    p1.Infant.DOBSpecified = true;
        //                    p1.Infant.DOB = Convert.ToDateTime("2023-08-01") ;//Convert.ToDateTime(_paxes.Infant_[cntAdt].dateOfBirth);
        //                                                                       //p1.Infant.Gender = Gender.Male;
        //                    if (_paxes.Infant_[cntAdt].title.ToUpper().Replace(".", "") == "MR")
        //                    {
        //                        p1.Infant.Gender = Gender.Male;
        //                    }
        //                    else
        //                    {
        //                        p1.Infant.Gender = Gender.Female;
        //                    }
        //                    p1.Infant.Names = new BookingName[1];
        //                    p1.Infant.Names[0] = new BookingName();
        //                    if (!string.IsNullOrEmpty(_paxes.Infant_[cntAdt].first))
        //                    {
        //                        p1.Infant.Names[0].FirstName = Convert.ToString(_paxes.Infant_[cntAdt].first.Trim());
        //                    }
        //                    if (!string.IsNullOrEmpty(_paxes.Infant_[cntAdt].middle))
        //                    {
        //                        p1.Infant.Names[0].MiddleName = Convert.ToString(_paxes.Infant_[cntAdt].middle.Trim());
        //                    }
        //                    if (!string.IsNullOrEmpty(_paxes.Infant_[cntAdt].last))
        //                    {
        //                        p1.Infant.Names[0].LastName = Convert.ToString(_paxes.Infant_[cntAdt].last.Trim());
        //                    }
        //                    p1.Infant.Names[0].Title = _paxes.Infant_[cntAdt].title.Replace(".", "");
        //                    p1.Infant.Nationality = _paxes.Infant_[cntAdt].nationality;
        //                    p1.Infant.ResidentCountry = _paxes.Infant_[cntAdt].residentCountry;
        //                    p1.State = MessageState.New;
        //                }

        //            }

        //            passengers[PassCnt] = p1;
        //            PassCnt++;
        //        }
        //        if (_paxes.Childs_ != null)
        //        {
        //            for (int cntChd = 0; cntChd < _paxes.Childs_.Count; cntChd++)
        //            {
        //                p1 = new Passenger();

        //                p1.PassengerNumberSpecified = true;
        //                p1.PassengerNumber = Convert.ToInt16(PassCnt);
        //                p1.Names = new BookingName[1];
        //                p1.Names[0] = new BookingName();

        //                if (!string.IsNullOrEmpty(_paxes.Childs_[cntChd].first))
        //                {
        //                    p1.Names[0].FirstName = Convert.ToString(_paxes.Childs_[cntChd].first).ToUpper();
        //                }
        //                if (!string.IsNullOrEmpty(_paxes.Childs_[cntChd].middle))
        //                {
        //                    p1.Names[0].MiddleName = Convert.ToString(_paxes.Childs_[cntChd].middle).ToUpper();
        //                }
        //                if (!string.IsNullOrEmpty(_paxes.Childs_[cntChd].last))
        //                {
        //                    p1.Names[0].LastName = Convert.ToString(_paxes.Childs_[cntChd].last).ToUpper();
        //                }
        //                p1.Names[0].Title = _paxes.Childs_[cntChd].title.ToUpper().Replace(".", "");
        //                p1.PassengerInfo = new PassengerInfo();
        //                if (_paxes.Childs_[cntChd].title.ToUpper().Replace(".", "") == "Mr")
        //                {
        //                    p1.PassengerInfo.Gender = Gender.Male;
        //                    p1.PassengerInfo.WeightCategory = WeightCategory.Male;
        //                }
        //                else
        //                {
        //                    p1.PassengerInfo.Gender = Gender.Female;
        //                    p1.PassengerInfo.WeightCategory = WeightCategory.Female;
        //                }
        //                p1.PassengerTypeInfos = new PassengerTypeInfo[1];
        //                p1.PassengerTypeInfos[0] = new PassengerTypeInfo();
        //                p1.PassengerTypeInfos[0].DOBSpecified = true;
        //                p1.PassengerTypeInfos[0].PaxType = _paxes.Childs_[cntChd].passengertypecode.ToString().ToUpper();
        //                passengers[PassCnt] = p1;
        //                PassCnt++;
        //            }
        //        }
        //    }
        //    catch (SystemException sex_)
        //    {
        //    }
        //    return passengers;
        //}


    }



}





