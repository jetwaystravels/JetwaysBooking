﻿using System.Collections;
using System.Data;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Bookingmanager_;
using DomainLayer.Model;
using DomainLayer.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Packaging.Signing;
using OnionArchitectureAPI.Services.Indigo;
using OnionConsumeWebAPI.Extensions;
using Utility;
using System.Text;
using static DomainLayer.Model.SeatMapResponceModel;
using OnionArchitectureAPI.Services.Travelport;
using Microsoft.IdentityModel.Tokens;
using static DomainLayer.Model.GDSResModel;
using OnionConsumeWebAPI.Models;

namespace OnionConsumeWebAPI.Controllers.RoundTrip
{
    public class ReturnTripsellController : Controller
    {

        string BaseURL = "https://dotrezapi.test.I5.navitaire.com";
        string BaseAkasaURL = "https://tbnk-reyalrb.qp.akasaair.com";
        string passengerkey12 = string.Empty;
        Logs logs = new Logs();
        TravelPort _objAvail = null;
        HttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();
        string _testURL = AppUrlConstant.GDSURL;
        string _targetBranch = string.Empty;
        string _userName = string.Empty;
        string _password = string.Empty;
        string newGuid = string.Empty;
        SpiceJetApiController objSpiceJet = new SpiceJetApiController();
        string HostTokenKey = string.Empty;
        string HostTokenValue = string.Empty;
        private readonly IConfiguration _configuration;

        public ReturnTripsellController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> ReturnTripsellView(List<string> fareKey, List<string> journeyKey, string Guid)
        {

            string journeySellKeyAA = "";
            string Supp = "";
            MongoHelper objMongoHelper = new MongoHelper();
            MongoSeatMealdetail seatMealdetail = new MongoSeatMealdetail();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();

            SearchLog searchLog = new SearchLog();
            searchLog = _mongoDBHelper.GetFlightSearchLog(Guid).Result;

            SimpleAvailabilityRequestModel availibiltyRQGDS = null;
            StringBuilder fareRepriceReq = new StringBuilder();
            string farebasisdata = string.Empty;
            SimpleAvailibilityaAddResponce Airfaredata = null;
            Airlinenameforcommit airlinenameforcommit = new Airlinenameforcommit();
            airlinenameforcommit.Airline = new List<string>();
            List<string> MainPassengerdata = new List<string>();
            List<string> MainSeatMapdata = new List<string>();
            List<string> MainMealsdata = new List<string>();

            List<string> Passengerdata = new List<string>();
            List<string> SeatMapdata = new List<string>();
            List<string> Mealsdata = new List<string>();

            List<string> _Passengerdata = new List<string>();
            List<string> _SeatMapdata = new List<string>();
            List<string> _Mealsdata = new List<string>();

            var flagsession = "NA";
            string airlineId = "0";
            var airlinename = "";
            string[] AirlineNamedesc = new string[2];
            string SeatMapres = string.Empty;
            for (int p = 0; p < fareKey.Count; p++)
            {

                if (fareKey[p].ToLower().Contains("indigo"))
                {
                    flagsession = "6E";
                    airlinename = "indigo";
                }
                else if (fareKey[p].ToLower().Contains("spicejet"))
                {
                    flagsession = "SG";
                    airlinename = "spicejet";
                }
                else if (fareKey[p].ToLower().Contains("airasia"))
                {
                    flagsession = "IX";
                    airlinename = "airasia";
                }
                else if (fareKey[p].ToLower().Contains("akasaair"))
                {
                    flagsession = "QP";
                    airlinename = "akasaair";
                }
                else if (fareKey[p].ToLower().Contains("vistara"))
                {
                    flagsession = "UK";
                    airlinename = "Vistara";
                }
                else if (fareKey[p].ToLower().Contains("airindia"))
                {
                    flagsession = "AI";
                    airlinename = "AirIndia";
                }
                else if (fareKey[p].ToLower().Contains("hehnair"))
                {
                    flagsession = "H1";
                    airlinename = "Hehnair";
                }

                airlinenameforcommit.Airline.Add(airlinename);


                #region AirAsia
                string token = string.Empty;
                AirAsiaTripResponceModel AirAsiaTripResponceobj = new AirAsiaTripResponceModel();
                List<_credentials> credentialslist = new List<_credentials>();
                string _JourneykeyData = string.Empty;
                string _FareKeyData = string.Empty;
                string _JourneyKeyOneway = journeyKey[p];
                string[] _Jparts = _JourneyKeyOneway.Split('@');
                string _JourneykeyRTData = _Jparts[2];
                string _journeySide = _Jparts[1];
                AirlineNamedesc[p] = _JourneykeyRTData;

              



                using (HttpClient client = new HttpClient())
                {
                    if (_JourneykeyRTData.ToLower() == "airasia")
                    {
                        airlineId = "4";
                        seatMealdetail.PSupp = "AirAsia";
                        seatMealdetail.Supp = "AirAsia";
                        Supp = "AirAsia";
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "AirAsia").Result;
                        string tokenview = string.Empty;
                        string infant = string.Empty;
                        if (_journeySide == "0j")
                        {
                            token = tokenData.Token;
                        }
                        else
                        {
                            token = tokenData.RToken;
                            tokenData.PassRequest = tokenData.PassRequestR;
                        }
                        if (token == "" || token == null)
                        {
                            return RedirectToAction("Index");
                        }
                        _mongoDBHelper.UpdateFlightTokenJourney(Guid, "AirAsia", JsonConvert.SerializeObject(journeyKey));
                        string Leftshowpopupdata = objMongoHelper.UnZip(tokenData.PassRequest);
                        SimpleAvailabilityRequestModel _SimpleAvailabilityobj = JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(Leftshowpopupdata);
                        var AdtType = "";
                        var AdtCount = 0;
                        var chdtype = "";
                        var chdcount = 0;
                        var infanttype = "";
                        var infantcount = 0;
                        int countpassenger = _SimpleAvailabilityobj.passengers.types.Count;
                        AdtType = _SimpleAvailabilityobj.passengers.types[0].type;
                        AirAsiaTripSellRequest AirAsiaTripSellRequestobj = new AirAsiaTripSellRequest();
                        _Key key = new _Key();
                        List<_Key> _keylist = new List<_Key>();


                        string JourneyKeyOneway = journeyKey[p];
                        string[] Jparts = JourneyKeyOneway.Split('@');
                        _JourneykeyData = Jparts[0];
                        key.journeyKey = _JourneykeyData;
                        journeySellKeyAA = JsonConvert.SerializeObject(_JourneykeyData);
                        string fareKeyKeyOneway = fareKey[p];
                        string[] Fparts = fareKeyKeyOneway.Split('@');
                        _FareKeyData = Fparts[0];
                        key.fareAvailabilityKey = _FareKeyData;

                        _keylist.Add(key);
                        AirAsiaTripSellRequestobj.keys = _keylist;

                        Passengers passengers = new Passengers();
                        List<_Types> _typeslist = new List<_Types>();

                        for (int i = 0; i < _SimpleAvailabilityobj.passengers.types.Count; i++)
                        {
                            _Types _Types = new _Types();

                            if (_SimpleAvailabilityobj.passengers.types[i].type == "ADT")
                            {
                                AdtType = _SimpleAvailabilityobj.passengers.types[i].type;
                                _Types.type = AdtType;
                                _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                            }
                            else if (_SimpleAvailabilityobj.passengers.types[i].type == "CHD")
                            {
                                chdtype = _SimpleAvailabilityobj.passengers.types[i].type;
                                _Types.type = chdtype;
                                _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                            }
                            else if (_SimpleAvailabilityobj.passengers.types[i].type == "INFT")
                            {
                                infanttype = _SimpleAvailabilityobj.passengers.types[i].type;
                                _Types.type = infanttype;
                                _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                            }
                            //	
                            _typeslist.Add(_Types);
                        }
                        List<_Types> _typeslistsell = new List<_Types>();
                        for (int i = 0; i < _typeslist.Count; i++)
                        {
                            if (_typeslist[i].type == "INFT")
                                continue;
                            _typeslistsell.Add(_typeslist[i]);

                        }
                        passengers.types = _typeslistsell;

                        AirAsiaTripSellRequestobj.passengers = passengers;
                        AirAsiaTripSellRequestobj.currencyCode = "INR";
                        AirAsiaTripSellRequestobj.preventOverlap = true;
                        AirAsiaTripSellRequestobj.suppressPassengerAgeValidation = true;
                        var AirasiaTripSellRequest = JsonConvert.SerializeObject(AirAsiaTripSellRequestobj, Formatting.Indented);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responseTripsell = await client.PostAsJsonAsync(AppUrlConstant.AirasiaTripsell, AirAsiaTripSellRequestobj);

                        if (responseTripsell.IsSuccessStatusCode)
                        {
                            AirAsiaTripResponceobj = new AirAsiaTripResponceModel();
                            var resultsTripsell = responseTripsell.Content.ReadAsStringAsync().Result;
                            if (p == 0)
                            {
                                logs.WriteLogsR(AirasiaTripSellRequest, "3-SellRequest_Left", "AirAsiaRT");
                                logs.WriteLogsR(resultsTripsell, "3-SellResponse_Left", "AirAsiaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(AirasiaTripSellRequest, "3-SellRequest_Right", "AirAsiaRT");
                                logs.WriteLogsR(resultsTripsell, "3-SellResponse_Right", "AirAsiaRT");
                            }
                            var JsonObjTripsell = JsonConvert.DeserializeObject<dynamic>(resultsTripsell);
                            var basefaretax = JsonObjTripsell.data.breakdown.journeyTotals.totalTax;
                            int journeyscount = JsonObjTripsell.data.journeys.Count;
                            List<AAJourney> AAJourneyList = new List<AAJourney>();
                            for (int i = 0; i < journeyscount; i++)
                            {
                                if (journeyscount == 2 && i == 0)
                                {
                                    continue;
                                }

                                AAJourney AAJourneyobj = new AAJourney();
                                AAJourneyobj.Airlinename = Airlines.Airasia.ToString();
                                AAJourneyobj.flightType = JsonObjTripsell.data.journeys[i].flightType;
                                AAJourneyobj.stops = JsonObjTripsell.data.journeys[i].stops;
                                AAJourneyobj.journeyKey = JsonObjTripsell.data.journeys[i].journeyKey;
                                var totalAmount = JsonObjTripsell.data.breakdown.journeys[AAJourneyobj.journeyKey].totalAmount;
                                var totalTax = JsonObjTripsell.data.breakdown.journeys[AAJourneyobj.journeyKey].totalTax;
                                AADesignator AADesignatorobj = new AADesignator();
                                AADesignatorobj.origin = JsonObjTripsell.data.journeys[0].designator.origin;
                                AADesignatorobj.destination = JsonObjTripsell.data.journeys[0].designator.destination;
                                AADesignatorobj.departure = JsonObjTripsell.data.journeys[0].designator.departure;
                                AADesignatorobj.arrival = JsonObjTripsell.data.journeys[0].designator.arrival;
                                AAJourneyobj.designator = AADesignatorobj;


                                int segmentscount = JsonObjTripsell.data.journeys[i].segments.Count;
                                List<AASegment> AASegmentlist = new List<AASegment>();
                                for (int j = 0; j < segmentscount; j++)
                                {
                                    AASegment AASegmentobj = new AASegment();
                                    AASegmentobj.isStandby = JsonObjTripsell.data.journeys[i].segments[j].isStandby;
                                    AASegmentobj.isHosted = JsonObjTripsell.data.journeys[i].segments[j].isHosted;
                                    AADesignator AASegmentDesignatorobj = new AADesignator();
                                    AASegmentDesignatorobj.origin = JsonObjTripsell.data.journeys[i].segments[j].designator.origin;
                                    AASegmentDesignatorobj.destination = JsonObjTripsell.data.journeys[i].segments[j].designator.destination;
                                    AASegmentDesignatorobj.departure = JsonObjTripsell.data.journeys[i].segments[j].designator.departure;
                                    AASegmentDesignatorobj.arrival = JsonObjTripsell.data.journeys[i].segments[j].designator.arrival;
                                    AASegmentobj.designator = AASegmentDesignatorobj;

                                    int fareCount = JsonObjTripsell.data.journeys[i].segments[j].fares.Count;
                                    List<AAFare> AAFarelist = new List<AAFare>();
                                    for (int k = 0; k < fareCount; k++)
                                    {
                                        AAFare AAFareobj = new AAFare();
                                        AAFareobj.fareKey = JsonObjTripsell.data.journeys[i].segments[j].fares[k].fareKey;
                                        AAFareobj.productClass = JsonObjTripsell.data.journeys[i].segments[j].fares[k].productClass;

                                        var passengerFares = JsonObjTripsell.data.journeys[i].segments[j].fares[k].passengerFares;

                                        int passengerFarescount = ((Newtonsoft.Json.Linq.JContainer)passengerFares).Count;
                                        List<AAPassengerfare> AAPassengerfarelist = new List<AAPassengerfare>();
                                        for (int l = 0; l < passengerFarescount; l++)
                                        {
                                            AAPassengerfare AAPassengerfareobj = new AAPassengerfare();
                                            AAPassengerfareobj.passengerType = JsonObjTripsell.data.journeys[i].segments[j].fares[k].passengerFares[l].passengerType;

                                            var serviceCharges1 = JsonObjTripsell.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges;
                                            int serviceChargescount = ((Newtonsoft.Json.Linq.JContainer)serviceCharges1).Count;
                                            List<AAServicecharge> AAServicechargelist = new List<AAServicecharge>();
                                            for (int m = 0; m < serviceChargescount; m++)
                                            {
                                                AAServicecharge AAServicechargeobj = new AAServicecharge();
                                                AAServicechargeobj.amount = JsonObjTripsell.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges[m].amount;

                                                AAServicechargelist.Add(AAServicechargeobj);
                                            }
                                            AAPassengerfareobj.serviceCharges = AAServicechargelist;
                                            AAPassengerfarelist.Add(AAPassengerfareobj);
                                        }
                                        AAFareobj.passengerFares = AAPassengerfarelist;

                                        AAFarelist.Add(AAFareobj);

                                    }
                                    AASegmentobj.fares = AAFarelist;
                                    AAIdentifier AAIdentifierobj = new AAIdentifier();

                                    AAIdentifierobj.identifier = JsonObjTripsell.data.journeys[i].segments[j].identifier.identifier;
                                    AAIdentifierobj.carrierCode = JsonObjTripsell.data.journeys[i].segments[j].identifier.carrierCode;

                                    AASegmentobj.identifier = AAIdentifierobj;

                                    var leg = JsonObjTripsell.data.journeys[i].segments[j].legs;
                                    int legcount = ((Newtonsoft.Json.Linq.JContainer)leg).Count;
                                    List<AALeg> AALeglist = new List<AALeg>();
                                    for (int n = 0; n < legcount; n++)
                                    {
                                        AALeg AALeg = new AALeg();
                                        AALeg.legKey = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legKey;
                                        AADesignator AAlegDesignatorobj = new AADesignator();
                                        AAlegDesignatorobj.origin = JsonObjTripsell.data.journeys[i].segments[j].legs[n].designator.origin;
                                        AAlegDesignatorobj.destination = JsonObjTripsell.data.journeys[i].segments[j].legs[n].designator.destination;
                                        AAlegDesignatorobj.departure = JsonObjTripsell.data.journeys[i].segments[j].legs[n].designator.departure;
                                        AAlegDesignatorobj.arrival = JsonObjTripsell.data.journeys[i].segments[j].legs[n].designator.arrival;
                                        AALeg.designator = AAlegDesignatorobj;

                                        AALeginfo AALeginfoobj = new AALeginfo();
                                        AALeginfoobj.arrivalTerminal = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal;
                                        AALeginfoobj.arrivalTime = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legInfo.arrivalTime;
                                        AALeginfoobj.departureTerminal = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal;
                                        AALeginfoobj.departureTime = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legInfo.departureTime;
                                        AALeg.legInfo = AALeginfoobj;
                                        AALeglist.Add(AALeg);
                                    }

                                    AASegmentobj.legs = AALeglist;
                                    AASegmentlist.Add(AASegmentobj);
                                }

                                AAJourneyobj.segments = AASegmentlist;
                                AAJourneyList.Add(AAJourneyobj);
                            }


                            var passanger = JsonObjTripsell.data.passengers;
                            int passengercount = ((Newtonsoft.Json.Linq.JContainer)passanger).Count;

                            List<AAPassengers> passkeylist = new List<AAPassengers>();

                            foreach (var items in JsonObjTripsell.data.passengers)
                            {
                                AAPassengers passkeytypeobj = new AAPassengers();
                                passkeytypeobj.passengerKey = items.Value.passengerKey;
                                passkeytypeobj.passengerTypeCode = items.Value.passengerTypeCode;
                                passkeytypeobj._Airlinename = _JourneykeyRTData;
                                passkeylist.Add(passkeytypeobj);
                                passengerkey12 = passkeytypeobj.passengerKey;

                            }

                            #region  for passenger view list
                            for (int i = 0; i < _typeslist.Count; i++)
                            {
                                if (_typeslist[i].type == "INFT")
                                {
                                    for (int i1 = 0; i1 < _typeslist[i].count; i1++)
                                    {
                                        AAPassengers passkeytypeobj = new AAPassengers();
                                        passkeytypeobj.passengerKey = "";
                                        passkeytypeobj.passengerTypeCode = "INFT";
                                        passkeylist.Add(passkeytypeobj);
                                    }
                                }
                            }
                            #endregion
                            AirAsiaTripResponceobj.basefaretax = basefaretax;
                            AirAsiaTripResponceobj.journeys = AAJourneyList;
                            AirAsiaTripResponceobj.passengers = passkeylist;
                            AirAsiaTripResponceobj.passengerscount = passengercount;


                            #region Itenary 
                            AirAsiaTripResponceModel AirAsiaTripResponceobject = new AirAsiaTripResponceModel();
                            if (infanttype != null && infanttype != "")
                            {
                                //  string passengerdatainfant = HttpContext.Session.GetString("keypassenger");
                                string passengerdatainfant = JsonConvert.SerializeObject(AirAsiaTripResponceobj);
                                AirAsiaTripResponceModel passeengerKeyListinfant = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passengerdatainfant, typeof(AirAsiaTripResponceModel));
                                SimpleAvailabilityRequestModel _SimpleAvailabilityobject = new SimpleAvailabilityRequestModel();
                                var jsonDataObject = objMongoHelper.UnZip(tokenData.PassRequest);
                                _SimpleAvailabilityobject = JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(jsonDataObject.ToString());
                                GetItenaryModel itenaryInfant = new GetItenaryModel();
                                List<Ssr1> ssr1slist = new List<Ssr1>();
                                Ssr1 ssr1 = new Ssr1();
                                Market marketobj = new Market();
                                Identifier1 identifier1 = new Identifier1();
                                //Market
                                identifier1.identifier = passeengerKeyListinfant.journeys[0].segments[0].identifier.identifier;
                                identifier1.carrierCode = passeengerKeyListinfant.journeys[0].segments[0].identifier.carrierCode;
                                marketobj.identifier = identifier1;
                                marketobj.destination = passeengerKeyListinfant.journeys[0].segments[0].designator.destination;
                                marketobj.origin = passeengerKeyListinfant.journeys[0].segments[0].designator.origin;
                                marketobj.departureDate = _SimpleAvailabilityobject.beginDate;
                                if (p == 1)
                                {
                                    marketobj.departureDate = _SimpleAvailabilityobject.endDate;
                                }
                                ssr1.market = marketobj;
                                ssr1slist.Add(ssr1);
                                //item
                                List<Item> itemList = new List<Item>();
                                int typecount = _SimpleAvailabilityobject.passengers.types.Count;
                                for (int i = 0; i < typecount; i++)
                                {
                                    infant = _SimpleAvailabilityobject.passengers.types[i].type;
                                    if (infant == "INFT")
                                    {
                                        int infantCount1 = _SimpleAvailabilityobject.passengers.types[i].count;

                                        for (int j = 0; j < infantCount1; j++)
                                        {

                                            Item itemobj = new Item();
                                            List<SsrItem> ssrItemslist = new List<SsrItem>();
                                            SsrItem ssrItemobj = new SsrItem();

                                            ssrItemobj.ssrCode = "INFT";
                                            ssrItemobj.count = 1;
                                            ssrItemslist.Add(ssrItemobj);
                                            Designatorr designatorr = new Designatorr();
                                            designatorr.destination = passeengerKeyListinfant.journeys[0].segments[0].designator.destination;
                                            designatorr.origin = passeengerKeyListinfant.journeys[0].segments[0].designator.origin;
                                            designatorr.departureDate = _SimpleAvailabilityobject.beginDate;
                                            ssrItemobj.designator = designatorr;
                                            itemobj.passengerType = passeengerKeyListinfant.passengers[0].passengerTypeCode;
                                            itemobj.ssrs = ssrItemslist;
                                            itemList.Add(itemobj);
                                        }
                                    }

                                }
                                ssr1.items = itemList;
                                itenaryInfant.ssrs = ssr1slist;
                                List<Key> keylist = new List<Key>();
                                Key Keyobj = new Key();
                                Keyobj.journeyKey = journeyKey[0];
                                Keyobj.fareAvailabilityKey = fareKey[0];
                                JourneyKeyOneway = journeyKey[p];
                                Jparts = JourneyKeyOneway.Split('@');
                                _JourneykeyData = Jparts[0];
                                Keyobj.journeyKey = _JourneykeyData;


                                fareKeyKeyOneway = fareKey[p];
                                Fparts = fareKeyKeyOneway.Split('@');
                                _FareKeyData = Fparts[0];
                                Keyobj.fareAvailabilityKey = _FareKeyData;

                                Keyobj.standbyPriorityCode = "";
                                Keyobj.inventoryControl = "HoldSpace";
                                keylist.Add(Keyobj);
                                itenaryInfant.keys = keylist;
                                Passengers1 passengers1 = new Passengers1();
                                passengers1.residentCountry = "IN";
                                List<Type2> typelist = new List<Type2>();
                                for (int i = 0; i < _SimpleAvailabilityobj.passengers.types.Count; i++)
                                {
                                    Type2 _Types = new Type2();

                                    if (_SimpleAvailabilityobj.passengers.types[i].type == "ADT")
                                    {
                                        AdtType = _SimpleAvailabilityobj.passengers.types[i].type;
                                        _Types.type = AdtType;
                                        _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                                    }
                                    else if (_SimpleAvailabilityobj.passengers.types[i].type == "CHD")
                                    {
                                        chdtype = _SimpleAvailabilityobj.passengers.types[i].type;
                                        _Types.type = chdtype;
                                        _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                                    }
                                    else if (_SimpleAvailabilityobj.passengers.types[i].type == "INFT")
                                    {
                                        infanttype = _SimpleAvailabilityobj.passengers.types[i].type;
                                        continue;
                                    }
                                    typelist.Add(_Types);
                                }
                                passengers1.types = typelist;
                                itenaryInfant.passengers = passengers1;
                                itenaryInfant.currencyCode = "INR";
                                if (infant == "INFT")
                                {
                                    var jsonPassengers = JsonConvert.SerializeObject(itenaryInfant, Formatting.Indented);
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.Airasiainfantquote, itenaryInfant);
                                    if (responsePassengers.IsSuccessStatusCode)
                                    {
                                        AirAsiaTripResponceobject = new AirAsiaTripResponceModel();
                                        var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                        if (p == 0)
                                        {
                                            logs.WriteLogsR(jsonPassengers, "4-ItenaryRequest_Left", "AirAsiaRT");
                                            logs.WriteLogsR(_responsePassengers, "4-ItenaryResponse_Left", "AirAsiaRT");
                                        }
                                        else
                                        {
                                            logs.WriteLogsR(jsonPassengers, "4-ItenaryRequest_Right", "AirAsiaRT");
                                            logs.WriteLogsR(_responsePassengers, "4-ItenaryResponse_Right", "AirAsiaRT");
                                        }
                                        var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                                        int Journeyscount = JsonObjPassengers.data.journeys.Count;
                                        int Inftcount = 0;
                                        int Inftbasefare = 0;
                                        AAJourneyList = new List<AAJourney>();
                                        for (int i = 0; i < Journeyscount; i++)
                                        {
                                            AAJourney AAJourneyobject = new AAJourney();
                                            AAJourneyobject.flightType = JsonObjPassengers.data.journeys[i].flightType;
                                            AAJourneyobject.stops = JsonObjPassengers.data.journeys[i].stops;
                                            AAJourneyobject.journeyKey = JsonObjPassengers.data.journeys[i].journeyKey;
                                            var TotalAmount = JsonObjPassengers.data.breakdown.journeys[AAJourneyobject.journeyKey].totalAmount;
                                            var TotalTax = JsonObjPassengers.data.breakdown.journeys[AAJourneyobject.journeyKey].totalTax;
                                            AADesignator AADesignatorobject = new AADesignator();
                                            AADesignatorobject.origin = JsonObjPassengers.data.journeys[0].designator.origin;
                                            AADesignatorobject.destination = JsonObjPassengers.data.journeys[0].designator.destination;
                                            AADesignatorobject.departure = JsonObjPassengers.data.journeys[0].designator.departure;
                                            AADesignatorobject.arrival = JsonObjPassengers.data.journeys[0].designator.arrival;
                                            AAJourneyobject.designator = AADesignatorobject;

                                            int Segmentscount = JsonObjPassengers.data.journeys[i].segments.Count;
                                            List<AASegment> AASegmentlist = new List<AASegment>();
                                            for (int j = 0; j < Segmentscount; j++)
                                            {
                                                AASegment AASegmentobject = new AASegment();
                                                AASegmentobject.isStandby = JsonObjPassengers.data.journeys[i].segments[j].isStandby;
                                                AASegmentobject.isHosted = JsonObjPassengers.data.journeys[i].segments[j].isHosted;
                                                AADesignator AASegmentDesignatorobject = new AADesignator();
                                                AASegmentDesignatorobject.origin = JsonObjPassengers.data.journeys[i].segments[j].designator.origin;
                                                AASegmentDesignatorobject.destination = JsonObjPassengers.data.journeys[i].segments[j].designator.destination;
                                                AASegmentDesignatorobject.departure = JsonObjPassengers.data.journeys[i].segments[j].designator.departure;
                                                AASegmentDesignatorobject.arrival = JsonObjPassengers.data.journeys[i].segments[j].designator.arrival;
                                                AASegmentobject.designator = AASegmentDesignatorobject;

                                                int FareCount = JsonObjPassengers.data.journeys[i].segments[j].fares.Count;
                                                List<AAFare> AAFareList = new List<AAFare>();
                                                for (int k = 0; k < FareCount; k++)
                                                {
                                                    AAFare AAFareobject = new AAFare();
                                                    AAFareobject.fareKey = JsonObjPassengers.data.journeys[i].segments[j].fares[k].fareKey;
                                                    AAFareobject.productClass = JsonObjPassengers.data.journeys[i].segments[j].fares[k].productClass;

                                                    var PassengerFares = JsonObjPassengers.data.journeys[i].segments[j].fares[k].passengerFares;

                                                    int PassengerFarescount = ((Newtonsoft.Json.Linq.JContainer)PassengerFares).Count;
                                                    List<AAPassengerfare> AAPassengerfareList = new List<AAPassengerfare>();
                                                    for (int l = 0; l < PassengerFarescount; l++)
                                                    {
                                                        AAPassengerfare AAPassengerfareobject = new AAPassengerfare();
                                                        AAPassengerfareobject.passengerType = JsonObjPassengers.data.journeys[i].segments[j].fares[k].passengerFares[l].passengerType;
                                                        var ServiceCharges1 = JsonObjPassengers.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges;
                                                        int ServiceChargescount = ((Newtonsoft.Json.Linq.JContainer)ServiceCharges1).Count;
                                                        List<AAServicecharge> AAServicechargeList = new List<AAServicecharge>();
                                                        for (int m = 0; m < ServiceChargescount; m++)
                                                        {
                                                            AAServicecharge AAServicechargeobject = new AAServicecharge();
                                                            AAServicechargeobject.amount = JsonObjPassengers.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges[m].amount;
                                                            AAServicechargeList.Add(AAServicechargeobject);
                                                        }
                                                        AAPassengerfareobject.serviceCharges = AAServicechargeList;

                                                        AAPassengerfareList.Add(AAPassengerfareobject);
                                                    }
                                                    AAFareobject.passengerFares = AAPassengerfareList;

                                                    AAFareList.Add(AAFareobject);
                                                }
                                                AASegmentobject.fares = AAFareList;
                                                AAIdentifier AAIdentifierobj = new AAIdentifier();
                                                AAIdentifierobj.identifier = JsonObjPassengers.data.journeys[i].segments[j].identifier.identifier;
                                                AAIdentifierobj.carrierCode = JsonObjPassengers.data.journeys[i].segments[j].identifier.carrierCode;
                                                AASegmentobject.identifier = AAIdentifierobj;

                                                var Leg = JsonObjPassengers.data.journeys[i].segments[j].legs;
                                                int Legcount = ((Newtonsoft.Json.Linq.JContainer)Leg).Count;
                                                List<AALeg> AALeglist = new List<AALeg>();
                                                for (int n = 0; n < Legcount; n++)
                                                {
                                                    AALeg AALegobj = new AALeg();
                                                    AALegobj.legKey = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legKey;
                                                    AADesignator AAlegDesignatorobject = new AADesignator();
                                                    AAlegDesignatorobject.origin = JsonObjPassengers.data.journeys[i].segments[j].legs[n].designator.origin;
                                                    AAlegDesignatorobject.destination = JsonObjPassengers.data.journeys[i].segments[j].legs[n].designator.destination;
                                                    AAlegDesignatorobject.departure = JsonObjPassengers.data.journeys[i].segments[j].legs[n].designator.departure;
                                                    AAlegDesignatorobject.arrival = JsonObjPassengers.data.journeys[i].segments[j].legs[n].designator.arrival;
                                                    AALegobj.designator = AAlegDesignatorobject;

                                                    AALeginfo AALeginfoobject = new AALeginfo();
                                                    AALeginfoobject.arrivalTerminal = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal;
                                                    AALeginfoobject.arrivalTime = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legInfo.arrivalTime;
                                                    AALeginfoobject.departureTerminal = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal;
                                                    AALeginfoobject.departureTime = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legInfo.departureTime;
                                                    AALegobj.legInfo = AALeginfoobject;
                                                    AALeglist.Add(AALegobj);
                                                }
                                                AASegmentobject.legs = AALeglist;
                                                AASegmentlist.Add(AASegmentobject);
                                            }
                                            AAJourneyobject.segments = AASegmentlist;
                                            AAJourneyList.Add(AAJourneyobject);
                                        }

                                        int ServiceInfttax = 0;
                                        var Passanger = JsonObjPassengers.data.passengers;
                                        passengercount = ((Newtonsoft.Json.Linq.JContainer)Passanger).Count;
                                        List<AAPassengers> passkeyList = new List<AAPassengers>();
                                        Infant infantobject = null;
                                        DomainLayer.Model.Fee feeobject = null;
                                        foreach (var items in JsonObjPassengers.data.passengers)
                                        {
                                            AAPassengers passkeytypeobject = new AAPassengers();
                                            passkeytypeobject.passengerKey = items.Value.passengerKey;
                                            passkeytypeobject.passengerTypeCode = items.Value.passengerTypeCode;
                                            passkeyList.Add(passkeytypeobject);
                                            passengerkey12 = passkeytypeobject.passengerKey;
                                            //infant
                                            if (passkeytypeobject.passengerTypeCode != "CHD")
                                            {

                                                if (JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant != null)
                                                {
                                                    int Feecount = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees.Count;
                                                    //Vinay Infant Base 
                                                    Inftcount += Feecount;
                                                    Inftbasefare = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[0].serviceCharges[0].amount;
                                                    var ServiceInft = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[0].serviceCharges;
                                                    int ServiceInftcount = ((Newtonsoft.Json.Linq.JContainer)ServiceInft).Count;

                                                    for (int inf = 1; inf < ServiceInftcount; inf++)
                                                    {
                                                        ServiceInfttax = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[0].serviceCharges[inf].amount;
                                                        ServiceInfttax += ServiceInfttax;
                                                    }
                                                    Inftbasefare = Inftbasefare - ServiceInfttax;
                                                    List<DomainLayer.Model.Fee> feeList = new List<DomainLayer.Model.Fee>();
                                                    for (int i = 0; i < Feecount; i++)
                                                    {
                                                        infantobject = new Infant();
                                                        feeobject = new DomainLayer.Model.Fee();
                                                        feeobject.isConfirmed = false;
                                                        feeobject.isConfirming = false;
                                                        feeobject.isConfirmingExternal = false;
                                                        feeobject.code = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[i].code;
                                                        feeobject._override = false;
                                                        feeobject.note = "";
                                                        feeobject.isProtected = false;
                                                        infantobject.nationality = "";
                                                        infantobject.dateOfBirth = "";
                                                        infantobject.travelDocuments = "";
                                                        infantobject.residentCountry = "";
                                                        infantobject.gender = 1;
                                                        infantobject.name = "";
                                                        infantobject.type = "";
                                                        feeList.Add(feeobject);
                                                        infantobject.fees = feeList;
                                                        passkeytypeobject.infant = infantobject;
                                                        ServicechargeInfant servicechargeInfantobj = new ServicechargeInfant();
                                                        var serviceChargesCount = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[i].serviceCharges.Count;
                                                        servicechargeInfantobj.amount = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[i].serviceCharges[0].amount;
                                                        feeobject.ServicechargeInfant = servicechargeInfantobj;
                                                    }
                                                }
                                            }
                                            AirAsiaTripResponceobject.inftcount = Inftcount;
                                            AirAsiaTripResponceobject.inftbasefare = Inftbasefare;
                                            AirAsiaTripResponceobject.infttax = ServiceInfttax;
                                            AirAsiaTripResponceobject.journeys = AAJourneyList;
                                            AirAsiaTripResponceobject.passengers = passkeyList;
                                            AirAsiaTripResponceobject.passengerscount = passengercount;
                                            // HttpContext.Session.SetString("keypassengerItanary", JsonConvert.SerializeObject(AirAsiaTripResponceobject));
                                            seatMealdetail.Infant = JsonConvert.SerializeObject(AirAsiaTripResponceobject);

                                        }
                                    }
                                    else
                                    {
                                        var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                        if (p == 0)
                                        {
                                            logs.WriteLogsR(jsonPassengers, "4-ItenaryRequest_Left", "AirAsiaRT");
                                            logs.WriteLogsR(_responsePassengers, "4-ItenaryResponse_Left", "AirAsiaRT");
                                        }
                                        else
                                        {
                                            logs.WriteLogsR(jsonPassengers, "4-ItenaryRequest_Right", "AirAsiaRT");
                                            logs.WriteLogsR(_responsePassengers, "4-ItenaryResponse_Right", "AirAsiaRT");
                                        }
                                    }

                                }
                            }
                            #endregion
                            AirAsiaTripResponceobj.inftbasefare = AirAsiaTripResponceobject.inftbasefare;
                            AirAsiaTripResponceobj.infttax = AirAsiaTripResponceobject.infttax;
                            _Passengerdata.Add("<Start>" + JsonConvert.SerializeObject(AirAsiaTripResponceobj) + "<End>");
                            // HttpContext.Session.SetString("keypassenger", JsonConvert.SerializeObject(AirAsiaTripResponceobj));

                            seatMealdetail.KPassenger = objMongoHelper.Zip(JsonConvert.SerializeObject(AirAsiaTripResponceobj));


                            //  HttpContext.Session.SetString("_keypassengerdata", JsonConvert.SerializeObject(_Passengerdata));

                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(_Passengerdata)))
                            {
                                if (_Passengerdata.Count == 2)
                                {
                                    MainPassengerdata = new List<string>();
                                }
                                MainPassengerdata.Add(JsonConvert.SerializeObject(_Passengerdata));
                            }
                        }
                        else
                        {
                            var resultsTripsell = responseTripsell.Content.ReadAsStringAsync().Result;
                            if (resultsTripsell.Contains("rawMessage") && resultsTripsell.Contains("errors"))
                            {
                                AirAsiaTripResponceobj.ErrorMsg = Regex.Match(resultsTripsell.ToString(), "rawMessage\":\"(?<msg>[\\s\\S]*?)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["msg"].Value;
                            }

                            if (p == 0)
                            {
                                logs.WriteLogsR(AirasiaTripSellRequest, "3-SellRequest_Left", "AirAsiaRT");
                                logs.WriteLogsR(resultsTripsell, "3-SellResponse_Left", "AirAsiaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(AirasiaTripSellRequest, "3-SellRequest_Right", "AirAsiaRT");
                                logs.WriteLogsR(resultsTripsell, "3-SellResponse_Right", "AirAsiaRT");
                            }
                            _Passengerdata = new List<string>();
                            seatMealdetail.KPassenger = objMongoHelper.Zip(JsonConvert.SerializeObject(AirAsiaTripResponceobj));
                            _Passengerdata.Add("<Start>" + JsonConvert.SerializeObject(AirAsiaTripResponceobj) + "<End>");
                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(_Passengerdata)))
                            {
                                if (_Passengerdata.Count == 2)
                                {
                                    MainPassengerdata = new List<string>();
                                }
                                MainPassengerdata.Add(JsonConvert.SerializeObject(_Passengerdata));
                            }
                        }



                    }


