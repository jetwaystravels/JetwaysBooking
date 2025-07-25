﻿using System;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
using static DomainLayer.Model.PassengersModel;
using static DomainLayer.Model.SeatMapResponceModel;
using static DomainLayer.Model.SSRAvailabiltyResponceModel;
//using static DomainLayer.Model.testseat;

namespace OnionConsumeWebAPI.Controllers
{
    public class AATripsellController : Controller
    {
        // string BaseURL = "https://dotrezapi.test.I5.navitaire.com";
        string token = string.Empty;
        string ssrKey = string.Empty;
        string journeyKey = string.Empty;
        string uniquekey = string.Empty;
        string MealsData = string.Empty;
        string Baggagesdata = string.Empty;
        private readonly IConfiguration _configuration;
        Logs logs = new Logs();


        public AATripsellController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> TripsellAsync(string Guid)
        {
            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);

            List<SelectListItem> Title = new()
            {
                new SelectListItem { Text = "Mr", Value = "Mr" },
                new SelectListItem { Text = "Ms" ,Value = "Ms" },
                new SelectListItem { Text = "Mrs", Value = "Mrs"},

            };

            ViewBag.Title = Title;
            var AirlineName = TempData["AirLineName"];
            ViewData["name"] = AirlineName;

            //Start:Coprate GST AND EMPLOYEE DETAIL
           
            //string employeeCode = "3";
            //string legalEntityCode = "23E008";
            int? airlineId = 6;

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

            //End:Coprate GST AND EMPLOYEE DETAIL

           
			MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
			seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(Guid, "AirAsia").Result;

