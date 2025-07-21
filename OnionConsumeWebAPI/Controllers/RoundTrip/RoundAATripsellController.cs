using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bookingmanager_;
using DomainLayer.Model;
using DomainLayer.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Nancy.Session;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Packaging.Signing;
using OnionArchitectureAPI.Services.Indigo;
using OnionConsumeWebAPI.Extensions;
using Utility;
using static DomainLayer.Model.PassengersModel;
using static DomainLayer.Model.SeatMapResponceModel;
using static OnionConsumeWebAPI.Controllers.IndigoTripsellController;
using OnionConsumeWebAPI.Models;
using System;
using OnionArchitectureAPI.Services.Travelport;
using System.Text;
using System.Web;

namespace OnionConsumeWebAPI.Controllers.RoundTrip
{
    public class RoundAATripsellController : Controller
    {
        IHttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();
        string token = string.Empty;
        string tokenview = string.Empty;
        string ssrKey = string.Empty;
        string journeyKey = string.Empty;
        string uniquekey = string.Empty;
        AirAsiaTripResponceModel passeengerlist = null;
        Logs logs = new Logs();
        private readonly IConfiguration _configuration;

        public RoundAATripsellController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult RoundAATripsellView(string Guid, string Supp)
        {

            List<SelectListItem> Title = new()
            {
                new SelectListItem { Text = "Mr", Value = "Mr" },
                new SelectListItem { Text = "Ms" ,Value = "Ms" },
                new SelectListItem { Text = "Mrs", Value = "Mrs"},

            };
            ViewBag.Title = Title;

            MongoHelper objMongoHelper = new MongoHelper();


            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
            seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(Guid, "AirAsia").Result;


            //	string passengerInfant = HttpContext.Session.GetString("keypassengerItanary");

            string passengerInfant = "";
            if (seatMealdetail != null && seatMealdetail.Infant != null)
            {
                passengerInfant = objMongoHelper.UnZip(seatMealdetail.Infant);
            }

            AirAsiaTripResponceModel passeengerlistItanary = null;
            if (passengerInfant != null)
            {
                passeengerlistItanary = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passengerInfant, typeof(AirAsiaTripResponceModel));
            }
            ViewModel vm = new ViewModel();
            string test = string.Empty;
            string Meals = string.Empty;
            SSRAvailabiltyResponceModel Mealslist = null;
            SeatMapResponceModel Seatmaplist = null;
            string Seatmap = string.Empty;
            //  string passenger = HttpContext.Session.GetString("keypassenger");

            string passenger = "";

            vm.SeatmaplistRT = new List<SeatMapResponceModel>();
            vm.passeengerlistRT = new List<AirAsiaTripResponceModel>();
            vm.MealslistRT = new List<SSRAvailabiltyResponceModel>();
            vm.passeengerlistItanary = passeengerlistItanary;
            //string Passenegrtext = HttpContext.Session.GetString("Mainpassengervm");
            //string Seattext = HttpContext.Session.GetString("Mainseatmapvm");
            //string Mealtext = HttpContext.Session.GetString("Mainmealvm");
            string passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");

            string Passenegrtext = "";
            string Seattext = "";
            string Mealtext = "";

            if (seatMealdetail != null && seatMealdetail.KPassenger != null)
            {
                passenger = objMongoHelper.UnZip(seatMealdetail.KPassenger);
            }


            if (seatMealdetail != null && seatMealdetail.ResultRequest != null)
            {
                Passenegrtext = objMongoHelper.UnZip(seatMealdetail.ResultRequest);
            }

            if (seatMealdetail != null && seatMealdetail.SeatMap != null)
            {
                Seattext = objMongoHelper.UnZip(seatMealdetail.SeatMap);
            }

            if (seatMealdetail != null && seatMealdetail.MainMeals != null)
            {
                Mealtext = objMongoHelper.UnZip(seatMealdetail.MainMeals);
            }

            // if (Guid != null)
            //	{
            //		tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "GDS").Result;
            //		passengerNamedetails = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
            //	}


            #region 2


            if (!string.IsNullOrEmpty(Passenegrtext))
            {
                test = Passenegrtext;

                foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                {
                    passenger = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                    if (passenger != null)
                    {
                        passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                        vm.passeengerlistRT.Add(passeengerlist);
                    }
                }
            }
            if (seatMealdetail != null)
            {
                if (!string.IsNullOrEmpty(passengerNamedetails))
                {
                    List<passkeytype> passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                    vm.passengerNamedetails = passengerNamedetailsdata;
                }
            }
            else
            {
                passengerNamedetails = null;

            }

            if (!string.IsNullOrEmpty(Seattext))
            {
                test = Seattext;
                Seatmap = string.Empty;
                foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                {
                    Seatmap = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                    if (Seatmap != null)
                    {
                        Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
                        vm.SeatmaplistRT.Add(Seatmaplist);
                    }
                }
            }