                    #endregion

                    //AkasaAir  TripSell
                    #region AkasaAir
                    token = string.Empty;
                    AirAsiaTripResponceModel AkasaAirTripResponceobj = new AirAsiaTripResponceModel();
                    credentialslist = new List<_credentials>();
                    _JourneykeyData = string.Empty;
                    _FareKeyData = string.Empty;
                    _JourneyKeyOneway = journeyKey[p];
                    _Jparts = _JourneyKeyOneway.Split('@');
                    _JourneykeyRTData = _Jparts[2];
                    _journeySide = _Jparts[1];
                    AirlineNamedesc[p] = _JourneykeyRTData;

                    if (_JourneykeyRTData.ToLower() == "akasaair")
                    {
                        airlineId = "5";
                        Supp = "Akasa";
                        seatMealdetail.Supp = "AirAsia";
                        seatMealdetail.PSupp = "Akasa";
                        string tokenview = string.Empty;
                        string infant = string.Empty;
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;
                        if (_journeySide == "0j")
                        {
                            token = tokenData.Token;
                        }
                        else
                        {
                            token = tokenData.RToken;
                        }

                        if (string.IsNullOrEmpty(token))
                        {
                            return RedirectToAction("Index");
                        }

                        _mongoDBHelper.UpdateFlightTokenJourney(Guid, "Akasa", JsonConvert.SerializeObject(journeyKey));
                        string Leftshowpopupdata = objMongoHelper.UnZip(tokenData.PassRequest); //HttpContext.Session.GetString("PassengerModel");
                        SimpleAvailabilityRequestModel _SimpleAvailabilityobj = JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(Leftshowpopupdata);
                        var AdtType = "";
                        var AdtCount = 0;
                        var chdtype = "";
                        var chdcount = 0;
                        var infanttype = "";
                        var infantcount = 0;
                        int countpassenger = _SimpleAvailabilityobj.passengers.types.Count;
                        AdtType = _SimpleAvailabilityobj.passengers.types[0].type;
                        AirAsiaTripSellRequest AkasaAirTripSellRequestobj = new AirAsiaTripSellRequest();
                        _Key key = new _Key();
                        List<_Key> _keylist = new List<_Key>();


                        string JourneyKeyOneway = journeyKey[p];
                        string[] Jparts = JourneyKeyOneway.Split('@');
                        _JourneykeyData = Jparts[0];
                        key.journeyKey = _JourneykeyData;

                        HttpContext.Session.SetString("journeySellKey", JsonConvert.SerializeObject(_JourneykeyData));

                        string fareKeyKeyOneway = fareKey[p];
                        string[] Fparts = fareKeyKeyOneway.Split('@');
                        _FareKeyData = Fparts[0];
                        key.fareAvailabilityKey = _FareKeyData;
                        _keylist.Add(key);
                        AkasaAirTripSellRequestobj.keys = _keylist;
                        Passengers passengers = new Passengers();
                        List<_Types> _typeslist = new List<_Types>();
                        for (int i = 0; i < _SimpleAvailabilityobj.passengers.types.Count; i++)
                        {
                            _Types _Types = new _Types();
                            if (_SimpleAvailabilityobj.passengers.types[i].type == "ADT")
                            {
                                AdtType = _SimpleAvailabilityobj.passengers.types[i].type;
                                _Types.type = AdtType;
                                _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                            }
                            else if (_SimpleAvailabilityobj.passengers.types[i].type == "CHD")
                            {
                                chdtype = _SimpleAvailabilityobj.passengers.types[i].type;
                                _Types.type = chdtype;
                                _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                            }
                            else if (_SimpleAvailabilityobj.passengers.types[i].type == "INFT")
                            {
                                infanttype = _SimpleAvailabilityobj.passengers.types[i].type;
                                _Types.type = infanttype;
                                _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                            }
                            _typeslist.Add(_Types);
                        }
                        List<_Types> _typeslistsell = new List<_Types>();
                        for (int i = 0; i < _typeslist.Count; i++)
                        {
                            if (_typeslist[i].type == "INFT")
                                continue;
                            _typeslistsell.Add(_typeslist[i]);

                        }
                        passengers.types = _typeslistsell;


                        AkasaAirTripSellRequestobj.passengers = passengers;
                        AkasaAirTripSellRequestobj.currencyCode = "INR";
                        var AkasaAirTripSellRequest = JsonConvert.SerializeObject(AkasaAirTripSellRequestobj, Formatting.Indented);

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responseTripsell = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirTripsell, AkasaAirTripSellRequestobj);

