using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Bookingmanager_;
using CoporateBooking.Models;
using DomainLayer.Model;
using DomainLayer.ViewModel;
using IndigoBookingManager_;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.IdentityModel.Tokens;
using Nancy;
using Nancy.Json;
using Nancy.Session;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using OnionConsumeWebAPI.Extensions;
using OnionConsumeWebAPI.Models;
using ServiceLayer.Service.Interface;
using Utility;
using ZXing.Aztec.Internal;
using static DomainLayer.Model.PassengersModel;
using static DomainLayer.Model.ReturnAirLineTicketBooking;
using static DomainLayer.Model.SeatMapResponceModel;
using static DomainLayer.Model.SSRAvailabiltyResponceModel;

namespace OnionConsumeWebAPI.Controllers.AkasaAir
{
    public class AKTripsellController : Controller
    {
        string token = string.Empty;
        string ssrKey = string.Empty;
        string journeyKey = string.Empty;
        Logs logs = new Logs();
        private readonly IConfiguration _configuration;

        public AKTripsellController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> AkTripsellView(string Guid)
        {
            List<SelectListItem> Title = new()
            {
                new SelectListItem { Text = "Mr", Value = "Mr" },
                new SelectListItem { Text = "Ms" ,Value = "Ms" },
                new SelectListItem { Text = "Mrs", Value = "Mrs"},

            };
            ViewBag.Title = Title;
            ViewModel vm = new ViewModel();

			MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
			MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
			MongoHelper objMongoHelper = new MongoHelper();
			seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(Guid, "Akasa").Result;


            int? airlineId = 5;

            _mongoDBHelper.UpdateSuppLegalEntity(Guid, Convert.ToString(airlineId.Value));

            LegalEntity legal = new LegalEntity();
            legal = _mongoDBHelper.GetlegalEntityByGUID(Guid).Result;

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

                catch when (Guid == null)
                {
                    // Handle the exception when Guid is null
                    // You can log the error or take appropriate action
                }
                catch
                {

                }



                ViewBag.GSTdata = gstList;
            }