            Meals = string.Empty;
            Mealslist = null;
            if (!string.IsNullOrEmpty(Mealtext))
            {
                test = Mealtext;
                foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                {
                    Meals = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                    if (Meals != null)
                    {
                        Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));
                        vm.MealslistRT.Add(Mealslist);
                    }
                }
            }

            #endregion

            if (vm.passeengerlistRT.Count > 1)
            {
                //    ViewBag.ErrorMessage = "";
                if (vm.passeengerlistRT[0].ErrorMsg != null || vm.passeengerlistRT[1].ErrorMsg!=null)
                {
                    if (!string.IsNullOrEmpty(vm.passeengerlistRT[0].ErrorMsg))
                    {
                        ViewBag.ErrorMessage = vm.passeengerlistRT[0].ErrorMsg;
                    }
                    else if (!string.IsNullOrEmpty(vm.passeengerlistRT[1].ErrorMsg))
                    {
                        ViewBag.ErrorMessage = vm.passeengerlistRT[1].ErrorMsg;
                    }
                    return View("service-error-msg");
                }
                else
                {
                    return View(vm);
                }

            }
            else if (vm.passeengerlistRT.Count == 1)
            {
                if (!string.IsNullOrEmpty(vm.passeengerlistRT[0].ErrorMsg))
                {
                    ViewBag.ErrorMessage = vm.passeengerlistRT[0].ErrorMsg;
                }
                return View("service-error-msg");
            }
            else
            {
                return View(vm);
            }
            //return View(vm);


        }
        public IActionResult RTPostSeatMapModaldataView(string Guid)
        {
            ViewModel vm = new ViewModel();
            string test = string.Empty;
            string Meals = string.Empty;
            SSRAvailabiltyResponceModel Mealslist = null;
            SeatMapResponceModel Seatmaplist = null;
            string Seatmap = string.Empty;
            vm.SeatmaplistRT = new List<SeatMapResponceModel>();
            vm.passeengerlistRT = new List<AirAsiaTripResponceModel>();
            vm.MealslistRT = new List<SSRAvailabiltyResponceModel>();

            List<SelectListItem> Title = new()
            {
                new SelectListItem { Text = "Mr", Value = "Mr" },
                new SelectListItem { Text = "Ms" ,Value = "Ms" },
                new SelectListItem { Text = "Mrs", Value = "Mrs"},

            };
            ViewBag.Title = Title;
            var AirlineName = TempData["AirLineName"];
            ViewData["name"] = AirlineName;

            //	string passengerInfant = HttpContext.Session.GetString("keypassengerItanary");
            //	string passenger = HttpContext.Session.GetString("keypassenger");
            //	string Passenegrtext = HttpContext.Session.GetString("Mainpassengervm");
            //	string Seattext = HttpContext.Session.GetString("Mainseatmapvm");
            //	string Mealtext = HttpContext.Session.GetString("Mainmealvm");
            //string passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");
            //	string BaggageDataR = HttpContext.Session.GetString("BaggageDetails");

            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
            seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(Guid, "AirAsia").Result;

            string passengerInfant = "";
            string passenger = "";
            string Passenegrtext = "";
            string Seattext = "";
            string Mealtext = "";
            string passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");
            string BaggageDataR = "";// HttpContext.Session.GetString("BaggageDetails");


            if (seatMealdetail != null)
            {
                passengerInfant = objMongoHelper.UnZip(seatMealdetail.KPassenger); // todo
                passenger = objMongoHelper.UnZip(seatMealdetail.KPassenger);
                Passenegrtext = objMongoHelper.UnZip(seatMealdetail.ResultRequest);
                Seattext = objMongoHelper.UnZip(seatMealdetail.SeatMap);
                Mealtext = objMongoHelper.UnZip(seatMealdetail.MainMeals);
                BaggageDataR = objMongoHelper.UnZip(seatMealdetail.Baggage);

                if (string.IsNullOrEmpty(passengerNamedetails))
                {
                    if (seatMealdetail.PSupp == "SpiceJet")
                    {
                        passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetailsSG");
                    }
                    else if (seatMealdetail.PSupp == "Indigo")
                    {
                        passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetailsIndigo");
                    }
                    else if (seatMealdetail.PSupp == "GDS")
                    {
                        MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "GDS").Result;
                        passengerNamedetails = objMongoHelper.UnZip(tokenData.OldPassengerRequest);

                    }
                }

            }

            //if (string.IsNullOrEmpty(passengerNamedetails))
            //         {
            //              passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetailsSG");
            //         }

            //if (string.IsNullOrEmpty(passengerNamedetails))
            //{
            //    passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetailsIndigo");
            //}

            #region 2
            if (!string.IsNullOrEmpty(Passenegrtext))
            {
                test = Passenegrtext;

                foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                {
                    passenger = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                    if (passenger != null)
                    {
                        passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                        vm.passeengerlistRT.Add(passeengerlist);
                    }
                }
            }
            if (!string.IsNullOrEmpty(BaggageDataR))
            {
                SSRAvailabiltyResponceModel RBaggageDataDetails = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(BaggageDataR, typeof(SSRAvailabiltyResponceModel));
                vm.Baggage = RBaggageDataDetails;
            }
            if (!string.IsNullOrEmpty(passengerNamedetails))
            {
                List<passkeytype> passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                vm.passengerNamedetails = passengerNamedetailsdata;
            }


            if (!string.IsNullOrEmpty(Seattext))
            {
                test = Seattext;

                Seatmap = string.Empty;
                foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                {
                    Seatmap = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                    if (Seatmap != null)
                    {
                        Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
                        vm.SeatmaplistRT.Add(Seatmaplist);
                    }
                }
            }

            Meals = string.Empty;
            Mealslist = null;
            if (!string.IsNullOrEmpty(Mealtext))
            {
                test = Mealtext;
                foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                {
                    Meals = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                    if (Meals != null)
                    {
                        Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));
                        vm.MealslistRT.Add(Mealslist);
                    }
                }
            }

            #endregion


            return View(vm);
        }

        public async Task<IActionResult> PostReturnContactData(ContactModel contactobject, AddGSTInformation addGSTInformation, string Guid)
        {

            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

            string contobj = objMongoHelper.Zip(JsonConvert.SerializeObject(contactobject));
            _mongoDBHelper.UpdateFlightTokenContact(Guid, "GDS", contobj);

            // HttpContext.Session.SetString("GDSContactdetails", JsonConvert.SerializeObject(contactobject));
            string SelectedAirlinedata = HttpContext.Session.GetString("SelectedAirlineName");
            string[] dataArray = JsonConvert.DeserializeObject<string[]>(SelectedAirlinedata);
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (dataArray[i] == null)
                    continue;
                string tokenview = string.Empty;


                if (string.IsNullOrEmpty(tokenview) && dataArray[i].ToLower() == "airasia")
                {
                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "AirAsia").Result;

                    if (i == 0)
                    {
                        token = tokenData.Token;
                        contactobject.notificationPreference = token;

                    }
                    else
                    {
                        token = tokenData.RToken;
                        contactobject.notificationPreferenceR = token;
                    }

                    using (HttpClient client = new HttpClient())
                    {
                        ContactModel _ContactModel = new ContactModel();
                        _ContactModel.emailAddress = contactobject.emailAddress;
                        _Phonenumber Phonenumber = new _Phonenumber();
                        List<_Phonenumber> Phonenumberlist = new List<_Phonenumber>();
                        Phonenumber.type = "Home";
                        Phonenumber.number = contactobject.countrycode + contactobject.number;
                        Phonenumberlist.Add(Phonenumber);
                        _Phonenumber Phonenumber1 = new _Phonenumber();
                        Phonenumber1.type = "Other";
                        Phonenumber1.number = contactobject.countrycode + contactobject.number;
                        Phonenumberlist.Add(Phonenumber1);
                        foreach (var item in Phonenumberlist)
                        {
                            _ContactModel.phoneNumbers = Phonenumberlist;
                        }
                        _ContactModel.contactTypeCode = "p";

                        _Address Address = new _Address();
                        Address.lineOne = "Barakhamba Road";
                        Address.countryCode = "IN";
                        Address.provinceState = "TN";
                        Address.city = "Dehli";
                        Address.postalCode = "110001";
                        _ContactModel.address = Address;

                        _Name Name = new _Name();
                        Name.first = contactobject.first;
                        Name.middle = "";
                        Name.last = contactobject.last;
                        Name.title = contactobject.title;
                        _ContactModel.name = Name;

                        var jsonContactRequest = JsonConvert.SerializeObject(_ContactModel, Formatting.Indented);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responseAddContact = await client.PostAsJsonAsync(AppUrlConstant.AirasiaContactDetail, _ContactModel);
                        if (responseAddContact.IsSuccessStatusCode)
                        {
                            var _responseAddContact = responseAddContact.Content.ReadAsStringAsync().Result;

                            if (i == 0)
                            {
                                logs.WriteLogsR(jsonContactRequest, "7-UpdateContactsRequest_Left", "AirAsiaRT");
                                logs.WriteLogsR(_responseAddContact, "7-UpdateContactsResponse_Left", "AirAsiaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(jsonContactRequest, "7-UpdateContactsRequest_Right", "AirAsiaRT");
                                logs.WriteLogsR(_responseAddContact, "7-UpdateContactsResponse_Right", "AirAsiaRT");
                            }
                            var JsonObjAddContact = JsonConvert.DeserializeObject<dynamic>(_responseAddContact);
                        }

                    }
                }

                tokenview = string.Empty;


                if (string.IsNullOrEmpty(tokenview) && dataArray[i].ToLower() == "akasaair")
                {
                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;

                    if (i == 0)
                    {
                        tokenview = tokenData.Token;
                        contactobject.notificationPreference = tokenview;
                    }
                    else
                    {
                        tokenview = tokenData.RToken;
                        contactobject.notificationPreferenceR = tokenview;
                    }


                    if (tokenview == null) { tokenview = ""; }
                    token = tokenview.Replace(@"""", string.Empty);
                    using (HttpClient client = new HttpClient())
                    {
                        ContactModel _ContactModel = new ContactModel();
                        _ContactModel.emailAddress = contactobject.emailAddress;
                        _ContactModel.customerNumber = null;
                        _ContactModel.sourceOrganization = "QPCCJ5003C";
                        _ContactModel.distributionOption = null;
                        _ContactModel.notificationPreference = null;
                        _ContactModel.companyName = contactobject.companyName;
                        _Phonenumber Phonenumber = new _Phonenumber();
                        List<_Phonenumber> Phonenumberlist = new List<_Phonenumber>();
                        Phonenumber.type = "Home";
                        Phonenumber.number = contactobject.countrycode + contactobject.number;
                        //Phonenumber.number = passengerdetails.mobile;
                        Phonenumberlist.Add(Phonenumber);
                        _Phonenumber Phonenumber1 = new _Phonenumber();
                        Phonenumber1.type = "Other";
                        Phonenumber1.number = contactobject.countrycode + contactobject.number;
                        Phonenumberlist.Add(Phonenumber1);
                        foreach (var item in Phonenumberlist)
                        {
                            _ContactModel.phoneNumbers = Phonenumberlist;
                        }
                        _ContactModel.contactTypeCode = "p";

                        _Address Address = new _Address();
                        Address.lineOne = "Barakhamba Road";
                        Address.countryCode = "IN";
                        Address.provinceState = "TN";
                        Address.city = "New Dehli";
                        Address.postalCode = "110001";
                        _ContactModel.address = Address;

                        _Name Name = new _Name();
                        Name.first = contactobject.first;
                        Name.middle = "";
                        Name.last = contactobject.last;
                        Name.title = contactobject.title;
                        _ContactModel.name = Name;

                        var jsonContactRequest = JsonConvert.SerializeObject(_ContactModel, Formatting.Indented);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responseAddContact = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirContactDetails, _ContactModel);
                        if (responseAddContact.IsSuccessStatusCode)
                        {
                            var _responseAddContact = responseAddContact.Content.ReadAsStringAsync().Result;
                            if (i == 0)
                            {
                                logs.WriteLogsR(jsonContactRequest, "7-UpdateContactsRequest_Left", "AkasaRT");
                                logs.WriteLogsR(_responseAddContact, "7-UpdateContactsResponse_Left", "AkasaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(jsonContactRequest, "7-UpdateContactsRequest_Right", "AkasaRT");
                                logs.WriteLogsR(_responseAddContact, "7-UpdateContactsResponse_Right", "AkasaRT");
                            }
                            var JsonObjAddContact = JsonConvert.DeserializeObject<dynamic>(_responseAddContact);
                        }

                    }
                }




                //SPICE JEt Return Contact APi Request
                string Signature = string.Empty;
                if (string.IsNullOrEmpty(Signature) && dataArray[i].ToLower() == "spicejet")
                {
                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "SpiceJet").Result;
                    if (i == 0)
                    {
                        Signature = tokenData.Token;
                    }
                    else
                    {
                        Signature = tokenData.RToken;
                    }

                    if (Signature == null) { Signature = ""; }
                    Signature = Signature.Replace(@"""", string.Empty);
                    UpdateContactsRequest _ContactModelSG = new UpdateContactsRequest();
                    _ContactModelSG.updateContactsRequestData = new UpdateContactsRequestData();
                    _ContactModelSG.Signature = Signature;
                    _ContactModelSG.ContractVersion = 420;
                    _ContactModelSG.updateContactsRequestData.BookingContactList = new BookingContact[1];
                    _ContactModelSG.updateContactsRequestData.BookingContactList[0] = new BookingContact();

                    if (contactobject.customerNumber != null && contactobject.customerNumber != "")
                    {
                        _ContactModelSG.updateContactsRequestData.BookingContactList[0].TypeCode = "G";
                        _ContactModelSG.updateContactsRequestData.BookingContactList[0].CompanyName = contactobject.companyName;
                        _ContactModelSG.updateContactsRequestData.BookingContactList[0].CustomerNumber = contactobject.customerNumber; //"22AAAAA0000A1Z5"; //GSTNumber Re_ Assistance required for SG API Integration\GST Logs.zip\GST Logs
                        _ContactModelSG.updateContactsRequestData.BookingContactList[0].EmailAddress = contactobject.emailAddressgst;
                    }
                    else
                    {
                        _ContactModelSG.updateContactsRequestData.BookingContactList[0].TypeCode = "P";
                        _ContactModelSG.updateContactsRequestData.BookingContactList[0].CountryCode = "IN";
                        _ContactModelSG.updateContactsRequestData.BookingContactList[0].HomePhone = contactobject.countrycode + contactobject.number;
                        _ContactModelSG.updateContactsRequestData.BookingContactList[0].EmailAddress = contactobject.emailAddress;
                        BookingName[] Name = new BookingName[1];
                        Name[0] = new BookingName();
                        Name[0].FirstName = contactobject.first;
                        Name[0].LastName = contactobject.last;
                        Name[0].Title = contactobject.title;
                        _ContactModelSG.updateContactsRequestData.BookingContactList[0].Names = Name;
                    }
                    SpiceJetApiController objSpiceJet = new SpiceJetApiController();
                    UpdateContactsResponse responseAddContactSG = await objSpiceJet.GetUpdateContactsAsync(_ContactModelSG);
                    HttpContext.Session.SetString("ContactDetails", JsonConvert.SerializeObject(_ContactModelSG));
                    string Str1 = JsonConvert.SerializeObject(responseAddContactSG);
                    logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_ContactModelSG) + "\n\n Response: " + JsonConvert.SerializeObject(responseAddContactSG), "UpdateContact", "SpiceJetRT");
                }


                if (string.IsNullOrEmpty(Signature) && dataArray[i].ToLower() == "indigo")
                {
                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Indigo").Result;
                    if (i == 0)
                    {
                        Signature = tokenData.Token;

                    }
                    else
                    {
                        Signature = tokenData.RToken;
                    }

                    if (Signature == null) { Signature = ""; }
                    _updateContact obj = new _updateContact(httpContextAccessorInstance);
                    IndigoBookingManager_.UpdateContactsResponse _responseAddContact6E = await obj.GetUpdateContacts(Signature, contactobject.emailAddress, contactobject.emailAddressgst, contactobject.number, contactobject.companyName, contactobject.customerNumber, contactobject.countrycode, contactobject.title, contactobject.first, contactobject.last, "");
                    string Str1 = JsonConvert.SerializeObject(_responseAddContact6E);
                }

            }

            return RedirectToAction("_RGetGstDetails", "RoundAATripsell", contactobject);
        }

        //GST Code For Akasa and Airasia
        public async Task<IActionResult> _RGetGstDetails(ContactModel contactobject, AddGSTInformation addGSTInformation)
        {

            string SelectedAirlinedata = HttpContext.Session.GetString("SelectedAirlineName");
            string[] dataArray = JsonConvert.DeserializeObject<string[]>(SelectedAirlinedata);
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (dataArray[i] == null) // Change for same Airline Roundtrip-26-09-2024
                    continue;
                string tokenview = string.Empty;
                if (i == 0)
                {
                    // tokenview = HttpContext.Session.GetString("AirasiaTokan");
                    tokenview = contactobject.notificationPreference;
                }
                else
                {
                    // tokenview = HttpContext.Session.GetString("AirasiaTokanR");
                    tokenview = contactobject.notificationPreferenceR;
                }

                if (!string.IsNullOrEmpty(tokenview) && dataArray[i].ToLower() == "airasia")
                {
                    //if (token == null) { token = ""; }
                    token = tokenview.Replace(@"""", string.Empty);


                    using (HttpClient client = new HttpClient())
                    {
                        AddGSTInformation addinformation = new AddGSTInformation();
                        addinformation.contactTypeCode = "G";
                        GSTPhonenumber Phonenumber = new GSTPhonenumber();
                        List<GSTPhonenumber> Phonenumberlist = new List<GSTPhonenumber>();
                        Phonenumber.type = "Other";
                        Phonenumber.number = contactobject.countrycode + contactobject.number; ;
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
                        Name.title = contactobject.title;
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
                                if (i == 0)
                                {
                                    logs.WriteLogsR(jsonContactRequest, "8-GstDetailRequest_Left", "AirAsiaRT");
                                    logs.WriteLogsR(_responseAddContact, "8-GstDetailResponse_Left", "AirAsiaRT");

                                }
                                else
                                {
                                    logs.WriteLogsR(jsonContactRequest, "8-GstDetailRequest_Right", "AirAsiaRT");
                                    logs.WriteLogsR(_responseAddContact, "8-GstDetailResponse_Right", "AirAsiaRT");
                                }
                                var JsonObjAddContact = JsonConvert.DeserializeObject<dynamic>(_responseAddContact);
                            }
                        }

                    }
                }

                //            tokenview = string.Empty;
                //            if (i == 0)
                //            {
                //               // tokenview = HttpContext.Session.GetString("AkasaTokan");
                //	tokenview = contactobject.notificationPreference;
                //}
                //            else
                //            {
                //               // tokenview = HttpContext.Session.GetString("AkasaTokanR");
                //	tokenview = contactobject.notificationPreferenceR;
                //}

                if (!string.IsNullOrEmpty(tokenview) && dataArray[i].ToLower() == "akasaair")
                {
                    //if (token == null) { token = ""; }
                    token = tokenview.Replace(@"""", string.Empty);
                    using (HttpClient client = new HttpClient())
                    {
                        AddGSTInformation addinformation = new AddGSTInformation();
                        addinformation.contactTypeCode = "G";
                        GSTPhonenumber Phonenumber = new GSTPhonenumber();
                        List<GSTPhonenumber> Phonenumberlist = new List<GSTPhonenumber>();
                        Phonenumber.type = "Other";
                        Phonenumber.number = contactobject.countrycode + contactobject.number; ;
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
                        addinformation.distributionOption = null;
                        addinformation.notificationPreference = null;
                        addinformation.companyName = contactobject.companyName;
                        GSTName Name = new GSTName();
                        Name.first = contactobject.first;
                        Name.last = contactobject.last;
                        Name.title = contactobject.title;
                        Name.suffix = "";
                        addinformation.Name = Name;
                        if (contactobject.companyName != null)
                        {
                            var jsonContactRequest = JsonConvert.SerializeObject(addinformation, Formatting.Indented);
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            HttpResponseMessage responseAddContact = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirContactDetails, addinformation);
                            if (responseAddContact.IsSuccessStatusCode)
                            {
                                var _responseAddContact = responseAddContact.Content.ReadAsStringAsync().Result;
                                if (i == 0)
                                {
                                    logs.WriteLogsR(jsonContactRequest, "8-GstDetailRequest_Left", "AkasaRT");
                                    logs.WriteLogsR(_responseAddContact, "8-GstDetailResponse_Left", "AkasaRT");

                                }
                                else
                                {
                                    logs.WriteLogsR(jsonContactRequest, "8-GstDetailRequest_Right", "AkasaRT");
                                    logs.WriteLogsR(_responseAddContact, "8-GstDetailResponse_Right", "AkasaRT");
                                }
                                var JsonObjAddContact = JsonConvert.DeserializeObject<dynamic>(_responseAddContact);
                            }
                        }

                    }
                }
            }
            return RedirectToAction("RoundAATripsellView", "RoundAATripsell", new { Guid = contactobject.Guid });
        }
        [HttpPost]
        public async Task<IActionResult> PostReturnTravllerData(List<passkeytype> passengerdetails, List<Infanttype> infanttype, string Guid)
        {
            // HttpContext.Session.SetString("newPassengerdetails", JsonConvert.SerializeObject(passengerdetails));
            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();



            SSRAvailabiltyResponceModel Mealslist = null;
            SSRAvailabiltyResponceModel Bagslist = null;
            SeatMapResponceModel Seatmaplist = null;
            ViewModel vm = new ViewModel();
            List<passkeytype> passengerNamedetailsdataL = null;
            List<passkeytype> passengerNamedetailsdataR = null;
            List<passkeytype> passengerNamedetailsdata = null;
            List<string> MainBaggagedata = new List<string>();
            string passobj = string.Empty;
            string SelectedAirlinedata = HttpContext.Session.GetString("SelectedAirlineName");
            string[] dataArray = JsonConvert.DeserializeObject<string[]>(SelectedAirlinedata);
            for (int i1 = 0; i1 < dataArray.Length; i1++)
            {
                string tokenview = "";
                string _data = JsonConvert.SerializeObject(passengerdetails);
                List<passkeytype> _newPassengerdetailsSG = (List<passkeytype>)JsonConvert.DeserializeObject(_data, typeof(List<passkeytype>));

                if (dataArray[i1] == null)
                    continue;
                using (HttpClient client = new HttpClient())
                {
                    if (string.IsNullOrEmpty(tokenview) && dataArray[i1].ToLower() == "airasia")
                    {
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "AirAsia").Result;

                        if (i1 == 0)
                        {
                            tokenview = tokenData.Token;
                        }
                        else
                        {
                            tokenview = tokenData.RToken;
                        }
                        token = tokenview;
                        PassengersModel _PassengersModel = new PassengersModel();
                        for (int i = 0; i < passengerdetails.Count; i++)
                        {
                            string _airlinename = string.Empty;
                            string[] arraypaxkey = passengerdetails[i].passengerkey.Split('@');
                            if (arraypaxkey.Length > 1)
                            {
                                string[] arraypaxdata = arraypaxkey[0].Split('^');
                                if (arraypaxdata.Length > 1)
                                {
                                    _airlinename = arraypaxdata[1];
                                }
                                if (dataArray[i1].ToLower() == _airlinename.ToLower())
                                {
                                    passengerdetails[i].passengerkey = arraypaxdata[0];
                                }
                                else
                                {
                                    arraypaxdata = arraypaxkey[1].Split('^');
                                    _airlinename = arraypaxdata[1];
                                    if (dataArray[i1].ToLower() == _airlinename.ToLower())
                                    {
                                        passengerdetails[i].passengerkey = arraypaxdata[0];
                                    }

                                }
                            }
                            if (passengerdetails[i].passengertypecode == "INFT" || passengerdetails[i].passengertypecode == "INF")
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
                                name.middle = "";
                                Info.dateOfBirth = "";
                                Info.nationality = "IN";
                                Info.residentCountry = "IN";
                                _PassengersModel.name = name;
                                _PassengersModel.info = Info;
                                var jsonPassengers = JsonConvert.SerializeObject(_PassengersModel, Formatting.Indented);
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                HttpResponseMessage responsePassengers = await client.PutAsJsonAsync(AppUrlConstant.AirasiaAddPassenger + passengerdetails[i].passengerkey, _PassengersModel);
                                if (responsePassengers.IsSuccessStatusCode)
                                {
                                    var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                    if (i == 0)
                                    {
                                        logs.WriteLogsR(jsonPassengers, "9-UpdatepassengerRequest_Left", "AirAsiaRT");
                                        logs.WriteLogsR(_responsePassengers, "9-UpdatepassengerResponse_Left", "AirAsiaRT");

                                    }
                                    else
                                    {
                                        logs.WriteLogsR(jsonPassengers, "9-UpdatepassengerRequest_Right", "AirAsiaRT");
                                        logs.WriteLogsR(_responsePassengers, "9-UpdatepassengerResponse_Right", "AirAsiaRT");
                                    }
                                    var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                                }
                            }
                        }

                        int infantcount = 0;
                        for (int k = 0; k < passengerdetails.Count; k++)
                        {
                            if (passengerdetails[k].passengertypecode == "INFT" || passengerdetails[k].passengertypecode == "INF")
                                infantcount++;

                        }

                        AddInFantModel _PassengersModel1 = new AddInFantModel();
                        for (int i = 0; i < passengerdetails.Count; i++)
                        {
                            if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD")
                                continue;
                            if (passengerdetails[i].passengertypecode == "INFT" || passengerdetails[i].passengertypecode == "INF")
                            {
                                for (int k = 0; k < infantcount; k++)
                                {


                                    _PassengersModel1.nationality = "IN";
                                    _PassengersModel1.dateOfBirth = passengerdetails[i].dateOfBirth;
                                    _PassengersModel1.residentCountry = "IN";
                                    _PassengersModel1.gender = "Male";

                                    InfantName nameINF = new InfantName();
                                    nameINF.first = passengerdetails[i].first;
                                    nameINF.middle = "";
                                    nameINF.last = passengerdetails[i].last;
                                    nameINF.title = passengerdetails[i].title;
                                    nameINF.suffix = "";
                                    _PassengersModel1.name = nameINF;


                                    var jsonPassengers = JsonConvert.SerializeObject(_PassengersModel1, Formatting.Indented);
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.AirasiaAddPassenger + passengerdetails[k].passengerkey + "/infant", _PassengersModel1);
                                    if (responsePassengers.IsSuccessStatusCode)
                                    {
                                        var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                        if (i == 0)
                                        {
                                            logs.WriteLogsR(jsonPassengers, "10-Updatepassenger_infantRequest_Left", "AirAsiaRT");
                                            logs.WriteLogsR(_responsePassengers, "10-Updatepassenger_InfantResponse_Left", "AirAsiaRT");

                                        }
                                        else
                                        {
                                            logs.WriteLogsR(jsonPassengers, "10-Updatepassenger_InfantRequest_Right", "AirAsiaRT");
                                            logs.WriteLogsR(_responsePassengers, "10-Updatepassenger_InfantResponse_Right", "AirAsiaRT");
                                        }
                                        var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                                    }
                                    i++;
                                }

                                // STRAT Get INFO
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                HttpResponseMessage responceGetBooking = await client.GetAsync(AppUrlConstant.AirasiaGetBoking);
                                if (responceGetBooking.IsSuccessStatusCode)
                                {
                                    var _responceGetBooking = responceGetBooking.Content.ReadAsStringAsync().Result;
                                    if (i == 0)
                                    {
                                        logs.WriteLogsR(AppUrlConstant.URLAirasia + "/api/nsk/v1/booking", "11-GetBookingRequest_Left", "AirAsiaRT");
                                        logs.WriteLogsR(_responceGetBooking, "11-GetBookingResponse_Left", "AirAsiaRT");

                                    }
                                    else
                                    {
                                        logs.WriteLogsR(AppUrlConstant.URLAirasia + "/api/nsk/v1/booking", "11-GetBookingRequest_Right", "AirAsiaRT");
                                        logs.WriteLogsR(_responceGetBooking, "11-GetBookingResponse_Right", "AirAsiaRT");
                                    }
                                    var JsonObjGetBooking = JsonConvert.DeserializeObject<dynamic>(_responceGetBooking);
                                }
                            }
                        }


                        //SpiceJet Passenger DEtails Round Trip Request API
                        HttpContext.Session.SetString("PassengerNameDetails", JsonConvert.SerializeObject(passengerdetails));
                    }

                    // Akasa trevvel Details Request ********
                    if (string.IsNullOrEmpty(tokenview) && dataArray[i1].ToLower() == "akasaair")
                    {

                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;
                        if (i1 == 0)
                        {
                            tokenview = tokenData.Token;
                        }
                        else
                        {
                            tokenview = tokenData.RToken;
                        }
                        if (tokenview == null) { tokenview = ""; }
                        token = tokenview.Replace(@"""", string.Empty);
                        PassengersModel _PassengersModel = new PassengersModel();
                        for (int i = 0; i < passengerdetails.Count; i++)
                        {
                            string _airlinename = string.Empty;
                            string[] arraypaxkey = passengerdetails[i].passengerkey.Split('@');
                            if (arraypaxkey.Length > 1)
                            {
                                string[] arraypaxdata = arraypaxkey[0].Split('^');
                                if (arraypaxdata.Length > 1)
                                {
                                    _airlinename = arraypaxdata[1];
                                }
                                if (dataArray[i1].ToLower() == _airlinename.ToLower())
                                {
                                    passengerdetails[i].passengerkey = arraypaxdata[0];
                                }
                                else
                                {
                                    arraypaxdata = arraypaxkey[1].Split('^');
                                    _airlinename = arraypaxdata[1];
                                    if (dataArray[i1].ToLower() == _airlinename.ToLower())
                                    {
                                        passengerdetails[i].passengerkey = arraypaxdata[0];
                                    }

                                }
                            }
                            if (passengerdetails[i].passengertypecode == "INFT" || passengerdetails[i].passengertypecode == "INF")
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
                                name.middle = "";
                                Info.dateOfBirth = "";
                                Info.nationality = "IN";
                                Info.residentCountry = "IN";
                                _PassengersModel.name = name;
                                _PassengersModel.info = Info;
                                var jsonPassengers = JsonConvert.SerializeObject(_PassengersModel, Formatting.Indented);
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                HttpResponseMessage responsePassengers = await client.PutAsJsonAsync(AppUrlConstant.AkasaAirPassengerDetails + passengerdetails[i].passengerkey, _PassengersModel);
                                if (responsePassengers.IsSuccessStatusCode)
                                {
                                    var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                    if (i == 0)
                                    {
                                        logs.WriteLogsR(jsonPassengers, "9-UpdatepassengerRequest_Left", "AkasaRT");
                                        logs.WriteLogsR(_responsePassengers, "9-UpdatepassengerResponse_Left", "AkasaRT");

                                    }
                                    else
                                    {
                                        logs.WriteLogsR(jsonPassengers, "9-UpdatepassengerRequest_Right", "AkasaRT");
                                        logs.WriteLogsR(_responsePassengers, "9-UpdatepassengerResponse_Right", "AkasaRT");
                                    }

                                    var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                                }
                            }
                        }

                        int infantcount = 0;
                        for (int k = 0; k < passengerdetails.Count; k++)
                        {
                            if (passengerdetails[k].passengertypecode == "INFT" || passengerdetails[k].passengertypecode == "INF")
                                infantcount++;

                        }

                        AddInFantModel _PassengersModel1 = new AddInFantModel();
                        for (int i = 0; i < passengerdetails.Count; i++)
                        {
                            if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD")
                                continue;
                            if (passengerdetails[i].passengertypecode == "INFT" || passengerdetails[i].passengertypecode == "INF")
                            {
                                for (int k = 0; k < infantcount; k++)
                                {


                                    _PassengersModel1.nationality = "IN";
                                    _PassengersModel1.dateOfBirth = passengerdetails[i].dateOfBirth;
                                    _PassengersModel1.residentCountry = "IN";
                                    _PassengersModel1.gender = "Male";

                                    InfantName nameINF = new InfantName();
                                    nameINF.first = passengerdetails[i].first;
                                    nameINF.middle = "";
                                    nameINF.last = passengerdetails[i].last;
                                    nameINF.title = "Mr";
                                    nameINF.suffix = "";
                                    _PassengersModel1.name = nameINF;


                                    var jsonPassengers = JsonConvert.SerializeObject(_PassengersModel1, Formatting.Indented);
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirPassengerDetails + passengerdetails[k].passengerkey + "/infant", _PassengersModel1);
                                    if (responsePassengers.IsSuccessStatusCode)
                                    {
                                        var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                        if (i == 0)
                                        {
                                            logs.WriteLogsR(jsonPassengers, "10-Updatepassenger_infantRequest_Left", "AkasaRT");
                                            logs.WriteLogsR(_responsePassengers, "10-Updatepassenger_InfantResponse_Left", "AkasaRT");

                                        }
                                        else
                                        {
                                            logs.WriteLogsR(jsonPassengers, "10-Updatepassenger_InfantRequest_Right", "AkasaRT");
                                            logs.WriteLogsR(_responsePassengers, "10-Updatepassenger_InfantResponse_Right", "AkasaRT");
                                        }

                                        var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                                    }
                                    i++;
                                }

                                // STRAT Get INFO
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                HttpResponseMessage responceGetBooking = await client.GetAsync(AppUrlConstant.AkasaAirGetBooking);
                                if (responceGetBooking.IsSuccessStatusCode)
                                {
                                    var _responceGetBooking = responceGetBooking.Content.ReadAsStringAsync().Result;
                                    if (i == 0)
                                    {
                                        logs.WriteLogsR(AppUrlConstant.URLAkasaAir + "/api/nsk/v1/booking", "11-GetBookingRequest_Left", "AkasaRT");
                                        logs.WriteLogsR(_responceGetBooking, "11-GetBookingResponse_Left", "AkasaRT");

                                    }
                                    else
                                    {
                                        logs.WriteLogsR(AppUrlConstant.URLAkasaAir + "/api/nsk/v1/booking", "11-GetBookingRequest_Right", "AkasaRT");
                                        logs.WriteLogsR(_responceGetBooking, "11-GetBookingResponse_Right", "AkasaRT");
                                    }
                                    var JsonObjGetBooking = JsonConvert.DeserializeObject<dynamic>(_responceGetBooking);
                                }
                            }
                        }

                        HttpContext.Session.SetString("PassengerNameDetails", JsonConvert.SerializeObject(passengerdetails));
                    }

                    // Spice Jet **********
                    string Signature = string.Empty;
                    if (string.IsNullOrEmpty(Signature) && dataArray[i1].ToLower() == "spicejet")
                    {
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "SpiceJet").Result;
                        if (i1 == 0)
                        {
                            Signature = tokenData.Token;
                        }
                        else
                        {
                            Signature = tokenData.RToken;
                        }

                        if (Signature == null) { Signature = ""; }
                        UpdatePassengersResponse updatePaxResp = null;
                        UpdatePassengersRequest updatePaxReq = null;
                        try
                        {
                            updatePaxReq = new UpdatePassengersRequest();
                            updatePaxReq.Signature = Signature;
                            updatePaxReq.ContractVersion = 420;
                            updatePaxReq.updatePassengersRequestData = new UpdatePassengersRequestData();
                            updatePaxReq.updatePassengersRequestData.Passengers = GetPassenger(passengerdetails);
                            try
                            {
                                SpiceJetApiController objSpiceJet = new SpiceJetApiController();
                                updatePaxResp = await objSpiceJet.UpdatePassengers(updatePaxReq);
                                string Str2 = JsonConvert.SerializeObject(updatePaxResp);
                                Logs logs = new Logs();
                                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(updatePaxReq) + "\n\n Response: " + JsonConvert.SerializeObject(updatePaxResp), "UpdatePassenger", "SpiceJetRT");

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        catch (Exception ex)
                        {


                        }
                        HttpContext.Session.SetString("PassengerNameDetailsSG", JsonConvert.SerializeObject(_newPassengerdetailsSG));
                    }


                    if (string.IsNullOrEmpty(Signature) && dataArray[i1].ToLower() == "indigo")
                    {
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Indigo").Result;

                        if (i1 == 0)
                        {
                            Signature = tokenData.Token;

                        }
                        else
                        {
                            Signature = tokenData.RToken;
                        }

                        if (Signature == null) { Signature = ""; }
                        _updateContact obj = new _updateContact(httpContextAccessorInstance);
                        IndigoBookingManager_.UpdatePassengersResponse updatePaxResp = await obj.UpdatePassengers(Signature, _newPassengerdetailsSG);
                        string Str2 = JsonConvert.SerializeObject(updatePaxResp);

                        #region GetState
                        //_sell objsell = new _sell();
                        //IndigoBookingManager_.GetBookingFromStateResponse _GetBookingFromStateRS1 = await objsell.GetBookingFromState(Signature, "");

                        //string strdata = JsonConvert.SerializeObject(_GetBookingFromStateRS1);
                        #endregion
                    }
                    if (dataArray[i1].ToLower() == "vistara" || dataArray[i1].ToLower() == "airindia")
                    {
                        //HttpContext.Session.SetString("PassengerNameDetails", JsonConvert.SerializeObject(passengerdetails));
                        passobj = objMongoHelper.Zip(JsonConvert.SerializeObject(passengerdetails));
                        _mongoDBHelper.UpdateFlightTokenOldPassengerGDS(Guid, "GDS", passobj);

                    }
                    vm.SeatmaplistRT = new List<SeatMapResponceModel>();
                    vm.passeengerlistRT = new List<AirAsiaTripResponceModel>();
                    vm.MealslistRT = new List<SSRAvailabiltyResponceModel>();
                    vm.BaggagelistRT = new List<SSRAvailabiltyResponceModel>();
                    string test = string.Empty;
                    //string passengerInfant = HttpContext.Session.GetString("keypassengerItanary");
                    //string passenger = HttpContext.Session.GetString("keypassenger");
                    //string Passenegrtext = HttpContext.Session.GetString("Mainpassengervm");
                    //string Seatmap = HttpContext.Session.GetString("Mainseatmapvm");
                    //string Meals = HttpContext.Session.GetString("Mainmealvm");

                    string passengerInfant = string.Empty;
                    string passenger = string.Empty;
                    string Passenegrtext = string.Empty;
                    string Seatmap = string.Empty;
                    string Meals = string.Empty;


                    MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
                    seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(Guid, "AirAsia").Result;

                    if (seatMealdetail != null)
                    {
                        passengerInfant = objMongoHelper.UnZip(seatMealdetail.Infant);
                        passenger = objMongoHelper.UnZip(seatMealdetail.KPassenger);
                        Passenegrtext = objMongoHelper.UnZip(seatMealdetail.ResultRequest);
                        Seatmap = objMongoHelper.UnZip(seatMealdetail.SeatMap);
                        Meals = objMongoHelper.UnZip(seatMealdetail.MainMeals);

                    }

                    //string passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");
                    string passengerNamedetails = string.Empty;// HttpContext.Session.GetString("PassengerNameDetails");
                    if (dataArray[i1].ToLower() == "spicejet")
                    {
                        passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetailsSG");
                        //passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetailsIndigo");
                        List<string> Baggagedata = new List<string>();
                        Baggagedata.Add("<Start>" + JsonConvert.SerializeObject("") + "<End>");
                        //HttpContext.Session.SetString("SGMealsRT", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                        HttpContext.Session.SetString("Baggagedata", JsonConvert.SerializeObject(Baggagedata));
                        if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Baggagedata)))
                        {
                            if (Baggagedata.Count == 2)
                            {
                                MainBaggagedata = new List<string>();
                            }
                            MainBaggagedata.Add(JsonConvert.SerializeObject(Baggagedata));
                        }

                    }
                    else if (dataArray[i1].ToLower() == "airasia")
                    {
                        passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");
                        List<string> Baggagedata = new List<string>();
                        Baggagedata.Add("<Start>" + JsonConvert.SerializeObject("") + "<End>");
                        //HttpContext.Session.SetString("SGMealsRT", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                        HttpContext.Session.SetString("Baggagedata", JsonConvert.SerializeObject(Baggagedata));
                        if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Baggagedata)))
                        {
                            if (Baggagedata.Count == 2)
                            {
                                MainBaggagedata = new List<string>();
                            }
                            MainBaggagedata.Add(JsonConvert.SerializeObject(Baggagedata));
                        }

                    }
                    else if (dataArray[i1].ToLower() == "akasaair")
                    {
                        passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");
                        List<string> Baggagedata = new List<string>();
                        Baggagedata.Add("<Start>" + JsonConvert.SerializeObject("") + "<End>");
                        //HttpContext.Session.SetString("SGMealsRT", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                        HttpContext.Session.SetString("Baggagedata", JsonConvert.SerializeObject(Baggagedata));
                        if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Baggagedata)))
                        {
                            if (Baggagedata.Count == 2)
                            {
                                MainBaggagedata = new List<string>();
                            }
                            MainBaggagedata.Add(JsonConvert.SerializeObject(Baggagedata));
                        }

                    }
                    else if (dataArray[i1].ToLower() == "indigo")
                    {
                        passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetailsIndigo");
                        List<string> Baggagedata = new List<string>();
                        Baggagedata.Add("<Start>" + JsonConvert.SerializeObject("") + "<End>");
                        //HttpContext.Session.SetString("SGMealsRT", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                        HttpContext.Session.SetString("Baggagedata", JsonConvert.SerializeObject(Baggagedata));
                        if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Baggagedata)))
                        {
                            if (Baggagedata.Count == 2)
                            {
                                MainBaggagedata = new List<string>();
                            }
                            MainBaggagedata.Add(JsonConvert.SerializeObject(Baggagedata));
                        }
                    }
                    else if (dataArray[i1].ToLower() == "airindia")
                    {

                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "GDS").Result;
                        passengerNamedetails = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
                        string _pricesolution = string.Empty;
                        if (i1 == 0)
                        {
                            _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_0");
                        }
                        else
                        {
                            _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_1");
                        }

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
                        //_data = HttpContext.Session.GetString("SGkeypassengerRT");
                        _data = passenger;
                        //string _Total = HttpContext.Session.GetString("Total");

                        //string serializedUnitKey = HttpContext.Session.GetString("UnitKey");
                        //List<string> _unitkey = new List<string>();
                        //if (!string.IsNullOrEmpty(serializedUnitKey))
                        //{
                        //    // Deserialize the JSON string back into a List<string>
                        //    _unitkey = JsonConvert.DeserializeObject<List<string>>(serializedUnitKey);
                        //}

                        //string serializedSSRKey = HttpContext.Session.GetString("ssrKey");
                        //List<string> _SSRkey = new List<string>();
                        //if (!string.IsNullOrEmpty(serializedSSRKey))
                        //{
                        //    // Deserialize the JSON string back into a List<string>
                        //    _SSRkey = JsonConvert.DeserializeObject<List<string>>(serializedSSRKey);
                        //}
                        string newGuid = tokenData.Token;
                        string segmentdata = string.Empty;

                        foreach (Match item in Regex.Matches(_pricesolution.Replace("\\", ""), "<air:AirSegment Key=\"[\\s\\S]*?</air:AirSegment><air:AirPricingInfo", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                        {
                            segmentdata += item.Value.Replace("<air:AirPricingInfo", "");
                        }
                        Hashtable _htpaxwiseBaggage = new Hashtable();
                        //string stravailibitilityrequest = HttpContext.Session.GetString("GDSAvailibilityRequest");
                        //SimpleAvailabilityRequestModel availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(stravailibitilityrequest);
                        string res = _objAvail.GetAirMerchandisingOfferAvailabilityReq(_testURL, createAirmerchandReq, newGuid.ToString(), _targetBranch, _userName, _password, AdultTraveller, _data, "GDSRT", segmentdata);
                        SSRAvailabiltyResponceModel SSRAvailabiltyResponceobj = new SSRAvailabiltyResponceModel();
                        if (res != null)
                        {
                            string weight = "";
                            string BookingTravellerref = "";
                            Hashtable htSSr = new Hashtable();
                            _htpaxwiseBaggage = new Hashtable();

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
                                    htSSr.Add(weight, item.Groups["Price"].Value.Trim() + "*" + item.Value.ToString());
                                }
                                _htpaxwiseBaggage.Add(weight + "_" + BookingTravellerref + "_" + item.Groups["Price"].Value.Trim().Replace("INR", ""), item.Value.ToString());
                            }


                            List<legSsrs> SSRAvailabiltyLegssrlist = new List<legSsrs>();
                            SSRAvailabiltyResponceobj = new SSRAvailabiltyResponceModel();
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
                                        legDetailsobj.destination = Regex.Match(segmentdata, @"FlightNumber=""[\s\S]*?Origin=""(?<Source>[\s\S]*?)""\s*Destination=""(?<Destination>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["Destination"].Value;
                                        legDetailsobj.origin = Regex.Match(segmentdata, @"FlightNumber=""[\s\S]*?Origin=""(?<Source>[\s\S]*?)""\s*Destination=""(?<Destination>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["Source"].Value;
                                        legDetailsobj.departureDate = Regex.Match(segmentdata, @"FlightNumber=""[\s\S]*?Origin=""(?<Source>[\s\S]*?)""\s*Destination=""(?<Destination>[\s\S]*?)""\s*DepartureTime=""(?<Departure>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["Departure"].Value;
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
                                    //SSRAvailabiltyLegssrobj._HashpaxwiseBaggage = _htpaxwiseBaggage;
                                    SSRAvailabiltyLegssrlist.Add(SSRAvailabiltyLegssrobj);
                                }

                            }
                            catch (Exception ex)
                            {

                            }
                            SSRAvailabiltyResponceobj.legSsrs = SSRAvailabiltyLegssrlist;
                            HttpContext.Session.SetString("BaggageDetails", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                        }

                        if (i1 == 0)
                        {
                            HttpContext.Session.SetString("PaxwiseBaggageLeft", JsonConvert.SerializeObject(_htpaxwiseBaggage));
                            passengerNamedetailsdataL = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                            for (int i = 0; i < passengerNamedetailsdataL.Count; i++)
                            {
                                foreach (Match mitem in Regex.Matches(res, "SearchTraveler\\s*Key=\"(?<Key>[\\s\\S]*?)\"[\\s\\S]*?Code=\"(?<TravellerType>[\\s\\S]*?)\"[\\s\\S]*?First=\"(?<Fname>[\\s\\S]*?)\"[\\s\\S]*?Last=\"(?<Lname>[\\s\\S]*?)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {
                                    if (passengerNamedetailsdataL[i].first.ToUpper() == mitem.Groups["Fname"].ToString().ToUpper() && passengerNamedetailsdataL[i].last.ToUpper() == mitem.Groups["Lname"].ToString().ToUpper())
                                    {
                                        passengerNamedetailsdataL[i].passengerkey = mitem.Groups["Key"].Value;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                            }
                            passengerNamedetailsdata = passengerNamedetailsdataL;
                        }
                        else
                        {
                            HttpContext.Session.SetString("PaxwiseBaggageRight", JsonConvert.SerializeObject(_htpaxwiseBaggage));
                            passengerNamedetailsdataR = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                            for (int i = 0; i < passengerNamedetailsdataR.Count; i++)
                            {
                                foreach (Match mitem in Regex.Matches(res, "SearchTraveler\\s*Key=\"(?<Key>[\\s\\S]*?)\"[\\s\\S]*?Code=\"(?<TravellerType>[\\s\\S]*?)\"[\\s\\S]*?First=\"(?<Fname>[\\s\\S]*?)\"[\\s\\S]*?Last=\"(?<Lname>[\\s\\S]*?)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {
                                    if (passengerNamedetailsdataR[i].first.ToUpper() == mitem.Groups["Fname"].ToString().ToUpper() && passengerNamedetailsdataR[i].last.ToUpper() == mitem.Groups["Lname"].ToString().ToUpper())
                                    {
                                        if (passengerNamedetailsdataL != null)
                                        {
                                            passengerNamedetailsdataL[i].passengerkey += "**" + mitem.Groups["Key"].Value;
                                        }
                                        else
                                        {
                                            passengerNamedetailsdataR[i].passengerkey = mitem.Groups["Key"].Value;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                            }
                            if (passengerNamedetailsdataL != null)
                            {
                                passengerNamedetailsdata = passengerNamedetailsdataL;
                            }
                            else
                            {
                                passengerNamedetailsdata = passengerNamedetailsdataR;
                            }
                        }

                        //List<passkeytype> passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));


                        passobj = objMongoHelper.Zip(JsonConvert.SerializeObject(passengerNamedetailsdata));

                        _mongoDBHelper.UpdateFlightTokenPassengerGDS(Guid, "GDS", passobj);

                        if (!string.IsNullOrEmpty(passengerNamedetails) && dataArray[i1].ToLower() == "airindia")
                        {
                            //vm.passengerNamedetails = passengerNamedetailsdata;
                            vm.passengerNamedetails = passengerNamedetailsdata;
                        }
                        else
                        {
                            passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                            vm.passengerNamedetails = passengerNamedetailsdata;
                        }

                        List<string> Baggagedata = new List<string>();
                        Baggagedata.Add("<Start>" + JsonConvert.SerializeObject(SSRAvailabiltyResponceobj) + "<End>");
                        //HttpContext.Session.SetString("SGMealsRT", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                        HttpContext.Session.SetString("Baggagedata", JsonConvert.SerializeObject(Baggagedata));
                        if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Baggagedata)))
                        {
                            if (Baggagedata.Count == 2)
                            {
                                MainBaggagedata = new List<string>();
                            }
                            MainBaggagedata.Add(JsonConvert.SerializeObject(Baggagedata));
                        }

                    }
                    else
                        passengerNamedetails = HttpContext.Session.GetString("PassengerNameDetails");

                    if (!string.IsNullOrEmpty(Passenegrtext))
                    {
                        test = Passenegrtext;

                        foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                        {
                            passenger = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                            if (passenger != null)
                            {
                                passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                                vm.passeengerlistRT.Add(passeengerlist);
                            }
                        }
                    }

                    if (dataArray[i1].ToLower() != "airindia")
                    {
                        if (!string.IsNullOrEmpty(passengerNamedetails))
                        {
                            passengerNamedetailsdata = (List<passkeytype>)JsonConvert.DeserializeObject(passengerNamedetails, typeof(List<passkeytype>));
                            vm.passengerNamedetails = passengerNamedetailsdata;
                        }
                    }


                    if (!string.IsNullOrEmpty(Seatmap))
                    {
                        test = Seatmap;
                        Seatmap = string.Empty;
                        foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                        {
                            Seatmap = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                            if (Seatmap != null)
                            {
                                Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
                                vm.SeatmaplistRT.Add(Seatmaplist);
                            }
                        }
                    }

                    // Meals = string.Empty;
                    // Mealslist = null;
                    if (!string.IsNullOrEmpty(Meals))
                    {
                        test = Meals;
                        foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                        {
                            Meals = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                            if (Meals != null)
                            {
                                Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));
                                vm.MealslistRT.Add(Mealslist);
                            }
                        }
                    }
                }
            }
            HttpContext.Session.SetString("MainBaggagevm", JsonConvert.SerializeObject(MainBaggagedata));
            //To do
            string Baggage = string.Empty;
            string bags = string.Empty;
            Bagslist = null;
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("MainBaggagevm")))
            {
                string test = HttpContext.Session.GetString("MainBaggagevm");
                List<string> rawItems = JsonConvert.DeserializeObject<List<string>>(test);
                foreach (var _test in rawItems)
                {
                    foreach (Match item in Regex.Matches(_test, @"<Start>(?<test>[\s\S]*?)<End>"))
                    {
                        var jsonText = item.Groups["test"].Value;

                        // Step 2: Unescape JSON content
                        string unescaped = Regex.Unescape(jsonText);

                        // Step 3: Deserialize to object
                        SSRAvailabiltyResponceModel _bags = JsonConvert.DeserializeObject<SSRAvailabiltyResponceModel>(unescaped);

                        //if (_bags != null)
                        //{
                        vm.BaggagelistRT.Add(_bags);
                        //}
                    }
                }

                //foreach (Match item in Regex.Matches(test, @"<Start>(?<test>[\s\S]*?)<End>"))
                //{
                //	bags = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                //	if (bags != null)
                //	{
                //		Bagslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(bags, typeof(SSRAvailabiltyResponceModel));
                //		vm.BaggagelistRT.Add(Bagslist);
                //	}
                //}
            }
            //HttpContext.Session.SetString("PassengerNameDetails", JsonConvert.SerializeObject(passengerdetails));

            //HttpContext.Session.SetString("hashdataBaggageRT", JsonConvert.SerializeObject(vm.htpaxwiseBaggageRT));
            return PartialView("_ServiceRequestsPartialView", vm);
        }


        //Post Unit Key
        public async Task<IActionResult> PostUnitkey(List<string> unitKey, List<string> ssrKey, List<string> BaggageSSrkey, List<string> FastfarwardAddon, List<string> PPBGAddon, string Guid)
        {
            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();


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
            List<string> _BaggageSSrkey = new List<string>();
            for (int i = 0; i < BaggageSSrkey.Count; i++)
            {
                if (BaggageSSrkey[i] == null)
                    continue;
                _BaggageSSrkey.Add(BaggageSSrkey[i].Trim());
            }

            if (_BaggageSSrkey.Count > 0 && _BaggageSSrkey[0] == null)
            {
                _BaggageSSrkey = new List<string>();
            }
            if (_ssrKey.Count > 0 && _ssrKey[0] == null)
            {
                _ssrKey = new List<string>();
            }
            ssrKey = new List<string>();
            BaggageSSrkey = new List<string>();
            ssrKey = _ssrKey;
            BaggageSSrkey = _BaggageSSrkey;
            string serializedSSRKey = JsonConvert.SerializeObject(ssrKey);
            HttpContext.Session.SetString("ssrKey", serializedSSRKey);
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
            //Seat


            MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
            seatMealdetail = _mongoDBHelper.GetSuppSeatMealByGUID(Guid, "AirAsia").Result;

            string Meals = string.Empty;
            List<AirAsiaTripResponceModel> passeengerKeyListRT = new List<AirAsiaTripResponceModel>();
            string passenger = objMongoHelper.UnZip(seatMealdetail.KPassenger); // HttpContext.Session.GetString("keypassenger");
            AirAsiaTripResponceModel passeengerKeyList = null;
            if (!string.IsNullOrEmpty(passenger))
            {
                passeengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));

                passeengerKeyListRT.Add(passeengerKeyList);
            }
            //    passenger = HttpContext.Session.GetString("SGkeypassengerRT");
            //  passenger = seatMealdetail.Infant;
            if (!string.IsNullOrEmpty(passenger))
            {
                passeengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                passeengerKeyListRT.Add(passeengerKeyList);
            }


            List<SeatMapResponceModel> SeatmapListRT = new List<SeatMapResponceModel>();
            SeatMapResponceModel Seatmaplist = null;
            string Seatmap = objMongoHelper.UnZip(seatMealdetail.SeatMap);// HttpContext.Session.GetString("Seatmap");
            if (!string.IsNullOrEmpty(Seatmap) && (!Seatmap.ToLower().Contains("<start>")))
            {
                Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
                SeatmapListRT.Add(Seatmaplist);
            }
            Seatmap = HttpContext.Session.GetString("SeatmapRT");
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SeatmapRT")))
            {
                Seatmaplist = (SeatMapResponceModel)JsonConvert.DeserializeObject(Seatmap, typeof(SeatMapResponceModel));
                SeatmapListRT.Add(Seatmaplist);
            }

            List<SSRAvailabiltyResponceModel> mealListRT = new List<SSRAvailabiltyResponceModel>();
            SSRAvailabiltyResponceModel Mealslist = null;
            Meals = objMongoHelper.UnZip(seatMealdetail.Meals);// HttpContext.Session.GetString("Meals");
            if (!string.IsNullOrEmpty(Meals) && (!Meals.ToLower().Contains("<start>")))
            {
                Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));
                mealListRT.Add(Mealslist);
            }

            Meals = HttpContext.Session.GetString("SGMealsRT");
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SGMealsRT")))
            {
                Mealslist = (SSRAvailabiltyResponceModel)JsonConvert.DeserializeObject(Meals, typeof(SSRAvailabiltyResponceModel));
                mealListRT.Add(Mealslist);
            }
            int passengerscount = 0; //passeengerKeyList.passengerscount;
            int Seatcount = unitKey.Count;
            #region RoundTripSSR

            Logs logs1 = new Logs();
            bool flagGDSSSR = false;
            if (ssrKey.Count > 0 || BaggageSSrkey.Count > 0)
            {

                try
                {
                    var ssrKey_1 = ssrKey;
                    string[] ssrKey2 = null;
                    string[] ssrsubKey2 = null;
                    string pas_ssrKey = string.Empty;

                    int journeyscount = 0;
                    int mealid = 0;
                    int bagid = 0;
                    if (!string.IsNullOrEmpty(objMongoHelper.UnZip(seatMealdetail.ResultRequest)))
                    {
                        passenger = objMongoHelper.UnZip(seatMealdetail.ResultRequest);
                        int _a = 0;
                        foreach (Match item in Regex.Matches(passenger, @"<Start>(?<test>[\s\S]*?)<End>"))
                        {

                            passenger = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                            if (passenger != null)
                            {
                                passeengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                                if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "spicejet")
                                {

                                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "SpiceJet").Result;

                                    string tokenview = string.Empty;
                                    if (_a == 0)
                                    {
                                        tokenview = tokenData.Token;
                                    }
                                    else
                                    {
                                        tokenview = tokenData.RToken;
                                    }
                                    token = tokenview;
                                    passengerscount = passeengerKeyList.passengerscount;
                                    Logs logs = new Logs();
                                    using (HttpClient client = new HttpClient())
                                    {
                                        if (ssrKey.Count >= 0 || BaggageSSrkey.Count > 0)
                                        {
                                            #region SellSSr
                                            for (int i = 0; i < passeengerKeyList.passengers.Count; i++)
                                            {
                                                if (passeengerKeyList.passengers[i].passengerTypeCode == "INFT")
                                                    continue;
                                                if (_a == 0)
                                                {
                                                    FastfarwardAddon.Add("FFWD_OneWay0");
                                                }
                                                else
                                                {
                                                    FastfarwardAddon.Add("FFWD__RT0");
                                                }

                                            }
                                            FastfarwardAddon = new List<string>();
                                            SellRequest sellSsrRequest = new SellRequest();
                                            SellRequestData sellreqd = new SellRequestData();
                                            sellSsrRequest.Signature = token;
                                            sellSsrRequest.ContractVersion = 420;
                                            sellreqd.SellBy = SellBy.SSR;
                                            sellreqd.SellBySpecified = true;
                                            sellreqd.SellSSR = new SellSSR();
                                            sellreqd.SellSSR.SSRRequest = new SSRRequest();

                                            journeyscount = passeengerKeyList.journeys.Count;
                                            for (int i = 0; i < journeyscount; i++)
                                            {
                                                int segmentscount = passeengerKeyList.journeys[i].segments.Count;
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests = new SegmentSSRRequest[segmentscount];
                                                for (int j = 0; j < segmentscount; j++)
                                                {
                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j] = new SegmentSSRRequest();
                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].STD = passeengerKeyList.journeys[i].segments[j].designator.departure;
                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].STDSpecified = true;
                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].FlightDesignator = new FlightDesignator();
                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].FlightDesignator.CarrierCode = passeengerKeyList.journeys[i].segments[j].identifier.carrierCode;
                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].FlightDesignator.FlightNumber = passeengerKeyList.journeys[i].segments[j].identifier.identifier;
                                                    string numinfant = HttpContext.Session.GetString("PaxArray");
                                                    Paxes PaxNum = null;
                                                    if (!string.IsNullOrEmpty(numinfant))
                                                    {
                                                        PaxNum = (Paxes)JsonConvert.DeserializeObject(numinfant, typeof(Paxes));
                                                    }
                                                    PaxNum.Infant_ = new List<passkeytype>();
                                                    bool infant = false;
                                                    ssrsegmentwise _obj = new ssrsegmentwise();
                                                    _obj.SSRcodeOneWayI = new List<ssrsKey>();
                                                    _obj.SSRcodeOneWayII = new List<ssrsKey>();
                                                    _obj.SSRcodeRTI = new List<ssrsKey>();
                                                    _obj.SSRcodeRTII = new List<ssrsKey>();
                                                    _obj.SSRbaggagecodeOneWayI = new List<ssrsKey>();
                                                    _obj.SSRbaggagecodeOneWayII = new List<ssrsKey>();
                                                    _obj.SSRbaggagecodeRTI = new List<ssrsKey>();
                                                    _obj.SSRbaggagecodeRTII = new List<ssrsKey>();
                                                    _obj.SSRffwcodeRTI = new List<ssrsKey>();
                                                    _obj.PPBGcodeRTI = new List<ssrsKey>();
                                                    _obj.SSRffwOneWayI = new List<ssrsKey>();
                                                    _obj.PPBGOneWayI = new List<ssrsKey>();
                                                    for (int k = 0; k < ssrKey.Count; k++)
                                                    {
                                                        if (ssrKey[k] == null)
                                                        {
                                                            continue;
                                                        }
                                                        if (ssrKey[k].ToLower().Contains("airasia"))
                                                            continue;

                                                        if (ssrKey[k].Contains("_OneWay0"))
                                                        {
                                                            string[] wordsArray = ssrKey[k].ToString().Split('_');
                                                            if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                            {
                                                                ssrsKey _obj0 = new ssrsKey();
                                                                _obj0.key = ssrKey[k];
                                                                _obj.SSRcodeOneWayI.Add(_obj0);
                                                            }

                                                        }
                                                        else if (ssrKey[k].Contains("_OneWay1"))
                                                        {
                                                            string[] wordsArray = ssrKey[k].ToString().Split('_');
                                                            if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                            {
                                                                ssrsKey _obj1 = new ssrsKey();
                                                                _obj1.key = ssrKey[k];
                                                                _obj.SSRcodeOneWayII.Add(_obj1);
                                                            }
                                                        }
                                                        else if (ssrKey[k].Contains("_RT0"))
                                                        {
                                                            string[] wordsArray = ssrKey[k].ToString().Split('_');
                                                            if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                            {
                                                                ssrsKey _obj2 = new ssrsKey();
                                                                _obj2.key = ssrKey[k];
                                                                _obj.SSRcodeRTI.Add(_obj2);
                                                            }
                                                        }
                                                        else if (ssrKey[k].Contains("_RT1"))
                                                        {
                                                            string[] wordsArray = ssrKey[k].ToString().Split('_');
                                                            if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                            {
                                                                ssrsKey _obj3 = new ssrsKey();
                                                                _obj3.key = ssrKey[k];
                                                                _obj.SSRcodeRTII.Add(_obj3);
                                                            }
                                                        }

                                                    }
                                                    if (BaggageSSrkey.Count > 0)
                                                    {
                                                        for (int k = 0; k < BaggageSSrkey.Count; k++)
                                                        {
                                                            string[] sskeydata = new string[2];
                                                            if (BaggageSSrkey[k].Contains("_OneWay0"))
                                                            {
                                                                //split
                                                                string[] wordsArray = BaggageSSrkey[k].ToString().Split('_');
                                                                if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                                {
                                                                    sskeydata = BaggageSSrkey[k].Split("_");
                                                                    ssrsKey _objBag0 = new ssrsKey();
                                                                    _objBag0.key = sskeydata[0];
                                                                    _obj.SSRbaggagecodeOneWayI.Add(_objBag0);
                                                                }
                                                            }
                                                            else if (BaggageSSrkey[k].Contains("_OneWay1"))
                                                            {
                                                                string[] wordsArray = BaggageSSrkey[k].ToString().Split('_');
                                                                if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                                {
                                                                    sskeydata = BaggageSSrkey[k].Split("_");
                                                                    ssrsKey _objBag1 = new ssrsKey();
                                                                    _objBag1.key = sskeydata[0];
                                                                    _obj.SSRbaggagecodeOneWayII.Add(_objBag1);
                                                                }
                                                            }
                                                            else if (BaggageSSrkey[k].Contains("_RT0"))
                                                            {
                                                                string[] wordsArray = BaggageSSrkey[k].ToString().Split('_');
                                                                if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                                {
                                                                    sskeydata = BaggageSSrkey[k].Split("_");
                                                                    ssrsKey _objBagRT1 = new ssrsKey();
                                                                    _objBagRT1.key = sskeydata[0];
                                                                    _obj.SSRbaggagecodeRTI.Add(_objBagRT1);
                                                                }
                                                            }
                                                            else if (BaggageSSrkey[k].Contains("_RT1"))
                                                            {
                                                                string[] wordsArray = BaggageSSrkey[k].ToString().Split('_');
                                                                if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                                {
                                                                    sskeydata = BaggageSSrkey[k].Split("_");
                                                                    ssrsKey _objBagRT2 = new ssrsKey();
                                                                    _objBagRT2.key = sskeydata[0];
                                                                    _obj.SSRbaggagecodeRTII.Add(_objBagRT2);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    for (int k = 0; k < FastfarwardAddon.Count; k++)
                                                    {
                                                        string[] sskeydata = new string[2];
                                                        if (FastfarwardAddon[k].Contains("_OneWay0"))
                                                        {
                                                            //split
                                                            string[] wordsArray = FastfarwardAddon[k].ToString().Split('_');
                                                            if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                            {
                                                                sskeydata = FastfarwardAddon[k].Split("_");
                                                                ssrsKey _objffw0 = new ssrsKey();
                                                                _objffw0.key = sskeydata[0];
                                                                _obj.SSRffwOneWayI.Add(_objffw0);
                                                            }
                                                        }
                                                        else if (FastfarwardAddon[k].Contains("_RT0"))
                                                        {
                                                            string[] wordsArray = FastfarwardAddon[k].ToString().Split('_');
                                                            if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                            {
                                                                sskeydata = FastfarwardAddon[k].Split("_");
                                                                ssrsKey _objffwRT1 = new ssrsKey();
                                                                _objffwRT1.key = sskeydata[0];
                                                                _obj.SSRffwcodeRTI.Add(_objffwRT1);
                                                            }
                                                        }

                                                    }
                                                    //Priority Boarding
                                                    for (int k = 0; k < PPBGAddon.Count; k++)
                                                    {
                                                        string[] sskeydata = new string[2];
                                                        if (PPBGAddon[k].Contains("_OneWay0"))
                                                        {
                                                            //split
                                                            string[] wordsArray = PPBGAddon[k].ToString().Split('_');
                                                            if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                            {
                                                                sskeydata = PPBGAddon[k].Split("_");
                                                                ssrsKey _objppbg0 = new ssrsKey();
                                                                _objppbg0.key = sskeydata[0];
                                                                _obj.PPBGOneWayI.Add(_objppbg0);
                                                            }
                                                        }
                                                        else if (PPBGAddon[k].Contains("_RT0"))
                                                        {
                                                            string[] wordsArray = PPBGAddon[k].ToString().Split('_');
                                                            if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                                            {
                                                                sskeydata = PPBGAddon[k].Split("_");
                                                                ssrsKey _objppbg1 = new ssrsKey();
                                                                _objppbg1.key = sskeydata[0];
                                                                _obj.PPBGcodeRTI.Add(_objppbg1);
                                                            }
                                                        }

                                                    }
                                                    if (j == 0 && _a == 0)
                                                    {
                                                        sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs = new PaxSSR[PaxNum.Infant_.Count + _obj.SSRcodeOneWayI.Count + _obj.SSRbaggagecodeOneWayI.Count + _obj.SSRffwOneWayI.Count + _obj.PPBGOneWayI.Count];
                                                    }
                                                    else if (j == 1 && _a == 0)
                                                    {
                                                        sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs = new PaxSSR[PaxNum.Infant_.Count + _obj.SSRcodeOneWayII.Count + _obj.SSRbaggagecodeOneWayII.Count];
                                                    }
                                                    else if (j == 0 && _a == 1)
                                                    {
                                                        sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs = new PaxSSR[PaxNum.Infant_.Count + _obj.SSRcodeRTI.Count + _obj.SSRbaggagecodeRTI.Count + _obj.SSRffwcodeRTI.Count + _obj.PPBGcodeRTI.Count];
                                                    }
                                                    else if (j == 1 && _a == 1)
                                                    {
                                                        sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs = new PaxSSR[PaxNum.Infant_.Count + _obj.SSRcodeRTII.Count + _obj.SSRbaggagecodeRTII.Count];
                                                    }


                                                    int TotalPaxcount = PaxNum.Adults_.Count + PaxNum.Childs_.Count;

                                                    if (j == 0 && _a == 0)
                                                    {
                                                        //int k = 0;

                                                        if (TotalPaxcount > 0)
                                                        {
                                                            for (int j1 = 0; j1 < PaxNum.Infant_.Count + _obj.SSRcodeOneWayI.Count + _obj.SSRbaggagecodeOneWayI.Count + _obj.SSRffwOneWayI.Count + _obj.PPBGOneWayI.Count; j1++)
                                                            {

                                                                if (j1 < PaxNum.Infant_.Count)
                                                                {
                                                                    for (int i2 = 0; i2 < PaxNum.Adults_.Count; i2++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                    {
                                                                        int infantcount = PaxNum.Infant_.Count;
                                                                        if (infantcount > 0 && i2 + 1 <= infantcount)
                                                                        {
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2] = new PaxSSR();
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].ActionStatusCode = "NN";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].SSRCode = "INFT";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].PassengerNumberSpecified = true;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].PassengerNumber = Convert.ToInt16(i2);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].SSRNumber = Convert.ToInt16(0);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                            j1 = PaxNum.Infant_.Count - 1;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    int idx = 0;
                                                                    if (_obj.SSRcodeOneWayI.Count > 0 || _obj.SSRbaggagecodeOneWayI.Count > 0)//&& i1 + 1 <= ssrKey.Count
                                                                    {
                                                                        for (int i2 = 0; i2 < _obj.SSRcodeOneWayI.Count; i2++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                        {
                                                                            string[] wordsArray = _obj.SSRcodeOneWayI[i2].key.ToString().Split('_');
                                                                            //alert(wordsArray);
                                                                            //var meal = null;
                                                                            string ssrCodeKey = "";
                                                                            if (wordsArray.Length > 1)
                                                                            {
                                                                                ssrCodeKey = wordsArray[0];
                                                                                ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                            }
                                                                            else
                                                                                ssrCodeKey = _obj.SSRcodeOneWayI[i2].key.ToString();
                                                                            idx = j1 + i2;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(i2);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                            //j1 = j1 + i1;

                                                                        }
                                                                        if (_obj.SSRbaggagecodeOneWayI.Count > 0)
                                                                        {
                                                                            for (int k = 0; k < PaxNum.Adults_.Count + PaxNum.Childs_.Count; k++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                            {
                                                                                int baggagecount = _obj.SSRbaggagecodeOneWayI.Count;
                                                                                if (baggagecount > 0 && k + 1 <= baggagecount)
                                                                                {
                                                                                    if (sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] != null)
                                                                                    {
                                                                                        idx++;
                                                                                    }
                                                                                    else
                                                                                        idx = k;

                                                                                    string[] wordsArray = _obj.SSRbaggagecodeOneWayI[k].key.ToString().Split('_');
                                                                                    //alert(wordsArray);
                                                                                    //var meal = null;
                                                                                    string ssrCodeKey = "";
                                                                                    if (wordsArray.Length > 1)
                                                                                    {
                                                                                        ssrCodeKey = wordsArray[0];
                                                                                        ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                                    }
                                                                                    else
                                                                                        ssrCodeKey = _obj.SSRbaggagecodeOneWayI[k].key.ToString();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(k);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;

                                                                                }
                                                                            }
                                                                        }
                                                                        if (_obj.SSRffwOneWayI.Count > 0)
                                                                        {
                                                                            for (int k = 0; k < PaxNum.Adults_.Count + PaxNum.Childs_.Count; k++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                            {
                                                                                int baggagecount = _obj.SSRffwOneWayI.Count;
                                                                                if (baggagecount > 0 && k + 1 <= baggagecount)
                                                                                {
                                                                                    idx++;
                                                                                    string[] wordsArray = _obj.SSRffwOneWayI[k].key.ToString().Split(' ');
                                                                                    //alert(wordsArray);
                                                                                    //var meal = null;
                                                                                    string ssrCodeKey = "";
                                                                                    if (wordsArray.Length > 1)
                                                                                    {
                                                                                        ssrCodeKey = wordsArray[0];
                                                                                        ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                                    }
                                                                                    else
                                                                                        ssrCodeKey = _obj.SSRffwOneWayI[k].key.ToString();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(k);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;

                                                                                }
                                                                            }
                                                                        }
                                                                        if (_obj.PPBGOneWayI.Count > 0)
                                                                        {
                                                                            for (int k = 0; k < PaxNum.Adults_.Count + PaxNum.Childs_.Count; k++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                            {
                                                                                int baggagecount = _obj.PPBGOneWayI.Count;
                                                                                if (baggagecount > 0 && k + 1 <= baggagecount)
                                                                                {
                                                                                    idx++;
                                                                                    string[] wordsArray = _obj.PPBGOneWayI[k].key.ToString().Split(' ');
                                                                                    //alert(wordsArray);
                                                                                    //var meal = null;
                                                                                    string ssrCodeKey = "";
                                                                                    if (wordsArray.Length > 1)
                                                                                    {
                                                                                        ssrCodeKey = wordsArray[0];
                                                                                        ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                                    }
                                                                                    else
                                                                                        ssrCodeKey = _obj.PPBGOneWayI[k].key.ToString();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(k);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;

                                                                                }
                                                                            }
                                                                        }

                                                                    }
                                                                    j1 = idx;
                                                                }
                                                            }

                                                        }


                                                    }
                                                    else if (j == 1 && _a == 0)
                                                    {
                                                        if (TotalPaxcount > 0)
                                                        {
                                                            for (int j1 = 0; j1 < PaxNum.Infant_.Count + _obj.SSRcodeOneWayII.Count + _obj.SSRbaggagecodeOneWayII.Count; j1++)
                                                            {

                                                                if (j1 < PaxNum.Infant_.Count)
                                                                {
                                                                    for (int i2 = 0; i2 < PaxNum.Adults_.Count; i2++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                    {
                                                                        int infantcount = PaxNum.Infant_.Count;
                                                                        if (infantcount > 0 && i2 + 1 <= infantcount)
                                                                        {
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2] = new PaxSSR();
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].ActionStatusCode = "NN";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].SSRCode = "INFT";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].PassengerNumberSpecified = true;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].PassengerNumber = Convert.ToInt16(i2);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].SSRNumber = Convert.ToInt16(0);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                            j1 = PaxNum.Infant_.Count - 1;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    int idx = 0;
                                                                    if (_obj.SSRcodeOneWayII.Count > 0 || _obj.SSRbaggagecodeOneWayII.Count > 0)//&& i1 + 1 <= ssrKey.Count
                                                                    {
                                                                        for (int i2 = 0; i2 < _obj.SSRcodeOneWayII.Count; i2++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                        {
                                                                            string[] wordsArray = _obj.SSRcodeOneWayII[i2].key.ToString().Split('_');
                                                                            //alert(wordsArray);
                                                                            //var meal = null;
                                                                            string ssrCodeKey = "";
                                                                            if (wordsArray.Length > 1)
                                                                            {
                                                                                ssrCodeKey = wordsArray[0];
                                                                                ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                            }
                                                                            else
                                                                                ssrCodeKey = _obj.SSRcodeOneWayII[i2].key.ToString();
                                                                            idx = j1 + i2;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(i2);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                            //j1 = j1 + i1;

                                                                        }

                                                                    }
                                                                    if (_obj.SSRbaggagecodeOneWayII.Count > 0)
                                                                    {
                                                                        if (idx > 0)
                                                                            idx++;
                                                                        for (int k = 0; k < PaxNum.Adults_.Count + PaxNum.Childs_.Count; k++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                        {
                                                                            int baggagecount = _obj.SSRbaggagecodeOneWayII.Count;
                                                                            if (baggagecount > 0 && k + 1 <= baggagecount)
                                                                            {

                                                                                string[] wordsArray = _obj.SSRbaggagecodeOneWayII[k].key.ToString().Split('_');
                                                                                //alert(wordsArray);
                                                                                //var meal = null;
                                                                                string ssrCodeKey = "";
                                                                                if (wordsArray.Length > 1)
                                                                                {
                                                                                    ssrCodeKey = wordsArray[0];
                                                                                    ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                                }
                                                                                else
                                                                                    ssrCodeKey = _obj.SSRbaggagecodeOneWayII[k].key.ToString();
                                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(k);
                                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                                idx++;
                                                                            }
                                                                        }
                                                                    }
                                                                    j1 = idx;
                                                                }
                                                            }

                                                        }
                                                    }
                                                    else if (j == 0 && _a == 1)
                                                    {
                                                        //int k = 0;

                                                        if (TotalPaxcount > 0)
                                                        {
                                                            for (int j1 = 0; j1 < PaxNum.Infant_.Count + _obj.SSRcodeRTI.Count + _obj.SSRbaggagecodeRTI.Count + _obj.SSRffwcodeRTI.Count + _obj.PPBGcodeRTI.Count; j1++)
                                                            {

                                                                if (j1 < PaxNum.Infant_.Count)
                                                                {
                                                                    for (int i2 = 0; i2 < PaxNum.Adults_.Count; i2++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                    {
                                                                        int infantcount = PaxNum.Infant_.Count;
                                                                        if (infantcount > 0 && i2 + 1 <= infantcount)
                                                                        {
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2] = new PaxSSR();
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].ActionStatusCode = "NN";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].SSRCode = "INFT";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].PassengerNumberSpecified = true;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].PassengerNumber = Convert.ToInt16(i2);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].SSRNumber = Convert.ToInt16(0);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                            j1 = PaxNum.Infant_.Count - 1;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    int idx = 0;
                                                                    if (_obj.SSRcodeRTI.Count > 0 || _obj.SSRbaggagecodeRTI.Count > 0)//&& i1 + 1 <= ssrKey.Count
                                                                    {
                                                                        for (int i2 = 0; i2 < _obj.SSRcodeRTI.Count; i2++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                        {
                                                                            string[] wordsArray = _obj.SSRcodeRTI[i2].key.ToString().Split('_');
                                                                            //alert(wordsArray);
                                                                            //var meal = null;
                                                                            string ssrCodeKey = "";
                                                                            if (wordsArray.Length > 1)
                                                                            {
                                                                                ssrCodeKey = wordsArray[0];
                                                                                ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                            }
                                                                            else
                                                                                ssrCodeKey = _obj.SSRcodeRTI[i2].key.ToString();
                                                                            idx = j1 + i2;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(i2);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                            //j1 = j1 + i1;

                                                                        }
                                                                        if (_obj.SSRbaggagecodeRTI.Count > 0)
                                                                        {
                                                                            //idx++;
                                                                            for (int k = 0; k < PaxNum.Adults_.Count + PaxNum.Childs_.Count; k++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                            {
                                                                                int baggagecount = _obj.SSRbaggagecodeRTI.Count;
                                                                                if (baggagecount > 0 && k + 1 <= baggagecount)
                                                                                {
                                                                                    if (sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] != null)
                                                                                    {
                                                                                        idx++;
                                                                                    }
                                                                                    else
                                                                                        idx = k;

                                                                                    string[] wordsArray = _obj.SSRbaggagecodeRTI[k].key.ToString().Split('_');
                                                                                    //alert(wordsArray);
                                                                                    //var meal = null;
                                                                                    string ssrCodeKey = "";
                                                                                    if (wordsArray.Length > 1)
                                                                                    {
                                                                                        ssrCodeKey = wordsArray[0];
                                                                                        ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                                    }
                                                                                    else
                                                                                        ssrCodeKey = _obj.SSRbaggagecodeRTI[k].key.ToString();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(k);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                                    //idx++;
                                                                                }
                                                                            }
                                                                        }
                                                                        //
                                                                        if (_obj.SSRffwcodeRTI.Count > 0)
                                                                        {
                                                                            idx++;
                                                                            for (int k = 0; k < PaxNum.Adults_.Count + PaxNum.Childs_.Count; k++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                            {
                                                                                int baggagecount = _obj.SSRffwcodeRTI.Count;
                                                                                if (baggagecount > 0 && k + 1 <= baggagecount)
                                                                                {

                                                                                    string[] wordsArray = _obj.SSRffwcodeRTI[k].key.ToString().Split(' ');
                                                                                    //alert(wordsArray);
                                                                                    //var meal = null;
                                                                                    string ssrCodeKey = "";
                                                                                    if (wordsArray.Length > 1)
                                                                                    {
                                                                                        ssrCodeKey = wordsArray[0];
                                                                                        ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                                    }
                                                                                    else
                                                                                        ssrCodeKey = _obj.SSRffwcodeRTI[k].key.ToString();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(k);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                                    idx++;
                                                                                }
                                                                            }
                                                                        }
                                                                        if (_obj.PPBGcodeRTI.Count > 0)
                                                                        {
                                                                            idx++;
                                                                            for (int k = 0; k < PaxNum.Adults_.Count + PaxNum.Childs_.Count; k++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                            {
                                                                                int baggagecount = _obj.PPBGcodeRTI.Count;
                                                                                if (baggagecount > 0 && k + 1 <= baggagecount)
                                                                                {

                                                                                    string[] wordsArray = _obj.PPBGcodeRTI[k].key.ToString().Split(' ');
                                                                                    //alert(wordsArray);
                                                                                    //var meal = null;
                                                                                    string ssrCodeKey = "";
                                                                                    if (wordsArray.Length > 1)
                                                                                    {
                                                                                        ssrCodeKey = wordsArray[0];
                                                                                        ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                                    }
                                                                                    else
                                                                                        ssrCodeKey = _obj.PPBGcodeRTI[k].key.ToString();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(k);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                                    idx++;
                                                                                }
                                                                            }
                                                                        }



                                                                    }
                                                                    j1 = idx;
                                                                }
                                                            }

                                                        }


                                                    }
                                                    else if (j == 1 && _a == 1)
                                                    {
                                                        if (TotalPaxcount > 0)
                                                        {
                                                            for (int j1 = 0; j1 < PaxNum.Infant_.Count + _obj.SSRcodeRTII.Count + _obj.SSRbaggagecodeRTII.Count; j1++)
                                                            {

                                                                if (j1 < PaxNum.Infant_.Count)
                                                                {
                                                                    for (int i2 = 0; i2 < PaxNum.Adults_.Count; i2++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                    {
                                                                        int infantcount = PaxNum.Infant_.Count;
                                                                        if (infantcount > 0 && i2 + 1 <= infantcount)
                                                                        {
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2] = new PaxSSR();
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].ActionStatusCode = "NN";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].SSRCode = "INFT";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].PassengerNumberSpecified = true;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].PassengerNumber = Convert.ToInt16(i2);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].SSRNumber = Convert.ToInt16(0);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i2].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                            j1 = PaxNum.Infant_.Count - 1;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    int idx = 0;
                                                                    if (_obj.SSRcodeRTII.Count > 0 || _obj.SSRbaggagecodeRTII.Count > 0)//&& i1 + 1 <= ssrKey.Count
                                                                    {
                                                                        for (int i2 = 0; i2 < _obj.SSRcodeRTII.Count; i2++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                        {
                                                                            string[] wordsArray = _obj.SSRcodeRTII[i2].key.ToString().Split('_');
                                                                            //alert(wordsArray);
                                                                            //var meal = null;
                                                                            string ssrCodeKey = "";
                                                                            if (wordsArray.Length > 1)
                                                                            {
                                                                                ssrCodeKey = wordsArray[0];
                                                                                ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                            }
                                                                            else
                                                                                ssrCodeKey = _obj.SSRcodeRTII[i2].key.ToString();
                                                                            idx = j1 + i2;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(i2);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                            //j1 = j1 + i1;

                                                                        }
                                                                        if (_obj.SSRbaggagecodeRTII.Count > 0)
                                                                        {
                                                                            idx++;
                                                                            for (int k = 0; k < PaxNum.Adults_.Count + PaxNum.Childs_.Count; k++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                                            {
                                                                                int baggagecount = _obj.SSRbaggagecodeRTII.Count;
                                                                                if (baggagecount > 0 && k + 1 <= baggagecount)
                                                                                {

                                                                                    string[] wordsArray = _obj.SSRbaggagecodeRTII[k].key.ToString().Split('_');
                                                                                    //alert(wordsArray);
                                                                                    //var meal = null;
                                                                                    string ssrCodeKey = "";
                                                                                    if (wordsArray.Length > 1)
                                                                                    {
                                                                                        ssrCodeKey = wordsArray[0];
                                                                                        ssrCodeKey = ssrCodeKey.Replace(@"""", "");
                                                                                    }
                                                                                    else
                                                                                        ssrCodeKey = _obj.SSRbaggagecodeRTII[k].key.ToString();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx] = new PaxSSR();
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ActionStatusCode = "NN";
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRCode = ssrCodeKey;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumberSpecified = true;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].PassengerNumber = Convert.ToInt16(k);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].SSRNumber = Convert.ToInt16(0);
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].DepartureStation = passeengerKeyList.journeys[i].segments[j].designator.origin;
                                                                                    sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[idx].ArrivalStation = passeengerKeyList.journeys[i].segments[j].designator.destination;
                                                                                    idx++;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    j1 = idx;
                                                                }
                                                            }

                                                        }
                                                    }

                                                    //to do 
                                                    //sellSsrRequest.SellRequestData = sellreqd;
                                                    //SellResponse sellSsrResponse = null;
                                                    //sellreqd.SellSSR.SSRRequest.SellSSRMode = SellSSRMode.NonBundle;
                                                    //sellreqd.SellSSR.SSRRequest.SellSSRModeSpecified = true;
                                                    //SpiceJetApiController objSpiceJet = new SpiceJetApiController();
                                                    //sellSsrResponse = await objSpiceJet.sellssR(sellSsrRequest);

                                                    //string Str3 = JsonConvert.SerializeObject(sellSsrResponse);
                                                }
                                            }

                                            sellSsrRequest.SellRequestData = sellreqd;
                                            SellResponse sellSsrResponse = null;
                                            sellreqd.SellSSR.SSRRequest.SellSSRMode = SellSSRMode.NonBundle;
                                            sellreqd.SellSSR.SSRRequest.SellSSRModeSpecified = true;
                                            SpiceJetApiController objSpiceJet = new SpiceJetApiController();
                                            sellSsrResponse = await objSpiceJet.sellssR(sellSsrRequest);

                                            string Str3 = JsonConvert.SerializeObject(sellSsrResponse);
                                            logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(sellSsrRequest) + "\n\n Response: " + JsonConvert.SerializeObject(sellSsrResponse), "SellSSR", "SpiceJetRT");


                                            if (sellSsrResponse != null)
                                            {
                                                var JsonObjSeatAssignment = sellSsrResponse;
                                            }
                                            #endregion
                                        }
                                    }
                                }
                                else if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "airasia")
                                {
                                    bagid = 0;
                                    mealid = 0;
                                    string tokenview = string.Empty;

                                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "AirAsia").Result;

                                    //token = tokenData.Token;

                                    if (string.IsNullOrEmpty(tokenview))
                                    {
                                        if (_a == 0)
                                        {
                                            tokenview = tokenData.Token;
                                        }
                                        else
                                        {
                                            tokenview = tokenData.RToken;
                                        }
                                        token = tokenview.Replace(@"""", string.Empty);
                                        if (token == "" || token == null)
                                        {
                                            return RedirectToAction("Index");
                                        }
                                    }
                                    for (int l1 = 0; l1 < ssrKey.Count; l1++)
                                    {
                                        int l = 0;
                                        int m = 0;
                                        int idx = 0;
                                        int paxnum = 0;
                                        if (mealid < ssrKey.Count)
                                        {
                                            ssrsubKey2 = null;
                                            pas_ssrKey = string.Empty;
                                            if (ssrKey[mealid] == null)
                                            {
                                                continue;
                                            }
                                            if (ssrKey[mealid].ToLower().Contains("airasia") && _a == 0 && (ssrKey[mealid].ToLower().Contains("oneway0") || ssrKey[mealid].ToLower().Contains("oneway1")))
                                            {
                                                if (ssrKey[mealid].Length > 1)
                                                {
                                                    ssrsubKey2 = ssrKey[mealid].Split('_');
                                                    pas_ssrKey = ssrsubKey2[0].Trim();
                                                }
                                                string mealskey = pas_ssrKey;
                                                mealskey = mealskey.Replace(@"""", string.Empty);
                                                if (!string.IsNullOrEmpty(token))
                                                {
                                                    using (HttpClient client = new HttpClient())
                                                    {
                                                        SellSSRModel _sellSSRModel = new SellSSRModel();
                                                        _sellSSRModel.count = 1;
                                                        _sellSSRModel.note = "DevTest";
                                                        _sellSSRModel.forceWaveOnSell = false;
                                                        _sellSSRModel.currencyCode = "INR";
                                                        var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                        HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AirasiaMealSelect + mealskey, _sellSSRModel);
                                                        if (responseSellSSR.IsSuccessStatusCode)
                                                        {

                                                            var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                                            if (_a == 0)
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequest_Left", "AirAsiaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponse_Left", "AirAsiaRT");

                                                            }
                                                            else
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequest_Right", "AirAsiaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponse_Right", "AirAsiaRT");
                                                            }

                                                            var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                                        }

                                                    }
                                                }
                                            }
                                            else if (ssrKey[mealid].ToLower().Contains("airasia") && _a == 1 && (ssrKey[mealid].ToLower().Contains("rt0") || ssrKey[mealid].ToLower().Contains("rt1")))
                                            {
                                                if (ssrKey[mealid].Length > 1)
                                                {
                                                    ssrsubKey2 = ssrKey[mealid].Split('_');
                                                    pas_ssrKey = ssrsubKey2[0].Trim();
                                                }
                                                string mealskey = pas_ssrKey;
                                                mealskey = mealskey.Replace(@"""", string.Empty);
                                                if (!string.IsNullOrEmpty(token))
                                                {
                                                    using (HttpClient client = new HttpClient())
                                                    {
                                                        SellSSRModel _sellSSRModel = new SellSSRModel();
                                                        _sellSSRModel.count = 1;
                                                        _sellSSRModel.note = "DevTest";
                                                        _sellSSRModel.forceWaveOnSell = false;
                                                        _sellSSRModel.currencyCode = "INR";
                                                        var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                        HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AirasiaMealSelect + mealskey, _sellSSRModel);
                                                        if (responseSellSSR.IsSuccessStatusCode)
                                                        {
                                                            var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                                            if (_a == 0)
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestMeal_Left", "AirAsiaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseMeal_Left", "AirAsiaRT");

                                                            }
                                                            else
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestMeal_Right", "AirAsiaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseMeal_Right", "AirAsiaRT");
                                                            }
                                                            var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                mealid++;
                                                continue;
                                            }
                                            mealid++;
                                        }
                                    }
                                    for (int b = 0; b < BaggageSSrkey.Count; b++)
                                    {
                                        int l = 0;
                                        int m = 0;
                                        int idx = 0;
                                        int paxnum = 0;
                                        if (bagid < BaggageSSrkey.Count)
                                        {
                                            if (BaggageSSrkey[bagid] == null)
                                            {
                                                continue;
                                            }
                                            if (BaggageSSrkey[bagid].ToLower().Contains("airasia") && _a == 0 && (BaggageSSrkey[bagid].ToLower().Contains("oneway0") || BaggageSSrkey[bagid].ToLower().Contains("oneway1")))
                                            {
                                                if (BaggageSSrkey[bagid].Length > 1)
                                                {
                                                    ssrsubKey2 = BaggageSSrkey[bagid].Split('_');
                                                    pas_ssrKey = ssrsubKey2[0].Trim();
                                                }
                                                string bagskey = pas_ssrKey;
                                                bagskey = bagskey.Replace(@"""", string.Empty);
                                                if (!string.IsNullOrEmpty(token))
                                                {
                                                    using (HttpClient client = new HttpClient())
                                                    {
                                                        SellSSRModel _sellSSRModel = new SellSSRModel();
                                                        _sellSSRModel.count = 1;
                                                        _sellSSRModel.note = "DevTest";
                                                        _sellSSRModel.forceWaveOnSell = false;
                                                        _sellSSRModel.currencyCode = "INR";
                                                        var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                        HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AirasiaMealSelect + bagskey, _sellSSRModel);
                                                        if (responseSellSSR.IsSuccessStatusCode)
                                                        {

                                                            var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                                            if (_a == 0)
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestBaggage_Left", "AirAsiaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseBaggage_Left", "AirAsiaRT");

                                                            }
                                                            else
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestBaggage_Right", "AirAsiaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseBaggage_Right", "AirAsiaRT");
                                                            }
                                                            var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                                        }

                                                    }
                                                }
                                            }
                                            else if (BaggageSSrkey[bagid].ToLower().Contains("airasia") && _a == 1 && (BaggageSSrkey[bagid].ToLower().Contains("rt0") || BaggageSSrkey[bagid].ToLower().Contains("rt1")))
                                            {
                                                if (BaggageSSrkey[bagid].Length > 1)
                                                {
                                                    ssrsubKey2 = BaggageSSrkey[bagid].Split('_');
                                                    pas_ssrKey = ssrsubKey2[0].Trim();
                                                }
                                                string bagskeyCon = pas_ssrKey;
                                                bagskeyCon = bagskeyCon.Replace(@"""", string.Empty);
                                                if (!string.IsNullOrEmpty(token))
                                                {
                                                    using (HttpClient client = new HttpClient())
                                                    {
                                                        SellSSRModel _sellSSRModel = new SellSSRModel();
                                                        _sellSSRModel.count = 1;
                                                        _sellSSRModel.note = "DevTest";
                                                        _sellSSRModel.forceWaveOnSell = false;
                                                        _sellSSRModel.currencyCode = "INR";
                                                        var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                        HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AirasiaMealSelect + bagskeyCon, _sellSSRModel);
                                                        if (responseSellSSR.IsSuccessStatusCode)
                                                        {
                                                            var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                                            if (_a == 0)
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestBaggage_Left", "AirAsiaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseBaggage_Left", "AirAsiaRT");

                                                            }
                                                            else
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestBaggage_Right", "AirAsiaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseBaggage_Right", "AirAsiaRT");
                                                            }
                                                            var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bagid++;
                                                continue;
                                            }
                                            bagid++;
                                        }
                                    }


                                }
                                else if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "akasaair")
                                {
                                    bagid = 0;
                                    mealid = 0;

                                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;

                                    string tokenview = string.Empty;
                                    if (string.IsNullOrEmpty(tokenview))
                                    {
                                        if (_a == 0)
                                        {
                                            tokenview = tokenData.Token;
                                        }
                                        else
                                        {
                                            tokenview = tokenData.RToken;
                                        }
                                        if (tokenview == null) { tokenview = ""; }
                                        token = tokenview.Replace(@"""", string.Empty);
                                        if (token == "" || token == null)
                                        {
                                            return RedirectToAction("Index");
                                        }
                                    }
                                    for (int l1 = 0; l1 < ssrKey.Count; l1++)
                                    {
                                        int l = 0;
                                        int m = 0;
                                        int idx = 0;
                                        int paxnum = 0;

                                        if (mealid < ssrKey.Count)
                                        {
                                            ssrsubKey2 = null;
                                            pas_ssrKey = string.Empty;
                                            if (ssrKey[mealid] == null)
                                            {
                                                continue;
                                            }
                                            if (ssrKey[mealid].ToLower().Contains("akasaair") && _a == 0 && (ssrKey[mealid].ToLower().Contains("oneway0") || ssrKey[mealid].ToLower().Contains("oneway1")))
                                            {
                                                if (ssrKey[mealid].Length > 1)
                                                {
                                                    ssrsubKey2 = ssrKey[mealid].Split('_');
                                                    pas_ssrKey = ssrsubKey2[0].Trim();
                                                }
                                                string mealskey = pas_ssrKey;
                                                mealskey = mealskey.Replace(@"""", string.Empty);
                                                if (!string.IsNullOrEmpty(token))
                                                {
                                                    using (HttpClient client = new HttpClient())
                                                    {
                                                        SellSSRModel _sellSSRModel = new SellSSRModel();
                                                        _sellSSRModel.count = 1;
                                                        _sellSSRModel.note = "PYOG";
                                                        _sellSSRModel.currencyCode = "INR";
                                                        var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                        HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirMealBaggagePost + mealskey, _sellSSRModel);
                                                        if (responseSellSSR.IsSuccessStatusCode)
                                                        {

                                                            var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                                            if (_a == 0)
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestmeal_Left", "AkasaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseMeal_Left", "AkasaRT");

                                                            }
                                                            else
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestMeal_Right", "AkasaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseMeal_Right", "AkasaRT");
                                                            }
                                                            var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                                        }

                                                    }
                                                }
                                            }

                                            else if (ssrKey[mealid].ToLower().Contains("akasaair") && _a == 1 && (ssrKey[mealid].ToLower().Contains("rt0") || ssrKey[mealid].ToLower().Contains("rt1")))
                                            {
                                                if (ssrKey[mealid].Length > 1)
                                                {
                                                    ssrsubKey2 = ssrKey[mealid].Split('_');
                                                    pas_ssrKey = ssrsubKey2[0].Trim();
                                                }
                                                string mealskey = pas_ssrKey;
                                                mealskey = mealskey.Replace(@"""", string.Empty);
                                                if (!string.IsNullOrEmpty(token))
                                                {
                                                    using (HttpClient client = new HttpClient())
                                                    {
                                                        SellSSRModel _sellSSRModel = new SellSSRModel();
                                                        _sellSSRModel.count = 1;
                                                        _sellSSRModel.note = "PYOG";
                                                        _sellSSRModel.currencyCode = "INR";
                                                        var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                        HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirMealBaggagePost + mealskey, _sellSSRModel);
                                                        if (responseSellSSR.IsSuccessStatusCode)
                                                        {
                                                            var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                                            if (_a == 0)
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestmeal_Left", "AkasaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseMeal_Left", "AkasaRT");

                                                            }
                                                            else
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestMeal_Right", "AkasaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseMeal_Right", "AkasaRT");
                                                            }
                                                            var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                mealid++;
                                                continue;
                                            }
                                            mealid++;
                                        }
                                    }
                                    //SellBaggage
                                    for (int b = 0; b < BaggageSSrkey.Count; b++)
                                    {
                                        int l = 0;
                                        int m = 0;
                                        int idx = 0;
                                        int paxnum = 0;
                                        if (bagid < BaggageSSrkey.Count)
                                        {
                                            if (BaggageSSrkey[bagid] == null)
                                            {
                                                continue;
                                            }
                                            if (BaggageSSrkey[bagid].ToLower().Contains("akasaair") && _a == 0 && (BaggageSSrkey[bagid].ToLower().Contains("oneway0") || BaggageSSrkey[bagid].ToLower().Contains("oneway1")))
                                            {
                                                if (BaggageSSrkey[bagid].Length > 1)
                                                {
                                                    ssrsubKey2 = BaggageSSrkey[bagid].Split('_');
                                                    pas_ssrKey = ssrsubKey2[0].Trim();
                                                }
                                                string bagskey = pas_ssrKey;
                                                bagskey = bagskey.Replace(@"""", string.Empty);
                                                if (!string.IsNullOrEmpty(token))
                                                {
                                                    using (HttpClient client = new HttpClient())
                                                    {
                                                        SellSSRModel _sellSSRModel = new SellSSRModel();
                                                        _sellSSRModel.count = 1;
                                                        _sellSSRModel.note = "PYOG";
                                                        _sellSSRModel.currencyCode = "INR";
                                                        var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                        HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirMealBaggagePost + bagskey, _sellSSRModel);
                                                        if (responseSellSSR.IsSuccessStatusCode)
                                                        {

                                                            var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                                            if (_a == 0)
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestBaggage_Left", "AkasaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseBaggage_Left", "AkasaRT");

                                                            }
                                                            else
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestBaggage_Right", "AkasaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseBaggage_Right", "AkasaRT");
                                                            }
                                                            var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                                        }

                                                    }
                                                }
                                            }
                                            else if (BaggageSSrkey[bagid].ToLower().Contains("akasaair") && _a == 1 && (BaggageSSrkey[bagid].ToLower().Contains("rt0") || BaggageSSrkey[bagid].ToLower().Contains("rt1")))
                                            {
                                                if (BaggageSSrkey[bagid].Length > 1)
                                                {
                                                    ssrsubKey2 = BaggageSSrkey[bagid].Split('_');
                                                    pas_ssrKey = ssrsubKey2[0].Trim();
                                                }
                                                string bagskeyCon = pas_ssrKey;
                                                bagskeyCon = bagskeyCon.Replace(@"""", string.Empty);
                                                if (!string.IsNullOrEmpty(token))
                                                {
                                                    using (HttpClient client = new HttpClient())
                                                    {
                                                        SellSSRModel _sellSSRModel = new SellSSRModel();
                                                        _sellSSRModel.count = 1;
                                                        _sellSSRModel.note = "PYOG";
                                                        _sellSSRModel.currencyCode = "INR";
                                                        var jsonSellSSR = JsonConvert.SerializeObject(_sellSSRModel, Formatting.Indented);
                                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                        HttpResponseMessage responseSellSSR = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirMealBaggagePost + bagskeyCon, _sellSSRModel);
                                                        if (responseSellSSR.IsSuccessStatusCode)
                                                        {
                                                            var _responseresponseSellSSR = responseSellSSR.Content.ReadAsStringAsync().Result;
                                                            if (_a == 0)
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestBaggage_Left", "AkasaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseBaggage_Left", "AkasaRT");

                                                            }
                                                            else
                                                            {
                                                                logs.WriteLogsR(jsonSellSSR, "12-SellSSRRequestBaggage_Right", "AkasaRT");
                                                                logs.WriteLogsR(_responseresponseSellSSR, "12-SellSSRResponseBaggage_Right", "AkasaRT");
                                                            }
                                                            var JsonObjresponseresponseSellSSR = JsonConvert.DeserializeObject<dynamic>(_responseresponseSellSSR);
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bagid++;
                                                continue;
                                            }
                                            bagid++;
                                        }
                                    }

                                }
                                else if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "indigo")
                                {
                                    bool Boolfastforward = false;
                                    string tokenview = string.Empty;

                                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Indigo").Result;
                                    if (_a == 0)
                                    {
                                        tokenview = tokenData.Token;
                                    }
                                    else
                                    {
                                        tokenview = tokenData.RToken;
                                    }
                                    if (tokenview == null) { tokenview = ""; }
                                    token = tokenview.Replace(@"""", string.Empty);
                                    _SellSSR obj_ = new _SellSSR(httpContextAccessorInstance);
                                    IndigoBookingManager_.SellResponse sellSsrResponse = await obj_.sellssr(token, passeengerKeyList, ssrKey, BaggageSSrkey, FastfarwardAddon, PPBGAddon, Boolfastforward, _a);
                                }
                                _a++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }



            }

            #endregion
            #region SeatMap RoundTrip Both
            if (unitKey.Count > 0)
            {

                try
                {
                    int journeyscount = 0;
                    int keycount1 = 0;
                    int keycount0 = 0;
                    int seatid = 0;
                    string[] unitKey2 = null;
                    string[] unitsubKey2 = null;
                    string pas_unitKey = string.Empty;
                    for (int k = 0; k < unitKey.Count; k++)
                    {
                        if ((unitKey[k].ToLower().Contains("spicejet") || unitKey[k].ToLower().Contains("indigo")) && (unitKey[k].ToString().Contains("OneWay0") || unitKey[k].ToString().Contains("OneWay1")))
                            keycount0++;
                        if ((unitKey[k].ToLower().Contains("spicejet") || unitKey[k].ToLower().Contains("indigo")) && (unitKey[k].ToString().Contains("RT0") || unitKey[k].ToString().Contains("RT1")))
                            keycount1++;

                    }
                    Logs _logs = new Logs();
                    int _index = 0;
                    int p = 0;
                    if (!string.IsNullOrEmpty(seatMealdetail.ResultRequest))
                    {
                        passenger = objMongoHelper.UnZip(seatMealdetail.ResultRequest);
                        foreach (Match item in Regex.Matches(passenger, @"<Start>(?<test>[\s\S]*?)<End>"))
                        {
                            passenger = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                            if (passenger != null)
                            {
                                passeengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                                if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "spicejet")
                                {
                                    seatid = 0;
                                    _index = 0;
                                    journeyscount = passeengerKeyList.journeys.Count;
                                    AssignSeatsResponse _AssignseatRes = new AssignSeatsResponse();
                                    AssignSeatsRequest _AssignSeatReq = new AssignSeatsRequest();
                                    string Signature = string.Empty;
                                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "SpiceJet").Result;

                                    if (p == 0)
                                    {
                                        Signature = tokenData.Token; // HttpContext.Session.GetString("SpicejetSignature");
                                    }
                                    else
                                    {
                                        Signature = tokenData.RToken;
                                    }
                                    if (Signature == null) { Signature = ""; }
                                    Signature = Signature.Replace(@"""", string.Empty);
                                    if (!string.IsNullOrEmpty(Signature))
                                    {
                                        Signature = Signature.Replace(@"""", string.Empty);
                                        _AssignSeatReq.Signature = Signature;
                                        _AssignSeatReq.ContractVersion = 420;
                                        _AssignSeatReq.SellSeatRequest = new SeatSellRequest();
                                        _AssignSeatReq.SellSeatRequest.SeatAssignmentMode = SeatAssignmentMode.PreSeatAssignment;
                                        _AssignSeatReq.SellSeatRequest.SeatAssignmentModeSpecified = true;
                                        int keycount = 0;
                                        if (p == 0)
                                            keycount = keycount0;
                                        else
                                            keycount = keycount1;
                                        _AssignSeatReq.SellSeatRequest.SegmentSeatRequests = new SegmentSeatRequest[keycount];// [unitKey.Count];//to do

                                        for (int i2 = 0; i2 < journeyscount; i2++)
                                        {
                                            int l = 0;
                                            int m = 0;
                                            for (int k = 0; k < unitKey.Count; k++)
                                            {
                                                int idx = 0;
                                                int paxnum = 0;
                                                if (seatid < unitKey.Count)
                                                {
                                                    if (unitKey[seatid].Length > 1)
                                                    {
                                                        if ((unitKey[seatid].ToString().Contains("OneWay0") || unitKey[seatid].ToString().Contains("OneWay1")) && p == 0)
                                                        {
                                                            unitsubKey2 = unitKey[seatid].Split('_');
                                                            pas_unitKey = unitsubKey2[1];
                                                            idx = int.Parse(unitsubKey2[3]);
                                                            if (idx == 0)
                                                            {
                                                                paxnum = l++;
                                                            }
                                                            else
                                                            {
                                                                paxnum = m++;
                                                            }
                                                            //keycount++;

                                                        }
                                                        else if ((unitKey[seatid].ToString().Contains("RT0") || unitKey[seatid].ToString().Contains("RT1")) && p == 1)
                                                        {
                                                            unitsubKey2 = unitKey[seatid].Split('_');
                                                            pas_unitKey = unitsubKey2[1];
                                                            idx = int.Parse(unitsubKey2[3]);
                                                            if (idx == 0)
                                                            {
                                                                paxnum = l++;
                                                            }
                                                            else
                                                            {
                                                                paxnum = m++;
                                                            }
                                                            //keycount++;

                                                        }
                                                        else
                                                        {
                                                            seatid++;
                                                            continue;
                                                        }
                                                    }

                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index] = new SegmentSeatRequest();
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].FlightDesignator = new FlightDesignator();
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].FlightDesignator.CarrierCode = passeengerKeyList.journeys[i2].segments[idx].identifier.carrierCode;
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].FlightDesignator.FlightNumber = passeengerKeyList.journeys[i2].segments[idx].identifier.identifier;
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].STD = passeengerKeyList.journeys[i2].segments[idx].designator.departure;
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].STDSpecified = true;
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].DepartureStation = passeengerKeyList.journeys[i2].segments[idx].designator.origin;
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].ArrivalStation = passeengerKeyList.journeys[i2].segments[idx].designator.destination;
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].UnitDesignator = pas_unitKey.Trim();
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].PassengerNumbers = new short[1];
                                                    _AssignSeatReq.SellSeatRequest.SegmentSeatRequests[_index].PassengerNumbers[0] = Convert.ToInt16(paxnum);
                                                    seatid++;
                                                    _index++;
                                                }
                                            }

                                        }

                                        _AssignSeatReq.SellSeatRequest.IncludeSeatData = true;
                                        _AssignSeatReq.SellSeatRequest.IncludeSeatDataSpecified = true;

                                        SpiceJetApiController objSpiceJet = new SpiceJetApiController();
                                        _AssignseatRes = await objSpiceJet.Assignseat(_AssignSeatReq);

                                        string Str2 = JsonConvert.SerializeObject(_AssignseatRes);

                                        _logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_AssignSeatReq) + "\n\n Response: " + JsonConvert.SerializeObject(_AssignseatRes), "AssignSeat", "SpiceJetRT");
                                    }
                                }
                                else if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "airasia")
                                {
                                    //seatid = 0;
                                    string tokenview = string.Empty;

                                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "AirAsia").Result;

                                    if (p == 0)
                                    {
                                        tokenview = tokenData.Token;
                                    }
                                    else
                                    {
                                        tokenview = tokenData.RToken;
                                    }
                                    if (!string.IsNullOrEmpty(tokenview))
                                    {

                                        token = tokenview.Replace(@"""", string.Empty);
                                        if (token == "" || token == null)
                                        {
                                            return RedirectToAction("Index");
                                        }
                                    }
                                    journeyscount = passeengerKeyList.journeys.Count;
                                    ssrsegmentwise _obj = new ssrsegmentwise();
                                    _obj.SSRcodeOneWayI = new List<ssrsKey>();
                                    _obj.SSRcodeOneWayII = new List<ssrsKey>();
                                    _obj.SSRcodeRTI = new List<ssrsKey>();
                                    _obj.SSRcodeRTII = new List<ssrsKey>();
                                    for (int k = 0; k < unitKey.Count; k++)
                                    {
                                        if (unitKey[k].ToLower().Contains("spicejet"))
                                            continue;

                                        if (unitKey[k].Contains("_OneWay0") && p == 0)
                                        {
                                            unitsubKey2 = unitKey[k].Split('_');
                                            pas_unitKey = unitsubKey2[1].Trim();
                                            ssrsKey _obj0 = new ssrsKey();
                                            _obj0.key = pas_unitKey;
                                            _obj.SSRcodeOneWayI.Add(_obj0);
                                        }
                                        else if (unitKey[k].Contains("_OneWay1") && p == 0)
                                        {
                                            unitsubKey2 = unitKey[k].Split('_');
                                            pas_unitKey = unitsubKey2[1].Trim();
                                            ssrsKey _obj1 = new ssrsKey();
                                            _obj1.key = pas_unitKey;
                                            _obj.SSRcodeOneWayII.Add(_obj1);
                                        }
                                        else if (unitKey[k].Contains("_RT0") && p == 1)
                                        {
                                            unitsubKey2 = unitKey[k].Split('_');
                                            pas_unitKey = unitsubKey2[1].Trim();
                                            ssrsKey _obj2 = new ssrsKey();
                                            _obj2.key = pas_unitKey;
                                            _obj.SSRcodeRTI.Add(_obj2);
                                        }
                                        else if (unitKey[k].Contains("_RT1") && p == 1)
                                        {
                                            unitsubKey2 = unitKey[k].Split('_');
                                            pas_unitKey = unitsubKey2[1].Trim();
                                            ssrsKey _obj3 = new ssrsKey();
                                            _obj3.key = pas_unitKey;
                                            _obj.SSRcodeRTII.Add(_obj3);
                                        }
                                    }
                                    for (int i = 0; i < journeyscount; i++)
                                    {
                                        int segmentscount = passeengerKeyList.journeys[i].segments.Count;

                                        for (int l2 = 0; l2 < _obj.SSRcodeOneWayI.Count; l2++)
                                        {
                                            if (passeengerKeyList.passengers[l2].passengerTypeCode == "INFT")
                                                continue;
                                            string passengerkey = string.Empty;
                                            passengerkey = passeengerKeyList.passengers[l2].passengerKey;
                                            pas_unitKey = _obj.SSRcodeOneWayI[l2].key.Trim();
                                            using (HttpClient client = new HttpClient())
                                            {
                                                string journeyKey = passeengerKeyList.journeys[i].journeyKey;
                                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                                _SeatAssignmentModel.journeyKey = journeyKey;
                                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AirasiaSeatSelect + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                                if (responceSeatAssignment.IsSuccessStatusCode)
                                                {
                                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                                    if (p == 0)
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Left", "AirAsiaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Left", "AirAsiaRT");

                                                    }
                                                    else
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Right", "AirAsiaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Right", "AirAsiaRT");
                                                    }
                                                    var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                                                }

                                            }
                                        }

                                        for (int l2 = 0; l2 < _obj.SSRcodeOneWayII.Count; l2++)
                                        {
                                            if (passeengerKeyList.passengers[l2].passengerTypeCode == "INFT")
                                                continue;
                                            string passengerkey = string.Empty;
                                            passengerkey = passeengerKeyList.passengers[l2].passengerKey;
                                            pas_unitKey = _obj.SSRcodeOneWayII[l2].key.Trim();
                                            using (HttpClient client = new HttpClient())
                                            {
                                                string journeyKey = passeengerKeyList.journeys[i].journeyKey;
                                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                                _SeatAssignmentModel.journeyKey = journeyKey;
                                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AirasiaSeatSelect + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                                if (responceSeatAssignment.IsSuccessStatusCode)
                                                {
                                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                                    if (p == 0)
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Left", "AirAsiaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Left", "AirAsiaRT");

                                                    }
                                                    else
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Right", "AirAsiaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Right", "AirAsiaRT");
                                                    }
                                                    var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                                                }

                                            }
                                        }

                                        for (int l2 = 0; l2 < _obj.SSRcodeRTI.Count; l2++)
                                        {
                                            if (passeengerKeyList.passengers[l2].passengerTypeCode == "INFT")
                                                continue;
                                            string passengerkey = string.Empty;
                                            passengerkey = passeengerKeyList.passengers[l2].passengerKey;
                                            pas_unitKey = _obj.SSRcodeRTI[l2].key.Trim();
                                            using (HttpClient client = new HttpClient())
                                            {
                                                string journeyKey = passeengerKeyList.journeys[i].journeyKey;
                                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                                _SeatAssignmentModel.journeyKey = journeyKey;
                                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AirasiaSeatSelect + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                                if (responceSeatAssignment.IsSuccessStatusCode)
                                                {
                                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                                    if (p == 0)
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Left", "AirAsiaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Left", "AirAsiaRT");

                                                    }
                                                    else
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Right", "AirAsiaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Right", "AirAsiaRT");
                                                    }
                                                    var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                                                }

                                            }
                                        }
                                        for (int l2 = 0; l2 < _obj.SSRcodeRTII.Count; l2++)
                                        {
                                            if (passeengerKeyList.passengers[l2].passengerTypeCode == "INFT")
                                                continue;
                                            string passengerkey = string.Empty;
                                            passengerkey = passeengerKeyList.passengers[l2].passengerKey;
                                            pas_unitKey = _obj.SSRcodeRTII[l2].key.Trim();
                                            using (HttpClient client = new HttpClient())
                                            {
                                                string journeyKey = passeengerKeyList.journeys[i].journeyKey;
                                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                                _SeatAssignmentModel.journeyKey = journeyKey;
                                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AirasiaSeatSelect + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                                if (responceSeatAssignment.IsSuccessStatusCode)
                                                {
                                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                                    if (p == 0)
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Left", "AirAsiaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Left", "AirAsiaRT");

                                                    }
                                                    else
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Right", "AirAsiaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Right", "AirAsiaRT");
                                                    }
                                                    var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                                                }

                                            }
                                        }
                                    }

                                    int l = 0;
                                }
                                //*************Akasa AssignSeat API************
                                else if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "akasaair")
                                {
                                    string tokenview = string.Empty;

                                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;

                                    if (p == 0)
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
                                        if (token == "" || token == null)
                                        {
                                            return RedirectToAction("Index");
                                        }
                                    }
                                    journeyscount = passeengerKeyList.journeys.Count;
                                    //To do

                                    ssrsegmentwise _obj = new ssrsegmentwise();
                                    _obj.SSRcodeOneWayI = new List<ssrsKey>();
                                    _obj.SSRcodeOneWayII = new List<ssrsKey>();
                                    _obj.SSRcodeRTI = new List<ssrsKey>();
                                    _obj.SSRcodeRTII = new List<ssrsKey>();
                                    for (int k = 0; k < unitKey.Count; k++)
                                    {
                                        if (unitKey[k].ToLower().Contains("spicejet") || unitKey[k].ToLower().Contains("airasia"))
                                            continue;

                                        if (unitKey[k].Contains("_OneWay0") && p == 0)
                                        {
                                            unitsubKey2 = unitKey[k].Split('_');
                                            pas_unitKey = unitsubKey2[1].Trim();
                                            ssrsKey _obj0 = new ssrsKey();
                                            _obj0.key = pas_unitKey;
                                            _obj.SSRcodeOneWayI.Add(_obj0);
                                        }
                                        else if (unitKey[k].Contains("_OneWay1") && p == 0)
                                        {
                                            unitsubKey2 = unitKey[k].Split('_');
                                            pas_unitKey = unitsubKey2[1].Trim();
                                            ssrsKey _obj1 = new ssrsKey();
                                            _obj1.key = pas_unitKey;
                                            _obj.SSRcodeOneWayII.Add(_obj1);
                                        }
                                        else if (unitKey[k].Contains("_RT0") && p == 1)
                                        {
                                            unitsubKey2 = unitKey[k].Split('_');
                                            pas_unitKey = unitsubKey2[1].Trim();
                                            ssrsKey _obj2 = new ssrsKey();
                                            _obj2.key = pas_unitKey;
                                            _obj.SSRcodeRTI.Add(_obj2);
                                        }
                                        else if (unitKey[k].Contains("_RT1") && p == 1)
                                        {
                                            unitsubKey2 = unitKey[k].Split('_');
                                            pas_unitKey = unitsubKey2[1].Trim();
                                            ssrsKey _obj3 = new ssrsKey();
                                            _obj3.key = pas_unitKey;
                                            _obj.SSRcodeRTII.Add(_obj3);
                                        }

                                    }




                                    for (int i = 0; i < journeyscount; i++)
                                    {
                                        int segmentscount = passeengerKeyList.journeys[i].segments.Count;

                                        for (int l2 = 0; l2 < _obj.SSRcodeOneWayI.Count; l2++)
                                        {
                                            if (passeengerKeyList.passengers[l2].passengerTypeCode == "INFT")
                                                continue;
                                            string passengerkey = string.Empty;
                                            passengerkey = passeengerKeyList.passengers[l2].passengerKey;
                                            pas_unitKey = _obj.SSRcodeOneWayI[l2].key.Trim();
                                            using (HttpClient client = new HttpClient())
                                            {
                                                string journeyKey = passeengerKeyList.journeys[i].journeyKey;
                                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                                _SeatAssignmentModel.journeyKey = journeyKey;
                                                _SeatAssignmentModel.collectedCurrencyCode = "INR";
                                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirSeatAssign + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                                if (responceSeatAssignment.IsSuccessStatusCode)
                                                {
                                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                                    if (p == 0)
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Left", "AkasaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Left", "AkasaRT");

                                                    }
                                                    else
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Right", "AkasaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Right", "AkasaRT");
                                                    }
                                                    var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                                                }

                                            }
                                        }

                                        for (int l2 = 0; l2 < _obj.SSRcodeOneWayII.Count; l2++)
                                        {
                                            if (passeengerKeyList.passengers[l2].passengerTypeCode == "INFT")
                                                continue;
                                            string passengerkey = string.Empty;
                                            passengerkey = passeengerKeyList.passengers[l2].passengerKey;
                                            pas_unitKey = _obj.SSRcodeOneWayII[l2].key.Trim();
                                            using (HttpClient client = new HttpClient())
                                            {
                                                string journeyKey = passeengerKeyList.journeys[i].journeyKey;
                                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                                _SeatAssignmentModel.journeyKey = journeyKey;
                                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirSeatAssign + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                                if (responceSeatAssignment.IsSuccessStatusCode)
                                                {
                                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                                    if (p == 0)
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Left", "AkasaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Left", "AkasaRT");

                                                    }
                                                    else
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Right", "AkasaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Right", "AkasaRT");
                                                    }
                                                    var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                                                }

                                            }
                                        }
                                        // SSr CODE API For Infant AkasaAir
                                        for (int l2 = 0; l2 < _obj.SSRcodeRTI.Count; l2++)
                                        {
                                            if (passeengerKeyList.passengers[l2].passengerTypeCode == "INFT")
                                                continue;
                                            string passengerkey = string.Empty;
                                            passengerkey = passeengerKeyList.passengers[l2].passengerKey;
                                            pas_unitKey = _obj.SSRcodeRTI[l2].key.Trim();
                                            using (HttpClient client = new HttpClient())
                                            {
                                                string journeyKey = passeengerKeyList.journeys[i].journeyKey;
                                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                                _SeatAssignmentModel.journeyKey = journeyKey;
                                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirSeatAssign + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                                if (responceSeatAssignment.IsSuccessStatusCode)
                                                {
                                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                                    if (p == 0)
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Left", "AkasaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Left", "AkasaRT");

                                                    }
                                                    else
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Right", "AkasaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Right", "AkasaRT");
                                                    }
                                                    var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                                                }

                                            }
                                        }
                                        for (int l2 = 0; l2 < _obj.SSRcodeRTII.Count; l2++)
                                        {
                                            if (passeengerKeyList.passengers[l2].passengerTypeCode == "INFT")
                                                continue;
                                            string passengerkey = string.Empty;
                                            passengerkey = passeengerKeyList.passengers[l2].passengerKey;
                                            pas_unitKey = _obj.SSRcodeRTII[l2].key.Trim();
                                            using (HttpClient client = new HttpClient())
                                            {
                                                string journeyKey = passeengerKeyList.journeys[i].journeyKey;
                                                SeatAssignmentModel _SeatAssignmentModel = new SeatAssignmentModel();
                                                _SeatAssignmentModel.journeyKey = journeyKey;
                                                var jsonSeatAssignmentRequest = JsonConvert.SerializeObject(_SeatAssignmentModel, Formatting.Indented);
                                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                HttpResponseMessage responceSeatAssignment = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirSeatAssign + passengerkey + "/seats/" + pas_unitKey, _SeatAssignmentModel);
                                                if (responceSeatAssignment.IsSuccessStatusCode)
                                                {
                                                    var _responseSeatAssignment = responceSeatAssignment.Content.ReadAsStringAsync().Result;
                                                    if (p == 0)
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Left", "AkasaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Left", "AkasaRT");

                                                    }
                                                    else
                                                    {
                                                        logs.WriteLogsR(jsonSeatAssignmentRequest, "13-AssignSeatRequest_Right", "AkasaRT");
                                                        logs.WriteLogsR(_responseSeatAssignment, "13-AssignSeatResponse_Right", "AkasaRT");
                                                    }
                                                    var JsonObjSeatAssignment = JsonConvert.DeserializeObject<dynamic>(_responseSeatAssignment);
                                                }

                                            }
                                        }
                                    }
                                    int l = 0;
                                }

                                //*************Indigo AssignSeat API*************
                                else if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "indigo")
                                {
                                    string Signature = string.Empty;
                                    seatid = 0;
                                    _index = 0;
                                    tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Indigo").Result;

                                    if (p == 0)
                                    {
                                        Signature = tokenData.Token;
                                    }
                                    else
                                    {
                                        Signature = tokenData.RToken;
                                    }
                                    if (Signature == null) { Signature = ""; }
                                    Signature = Signature.Replace(@"""", string.Empty);
                                    _SellSSR obj_ = new _SellSSR(httpContextAccessorInstance);
                                    IndigoBookingManager_.AssignSeatsResponse _AssignseatRes = await obj_.AssignSeat(Signature, passeengerKeyList, unitKey, p, keycount0, keycount1);

                                }
                                p++;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                }

            }
            if (!string.IsNullOrEmpty(objMongoHelper.UnZip(seatMealdetail.ResultRequest)))
            {
                passenger = objMongoHelper.UnZip(seatMealdetail.ResultRequest);
                int _a = 0;
                int k1 = 0;
                foreach (Match item in Regex.Matches(passenger, @"<Start>(?<test>[\s\S]*?)<End>"))
                {

                    passenger = item.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                    if (passenger != null)
                    {
                        passeengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                        if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "airindia")
                        {
                            flagGDSSSR = true;
                            //if (!string.IsNullOrEmpty(seatMealdetail.ResultRequest))
                            //{
                            //passenger = objMongoHelper.UnZip(seatMealdetail.ResultRequest);

                            //foreach (Match mitem in Regex.Matches(passenger, @"<Start>(?<test>[\s\S]*?)<End>"))
                            //{
                            //passenger = mitem.Groups["test"].Value.ToString().Replace("/\"", "\"").Replace("\\\"", "\"").Replace("\\\\", "");
                            //if (passenger != null)
                            //{
                            //passeengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                            //if (passeengerKeyList.journeys[0].Airlinename.ToLower() == "airindia")
                            //{
                            token = string.Empty;
                            string tokenview = string.Empty;
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
                                using (HttpClient client1 = new HttpClient())
                                {
                                    #region Commit Booking
                                    TravelPort _objAvail = null;
                                    HttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();
                                    _objAvail = new TravelPort(httpContextAccessorInstance);
                                    string _UniversalRecordURL = AppUrlConstant.GDSUniversalRecordURL;
                                    string _testURL = AppUrlConstant.GDSURL;
                                    string _targetBranch = string.Empty;
                                    string Logfolder = string.Empty;
                                    string _userName = string.Empty;
                                    string _password = string.Empty;
                                    _targetBranch = "P7027135";
                                    _userName = "Universal API/uAPI5098257106-beb65aec";
                                    _password = "Q!f5-d7A3D";
                                    StringBuilder createPNRReq = new StringBuilder();
                                    string AdultTraveller = objMongoHelper.UnZip(tokenData.OldPassengerRequest);
                                    string _data = objMongoHelper.UnZip(seatMealdetail.KPassenger); // HttpContext.Session.GetString("SGkeypassengerRT");
                                    string _Total = HttpContext.Session.GetString("Total");
                                    string stravailibitilityrequest = objMongoHelper.UnZip(tokenData.PassRequest); //HttpContext.Session.GetString("PassengerModel");
                                    SimpleAvailabilityRequestModel availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(stravailibitilityrequest);

                                    //retrive PNR
                                    string _pricesolution = string.Empty;
                                    string _htbaggagedataStringL = string.Empty;
                                    string _htbaggagedataStringR = string.Empty;
                                    if (k1 == 0)
                                    {
                                        //Logfolder = "GDSOneWay";
                                        _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_0");
                                        _htbaggagedataStringL = HttpContext.Session.GetString("PaxwiseBaggageLeft");

                                    }
                                    else
                                    {
                                        //Logfolder = "GDSRT";
                                        _pricesolution = HttpContext.Session.GetString("PricingSolutionValue_1");
                                        _htbaggagedataStringR = HttpContext.Session.GetString("PaxwiseBaggageRight");
                                    }

                                    string segmentdata = string.Empty;
                                    foreach (Match _item in Regex.Matches(_pricesolution.Replace("\\", ""), "<air:AirSegment Key=\"[\\s\\S]*?</air:AirSegment><air:AirPricingInfo", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                    {
                                        segmentdata += _item.Value.Replace("<air:AirPricingInfo", "");
                                    }
                                    string strAirTicket = string.Empty;
                                    StringBuilder createSSRReq = new StringBuilder();
                                    string strResponse = string.Empty;
                                    string segmentblock = string.Empty;
                                    string res = string.Empty;
                                    string RecordLocator = string.Empty;
                                    string _TicketRecordLocator = string.Empty;
                                    segmentblock = segmentdata;
                                    res = _objAvail.CreatePNRRoundTrip(_testURL, createPNRReq, newGuid.ToString(), _targetBranch, _userName, _password, AdultTraveller, _data, _Total, Logfolder, k1, _unitkey, _ssrKey, _pricesolution);
                                    RecordLocator = Regex.Match(res, @"universal:UniversalRecord\s*LocatorCode=""(?<LocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["LocatorCode"].Value.Trim();
                                    //getdetails
                                    strResponse = _objAvail.RetrivePnr(RecordLocator, _UniversalRecordURL, newGuid.ToString(), _targetBranch, _userName, _password, Logfolder);

                                    string ProvidelocatorCode = Regex.Match(strResponse, @"universal:ProviderReservationInfo[\s\S]*?LocatorCode=""(?<ProviderLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["ProviderLocatorCode"].Value.Trim();
                                    string supplierLocatorCode = Regex.Match(strResponse, @"SupplierLocatorCode=""(?<SupplierLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["SupplierLocatorCode"].Value.Trim();
                                    string UniversalLocatorCode = Regex.Match(strResponse, @"UniversalRecord\s*LocatorCode=""(?<UniversalLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["UniversalLocatorCode"].Value.Trim();
                                    segmentblock += "@" + ProvidelocatorCode + "@" + supplierLocatorCode + "@" + UniversalLocatorCode;

                                    var jsonDataObject = objMongoHelper.UnZip(tokenData.PassengerRequest); //HttpContext.Session.GetString("PassengerModel");
                                    List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(jsonDataObject.ToString(), typeof(List<passkeytype>));
                                    string strSeatResponseleft = HttpContext.Session.GetString("SeatResponseleft");
                                    string strSeatResponseright = HttpContext.Session.GetString("SeatResponseright");
                                    res = _objAvail.AirMerchandisingFulfillmentReqRoundTrip(_testURL, createSSRReq, newGuid.ToString(), _targetBranch, _userName, _password, Logfolder, unitKey, ssrKey, BaggageSSrkey, availibiltyRQGDS, passengerdetails, _htbaggagedataStringL, _htbaggagedataStringR, strSeatResponseleft, strSeatResponseright, k1, segmentblock);

                                    UniversalLocatorCode = Regex.Match(res, @"UniversalRecord\s*LocatorCode=""(?<UniversalLocatorCode>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["UniversalLocatorCode"].Value.Trim();
                                    if (k1 == 0)
                                    {
                                        HttpContext.Session.SetString("PNRL", res + "@@" + UniversalLocatorCode);
                                    }
                                    else
                                    {
                                        HttpContext.Session.SetString("PNRR", res + "@@" + UniversalLocatorCode);
                                    }
                                    #endregion
                                }


                            }
                            //}
                            //k1++;
                            //}
                            //}
                            //}
                        }
                        k1++;
                    }
                }
            }

            #endregion
            return RedirectToAction("RoundTripPaymentView", "RoundTripPaymentGateway", new { Guid = Guid });
        }

        public class Paxes
        {
            public List<passkeytype> Adults_ { get; set; }
            public List<passkeytype> Childs_ { get; set; }

            public List<passkeytype> Infant_ { get; set; }
        }
        Paxes _paxes = new Paxes();

        public Passenger[] GetPassenger(List<passkeytype> travellers_)
        {

            _paxes.Adults_ = new List<passkeytype>();
            _paxes.Childs_ = new List<passkeytype>();
            _paxes.Infant_ = new List<passkeytype>();
            for (int i = 0; i < travellers_.Count; i++)
            {
                if (travellers_[i].passengertypecode == "ADT")
                    _paxes.Adults_.Add(travellers_[i]);
                else if (travellers_[i].passengertypecode == "CHD")
                    _paxes.Childs_.Add(travellers_[i]);
                else if (travellers_[i].passengertypecode == "INFT")
                    _paxes.Infant_.Add(travellers_[i]);

            }

            HttpContext.Session.SetString("PaxArray", JsonConvert.SerializeObject(_paxes));

            Passenger[] passengers = null;
            try
            {



                int chdPax = 0;
                int infFax = 0;
                if (_paxes.Childs_ != null)
                {
                    chdPax = _paxes.Childs_.Count;
                }
                if (_paxes.Infant_ != null)
                {
                    infFax = _paxes.Infant_.Count;
                }
                passengers = new Passenger[_paxes.Adults_.Count + chdPax]; //Assign Passenger Information 
                Passenger p1 = null;
                int PassCnt = 0;
                for (int cntAdt = 0; cntAdt < _paxes.Adults_.Count; cntAdt++)
                {
                    p1 = new Passenger();
                    p1.PassengerNumberSpecified = true;
                    p1.PassengerNumber = Convert.ToInt16(PassCnt);
                    p1.Names = new BookingName[1];
                    p1.Names[0] = new BookingName();
                    if (!string.IsNullOrEmpty(_paxes.Adults_[cntAdt].first))
                    {
                        p1.Names[0].FirstName = Convert.ToString(_paxes.Adults_[cntAdt].first.Trim()).ToUpper();
                    }
                    if (!string.IsNullOrEmpty(_paxes.Adults_[cntAdt].middle))
                    {
                        p1.Names[0].MiddleName = Convert.ToString(_paxes.Adults_[cntAdt].middle.Trim()).ToUpper();
                    }
                    if (!string.IsNullOrEmpty(_paxes.Adults_[cntAdt].last))
                    {
                        p1.Names[0].LastName = Convert.ToString(_paxes.Adults_[cntAdt].last.Trim()).ToUpper();
                    }
                    p1.Names[0].Title = _paxes.Adults_[cntAdt].title.ToUpper().Replace(".", "");
                    p1.PassengerInfo = new PassengerInfo();
                    if (_paxes.Adults_[cntAdt].title.ToUpper().Replace(".", "") == "MR")
                    {
                        p1.PassengerInfo.Gender = Gender.Male;
                        p1.PassengerInfo.WeightCategory = WeightCategory.Male;
                    }
                    else
                    {
                        p1.PassengerInfo.Gender = Gender.Female;
                        p1.PassengerInfo.WeightCategory = WeightCategory.Female;
                    }
                    p1.PassengerTypeInfos = new PassengerTypeInfo[1];
                    p1.PassengerTypeInfos[0] = new PassengerTypeInfo();
                    p1.PassengerTypeInfos[0].DOBSpecified = true;
                    p1.PassengerTypeInfos[0].PaxType = _paxes.Adults_[cntAdt].passengertypecode.ToString().ToUpper();
                    if (_paxes.Infant_ != null && _paxes.Infant_.Count > 0)
                    {
                        if (cntAdt < _paxes.Infant_.Count)
                        {
                            p1.Infant = new PassengerInfant();
                            p1.Infant.DOBSpecified = true;
                            p1.Infant.DOB = Convert.ToDateTime(_paxes.Infant_[cntAdt].dateOfBirth);
                            if (_paxes.Infant_[cntAdt].title.ToUpper().Replace(".", "") == "MSTR")
                            {
                                p1.Infant.Gender = Gender.Male;
                            }
                            else
                            {
                                p1.Infant.Gender = Gender.Female;
                            }
                            p1.Infant.Names = new BookingName[1];
                            p1.Infant.Names[0] = new BookingName();
                            if (!string.IsNullOrEmpty(_paxes.Infant_[cntAdt].first))
                            {
                                p1.Infant.Names[0].FirstName = Convert.ToString(_paxes.Infant_[cntAdt].first.Trim());
                            }
                            if (!string.IsNullOrEmpty(_paxes.Infant_[cntAdt].middle))
                            {
                                p1.Infant.Names[0].MiddleName = Convert.ToString(_paxes.Infant_[cntAdt].middle.Trim());
                            }
                            if (!string.IsNullOrEmpty(_paxes.Infant_[cntAdt].last))
                            {
                                p1.Infant.Names[0].LastName = Convert.ToString(_paxes.Infant_[cntAdt].last.Trim());
                            }
                            p1.Infant.Names[0].Title = _paxes.Infant_[cntAdt].title.Replace(".", "");
                            p1.Infant.Nationality = _paxes.Infant_[cntAdt].nationality;
                            p1.Infant.ResidentCountry = _paxes.Infant_[cntAdt].residentCountry;
                            p1.State = MessageState.New;
                        }

                    }

                    passengers[PassCnt] = p1;
                    PassCnt++;
                }
                if (_paxes.Childs_ != null)
                {
                    for (int cntChd = 0; cntChd < _paxes.Childs_.Count; cntChd++)
                    {
                        p1 = new Passenger();

                        p1.PassengerNumberSpecified = true;
                        p1.PassengerNumber = Convert.ToInt16(PassCnt);
                        p1.Names = new BookingName[1];
                        p1.Names[0] = new BookingName();

                        if (!string.IsNullOrEmpty(_paxes.Childs_[cntChd].first))
                        {
                            p1.Names[0].FirstName = Convert.ToString(_paxes.Childs_[cntChd].first).ToUpper();
                        }
                        if (!string.IsNullOrEmpty(_paxes.Childs_[cntChd].middle))
                        {
                            p1.Names[0].MiddleName = Convert.ToString(_paxes.Childs_[cntChd].middle).ToUpper();
                        }
                        if (!string.IsNullOrEmpty(_paxes.Childs_[cntChd].last))
                        {
                            p1.Names[0].LastName = Convert.ToString(_paxes.Childs_[cntChd].last).ToUpper();
                        }
                        p1.Names[0].Title = _paxes.Childs_[cntChd].title.ToUpper().Replace(".", "");
                        p1.PassengerInfo = new PassengerInfo();
                        if (_paxes.Childs_[cntChd].title.ToUpper().Replace(".", "") == "MSTR")
                        {
                            p1.PassengerInfo.Gender = Gender.Male;
                            p1.PassengerInfo.WeightCategory = WeightCategory.Male;
                        }
                        else
                        {
                            p1.PassengerInfo.Gender = Gender.Female;
                            p1.PassengerInfo.WeightCategory = WeightCategory.Female;
                        }
                        p1.PassengerTypeInfos = new PassengerTypeInfo[1];
                        p1.PassengerTypeInfos[0] = new PassengerTypeInfo();
                        p1.PassengerTypeInfos[0].DOBSpecified = true;
                        p1.PassengerTypeInfos[0].PaxType = _paxes.Childs_[cntChd].passengertypecode.ToString().ToUpper();
                        passengers[PassCnt] = p1;
                        PassCnt++;
                    }
                }
            }
            catch (SystemException sex_)
            {
            }
            return passengers;
        }
    }
}