                        if (responseTripsell.IsSuccessStatusCode)
                        {
                            AirAsiaTripResponceobj = new AirAsiaTripResponceModel();
                            var resultsTripsell = responseTripsell.Content.ReadAsStringAsync().Result;
                            if (p == 0)
                            {
                                logs.WriteLogsR(AkasaAirTripSellRequest, "3-SellRequest_Left", "AkasaRT");
                                logs.WriteLogsR(resultsTripsell, "3-SellResponse_Left", "AkasaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(AkasaAirTripSellRequest, "3-SellRequest_Right", "AkasaRT");
                                logs.WriteLogsR(resultsTripsell, "3-SellResponse_Right", "AkasaRT");
                            }
                            var JsonObjTripsell = JsonConvert.DeserializeObject<dynamic>(resultsTripsell);

                            var basefaretax = JsonObjTripsell.data.breakdown.journeyTotals.totalTax;
                            int journeyscount = JsonObjTripsell.data.journeys.Count;
                            List<AAJourney> AAJourneyList = new List<AAJourney>();
                            for (int i = 0; i < journeyscount; i++)
                            {
                                if (journeyscount == 2 && i == 0)
                                {
                                    continue;
                                }

                                AAJourney AAJourneyobj = new AAJourney();
                                AAJourneyobj.Airlinename = Airlines.AkasaAir.ToString();
                                AAJourneyobj.flightType = JsonObjTripsell.data.journeys[i].flightType;
                                AAJourneyobj.stops = JsonObjTripsell.data.journeys[i].stops;
                                AAJourneyobj.journeyKey = JsonObjTripsell.data.journeys[i].journeyKey;
                                var totalAmount = JsonObjTripsell.data.breakdown.journeys[AAJourneyobj.journeyKey].totalAmount;
                                var totalTax = JsonObjTripsell.data.breakdown.journeys[AAJourneyobj.journeyKey].totalTax;
                                AADesignator AADesignatorobj = new AADesignator();
                                AADesignatorobj.origin = JsonObjTripsell.data.journeys[0].designator.origin;
                                AADesignatorobj.destination = JsonObjTripsell.data.journeys[0].designator.destination;
                                AADesignatorobj.departure = JsonObjTripsell.data.journeys[0].designator.departure;
                                AADesignatorobj.arrival = JsonObjTripsell.data.journeys[0].designator.arrival;
                                AAJourneyobj.designator = AADesignatorobj;


                                int segmentscount = JsonObjTripsell.data.journeys[i].segments.Count;
                                List<AASegment> AASegmentlist = new List<AASegment>();
                                for (int j = 0; j < segmentscount; j++)
                                {
                                    AASegment AASegmentobj = new AASegment();
                                    AASegmentobj.isStandby = JsonObjTripsell.data.journeys[i].segments[j].isStandby;
                                    AASegmentobj.isHosted = JsonObjTripsell.data.journeys[i].segments[j].isHosted;
                                    AADesignator AASegmentDesignatorobj = new AADesignator();
                                    AASegmentDesignatorobj.origin = JsonObjTripsell.data.journeys[i].segments[j].designator.origin;
                                    AASegmentDesignatorobj.destination = JsonObjTripsell.data.journeys[i].segments[j].designator.destination;
                                    AASegmentDesignatorobj.departure = JsonObjTripsell.data.journeys[i].segments[j].designator.departure;
                                    AASegmentDesignatorobj.arrival = JsonObjTripsell.data.journeys[i].segments[j].designator.arrival;
                                    AASegmentobj.designator = AASegmentDesignatorobj;

                                    int fareCount = JsonObjTripsell.data.journeys[i].segments[j].fares.Count;
                                    List<AAFare> AAFarelist = new List<AAFare>();
                                    for (int k = 0; k < fareCount; k++)
                                    {
                                        AAFare AAFareobj = new AAFare();
                                        AAFareobj.fareKey = JsonObjTripsell.data.journeys[i].segments[j].fares[k].fareKey;
                                        AAFareobj.productClass = JsonObjTripsell.data.journeys[i].segments[j].fares[k].productClass;

                                        var passengerFares = JsonObjTripsell.data.journeys[i].segments[j].fares[k].passengerFares;

                                        int passengerFarescount = ((Newtonsoft.Json.Linq.JContainer)passengerFares).Count;
                                        List<AAPassengerfare> AAPassengerfarelist = new List<AAPassengerfare>();
                                        for (int l = 0; l < passengerFarescount; l++)
                                        {
                                            AAPassengerfare AAPassengerfareobj = new AAPassengerfare();
                                            AAPassengerfareobj.passengerType = JsonObjTripsell.data.journeys[i].segments[j].fares[k].passengerFares[l].passengerType;

                                            var serviceCharges1 = JsonObjTripsell.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges;
                                            int serviceChargescount = ((Newtonsoft.Json.Linq.JContainer)serviceCharges1).Count;
                                            List<AAServicecharge> AAServicechargelist = new List<AAServicecharge>();
                                            for (int m = 0; m < serviceChargescount; m++)
                                            {
                                                AAServicecharge AAServicechargeobj = new AAServicecharge();
                                                AAServicechargeobj.amount = JsonObjTripsell.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges[m].amount;

                                                AAServicechargelist.Add(AAServicechargeobj);
                                            }
                                            AAPassengerfareobj.serviceCharges = AAServicechargelist;
                                            AAPassengerfarelist.Add(AAPassengerfareobj);
                                        }
                                        AAFareobj.passengerFares = AAPassengerfarelist;

                                        AAFarelist.Add(AAFareobj);

                                    }
                                    AASegmentobj.fares = AAFarelist;
                                    AAIdentifier AAIdentifierobj = new AAIdentifier();

                                    AAIdentifierobj.identifier = JsonObjTripsell.data.journeys[i].segments[j].identifier.identifier;
                                    AAIdentifierobj.carrierCode = JsonObjTripsell.data.journeys[i].segments[j].identifier.carrierCode;

                                    AASegmentobj.identifier = AAIdentifierobj;

                                    var leg = JsonObjTripsell.data.journeys[i].segments[j].legs;
                                    int legcount = ((Newtonsoft.Json.Linq.JContainer)leg).Count;
                                    List<AALeg> AALeglist = new List<AALeg>();
                                    for (int n = 0; n < legcount; n++)
                                    {
                                        AALeg AALeg = new AALeg();
                                        AALeg.legKey = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legKey;
                                        AADesignator AAlegDesignatorobj = new AADesignator();
                                        AAlegDesignatorobj.origin = JsonObjTripsell.data.journeys[i].segments[j].legs[n].designator.origin;
                                        AAlegDesignatorobj.destination = JsonObjTripsell.data.journeys[i].segments[j].legs[n].designator.destination;
                                        AAlegDesignatorobj.departure = JsonObjTripsell.data.journeys[i].segments[j].legs[n].designator.departure;
                                        AAlegDesignatorobj.arrival = JsonObjTripsell.data.journeys[i].segments[j].legs[n].designator.arrival;
                                        AALeg.designator = AAlegDesignatorobj;

                                        AALeginfo AALeginfoobj = new AALeginfo();
                                        AALeginfoobj.arrivalTerminal = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal;
                                        AALeginfoobj.arrivalTime = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legInfo.arrivalTime;
                                        AALeginfoobj.departureTerminal = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal;
                                        AALeginfoobj.departureTime = JsonObjTripsell.data.journeys[i].segments[j].legs[n].legInfo.departureTime;
                                        AALeg.legInfo = AALeginfoobj;
                                        AALeglist.Add(AALeg);
                                    }

                                    AASegmentobj.legs = AALeglist;
                                    AASegmentlist.Add(AASegmentobj);
                                }

                                AAJourneyobj.segments = AASegmentlist;
                                AAJourneyList.Add(AAJourneyobj);
                            }


                            var passanger = JsonObjTripsell.data.passengers;
                            int passengercount = ((Newtonsoft.Json.Linq.JContainer)passanger).Count;

                            List<AAPassengers> passkeylist = new List<AAPassengers>();

                            foreach (var items in JsonObjTripsell.data.passengers)
                            {
                                AAPassengers passkeytypeobj = new AAPassengers();
                                passkeytypeobj.passengerKey = items.Value.passengerKey;
                                passkeytypeobj.passengerTypeCode = items.Value.passengerTypeCode;
                                passkeytypeobj._Airlinename = _JourneykeyRTData;
                                passkeylist.Add(passkeytypeobj);
                                passengerkey12 = passkeytypeobj.passengerKey;

                            }

                            #region  for passenger view list
                            for (int i = 0; i < _typeslist.Count; i++)
                            {
                                if (_typeslist[i].type == "INFT")
                                {
                                    for (int i1 = 0; i1 < _typeslist[i].count; i1++)
                                    {
                                        AAPassengers passkeytypeobj = new AAPassengers();
                                        passkeytypeobj.passengerKey = "";
                                        passkeytypeobj.passengerTypeCode = "INFT";
                                        passkeylist.Add(passkeytypeobj);
                                    }

                                }
                            }
                            #endregion
                            AirAsiaTripResponceobj.basefaretax = basefaretax;
                            AirAsiaTripResponceobj.journeys = AAJourneyList;
                            AirAsiaTripResponceobj.passengers = passkeylist;
                            AirAsiaTripResponceobj.passengerscount = passengercount;


                            #region Itenary 
                            AirAsiaTripResponceModel AirAsiaTripResponceobject = new AirAsiaTripResponceModel();
                            if (infanttype != null && infanttype != "")
                            {
                                // string passengerdatainfant = HttpContext.Session.GetString("keypassenger");
                                string passengerdatainfant = JsonConvert.SerializeObject(AirAsiaTripResponceobj);

                                AirAsiaTripResponceModel passeengerKeyListinfant = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passengerdatainfant, typeof(AirAsiaTripResponceModel));
                                SimpleAvailabilityRequestModel _SimpleAvailabilityobject = new SimpleAvailabilityRequestModel();
                                //var jsonDataObject = TempData["PassengerModel"];
                                var jsonDataObject = Leftshowpopupdata;  // HttpContext.Session.GetString("PassengerModel");
                                _SimpleAvailabilityobject = JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(jsonDataObject.ToString());
                                GetItenaryModel itenaryInfant = new GetItenaryModel();
                                List<Ssr1> ssr1slist = new List<Ssr1>();
                                Ssr1 ssr1 = new Ssr1();
                                Market marketobj = new Market();
                                Identifier1 identifier1 = new Identifier1();
                                //Market
                                identifier1.identifier = passeengerKeyListinfant.journeys[0].segments[0].identifier.identifier;
                                identifier1.carrierCode = passeengerKeyListinfant.journeys[0].segments[0].identifier.carrierCode;
                                marketobj.identifier = identifier1;
                                marketobj.destination = passeengerKeyListinfant.journeys[0].segments[0].designator.destination;
                                marketobj.origin = passeengerKeyListinfant.journeys[0].segments[0].designator.origin;
                                marketobj.departureDate = _SimpleAvailabilityobject.beginDate;
                                if (p == 1)
                                {
                                    marketobj.departureDate = _SimpleAvailabilityobject.endDate;
                                }
                                ssr1.market = marketobj;
                                ssr1slist.Add(ssr1);
                                //item
                                List<Item> itemList = new List<Item>();
                                int typecount = _SimpleAvailabilityobject.passengers.types.Count;

                                for (int i = 0; i < typecount; i++)
                                {
                                    infant = _SimpleAvailabilityobject.passengers.types[i].type;
                                    if (infant == "INFT")
                                    {
                                        int infantCount1 = _SimpleAvailabilityobject.passengers.types[i].count;

                                        for (int j = 0; j < infantCount1; j++)
                                        {

                                            Item itemobj = new Item();
                                            List<SsrItem> ssrItemslist = new List<SsrItem>();
                                            SsrItem ssrItemobj = new SsrItem();
                                            ssrItemobj.ssrCode = "INFT";
                                            ssrItemobj.count = 1;
                                            ssrItemslist.Add(ssrItemobj);
                                            Designatorr designatorr = new Designatorr();
                                            designatorr.destination = passeengerKeyListinfant.journeys[0].segments[0].designator.destination;
                                            designatorr.origin = passeengerKeyListinfant.journeys[0].segments[0].designator.origin;
                                            designatorr.departureDate = _SimpleAvailabilityobject.beginDate;
                                            ssrItemobj.designator = designatorr;
                                            itemobj.passengerType = passeengerKeyListinfant.passengers[0].passengerTypeCode;
                                            itemobj.ssrs = ssrItemslist;
                                            itemList.Add(itemobj);
                                        }
                                    }

                                }
                                ssr1.items = itemList;
                                itenaryInfant.ssrs = ssr1slist;
                                List<Key> keylist = new List<Key>();
                                Key Keyobj = new Key();
                                Keyobj.journeyKey = journeyKey[0];
                                Keyobj.fareAvailabilityKey = fareKey[0];
                                JourneyKeyOneway = journeyKey[p];
                                Jparts = JourneyKeyOneway.Split('@');
                                _JourneykeyData = Jparts[0];
                                Keyobj.journeyKey = _JourneykeyData;
                                fareKeyKeyOneway = fareKey[p];
                                Fparts = fareKeyKeyOneway.Split('@');
                                _FareKeyData = Fparts[0];
                                Keyobj.fareAvailabilityKey = _FareKeyData;
                                Keyobj.standbyPriorityCode = "";
                                Keyobj.inventoryControl = "HoldSpace";
                                keylist.Add(Keyobj);
                                itenaryInfant.keys = keylist;
                                Passengers1 passengers1 = new Passengers1();
                                passengers1.residentCountry = "IN";
                                List<Type2> typelist = new List<Type2>();
                                for (int i = 0; i < _SimpleAvailabilityobj.passengers.types.Count; i++)
                                {
                                    Type2 _Types = new Type2();

                                    if (_SimpleAvailabilityobj.passengers.types[i].type == "ADT")
                                    {
                                        AdtType = _SimpleAvailabilityobj.passengers.types[i].type;
                                        _Types.type = AdtType;
                                        _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                                    }
                                    else if (_SimpleAvailabilityobj.passengers.types[i].type == "CHD")
                                    {
                                        chdtype = _SimpleAvailabilityobj.passengers.types[i].type;
                                        _Types.type = chdtype;
                                        _Types.count = _SimpleAvailabilityobj.passengers.types[i].count;
                                    }
                                    else if (_SimpleAvailabilityobj.passengers.types[i].type == "INFT")
                                    {
                                        infanttype = _SimpleAvailabilityobj.passengers.types[i].type;
                                        continue;
                                    }
                                    typelist.Add(_Types);
                                }
                                passengers1.types = typelist;
                                itenaryInfant.passengers = passengers1;
                                itenaryInfant.currencyCode = "INR";
                                if (infant == "INFT")
                                {
                                    var jsonPassengers = JsonConvert.SerializeObject(itenaryInfant, Formatting.Indented);
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                    HttpResponseMessage responsePassengers = await client.PostAsJsonAsync(AppUrlConstant.Akasainfant, itenaryInfant);
                                    if (responsePassengers.IsSuccessStatusCode)
                                    {
                                        AirAsiaTripResponceobject = new AirAsiaTripResponceModel();
                                        var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                        if (p == 0)
                                        {
                                            logs.WriteLogsR(jsonPassengers, "4-ItenaryRequest_Left", "AkasaRT");
                                            logs.WriteLogsR(_responsePassengers, "4-ItenaryResponse_Left", "AkasaRT");

                                        }
                                        else
                                        {
                                            logs.WriteLogsR(jsonPassengers, "4-ItenaryRequest_Right", "AkasaRT");
                                            logs.WriteLogsR(_responsePassengers, "4-ItenaryResponse_Right", "AkasaRT");

                                        }
                                        var JsonObjPassengers = JsonConvert.DeserializeObject<dynamic>(_responsePassengers);
                                        int Journeyscount = JsonObjPassengers.data.journeys.Count;
                                        //end
                                        int Inftcount = 0;
                                        int Inftbasefare = 0;
                                        AAJourneyList = new List<AAJourney>();
                                        for (int i = 0; i < Journeyscount; i++)
                                        {
                                            AAJourney AAJourneyobject = new AAJourney();
                                            AAJourneyobject.flightType = JsonObjPassengers.data.journeys[i].flightType;
                                            AAJourneyobject.stops = JsonObjPassengers.data.journeys[i].stops;
                                            AAJourneyobject.journeyKey = JsonObjPassengers.data.journeys[i].journeyKey;
                                            var TotalAmount = JsonObjPassengers.data.breakdown.journeys[AAJourneyobject.journeyKey].totalAmount;
                                            var TotalTax = JsonObjPassengers.data.breakdown.journeys[AAJourneyobject.journeyKey].totalTax;
                                            AADesignator AADesignatorobject = new AADesignator();
                                            AADesignatorobject.origin = JsonObjPassengers.data.journeys[0].designator.origin;
                                            AADesignatorobject.destination = JsonObjPassengers.data.journeys[0].designator.destination;
                                            AADesignatorobject.departure = JsonObjPassengers.data.journeys[0].designator.departure;
                                            AADesignatorobject.arrival = JsonObjPassengers.data.journeys[0].designator.arrival;
                                            AAJourneyobject.designator = AADesignatorobject;

                                            int Segmentscount = JsonObjPassengers.data.journeys[i].segments.Count;
                                            List<AASegment> AASegmentlist = new List<AASegment>();
                                            for (int j = 0; j < Segmentscount; j++)
                                            {
                                                AASegment AASegmentobject = new AASegment();
                                                AASegmentobject.isStandby = JsonObjPassengers.data.journeys[i].segments[j].isStandby;
                                                AASegmentobject.isHosted = JsonObjPassengers.data.journeys[i].segments[j].isHosted;
                                                AADesignator AASegmentDesignatorobject = new AADesignator();
                                                AASegmentDesignatorobject.origin = JsonObjPassengers.data.journeys[i].segments[j].designator.origin;
                                                AASegmentDesignatorobject.destination = JsonObjPassengers.data.journeys[i].segments[j].designator.destination;
                                                AASegmentDesignatorobject.departure = JsonObjPassengers.data.journeys[i].segments[j].designator.departure;
                                                AASegmentDesignatorobject.arrival = JsonObjPassengers.data.journeys[i].segments[j].designator.arrival;
                                                AASegmentobject.designator = AASegmentDesignatorobject;

                                                int FareCount = JsonObjPassengers.data.journeys[i].segments[j].fares.Count;
                                                List<AAFare> AAFareList = new List<AAFare>();
                                                for (int k = 0; k < FareCount; k++)
                                                {
                                                    AAFare AAFareobject = new AAFare();
                                                    AAFareobject.fareKey = JsonObjPassengers.data.journeys[i].segments[j].fares[k].fareKey;
                                                    AAFareobject.productClass = JsonObjPassengers.data.journeys[i].segments[j].fares[k].productClass;

                                                    var PassengerFares = JsonObjPassengers.data.journeys[i].segments[j].fares[k].passengerFares;

                                                    int PassengerFarescount = ((Newtonsoft.Json.Linq.JContainer)PassengerFares).Count;
                                                    List<AAPassengerfare> AAPassengerfareList = new List<AAPassengerfare>();
                                                    for (int l = 0; l < PassengerFarescount; l++)
                                                    {
                                                        AAPassengerfare AAPassengerfareobject = new AAPassengerfare();
                                                        AAPassengerfareobject.passengerType = JsonObjPassengers.data.journeys[i].segments[j].fares[k].passengerFares[l].passengerType;
                                                        var ServiceCharges1 = JsonObjPassengers.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges;
                                                        int ServiceChargescount = ((Newtonsoft.Json.Linq.JContainer)ServiceCharges1).Count;
                                                        List<AAServicecharge> AAServicechargeList = new List<AAServicecharge>();
                                                        for (int m = 0; m < ServiceChargescount; m++)
                                                        {
                                                            AAServicecharge AAServicechargeobject = new AAServicecharge();
                                                            AAServicechargeobject.amount = JsonObjPassengers.data.journeys[i].segments[j].fares[k].passengerFares[l].serviceCharges[m].amount;
                                                            AAServicechargeList.Add(AAServicechargeobject);
                                                        }
                                                        AAPassengerfareobject.serviceCharges = AAServicechargeList;

                                                        AAPassengerfareList.Add(AAPassengerfareobject);
                                                    }
                                                    AAFareobject.passengerFares = AAPassengerfareList;

                                                    AAFareList.Add(AAFareobject);
                                                }
                                                AASegmentobject.fares = AAFareList;
                                                AAIdentifier AAIdentifierobj = new AAIdentifier();
                                                AAIdentifierobj.identifier = JsonObjPassengers.data.journeys[i].segments[j].identifier.identifier;
                                                AAIdentifierobj.carrierCode = JsonObjPassengers.data.journeys[i].segments[j].identifier.carrierCode;
                                                AASegmentobject.identifier = AAIdentifierobj;

                                                var Leg = JsonObjPassengers.data.journeys[i].segments[j].legs;
                                                int Legcount = ((Newtonsoft.Json.Linq.JContainer)Leg).Count;
                                                List<AALeg> AALeglist = new List<AALeg>();
                                                for (int n = 0; n < Legcount; n++)
                                                {
                                                    AALeg AALegobj = new AALeg();
                                                    AALegobj.legKey = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legKey;
                                                    AADesignator AAlegDesignatorobject = new AADesignator();
                                                    AAlegDesignatorobject.origin = JsonObjPassengers.data.journeys[i].segments[j].legs[n].designator.origin;
                                                    AAlegDesignatorobject.destination = JsonObjPassengers.data.journeys[i].segments[j].legs[n].designator.destination;
                                                    AAlegDesignatorobject.departure = JsonObjPassengers.data.journeys[i].segments[j].legs[n].designator.departure;
                                                    AAlegDesignatorobject.arrival = JsonObjPassengers.data.journeys[i].segments[j].legs[n].designator.arrival;
                                                    AALegobj.designator = AAlegDesignatorobject;

                                                    AALeginfo AALeginfoobject = new AALeginfo();
                                                    AALeginfoobject.arrivalTerminal = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legInfo.arrivalTerminal;
                                                    AALeginfoobject.arrivalTime = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legInfo.arrivalTime;
                                                    AALeginfoobject.departureTerminal = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legInfo.departureTerminal;
                                                    AALeginfoobject.departureTime = JsonObjPassengers.data.journeys[i].segments[j].legs[n].legInfo.departureTime;
                                                    AALegobj.legInfo = AALeginfoobject;
                                                    AALeglist.Add(AALegobj);
                                                }
                                                AASegmentobject.legs = AALeglist;
                                                AASegmentlist.Add(AASegmentobject);
                                            }
                                            AAJourneyobject.segments = AASegmentlist;
                                            AAJourneyList.Add(AAJourneyobject);
                                        }
                                        int ServiceInfttax = 0;
                                        var Passanger = JsonObjPassengers.data.passengers;
                                        passengercount = ((Newtonsoft.Json.Linq.JContainer)Passanger).Count;
                                        List<AAPassengers> passkeyList = new List<AAPassengers>();
                                        Infant infantobject = null;
                                        DomainLayer.Model.Fee feeobject = null;
                                        foreach (var items in JsonObjPassengers.data.passengers)
                                        {
                                            AAPassengers passkeytypeobject = new AAPassengers();
                                            passkeytypeobject.passengerKey = items.Value.passengerKey;
                                            passkeytypeobject.passengerTypeCode = items.Value.passengerTypeCode;
                                            passkeyList.Add(passkeytypeobject);
                                            passengerkey12 = passkeytypeobject.passengerKey;
                                            //infant
                                            if (passkeytypeobject.passengerTypeCode != "CHD")
                                            {

                                                if (JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant != null)
                                                {
                                                    int Feecount = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees.Count;
                                                    //Vinay Infant Base 
                                                    Inftcount += Feecount;
                                                    Inftbasefare = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[0].serviceCharges[0].amount;

                                                    var ServiceInft = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[0].serviceCharges;
                                                    int ServiceInftcount = ((Newtonsoft.Json.Linq.JContainer)ServiceInft).Count;

                                                    for (int inf = 1; inf < ServiceInftcount; inf++)
                                                    {
                                                        ServiceInfttax = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[0].serviceCharges[inf].amount;
                                                        ServiceInfttax += ServiceInfttax;
                                                    }
                                                    Inftbasefare = Inftbasefare - ServiceInfttax;
                                                    List<DomainLayer.Model.Fee> feeList = new List<DomainLayer.Model.Fee>();
                                                    for (int i = 0; i < Feecount; i++)
                                                    {
                                                        infantobject = new Infant();
                                                        feeobject = new DomainLayer.Model.Fee();
                                                        feeobject.isConfirmed = false;
                                                        feeobject.isConfirming = false;
                                                        feeobject.isConfirmingExternal = false;
                                                        feeobject.code = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[i].code;
                                                        feeobject._override = false;
                                                        feeobject.note = "";
                                                        feeobject.isProtected = false;
                                                        infantobject.nationality = "";
                                                        infantobject.dateOfBirth = "";
                                                        infantobject.travelDocuments = "";
                                                        infantobject.residentCountry = "";
                                                        infantobject.gender = 1;
                                                        infantobject.name = "";
                                                        infantobject.type = "";
                                                        feeList.Add(feeobject);

                                                        infantobject.fees = feeList;
                                                        passkeytypeobject.infant = infantobject;
                                                        ServicechargeInfant servicechargeInfantobj = new ServicechargeInfant();
                                                        var serviceChargesCount = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[i].serviceCharges.Count;
                                                        servicechargeInfantobj.amount = JsonObjPassengers.data.passengers[passkeytypeobject.passengerKey].infant.fees[i].serviceCharges[0].amount;
                                                        feeobject.ServicechargeInfant = servicechargeInfantobj;
                                                    }
                                                }
                                            }
                                            AirAsiaTripResponceobject.inftcount = Inftcount;
                                            AirAsiaTripResponceobject.inftbasefare = Inftbasefare;
                                            AirAsiaTripResponceobject.infttax = ServiceInfttax;
                                            AirAsiaTripResponceobject.journeys = AAJourneyList;
                                            AirAsiaTripResponceobject.passengers = passkeyList;
                                            AirAsiaTripResponceobject.passengerscount = passengercount;
                                            // HttpContext.Session.SetString("keypassengerItanary", JsonConvert.SerializeObject(AirAsiaTripResponceobject));
                                            seatMealdetail.Infant = JsonConvert.SerializeObject(AirAsiaTripResponceobject);
                                        }
                                    }
                                    else
                                    {
                                        var _responsePassengers = responsePassengers.Content.ReadAsStringAsync().Result;
                                        if (p == 0)
                                        {
                                            logs.WriteLogsR(jsonPassengers, "4-ItenaryRequest_Left", "AkasaRT");
                                            logs.WriteLogsR(_responsePassengers, "4-ItenaryResponse_Left", "AkasaRT");

                                        }
                                        else
                                        {
                                            logs.WriteLogsR(jsonPassengers, "4-ItenaryRequest_Right", "AkasaRT");
                                            logs.WriteLogsR(_responsePassengers, "4-ItenaryResponse_Right", "AkasaRT");

                                        }
                                    }
                                }
                            }
                            #endregion
                            AirAsiaTripResponceobj.inftbasefare = AirAsiaTripResponceobject.inftbasefare;
                            AirAsiaTripResponceobj.infttax = AirAsiaTripResponceobject.infttax;

                            _Passengerdata.Add("<Start>" + JsonConvert.SerializeObject(AirAsiaTripResponceobj) + "<End>");
                            // HttpContext.Session.SetString("keypassenger", JsonConvert.SerializeObject(AirAsiaTripResponceobj));

                            seatMealdetail.KPassenger = objMongoHelper.Zip(JsonConvert.SerializeObject(AirAsiaTripResponceobj));

                            //  HttpContext.Session.SetString("_keypassengerdata", JsonConvert.SerializeObject(_Passengerdata));

                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(_Passengerdata)))
                            {
                                if (_Passengerdata.Count == 2)
                                {
                                    MainPassengerdata = new List<string>();
                                }
                                MainPassengerdata.Add(JsonConvert.SerializeObject(_Passengerdata));
                            }
                        }
                        else
                        {
                            var resultsTripsell = responseTripsell.Content.ReadAsStringAsync().Result;
                            if (resultsTripsell.Contains("rawMessage") && resultsTripsell.Contains("errors"))
                            {
                                AirAsiaTripResponceobj.ErrorMsg = Regex.Match(resultsTripsell.ToString(), "rawMessage\":\"(?<msg>[\\s\\S]*?)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["msg"].Value;
                            }
                            if (p == 0)
                            {
                                logs.WriteLogsR(AkasaAirTripSellRequest, "3-SellRequest_Left", "AkasaRT");
                                logs.WriteLogsR(resultsTripsell, "3-SellResponse_Left", "AkasaRT");

                            }
                            else
                            {
                                logs.WriteLogsR(AkasaAirTripSellRequest, "3-SellRequest_Right", "AkasaRT");
                                logs.WriteLogsR(resultsTripsell, "3-SellResponse_Right", "AkasaRT");
                            }
                            _Passengerdata = new List<string>();
                            seatMealdetail.KPassenger = objMongoHelper.Zip(JsonConvert.SerializeObject(AirAsiaTripResponceobj));
                            _Passengerdata.Add("<Start>" + JsonConvert.SerializeObject(AirAsiaTripResponceobj) + "<End>");
                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(_Passengerdata)))
                            {
                                if (_Passengerdata.Count == 2)
                                {
                                    MainPassengerdata = new List<string>();
                                }
                                MainPassengerdata.Add(JsonConvert.SerializeObject(_Passengerdata));
                            }
                        }
                    }

                    #endregion


                    //Spicejet
                    string Signature = string.Empty;
                    int TotalCount = 0;
                    string str3 = string.Empty;
                    if (_JourneykeyRTData.ToLower() == "spicejet")
                    {
                        airlineId = "2";
                        #region SpiceJetSellRequest

                        seatMealdetail.Supp = "AirAsia";
                        seatMealdetail.PSupp = "SpiceJet";
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "SpiceJet").Result;
                        // string stravailibitilityrequest = HttpContext.Session.GetString("SpicejetAvailibilityRequest");

                        string stravailibitilityrequest = objMongoHelper.UnZip(tokenData.PassRequest);
                        GetAvailabilityRequest availibiltyRQ = JsonConvert.DeserializeObject<GetAvailabilityRequest>(stravailibitilityrequest);
                        Signature = string.Empty;
                        str3 = string.Empty;
                        TotalCount = 0;
                        if (_journeySide == "0j")
                        {
                            Signature = tokenData.Token;
                        }
                        else
                        {
                            Signature = tokenData.RToken;
                        }

                        if (Signature == null) { Signature = ""; }
                        int adultcount = searchLog.Adults;
                        int childcount = searchLog.Children;
                        int infantcount = searchLog.Infants;
                        TotalCount = adultcount + childcount;
                        SellResponse _getSellRS = null;
                        SellRequest _getSellRQ = null;
                        _getSellRQ = new SellRequest();
                        _getSellRQ.SellRequestData = new SellRequestData(); ;
                        _getSellRQ.Signature = Signature;
                        _getSellRQ.ContractVersion = 420;
                        _getSellRQ.SellRequestData.SellBy = SellBy.JourneyBySellKey;
                        _JourneykeyData = _Jparts[0];


                        string fareKeyRTway = fareKey[p];
                        string[] FRTparts = fareKeyRTway.Split('@');
                        _FareKeyData = FRTparts[0];
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest = new SellJourneyByKeyRequest();
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData = new SellJourneyByKeyRequestData();
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.ActionStatusCode = "NN";
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.CurrencyCode = "INR";
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.JourneySellKeys = new SellKeyList[1];
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.JourneySellKeys[0] = new SellKeyList();
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.JourneySellKeys[0].JourneySellKey = _JourneykeyData;
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.JourneySellKeys[0].FareSellKey = _FareKeyData;
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.PaxPriceType = getPaxdetails(adultcount, childcount, 0);
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.SourcePOS = GetPointOfSale();
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.PaxCountSpecified = true;
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.PaxCount = Convert.ToInt16(TotalCount);
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.LoyaltyFilter = LoyaltyFilter.MonetaryOnly;
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.IsAllotmentMarketFare = false;
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.PreventOverLap = false;
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.ReplaceAllPassengersOnUpdate = false;
                        _getSellRQ.SellRequestData.SellJourneyByKeyRequest.SellJourneyByKeyRequestData.ApplyServiceBundle = ApplyServiceBundle.No;
                        _getSellRQ.SellRequestData.SellSSR = new SellSSR();
                        _getSellRS = await objSpiceJet.GetSellAsync(_getSellRQ);

                        //string str = JsonConvert.SerializeObject(_getSellRS);
                        if (p == 0)
                        {
                            //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_getSellRQ) + "\n\n Response: " + JsonConvert.SerializeObject(_getSellRS), "SellRequest_Left", "SpiceJetRT");
                            logs.WriteLogsR(JsonConvert.SerializeObject(_getSellRQ), "3-SellRequest_Left", "SpiceJetRT");
                            logs.WriteLogsR(JsonConvert.SerializeObject(_getSellRS), "3-SellResponse_Left", "SpiceJetRT");
                        }
                        else
                        {
                            //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_getSellRQ) + "\n\n Response: " + JsonConvert.SerializeObject(_getSellRS), "SellRequest_Right", "SpiceJetRT");
                            logs.WriteLogsR(JsonConvert.SerializeObject(_getSellRQ), "3-SellRequest_Right", "SpiceJetRT");
                            logs.WriteLogsR(JsonConvert.SerializeObject(_getSellRS), "3-SellResponse_Right", "SpiceJetRT");
                        }
                        if (_getSellRS.BookingUpdateResponseData.Error != null)
                        {
                            AirAsiaTripResponceobj.ErrorMsg = _getSellRS.BookingUpdateResponseData.Error.ErrorText;
                        }
                        #endregion
                        if (string.IsNullOrEmpty(AirAsiaTripResponceobj.ErrorMsg))
                        {
                            #region GetState
                            GetBookingFromStateResponse _GetBookingFromStateRS1 = null;
                            GetBookingFromStateRequest _GetBookingFromStateRQ1 = null;
                            _GetBookingFromStateRQ1 = new GetBookingFromStateRequest();
                            _GetBookingFromStateRQ1.Signature = Signature;
                            _GetBookingFromStateRQ1.ContractVersion = 420;
                            objSpiceJet = new SpiceJetApiController();
                            _GetBookingFromStateRS1 = await objSpiceJet.GetBookingFromState(_GetBookingFromStateRQ1);
                            if (p == 0)
                            {
                                logs.WriteLogsR(JsonConvert.SerializeObject(_GetBookingFromStateRQ1), "4-GetBookingFromStateAftersellrequest_Left", "SpiceJetRT");
                                logs.WriteLogsR(JsonConvert.SerializeObject(_GetBookingFromStateRS1), "4-GetBookingFromStateAftersellresponse_Left", "SpiceJetRT");
                            }
                            else
                            {
                                logs.WriteLogsR(JsonConvert.SerializeObject(_GetBookingFromStateRQ1), "4-GetBookingFromStateAftersellrequest_Right", "SpiceJetRT");
                                logs.WriteLogsR(JsonConvert.SerializeObject(_GetBookingFromStateRS1), "4-GetBookingFromStateAftersellresponse_Right", "SpiceJetRT");
                            }
                            #endregion
                            if (_GetBookingFromStateRS1 != null)
                            {
                                AirAsiaTripResponceobj = new AirAsiaTripResponceModel();
                                var totalAmount = _GetBookingFromStateRS1.BookingData.BookingSum.TotalCost;

                                var totalTax = "";
                                #region Itenary segment and legs
                                int journeyscount = _GetBookingFromStateRS1.BookingData.Journeys.Length;
                                List<AAJourney> AAJourneyList = new List<AAJourney>();
                                for (int i = 0; i < journeyscount; i++)
                                {
                                    if (journeyscount > 1 && i == 0)
                                        continue;
                                    AAJourney AAJourneyobj = new AAJourney();
                                    AAJourneyobj.Airlinename = Airlines.Spicejet.ToString();
                                    AAJourneyobj.journeyKey = _GetBookingFromStateRS1.BookingData.Journeys[i].JourneySellKey;

                                    int segmentscount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments.Length;
                                    List<AASegment> AASegmentlist = new List<AASegment>();
                                    for (int j = 0; j < segmentscount; j++)
                                    {
                                        AADesignator AADesignatorobj = new AADesignator();
                                        AADesignatorobj.origin = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[0].DepartureStation;
                                        AADesignatorobj.destination = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[segmentscount - 1].ArrivalStation;
                                        AADesignatorobj.departure = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[0].STD;
                                        AADesignatorobj.arrival = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[segmentscount - 1].STA;
                                        AAJourneyobj.designator = AADesignatorobj;

                                        AASegment AASegmentobj = new AASegment();
                                        AADesignator AASegmentDesignatorobj = new AADesignator();

                                        AASegmentDesignatorobj.origin = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].DepartureStation;
                                        AASegmentDesignatorobj.destination = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].ArrivalStation;
                                        AASegmentDesignatorobj.departure = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].STD;
                                        AASegmentDesignatorobj.arrival = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].STA;
                                        AASegmentobj.designator = AASegmentDesignatorobj;

                                        int fareCount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares.Length;
                                        List<AAFare> AAFarelist = new List<AAFare>();
                                        for (int k = 0; k < fareCount; k++)
                                        {
                                            AAFare AAFareobj = new AAFare();
                                            AAFareobj.fareKey = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].FareSellKey;
                                            AAFareobj.productClass = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].ProductClass;

                                            var passengerFares = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares;

                                            int passengerFarescount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares.Length;
                                            List<AAPassengerfare> AAPassengerfarelist = new List<AAPassengerfare>();
                                            for (int l = 0; l < passengerFarescount; l++)
                                            {
                                                AAPassengerfare AAPassengerfareobj = new AAPassengerfare();
                                                AAPassengerfareobj.passengerType = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].PaxType;

                                                var serviceCharges1 = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges;
                                                int serviceChargescount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges.Length;
                                                List<AAServicecharge> AAServicechargelist = new List<AAServicecharge>();
                                                for (int m = 0; m < serviceChargescount; m++)
                                                {
                                                    AAServicecharge AAServicechargeobj = new AAServicecharge();
                                                    AAServicechargeobj.amount = Convert.ToInt32(_GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                    AAServicechargeobj.code = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].ChargeCode;
                                                    if (AAPassengerfareobj.passengerType.Equals("CHD") && AAServicechargeobj.code.Contains("PRCT"))
                                                    {
                                                        if (AAServicechargelist[0].amount != null && AAServicechargeobj.amount != null)
                                                        {
                                                            AAServicechargelist[0].amount = AAServicechargelist[0].amount - AAServicechargeobj.amount;
                                                        }
                                                        continue;
                                                    }

                                                    AAServicechargelist.Add(AAServicechargeobj);
                                                }

                                                AAPassengerfareobj.serviceCharges = AAServicechargelist;

                                                AAPassengerfarelist.Add(AAPassengerfareobj);
                                            }
                                            AAFareobj.passengerFares = AAPassengerfarelist;

                                            AAFarelist.Add(AAFareobj);
                                        }
                                        AASegmentobj.fares = AAFarelist;
                                        AAIdentifier AAIdentifierobj = new AAIdentifier();
                                        AAIdentifierobj.identifier = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].FlightDesignator.FlightNumber;
                                        AAIdentifierobj.carrierCode = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].FlightDesignator.CarrierCode;
                                        AASegmentobj.identifier = AAIdentifierobj;

                                        var leg = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs;
                                        int legcount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs.Length;
                                        List<AALeg> AALeglist = new List<AALeg>();
                                        for (int n = 0; n < legcount; n++)
                                        {
                                            AALeg AALeg = new AALeg();
                                            AADesignator AAlegDesignatorobj = new AADesignator();
                                            AAlegDesignatorobj.origin = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].DepartureStation;
                                            AAlegDesignatorobj.destination = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].ArrivalStation;
                                            AAlegDesignatorobj.departure = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].STD;
                                            AAlegDesignatorobj.arrival = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].STA;
                                            AALeg.designator = AAlegDesignatorobj;

                                            AALeginfo AALeginfoobj = new AALeginfo();
                                            AALeginfoobj.arrivalTerminal = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].LegInfo.ArrivalTerminal;
                                            AALeginfoobj.arrivalTime = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].LegInfo.PaxSTA;
                                            AALeginfoobj.departureTerminal = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].LegInfo.DepartureTerminal;
                                            AALeginfoobj.departureTime = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].LegInfo.PaxSTD;
                                            AALeg.legInfo = AALeginfoobj;
                                            AALeglist.Add(AALeg);
                                        }
                                        AASegmentobj.legs = AALeglist;
                                        AASegmentlist.Add(AASegmentobj);
                                    }
                                    AAJourneyobj.segments = AASegmentlist;
                                    AAJourneyList.Add(AAJourneyobj);
                                }
                                var passanger = _GetBookingFromStateRS1.BookingData.Passengers;
                                int passengercount = availibiltyRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCount;

                                List<AAPassengers> passkeylist = new List<AAPassengers>();
                                int a = 0;
                                foreach (var items in availibiltyRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxPriceTypes)
                                {
                                    for (int i = 0; i < items.PaxCount; i++)
                                    {

                                        AAPassengers passkeytypeobj = new AAPassengers();
                                        passkeytypeobj.passengerKey = a.ToString();
                                        passkeytypeobj.passengerTypeCode = items.PaxType;
                                        passkeytypeobj._Airlinename = _JourneykeyRTData;
                                        passkeylist.Add(passkeytypeobj);
                                        a++;
                                    }
                                }
                                //To do for basefare and taxes

                                int Adulttax = 0;
                                int childtax = 0;
                                if (AAJourneyList.Count > 0)
                                {
                                    for (int i = 0; i < AAJourneyList[0].segments.Count; i++)
                                    {
                                        for (int k = 0; k < AAJourneyList[0].segments[i].fares.Count; k++)
                                        {
                                            for (int l = 0; l < AAJourneyList[0].segments[i].fares[k].passengerFares.Count; l++)
                                            {
                                                if (AAJourneyList[0].segments[i].fares[k].passengerFares[l].passengerType == "ADT")
                                                {
                                                    for (int i2 = 0; i2 < AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges.Count; i2++)
                                                    {
                                                        if (i2 == 0)
                                                        {
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            Adulttax += AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges[i2].amount;
                                                        }
                                                    }
                                                }
                                                if (AAJourneyList[0].segments[i].fares[k].passengerFares[l].passengerType == "CHD")
                                                {
                                                    for (int i2 = 0; i2 < AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges.Count; i2++)
                                                    {
                                                        if (i2 == 0)
                                                        {
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            childtax += AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges[i2].amount;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                int basefaretax = 0;
                                if (Adulttax > 0)
                                {
                                    basefaretax = Adulttax * adultcount;
                                }
                                if (childtax > 0)
                                {
                                    basefaretax += childtax * childcount;
                                }
                                AirAsiaTripResponceobj.basefaretax = basefaretax;
                                AirAsiaTripResponceobj.journeys = AAJourneyList;
                                AirAsiaTripResponceobj.passengers = passkeylist;
                                AirAsiaTripResponceobj.passengerscount = passengercount;
                                #endregion

                                #region SpiceJet ItenaryRequest
                                PriceItineraryResponse _getPriceItineraryRS = null;
                                PriceItineraryRequest _getPriceItineraryRQ = null;
                                _getPriceItineraryRQ = new PriceItineraryRequest();
                                _getPriceItineraryRQ.ItineraryPriceRequest = new ItineraryPriceRequest();
                                _getPriceItineraryRQ.Signature = Signature;
                                _getPriceItineraryRQ.ContractVersion = 420;
                                _getPriceItineraryRQ.ItineraryPriceRequest.PriceItineraryBy = PriceItineraryBy.JourneyBySellKey;

                                _getPriceItineraryRQ.ItineraryPriceRequest.BookingStatus = default;
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest = new SellJourneyByKeyRequestData();
                                SellKeyList _getSellKeyList = new SellKeyList();
                                _getSellKeyList.JourneySellKey = _JourneykeyData;
                                _getSellKeyList.FareSellKey = _FareKeyData;
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.JourneySellKeys = new SellKeyList[1];
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.JourneySellKeys[0] = new SellKeyList();
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.JourneySellKeys[0].JourneySellKey = _getSellKeyList.JourneySellKey;
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.JourneySellKeys[0].FareSellKey = _getSellKeyList.FareSellKey;
                                // Changes for Adult child infant
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.PaxCount = Convert.ToInt16(TotalCount);
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.CurrencyCode = "INR";
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.PaxPriceType = getPaxdetails(adultcount, childcount, 0);
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.SourcePOS = GetPointOfSale();
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.LoyaltyFilter = LoyaltyFilter.MonetaryOnly;
                                _getPriceItineraryRQ.ItineraryPriceRequest.SellByKeyRequest.IsAllotmentMarketFare = false;
                                _getPriceItineraryRQ.ItineraryPriceRequest.SSRRequest = new SSRRequest();
                                _getPriceItineraryRS = await objSpiceJet.GetItineraryPriceAsync(_getPriceItineraryRQ);
                                if (p == 0)
                                {
                                    logs.WriteLogsR(JsonConvert.SerializeObject(_getPriceItineraryRQ), "5-PriceIteniryReq_Left", "SpiceJetRT");
                                    logs.WriteLogsR(JsonConvert.SerializeObject(_getPriceItineraryRS), "5-PriceIteniryRes_Left", "SpiceJetRT");
                                }
                                else
                                {
                                    logs.WriteLogsR(JsonConvert.SerializeObject(_getPriceItineraryRQ), "5-PriceIteniryReq_Right", "SpiceJetRT");
                                    logs.WriteLogsR(JsonConvert.SerializeObject(_getPriceItineraryRS), "5-PriceIteniryRes_Right", "SpiceJetRT");
                                }

                                #endregion

                                HttpContext.Session.SetString("journeySellKey", JsonConvert.SerializeObject(_JourneykeyData));
                                SimpleAvailabilityRequestModel _SimpleAvailabilityobj = new SimpleAvailabilityRequestModel();

                                var jsonData = objMongoHelper.UnZip(tokenData.PassRequest);

                                _SimpleAvailabilityobj = JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(jsonData.ToString());
                                SellResponse sellSsrResponse = null;
                                if (_getPriceItineraryRS != null)
                                {
                                    if (infantcount > 0)
                                    {
                                        #region SellSSrInfant
                                        SellRequest sellSsrRequest = new SellRequest();
                                        SellRequestData sellreqd = new SellRequestData();
                                        sellSsrRequest.Signature = Signature;
                                        sellSsrRequest.ContractVersion = 420;
                                        sellreqd.SellBy = SellBy.SSR;
                                        sellreqd.SellBySpecified = true;
                                        sellreqd.SellSSR = new SellSSR();
                                        sellreqd.SellSSR.SSRRequest = new SSRRequest();
                                        journeyscount = _getPriceItineraryRS.Booking.Journeys.Length;
                                        for (int i = 0; i < journeyscount; i++)
                                        {
                                            int segmentscount = _getPriceItineraryRS.Booking.Journeys[i].Segments.Length;
                                            sellreqd.SellSSR.SSRRequest.SegmentSSRRequests = new SegmentSSRRequest[segmentscount];
                                            for (int j = 0; j < segmentscount; j++)
                                            {
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j] = new SegmentSSRRequest();
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].DepartureStation = _getPriceItineraryRS.Booking.Journeys[i].Segments[j].DepartureStation;
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].ArrivalStation = _getPriceItineraryRS.Booking.Journeys[i].Segments[j].ArrivalStation;
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].STD = _getPriceItineraryRS.Booking.Journeys[i].Segments[j].STD;
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].STDSpecified = true;
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].FlightDesignator = new FlightDesignator();
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].FlightDesignator.CarrierCode = _getPriceItineraryRS.Booking.Journeys[i].Segments[j].FlightDesignator.CarrierCode; ;
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].FlightDesignator.FlightNumber = _getPriceItineraryRS.Booking.Journeys[i].Segments[j].FlightDesignator.FlightNumber;
                                                int numinfant = 0;
                                                numinfant = infantcount;
                                                //if (!string.IsNullOrEmpty(HttpContext.Session.GetString("infantCount")))
                                                //{
                                                //numinfant = Convert.ToInt32(HttpContext.Session.GetString("infantCount"));
                                                //}

                                                bool infant = false;
                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs = new PaxSSR[numinfant];

                                                for (int j1 = 0; j1 < numinfant; j1++)
                                                {

                                                    if (j1 < numinfant)
                                                    {
                                                        for (int i1 = 0; i1 < numinfant; i1++)//Paxnum 1 adult,1 child,1 infant 2 meal
                                                        {
                                                            infantcount = numinfant;
                                                            if (infantcount > 0 && i1 + 1 <= infantcount)
                                                            {
                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i1] = new PaxSSR();
                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i1].ActionStatusCode = "NN";
                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i1].SSRCode = "INFT";
                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i1].PassengerNumberSpecified = true;
                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i1].PassengerNumber = Convert.ToInt16(i1);
                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i1].SSRNumber = Convert.ToInt16(0);
                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i1].DepartureStation = _getPriceItineraryRS.Booking.Journeys[i].Segments[j].DepartureStation;
                                                                sellreqd.SellSSR.SSRRequest.SegmentSSRRequests[j].PaxSSRs[i1].ArrivalStation = _getPriceItineraryRS.Booking.Journeys[i].Segments[j].ArrivalStation;
                                                                j1 = numinfant - 1;
                                                            }
                                                        }
                                                    }
                                                }


                                            }
                                        }
                                        sellSsrRequest.SellRequestData = sellreqd;
                                        objSpiceJet = new SpiceJetApiController();
                                        sellSsrResponse = await objSpiceJet.sellssR(sellSsrRequest);

                                        if (p == 0)
                                        {
                                            logs.WriteLogsR(JsonConvert.SerializeObject(sellSsrRequest), "6-SellSSRInfantReq_Left", "SpicejetRT");
                                            logs.WriteLogsR(JsonConvert.SerializeObject(sellSsrResponse), "6-SellSSRInfantRes_Left", "SpicejetRT");
                                        }
                                        else
                                        {
                                            logs.WriteLogsR(JsonConvert.SerializeObject(sellSsrRequest), "6-SellSSRInfantReq_Right", "SpicejetRT");
                                            logs.WriteLogsR(JsonConvert.SerializeObject(sellSsrResponse), "6-SellSSRInfantRes_Right", "SpicejetRT");
                                        }

                                    }
                                    #endregion
                                }
                                if (JsonConvert.SerializeObject(sellSsrResponse).ToLower().Contains("ssr inft is not available"))
                                {
                                    AirAsiaTripResponceobj.ErrorMsg = Regex.Match(JsonConvert.SerializeObject(sellSsrResponse), "\"Text\":\"(?<msg>[\\s\\S]*?)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["msg"].Value.Trim();
                                }
                                else
                                {

                                    #region GetState
                                    GetBookingFromStateResponse _GetBookingFromStateRS = null;
                                    GetBookingFromStateRequest _GetBookingFromStateRQ = null;
                                    _GetBookingFromStateRQ = new GetBookingFromStateRequest();
                                    _GetBookingFromStateRQ.Signature = Signature;
                                    _GetBookingFromStateRQ.ContractVersion = 420;


                                    objSpiceJet = new SpiceJetApiController();
                                    _GetBookingFromStateRS = await objSpiceJet.GetBookingFromState(_GetBookingFromStateRQ);
                                    if (p == 0)
                                    {
                                        logs.WriteLogsR(JsonConvert.SerializeObject(_GetBookingFromStateRQ), "7-GetBookingFromStateafterSellInfantReq_Left", "SpicejetRT");
                                        logs.WriteLogsR(JsonConvert.SerializeObject(_GetBookingFromStateRS), "7-GetBookingFromStateafterSellInfantRes_Left", "SpicejetRT");
                                    }
                                    else
                                    {
                                        logs.WriteLogsR(JsonConvert.SerializeObject(_GetBookingFromStateRQ), "7-GetBookingFromStateafterSellInfantReq_Right", "SpicejetRT");
                                        logs.WriteLogsR(JsonConvert.SerializeObject(_GetBookingFromStateRS), "7-GetBookingFromStateafterSellInfantRes_Right", "SpicejetRT");
                                    }


                                    if (_GetBookingFromStateRS != null)
                                    {
                                        var JsonSellSSrInfant = _GetBookingFromStateRS;
                                        int Inftbasefare = 0;
                                        int Inftcount = 0;
                                        int infttax = 0;
                                        if (_GetBookingFromStateRS.BookingData.Passengers.Length > 0 && _GetBookingFromStateRS.BookingData.Passengers[0].PassengerFees.Length > 0)
                                        {
                                            for (int i = 0; i < _GetBookingFromStateRS.BookingData.Passengers[0].PassengerFees[0].ServiceCharges.Length; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    Inftbasefare = Convert.ToInt32(_GetBookingFromStateRS.BookingData.Passengers[0].PassengerFees[0].ServiceCharges[0].Amount);
                                                    Inftcount += Convert.ToInt32(_GetBookingFromStateRS.BookingData.Passengers.Length);
                                                    AirAsiaTripResponceobj.inftcount = Inftcount;
                                                    AirAsiaTripResponceobj.inftbasefare = Inftbasefare;
                                                }
                                                else
                                                {
                                                    infttax += Convert.ToInt32(_GetBookingFromStateRS.BookingData.Passengers[0].PassengerFees[0].ServiceCharges[i].Amount);
                                                }

                                            }
                                            AirAsiaTripResponceobj.infttax = infttax * infantcount;
                                        }
                                    }
                                    AirAsiaTripResponceobj.basefaretax += AirAsiaTripResponceobj.infttax;
                                    #endregion
                                }

                            }
                        }
                        Passengerdata = new List<string>();
                        Passengerdata.Add("<Start>" + JsonConvert.SerializeObject(AirAsiaTripResponceobj) + "<End>");
                        //HttpContext.Session.SetString("SGkeypassengerRT", JsonConvert.SerializeObject(AirAsiaTripResponceobj));
                        // HttpContext.Session.SetString("keypassengerdata", JsonConvert.SerializeObject(Passengerdata));
                        //checking 
                        seatMealdetail.KPassenger = JsonConvert.SerializeObject(AirAsiaTripResponceobj);

                        if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Passengerdata)))
                        {
                            if (Passengerdata.Count == 2)
                            {
                                MainPassengerdata = new List<string>();
                            }
                            MainPassengerdata.Add(JsonConvert.SerializeObject(Passengerdata));
                        }
                        //  HttpContext.Session.SetString("keypassengerItanary", JsonConvert.SerializeObject(AirAsiaTripResponceobj));

                        seatMealdetail.Infant = JsonConvert.SerializeObject(AirAsiaTripResponceobj);

                    }

                    //Indigo airline
                    if (_JourneykeyRTData.ToLower() == "indigo")
                    {
                        seatMealdetail.Supp = "AirAsia";
                        seatMealdetail.PSupp = "Indigo";
                        airlineId = "3";
                        //string stravailibitilityrequest = HttpContext.Session.GetString("IndigoAvailibilityRequest");

                        Signature = string.Empty;
                        str3 = string.Empty;
                        TotalCount = 0;
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Indigo").Result;

                        string stravailibitilityrequest = objMongoHelper.UnZip(tokenData.PassRequest);
                        GetAvailabilityRequest availibiltyRQ = JsonConvert.DeserializeObject<GetAvailabilityRequest>(stravailibitilityrequest);


                        if (_journeySide == "0j")
                        {
                            Signature = tokenData.Token;
                        }
                        else
                        {
                            Signature = tokenData.RToken;
                        }

                        int adultcount = searchLog.Adults;
                        int childcount = searchLog.Children;
                        int infantcount = searchLog.Infants;
                        TotalCount = adultcount + childcount;
                        _JourneykeyData = _Jparts[0];
                        string fareKeyRTway = fareKey[p];
                        string[] FRTparts = fareKeyRTway.Split('@');
                        _FareKeyData = FRTparts[0];
                        #region IndigoSellRequest
                        _sell objsell = new _sell();
                        IndigoBookingManager_.SellResponse _getSellRS = await objsell.Sell(Signature, _JourneykeyData, _FareKeyData, _Jparts[0], fareKey[p], TotalCount, adultcount, childcount, infantcount, p);
                        #endregion
                        #region GetState

                        IndigoBookingManager_.GetBookingFromStateResponse _GetBookingFromStateRS1 = await objsell.GetBookingFromState(Signature, p, "");
                        if (_GetBookingFromStateRS1 != null)
                        {
                            AirAsiaTripResponceobj = new AirAsiaTripResponceModel();

                            if (_getSellRS.BookingUpdateResponseData.Error != null)
                            {
                                AirAsiaTripResponceobj.ErrorMsg = _getSellRS.BookingUpdateResponseData.Error.ErrorText;
                            }
                            var totalAmount = _GetBookingFromStateRS1.BookingData.BookingSum.TotalCost;
                            var totalTax = "";
                            #region Itenary segment and legs
                            int journeyscount = _GetBookingFromStateRS1.BookingData.Journeys.Length;
                            List<AAJourney> AAJourneyList = new List<AAJourney>();
                            for (int i = 0; i < journeyscount; i++)
                            {
                                if (journeyscount > 1 && i == 0)
                                    continue;
                                AAJourney AAJourneyobj = new AAJourney();
                                AAJourneyobj.Airlinename = Airlines.Indigo.ToString();
                                AAJourneyobj.journeyKey = _GetBookingFromStateRS1.BookingData.Journeys[i].JourneySellKey;
                                int segmentscount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments.Length;
                                List<AASegment> AASegmentlist = new List<AASegment>();
                                for (int j = 0; j < segmentscount; j++)
                                {
                                    AADesignator AADesignatorobj = new AADesignator();
                                    AADesignatorobj.origin = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[0].DepartureStation;
                                    AADesignatorobj.destination = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[segmentscount - 1].ArrivalStation;
                                    AADesignatorobj.departure = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[0].STD;
                                    AADesignatorobj.arrival = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[segmentscount - 1].STA;
                                    AAJourneyobj.designator = AADesignatorobj;
                                    AASegment AASegmentobj = new AASegment();
                                    AADesignator AASegmentDesignatorobj = new AADesignator();
                                    AASegmentDesignatorobj.origin = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].DepartureStation;
                                    AASegmentDesignatorobj.destination = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].ArrivalStation;
                                    AASegmentDesignatorobj.departure = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].STD;
                                    AASegmentDesignatorobj.arrival = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].STA;
                                    AASegmentobj.designator = AASegmentDesignatorobj;
                                    int fareCount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares.Length;
                                    List<AAFare> AAFarelist = new List<AAFare>();
                                    for (int k = 0; k < fareCount; k++)
                                    {
                                        AAFare AAFareobj = new AAFare();
                                        AAFareobj.fareKey = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].FareSellKey;
                                        AAFareobj.productClass = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].ProductClass;
                                        var passengerFares = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares;
                                        int passengerFarescount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares.Length;
                                        List<AAPassengerfare> AAPassengerfarelist = new List<AAPassengerfare>();
                                        for (int l = 0; l < passengerFarescount; l++)
                                        {
                                            AAPassengerfare AAPassengerfareobj = new AAPassengerfare();
                                            AAPassengerfareobj.passengerType = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].PaxType;
                                            var serviceCharges1 = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges;
                                            int serviceChargescount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges.Length;
                                            List<AAServicecharge> AAServicechargelist = new List<AAServicecharge>();
                                            for (int m = 0; m < serviceChargescount; m++)
                                            {
                                                AAServicecharge AAServicechargeobj = new AAServicecharge();
                                                AAServicechargeobj.amount = Convert.ToInt32(_GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges[m].Amount);
                                                AAServicechargelist.Add(AAServicechargeobj);
                                            }
                                            AAPassengerfareobj.serviceCharges = AAServicechargelist;
                                            AAPassengerfarelist.Add(AAPassengerfareobj);
                                        }
                                        AAFareobj.passengerFares = AAPassengerfarelist;
                                        AAFarelist.Add(AAFareobj);
                                    }
                                    AASegmentobj.fares = AAFarelist;
                                    AAIdentifier AAIdentifierobj = new AAIdentifier();
                                    AAIdentifierobj.identifier = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].FlightDesignator.FlightNumber;
                                    AAIdentifierobj.carrierCode = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].FlightDesignator.CarrierCode;
                                    AASegmentobj.identifier = AAIdentifierobj;
                                    var leg = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs;
                                    int legcount = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs.Length;
                                    List<AALeg> AALeglist = new List<AALeg>();
                                    for (int n = 0; n < legcount; n++)
                                    {
                                        AALeg AALeg = new AALeg();
                                        AADesignator AAlegDesignatorobj = new AADesignator();
                                        AAlegDesignatorobj.origin = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].DepartureStation;
                                        AAlegDesignatorobj.destination = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].ArrivalStation;
                                        AAlegDesignatorobj.departure = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].STD;
                                        AAlegDesignatorobj.arrival = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].STA;
                                        AALeg.designator = AAlegDesignatorobj;
                                        AALeginfo AALeginfoobj = new AALeginfo();
                                        AALeginfoobj.arrivalTerminal = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].LegInfo.ArrivalTerminal;
                                        AALeginfoobj.arrivalTime = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].LegInfo.PaxSTA;
                                        AALeginfoobj.departureTerminal = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].LegInfo.DepartureTerminal;
                                        AALeginfoobj.departureTime = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Legs[n].LegInfo.PaxSTD;
                                        AALeg.legInfo = AALeginfoobj;
                                        AALeglist.Add(AALeg);
                                    }
                                    AASegmentobj.legs = AALeglist;
                                    AASegmentlist.Add(AASegmentobj);
                                }
                                AAJourneyobj.segments = AASegmentlist;
                                AAJourneyList.Add(AAJourneyobj);
                            }
                            #endregion
                            var passanger = _GetBookingFromStateRS1.BookingData.Passengers;
                            int passengercount = availibiltyRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCount;
                            List<AAPassengers> passkeylist = new List<AAPassengers>();
                            int a = 0;
                            foreach (var items in availibiltyRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxPriceTypes)
                            {
                                for (int i = 0; i < items.PaxCount; i++)
                                {
                                    AAPassengers passkeytypeobj = new AAPassengers();
                                    passkeytypeobj.passengerKey = a.ToString();
                                    passkeytypeobj.passengerTypeCode = items.PaxType;
                                    passkeytypeobj._Airlinename = _JourneykeyRTData;
                                    passkeylist.Add(passkeytypeobj);
                                    a++;
                                }
                            }
                            //To do for basefare and taxes
                            int Adulttax = 0;
                            int childtax = 0;
                            if (AAJourneyList.Count > 0)
                            {
                                for (int i = 0; i < AAJourneyList[0].segments.Count; i++)
                                {
                                    for (int k = 0; k < AAJourneyList[0].segments[i].fares.Count; k++)
                                    {
                                        for (int l = 0; l < AAJourneyList[0].segments[i].fares[k].passengerFares.Count; l++)
                                        {
                                            if (AAJourneyList[0].segments[i].fares[k].passengerFares[l].passengerType == "ADT")
                                            {
                                                for (int i2 = 0; i2 < AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges.Count; i2++)
                                                {
                                                    if (i2 == 0)
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        Adulttax += AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges[i2].amount;
                                                    }
                                                }
                                            }
                                            if (AAJourneyList[0].segments[i].fares[k].passengerFares[l].passengerType == "CHD")
                                            {
                                                for (int i2 = 0; i2 < AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges.Count; i2++)
                                                {
                                                    if (i2 == 0)
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        childtax += AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges[i2].amount;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            int basefaretax = 0;
                            if (Adulttax > 0)
                            {
                                basefaretax = Adulttax * adultcount;
                            }
                            if (childtax > 0)
                            {
                                basefaretax += childtax * childcount;
                            }
                            AirAsiaTripResponceobj.basefaretax = basefaretax;
                            AirAsiaTripResponceobj.journeys = AAJourneyList;
                            AirAsiaTripResponceobj.passengers = passkeylist;
                            AirAsiaTripResponceobj.passengerscount = passengercount;
                            #region Indigo ItenaryRequest
                            IndigoBookingManager_.PriceItineraryResponse _getPriceItineraryRS = await objsell.GetItineraryPrice(Signature, _JourneykeyData, _FareKeyData, _Jparts[0], fareKey[p], TotalCount, adultcount, childcount, infantcount, p);
                            #endregion
                            HttpContext.Session.SetString("journeySellKey", JsonConvert.SerializeObject(_JourneykeyData));
                            SimpleAvailabilityRequestModel _SimpleAvailabilityobj = new SimpleAvailabilityRequestModel();
                            var jsonData = objMongoHelper.UnZip(tokenData.PassRequest);
                            _SimpleAvailabilityobj = JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(jsonData.ToString());
                            IndigoBookingManager_.SellResponse sellSsrResponse = null;
                            if (_getPriceItineraryRS != null)
                            {
                                #region SellSSrInfant
                                if (infantcount > 0)
                                {
                                    sellSsrResponse = await objsell.sellssrInft(Signature, _getPriceItineraryRS, infantcount, 0, p, "");
                                    str3 = JsonConvert.SerializeObject(sellSsrResponse);
                                    if (sellSsrResponse != null)
                                    {
                                        var JsonsellSsrResponse = sellSsrResponse;
                                    }
                                }
                                #endregion
                            }
                            if (JsonConvert.SerializeObject(sellSsrResponse).ToLower().Contains("ssr inft is not available"))
                            {
                                AirAsiaTripResponceobj.ErrorMsg = Regex.Match(JsonConvert.SerializeObject(sellSsrResponse), "\"Text\":\"(?<msg>[\\s\\S]*?)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["msg"].Value.Trim();
                            }
                            else
                            {
                                #region GetBookingFromState
                                IndigoBookingManager_.GetBookingFromStateResponse _GetBookingFromStateRS = await objsell.GetBookingFromState(Signature, p, "");
                                if (_GetBookingFromStateRS != null)
                                {
                                    var JsonSellSSrInfant = _GetBookingFromStateRS;
                                    int Inftbasefare = 0;
                                    int Inftcount = 0;
                                    int infttax = 0;
                                    if (_GetBookingFromStateRS.BookingData.Passengers.Length > 0 && _GetBookingFromStateRS.BookingData.Passengers[0].PassengerFees.Length > 0)
                                    {
                                        for (int i = 0; i < _GetBookingFromStateRS.BookingData.Passengers[0].PassengerFees[0].ServiceCharges.Length; i++)
                                        {
                                            if (i == 0)
                                            {
                                                Inftbasefare = Convert.ToInt32(_GetBookingFromStateRS.BookingData.Passengers[0].PassengerFees[0].ServiceCharges[0].Amount);
                                                Inftcount += Convert.ToInt32(_GetBookingFromStateRS.BookingData.Passengers.Length);
                                                AirAsiaTripResponceobj.inftcount = Inftcount;
                                                AirAsiaTripResponceobj.inftbasefare = Inftbasefare;
                                            }
                                            else
                                            {
                                                infttax += Convert.ToInt32(_GetBookingFromStateRS.BookingData.Passengers[0].PassengerFees[0].ServiceCharges[i].Amount);
                                            }

                                        }
                                        AirAsiaTripResponceobj.inftbasefare = AirAsiaTripResponceobj.inftbasefare - infttax;
                                        AirAsiaTripResponceobj.infttax = infttax * infantcount;
                                    }
                                }
                                #endregion
                                AirAsiaTripResponceobj.basefaretax += AirAsiaTripResponceobj.infttax;
                            }

                            Passengerdata = new List<string>();
                            Passengerdata.Add("<Start>" + JsonConvert.SerializeObject(AirAsiaTripResponceobj) + "<End>");
                            // HttpContext.Session.SetString("SGkeypassengerRT", JsonConvert.SerializeObject(AirAsiaTripResponceobj));
                            //  HttpContext.Session.SetString("keypassengerdata", JsonConvert.SerializeObject(Passengerdata));

                            seatMealdetail.KPassenger = JsonConvert.SerializeObject(AirAsiaTripResponceobj);

                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Passengerdata)))
                            {
                                if (Passengerdata.Count == 2)
                                {
                                    MainPassengerdata = new List<string>();
                                }
                                MainPassengerdata.Add(JsonConvert.SerializeObject(Passengerdata));
                            }
                            #endregion
                            // HttpContext.Session.SetString("keypassengerItanary", JsonConvert.SerializeObject(AirAsiaTripResponceobj));

                            seatMealdetail.Infant = JsonConvert.SerializeObject(AirAsiaTripResponceobj);
                            //  seatMealdetail.KPassenger = JsonConvert.SerializeObject(AirAsiaTripResponceobj);

                        }
                    }

                    //GDS airline
                    //GDSTraceid
                    if (_JourneykeyRTData.ToLower() == "vistara" || _JourneykeyRTData.ToLower() == "airindia" || _JourneykeyRTData.ToLower() == "hehnair")
                    {
                        airlineId = "6";
                        Supp = "GDS";
                        seatMealdetail.Supp = "AirAsia";
                        seatMealdetail.PSupp = "GDS";

                        #region GDS
                        AAIdentifier AAIdentifierobj = null;
                        string stravailibitilityrequest = string.Empty;
                        TempData["farekey"] = fareKey;
                        TempData["journeyKey"] = journeyKey;

                        Signature = string.Empty;
                        str3 = string.Empty;
                        TotalCount = 0;
                        farebasisdata = string.Empty;
                        string _farebasis = string.Empty;
                        string[] _dataNew = null;

                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "GDS").Result;

                        if (_journeySide == "0j")
                        {
                            Signature = tokenData.Token; // HttpContext.Session.GetString("GDSTraceid");
                            if (Signature == null) { Signature = ""; }
                            string[] _data = fareKey[0].ToString().Split("@0f");
                            if (!string.IsNullOrEmpty(_data[0]))
                            {
                                Airfaredata = JsonConvert.DeserializeObject<SimpleAvailibilityaAddResponce>(_data[0]);
                            }
                            _dataNew = journeyKey[0].ToString().Split("@0");
                            _farebasis = _dataNew[0].ToString().Split("_")[1];
                            farebasisdata = _farebasis;
                        }
                        else
                        {
                            Signature = tokenData.RToken; // HttpContext.Session.GetString("GDSTraceidR");
                            string[] _data = fareKey[1].Split("@1f");
                            if (!string.IsNullOrEmpty(_data[0]))
                            {
                                Airfaredata = JsonConvert.DeserializeObject<SimpleAvailibilityaAddResponce>(_data[0]);
                            }
                            _dataNew = journeyKey[1].ToString().Split("@1");
                            _farebasis = _dataNew[0].ToString().Split("_")[1];
                            farebasisdata = _farebasis;
                        }
                        newGuid = Signature.Replace(@"""", string.Empty);
                        int adultcount = searchLog.Adults;
                        int childcount = searchLog.Children;
                        int infantcount = searchLog.Infants;
                        TotalCount = adultcount + childcount;
                        if (newGuid == "" || newGuid == null)
                        {
                            return RedirectToAction("Index");
                        }
                        //stravailibitilityrequest = HttpContext.Session.GetString("GDSAvailibilityRequest");
                        //availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(stravailibitilityrequest);

                        var jsonData = objMongoHelper.UnZip(tokenData.PassRequest);
                        availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(jsonData);



                        #region GDSAirPricelRequest
                        httpContextAccessorInstance = new HttpContextAccessor();
                        _testURL = AppUrlConstant.GDSURL;
                        _targetBranch = string.Empty;
                        _userName = string.Empty;
                        _password = string.Empty;
                        _objAvail = new TravelPort(httpContextAccessorInstance);
                        _targetBranch = "P7027135";
                        _userName = "Universal API/uAPI5098257106-beb65aec";
                        _password = "Q!f5-d7A3D";
                        fareRepriceReq = new StringBuilder();
                        string res = _objAvail.AirPriceGetRoundTripCorporate(_testURL, fareRepriceReq, availibiltyRQGDS, newGuid.ToString(), _targetBranch, _userName, _password, Airfaredata, farebasisdata, p, "");
                        HostTokenKey = Regex.Match(res, @"HostToken\s*Key=""(?<HostTokenKey>[\s\S]*?)"">(?<Value>[\s\S]*?)</").Groups["HostTokenKey"].Value.Trim();
                        HostTokenValue = Regex.Match(res, @"HostToken\s*Key=""(?<HostTokenKey>[\s\S]*?)"">(?<Value>[\s\S]*?)</").Groups["Value"].Value.Trim();
                        TravelPortParsing _objP = new TravelPortParsing();
                        List<GDSResModel.Segment> getAirPriceRes = new List<GDSResModel.Segment>();
                        if (res != null && !res.Contains("Bad Request") && !res.Contains("Internal Server Error"))
                        {
                            getAirPriceRes = _objP.ParseAirFareRsp(res, "OneWay", availibiltyRQGDS);
                        }
                        string str = JsonConvert.SerializeObject(getAirPriceRes);
                        #endregion

                        #region GetState
                        //IndigoBookingManager_.GetBookingFromStateResponse _GetBookingFromStateRS1 = null;// await objsell.GetBookingFromState(Signature, "OneWay");
                        //str3 = JsonConvert.SerializeObject(_GetBookingFromStateRS1);
                        if (getAirPriceRes != null && getAirPriceRes.Count > 0)
                        {
                            HttpContext.Session.SetString("Total", getAirPriceRes[0].Fare.TotalFareWithOutMarkUp.ToString());
                            AirAsiaTripResponceobj = new AirAsiaTripResponceModel();
                            #region Itenary segment and legs
                            int journeyscount = getAirPriceRes.Count;// _GetBookingFromStateRS1.BookingData.Journeys.Length;
                            List<AAJourney> AAJourneyList = new List<AAJourney>();
                            AASegment AASegmentobj = null;
                            string paxType = String.Empty;
                            List<AAPassengerfare> AAPassengerfarelist = new List<AAPassengerfare>();
                            for (int i = 0; i < journeyscount; i++)
                            {
                                AAJourney AAJourneyobj = new AAJourney();
                                AAJourneyobj.journeyKey = ""; // _GetBookingFromStateRS1.BookingData.Journeys[i].JourneySellKey;
                                int segmentscount = getAirPriceRes[0].Bonds[0].Legs.Count;//.BookingData.Journeys[i].Segments.Length;
                                List<AASegment> AASegmentlist = new List<AASegment>();
                                for (int j = 0; j < segmentscount; j++)
                                {
                                    AAPassengerfarelist = new List<AAPassengerfare>();
                                    AADesignator AADesignatorobj = new AADesignator();
                                    AADesignatorobj.origin = getAirPriceRes[0].Bonds[0].Legs[0].Origin;
                                    AADesignatorobj.destination = getAirPriceRes[0].Bonds[0].Legs[j].Destination;
                                    AADesignatorobj.departure = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[0].DepartureTime);
                                    if (segmentscount > 1)
                                    {
                                        AADesignatorobj.arrival = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[segmentscount - 1].ArrivalTime);
                                    }
                                    else
                                    {
                                        AADesignatorobj.arrival = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[j].ArrivalTime);
                                    }
                                    if (getAirPriceRes[0].Bonds[0].Legs[0].AirlineName == "UK")
                                        getAirPriceRes[0].Bonds[0].Legs[0].AirlineName = "Vistara";
                                    else if (getAirPriceRes[0].Bonds[0].Legs[0].AirlineName == "AI")
                                        getAirPriceRes[0].Bonds[0].Legs[0].AirlineName = "AirIndia";
                                    else if (getAirPriceRes[0].Bonds[0].Legs[0].AirlineName == "H1")
                                        getAirPriceRes[0].Bonds[0].Legs[0].AirlineName = "hahnair";

                                    AAJourneyobj.Airlinename = getAirPriceRes[0].Bonds[0].Legs[0].AirlineName;
                                    AAJourneyobj.designator = AADesignatorobj;
                                    AASegmentobj = new AASegment();
                                    AADesignator AASegmentDesignatorobj = new AADesignator();
                                    AASegmentDesignatorobj.origin = getAirPriceRes[0].Bonds[0].Legs[j].Origin;
                                    AASegmentDesignatorobj.destination = getAirPriceRes[0].Bonds[0].Legs[j].Destination;
                                    AASegmentDesignatorobj.departure = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[j].DepartureTime);
                                    AASegmentDesignatorobj.arrival = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[j].ArrivalTime);
                                    AASegmentobj.designator = AASegmentDesignatorobj;

                                    //int fareCount = getAirPriceRes[0].Fare.cou
                                    List<AAFare> AAFarelist = new List<AAFare>();
                                    //for (int k = 0; k < fareCount; k++)
                                    //{
                                    AAFare AAFareobj = new AAFare();
                                    AAFareobj.fareKey = ""; //_GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].FareSellKey;
                                    AAFareobj.productClass = "Corporate " +journeyKey[p].Split('@')[0];// _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].ProductClass;
                                                                                         //var passengerFares = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares;
                                    int passengerFarescount = getAirPriceRes[0].Fare.PaxFares.Count;// _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares.Length;
                                    if (j == 0)                                                           //List<AAPassengerfare> AAPassengerfarelist = new List<AAPassengerfare>();
                                    {

                                        for (int l = 0; l < passengerFarescount; l++)
                                        {
                                            if (getAirPriceRes[0].Fare.PaxFares[l].PaxType == PAXTYPE.ADT)
                                            {
                                                paxType = "ADT";
                                            }
                                            else if (getAirPriceRes[0].Fare.PaxFares[l].PaxType == PAXTYPE.CHD)
                                            {
                                                paxType = "CHD";
                                            }
                                            else if (getAirPriceRes[0].Fare.PaxFares[l].PaxType == PAXTYPE.INF)
                                            {
                                                paxType = "INF";
                                            }
                                            AAPassengerfare AAPassengerfareobj = new AAPassengerfare();
                                            AAPassengerfareobj.passengerType = paxType;
                                            //var serviceCharges1 = _GetBookingFromStateRS1.BookingData.Journeys[i].Segments[j].Fares[k].PaxFares[l].ServiceCharges;
                                            int serviceChargescount = getAirPriceRes[0].Fare.PaxFares[l].Fare.Count;
                                            List<AAServicecharge> AAServicechargelist = new List<AAServicecharge>();
                                            for (int m = 0; m < serviceChargescount; m++)
                                            {
                                                AAServicecharge AAServicechargeobj = new AAServicecharge();
                                                AAServicechargeobj.amount = Convert.ToInt32(getAirPriceRes[0].Fare.PaxFares[l].Fare[m].Amount);
                                                AAServicechargeobj.code = getAirPriceRes[0].Fare.PaxFares[l].Fare[m].ChargeCode;
                                                AAServicechargelist.Add(AAServicechargeobj);
                                            }
                                            AAPassengerfareobj.basicAmount = Convert.ToInt32(getAirPriceRes[0].Fare.PaxFares[l].BasicFare);
                                            AAPassengerfareobj.serviceCharges = AAServicechargelist;
                                            AAPassengerfarelist.Add(AAPassengerfareobj);
                                        }
                                    }
                                    AAFareobj.passengerFares = AAPassengerfarelist;
                                    AAFarelist.Add(AAFareobj);
                                    //}
                                    AASegmentobj.fares = AAFarelist;
                                    AAIdentifierobj = new AAIdentifier();

                                    AAIdentifierobj.identifier = getAirPriceRes[0].Bonds[0].Legs[j].FlightNumber;
                                    AAIdentifierobj.carrierCode = getAirPriceRes[0].Bonds[0].Legs[j].CarrierCode;

                                    AASegmentobj.identifier = AAIdentifierobj;

                                    var leg = getAirPriceRes[0].Bonds[0].Legs;
                                    int legcount = getAirPriceRes[0].Bonds[0].Legs.Count;
                                    List<AALeg> AALeglist = new List<AALeg>();
                                    //for (int n = 0; n < legcount; n++)
                                    //{
                                    AALeg AALeg = new AALeg();
                                    AADesignator AAlegDesignatorobj = new AADesignator();
                                    AAlegDesignatorobj.origin = getAirPriceRes[0].Bonds[0].Legs[j].Origin;
                                    AAlegDesignatorobj.destination = getAirPriceRes[0].Bonds[0].Legs[j].Destination;
                                    AAlegDesignatorobj.departure = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[j].DepartureTime);
                                    AAlegDesignatorobj.arrival = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[j].ArrivalTime);
                                    AALeg.designator = AAlegDesignatorobj;

                                    AALeginfo AALeginfoobj = new AALeginfo();
                                    AALeginfoobj.arrivalTerminal = getAirPriceRes[0].Bonds[0].Legs[j].ArrivalTerminal;
                                    AALeginfoobj.arrivalTime = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[j].ArrivalTime);
                                    AALeginfoobj.departureTerminal = getAirPriceRes[0].Bonds[0].Legs[j].DepartureTerminal;
                                    AALeginfoobj.departureTime = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[j].DepartureTime);
                                    AALeginfoobj.equipmentType = getAirPriceRes[0].Bonds[0].Legs[j]._Equipment;
                                    AALeg.legInfo = AALeginfoobj;
                                    AALeglist.Add(AALeg);

                                    //}
                                    AASegmentobj.legs = AALeglist;
                                    AASegmentlist.Add(AASegmentobj);
                                }

                                AAJourneyobj.segments = AASegmentlist;
                                AAJourneyList.Add(AAJourneyobj);

                            }

                            #endregion
                            int passengercount = 0;
                            if (availibiltyRQGDS.passengercount != null)
                            {
                                passengercount = availibiltyRQGDS.passengercount.adultcount + availibiltyRQGDS.passengercount.childcount + availibiltyRQGDS.passengercount.infantcount;
                            }
                            else
                            {
                                passengercount = availibiltyRQGDS.adultcount + availibiltyRQGDS.childcount + availibiltyRQGDS.infantcount;
                            }
                            List<AAPassengers> passkeylist = new List<AAPassengers>();
                            int a = 0;
                            //foreach (var items in availibiltyRQGDS.passengers)
                            //{
                            int paxcount = 0;

                            for (int i = 0; i < getAirPriceRes[0].Fare.PaxFares.Count; i++)
                            {
                                if (getAirPriceRes[0].Fare.PaxFares[i].PaxType == PAXTYPE.ADT)
                                {
                                    //a = Convert.ToInt32(getAirPriceRes[0].Fare.PaxFares[i].PaxType);
                                    paxType = "ADT";
                                    if (availibiltyRQGDS.passengercount != null)
                                    {
                                        paxcount = availibiltyRQGDS.passengercount.adultcount;
                                    }
                                    else
                                    {
                                        paxcount = availibiltyRQGDS.adultcount;
                                    }

                                }
                                else if (getAirPriceRes[0].Fare.PaxFares[i].PaxType == PAXTYPE.CHD)
                                {
                                    //a = Convert.ToInt32(getAirPriceRes[0].Fare.PaxFares[i].PaxType);
                                    paxType = "CHD";
                                    if (availibiltyRQGDS.passengercount != null)
                                    {
                                        paxcount = availibiltyRQGDS.passengercount.childcount;
                                    }
                                    else
                                    {
                                        paxcount = availibiltyRQGDS.childcount;
                                    }

                                }
                                else if (getAirPriceRes[0].Fare.PaxFares[i].PaxType == PAXTYPE.INF)
                                {
                                    //a = Convert.ToInt32(getAirPriceRes[0].Fare.PaxFares[i].PaxType);
                                    paxType = "INF";
                                    if (availibiltyRQGDS.passengercount != null)
                                    {
                                        paxcount = availibiltyRQGDS.passengercount.infantcount;
                                    }
                                    else
                                    {
                                        paxcount = availibiltyRQGDS.infantcount;
                                    }
                                }
                                for (int k = 0; k < paxcount; k++)
                                {

                                    AAPassengers passkeytypeobj = new AAPassengers();
                                    passkeytypeobj.passengerKey = a.ToString();
                                    passkeytypeobj.passengerTypeCode = paxType;
                                    passkeytypeobj._Airlinename = _JourneykeyRTData;
                                    passkeylist.Add(passkeytypeobj);
                                    a++;
                                }

                            }
                            var sortedList = passkeylist.OrderBy(p => p.passengerTypeCode == "INF" ? 1 : 0).ToList();
                            //}

                            //}
                            //To do for basefare and taxes

                            int Adulttax = 0;
                            int childtax = 0;
                            int Inftbasefare = 0;
                            int Inftcount = 0;
                            int infttax = 0;
                            if (AAJourneyList.Count > 0)
                            {
                                for (int i = 0; i < AAJourneyList[0].segments.Count; i++)
                                {
                                    for (int k = 0; k < AAJourneyList[0].segments[i].fares.Count; k++)
                                    {
                                        for (int l = 0; l < AAJourneyList[0].segments[i].fares[k].passengerFares.Count; l++)
                                        {
                                            if (AAJourneyList[0].segments[i].fares[k].passengerFares[l].passengerType == "ADT")
                                            {
                                                for (int i2 = 0; i2 < AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges.Count; i2++)
                                                {
                                                    Adulttax += AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges[i2].amount;

                                                }
                                            }
                                            if (AAJourneyList[0].segments[i].fares[k].passengerFares[l].passengerType == "CHD")
                                            {
                                                for (int i2 = 0; i2 < AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges.Count; i2++)
                                                {

                                                    childtax += AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges[i2].amount;
                                                }
                                            }

                                            if (AAJourneyList[0].segments[i].fares[k].passengerFares[l].passengerType == "INF")
                                            {
                                                Inftcount = availibiltyRQGDS.infantcount;
                                                for (int i2 = 0; i2 < AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges.Count; i2++)
                                                {

                                                    infttax += AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges[i2].amount;
                                                }
                                                Inftbasefare = Convert.ToInt32(AAJourneyList[0].segments[i].fares[k].passengerFares[l].basicAmount);
                                                //Inftcount += Convert.ToInt32(_GetBookingFromStateRS.BookingData.Passengers.Length);
                                                AirAsiaTripResponceobj.inftcount = Inftcount;
                                                AirAsiaTripResponceobj.inftbasefare = Inftbasefare;
                                            }
                                        }
                                    }
                                }
                            }

                            int basefaretax = 0;
                            int basefareInfttax = 0;
                            if (Adulttax > 0)
                            {
                                basefaretax = Adulttax * adultcount;
                            }
                            if (childtax > 0)
                            {
                                basefaretax += childtax * childcount;
                            }
                            if (infttax > 0)
                            {
                                basefareInfttax = infttax * infantcount;
                                basefaretax += infttax * infantcount;
                            }
                            AirAsiaTripResponceobj.basefaretax = basefaretax;
                            AirAsiaTripResponceobj.journeys = AAJourneyList;
                            //passkeylist = passkeylist.OrderBy(p => p.passengerTypeCode).ToList();
                            AirAsiaTripResponceobj.passengers = sortedList;
                            AirAsiaTripResponceobj.passengerscount = passengercount;
                            AirAsiaTripResponceobj.infttax = basefareInfttax;
                            if (_journeySide == "0j")
                            {
                                HttpContext.Session.SetString("PricingSolutionValue_0", JsonConvert.SerializeObject(getAirPriceRes[0].PricingSolutionValue));
                            }
                            else
                            {
                                HttpContext.Session.SetString("PricingSolutionValue_1", JsonConvert.SerializeObject(getAirPriceRes[0].PricingSolutionValue));
                            }
                            //AirAsiaTripResponceobj.PriceSolution = getAirPriceRes[0].PricingSolutionValue;
                            #endregion
                            Passengerdata = new List<string>();
                            Passengerdata.Add("<Start>" + JsonConvert.SerializeObject(AirAsiaTripResponceobj) + "<End>");
                            //  HttpContext.Session.SetString("SGkeypassengerRT", JsonConvert.SerializeObject(AirAsiaTripResponceobj));
                            HttpContext.Session.SetString("keypassengerdata", JsonConvert.SerializeObject(Passengerdata));

                            seatMealdetail.KPassenger = objMongoHelper.Zip(JsonConvert.SerializeObject(AirAsiaTripResponceobj));
                            // seatMealdetail.Infant = objMongoHelper.Zip(JsonConvert.SerializeObject(Passengerdata));

                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Passengerdata)))
                            {
                                if (Passengerdata.Count == 2)
                                {
                                    MainPassengerdata = new List<string>();
                                }
                                MainPassengerdata.Add(JsonConvert.SerializeObject(Passengerdata));
                            }
                            #endregion

                        }
                    }
                    #region SeatMap 

                    #region AirAsia SeatMap 
                    if (_JourneykeyRTData.ToLower() == "airasia")
                    {
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "AirAsia").Result;
                        if (_journeySide == "0j")
                        {
                            token = tokenData.Token;
                        }
                        else
                        {
                            token = tokenData.RToken;
                        }

                        TimeSpan timeSpan1 = new TimeSpan(0, 0, 0, 10);
                        string _JourneykeyDataAA = journeySellKeyAA;
                        _JourneykeyDataAA = _JourneykeyDataAA.Replace(@"""", string.Empty);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responseSeatmap = await client.GetAsync(AppUrlConstant.Airasiaseatmap + _JourneykeyDataAA + "?IncludePropertyLookup=true");
                        if (responseSeatmap.IsSuccessStatusCode)
                        {
                            string columncount0 = string.Empty;
                            var _responseSeatmap = responseSeatmap.Content.ReadAsStringAsync().Result;
                            if (p == 0)
                            {
                                logs.WriteLogsR("getRequest" + " Url: " + AppUrlConstant.Airasiaseatmap + _JourneykeyDataAA + "?IncludePropertyLookup=true", "5-GetSeatmapReq_Left", "AirAsiaRT");
                                logs.WriteLogsR(_responseSeatmap, "5-GetSeatmapRes_Left", "AirAsiaRT");
                            }
                            else
                            {
                                logs.WriteLogsR("getRequest" + " Url: " + AppUrlConstant.Airasiaseatmap + _JourneykeyDataAA + "?IncludePropertyLookup=true", "5-GetSeatmap_Right", "AirAsiaRT");
                                logs.WriteLogsR(_responseSeatmap, "5-GetSeatmap_Right", "AirAsiaRT");
                            }

                            var JsonObjSeatmap = JsonConvert.DeserializeObject<dynamic>(_responseSeatmap);
                            var data = JsonObjSeatmap.data.Count;

                            List<data> datalist = new List<data>();
                            SeatMapResponceModel SeatMapResponceModel = null;
                            int x = 0;
                            foreach (Match mitem in Regex.Matches(_responseSeatmap, @"seatMap"":[\s\S]*?ssrLookup", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                            {
                                try
                                {
                                    data dataobj = new data();

                                    SeatMapResponceModel = new SeatMapResponceModel();
                                    List<SeatMapResponceModel> SeatMapResponceModellist = new List<SeatMapResponceModel>();
                                    Fees Fees = new Fees();
                                    Seatmap Seatmapobj = new Seatmap();
                                    Seatmapobj.name = JsonObjSeatmap.data[x].seatMap.name;
                                    Seatmapobj.arrivalStation = JsonObjSeatmap.data[x].seatMap.arrivalStation;
                                    Seatmapobj.departureStation = JsonObjSeatmap.data[x].seatMap.departureStation;
                                    Seatmapobj.marketingCode = JsonObjSeatmap.data[x].seatMap.marketingCode;
                                    Seatmapobj.equipmentType = JsonObjSeatmap.data[x].seatMap.equipmentType;
                                    Seatmapobj.equipmentTypeSuffix = JsonObjSeatmap.data[x].seatMap.equipmentTypeSuffix;
                                    Seatmapobj.category = JsonObjSeatmap.data[x].seatMap.category;
                                    Seatmapobj.seatmapReference = JsonObjSeatmap.data[x].seatMap.seatmapReference;

                                    List<Unit> compartmentsunitlist = new List<Unit>();
                                    Seatmapobj.decksindigo = new List<Decks>();
                                    Decks Decksobj = null;
                                    string compartmenttext = Regex.Match(mitem.Value, "compartments\":(?<data>[\\s\\S]*?),\"seatmapReference", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["data"].Value.ToString();
                                    foreach (Match itemn in Regex.Matches(compartmenttext, @"availableunits[\s\S]*?""designator"":""(?<t>[^\""""]+)""[\s\S]*?]}]", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                    {
                                        string _compartmentblock = itemn.Groups["t"].Value.Trim();
                                        compartmentsunitlist = new List<Unit>();
                                        Decksobj = new Decks();
                                        Decksobj.availableUnits = JsonObjSeatmap.data[x].seatMap.availableUnits;
                                        Decksobj.designator = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].designator;
                                        Decksobj.length = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].length;
                                        Decksobj.width = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].width;
                                        Decksobj.sequence = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].sequence;
                                        Decksobj.orientation = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].orientation;
                                        Seatmapobj.decks = Decksobj;
                                        int _count = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock]["units"].Count;
                                        for (int i1 = 0; i1 < JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock]["units"].Count; i1++)
                                        {
                                            try
                                            {
                                                Unit compartmentsunitobj = new Unit();
                                                compartmentsunitobj.unitKey = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].unitKey;
                                                compartmentsunitobj.assignable = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].assignable;
                                                compartmentsunitobj.availability = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].availability;
                                                compartmentsunitobj.compartmentDesignator = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].compartmentDesignator;
                                                compartmentsunitobj.designator = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].designator;
                                                compartmentsunitobj.type = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].type;
                                                compartmentsunitobj.travelClassCode = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].travelClassCode;
                                                compartmentsunitobj.set = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].set;
                                                compartmentsunitobj.group = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].group;
                                                compartmentsunitobj.priority = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].priority;
                                                compartmentsunitobj.text = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].text;
                                                compartmentsunitobj.setVacancy = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].setVacancy;
                                                compartmentsunitobj.angle = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].angle;
                                                compartmentsunitobj.width = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].width;
                                                compartmentsunitobj.height = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].height;
                                                compartmentsunitobj.zone = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].zone;
                                                compartmentsunitobj.x = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].x;
                                                compartmentsunitobj.y = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].y;

                                                compartmentsunitobj.Airline = Airlines.Airasia;
                                                foreach (var strTextdata in Regex.Matches(mitem.Value, @"seatMap"":[\s\S]*?ssrLookup"))
                                                {
                                                    foreach (Match item in Regex.Matches(strTextdata.ToString(), @"fees[\s\S]*?groups""(?<data>[\s\S]*?)ssrLookup"))
                                                    {
                                                        foreach (var groupid in Regex.Matches(item.ToString(), @"group"":(?<key>[\s\S]*?),[\s\S]*?type[\s\S]*?}"))
                                                        {

                                                            string farearraygroupid = Regex.Match(groupid.ToString(), @"group"":(?<key>[\s\S]*?),", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["key"].Value;

                                                            var feesgroupserviceChargescount = JsonObjSeatmap.data[x].fees[passengerkey12].groups[farearraygroupid].fees[0].serviceCharges.Count;

                                                            if (compartmentsunitobj.group == Convert.ToInt32(farearraygroupid))
                                                            {
                                                                compartmentsunitobj.servicechargefeeAmount = Convert.ToInt32(JsonObjSeatmap.data[x].fees[passengerkey12].groups[farearraygroupid].fees[0].serviceCharges[0].amount);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                compartmentsunitlist.Add(compartmentsunitobj);
                                                int compartmentypropertiesCount = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].properties.Count;
                                                List<Properties> Propertieslist = new List<Properties>();
                                                for (int j = 0; j < compartmentypropertiesCount; j++)
                                                {
                                                    Properties compartmentyproperties = new Properties();
                                                    compartmentyproperties.code = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].properties[j].code;
                                                    compartmentyproperties.value = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].properties[j].value;
                                                    Propertieslist.Add(compartmentyproperties);
                                                }
                                                compartmentsunitobj.properties = Propertieslist;
                                                if (compartmentsunitobj.designator.Contains('$'))
                                                {
                                                    columncount0 = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1 - 1].designator;
                                                    break;
                                                }

                                                compartmentsunitlist.Add(compartmentsunitobj);
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                        Decksobj.units = compartmentsunitlist;
                                        Seatmapobj.SeatColumnCount = Regex.Replace(columncount0, "[^0-9]", "");
                                        Seatmapobj.decksindigo.Add(Decksobj);
                                    }
                                    dataobj.seatMap = Seatmapobj;
                                    datalist.Add(dataobj);
                                    SeatMapResponceModel.datalist = datalist;

                                    x++;

                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            _SeatMapdata.Add("<Start>" + JsonConvert.SerializeObject(SeatMapResponceModel) + "<End>");
                            HttpContext.Session.SetString("_SeatmapData", JsonConvert.SerializeObject(_SeatMapdata));
                            HttpContext.Session.SetString("SeatmapRT", JsonConvert.SerializeObject(SeatMapResponceModel));
                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(_SeatMapdata)))
                            {
                                if (_SeatMapdata.Count == 2)
                                {
                                    MainSeatMapdata = new List<string>();
                                }
                                MainSeatMapdata.Add(JsonConvert.SerializeObject(_SeatMapdata));
                            }
                        }
                    }
                    #endregion

                    // Akasa Seat Map 
                    #region AkasaSeatMap 
                    if (_JourneykeyRTData.ToLower() == "akasaair")
                    {
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;

                        if (_journeySide == "0j")
                        {
                            token = tokenData.Token;
                        }
                        else
                        {
                            token = tokenData.RToken;
                        }
                        TimeSpan timeSpan1 = new TimeSpan(0, 0, 0, 10);
                        string _JourneykeyDataAA = HttpContext.Session.GetString("journeySellKey");
                        _JourneykeyDataAA = _JourneykeyDataAA.Replace(@"""", string.Empty);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        HttpResponseMessage responseSeatmap = await client.GetAsync(AppUrlConstant.AkasaAirSeatMap + _JourneykeyDataAA + "?IncludePropertyLookup=true");
                        if (responseSeatmap.IsSuccessStatusCode)
                        {
                            try
                            {
                                _SeatMapdata = new List<string>();
                                string columncount0 = string.Empty;
                                Logs logs = new Logs();
                                var _responseSeatmap = responseSeatmap.Content.ReadAsStringAsync().Result;
                                if (p == 0)
                                {
                                    logs.WriteLogsR("getRequest" + " Url: " + BaseAkasaURL + "/api/nsk/v3/booking/seatmaps/journey/" + _JourneykeyDataAA + "?IncludePropertyLookup=true", "5-GetSeatmapReq_Left", "AkasaRT");
                                    logs.WriteLogsR(_responseSeatmap, "5-GetSeatmapRes_Left", "AkasaRT");
                                }
                                else
                                {
                                    logs.WriteLogsR("getRequest" + " Url: " + BaseAkasaURL + "/api/nsk/v3/booking/seatmaps/journey/" + _JourneykeyDataAA + "?IncludePropertyLookup=true", "5-GetSeatmapReq_Right", "AkasaRT");
                                    logs.WriteLogsR(_responseSeatmap, "5-GetSeatmapRes_Right", "AkasaRT");
                                }


                                var JsonObjSeatmap = JsonConvert.DeserializeObject<dynamic>(_responseSeatmap);
                                var data = JsonObjSeatmap.data.Count;
                                int x = 0;
                                List<data> datalist = new List<data>();
                                SeatMapResponceModel SeatMapResponceModel = null;
                                foreach (Match mitem in Regex.Matches(_responseSeatmap, @"seatMap"":[\s\S]*?ssrLookup", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {
                                    try
                                    {
                                        data dataobj = new data();

                                        SeatMapResponceModel = new SeatMapResponceModel();
                                        List<SeatMapResponceModel> SeatMapResponceModellist = new List<SeatMapResponceModel>();
                                        Fees Fees = new Fees();
                                        Seatmap Seatmapobj = new Seatmap();
                                        Seatmapobj.name = JsonObjSeatmap.data[x].seatMap.name;
                                        TempData["AirLineName"] = Seatmapobj.name;
                                        Seatmapobj.arrivalStation = JsonObjSeatmap.data[x].seatMap.arrivalStation;
                                        Seatmapobj.departureStation = JsonObjSeatmap.data[x].seatMap.departureStation;
                                        Seatmapobj.marketingCode = JsonObjSeatmap.data[x].seatMap.marketingCode;
                                        Seatmapobj.equipmentType = JsonObjSeatmap.data[x].seatMap.equipmentType;
                                        Seatmapobj.equipmentTypeSuffix = JsonObjSeatmap.data[x].seatMap.equipmentTypeSuffix;
                                        Seatmapobj.category = JsonObjSeatmap.data[x].seatMap.category;
                                        Seatmapobj.seatmapReference = JsonObjSeatmap.data[x].seatMap.seatmapReference;
                                        List<Unit> compartmentsunitlist = new List<Unit>();
                                        Seatmapobj.decksindigo = new List<Decks>();
                                        Decks Decksobj = null;
                                        string compartmenttext = Regex.Match(mitem.Value, "compartments\":(?<data>[\\s\\S]*?),\"seatmapReference", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["data"].Value.ToString();
                                        foreach (Match itemn in Regex.Matches(compartmenttext, @"availableunits[\s\S]*?""designator"":""(?<t>[^\""""]+)""[\s\S]*?(?:]}]|]})", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                        {
                                            try
                                            {
                                                string _compartmentblock = itemn.Groups["t"].Value.Trim();
                                                compartmentsunitlist = new List<Unit>();
                                                Decksobj = new Decks();
                                                Decksobj.availableUnits = JsonObjSeatmap.data[x].seatMap.availableUnits;
                                                Decksobj.designator = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].designator;
                                                Decksobj.length = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].length;
                                                Decksobj.width = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].width;
                                                Decksobj.sequence = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].sequence;
                                                Decksobj.orientation = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].orientation;
                                                Seatmapobj.decks = Decksobj;
                                                int _count = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock]["units"].Count;
                                                for (int i1 = 0; i1 < JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock]["units"].Count; i1++)
                                                {
                                                    try
                                                    {
                                                        Unit compartmentsunitobj = new Unit();
                                                        compartmentsunitobj.Airline = Airlines.AkasaAir;
                                                        compartmentsunitobj.unitKey = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].unitKey;
                                                        compartmentsunitobj.assignable = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].assignable;
                                                        compartmentsunitobj.availability = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].availability;
                                                        compartmentsunitobj.compartmentDesignator = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].compartmentDesignator;
                                                        compartmentsunitobj.designator = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].designator;
                                                        compartmentsunitobj.type = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].type;
                                                        compartmentsunitobj.travelClassCode = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].travelClassCode;
                                                        compartmentsunitobj.set = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].set;
                                                        compartmentsunitobj.group = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].group;
                                                        compartmentsunitobj.priority = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].priority;
                                                        compartmentsunitobj.text = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].text;
                                                        compartmentsunitobj.setVacancy = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].setVacancy;
                                                        compartmentsunitobj.angle = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].angle;
                                                        compartmentsunitobj.width = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].width;
                                                        compartmentsunitobj.height = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].height;
                                                        compartmentsunitobj.zone = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].zone;
                                                        compartmentsunitobj.x = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].x;
                                                        compartmentsunitobj.y = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].y;

                                                        foreach (var strTextdata in Regex.Matches(mitem.Value, @"seatMap"":[\s\S]*?ssrLookup"))
                                                        {
                                                            foreach (Match item in Regex.Matches(strTextdata.ToString(), @"fees[\s\S]*?groups""(?<data>[\s\S]*?)ssrLookup"))
                                                            {
                                                                foreach (var groupid in Regex.Matches(item.ToString(), @"group"":(?<key>[\s\S]*?),[\s\S]*?type[\s\S]*?}"))
                                                                {

                                                                    string farearraygroupid = Regex.Match(groupid.ToString(), @"group"":(?<key>[\s\S]*?),", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["key"].Value;

                                                                    var feesgroupserviceChargescount = JsonObjSeatmap.data[x].fees[passengerkey12].groups[farearraygroupid].fees[0].serviceCharges.Count;

                                                                    if (compartmentsunitobj.group == Convert.ToInt32(farearraygroupid))
                                                                    {
                                                                        compartmentsunitobj.servicechargefeeAmount = Convert.ToInt32(JsonObjSeatmap.data[x].fees[passengerkey12].groups[farearraygroupid].fees[0].serviceCharges[0].amount);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        compartmentsunitlist.Add(compartmentsunitobj);
                                                        int compartmentypropertiesCount = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].properties.Count;
                                                        List<Properties> Propertieslist = new List<Properties>();
                                                        for (int j = 0; j < compartmentypropertiesCount; j++)
                                                        {
                                                            Properties compartmentyproperties = new Properties();
                                                            compartmentyproperties.code = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].properties[j].code;
                                                            compartmentyproperties.value = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1].properties[j].value;
                                                            Propertieslist.Add(compartmentyproperties);
                                                        }
                                                        compartmentsunitobj.properties = Propertieslist;
                                                        if (compartmentsunitobj.designator.Contains('$'))
                                                        {
                                                            columncount0 = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments[_compartmentblock].units[i1 - 1].designator;
                                                            break;
                                                        }

                                                        compartmentsunitlist.Add(compartmentsunitobj);
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }
                                                Decksobj.units = compartmentsunitlist;
                                                Seatmapobj.SeatColumnCount = Regex.Replace(columncount0, "[^0-9]", "");
                                                Seatmapobj.decksindigo.Add(Decksobj);
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                        dataobj.seatMap = Seatmapobj;
                                        datalist.Add(dataobj);
                                        SeatMapResponceModel.datalist = datalist;

                                        x++;
                                    }
                                    catch (Exception ex)
                                    {

                                    }


                                }
                                _SeatMapdata = new List<string>();
                                _SeatMapdata.Add("<Start>" + JsonConvert.SerializeObject(SeatMapResponceModel) + "<End>");
                                HttpContext.Session.SetString("_SeatmapData", JsonConvert.SerializeObject(_SeatMapdata));
                                HttpContext.Session.SetString("SeatmapRT", JsonConvert.SerializeObject(SeatMapResponceModel));
                                if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(_SeatMapdata)))
                                {
                                    if (_SeatMapdata.Count == 2)
                                    {
                                        MainSeatMapdata = new List<string>();
                                    }
                                    MainSeatMapdata.Add(JsonConvert.SerializeObject(_SeatMapdata));
                                }

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }

                    #endregion

                    //SpicejetSeat map
                    #region SeatMap Spicejet
                    if (_JourneykeyRTData.ToLower() == "spicejet")
                    {
                        if (string.IsNullOrEmpty(AirAsiaTripResponceobj.ErrorMsg))
                        {
                            GetSeatAvailabilityRequest _getseatAvailabilityRequest = new GetSeatAvailabilityRequest();
                            GetSeatAvailabilityResponse _getSeatAvailabilityResponse = new GetSeatAvailabilityResponse();

                            _getseatAvailabilityRequest.Signature = Signature;
                            _getseatAvailabilityRequest.ContractVersion = 420;

                            SeatAvailabilityRequest _seatRequest = new SeatAvailabilityRequest();
                            List<GetSeatAvailabilityResponse> SeatGroup = new List<GetSeatAvailabilityResponse>();
                            for (int i = 0; i < AirAsiaTripResponceobj.journeys[0].segments.Count; i++)
                            {
                                _seatRequest = new SeatAvailabilityRequest();
                                _seatRequest.STDSpecified = true;
                                _seatRequest.STD = AirAsiaTripResponceobj.journeys[0].segments[i].designator.departure;
                                _seatRequest.DepartureStation = AirAsiaTripResponceobj.journeys[0].segments[i].designator.origin;
                                _seatRequest.ArrivalStation = AirAsiaTripResponceobj.journeys[0].segments[i].designator.destination;
                                _seatRequest.IncludeSeatFees = true;
                                _seatRequest.IncludeSeatFeesSpecified = true;
                                _seatRequest.SeatAssignmentModeSpecified = true;
                                _seatRequest.SeatAssignmentMode = SeatAssignmentMode.PreSeatAssignment;
                                _seatRequest.FlightNumber = AirAsiaTripResponceobj.journeys[0].segments[i].identifier.identifier;
                                _seatRequest.OverrideSTDSpecified = true;
                                _seatRequest.OverrideSTD = AirAsiaTripResponceobj.journeys[0].segments[i].designator.departure;
                                _seatRequest.CarrierCode = AirAsiaTripResponceobj.journeys[0].segments[i].identifier.carrierCode;
                                _getseatAvailabilityRequest.SeatAvailabilityRequest = _seatRequest;
                                _getSeatAvailabilityResponse = await objSpiceJet.GetseatAvaialbility(_getseatAvailabilityRequest);
                                SeatGroup.Add(_getSeatAvailabilityResponse);

                            }
                            if (p == 0)
                            {
                                logs.WriteLogsR(JsonConvert.SerializeObject(_getseatAvailabilityRequest), "8-GetSeatAvailabilityReq_Left", "SpiceJetRT");
                                logs.WriteLogsR(JsonConvert.SerializeObject(SeatGroup), "8-GetSeatAvailabilityRes_Left", "SpiceJetRT");
                            }
                            else
                            {
                                logs.WriteLogsR(JsonConvert.SerializeObject(_getseatAvailabilityRequest), "8-GetSeatAvailabilityReq_Right", "SpiceJetRT");
                                logs.WriteLogsR(JsonConvert.SerializeObject(SeatGroup), "8-GetSeatAvailabilityRes_Right", "SpiceJetRT");
                            }
                            if (SeatGroup != null)
                            {
                                string columncount0 = string.Empty;
                                var data = SeatGroup.Count;// _getSeatAvailabilityResponse.SeatAvailabilityResponse.EquipmentInfos.Length;

                                List<data> datalist = new List<data>();
                                SeatMapResponceModel SeatMapResponceModel = new SeatMapResponceModel();
                                List<SeatMapResponceModel> SeatMapResponceModellist = new List<SeatMapResponceModel>();
                                for (int x = 0; x < data; x++)
                                {
                                    data dataobj = new data();

                                    SeatMapResponceModel = new SeatMapResponceModel();
                                    SeatMapResponceModellist = new List<SeatMapResponceModel>();
                                    Fees Fees = new Fees();
                                    Seatmap Seatmapobj = new Seatmap();
                                    if (SeatGroup[x] != null)
                                    {
                                        Seatmapobj.name = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Name;
                                        TempData["AirCraftName"] = Seatmapobj.name;
                                        Seatmapobj.arrivalStation = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].ArrivalStation;
                                        Seatmapobj.departureStation = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].DepartureStation;
                                        Seatmapobj.marketingCode = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].MarketingCode;
                                        Seatmapobj.equipmentType = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].EquipmentType;
                                        Seatmapobj.equipmentTypeSuffix = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].EquipmentTypeSuffix;
                                        int compartmentsunitCount = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments.Length;
                                        List<Unit> compartmentsunitlist = new List<Unit>();
                                        Seatmapobj.decksindigo = new List<Decks>();
                                        Decks Decksobj = null;
                                        for (int i = 0; i < compartmentsunitCount; i++)
                                        {
                                            compartmentsunitlist = new List<Unit>();
                                            Decksobj = new Decks();
                                            Decksobj.availableUnits = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].AvailableUnits;
                                            Decksobj.designator = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].CompartmentDesignator;
                                            Decksobj.length = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Length;
                                            Decksobj.width = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Width;
                                            Decksobj.sequence = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Sequence;
                                            Decksobj.orientation = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Orientation;
                                            for (int i1 = 0; i1 < SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats.Length; i1++)
                                            {
                                                try
                                                {
                                                    Unit compartmentsunitobj = new Unit();

                                                    compartmentsunitobj.assignable = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Assignable;
                                                    compartmentsunitobj.availability = Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatAvailability);
                                                    compartmentsunitobj.compartmentDesignator = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].CompartmentDesignator;
                                                    compartmentsunitobj.designator = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatDesignator;
                                                    compartmentsunitobj.type = Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatGroup);
                                                    compartmentsunitobj.travelClassCode = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].TravelClassCode;
                                                    compartmentsunitobj.set = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatSet;
                                                    compartmentsunitobj.group = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatGroup;
                                                    compartmentsunitobj.priority = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Priority;
                                                    compartmentsunitobj.text = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Text;
                                                    compartmentsunitobj.angle = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatAngle;
                                                    compartmentsunitobj.width = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Width;
                                                    compartmentsunitobj.height = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Height;
                                                    compartmentsunitobj.zone = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Zone;
                                                    compartmentsunitobj.x = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].X;
                                                    compartmentsunitobj.y = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Y;

                                                    compartmentsunitobj.Airline = Airlines.Spicejet;

                                                    for (int k = 0; k < SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees.Length; k++)
                                                    {
                                                        if (compartmentsunitobj.group == Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[k].SeatGroup))
                                                        {
                                                            var feesgroupserviceChargescount = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[k].PassengerFee.ServiceCharges.Length;

                                                            List<Servicecharge> feesgroupserviceChargeslist = new List<Servicecharge>();
                                                            for (int l = 0; l < feesgroupserviceChargescount; l++)
                                                            {
                                                                if (l > 0)
                                                                {
                                                                    break;
                                                                }
                                                                else
                                                                {
                                                                    compartmentsunitobj.servicechargefeeAmount += Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[k].PassengerFee.ServiceCharges[l].Amount);
                                                                }

                                                            }
                                                            break;
                                                        }
                                                    }
                                                    compartmentsunitobj.unitKey = compartmentsunitobj.designator;
                                                    int compartmentypropertiesCount = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].PropertyList.Length;
                                                    List<Properties> Propertieslist = new List<Properties>();
                                                    for (int j = 0; j < compartmentypropertiesCount; j++)
                                                    {
                                                        Properties compartmentyproperties = new Properties();
                                                        compartmentyproperties.code = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].PropertyList[j].TypeCode;
                                                        compartmentyproperties.value = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].PropertyList[j].Value;
                                                        Propertieslist.Add(compartmentyproperties);
                                                    }

                                                    compartmentsunitobj.properties = Propertieslist;
                                                    if (compartmentsunitobj.designator.Contains('$'))
                                                    {
                                                        columncount0 = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1 - 1].SeatDesignator;
                                                        break;
                                                    }
                                                    compartmentsunitlist.Add(compartmentsunitobj);

                                                }
                                                catch (Exception ex)
                                                {

                                                }

                                            }
                                            Decksobj.units = compartmentsunitlist;
                                            Seatmapobj.SeatColumnCount = Regex.Replace(columncount0, "[^0-9]", "");
                                            Seatmapobj.decksindigo.Add(Decksobj);
                                        }

                                        List<Groups> GroupsFeelist = new List<Groups>();
                                        int testcount = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees.Length;
                                        for (int i = 0; i < testcount; i++)
                                        {
                                            Groups Groupsobj = new Groups();
                                            GroupsFee GroupsFeeobj = new GroupsFee();
                                            string feeseatGroup = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].SeatGroup.ToString();
                                            //doubt
                                            GroupsFeeobj.SeatGroup = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].SeatGroup.ToString();
                                            GroupsFeeobj.type = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeNumber;
                                            GroupsFeeobj.ssrCode = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.SSRCode;
                                            GroupsFeeobj.ssrNumber = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.SSRNumber;
                                            GroupsFeeobj.paymentNumber = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.PaymentNumber;
                                            GroupsFeeobj.isConfirmed = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeOverride;
                                            GroupsFeeobj.isConfirming = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeOverride;
                                            GroupsFeeobj.isConfirmingExternal = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeOverride;
                                            GroupsFeeobj.code = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeCode;
                                            GroupsFeeobj.detail = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeDetail;
                                            GroupsFeeobj.flightReference = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FlightReference;
                                            GroupsFeeobj.note = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.Note;
                                            GroupsFeeobj.createdDate = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.CreatedDate;
                                            GroupsFeeobj.isProtected = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.IsProtected;

                                            var feesgroupserviceChargescount = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges.Length;

                                            List<Servicecharge> feesgroupserviceChargeslist = new List<Servicecharge>();
                                            for (int l = 0; l < feesgroupserviceChargescount; l++)
                                            {
                                                Servicecharge feesgroupserviceChargesobj = new Servicecharge();
                                                feesgroupserviceChargesobj.amount = Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].Amount);
                                                feesgroupserviceChargesobj.code = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].ChargeCode; ;
                                                feesgroupserviceChargesobj.detail = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].ChargeDetail;
                                                feesgroupserviceChargesobj.currencyCode = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].CurrencyCode;

                                                feesgroupserviceChargesobj.foreignAmount = Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].ForeignAmount);
                                                feesgroupserviceChargesobj.ticketCode = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].TicketCode;
                                                feesgroupserviceChargeslist.Add(feesgroupserviceChargesobj);

                                            }
                                            GroupsFeeobj.serviceCharges = feesgroupserviceChargeslist;

                                            Groupsobj.groupsFee = GroupsFeeobj;
                                            GroupsFeelist.Add(Groupsobj);

                                            Fees.groups = GroupsFeelist;

                                        }
                                        dataobj.seatMap = Seatmapobj;
                                        dataobj.seatMapfees = Fees;
                                        datalist.Add(dataobj);
                                        SeatMapResponceModel.datalist = datalist;

                                        string strseat = JsonConvert.SerializeObject(SeatMapResponceModel);

                                        SeatMapdata = new List<string>();
                                        SeatMapdata.Add("<Start>" + JsonConvert.SerializeObject(SeatMapResponceModel) + "<End>");
                                        HttpContext.Session.SetString("SeatmapRT", JsonConvert.SerializeObject(SeatMapResponceModel));
                                        HttpContext.Session.SetString("SeatmapData", JsonConvert.SerializeObject(SeatMapdata));
                                    }
                                    else
                                    {
                                        SeatMapdata = new List<string>();
                                        SeatMapResponceModel = new SeatMapResponceModel();
                                        SeatMapResponceModel.datalist = new List<data>();
                                        SeatMapdata.Add("<Start>" + JsonConvert.SerializeObject(SeatMapResponceModel) + "<End>");
                                        HttpContext.Session.SetString("SeatmapRT", JsonConvert.SerializeObject(SeatMapResponceModel));
                                        HttpContext.Session.SetString("SeatmapData", JsonConvert.SerializeObject(SeatMapdata));
                                    }
                                }
                                if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(SeatMapdata)))
                                {
                                    if (SeatMapdata.Count == 2)
                                    {
                                        MainSeatMapdata = new List<string>();
                                    }
                                    MainSeatMapdata.Add(JsonConvert.SerializeObject(SeatMapdata));
                                }
                            }
                        }
                    }

                    #endregion
                    //IndigoSeat map
                    #region SeatMap Indigo
                    if (_JourneykeyRTData.ToLower() == "indigo")
                    {
                        //if (AirAsiaTripResponceobj == null)
                        //{
                        //AirAsiaTripResponceobj = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(HttpContext.Session.GetString("SGkeypassengerRT"), typeof(AirAsiaTripResponceModel));
                        //AirAsiaTripResponceobj = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(seatMealdetail.Infant, typeof(AirAsiaTripResponceModel));
                        //}

                        _GetSSR objssr = new _GetSSR();
                        List<IndigoBookingManager_.GetSeatAvailabilityResponse> SeatGroup = await objssr.GetseatAvailability(Signature, AirAsiaTripResponceobj, p);
                        if (SeatGroup != null)
                        {
                            string columncount0 = string.Empty;
                            var data = SeatGroup.Count;// _getSeatAvailabilityResponse.SeatAvailabilityResponse.EquipmentInfos.Length;
                            List<data> datalist = new List<data>();
                            SeatMapResponceModel SeatMapResponceModel = new SeatMapResponceModel();
                            List<SeatMapResponceModel> SeatMapResponceModellist = new List<SeatMapResponceModel>();
                            for (int x = 0; x < data; x++)
                            {
                                data dataobj = new data();
                                SeatMapResponceModel = new SeatMapResponceModel();
                                SeatMapResponceModellist = new List<SeatMapResponceModel>();
                                Fees Fees = new Fees();
                                Seatmap Seatmapobj = new Seatmap();
                                Seatmapobj.name = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Name;
                                TempData["AirCraftName"] = Seatmapobj.name;
                                Seatmapobj.arrivalStation = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].ArrivalStation;
                                Seatmapobj.departureStation = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].DepartureStation;
                                Seatmapobj.marketingCode = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].MarketingCode;
                                Seatmapobj.equipmentType = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].EquipmentType;
                                Seatmapobj.equipmentTypeSuffix = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].EquipmentTypeSuffix;
                                int compartmentsunitCount = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments.Length;
                                List<Unit> compartmentsunitlist = new List<Unit>();
                                Seatmapobj.decksindigo = new List<Decks>();
                                Decks Decksobj = null;
                                for (int i = 0; i < compartmentsunitCount; i++)
                                {
                                    compartmentsunitlist = new List<Unit>();
                                    Decksobj = new Decks();
                                    Decksobj.availableUnits = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].AvailableUnits;
                                    Decksobj.designator = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].CompartmentDesignator;
                                    Decksobj.length = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Length;
                                    Decksobj.width = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Width;
                                    Decksobj.sequence = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Sequence;
                                    Decksobj.orientation = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Orientation;
                                    for (int i1 = 0; i1 < SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats.Length; i1++)
                                    {
                                        try
                                        {
                                            Unit compartmentsunitobj = new Unit();
                                            compartmentsunitobj.assignable = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Assignable;
                                            compartmentsunitobj.availability = Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatAvailability);
                                            compartmentsunitobj.compartmentDesignator = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].CompartmentDesignator;
                                            compartmentsunitobj.designator = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatDesignator;
                                            compartmentsunitobj.type = Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatGroup);
                                            compartmentsunitobj.travelClassCode = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].TravelClassCode;
                                            compartmentsunitobj.set = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatSet;
                                            compartmentsunitobj.group = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatGroup;
                                            compartmentsunitobj.priority = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Priority;
                                            compartmentsunitobj.text = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Text;
                                            compartmentsunitobj.angle = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatAngle;
                                            compartmentsunitobj.width = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Width;
                                            compartmentsunitobj.height = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Height;
                                            compartmentsunitobj.zone = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Zone;
                                            compartmentsunitobj.x = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].X;
                                            compartmentsunitobj.y = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Y;

                                            compartmentsunitobj.Airline = Airlines.Indigo;
                                            for (int k = 0; k < SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees.Length; k++)
                                            {
                                                if (compartmentsunitobj.group == Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[k].SeatGroup))
                                                {
                                                    var feesgroupserviceChargescount = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[k].PassengerFee.ServiceCharges.Length;
                                                    List<Servicecharge> feesgroupserviceChargeslist = new List<Servicecharge>();
                                                    for (int l = 0; l < feesgroupserviceChargescount; l++)
                                                    {
                                                        if (l > 0)
                                                        {
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            compartmentsunitobj.servicechargefeeAmount += Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[k].PassengerFee.ServiceCharges[l].Amount);
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                            compartmentsunitobj.unitKey = compartmentsunitobj.designator;
                                            int compartmentypropertiesCount = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].PropertyList.Length;
                                            List<Properties> Propertieslist = new List<Properties>();
                                            for (int j = 0; j < compartmentypropertiesCount; j++)
                                            {
                                                Properties compartmentyproperties = new Properties();
                                                compartmentyproperties.code = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].PropertyList[j].TypeCode;
                                                compartmentyproperties.value = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].PropertyList[j].Value;
                                                Propertieslist.Add(compartmentyproperties);
                                            }
                                            compartmentsunitobj.properties = Propertieslist;
                                            if (compartmentsunitobj.designator.Contains('$'))
                                            {
                                                columncount0 = SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1 - 1].SeatDesignator;
                                                break;
                                            }
                                            compartmentsunitlist.Add(compartmentsunitobj);

                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                    }
                                    Decksobj.units = compartmentsunitlist;
                                    Seatmapobj.SeatColumnCount = Regex.Replace(columncount0, "[^0-9]", "");
                                    Seatmapobj.decksindigo.Add(Decksobj);
                                }
                                List<Groups> GroupsFeelist = new List<Groups>();
                                int testcount = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees.Length;
                                for (int i = 0; i < testcount; i++)
                                {
                                    Groups Groupsobj = new Groups();
                                    GroupsFee GroupsFeeobj = new GroupsFee();
                                    string feeseatGroup = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].SeatGroup.ToString();
                                    GroupsFeeobj.SeatGroup = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].SeatGroup.ToString();
                                    GroupsFeeobj.type = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeNumber;
                                    GroupsFeeobj.ssrCode = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.SSRCode;
                                    GroupsFeeobj.ssrNumber = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.SSRNumber;
                                    GroupsFeeobj.paymentNumber = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.PaymentNumber;
                                    GroupsFeeobj.isConfirmed = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeOverride;
                                    GroupsFeeobj.isConfirming = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeOverride;
                                    GroupsFeeobj.isConfirmingExternal = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeOverride;
                                    GroupsFeeobj.code = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeCode;
                                    GroupsFeeobj.detail = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FeeDetail;
                                    GroupsFeeobj.flightReference = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.FlightReference;
                                    GroupsFeeobj.note = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.Note;
                                    GroupsFeeobj.createdDate = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.CreatedDate;
                                    GroupsFeeobj.isProtected = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.IsProtected;
                                    var feesgroupserviceChargescount = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges.Length;
                                    List<Servicecharge> feesgroupserviceChargeslist = new List<Servicecharge>();
                                    for (int l = 0; l < feesgroupserviceChargescount; l++)
                                    {
                                        Servicecharge feesgroupserviceChargesobj = new Servicecharge();
                                        feesgroupserviceChargesobj.amount = Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].Amount);
                                        feesgroupserviceChargesobj.code = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].ChargeCode; ;
                                        feesgroupserviceChargesobj.detail = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].ChargeDetail;
                                        feesgroupserviceChargesobj.currencyCode = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].CurrencyCode;
                                        feesgroupserviceChargesobj.foreignAmount = Convert.ToInt32(SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].ForeignAmount);
                                        feesgroupserviceChargesobj.ticketCode = SeatGroup[x].SeatAvailabilityResponse.SeatGroupPassengerFees[i].PassengerFee.ServiceCharges[l].TicketCode;
                                        feesgroupserviceChargeslist.Add(feesgroupserviceChargesobj);
                                    }
                                    GroupsFeeobj.serviceCharges = feesgroupserviceChargeslist;
                                    Groupsobj.groupsFee = GroupsFeeobj;
                                    GroupsFeelist.Add(Groupsobj);
                                    Fees.groups = GroupsFeelist;
                                }
                                dataobj.seatMap = Seatmapobj;
                                dataobj.seatMapfees = Fees;
                                datalist.Add(dataobj);
                                SeatMapResponceModel.datalist = datalist;
                            }
                            string strseat = JsonConvert.SerializeObject(SeatMapResponceModel);
                            SeatMapdata = new List<string>();
                            SeatMapdata.Add("<Start>" + JsonConvert.SerializeObject(SeatMapResponceModel) + "<End>");
                            HttpContext.Session.SetString("SeatmapRT", JsonConvert.SerializeObject(SeatMapResponceModel));
                            HttpContext.Session.SetString("SeatmapData", JsonConvert.SerializeObject(SeatMapdata));
                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(SeatMapdata)))
                            {
                                if (SeatMapdata.Count == 2)
                                {
                                    MainSeatMapdata = new List<string>();
                                }
                                MainSeatMapdata.Add(JsonConvert.SerializeObject(SeatMapdata));
                            }
                        }
                    }
                    #endregion

                    #region SeatMap GDS
                    if (_JourneykeyRTData.ToLower() == "vistara" || _JourneykeyRTData.ToLower() == "airindia" || _JourneykeyRTData.ToLower() == "hehnair")
                    {
                        #region SeatMap
                        _testURL = AppUrlConstant.GDSSeatURL;
                        SeatMapres = _objAvail.GetSeatMapRoundTrip(_testURL, fareRepriceReq, availibiltyRQGDS, newGuid.ToString(), _targetBranch, _userName, _password, Airfaredata, farebasisdata, HostTokenKey, HostTokenValue, p, "");
                        List<IndigoBookingManager_.GetSeatAvailabilityResponse> SeatGroup = null;// await objssr.GetseatAvailability(Signature, AirAsiaTripResponceobj, "OneWay");
                        if (SeatMapres != null)
                        {
                            if (p == 0)
                            {
                                HttpContext.Session.SetString("SeatResponseleft", JsonConvert.SerializeObject(SeatMapres));
                            }
                            else
                            {
                                HttpContext.Session.SetString("SeatResponseright", JsonConvert.SerializeObject(SeatMapres));
                            }
                            string columncount0 = string.Empty;
                            var data = 2;// SeatMapres.Count;// _getSeatAvailabilityResponse.SeatAvailabilityResponse.EquipmentInfos.Length;
                            List<data> datalist = new List<data>();
                            Hashtable htseat = new Hashtable();
                            //Dictionary<string, string> dictSeat = new Dictionary<string, string>();
                            SeatMapResponceModel SeatMapResponceModel = new SeatMapResponceModel();
                            List<SeatMapResponceModel> SeatMapResponceModellist = new List<SeatMapResponceModel>();
                            foreach (Match mitem in Regex.Matches(SeatMapres, @"<air:AirSegment\s*Key=""(?<segmentkey>[\s\S]*?)""[\s\S]*?</air:Airsegment>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                            {
                                data dataobj = new data();

                                SeatMapResponceModel = new SeatMapResponceModel();
                                SeatMapResponceModellist = new List<SeatMapResponceModel>();
                                Fees Fees = new Fees();
                                Seatmap Seatmapobj = new Seatmap();
                                Regex obj = new Regex("<air:AirSegment\\s*Key=\"(?<segmentkey>[\\s\\S]*?)\"[\\s\\S]*?Origin=\"(?<departureStation>[\\s\\S]*?)\"[\\s\\S]*?Destination=\"(?<arrivalStation>[\\s\\S]*?)\"[\\s\\S]*?Equipment=\"(?<Equipment>[\\s\\S]*?)\"[\\s\\S]*?</air:Airsegment>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                Seatmapobj.name = obj.Match(mitem.Value).Groups["Equipment"].Value.Trim();// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Name;
                                TempData["AirCraftName"] = ""; //Seatmapobj.name;
                                Seatmapobj.arrivalStation = obj.Match(mitem.Value).Groups["arrivalStation"].Value.Trim(); //SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].ArrivalStation;
                                Seatmapobj.departureStation = obj.Match(mitem.Value).Groups["departureStation"].Value.Trim(); //SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].DepartureStation;
                                Seatmapobj.marketingCode = ""; //SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].MarketingCode;
                                Seatmapobj.equipmentType = ""; //SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].EquipmentType;
                                Seatmapobj.equipmentTypeSuffix = "";// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].EquipmentTypeSuffix;

                                int compartmentsunitCount = 2;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments.Length;
                                List<Unit> compartmentsunitlist = new List<Unit>();
                                //List<Decks> Decksobjarray = new List<Decks>();
                                Seatmapobj.decksindigo = new List<Decks>();
                                Decks Decksobj = null;
                                string _seatPosition = "";
                                //for (int i = 0; i < compartmentsunitCount; i++) // 2 times 
                                Hashtable htPaidSeatPrice = new Hashtable();
                                string BookingTravellerref = string.Empty;
                                foreach (Match mSeat in Regex.Matches(SeatMapres, @"<air:OptionalService Type=""PreReservedSeatAssignment[\s\S]*?TotalPrice=""(?<Price>[\s\S]*?)""[\s\S]*?Key=""(?<Key>[\s\S]*?)""[\s\S]*?</air:OptionalService>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {
                                    if (!htPaidSeatPrice.Contains(mSeat.Groups["Key"].Value.Trim()))
                                    {
                                        htPaidSeatPrice.Add(mSeat.Groups["Key"].Value.Trim(), mSeat.Groups["Price"].Value.Trim());
                                    }

                                    //string key = mSeat.Groups["Price"].Value.Trim().Replace("INR", "");
                                    //string value = mSeat.Value.Trim();

                                    //if (!dictSeat.ContainsKey(key))
                                    //{
                                    //    dictSeat.Add(key, value);
                                    //}

                                    if (!htseat.Contains(mSeat.Groups["Price"].Value.Trim().Replace("INR", "")))
                                    {
                                        htseat.Add(mSeat.Groups["Price"].Value.Trim().Replace("INR", ""), mSeat.Value.Trim());
                                    }
                                    BookingTravellerref = Regex.Match(mSeat.Value.Trim(), "BookingTravelerRef=\"(?<Travellerref>[\\s\\S]*?)\"").Groups["Travellerref"].Value.Trim();

                                }


                                foreach (Match mRows in Regex.Matches(SeatMapres, @"<air:Rows SegmentRef=""(?<Key>[\s\S]*?)""[\s\S]*?</air:Rows>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                {

                                    compartmentsunitlist = new List<Unit>();
                                    Decksobj = new Decks();
                                    if (obj.Match(mitem.Value).Groups["segmentkey"].Value.Trim() == mRows.Groups["Key"].Value)
                                    {
                                        foreach (Match mFacility in Regex.Matches(mRows.Value, @"<air:Facility Type=""[\s\S]*?SeatCode=""(?<SeatNumber>[\s\S]*?)""\s*Availability=""(?<Availablity>[\s\S]*?)""[\s\S]*?>[\s\S]*?</air:Facility>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                        {
                                            string _OptionalServiceRef = string.Empty;
                                            if (mFacility.Value.Contains("OptionalServiceRef"))
                                            {
                                                _OptionalServiceRef = Regex.Match(mFacility.Value, @"<air:Facility Type=""[\s\S]*?OptionalServiceRef=""(?<optionkey>[\s\S]*?)""[\s\S]*?</air:Facility>", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["optionkey"].Value.Trim();
                                            }
                                            int _Count = Regex.Matches(mRows.Value, @"<air:Facility Type=""(?<SeatNumber>[\s\S]*?)""[s\S]*?</air:Facility>", RegexOptions.IgnoreCase | RegexOptions.Multiline).Count;
                                            Decksobj.availableUnits = _Count; //; SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].AvailableUnits;
                                            Decksobj.designator = "";// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].CompartmentDesignator;
                                            Decksobj.length = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Length;
                                            Decksobj.width = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Width;
                                            Decksobj.sequence = 0;// SeatGroup[x].SeatAvailabilityResponse.Equipm=entInfos[0].Compartments[i].Sequence;
                                            Decksobj.orientation = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Orientation;
                                            try
                                            {
                                                Unit compartmentsunitobj = new Unit();
                                                //doubt
                                                compartmentsunitobj.Airline = Airlines.AirIndia;
                                                if (mFacility.Groups["Availablity"].Value.Trim().ToLower() == "available")
                                                    compartmentsunitobj.assignable = true;
                                                else
                                                    compartmentsunitobj.assignable = false;
                                                //compartmentsunitobj.availability = Convert.ToInt32("1");
                                                compartmentsunitobj.compartmentDesignator = "";// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].CompartmentDesignator;
                                                compartmentsunitobj.designator = mFacility.Groups["SeatNumber"].Value.Trim();// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatDesignator;
                                                if (!string.IsNullOrEmpty(_OptionalServiceRef))
                                                {
                                                    compartmentsunitobj.servicechargefeeAmount = Convert.ToDecimal(Regex.Match(htPaidSeatPrice[_OptionalServiceRef].ToString(), @"\d+").Value);
                                                }
                                                else
                                                    compartmentsunitobj.servicechargefeeAmount = 0M;
                                                compartmentsunitobj.type = Convert.ToInt32(0);
                                                compartmentsunitobj.travelClassCode = "0";// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].TravelClassCode;
                                                compartmentsunitobj.set = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatSet;
                                                compartmentsunitobj.group = 1;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatGroup;
                                                compartmentsunitobj.priority = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Priority;
                                                compartmentsunitobj.text = "";// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Text;
                                                                              //compartmentsunitobj.setVacancy = JsonObjSeatmap.data[x].seatMap.decks["1"].compartments.Y.units[i].setVacancy;
                                                compartmentsunitobj.angle = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].SeatAngle;
                                                compartmentsunitobj.width = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Width;
                                                compartmentsunitobj.height = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Height;
                                                compartmentsunitobj.zone = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Zone;
                                                compartmentsunitobj.x = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].X;
                                                compartmentsunitobj.y = 0;// SeatGroup[x].SeatAvailabilityResponse.EquipmentInfos[0].Compartments[i].Seats[i1].Y;
                                                compartmentsunitobj.unitKey = compartmentsunitobj.designator;
                                                List<Properties> Propertieslist = new List<Properties>();
                                                foreach (Match item in Regex.Matches(mFacility.Value, @"<air:Characteristic\s*value=""(?<value>[\s\S]*?)""\s*PADISCode=""(?<Code>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                                {
                                                    Properties compartmentyproperties = new Properties();
                                                    compartmentyproperties.code = item.Groups["Code"].Value.Trim();
                                                    compartmentyproperties.value = item.Groups["value"].Value.Trim();
                                                    if (compartmentyproperties.value.Contains("PaidGeneralSeat") && (mFacility.Groups["Availablity"].Value.Trim().ToLower() == "available" && mFacility.Value.Contains("Paid=\"true\"")))
                                                    {
                                                        compartmentsunitobj.availability = Convert.ToInt32("100");
                                                    }
                                                    else if (compartmentyproperties.value.Contains("PaidGeneralSeat") && mFacility.Groups["Availablity"].Value.Trim().ToLower() == "occupied")
                                                    {
                                                        compartmentsunitobj.availability = Convert.ToInt32("10");
                                                    }
                                                    else if (!mFacility.Value.Contains("PaidGeneralSeat") && mFacility.Groups["Availablity"].Value.Trim().ToLower() == "occupied")
                                                    {
                                                        compartmentsunitobj.availability = Convert.ToInt32("5");
                                                    }
                                                    else if (!mFacility.Value.Contains("PaidGeneralSeat") && mFacility.Groups["Availablity"].Value.Trim().ToLower() == "available")
                                                    {
                                                        compartmentsunitobj.availability = Convert.ToInt32("1");
                                                    }
                                                    else if (!mFacility.Value.Contains("PaidGeneralSeat") && mFacility.Groups["Availablity"].Value.Trim().ToLower() == "noseat")
                                                    {
                                                        compartmentsunitobj.availability = Convert.ToInt32("11");
                                                    }
                                                    Propertieslist.Add(compartmentyproperties);
                                                }

                                                compartmentsunitobj.properties = Propertieslist;
                                                bool containsUnitKey = compartmentsunitlist.Any(unit => unit.unitKey == compartmentsunitobj.unitKey);
                                                if (containsUnitKey == false)
                                                {
                                                    _seatPosition = compartmentsunitobj.unitKey.Split('-')[0];
                                                    compartmentsunitlist.Add(compartmentsunitobj);
                                                }
                                                else
                                                {
                                                    continue;
                                                }

                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }

                                    }
                                    else
                                        continue;

                                    columncount0 = _seatPosition;
                                    Decksobj.units = compartmentsunitlist;
                                    Seatmapobj.SeatColumnCount = Regex.Replace(columncount0, "[^0-9]", "");
                                    Seatmapobj.decksindigo.Add(Decksobj);
                                }

                                List<Groups> GroupsFeelist = new List<Groups>();
                                int testcount = 2;
                                dataobj.seatMap = Seatmapobj;
                                dataobj.seatMapfees = Fees;
                                datalist.Add(dataobj);
                                SeatMapResponceModel.datalist = datalist;
                            }
                            #endregion
                            _SeatMapdata = new List<string>();
                            _SeatMapdata.Add("<Start>" + JsonConvert.SerializeObject(SeatMapResponceModel) + "<End>");
                            HttpContext.Session.SetString("_SeatmapData", JsonConvert.SerializeObject(SeatMapResponceModel));
                            HttpContext.Session.SetString("SeatmapRT", JsonConvert.SerializeObject(""));
                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(_SeatMapdata)))
                            {
                                if (_SeatMapdata.Count == 2)
                                {
                                    MainSeatMapdata = new List<string>();
                                }
                                MainSeatMapdata.Add(JsonConvert.SerializeObject(_SeatMapdata));
                            }
                        }
                    }
                    #endregion

                    #endregion
                    #region Meals AirAsia
                    if (_JourneykeyRTData.ToLower() == "airasia")
                    {
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "AirAsia").Result;
                        if (_journeySide == "0j")
                        {
                            token = tokenData.Token;
                        }
                        else
                        {
                            token = tokenData.RToken;
                        }

                        // string passengerdata = HttpContext.Session.GetString("keypassenger");

                        string passengerdata = objMongoHelper.UnZip(seatMealdetail.KPassenger);

                        AirAsiaTripResponceModel passeengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passengerdata, typeof(AirAsiaTripResponceModel));
                        int passengerscount = passeengerKeyList.passengerscount;

                        string departuredate = string.Empty;
                        SSRAvailabiltyModel _SSRAvailabilty = new SSRAvailabiltyModel();
                        _SSRAvailabilty.passengerKeys = new string[passengerscount];
                        for (int i = 0; i < passengerscount; i++)
                        {
                            _SSRAvailabilty.passengerKeys[i] = passeengerKeyList.passengers[i].passengerKey;
                        }
                        _SSRAvailabilty.currencyCode = _SSRAvailabilty.currencyCode;
                        if (passeengerKeyList.ErrorMsg == null)
                        {
                            List<Trip> Tripslist = new List<Trip>();
                            Trip Tripobj = new Trip();
                            Tripobj.origin = passeengerKeyList.journeys[0].designator.origin;
                            Tripobj.departureDate = passeengerKeyList.journeys[0].designator.departure.ToString();
                            List<TripIdentifier> TripIdentifierlist = new List<TripIdentifier>();
                            TripIdentifier TripIdentifierobj = new TripIdentifier();
                            TripIdentifierobj.carrierCode = passeengerKeyList.journeys[0].segments[0].identifier.carrierCode;
                            TripIdentifierobj.identifier = passeengerKeyList.journeys[0].segments[0].identifier.identifier;
                            TripIdentifierlist.Add(TripIdentifierobj);
                            Tripobj.identifier = TripIdentifierlist;
                            Tripslist.Add(Tripobj);
                            _SSRAvailabilty.trips = Tripslist;


                            var jsonSSRAvailabiltyRequest = JsonConvert.SerializeObject(_SSRAvailabilty, Formatting.Indented);
                            SSRAvailabiltyResponceModel SSRAvailabiltyResponceobj = new SSRAvailabiltyResponceModel();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            HttpResponseMessage responseSSRAvailabilty = await client.PostAsJsonAsync(AppUrlConstant.Airasiassravailability, _SSRAvailabilty);
                            if (responseSSRAvailabilty.IsSuccessStatusCode)
                            {
                                var _responseSSRAvailabilty = responseSSRAvailabilty.Content.ReadAsStringAsync().Result;
                                if (p == 0)
                                {
                                    logs.WriteLogsR(jsonSSRAvailabiltyRequest, "6-GetMealmapReq_Left", "AirAsiaRT");
                                    logs.WriteLogsR(_responseSSRAvailabilty, "6-GetMealmapRes_Left", "AirAsiaRT");
                                }
                                else
                                {
                                    logs.WriteLogsR(jsonSSRAvailabiltyRequest, "6-GetMealmapReq_Right", "AirAsiaRT");
                                    logs.WriteLogsR(_responseSSRAvailabilty, "6-GetMealmapRes_Right", "AirAsiaRT");
                                }

                                var JsonObjresponseSSRAvailabilty = JsonConvert.DeserializeObject<dynamic>(_responseSSRAvailabilty);
                                var journeyKey1 = JsonObjresponseSSRAvailabilty.data.journeySsrs[0].journeyKey;
                                int JouneyBaggage = JsonObjresponseSSRAvailabilty.data.journeySsrs.Count;
                                List<JourneyssrBaggage> journeyssrBaggagesList = new List<JourneyssrBaggage>();
                                for (int k = 0; k < JouneyBaggage; k++)
                                {
                                    JourneyssrBaggage journeyssrBaggageObj = new JourneyssrBaggage();

                                    journeyssrBaggageObj.journeyBaggageKey = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyKey;
                                    JourneyDetailsBaggage journeydetailsBaggageObj = new JourneyDetailsBaggage();

                                    journeydetailsBaggageObj.origin = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.origin;
                                    journeydetailsBaggageObj.destination = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.destination;
                                    journeydetailsBaggageObj.departureDate = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.departureDate;

                                    JBaggageIdentifier jBaggageIdentifierObj = new JBaggageIdentifier();
                                    jBaggageIdentifierObj.identifier = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.identifier.identifier;
                                    jBaggageIdentifierObj.carrierCode = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.identifier.carrierCode;
                                    journeydetailsBaggageObj.identifier = jBaggageIdentifierObj;

                                    int SSrCodeBaggageCount = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs.Count;
                                    List<BaggageSsr> baggageSsrsList = new List<BaggageSsr>();
                                    for (int l = 0; l < SSrCodeBaggageCount; l++)
                                    {
                                        BaggageSsr baggageSsrObj = new BaggageSsr();
                                        baggageSsrObj.ssrCode = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].ssrCode;
                                        baggageSsrObj.ssrType = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].ssrType;
                                        baggageSsrObj.name = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].name;
                                        baggageSsrObj.limitPerPassenger = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].limitPerPassenger;
                                        baggageSsrObj.available = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].available;
                                        baggageSsrObj.feeCode = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].feeCode;
                                        baggageSsrObj.seatRestriction = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].seatRestriction;

                                        List<PassengersAvailabilityBaggage> passengersAvailabilityBaggageList = new List<PassengersAvailabilityBaggage>();
                                        foreach (var itemObject in JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].passengersAvailability)
                                        {
                                            PassengersAvailabilityBaggage passengersAvailabilityBaggageObj = new PassengersAvailabilityBaggage();
                                            passengersAvailabilityBaggageObj.passengerKey = itemObject.Value.passengerKey;
                                            passengersAvailabilityBaggageObj.price = itemObject.Value.price;
                                            passengersAvailabilityBaggageObj.ssrKey = itemObject.Value.ssrKey;
                                            passengersAvailabilityBaggageList.Add(passengersAvailabilityBaggageObj);
                                        }
                                        baggageSsrObj.passengersAvailabilityBaggage = passengersAvailabilityBaggageList;
                                        baggageSsrsList.Add(baggageSsrObj);

                                    }
                                    journeyssrBaggageObj.journeydetailsBaggage = journeydetailsBaggageObj;
                                    journeyssrBaggageObj.baggageSsr = baggageSsrsList;
                                    journeyssrBaggagesList.Add(journeyssrBaggageObj);

                                }
                                SSRAvailabiltyResponceobj.journeySsrsBaggage = journeyssrBaggagesList;
                                //  HttpContext.Session.SetString("BaggageDetails", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));

                                seatMealdetail.Baggage = JsonConvert.SerializeObject(SSRAvailabiltyResponceobj);



                                int legSsrscount = JsonObjresponseSSRAvailabilty.data.legSsrs.Count;
                                List<legSsrs> SSRAvailabiltyLegssrlist = new List<legSsrs>();
                                int SegmentSSrcount = JsonObjresponseSSRAvailabilty.data.segmentSsrs.Count;

                                for (int i = 0; i < legSsrscount; i++)
                                {
                                    if (i <= 1 && legSsrscount > 2)
                                    {
                                        continue;
                                    }
                                    legSsrs SSRAvailabiltyLegssrobj = new legSsrs();
                                    SSRAvailabiltyLegssrobj.legKey = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legKey;
                                    legDetails legDetailsobj = new legDetails();
                                    legDetailsobj.destination = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.destination;
                                    legDetailsobj.origin = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.origin;
                                    legDetailsobj.departureDate = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.departureDate;
                                    legidentifier legidentifierobj = new legidentifier();
                                    legidentifierobj.identifier = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.identifier.identifier;
                                    legidentifierobj.carrierCode = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.identifier.carrierCode;
                                    legDetailsobj.legidentifier = legidentifierobj;

                                    var ssrscount = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs.Count;

                                    List<childlegssrs> legssrslist = new List<childlegssrs>();


                                    for (int j = 0; j < ssrscount; j++)
                                    {
                                        childlegssrs legssrs = new childlegssrs();
                                        legssrs.ssrCode = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].ssrCode;
                                        legssrs.ssrType = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].ssrType;
                                        legssrs.name = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].name;
                                        legssrs.limitPerPassenger = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].limitPerPassenger;
                                        legssrs.available = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].available;
                                        legssrs.feeCode = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].feeCode;
                                        List<legpassengers> legpassengerslist = new List<legpassengers>();

                                        foreach (var items in JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].passengersAvailability)
                                        {
                                            legpassengers passengersdetail = new legpassengers();
                                            passengersdetail.passengerKey = items.Value.passengerKey;
                                            passengersdetail.price = items.Value.price;
                                            passengersdetail.ssrKey = items.Value.ssrKey;
                                            passengersdetail.Airline = Airlines.Airasia;
                                            legpassengerslist.Add(passengersdetail);

                                        }

                                        legssrs.legpassengers = legpassengerslist;
                                        legssrslist.Add(legssrs);
                                    }
                                    SSRAvailabiltyLegssrobj.legDetails = legDetailsobj;
                                    SSRAvailabiltyLegssrobj.legssrs = legssrslist;
                                    SSRAvailabiltyLegssrlist.Add(SSRAvailabiltyLegssrobj);

                                }
                                SSRAvailabiltyResponceobj.legSsrs = SSRAvailabiltyLegssrlist;
                                SSRAvailabiltyResponceobj.SegmentSSrcount = SegmentSSrcount;
                                _Mealsdata.Add("<Start>" + JsonConvert.SerializeObject(SSRAvailabiltyResponceobj) + "<End>");
                                //   HttpContext.Session.SetString("Meals", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                                //   HttpContext.Session.SetString("_MealsData", JsonConvert.SerializeObject(_Mealsdata));
                                seatMealdetail.Meals = objMongoHelper.Zip(JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                                if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(_Mealsdata)))
                                {
                                    if (_Mealsdata.Count == 2)
                                    {
                                        MainMealsdata = new List<string>();
                                    }
                                    MainMealsdata.Add(JsonConvert.SerializeObject(_Mealsdata));
                                }
                            }
                        }
                    }
                    #endregion

                    // Meal SSR Akasa  Air***********
                    #region Meals AkasaAir
                    if (_JourneykeyRTData.ToLower() == "akasaair")
                    {
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Akasa").Result;

                        if (_journeySide == "0j")
                        {
                            token = tokenData.Token;
                        }
                        else
                        {
                            token = tokenData.RToken;
                        }

                        //string passengerdata = HttpContext.Session.GetString("keypassenger");

                        string passengerdata = objMongoHelper.UnZip(seatMealdetail.KPassenger);

                        AirAsiaTripResponceModel AKpasseengerKeyList = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passengerdata, typeof(AirAsiaTripResponceModel));
                        int passengerscount = AKpasseengerKeyList.passengerscount;
                        AkasaSSRavailRequest _AkasaSSRAvailabilty = new AkasaSSRavailRequest();
                        _AkasaSSRAvailabilty.passengerKeys = new string[passengerscount];

                        for (int i = 0; i < passengerscount; i++)
                        {
                            _AkasaSSRAvailabilty.passengerKeys[i] = AKpasseengerKeyList.passengers[i].passengerKey;
                        }

                        _AkasaSSRAvailabilty.currencyCode = "INR"; // Ensure currency code is assigned properly

                        List<TripAA> AkasaTripslist = new List<TripAA>();
                        if (AKpasseengerKeyList.ErrorMsg == null)
                        {
                            int segsmealBagcount = AKpasseengerKeyList.journeys[0].segments.Count;

                            for (int i = 0; i < segsmealBagcount; i++)
                            {
                                TripAA AkasaTripobj = new TripAA();

                                TripIdentifier AkasaTripIdentifierobj = new TripIdentifier
                                {
                                    carrierCode = AKpasseengerKeyList.journeys[0].segments[i].identifier.carrierCode,
                                    identifier = AKpasseengerKeyList.journeys[0].segments[i].identifier.identifier
                                };

                                AkasaTripobj.origin = AKpasseengerKeyList.journeys[0].segments[i].designator.origin;
                                AkasaTripobj.destination = AKpasseengerKeyList.journeys[0].segments[i].designator.destination;
                                AkasaTripobj.departureDate = AKpasseengerKeyList.journeys[0].designator.departure.ToString("yyyy-MM-dd");
                                AkasaTripobj.identifier = AkasaTripIdentifierobj; // ✅ Assign as an object, NOT a list

                                AkasaTripslist.Add(AkasaTripobj);
                            }

                            _AkasaSSRAvailabilty.trips = AkasaTripslist;

                            var jsonAkasaSSRAvailabiltyRequest = JsonConvert.SerializeObject(_AkasaSSRAvailabilty, Formatting.Indented);

                            SSRAvailabiltyResponceModel SSRAvailabiltyResponceobj = new SSRAvailabiltyResponceModel();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            HttpResponseMessage responseSSRAvailabilty = await client.PostAsJsonAsync(AppUrlConstant.URLAkasaAir + "/api/nsk/v2/booking/ssrs/availability", _AkasaSSRAvailabilty);
                            if (responseSSRAvailabilty.IsSuccessStatusCode)
                            {
                                var _responseSSRAvailabilty = responseSSRAvailabilty.Content.ReadAsStringAsync().Result;
                                if (p == 0)
                                {
                                    logs.WriteLogsR(jsonAkasaSSRAvailabiltyRequest, "6-GetMealmapReq_Left", "AkasaRT");
                                    logs.WriteLogsR(_responseSSRAvailabilty, "6-GetMealmapRes_Left", "AkasaRT");
                                }
                                else
                                {
                                    logs.WriteLogsR(jsonAkasaSSRAvailabiltyRequest, "6-GetMealmapReq_Right", "AkasaRT");
                                    logs.WriteLogsR(_responseSSRAvailabilty, "6-GetMealmapRes_Right", "AkasaRT");
                                }

                                var JsonObjresponseSSRAvailabilty = JsonConvert.DeserializeObject<dynamic>(_responseSSRAvailabilty);
                                var journeyKey1 = JsonObjresponseSSRAvailabilty.data.journeySsrs[0].journeyKey;
                                int JouneyBaggage = JsonObjresponseSSRAvailabilty.data.journeySsrs.Count;
                                List<JourneyssrBaggage> journeyssrBaggagesList = new List<JourneyssrBaggage>();
                                for (int k = 0; k < JouneyBaggage; k++)
                                {
                                    JourneyssrBaggage journeyssrBaggageObj = new JourneyssrBaggage();
                                    journeyssrBaggageObj.journeyBaggageKey = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyKey;
                                    JourneyDetailsBaggage journeydetailsBaggageObj = new JourneyDetailsBaggage();
                                    journeydetailsBaggageObj.origin = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.origin;
                                    journeydetailsBaggageObj.destination = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.destination;
                                    journeydetailsBaggageObj.departureDate = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.departureDate;
                                    JBaggageIdentifier jBaggageIdentifierObj = new JBaggageIdentifier();
                                    jBaggageIdentifierObj.identifier = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.identifier.identifier;
                                    jBaggageIdentifierObj.carrierCode = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].journeyDetails.identifier.carrierCode;
                                    journeydetailsBaggageObj.identifier = jBaggageIdentifierObj;
                                    int SSrCodeBaggageCount = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs.Count;
                                    List<BaggageSsr> baggageSsrsList = new List<BaggageSsr>();
                                    for (int l = 0; l < SSrCodeBaggageCount; l++)
                                    {
                                        BaggageSsr baggageSsrObj = new BaggageSsr();
                                        baggageSsrObj.ssrCode = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].ssrCode;
                                        baggageSsrObj.ssrType = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].ssrType;
                                        baggageSsrObj.name = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].name;
                                        baggageSsrObj.limitPerPassenger = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].limitPerPassenger;
                                        baggageSsrObj.available = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].available;
                                        baggageSsrObj.feeCode = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].feeCode;
                                        baggageSsrObj.seatRestriction = JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].seatRestriction;
                                        List<PassengersAvailabilityBaggage> passengersAvailabilityBaggageList = new List<PassengersAvailabilityBaggage>();
                                        foreach (var itemObject in JsonObjresponseSSRAvailabilty.data.journeySsrs[k].ssrs[l].passengersAvailability)
                                        {
                                            PassengersAvailabilityBaggage passengersAvailabilityBaggageObj = new PassengersAvailabilityBaggage();
                                            passengersAvailabilityBaggageObj.passengerKey = itemObject.Value.passengerKey;
                                            passengersAvailabilityBaggageObj.price = itemObject.Value.price;
                                            passengersAvailabilityBaggageObj.ssrKey = itemObject.Value.ssrKey;
                                            passengersAvailabilityBaggageObj.Airline = Airlines.AkasaAir;
                                            passengersAvailabilityBaggageList.Add(passengersAvailabilityBaggageObj);
                                        }
                                        baggageSsrObj.passengersAvailabilityBaggage = passengersAvailabilityBaggageList;
                                        baggageSsrsList.Add(baggageSsrObj);

                                    }
                                    journeyssrBaggageObj.journeydetailsBaggage = journeydetailsBaggageObj;
                                    journeyssrBaggageObj.baggageSsr = baggageSsrsList;
                                    journeyssrBaggagesList.Add(journeyssrBaggageObj);

                                }
                                SSRAvailabiltyResponceobj.journeySsrsBaggage = journeyssrBaggagesList;
                                // HttpContext.Session.SetString("BaggageDetails", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));

                                seatMealdetail.Baggage = JsonConvert.SerializeObject(SSRAvailabiltyResponceobj);

                                int legSsrscount = JsonObjresponseSSRAvailabilty.data.legSsrs.Count;
                                List<legSsrs> SSRAvailabiltyLegssrlist = new List<legSsrs>();
                                int SegmentSSrcount = JsonObjresponseSSRAvailabilty.data.segmentSsrs.Count;

                                for (int i = 0; i < legSsrscount; i++)
                                {
                                    if (i <= 1 && legSsrscount > 2)
                                    {
                                        continue;
                                    }
                                    legSsrs SSRAvailabiltyLegssrobj = new legSsrs();
                                    SSRAvailabiltyLegssrobj.legKey = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legKey;
                                    legDetails legDetailsobj = new legDetails();
                                    legDetailsobj.destination = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.destination;
                                    legDetailsobj.origin = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.origin;
                                    legDetailsobj.departureDate = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.departureDate;
                                    legidentifier legidentifierobj = new legidentifier();
                                    legidentifierobj.identifier = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.identifier.identifier;
                                    legidentifierobj.carrierCode = JsonObjresponseSSRAvailabilty.data.legSsrs[i].legDetails.identifier.carrierCode;
                                    legDetailsobj.legidentifier = legidentifierobj;

                                    var ssrscount = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs.Count;

                                    List<childlegssrs> legssrslist = new List<childlegssrs>();


                                    for (int j = 0; j < ssrscount; j++)
                                    {
                                        childlegssrs legssrs = new childlegssrs();
                                        legssrs.ssrCode = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].ssrCode;
                                        legssrs.ssrType = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].ssrType;
                                        legssrs.name = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].name;
                                        legssrs.limitPerPassenger = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].limitPerPassenger;
                                        legssrs.available = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].available;
                                        legssrs.feeCode = JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].feeCode;
                                        List<legpassengers> legpassengerslist = new List<legpassengers>();

                                        foreach (var items in JsonObjresponseSSRAvailabilty.data.legSsrs[i].ssrs[j].passengersAvailability)
                                        {
                                            legpassengers passengersdetail = new legpassengers();
                                            passengersdetail.passengerKey = items.Value.passengerKey;
                                            passengersdetail.price = items.Value.price;
                                            passengersdetail.ssrKey = items.Value.ssrKey;
                                            passengersdetail.Airline = Airlines.AkasaAir;
                                            legpassengerslist.Add(passengersdetail);

                                        }

                                        legssrs.legpassengers = legpassengerslist;
                                        legssrslist.Add(legssrs);
                                    }
                                    SSRAvailabiltyLegssrobj.legDetails = legDetailsobj;
                                    SSRAvailabiltyLegssrobj.legssrs = legssrslist;
                                    SSRAvailabiltyLegssrlist.Add(SSRAvailabiltyLegssrobj);

                                }
                                SSRAvailabiltyResponceobj.legSsrs = SSRAvailabiltyLegssrlist;
                                SSRAvailabiltyResponceobj.SegmentSSrcount = SegmentSSrcount;
                                _Mealsdata.Add("<Start>" + JsonConvert.SerializeObject(SSRAvailabiltyResponceobj) + "<End>");
                                //HttpContext.Session.SetString("Meals", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                                //HttpContext.Session.SetString("_MealsData", JsonConvert.SerializeObject(_Mealsdata));
                                seatMealdetail.Meals = objMongoHelper.Zip(JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                                if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(_Mealsdata)))
                                {
                                    if (_Mealsdata.Count == 2)
                                    {
                                        MainMealsdata = new List<string>();
                                    }
                                    MainMealsdata.Add(JsonConvert.SerializeObject(_Mealsdata));
                                }
                            }
                        }
                    }
                    #endregion

                    //Spicejet Roundtrip SSR
                    #region ssravailability
                    if (_JourneykeyRTData.ToLower() == "spicejet")
                    {
                        if (string.IsNullOrEmpty(AirAsiaTripResponceobj.ErrorMsg))
                        {
                            tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "SpiceJet").Result;
                            Signature = string.Empty;
                            if (_journeySide == "0j")
                            {
                                Signature = tokenData.Token; // HttpContext.Session.GetString("SpicejetSignature");
                            }
                            else
                            {
                                Signature = tokenData.RToken; //HttpContext.Session.GetString("SpicejetSignatureR");
                            }
                            if (Signature == null) { Signature = ""; }
                            Signature = Signature.Replace(@"""", string.Empty);
                            List<legSsrs> SSRAvailabiltyLegssrlist = new List<legSsrs>();
                            SSRAvailabiltyResponceModel SSRAvailabiltyResponceobj = null;
                            AirAsiaTripResponceModel passeengerlist = null;
                            // string passenger = HttpContext.Session.GetString("SGkeypassengerRT");

                            string passenger = seatMealdetail.Infant;
                            passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                            GetSSRAvailabilityForBookingRequest _req = new GetSSRAvailabilityForBookingRequest();
                            GetSSRAvailabilityForBookingResponse _res = new GetSSRAvailabilityForBookingResponse();
                            try
                            {
                                int segmentcount = 0;
                                int journeyscount = passeengerlist.journeys.Count;
                                _req.Signature = Signature;
                                _req.ContractVersion = 420;
                                SSRAvailabilityForBookingRequest _SSRAvailabilityForBookingRequest = new SSRAvailabilityForBookingRequest();
                                for (int i = 0; i < journeyscount; i++)
                                {
                                    int segmentscount = passeengerlist.journeys[i].segments.Count;
                                    _SSRAvailabilityForBookingRequest.SegmentKeyList = new LegKey[segmentscount];
                                    for (int j = 0; j < segmentscount; j++)
                                    {
                                        int legcount = passeengerlist.journeys[i].segments[j].legs.Count;
                                        for (int n = 0; n < legcount; n++)
                                        {
                                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j] = new LegKey();
                                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].CarrierCode = passeengerlist.journeys[i].segments[j].identifier.carrierCode;
                                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].FlightNumber = passeengerlist.journeys[i].segments[j].identifier.identifier;
                                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].DepartureDateSpecified = true;
                                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].DepartureDate = Convert.ToDateTime(AirAsiaTripResponceobj.journeys[i].segments[j].designator.departure);//DateTime.ParseExact(strdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].ArrivalStation = AirAsiaTripResponceobj.journeys[i].segments[j].designator.destination;
                                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].DepartureStation = AirAsiaTripResponceobj.journeys[i].segments[j].designator.origin;
                                            segmentcount++;
                                        }
                                    }
                                }
                                _SSRAvailabilityForBookingRequest.PassengerNumberList = new short[Convert.ToInt16(TotalCount)];//new short[1];
                                int paxCount = _SSRAvailabilityForBookingRequest.PassengerNumberList.Length;//passeengerlist.passengerscount;
                                for (int i = 0; i < paxCount; i++)
                                {
                                    if (i > 0)
                                        continue;
                                    _SSRAvailabilityForBookingRequest.PassengerNumberList[i] = Convert.ToInt16(i);
                                }
                                _SSRAvailabilityForBookingRequest.InventoryControlled = true;
                                _SSRAvailabilityForBookingRequest.InventoryControlledSpecified = true;
                                _SSRAvailabilityForBookingRequest.NonInventoryControlled = true;
                                _SSRAvailabilityForBookingRequest.NonInventoryControlledSpecified = true;
                                _SSRAvailabilityForBookingRequest.SeatDependent = true;
                                _SSRAvailabilityForBookingRequest.SeatDependentSpecified = true;
                                _SSRAvailabilityForBookingRequest.NonSeatDependent = true;
                                _SSRAvailabilityForBookingRequest.NonSeatDependentSpecified = true;
                                _SSRAvailabilityForBookingRequest.CurrencyCode = "INR";
                                _SSRAvailabilityForBookingRequest.SSRAvailabilityMode = SSRAvailabilityMode.NonBundledSSRs;
                                _SSRAvailabilityForBookingRequest.SSRAvailabilityModeSpecified = true;
                                _req.SSRAvailabilityForBookingRequest = _SSRAvailabilityForBookingRequest;
                                objSpiceJet = new SpiceJetApiController();
                                _res = await objSpiceJet.GetSSRAvailabilityForBooking(_req);

                                string Str2 = JsonConvert.SerializeObject(_res);

                                if (p == 0)
                                {
                                    logs.WriteLogsR(Str2.ToString(), "9-GetSSRAvailabilityForBookingReq_Left", "SpiceJetRT");
                                    logs.WriteLogsR(Str2.ToString(), "9-GetSSRAvailabilityForBookingRes_Left", "SpiceJetRT");
                                }
                                else
                                {
                                    logs.WriteLogsR(Str2.ToString(), "8-GetSSRAvailabilityForBookingReq_Right", "SpiceJetRT");
                                    logs.WriteLogsR(Str2.ToString(), "8-GetSSRAvailabilityForBookingRes_Right", "SpiceJetRT");
                                }
                                //******Vinay***********//
                                if (_res != null)
                                {
                                    Hashtable htSSr = new Hashtable();
                                    SpicejetMealImageList.GetAllmealSG(htSSr);
                                    SSRAvailabiltyLegssrlist = new List<legSsrs>();

                                    SSRAvailabiltyResponceobj = new SSRAvailabiltyResponceModel();
                                    try
                                    {
                                        legSsrs SSRAvailabiltyLegssrobj = new legSsrs();
                                        legDetails legDetailsobj = null;
                                        List<childlegssrs> legssrslist = new List<childlegssrs>();
                                        for (int i1 = 0; i1 < _res.SSRAvailabilityForBookingResponse.SSRSegmentList.Length; i1++)
                                        {
                                            legssrslist = new List<childlegssrs>();
                                            for (int j = 0; j < _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList.Length; j++)
                                            {
                                                int legSsrscount = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRLegList.Length;
                                                try
                                                {
                                                    SSRAvailabiltyLegssrobj = new legSsrs();
                                                    SSRAvailabiltyLegssrobj.legKey = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.ToString();
                                                    legDetailsobj = new legDetails();
                                                    legDetailsobj.destination = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.ArrivalStation;
                                                    legDetailsobj.origin = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.DepartureStation;
                                                    legDetailsobj.departureDate = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.DepartureDate.ToString();
                                                    legidentifier legidentifierobj = new legidentifier();
                                                    legidentifierobj.identifier = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.FlightNumber;
                                                    legidentifierobj.carrierCode = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.CarrierCode;
                                                    legDetailsobj.legidentifier = legidentifierobj;
                                                    childlegssrs legssrs = new childlegssrs();
                                                    legssrs.ssrCode = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRCode.ToString();
                                                    if (htSSr[legssrs.ssrCode] != null)
                                                    {
                                                        legssrs.name = htSSr[legssrs.ssrCode].ToString();
                                                    }
                                                    else
                                                        continue;

                                                    legssrs.ssrCode = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRCode.ToString();
                                                    legssrs.available = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].Available;
                                                    if (_res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList.Length > 0)
                                                    {
                                                        legssrs.feeCode = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList[0].PaxFee.FeeCode;
                                                        List<legpassengers> legpassengerslist = new List<legpassengers>();
                                                        decimal Amount = decimal.Zero;
                                                        legpassengers passengersdetail = new legpassengers();
                                                        int i2 = 0;
                                                        foreach (var items in _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList[0].PaxFee.ServiceCharges)
                                                        {
                                                            if (i2 > 0)
                                                            {
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                Amount += items.Amount;
                                                                passengersdetail.price = Math.Round(Amount).ToString(); //Ammount
                                                            }
                                                            i2++;
                                                        }
                                                        passengersdetail.passengerKey = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList[0].PassengerNumberList.ToString();
                                                        passengersdetail.ssrKey = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRCode;
                                                        passengersdetail.Airline = Airlines.Spicejet;
                                                        legpassengerslist.Add(passengersdetail);
                                                        legssrs.legpassengers = legpassengerslist;
                                                        legssrslist.Add(legssrs);
                                                    }

                                                }
                                                catch (Exception ex)
                                                {

                                                }
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
                                    Mealsdata = new List<string>();
                                    Mealsdata.Add("<Start>" + JsonConvert.SerializeObject(SSRAvailabiltyResponceobj) + "<End>");
                                    HttpContext.Session.SetString("SGMealsRT", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                                    HttpContext.Session.SetString("MealsData", JsonConvert.SerializeObject(Mealsdata));
                                    if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Mealsdata)))
                                    {
                                        if (Mealsdata.Count == 2)
                                        {
                                            MainMealsdata = new List<string>();
                                        }
                                        MainMealsdata.Add(JsonConvert.SerializeObject(Mealsdata));
                                    }

                                }
                                else
                                {
                                    SSRAvailabiltyResponceobj = new SSRAvailabiltyResponceModel();
                                    Mealsdata = new List<string>();
                                    Mealsdata.Add("<Start>" + JsonConvert.SerializeObject(SSRAvailabiltyResponceobj) + "<End>");
                                    HttpContext.Session.SetString("SGMealsRT", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                                    HttpContext.Session.SetString("MealsData", JsonConvert.SerializeObject(Mealsdata));
                                    if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Mealsdata)))
                                    {
                                        if (Mealsdata.Count == 2)
                                        {
                                            MainMealsdata = new List<string>();
                                        }
                                        MainMealsdata.Add(JsonConvert.SerializeObject(Mealsdata));
                                    }

                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                    #endregion

                    //Indigo Roundtrip SSR
                    #region Indigo ssravailability
                    if (_JourneykeyRTData.ToLower() == "indigo")
                    {
                        Signature = string.Empty;
                        tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "Indigo").Result;
                        if (_journeySide == "0j")
                        {
                            Signature = tokenData.Token;
                        }
                        else
                        {
                            Signature = tokenData.RToken;
                        }

                        List<legSsrs> SSRAvailabiltyLegssrlist = new List<legSsrs>();
                        SSRAvailabiltyResponceModel SSRAvailabiltyResponceobj = null;
                        AirAsiaTripResponceModel passeengerlist = null;
                        // string passenger = HttpContext.Session.GetString("SGkeypassengerRT");

                        string passenger = seatMealdetail.Infant;
                        passeengerlist = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(passenger, typeof(AirAsiaTripResponceModel));
                        _GetSSR objssr = new _GetSSR();
                        IndigoBookingManager_.GetSSRAvailabilityForBookingResponse _res = await objssr.GetSSRAvailabilityForBooking(Signature, passeengerlist, TotalCount, _journeySide, "");
                        if (_res != null)
                        {
                            Hashtable htSSr = new Hashtable();
                            SpicejetMealImageList.GetAllmeal(htSSr);
                            SSRAvailabiltyLegssrlist = new List<legSsrs>();
                            SSRAvailabiltyResponceobj = new SSRAvailabiltyResponceModel();
                            try
                            {
                                legSsrs SSRAvailabiltyLegssrobj = new legSsrs();
                                legDetails legDetailsobj = null;
                                List<childlegssrs> legssrslist = new List<childlegssrs>();
                                for (int i1 = 0; i1 < _res.SSRAvailabilityForBookingResponse.SSRSegmentList.Length; i1++)
                                {
                                    legssrslist = new List<childlegssrs>();
                                    for (int j = 0; j < _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList.Length; j++)
                                    {
                                        int legSsrscount = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRLegList.Length;
                                        try
                                        {
                                            SSRAvailabiltyLegssrobj = new legSsrs();
                                            SSRAvailabiltyLegssrobj.legKey = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.ToString();
                                            legDetailsobj = new legDetails();
                                            legDetailsobj.destination = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.ArrivalStation;
                                            legDetailsobj.origin = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.DepartureStation;
                                            legDetailsobj.departureDate = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.DepartureDate.ToString();
                                            legidentifier legidentifierobj = new legidentifier();
                                            legidentifierobj.identifier = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.FlightNumber;
                                            legidentifierobj.carrierCode = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].LegKey.CarrierCode;
                                            legDetailsobj.legidentifier = legidentifierobj;

                                            childlegssrs legssrs = new childlegssrs();
                                            legssrs.ssrCode = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRCode.ToString();
                                            if (htSSr[legssrs.ssrCode] != null)
                                            {
                                                legssrs.name = htSSr[legssrs.ssrCode].ToString();
                                            }
                                            else
                                                continue;

                                            legssrs.ssrCode = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRCode.ToString();
                                            legssrs.available = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].Available;
                                            if (_res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList.Length > 0)
                                            {
                                                legssrs.feeCode = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList[0].PaxFee.FeeCode;
                                                List<legpassengers> legpassengerslist = new List<legpassengers>();
                                                decimal Amount = decimal.Zero;
                                                legpassengers passengersdetail = new legpassengers();
                                                int i2 = 0;
                                                foreach (var items in _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList[0].PaxFee.ServiceCharges)
                                                {
                                                    if (i2 > 0)
                                                    {
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        Amount += items.Amount;
                                                        passengersdetail.price = Math.Round(Amount).ToString(); //Ammount
                                                    }
                                                    i2++;
                                                }
                                                passengersdetail.passengerKey = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList[0].PassengerNumberList.ToString();
                                                passengersdetail.ssrKey = _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRCode;
                                                passengersdetail.Airline = Airlines.Indigo;
                                                legpassengerslist.Add(passengersdetail);
                                                legssrs.legpassengers = legpassengerslist;
                                                legssrslist.Add(legssrs);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                        }
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
                            Mealsdata = new List<string>();
                            Mealsdata.Add("<Start>" + JsonConvert.SerializeObject(SSRAvailabiltyResponceobj) + "<End>");
                            HttpContext.Session.SetString("SGMealsRT", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                            HttpContext.Session.SetString("MealsData", JsonConvert.SerializeObject(Mealsdata));
                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Mealsdata)))
                            {
                                if (Mealsdata.Count == 2)
                                {
                                    MainMealsdata = new List<string>();
                                }
                                MainMealsdata.Add(JsonConvert.SerializeObject(Mealsdata));
                            }
                        }
                    }
                    #endregion
                    #region SSR GDS
                    if (_JourneykeyRTData.ToLower() == "vistara" || _JourneykeyRTData.ToLower() == "airindia" || _JourneykeyRTData.ToLower() == "hehnair")
                    {
                        #region ssravailability
                        _objAvail = null;

                        httpContextAccessorInstance = new HttpContextAccessor();
                        _objAvail = new TravelPort(httpContextAccessorInstance);
                        _testURL = AppUrlConstant.GDSSSRURL;
                        _targetBranch = "P7027135";
                        _userName = "Universal API/uAPI5098257106-beb65aec";
                        _password = "Q!f5-d7A3D";
                        StringBuilder SSRReq = new StringBuilder();
                        string res = _objAvail.AirSSRGet(_testURL, SSRReq, "SsrType", newGuid.ToString(), _targetBranch, _userName, _password, p, "");
                        AirAsiaTripResponceModel passeengerlist = null;
                        if (res != null)
                        {
                            Hashtable htSSr = new Hashtable();
                            foreach (Match item in Regex.Matches(res, "SsrType Code=\"(?<Code>[\\s\\S]*?)\"\\s*Description=\"(?<value>[\\s\\S]*?)\""))
                            {
                                if (!htSSr.Contains(item.Groups["Code"].Value))
                                {
                                    htSSr.Add(item.Groups["Code"].Value, item.Groups["value"].Value.Trim());
                                }

                            }
                            List<legSsrs> SSRAvailabiltyLegssrlist = new List<legSsrs>();
                            SSRAvailabiltyResponceModel SSRAvailabiltyResponceobj = new SSRAvailabiltyResponceModel();
                            try
                            {
                                legSsrs SSRAvailabiltyLegssrobj = new legSsrs();
                                legDetails legDetailsobj = null;
                                List<childlegssrs> legssrslist = new List<childlegssrs>();
                                int matchCount = Regex.Matches(SeatMapres, @"<air:AirSegment[\s\S]*?</air:AirSegment>", RegexOptions.IgnoreCase | RegexOptions.Multiline).Count;
                                if (matchCount == 0)
                                    matchCount = 1;
                                for (int i = 0; i < matchCount; i++)
                                {
                                    legssrslist = new List<childlegssrs>();
                                    foreach (DictionaryEntry entry in htSSr)
                                    {

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
                                            if (htSSr[legssrs.ssrCode] != null)
                                            {
                                                legssrs.name = htSSr[legssrs.ssrCode].ToString();
                                            }
                                            else
                                                continue;

                                            legssrs.available = 0;// _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].Available;
                                            List<legpassengers> legpassengerslist = new List<legpassengers>();
                                            Decimal Amount = decimal.Zero;
                                            legpassengers passengersdetail = new legpassengers();


                                            passengersdetail.passengerKey = "";// _res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].PaxSSRPriceList[0].PassengerNumberList.ToString();
                                            passengersdetail.ssrKey = ""; //_res.SSRAvailabilityForBookingResponse.SSRSegmentList[i1].AvailablePaxSSRList[j].SSRCode;
                                            passengersdetail.price = "0";
                                            passengersdetail.Airline = Airlines.AirIndia;
                                            legpassengerslist.Add(passengersdetail);
                                            legssrs.legpassengers = legpassengerslist;
                                            legssrslist.Add(legssrs);
                                        }
                                        catch (Exception ex)
                                        {

                                        }

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
                            #endregion
                            Mealsdata = new List<string>();
                            Mealsdata.Add("<Start>" + JsonConvert.SerializeObject(SSRAvailabiltyResponceobj) + "<End>");
                            HttpContext.Session.SetString("SGMealsRT", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                            //  HttpContext.Session.SetString("MealsData", JsonConvert.SerializeObject(Mealsdata));
                            seatMealdetail.Meals = objMongoHelper.Zip(JsonConvert.SerializeObject(Mealsdata));
                            if (!string.IsNullOrEmpty(JsonConvert.SerializeObject(Mealsdata)))
                            {
                                if (Mealsdata.Count == 2)
                                {
                                    MainMealsdata = new List<string>();
                                }
                                MainMealsdata.Add(JsonConvert.SerializeObject(Mealsdata));
                            }
                        }
                    }
                    #endregion
                }
            }

            HttpContext.Session.SetString("SelectedAirlineName", JsonConvert.SerializeObject(AirlineNamedesc));
            HttpContext.Session.SetString("AirlineSelectedRT", JsonConvert.SerializeObject(airlinenameforcommit));
            //HttpContext.Session.SetString("Mainpassengervm", JsonConvert.SerializeObject(MainPassengerdata));
            //HttpContext.Session.SetString("Mainseatmapvm", JsonConvert.SerializeObject(MainSeatMapdata));
            //HttpContext.Session.SetString("Mainmealvm", JsonConvert.SerializeObject(MainMealsdata));




            seatMealdetail.SeatMap = objMongoHelper.Zip(JsonConvert.SerializeObject(MainSeatMapdata));
            seatMealdetail.MainMeals = objMongoHelper.Zip(JsonConvert.SerializeObject(MainMealsdata));
            seatMealdetail.ResultRequest = objMongoHelper.Zip(JsonConvert.SerializeObject(MainPassengerdata));

            seatMealdetail.Guid = Guid;
            _mongoDBHelper.SaveResultSeatMealRequest(seatMealdetail);

            _mongoDBHelper.UpdateSuppLegalEntity(Guid, airlineId);


            return RedirectToAction("RoundAATripsellView", "RoundAATripsell", new { Guid = Guid, Supp = Supp });
        }

        PaxPriceType[] getPaxdetails(int adult_, int child_, int infant_)
        {
            PaxPriceType[] paxPriceTypes = null;
            try
            {
                int i = 0;
                if (adult_ > 0) i++;
                if (child_ > 0) i++;
                if (infant_ > 0) i++;

                paxPriceTypes = new PaxPriceType[i];
                int j = 0;
                if (adult_ > 0)
                {
                    paxPriceTypes[j] = new PaxPriceType();
                    paxPriceTypes[j].PaxType = "ADT";
                    paxPriceTypes[j].PaxCountSpecified = true;
                    paxPriceTypes[j].PaxCount = Convert.ToInt16(adult_);
                    j++;
                }

                if (child_ > 0)
                {
                    paxPriceTypes[j] = new PaxPriceType();
                    paxPriceTypes[j].PaxType = "CHD";
                    paxPriceTypes[j].PaxCountSpecified = true;
                    paxPriceTypes[j].PaxCount = Convert.ToInt16(child_);
                    j++;
                }

                if (infant_ > 0)
                {
                    paxPriceTypes[j] = new PaxPriceType();
                    paxPriceTypes[j].PaxType = "INFT";
                    paxPriceTypes[j].PaxCountSpecified = true;
                    paxPriceTypes[j].PaxCount = Convert.ToInt16(infant_);
                    j++;
                }
            }
            catch (Exception e)
            {
            }
            return paxPriceTypes;
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