            //var AKpassenger = HttpContext.Session.GetString("ResultFlightPassenger");
            //         var AkMeals = HttpContext.Session.GetString("AKMealsBaggage");
            //         var Akbaggage = HttpContext.Session.GetString("AKBaggageDetails");
            //         var AkSeatMap = HttpContext.Session.GetString("AKSeatmap");
            //         var AkItanary = HttpContext.Session.GetString("AkasaAirItanary");
            if (seatMealdetail.Infant != null)
            {

                AirAsiaTripResponceModel AkPassenger = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                SSRAvailabiltyResponceModel AkMealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                SSRAvailabiltyResponceModel AkBaggageDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Baggage), typeof(SSRAvailabiltyResponceModel));
                SeatMapResponceModel AkSeatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                AirAsiaTripResponceModel AkpasseengerItanary = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Infant), typeof(AirAsiaTripResponceModel));
                vm.AkPassenger = AkPassenger;
                vm.AkMealslist = AkMealslist;
                vm.AkBaggageDetails = AkBaggageDetails;
                vm.AkSeatmaplist = AkSeatmaplist;
                vm.AkpasseengerItanary = AkpasseengerItanary;
                return View(vm);
            }
            else
            {
                if (!string.IsNullOrEmpty(seatMealdetail.ResultRequest))
                {
                    AirAsiaTripResponceModel AkPassenger = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                    vm.AkPassenger = AkPassenger;
                }
                if (!string.IsNullOrEmpty(seatMealdetail.Meals))
                {
                    SSRAvailabiltyResponceModel AkMealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                    vm.AkMealslist = AkMealslist;
                }

                if (!string.IsNullOrEmpty(seatMealdetail.Baggage))
                {
                    SSRAvailabiltyResponceModel AkBaggageDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Baggage), typeof(SSRAvailabiltyResponceModel));
                    vm.AkBaggageDetails = AkBaggageDetails;

                }
                if (!string.IsNullOrEmpty(seatMealdetail.SeatMap))
                {
                    SeatMapResponceModel AkSeatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                    vm.AkSeatmaplist = AkSeatmaplist;
                }
                return View(vm);

            }
        }
        public IActionResult AkPostSeatMapdataView(string GUID)
        {
			MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
			MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
			MongoHelper objMongoHelper = new MongoHelper();

			tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "Akasa").Result;

			MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
			seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(GUID, "Akasa").Result;

			ViewModel vm = new ViewModel();
            //var AKpassenger = HttpContext.Session.GetString("ResultFlightPassenger");
            //var AkSeatMap = HttpContext.Session.GetString("AKSeatmap");
          //  var AkpassengerDetails = HttpContext.Session.GetString("AKPassengerName");
			var AkpassengerDetails = objMongoHelper.UnZip(tokenData.PassengerRequest);

			AirAsiaTripResponceModel AkPassenger = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
            SeatMapResponceModel AkSeatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
            List<passkeytype> passkeytypesDetails = JsonConvert.DeserializeObject<List<passkeytype>>(AkpassengerDetails);
            vm.AkPassenger = AkPassenger;
            vm.AkSeatmaplist = AkSeatmaplist;
            vm.passkeytype = passkeytypesDetails;
            return View(vm);
        }
        public async Task<IActionResult> AKContactDetails(ContactModel contactobject, string GUID)
        {
            //string tokenview = HttpContext.Session.GetString("AkasaTokan");
            //if (tokenview == null) { tokenview = ""; }
            //token = tokenview.Replace(@"""", string.Empty);

            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "Akasa").Result;

            token = tokenData.Token;

            using (HttpClient client = new HttpClient())
            {
                ContactModel _AkContactModel = new ContactModel();
                string countryCode = contactobject.countrycode;
                TempData["CountryCodeAK"] = countryCode;
                _AkContactModel.emailAddress = contactobject.emailAddress;
                _Phonenumber AkPhonenumber = new _Phonenumber();
                List<_Phonenumber> AkPhonenumberlist = new List<_Phonenumber>();
                AkPhonenumber.type = "Home";
                AkPhonenumber.number = countryCode + "-" + contactobject.number;
                AkPhonenumberlist.Add(AkPhonenumber);
                _Phonenumber AkPhonenumber1 = new _Phonenumber();
                AkPhonenumber1.type = "Other";
                AkPhonenumber1.number = countryCode + "-" + contactobject.number;
                AkPhonenumberlist.Add(AkPhonenumber1);
                foreach (var item in AkPhonenumberlist)
                {
                    _AkContactModel.phoneNumbers = AkPhonenumberlist;
                }
                _AkContactModel.contactTypeCode = "p";
                _Address AkAddress = new _Address();
                _AkContactModel.address = AkAddress;
                _Name AkName = new _Name();
                AkName.first = contactobject.first;
                AkName.last = contactobject.last;
                AkName.title = contactobject.title;
                _AkContactModel.name = AkName;
                var jsonAkContactRequest = JsonConvert.SerializeObject(_AkContactModel, Formatting.Indented);
                logs.WriteLogs(jsonAkContactRequest, "7-ADDContactRequest", "AkasaOneWay", "oneway");
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage responseAkAddContact = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirContactDetails, _AkContactModel);
                if (responseAkAddContact.IsSuccessStatusCode)
                {
                    var _responseAkAddContact = responseAkAddContact.Content.ReadAsStringAsync().Result;
                   
                    logs.WriteLogs(_responseAkAddContact, "7-ADDContactResponse", "AkasaOneWay", "oneway");
                    //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_AkContactModel) + "Url: " + "\n\n Response: " + JsonConvert.SerializeObject(_responseAkAddContact), "Contact", "AkasaOneWay", "oneway");
                    //var JsonObjAddContact = JsonConvert.DeserializeObject<dynamic>(_responseAkAddContact);
                }
                else
                {
                    var _responseexeception = responseAkAddContact.Content.ReadAsStringAsync().Result;
                    logs.WriteLogs(_responseexeception, "7-ADDContactResponse", "AkasaOneWay", "oneway");
                }
                contactobject.notificationPreference = token;
                contactobject.Guid = GUID;

			}

            //return RedirectToAction("AkTripsellView", "AKTripsell");
            return RedirectToAction("_RGetGstDetails", "AKTripsell", contactobject);

        }

        // Code For GST 
        public async Task<IActionResult> _RGetGstDetails(ContactModel contactobject)
        {
            //string tokenview = string.Empty;
            //tokenview = HttpContext.Session.GetString("AkasaTokan");
            //if (tokenview == null) { tokenview = ""; }
            //token = tokenview.Replace(@"""", string.Empty);

			token = contactobject.notificationPreference;

			using (HttpClient client = new HttpClient())
            {
                string title = contactobject.title;
                string countryCode = contactobject.countrycode;
                AddGSTInformation addinformation = new AddGSTInformation();
                addinformation.contactTypeCode = "G";
                GSTPhonenumber Phonenumber = new GSTPhonenumber();
                List<GSTPhonenumber> Phonenumberlist = new List<GSTPhonenumber>();
                Phonenumber.type = "Other";
                Phonenumber.number = countryCode + contactobject.number;
                Phonenumberlist.Add(Phonenumber);

                foreach (var item in Phonenumberlist)
                {
                    addinformation.phoneNumbers = Phonenumberlist;
                }
                addinformation.cultureCode = "";
                GSTAddress Address = new GSTAddress();
                Address.lineOne = "Ashokenagar,bharathi cross str";
                Address.countryCode = "IN";
                Address.provinceState = "TN";
                Address.city = "Ashokenagar";
                Address.postalCode = "400006";
                addinformation.Address = Address;

                addinformation.emailAddress = contactobject.emailAddressgst;
                addinformation.customerNumber = contactobject.customerNumber;
                //addinformation.sourceOrganization = "QPCCJ5003C";
                addinformation.distributionOption = null;
                addinformation.notificationPreference = null;
                addinformation.companyName = contactobject.companyName;
                GSTName Name = new GSTName();
                Name.first = contactobject.first;
                Name.last = contactobject.last;
                Name.title = title;
                Name.suffix = "";
                addinformation.Name = Name;
                if (contactobject.companyName != null)
                {
                    var jsonContactRequest = JsonConvert.SerializeObject(addinformation, Formatting.Indented);
                    logs.WriteLogs(jsonContactRequest, "8-GstDetailsRequest", "AkasaOneWay", "oneway");
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage responseAddContact = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirContactDetails, addinformation);
                    if (responseAddContact.IsSuccessStatusCode)
                    {
                        var _responseAddContact = responseAddContact.Content.ReadAsStringAsync().Result;
                       
                        logs.WriteLogs(_responseAddContact, "8-GstDetailsResponse", "AkasaOneWay", "oneway");
                        var JsonObjAddContact = JsonConvert.DeserializeObject<dynamic>(_responseAddContact);
                    }
                    else
                    {
                        var _responseAddContactexception = responseAddContact.Content.ReadAsStringAsync().Result;
                        logs.WriteLogs(_responseAddContactexception, "8-GstDetailsResponse", "AkasaOneWay", "oneway");
                    }
                }

            }

            return RedirectToAction("AkTripsellView", "AKTripsell", new { Guid = contactobject.Guid });
        }

        public async Task<IActionResult> AKTravellerInfo(List<passkeytype> passengerdetails, string formattedDates, string GUID)
        {
            // string tokenview = HttpContext.Session.GetString("AkasaTokan");



            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
			MongoHelper objMongoHelper = new MongoHelper();
			tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "Akasa").Result;

            string tokenview = tokenData.Token;

            string[] dateStrings = JsonConvert.DeserializeObject<string[]>(formattedDates);
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(tokenview))
                {
                    if (tokenview == null) { tokenview = ""; }
                    token = tokenData.Token;
                    PassengersModel _AkPassengersModel = new PassengersModel();
                    string CountryCode = TempData["CountryCodeAK"].ToString();
                    for (int i = 0; i < passengerdetails.Count; i++)
                    {
                        if (passengerdetails[i].passengertypecode == "INFT")
                            continue;
                        if (passengerdetails[i].passengertypecode != null)
                        {

                            Name Akname = new Name();
                            _Info AkInfo = new _Info();
                            if (passengerdetails[i].title == "Mr" || passengerdetails[i].title == "MSTR")
                            {
                                AkInfo.gender = "Male";
                            }
                            else
                            {
                                AkInfo.gender = "Female";
                            }
                            Akname.title = passengerdetails[i].title;
                            Akname.first = passengerdetails[i].first;
                            Akname.last = passengerdetails[i].last;
                            Akname.mobile = CountryCode + passengerdetails[i].mobile;
                            Akname.middle = "";
                            AkInfo.dateOfBirth = "";
                            AkInfo.nationality = "IN";
                            AkInfo.residentCountry = "IN";
                            _AkPassengersModel.name = Akname;
                            _AkPassengersModel.info = AkInfo;
							// HttpContext.Session.SetString("AKPassengerName", JsonConvert.SerializeObject(passengerdetails));

							string passobj = objMongoHelper.Zip(JsonConvert.SerializeObject(passengerdetails));
							_mongoDBHelper.UpdatePassengerMongoFlightToken(GUID, "Akasa", passobj);

							var jsonPassengers = JsonConvert.SerializeObject(_AkPassengersModel, Formatting.Indented);
                            logs.WriteLogs(jsonPassengers, "9-ADDPassengerRequest" + i, "AkasaOneWay", "oneway");
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            HttpResponseMessage AkresponsePassengers = await client.PutAsJsonAsync(AppUrlConstant.AkasaAirPassengerDetails + passengerdetails[i].passengerkey, _AkPassengersModel);
                            if (AkresponsePassengers.IsSuccessStatusCode)
                            {
                                var _responsePassengers = AkresponsePassengers.Content.ReadAsStringAsync().Result;
                                
                                logs.WriteLogs(_responsePassengers, "9-ADDPassengerResponse" + i, "AkasaOneWay", "oneway");
                                //var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                            }
                            else
                            {
                                var _responsePassengersexpetion = AkresponsePassengers.Content.ReadAsStringAsync().Result;
                                logs.WriteLogs(_responsePassengersexpetion, "9-ADDPassengerResponse" + i, "AkasaOneWay", "oneway");

                            }
                        }
                    }

                    int infantcount = 0;
                    for (int k = 0; k < passengerdetails.Count; k++)
                    {
                        if (passengerdetails[k].passengertypecode == "INFT")
                            infantcount++;

                    }
                    AddInFantModel _AkPassengersModel1 = new AddInFantModel();
                    for (int i = 0; i < passengerdetails.Count; i++)
                    {
                        if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD")
                            continue;
                        if (passengerdetails[i].passengertypecode == "INFT")
                        {
                            for (int k = 0; k < infantcount; k++)
                            {
                                _AkPassengersModel1.nationality = "IN";
                                //_PassengersModel1.dateOfBirth = "2023-10-01";
                                _AkPassengersModel1.dateOfBirth = dateStrings[k];
                                _AkPassengersModel1.residentCountry = "IN";
                                _Info Info = new _Info();
                                if (passengerdetails[i].title == "MSTR")
                                {
                                    Info.gender = "Male";
                                }
                                else
                                {
                                    Info.gender = "Female";
                                }
                                _AkPassengersModel1.gender = Info.gender;

                                InfantName AknameINF = new InfantName();
                                AknameINF.first = passengerdetails[i].first;
                                AknameINF.middle = "";
                                AknameINF.last = passengerdetails[i].last;
                                AknameINF.title = passengerdetails[i].title;
                                AknameINF.suffix = "";
                                _AkPassengersModel1.name = AknameINF;


                                var jsonPassengers = JsonConvert.SerializeObject(_AkPassengersModel1, Formatting.Indented);
                                logs.WriteLogs(jsonPassengers, "10-ADD_InfantRequest" + k, "AkasaOneWay", "oneway");
                                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirInfantDetails + passengerdetails[k].passengerkey + "/infant", _AkPassengersModel1);
                                //HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirInfantDetails , _AkPassengersModel1);
                                if (responsePassengers.IsSuccessStatusCode)
                                {
                                    var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                    //Logs logs = new Logs();
                                    //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_AkPassengersModel1) + "Url: " + AppUrlConstant.URLAirasia + "/api/nsk/v3/booking/passengers/" + passengerdetails[k].passengerkey + "/infant" + "\n Response: " + JsonConvert.SerializeObject(_responsePassengers), "Update passenger_Infant", "AkasaOneWay", "oneway");
                                   
                                    logs.WriteLogs(_responsePassengers, "10-ADD_InfantResponse" + k, "AkasaOneWay", "oneway");

                                    //var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                                }
                                else
                                {
                                    var _responsePassengersexception = responsePassengers.Content.ReadAsStringAsync().Result;
                                    logs.WriteLogs(_responsePassengersexception, "10-ADD_InfantResponse" + k, "AkasaOneWay", "oneway");

                                }
                                i++;
                            }

                        }
                    }
                }

				#region post data 

				MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
				seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(GUID, "Akasa").Result;

				ViewModel vm = new ViewModel();
                //var AKpassenger = HttpContext.Session.GetString("ResultFlightPassenger");
                //var AkMeals = HttpContext.Session.GetString("AKMealsBaggage");
                //var Akbaggage = HttpContext.Session.GetString("AKBaggageDetails");
                //var AkSeatMap = HttpContext.Session.GetString("AKSeatmap");
                //var AkpassengerDetails = HttpContext.Session.GetString("AKPassengerName");

                var AkpassengerDetails = JsonConvert.SerializeObject(passengerdetails);

				if (!string.IsNullOrEmpty(seatMealdetail.ResultRequest))
                {
                    AirAsiaTripResponceModel AkPassenger = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                    vm.AkPassenger = AkPassenger;
                }
                if (!string.IsNullOrEmpty(seatMealdetail.Meals))
                {
                    SSRAvailabiltyResponceModel AkMealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                    vm.AkMealslist = AkMealslist;
                }
                if (!string.IsNullOrEmpty(seatMealdetail.Baggage))
                {
                    SSRAvailabiltyResponceModel AkBaggageDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Baggage), typeof(SSRAvailabiltyResponceModel));
                    vm.AkBaggageDetails = AkBaggageDetails;
                }
                if (!string.IsNullOrEmpty(seatMealdetail.SeatMap))
                {
                    SeatMapResponceModel AkSeatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                    vm.AkSeatmaplist = AkSeatmaplist;
                }
                if (!string.IsNullOrEmpty(AkpassengerDetails))
                {
                    List<passkeytype> passkeytypesDetails = JsonConvert.DeserializeObject<List<passkeytype>>(AkpassengerDetails);
                    vm.passkeytype = passkeytypesDetails;
                }


                #endregion

                return PartialView("_AkServiceRequestsPartialView", vm);

            }

        }
        public async Task<IActionResult> PostSeatmapMealdata(List<string> unitKey, List<string> mealssrKey, List<string> BaggageSSrkey, string Guid)
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

            //if (unitKey.Count > 0)
            //{
            //    if (unitKey[0] == null)
            //    {
            //        unitKey = new List<string>();
            //    }
            //}

            if (mealssrKey.Count > 0)
            {
                if (mealssrKey[0] == null)
                {
                    mealssrKey = new List<string>();
                }
            }
            if (BaggageSSrkey.Count > 0)
            {
                if (BaggageSSrkey[0] == null)
                {
                    BaggageSSrkey = new List<string>();
                }
            }


            //string tokenview = HttpContext.Session.GetString("AkasaTokan");
            //if (tokenview == null) { tokenview = ""; }
            //token = tokenview.Replace(@"""", string.Empty);

            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
			MongoHelper objMongoHelper = new MongoHelper();

			tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;

            token = tokenData.Token;

            if (token == "" || token == null)
            {
                return RedirectToAction("Index");
            }
            using (HttpClient client = new HttpClient())
            {

				MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
				seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(Guid, "Akasa").Result;

				

				//var AKpassenger = HttpContext.Session.GetString("ResultFlightPassenger");
    //            var AkMeals = HttpContext.Session.GetString("AKMealsBaggage");
    //            var Akbaggage = HttpContext.Session.GetString("AKBaggageDetails");
    //            var AkSeatMap = HttpContext.Session.GetString("AKSeatmap");
				//  var AkpassengerDetails = HttpContext.Session.GetString("AKPassengerName");
				var AkpassengerDetails = objMongoHelper.UnZip(tokenData.PassengerRequest);

				AirAsiaTripResponceModel AkPassenger = null;
                SSRAvailabiltyResponceModel AkBaggageDetails = null;
                SeatMapResponceModel AkSeatmaplist = null;
                SSRAvailabiltyResponceModel AkMealslist = null;
                if (!string.IsNullOrEmpty(seatMealdetail.ResultRequest))
                {
                    AkPassenger = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                }
                if (!string.IsNullOrEmpty(seatMealdetail.Meals))
                {
                    AkMealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                }
                if (!string.IsNullOrEmpty(seatMealdetail.Baggage))
                {
                    AkBaggageDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Baggage), typeof(SSRAvailabiltyResponceModel));
                }
                if (!string.IsNullOrEmpty(seatMealdetail.SeatMap))
                {
                    AkSeatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                }
                int passengerscount = AkPassenger.passengerscount;
                var data = AkSeatmaplist.datalist.Count;
                string legkey = AkPassenger.journeys[0].segments[0].legs[0].legKey;
                int Seatcount = unitKey.Count;
                if (Seatcount <= 0)
                {
                    for (int i = 0; i < data; i++)
                    {
                        for (int j = 0; j < passengerscount; j++)
                        {
                            string unitKey1 = string.Empty;
                            string passengerkey = AkPassenger.passengers[j].passengerKey;
                            string journeyKey = AkPassenger.journeys[0].journeyKey;
                            SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                            _SeatAssignmentModel.journeyKey = journeyKey;
                            var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            //HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(BaseURL + "/api/nsk/v2/booking/passengers/" + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                            HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AirasiaAutoSeat + passengerkey, _SeatAssignmentModel);
                            if (responceSeatAssignment.IsSuccessStatusCode)
                            {
                                var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                logs.WriteLogs(jsonSeatAssignmentRequest, "11-AutoSeatReq" + j, "AkasaOneWay", "oneway");
                                logs.WriteLogs(_responseSeatAssignment, "11-AutoSeatRes" + j, "AkasaOneWay", "oneway");

                                //var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                            }
                        }
                    }
                    var mealcount = mealssrKey.Count;
                    if (mealcount > 0)
                    {
                        int mealid = 0;
                        int mealssr = AkMealslist.legSsrs.Count;

                        for (int k = 0; k < mealssr; k++)
                        {
                            for (int l = 0; l < passengerscount; l++)
                            {
                                if (mealid < mealssrKey.Count) // Check if mealid is within bounds
                                {
                                    string mealskey = string.Empty;
                                    mealskey = mealssrKey[mealid];
                                    string[] MealSSrKeyData = mealskey.Split('_');
                                    string pas_SsrKey = MealSSrKeyData[0];
                                    SellSSRModel _sellSSRModel = new SellSSRModel();
                                    _sellSSRModel.count = 1;
                                    _sellSSRModel.note = "PYOG";
                                    // _sellSSRModel.forceWaveOnSell = false;
                                    _sellSSRModel.currencyCode = "INR";
                                    // _sellSSRModel.ssrSellMode = 2;

                                    var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                    logs.WriteLogs(jsonSellSSR, "12-SellSSRReq" + l, "AkasaOneWay", "oneway");
                                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirMealBaggagePost + pas_SsrKey, _sellSSRModel);
                                    if (responseSellSSR.IsSuccessStatusCode)
                                    {
                                        //Logs logs = new Logs();
                                        var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                        //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_sellSSRModel) + "Url: " + AppUrlConstant.AkasaAirMealBaggagePost + "\n Response: " + JsonConvert.SerializeObject(_responseresponseSellSSR), "SellSSR", "AkasaOneWay", "oneway");
                                       
                                        logs.WriteLogs(_responseresponseSellSSR, "12-SellSSRRes" + l, "AkasaOneWay", "oneway");

                                        //var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                    }
                                    else
                                    {
                                        var _responseresponseSellSSRexception = responseSellSSR.Content.ReadAsStringAsync().Result;
                                        logs.WriteLogs(_responseresponseSellSSRexception, "12-SellSSRRes" + l, "AkasaOneWay", "oneway");

                                    }
                                    mealid++;
                                }
                                else
                                {

                                    break;
                                }
                            }
                        }

                    }

                }
                else
                {
                    int seatid = 0;
                    for (int i = 0; i < data; i++)
                    {
                        for (int j = 0; j < passengerscount; j++)
                        {
                            if (seatid < unitKey.Count) // Check if mealid is within bounds
                            {
                                string unitKey1 = string.Empty;
                                string passengerkey = AkPassenger.passengers[j].passengerKey;
                                string journeyKey = AkPassenger.journeys[0].journeyKey;
                                string pas_unitKeyS = unitKey[seatid];
                                string[] partsPas_unitKey = pas_unitKeyS.Split('_');
                                string pas_unitKey = partsPas_unitKey[1];
                                Logs logs = new Logs();
                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                _SeatAssignmentModel.journeyKey = journeyKey;
                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                logs.WriteLogs(jsonSeatAssignmentRequest, "13-SeatAssignReq" + j, "AkasaOneWay", "oneway");
                                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirSeatAssign + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                if (responceSeatAssignment.IsSuccessStatusCode)
                                {
                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                    //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_SeatAssignmentModel) + "Url: " + AppUrlConstant.AkasaAirMealSeatAssign + passengerkey + "/seats/" + pas_unitKey + "\n Response: " + JsonConvert.SerializeObject(_responseSeatAssignment), "SeatAssign", "AkasaOneWay", "oneway");
                                   
                                    logs.WriteLogs(_responseSeatAssignment, "13-SeatAssignRes" + j, "AkasaOneWay", "oneway");

                                    //var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                                }
                                else
                                {
                                    var _responseSeatAssignmentexception = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                    logs.WriteLogs(_responseSeatAssignmentexception, "13-SeatAssignRes" + j, "AkasaOneWay", "oneway");

                                }
                                seatid++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    var mealcount = mealssrKey.Count;
                    if (mealcount > 0)
                    {
                        int mealid = 0;
                        int mealssr = AkMealslist.legSsrs.Count;
                        Logs logs = new Logs();
                        for (int k = 0; k < mealssr; k++)
                        {
                            for (int l = 0; l < passengerscount; l++)
                            {
                                if (mealid < mealssrKey.Count) // Check if mealid is within bounds
                                {
                                    string mealskey = string.Empty;
                                    mealskey = mealssrKey[mealid];
                                    string[] MealSSrKeyData = mealskey.Split('_');
                                    string pas_SsrKey = MealSSrKeyData[0];
                                    SellSSRModel _sellSSRModel = new SellSSRModel();
                                    _sellSSRModel.count = 1;
                                    _sellSSRModel.note = "PYOG";
                                    //_sellSSRModel.forceWaveOnSell = false;
                                    _sellSSRModel.currencyCode = "INR";
                                    //_sellSSRModel.ssrSellMode = 2;

                                    var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                    logs.WriteLogs(jsonSellSSR, "12-SellSSRReq" + l, "AkasaOneWay", "oneway");
                                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirMealBaggagePost + pas_SsrKey, _sellSSRModel);
                                    if (responseSellSSR.IsSuccessStatusCode)
                                    {
                                        var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                        //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_sellSSRModel) + "Url: " + AppUrlConstant.AkasaAirMealBaggagePost + pas_SsrKey + "\n Response: " + JsonConvert.SerializeObject(_responseresponseSellSSR), "SellSSr", "AkasaOneWay", "oneway");
                                       
                                        logs.WriteLogs(_responseresponseSellSSR, "12-SellSSRRes" + l, "AkasaOneWay", "oneway");


                                        //var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                    }
                                    else
                                    {
                                        var _responseresponseSellSSRexception = responseSellSSR.Content.ReadAsStringAsync().Result;
                                        logs.WriteLogs(_responseresponseSellSSRexception, "12-SellSSRRes" + l, "AkasaOneWay", "oneway");
                                    }
                                    mealid++;
                                }
                                else
                                {

                                    break;
                                }
                            }
                        }

                    }



                }
                #region Baggage
                //var baggagecount = BaggageSSrkey.Count;
                //int baggageSsr = BaggageDetails.journeySsrsBaggage.Count;
                int baggageSsr = BaggageSSrkey.Count;
                if (baggageSsr > 0)
                {
                    int baggageid = 0;
                    for (int k = 0; k < baggageSsr; k++)
                    {
                        for (int i = 0; i < passengerscount; i++)
                        {
                            if (baggageid < BaggageSSrkey.Count) // Check if mealid is within bounds
                            {


                                string BaggageKey = string.Empty;
                                BaggageKey = BaggageSSrkey[baggageid];
                                string[] BaggageSSrKeyData = BaggageKey.Split('_');
                                string pas_BaggageSsrKey = BaggageSSrKeyData[0];

                                SellSSRModel _sellSSRModel = new SellSSRModel();
                                _sellSSRModel.count = 1;
                                _sellSSRModel.note = "PYOG";
                                // _sellSSRModel.forceWaveOnSell = false;
                                _sellSSRModel.currencyCode = "INR";
                                var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                logs.WriteLogs(jsonSellSSR, "14-SellSSRBaggageReq" + k, "AirAsiaOneWay", "oneway");
                                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirMealBaggagePost + pas_BaggageSsrKey, _sellSSRModel);
                                if (responseSellSSR.IsSuccessStatusCode)
                                {
                                    var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                    //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.URLAirasia + "/api/nsk/v2/booking/ssrs/" + pas_BaggageSsrKey) + "Request: " + jsonSellSSR + "\n\n Response: " + JsonConvert.SerializeObject(_responseresponseSellSSR), "SellSSR Baggage" + k, "AirAsiaOneWay", "oneway");
                                   
                                    logs.WriteLogs(_responseresponseSellSSR, "14-SellSSRBaggageRes" + k, "AirAsiaOneWay", "oneway");
                                    // var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                }
                                else
                                {
                                    var _responseresponseSellSSRexception = responseSellSSR.Content.ReadAsStringAsync().Result;
                                    logs.WriteLogs(_responseresponseSellSSRexception, "14-SellSSRBaggageRes" + k, "AirAsiaOneWay", "oneway");

                                }
                                var errorResult = responseSellSSR.Content.ReadAsStringAsync().Result;
                                baggageid++;
                            }
                            else
                            {
                                break;
                            }

                        }
                    }
                }
                #endregion
            }
            return RedirectToAction("AkasaAirPaymentView", "AkasaAirPayment");

        }
    }

}