			//string passenger = HttpContext.Session.GetString("keypassenger");
   //         string passengerInfant = HttpContext.Session.GetString("InfantData");
   //         string Seatmap = HttpContext.Session.GetString("Seatmap");
   //         string Meals = HttpContext.Session.GetString("Meals");
   //         string BaggageData = HttpContext.Session.GetString("BaggageDetails");
            ViewModel vm = new ViewModel();
            if (!string.IsNullOrEmpty(seatMealdetail.Infant))
            {
                AirAsiaTripResponceModel passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                AirAsiaTripResponceModel passeengerlistItanary = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Infant), typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                SSRAvailabiltyResponceModel BaggageDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Baggage), typeof(SSRAvailabiltyResponceModel));
                vm.passeengerlist = passeengerlist;
                vm.passeengerlistItanary = passeengerlistItanary;
                vm.Seatmaplist = Seatmaplist;
                vm.Meals = Mealslist;
                vm.Baggage = BaggageDetails;
            }
            else
            {
                AirAsiaTripResponceModel passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                SSRAvailabiltyResponceModel BaggageDataDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Baggage), typeof(SSRAvailabiltyResponceModel));
                vm.passeengerlist = passeengerlist;
                vm.Seatmaplist = Seatmaplist;
                vm.Meals = Mealslist;
                vm.Baggage = BaggageDataDetails;
            }
            return View(vm);
        }
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

			MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
			MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
			tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "AirAsia").Result;

			MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
			seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(GUID, "AirAsia").Result;

            LegalEntity legal = new LegalEntity();
            legal = _mongoDBHelper.GetlegalEntityByGUID(GUID).Result;

            //string passenger = HttpContext.Session.GetString("keypassenger");
            //         string passengerInfant = HttpContext.Session.GetString("keypassengerItanary");
            //         string Seatmap = HttpContext.Session.GetString("Seatmap");
            //         string Meals = HttpContext.Session.GetString("Meals");
            //         string BaggageData = HttpContext.Session.GetString("BaggageDetails");

            MongoHelper objMongoHelper = new MongoHelper();
			// string PassengerData = HttpContext.Session.GetString("PassengerName");
			string PassengerData = objMongoHelper.UnZip(tokenData.PassengerRequest);


			ViewModel vm = new ViewModel();
            AirAsiaTripResponceModel passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
            SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
            SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
            SSRAvailabiltyResponceModel BaggageDataDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Baggage), typeof(SSRAvailabiltyResponceModel));
            List<passkeytype> PassengerDataDetails = JsonConvert.DeserializeObject<List<passkeytype>>(PassengerData);

            vm.passeengerlist = passeengerlist;
            vm.Seatmaplist = Seatmaplist;
            vm.Meals = Mealslist;
            vm.Baggage = BaggageDataDetails;
            vm.passkeytype = PassengerDataDetails;
            return View(vm);
        }
        public async Task<IActionResult> ContectDetails(ContactModel contactobject, string GUID)
        {
            //string tokenview = HttpContext.Session.GetString("AirasiaTokan");
            //token = tokenview.Replace(@"""", string.Empty);

            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "AirAsia").Result;

            LegalEntity legal = new LegalEntity();
            legal = _mongoDBHelper.GetlegalEntityByGUID(GUID).Result;

            token = tokenData.Token;

            using (HttpClient client = new HttpClient())
            {
                ContactModel _ContactModel = new ContactModel();
                string countryCode = contactobject.countrycode;

                _ContactModel.emailAddress = contactobject.emailAddress;
                _Phonenumber Phonenumber = new _Phonenumber();
                List<_Phonenumber> Phonenumberlist = new List<_Phonenumber>();
                Phonenumber.type = "Home";
                Phonenumber.number = countryCode + "-" + contactobject.number;
                Phonenumberlist.Add(Phonenumber);
                _Phonenumber Phonenumber1 = new _Phonenumber();
                Phonenumber1.type = "Other";
                Phonenumber1.number = countryCode + "-" + contactobject.number;
                Phonenumberlist.Add(Phonenumber1);
                foreach (var item in Phonenumberlist)
                {
                    _ContactModel.phoneNumbers = Phonenumberlist;
                }
                _ContactModel.contactTypeCode = "p";
                _Address Address = new _Address();
                _ContactModel.address = Address;
                _Name Name = new _Name();
                Name.first = contactobject.first;
                Name.last = contactobject.last;
                Name.title = contactobject.title;
                _ContactModel.name = Name;
                var jsonContactRequest = JsonConvert.SerializeObject(_ContactModel, Formatting.Indented);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage responseAddContact = await client.PostAsJsonAsync(AppUrlConstant.AirasiaContactDetail, _ContactModel);
                if (responseAddContact.IsSuccessStatusCode)
                {
                    var _responseAddContact = responseAddContact.Content.ReadAsStringAsync().Result;
                    logs.WriteLogs(jsonContactRequest, "7-ADDContactRequest", "AirAsiaOneWay", "oneway");
                    logs.WriteLogs(_responseAddContact, "7-ADDContactResponse", "AirAsiaOneWay", "oneway");
                    //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.AirasiaContactDetail) + "Request:"+ jsonContactRequest + "\n\n Response: " + JsonConvert.SerializeObject(_responseAddContact), "ADDContact", "AirAsiaOneWay", "oneway");
                    //var JsonObjAddContact = JsonConvert.DeserializeObject<dynamic>(_responseAddContact);
                }

                contactobject.notificationPreference = token;
                contactobject.Guid = GUID;


			}
            return RedirectToAction("GetGstDetails", "AATripsell", contactobject);
        }
        public async Task<IActionResult> GetGstDetails(ContactModel contactobject, AddGSTInformation addGSTInformation)
        {
            //string tokenview = HttpContext.Session.GetString("AirasiaTokan");
            //token = tokenview.Replace(@"""", string.Empty);

            // string token = "";

            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            //MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

            //tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID("", "AirAsia").Result;

            LegalEntity legal = new LegalEntity();
            legal = _mongoDBHelper.GetlegalEntityByGUID(contactobject.Guid).Result;

            token = contactobject.notificationPreference;

            using (HttpClient client = new HttpClient())
            {
                string title = contactobject.title;
                string countryCode = contactobject.countrycode;
                TempData["CountryCodeAA"] = countryCode;
                AddGSTInformation addinformation = new AddGSTInformation();
                addinformation.contactTypeCode = "G";
                GSTPhonenumber Phonenumber = new GSTPhonenumber();
                List<GSTPhonenumber> Phonenumberlist = new List<GSTPhonenumber>();
                Phonenumber.type = "Other";
                Phonenumber.number = countryCode + contactobject.number; ;
                Phonenumberlist.Add(Phonenumber);

                foreach (var item in Phonenumberlist)
                {
                    addinformation.phoneNumbers = Phonenumberlist;
                }
                addinformation.cultureCode = "";
                GSTAddress Address = new GSTAddress();
                addinformation.Address = Address;
                addinformation.emailAddress = contactobject.emailAddressgst;
                addinformation.customerNumber = contactobject.customerNumber;
                addinformation.sourceOrganization = "";
                addinformation.distributionOption = "None";
                addinformation.notificationPreference = "None";
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
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage responseAddContact = await client.PostAsJsonAsync(AppUrlConstant.AirasiaGstDetail, addinformation);
                    if (responseAddContact.IsSuccessStatusCode)
                    {
                        var _responseAddContact = responseAddContact.Content.ReadAsStringAsync().Result;
                        //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.AirasiaGstDetail) + "Request: " + jsonContactRequest+  "\n\n Response: " + JsonConvert.SerializeObject(_responseAddContact), "GstDetails", "AirAsiaOneWay", "oneway");
                        logs.WriteLogs(jsonContactRequest, "8-GstDetailsRequest", "AirAsiaOneWay", "oneway");
                        logs.WriteLogs(_responseAddContact, "8-GstDetailsResponse", "AirAsiaOneWay", "oneway");
                        //var JsonObjAddContact = JsonConvert.DeserializeObject<dynamic>(_responseAddContact);
                    }
                }

            }

            return RedirectToAction("Tripsell", "AATripsell", new { Guid = contactobject.Guid });
        }
        public async Task<PartialViewResult> TravllerDetails(List<passkeytype> passengerdetails, string formattedDates, string GUID)
        {
            // string tokenview = HttpContext.Session.GetString("AirasiaTokan");

            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
			MongoHelper objMongoHelper = new MongoHelper();
			tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "AirAsia").Result;

            LegalEntity legal = new LegalEntity();
            legal = _mongoDBHelper.GetlegalEntityByGUID(GUID).Result;

            string tokenview = tokenData.Token;

            string[] dateStrings = JsonConvert.DeserializeObject<string[]>(formattedDates);
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(tokenview))
                {
                    token = tokenview.Replace(@"""", string.Empty);
                    PassengersModel _PassengersModel = new PassengersModel();
                    string CountryCode = TempData["CountryCodeAA"].ToString();
                    for (int i = 0; i < passengerdetails.Count; i++)
                    {
                        if (passengerdetails[i].passengertypecode == "INFT")
                            continue;
                        if (passengerdetails[i].passengertypecode != null)
                        {

                            Name name = new Name();
                            _Info Info = new _Info();
                            if (passengerdetails[i].title == "Mr" || passengerdetails[i].title == "MSTR")
                            {
                                Info.gender = "Male";
                            }
                            else
                            {
                                Info.gender = "Female";
                            }

                            name.title = passengerdetails[i].title;
                            name.first = passengerdetails[i].first;
                            name.last = passengerdetails[i].last;
                            name.mobile = CountryCode + passengerdetails[i].mobile;
                            name.middle = "";
                            Info.dateOfBirth = "";
                            Info.nationality = "IN";
                            Info.residentCountry = "IN";
                            _PassengersModel.name = name;
                            _PassengersModel.info = Info;

							
							string passobj = objMongoHelper.Zip(JsonConvert.SerializeObject(passengerdetails));
							_mongoDBHelper.UpdatePassengerMongoFlightToken(GUID, "AirAsia", passobj);

							// HttpContext.Session.SetString("PassengerName", JsonConvert.SerializeObject(passengerdetails));
							var jsonPassengers = JsonConvert.SerializeObject(_PassengersModel, Formatting.Indented);
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            HttpResponseMessage responsePassengers = await client.PutAsJsonAsync(AppUrlConstant.AirasiaAddPassenger + passengerdetails[i].passengerkey, _PassengersModel);
                            if (responsePassengers.IsSuccessStatusCode)
                            {
                                var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_PassengersModel) + "Url: " + AppUrlConstant.URLAirasia + "/api/nsk/v3/booking/passengers/" + passengerdetails[i].passengerkey + "\n Response: " + JsonConvert.SerializeObject(_responsePassengers), "Update passenger", "AirAsiaRT");
                                //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.URLAirasia + "/api/nsk/v3/booking/passengers/" + passengerdetails[i].passengerkey) +"Request:" + jsonPassengers + "\n\n Response: " + JsonConvert.SerializeObject(_responsePassengers), "ADDPassenger"+ i, "AirAsiaOneWay","oneway");
                                logs.WriteLogs(jsonPassengers, "9-ADDPassengerRequest" + i, "AirAsiaOneWay", "oneway");
                                logs.WriteLogs(_responsePassengers, "9-ADDPassengerResponse" + i, "AirAsiaOneWay", "oneway");

                                // var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                            }
                        }
                    }

                    int infantcount = 0;
                    for (int k = 0; k < passengerdetails.Count; k++)
                    {
                        if (passengerdetails[k].passengertypecode == "INFT")
                            infantcount++;

                    }
                    AddInFantModel _PassengersModel1 = new AddInFantModel();
                    for (int i = 0; i < passengerdetails.Count; i++)
                    {
                        if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD")
                            continue;
                        if (passengerdetails[i].passengertypecode == "INFT")
                        {
                            for (int k = 0; k < infantcount; k++)
                            {
                                _PassengersModel1.nationality = "IN";
                                //_PassengersModel1.dateOfBirth = "2023-10-01";
                                _PassengersModel1.dateOfBirth = dateStrings[k];
                                _PassengersModel1.residentCountry = "IN";
                                _Info Info = new _Info();
                                if (passengerdetails[i].title == "MSTR")
                                {
                                    Info.gender = "Male";
                                }
                                else
                                {
                                    Info.gender = "Female";
                                }
                                _PassengersModel1.gender = Info.gender;
                                InfantName nameINF = new InfantName();
                                nameINF.first = passengerdetails[i].first;
                                nameINF.middle = "";
                                nameINF.last = passengerdetails[i].last;
                                nameINF.title = passengerdetails[i].title;
                                nameINF.suffix = "";
                                _PassengersModel1.name = nameINF;


                                var jsonPassengers = JsonConvert.SerializeObject(_PassengersModel1, Formatting.Indented);
                                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.AirasiaAddPassenger + passengerdetails[k].passengerkey + "/infant", _PassengersModel1);
                                if (responsePassengers.IsSuccessStatusCode)
                                {
                                    var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                    //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_PassengersModel1) + "Url: " + AppUrlConstant.URLAirasia + "/api/nsk/v3/booking/passengers/" + passengerdetails[k].passengerkey + "/infant" + "\n Response: " + JsonConvert.SerializeObject(_responsePassengers), "ADD_Infant"+ k, "AirAsiaOneWay", "oneway");
                                    logs.WriteLogs(jsonPassengers, "10-ADD_InfantRequest" + k, "AirAsiaOneWay", "oneway");
                                    logs.WriteLogs(_responsePassengers, "10-ADD_InfantResponse" + k, "AirAsiaOneWay", "oneway");
                                    // var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                                }
                                i++;
                            }
                            // STRAT Get INFO
                            // var jsonPassengers = JsonConvert.SerializeObject(_PassengersModel1, Formatting.Indented);
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            HttpResponseMessage responceGetBooking = await client.GetAsync(AppUrlConstant.AirasiaGetBoking);
                            if (responceGetBooking.IsSuccessStatusCode)
                            {
                                var _responceGetBooking = responceGetBooking.Content.ReadAsStringAsync().Result;
                                //logs.WriteLogs("Request: " + JsonConvert.SerializeObject("GetBookingRequest") + "Url: " + AppUrlConstant.URLAirasia + "/api/nsk/v1/booking" + "\n Response: " + JsonConvert.SerializeObject(_responceGetBooking), "GetBooking", "AirAsiaOneWay", "oneway");
                                logs.WriteLogs("Url: " + AppUrlConstant.URLAirasia + "/api/nsk/v1/booking", "11-GetBookingRequest", "AirAsiaOneWay", "oneway");
                                logs.WriteLogs(_responceGetBooking, "11-GetBookingResponse", "AirAsiaOneWay", "oneway");
                                //var JsonObjGetBooking = JsonConvert.DeserializeObject<dynamic>(_responceGetBooking);
                            }
                        }
                    }
                    //HttpContext.Session.SetString("PassengerNameDetails", JsonConvert.SerializeObject(passengerdetails));
                }

				#region post data 

				MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
				seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(GUID, "AirAsia").Result;

				//string passenger = HttpContext.Session.GetString("keypassenger");
    //            //string passengerInfant = HttpContext.Session.GetString("keypassengerItanary");
    //            string Seatmap = HttpContext.Session.GetString("Seatmap");
    //            string Meals = HttpContext.Session.GetString("Meals");
    //            string BaggageData = HttpContext.Session.GetString("BaggageDetails");
				//string PassengerData = HttpContext.Session.GetString("PassengerName");

				string PassengerData = JsonConvert.SerializeObject(passengerdetails);


				ViewModel vm = new ViewModel();
                AirAsiaTripResponceModel passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                SSRAvailabiltyResponceModel BaggageDataDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Baggage), typeof(SSRAvailabiltyResponceModel));
                //passkeytype PassengerDataDetails = (passkeytype)JsonConvert.DeserializeObject(PassengerData, typeof(passkeytype));
                List<passkeytype> PassengerDataDetails = JsonConvert.DeserializeObject<List<passkeytype>>(PassengerData);
                vm.passeengerlist = passeengerlist;
                vm.Seatmaplist = Seatmaplist;
                vm.Meals = Mealslist;
                vm.Baggage = BaggageDataDetails;
                vm.passkeytype = PassengerDataDetails;
                #endregion
                #region Booking GET
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage responceGetBooking01 = await client.GetAsync(AppUrlConstant.AirasiaGetBoking);
                if (responceGetBooking01.IsSuccessStatusCode)
                {
                    // string BaseURL1 = "http://localhost:5225/";
                    var _responceGetBooking = responceGetBooking01.Content.ReadAsStringAsync().Result;
                    //logs.WriteLogs( "Url: " + AppUrlConstant.AirasiaGetBoking + "\n Response: " + JsonConvert.SerializeObject(_responceGetBooking), "GetBooking", "AirAsiaOneWay", "oneway");
                    logs.WriteLogs("Url: " + AppUrlConstant.AirasiaGetBoking, "12-GetBookingRequest", "AirAsiaOneWay", "oneway");
                    logs.WriteLogs(_responceGetBooking, "12-GetBookingResponse", "AirAsiaOneWay", "oneway");
                    //var JsonObjGetBooking = JsonConvert.DeserializeObject<dynamic>(_responceGetBooking);

                }
                #endregion

                return PartialView("_ServiceRequestsPartialView", vm);
                //return RedirectToAction("Tripsell", "AATripsell", passengerdetails);


            }
        }
        //public async Task<IActionResult> GetGstDetails01(AddGSTInformation addGSTInformation, string lineOne, string lineTwo, string city, string number, string postalCode)
        //{
        //    string tokenview = HttpContext.Session.GetString("AirasiaTokan");
        //    token = tokenview.Replace(@"""", string.Empty);

        //    using (HttpClient client = new HttpClient())
        //    {
        //        AddGSTInformation addinformation = new AddGSTInformation();


        //        addinformation.contactTypeCode = "G";

        //        GSTPhonenumber Phonenumber = new GSTPhonenumber();
        //        List<GSTPhonenumber> Phonenumberlist = new List<GSTPhonenumber>();
        //        Phonenumber.type = "Other";
        //        Phonenumber.number = number;
        //        Phonenumberlist.Add(Phonenumber);

        //        foreach (var item in Phonenumberlist)
        //        {
        //            addinformation.phoneNumbers = Phonenumberlist;
        //        }
        //        addinformation.cultureCode = "";
        //        GSTAddress Address = new GSTAddress();
        //        Address.lineOne = lineOne;
        //        Address.lineTwo = lineTwo;
        //        Address.lineThree = "";
        //        Address.countryCode = "IN";
        //        Address.provinceState = "TN";
        //        Address.city = city;
        //        Address.postalCode = postalCode;
        //        addinformation.Address = Address;

        //        addinformation.emailAddress = addGSTInformation.emailAddress;
        //        addinformation.customerNumber = addGSTInformation.customerNumber;
        //        addinformation.sourceOrganization = "";
        //        addinformation.distributionOption = "None";
        //        addinformation.notificationPreference = "None";
        //        addinformation.companyName = addGSTInformation.companyName;

        //        GSTName Name = new GSTName();
        //        Name.first = "Vadivel";
        //        Name.middle = "raja";
        //        Name.last = "VR";
        //        Name.title = "MR";
        //        Name.suffix = "";
        //        addinformation.Name = Name;

        //        var jsonContactRequest = JsonConvert.SerializeObject(addinformation, Formatting.Indented);
        //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //        HttpResponseMessage responseAddContact = await client.PostAsJsonAsync(AppUrlConstant.AirasiaGstDetail, addinformation);
        //        if (responseAddContact.IsSuccessStatusCode)
        //        {
        //            var _responseAddContact = responseAddContact.Content.ReadAsStringAsync().Result;
        //            var JsonObjAddContact = JsonConvert.DeserializeObject<dynamic>(_responseAddContact);
        //        }
        //    }
        //    return RedirectToAction("Tripsell", "AATripsell");
        //}
        public async Task<IActionResult> PostUnitkey(List<string> unitKey, List<string> mealssrKey, List<string> BaggageSSrkey, List<string> wheelSsrkey, string GUID)
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
            if (wheelSsrkey.Count > 0)
            {
                if (wheelSsrkey[0] == null)
                {
                    wheelSsrkey = new List<string>();
                }
            }


            //string tokenview = HttpContext.Session.GetString("AirasiaTokan");
            //token = tokenview.Replace(@"""", string.Empty);

            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(GUID, "AirAsia").Result;

            LegalEntity legal = new LegalEntity();
            legal = _mongoDBHelper.GetlegalEntityByGUID(GUID).Result;

            token = tokenData.Token;

            if (token == "" || token == null)
            {
                return RedirectToAction("Index");
            }
            using (HttpClient client = new HttpClient())
            {
				MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
				seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(GUID, "AirAsia").Result;
				MongoHelper objMongoHelper = new MongoHelper();

				//string passenger = HttpContext.Session.GetString("keypassenger");
    //            string Seatmap = HttpContext.Session.GetString("Seatmap");
    //            string Meals = HttpContext.Session.GetString("Meals");
    //            string BaggageData = HttpContext.Session.GetString("BaggageDetails");




                AirAsiaTripResponceModel passeengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.ResultRequest), typeof(AirAsiaTripResponceModel));
                SeatMapResponceModel Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.SeatMap), typeof(SeatMapResponceModel));
                SSRAvailabiltyResponceModel Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Meals), typeof(SSRAvailabiltyResponceModel));
                SSRAvailabiltyResponceModel BaggageDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(objMongoHelper.UnZip(seatMealdetail.Baggage), typeof(SSRAvailabiltyResponceModel));
                int passengerscount = passeengerKeyList.passengerscount;
                var data = Seatmaplist.datalist.Count;
                string legkey = passeengerKeyList.journeys[0].segments[0].legs[0].legKey;
                int Seatcount = unitKey.Count;
                if (Seatcount <= 0)
                {
                    //for (int i = 0; i < data; i++)
                    //{
                    //    for (int j = 0; j < passengerscount; j++)
                    //    {
                    //        string unitKey1 = string.Empty;
                    //        string passengerkey = passeengerKeyList.passengers[j].passengerKey;
                    //        string journeyKey = passeengerKeyList.journeys[0].journeyKey;
                    //        SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                    //        _SeatAssignmentModel.journeyKey = journeyKey;
                    //        var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                    //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    //        //HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(BaseURL + "/api/nsk/v2/booking/passengers/" + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                    //        HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AirasiaAutoSeat + passengerkey, _SeatAssignmentModel);

                    //        if (responceSeatAssignment.IsSuccessStatusCode)
                    //        {
                    //            var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                    //            var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                    //        }
                    //    }
                    //}
                    var mealcount = mealssrKey.Count;
                    if (mealcount > 0)
                    {
                        int mealid = 0;
                        int mealssr = Mealslist.legSsrs.Count;

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
                                    _sellSSRModel.note = "DevTest";
                                    _sellSSRModel.forceWaveOnSell = false;
                                    _sellSSRModel.currencyCode = "INR";

                                    var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AirasiaMealSelect + pas_SsrKey, _sellSSRModel);
                                    if (responseSellSSR.IsSuccessStatusCode)
                                    {
                                        var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                        logs.WriteLogs(jsonSellSSR, "13-SellSSSRmealReq" + l, "AirAsiaOneWay", "oneway");
                                        logs.WriteLogs(_responseresponseSellSSR, "13-SellSSSRmealRes" + l, "AirAsiaOneWay", "oneway");
                                        //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.URLAirasia + "/api/nsk/v2/booking/ssrs/" + pas_SsrKey) + "Request: " + jsonSellSSR + "\n\n Response: " + JsonConvert.SerializeObject(_responseresponseSellSSR), "SellSSSRmeal"+ l, "AirAsiaOneWay", "oneway");
                                        //var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
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
                    #region Baggage
                    var baggagecount = BaggageSSrkey.Count;
                    int baggageSsr = BaggageDetails.journeySsrsBaggage.Count;
                    if (baggagecount > 0)
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
                                    _sellSSRModel.note = "DevTest";
                                    _sellSSRModel.forceWaveOnSell = false;
                                    _sellSSRModel.currencyCode = "INR";
                                    var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AirasiaMealSelect + pas_BaggageSsrKey, _sellSSRModel);
                                    if (responseSellSSR.IsSuccessStatusCode)
                                    {
                                        var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                        //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.URLAirasia + "/api/nsk/v2/booking/ssrs/" + pas_BaggageSsrKey) + "Request: " + jsonSellSSR + "\n\n Response: " + JsonConvert.SerializeObject(_responseresponseSellSSR), "SellSSRBaggage"+ k, "AirAsiaOneWay", "oneway");
                                        logs.WriteLogs(jsonSellSSR, "14-SellSSRBaggageReq" + k, "AirAsiaOneWay", "oneway");
                                        logs.WriteLogs(_responseresponseSellSSR, "14-SellSSRBaggageRes" + k, "AirAsiaOneWay", "oneway");

                                        //var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                    }
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

                    #region WheelBaggage
                    //var WheelChaircount = wheelSsrkey.Count;
                    //int WheelChairbaggageSsr = BaggageDetails.journeySsrsBaggage.Count;
                    //if (WheelChaircount > 0)
                    //{
                    //    int WheelChairid = 0;
                    //    for (int k = 0; k < WheelChairbaggageSsr; k++)
                    //    {
                    //        for (int i = 0; i < passengerscount; i++)
                    //        {
                    //            if (WheelChairid < wheelSsrkey.Count) // Check if mealid is within bounds
                    //            {
                    //                string wheelChairKey = string.Empty;
                    //                wheelChairKey = wheelSsrkey[WheelChairid];
                    //                string[] WheelSSrKeyData = wheelChairKey.Split('_');
                    //                string pas_WheelSsrKey = WheelSSrKeyData[0];

                    //                SellSSRModel _sellSSRModel = new SellSSRModel();
                    //                _sellSSRModel.count = 1;
                    //                _sellSSRModel.note = "DevTest";
                    //                _sellSSRModel.forceWaveOnSell = false;
                    //                _sellSSRModel.currencyCode = "INR";
                    //                var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                    //                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    //                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    //                HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.URLAirasia + "/api/nsk/v2/booking/ssrs/" + pas_WheelSsrKey, _sellSSRModel);
                    //                if (responseSellSSR.IsSuccessStatusCode)
                    //                {
                    //                    var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                    //                    var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                    //                }
                    //                WheelChairid++;
                    //            }
                    //            else
                    //            {
                    //                break;
                    //            }

                    //        }
                    //    }
                    //}
                    #endregion


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
                                string passengerkey = passeengerKeyList.passengers[j].passengerKey;
                                string journeyKey = passeengerKeyList.journeys[0].journeyKey;
                                string pas_unitKeyS = unitKey[seatid];
                                string[] partsPas_unitKey = pas_unitKeyS.Split('_');
                                string pas_unitKey = partsPas_unitKey[1];
                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                _SeatAssignmentModel.journeyKey = journeyKey;
                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AirasiaSeatSelect + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                //HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(BaseURL + "/api/nsk/v1/booking/seats/auto/" + passengerkey, _SeatAssignmentModel);
                                if (responceSeatAssignment.IsSuccessStatusCode)
                                {
                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                    //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.AirasiaSeatSelect + passengerkey + "/seats/" + pas_unitKey) + "Request: " + jsonSeatAssignmentRequest + "\n\n Response: " + JsonConvert.SerializeObject(_responseSeatAssignment), "Seat Assignment "+ j, "AirAsiaOneWay", "oneway");
                                    logs.WriteLogs(jsonSeatAssignmentRequest, "15-SeatAssignmentReq" + j, "AirAsiaOneWay", "oneway");
                                    logs.WriteLogs(_responseSeatAssignment, "15-SeatAssignmentRes" + j, "AirAsiaOneWay", "oneway");

                                    //var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
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
                        int mealssr = Mealslist.legSsrs.Count;

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
                                    _sellSSRModel.note = "DevTest";
                                    _sellSSRModel.forceWaveOnSell = false;
                                    _sellSSRModel.currencyCode = "INR";

                                    var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AirasiaMealSelect + pas_SsrKey, _sellSSRModel);
                                    if (responseSellSSR.IsSuccessStatusCode)
                                    {
                                        var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                        //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.URLAirasia + "/api/nsk/v2/booking/ssrs/" + pas_SsrKey) + "Request: " + jsonSellSSR + "\n\n Response: " + JsonConvert.SerializeObject(_responseresponseSellSSR), "SellSSR Meal"+ l, "AirAsiaOneWay", "oneway");
                                        logs.WriteLogs(jsonSellSSR, "13-SellSSSRmealReq" + l, "AirAsiaOneWay", "oneway");
                                        logs.WriteLogs(_responseresponseSellSSR, "13-SellSSSRmealRes" + l, "AirAsiaOneWay", "oneway");

                                        //var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
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
                    #region Baggage
                    var baggagecount = BaggageSSrkey.Count;
                    int baggageSsr = BaggageDetails.journeySsrsBaggage.Count;
                    if (baggagecount > 0)
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
                                    _sellSSRModel.note = "DevTest";
                                    _sellSSRModel.forceWaveOnSell = false;
                                    _sellSSRModel.currencyCode = "INR";
                                    var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AirasiaMealSelect + pas_BaggageSsrKey, _sellSSRModel);
                                    if (responseSellSSR.IsSuccessStatusCode)
                                    {
                                        var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                        //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.URLAirasia + "/api/nsk/v2/booking/ssrs/" + pas_BaggageSsrKey) + "Request: " + jsonSellSSR + "\n\n Response: " + JsonConvert.SerializeObject(_responseresponseSellSSR), "SellSSR Baggage" + k, "AirAsiaOneWay", "oneway");
                                        logs.WriteLogs(jsonSellSSR, "14-SellSSRBaggageReq" + k, "AirAsiaOneWay", "oneway");
                                        logs.WriteLogs(_responseresponseSellSSR, "14-SellSSRBaggageRes" + k, "AirAsiaOneWay", "oneway");
                                        // var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                    }
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

                    #region WheelBaggage
                    var WheelChaircount = wheelSsrkey.Count;
                    int WheelChairbaggageSsr = BaggageDetails.journeySsrsBaggage.Count;
                    if (WheelChaircount > 0)
                    {
                        int WheelChairid = 0;
                        for (int k = 0; k < WheelChairbaggageSsr; k++)
                        {
                            for (int i = 0; i < passengerscount; i++)
                            {
                                if (WheelChairid < wheelSsrkey.Count) // Check if mealid is within bounds
                                {
                                    string wheelChairKey = string.Empty;
                                    wheelChairKey = wheelSsrkey[WheelChairid];
                                    string[] WheelSSrKeyData = wheelChairKey.Split('_');
                                    string pas_WheelSsrKey = WheelSSrKeyData[0];

                                    SellSSRModel _sellSSRModel = new SellSSRModel();
                                    _sellSSRModel.count = 1;
                                    _sellSSRModel.note = "DevTest";
                                    _sellSSRModel.forceWaveOnSell = false;
                                    _sellSSRModel.currencyCode = "INR";
                                    var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AirasiaMealSelect + pas_WheelSsrKey, _sellSSRModel);
                                    if (responseSellSSR.IsSuccessStatusCode)
                                    {
                                        var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                        //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.URLAirasia + "/api/nsk/v2/booking/ssrs/" + pas_WheelSsrKey) + "Request: " + jsonSellSSR + "\n\n Response: " + JsonConvert.SerializeObject(_responseresponseSellSSR), "SellWheelchairSSR", "AirAsiaOneWay", "oneway");
                                        logs.WriteLogs(jsonSellSSR, "15-SellWheelchairSSRReq", "AirAsiaOneWay", "oneway");
                                        logs.WriteLogs(_responseresponseSellSSR, "15-SellWheelchairSSRRes", "AirAsiaOneWay", "oneway");

                                        //var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                    }
                                    WheelChairid++;
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

                #region Booking GET
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage responceGetBooking01 = await client.GetAsync(AppUrlConstant.AirasiaGetBoking);
                if (responceGetBooking01.IsSuccessStatusCode)
                {

                    var _responceGetBooking = responceGetBooking01.Content.ReadAsStringAsync().Result;
                    //logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.AirasiaGetBoking) +  "\n\n Response: " + JsonConvert.SerializeObject(_responceGetBooking), "GetBooking", "AirAsiaOneWay", "oneway");
                    logs.WriteLogs("Url: " + JsonConvert.SerializeObject(AppUrlConstant.AirasiaGetBoking), "16-GetBookingReq", "AirAsiaOneWay", "oneway");
                    logs.WriteLogs(_responceGetBooking, "16-GetBookingRes", "AirAsiaOneWay", "oneway");

                    //var JsonObjGetBooking = JsonConvert.DeserializeObject<dynamic>(_responceGetBooking);

                }
                #endregion
            }

            return RedirectToAction("AirAsiaOneWayPaymentView", "AirAsiaOneWayPayment");
        }

    }
}









