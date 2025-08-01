using static DomainLayer.Model.SeatMapResponceModel;
using System.Text.RegularExpressions;
using Utility;
using IndigoBookingManager_;
using System.Collections;
using DomainLayer.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnionArchitectureAPI.Services.Travelport;
using OnionConsumeWebAPI.Extensions;
using System.Text;
using static DomainLayer.Model.GDSResModel;
using OnionConsumeWebAPI.Models;

namespace OnionConsumeWebAPI.Controllers.TravelClick
{
    public class GDSResultFlightViewController : Controller
    {
        Logs logs = new Logs();
        private readonly IConfiguration _configuration;

        public GDSResultFlightViewController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        PaxPriceType[] getPaxdetails(int adult_, int child_, int infant_)
        {
            PaxPriceType[] paxPriceTypes = null;
            try
            {
                int idx = 0;
                if (adult_ > 0) idx++;
                if (child_ > 0) idx++;
                if (infant_ > 0) idx++;

                paxPriceTypes = new PaxPriceType[idx];

                int arrCount = 0;
                for (int cntAdt = 0; cntAdt < adult_; cntAdt++)
                {
                    if (cntAdt > 0) continue;
                    paxPriceTypes[arrCount] = new PaxPriceType();
                    paxPriceTypes[arrCount].PaxType = "ADT";
                    paxPriceTypes[arrCount].PaxCountSpecified = true;
                    paxPriceTypes[arrCount].PaxCount = Convert.ToInt16(adult_);
                    // paxPriceTypes[arrCount].PaxDiscountCode = "true";
                    arrCount++;
                }
                for (int cntChd = 0; cntChd < child_; cntChd++)
                {
                    if (cntChd > 0) continue;
                    paxPriceTypes[arrCount] = new PaxPriceType();
                    paxPriceTypes[arrCount].PaxType = "CHD";
                    paxPriceTypes[arrCount].PaxCountSpecified = true;
                    paxPriceTypes[arrCount].PaxCount = Convert.ToInt16(child_);
                    // paxPriceTypes[arrCount].PaxDiscountCode = "true";
                    arrCount++;
                }
                for (int cntInf = 0; cntInf < infant_; cntInf++)
                {
                    paxPriceTypes[arrCount] = new PaxPriceType();
                    paxPriceTypes[arrCount].PaxType = "INFT";
                    arrCount++;
                }
            }
            catch (Exception e)
            {
            }

            return paxPriceTypes;
        }
        public PaxPriceType[] getPaxdetails()
        {
            PaxPriceType[] paxPriceTypes = null;
            try
            {
                paxPriceTypes = new PaxPriceType[1];
                paxPriceTypes[0] = new PaxPriceType();
                paxPriceTypes[0].PaxType = "ADT";
            }
            catch { }
            return paxPriceTypes;
        }

        [HttpPost] // this APi is used to map trip data Amount
        public async Task<ActionResult> GDSTripsell(string fareKey, string journeyKey, string Guid)
        {
            AAIdentifier AAIdentifierobj = null;
            string _targetBranch = string.Empty;
            string _userName = string.Empty;
            string _password = string.Empty;
            dynamic Airfaredata = null;
            if (!string.IsNullOrEmpty(fareKey))
            {
                Airfaredata = JsonConvert.DeserializeObject<dynamic>(fareKey);
            }
            using (HttpClient client = new HttpClient())
            {
                MongoHelper objMongoHelper = new MongoHelper();
                MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
                MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
                SearchLog searchLog = new SearchLog();
                searchLog = _mongoDBHelper.GetFlightSearchLog(Guid).Result;
                tokenData = _mongoDBHelper.GetSuppFlightTokenByGUID(Guid, "GDS").Result;
                int adultcount = searchLog.Adults;
                int childcount = searchLog.Children;
                int infantcount = searchLog.Infants;
                int TotalCount = adultcount + childcount;
                string str3 = string.Empty;
                string newGuid = tokenData.Token;
                if (newGuid == "" || newGuid == null)
                {
                    return RedirectToAction("Index");
                }
                var jsonData = objMongoHelper.UnZip(tokenData.PassRequest);
                SimpleAvailabilityRequestModel availibiltyRQGDS = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleAvailabilityRequestModel>(jsonData);
                #region GDSAirPricelRequest
                TravelPort _objAvail = null;
                HttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();
                _objAvail = new TravelPort(httpContextAccessorInstance);
                string _testURL = AppUrlConstant.GDSURL;
                _targetBranch = "P7027135";
                _userName = "Universal API/uAPI5098257106-beb65aec";
                _password = "Q!f5-d7A3D";
                StringBuilder fareRepriceReq = new StringBuilder();
                SimpleAvailibilityaAddResponce AirfaredataL = null;
                string[] _data = fareKey.ToString().Split("@0f");
                if (!string.IsNullOrEmpty(_data[0]))
                {
                    AirfaredataL = JsonConvert.DeserializeObject<SimpleAvailibilityaAddResponce>(_data[0]);
                }

                // for farebaisis
                string farebasisdataL = null;
                _data = journeyKey.ToString().Split("@0");
                if (!string.IsNullOrEmpty(_data[0]))
                {
                    string _farebasisL = _data[0].ToString().Split("_")[1];
                    farebasisdataL = _farebasisL;
                }
                string res = _objAvail.AirPriceGetCorporate(_testURL, fareRepriceReq, availibiltyRQGDS, newGuid.ToString(), _targetBranch, _userName, _password, AirfaredataL, farebasisdataL, 0, "GDSOneWay");

                string HostTokenKey = Regex.Match(res, @"HostToken\s*Key=""(?<HostTokenKey>[\s\S]*?)"">(?<Value>[\s\S]*?)</").Groups["HostTokenKey"].Value.Trim();
                string HostTokenValue = Regex.Match(res, @"HostToken\s*Key=""(?<HostTokenKey>[\s\S]*?)"">(?<Value>[\s\S]*?)</").Groups["Value"].Value.Trim();

                TravelPortParsing _objP = new TravelPortParsing();
                List<GDSResModel.Segment> getAirPriceRes = new List<GDSResModel.Segment>();
                if (res != null && !res.Contains("Bad Request") && !res.Contains("Internal Server Error"))
                {
                    getAirPriceRes = _objP.ParseAirFareRsp(res, "OneWay", availibiltyRQGDS);
                }
                if (getAirPriceRes.Count > 0)
                {
                    HttpContext.Session.SetString("Total", getAirPriceRes[0].Fare.TotalFareWithOutMarkUp.ToString());
                }
                string str = JsonConvert.SerializeObject(getAirPriceRes);
                #endregion

                #region GetState
                if (getAirPriceRes != null && getAirPriceRes.Count > 0)
                {
                    AirAsiaTripResponceModel AirAsiaTripResponceobj = new AirAsiaTripResponceModel();
                    #region Itenary segment and legs
                    int journeyscount = getAirPriceRes.Count;
                    List<AAJourney> AAJourneyList = new List<AAJourney>();
                    AASegment AASegmentobj = null;
                    string paxType = String.Empty;
                    List<AAPassengerfare> AAPassengerfarelist = new List<AAPassengerfare>();
                    for (int i = 0; i < journeyscount; i++)
                    {
                        AAJourney AAJourneyobj = new AAJourney();
                        AAJourneyobj.journeyKey = "";
                        int segmentscount = getAirPriceRes[0].Bonds[0].Legs.Count;
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
                            AAJourneyobj.designator = AADesignatorobj;
                            AASegmentobj = new AASegment();
                            AADesignator AASegmentDesignatorobj = new AADesignator();
                            AASegmentDesignatorobj.origin = getAirPriceRes[0].Bonds[0].Legs[j].Origin;
                            AASegmentDesignatorobj.destination = getAirPriceRes[0].Bonds[0].Legs[j].Destination;
                            AASegmentDesignatorobj.departure = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[j].DepartureTime);
                            AASegmentDesignatorobj.arrival = Convert.ToDateTime(getAirPriceRes[0].Bonds[0].Legs[j].ArrivalTime);
                            AASegmentobj.designator = AASegmentDesignatorobj;
                            List<AAFare> AAFarelist = new List<AAFare>();
                            AAFare AAFareobj = new AAFare();
                            AAFareobj.fareKey = "";
                            AAFareobj.productClass = journeyKey;
                            int passengerFarescount = getAirPriceRes[0].Fare.PaxFares.Count;
                            if (j == 0)
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
                            AASegmentobj.fares = AAFarelist;
                            AAIdentifierobj = new AAIdentifier();

                            AAIdentifierobj.identifier = getAirPriceRes[0].Bonds[0].Legs[j].FlightNumber;
                            AAIdentifierobj.carrierCode = getAirPriceRes[0].Bonds[0].Legs[j].CarrierCode;
                            if (AAIdentifierobj.carrierCode == "AI")
                            {
                                AAJourneyobj.Airlinename = "AirIndia";
                            }
                            if (AAIdentifierobj.carrierCode == "UK")
                            {
                                AAJourneyobj.Airlinename = "Vistara";
                            }
                            AASegmentobj.identifier = AAIdentifierobj;

                            var leg = getAirPriceRes[0].Bonds[0].Legs;
                            int legcount = getAirPriceRes[0].Bonds[0].Legs.Count;
                            List<AALeg> AALeglist = new List<AALeg>();
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
                            AASegmentobj.legs = AALeglist;
                            AASegmentobj.externalIdentifier = getAirPriceRes[0].Bonds[0].Legs[j].AircraftCode;
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
                    int paxcount = 0;
                    for (int i = 0; i < getAirPriceRes[0].Fare.PaxFares.Count; i++)
                    {
                        if (getAirPriceRes[0].Fare.PaxFares[i].PaxType == PAXTYPE.ADT)
                        {
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
                            passkeylist.Add(passkeytypeobj);
                            a++;
                        }
                    }
                    var sortedList = passkeylist.OrderBy(p => p.passengerTypeCode == "INF" ? 1 : 0).ToList();
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
                                        if (availibiltyRQGDS.passengercount != null)
                                        {
                                            Inftcount = availibiltyRQGDS.passengercount.infantcount;
                                        }
                                        else
                                        {
                                            Inftcount = availibiltyRQGDS.infantcount;
                                        }
                                        for (int i2 = 0; i2 < AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges.Count; i2++)
                                        {
                                            infttax += AAJourneyList[0].segments[i].fares[k].passengerFares[l].serviceCharges[i2].amount;
                                        }
                                        Inftbasefare = Convert.ToInt32(AAJourneyList[0].segments[i].fares[k].passengerFares[l].basicAmount);
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
                    AirAsiaTripResponceobj.passengers = sortedList;
                    AirAsiaTripResponceobj.passengerscount = passengercount;
                    AirAsiaTripResponceobj.infttax = basefareInfttax;
                    HttpContext.Session.SetString("PricingSolutionValue_0", JsonConvert.SerializeObject(getAirPriceRes[0].PricingSolutionValue));
                    #endregion
                    HttpContext.Session.SetString("SGkeypassenger", JsonConvert.SerializeObject(AirAsiaTripResponceobj));
                    #region ssravailability
                    _objAvail = null;
                    httpContextAccessorInstance = new HttpContextAccessor();
                    _objAvail = new TravelPort(httpContextAccessorInstance);
                    _testURL = AppUrlConstant.GDSSSRURL;
                    _targetBranch = "P7027135";
                    _userName = "Universal API/uAPI5098257106-beb65aec";
                    _password = "Q!f5-d7A3D";
                    StringBuilder SSRReq = new StringBuilder();
                    res = _objAvail.AirSSRGet(_testURL, SSRReq, "SsrType", newGuid.ToString(), _targetBranch, _userName, _password, 0, "GDSOneWay");
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
                            foreach (DictionaryEntry entry in htSSr)
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
                                    if (htSSr[legssrs.ssrCode] != null)
                                    {
                                        legssrs.name = htSSr[legssrs.ssrCode].ToString();
                                    }
                                    else
                                        continue;

                                    legssrs.available = 0;
                                    List<legpassengers> legpassengerslist = new List<legpassengers>();
                                    Decimal Amount = decimal.Zero;
                                    legpassengers passengersdetail = new legpassengers();
                                    passengersdetail.passengerKey = "";
                                    passengersdetail.ssrKey = "";
                                    passengersdetail.price = "0";
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
                        HttpContext.Session.SetString("Meals", JsonConvert.SerializeObject(SSRAvailabiltyResponceobj));
                    }

                    #endregion

                    #region SeatMap
                    _testURL = AppUrlConstant.GDSSeatURL;
                    string SeatMapres = _objAvail.GetSeatMap(_testURL, fareRepriceReq, availibiltyRQGDS, newGuid.ToString(), _targetBranch, _userName, _password, AirfaredataL, farebasisdataL, HostTokenKey, HostTokenValue, 0, "GDSOneWay");
                    List<IndigoBookingManager_.GetSeatAvailabilityResponse> SeatGroup = null;
                    {
                        string columncount0 = string.Empty;
                        List<data> datalist = new List<data>();
                        Hashtable htseat = new Hashtable();
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
                            Seatmapobj.name = obj.Match(mitem.Value).Groups["Equipment"].Value.Trim();
                            TempData["AirCraftName"] = "";
                            Seatmapobj.arrivalStation = obj.Match(mitem.Value).Groups["arrivalStation"].Value.Trim();
                            Seatmapobj.departureStation = obj.Match(mitem.Value).Groups["departureStation"].Value.Trim();
                            Seatmapobj.marketingCode = "";
                            Seatmapobj.equipmentType = "";
                            Seatmapobj.equipmentTypeSuffix = "";
                            List<Unit> compartmentsunitlist = new List<Unit>();
                            Seatmapobj.decksindigo = new List<Decks>();
                            Decks Decksobj = null;
                            string _seatPosition = "";
                            Hashtable htPaidSeatPrice = new Hashtable();
                            Hashtable _htpaxwiseSeat = new Hashtable();
                            string BookingTravellerref = string.Empty;
                            foreach (Match mSeat in Regex.Matches(SeatMapres, @"<air:OptionalService Type=""PreReservedSeatAssignment[\s\S]*?TotalPrice=""(?<Price>[\s\S]*?)""[\s\S]*?Key=""(?<Key>[\s\S]*?)""[\s\S]*?</air:OptionalService>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                            {
                                if (!htPaidSeatPrice.Contains(mSeat.Groups["Key"].Value.Trim()))
                                {
                                    htPaidSeatPrice.Add(mSeat.Groups["Key"].Value.Trim(), mSeat.Groups["Price"].Value.Trim());
                                }

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
                                        Decksobj.availableUnits = _Count;
                                        Decksobj.designator = "";
                                        Decksobj.length = 0;
                                        Decksobj.width = 0;
                                        Decksobj.sequence = 0;
                                        Decksobj.orientation = 0;
                                        try
                                        {
                                            Unit compartmentsunitobj = new Unit();
                                            compartmentsunitobj.Airline = Airlines.AirIndia;
                                            if (mFacility.Groups["Availablity"].Value.Trim().ToLower() == "available")
                                                compartmentsunitobj.assignable = true;
                                            else
                                                compartmentsunitobj.assignable = false;
                                            compartmentsunitobj.compartmentDesignator = "";
                                            compartmentsunitobj.designator = mFacility.Groups["SeatNumber"].Value.Trim();
                                            if (!string.IsNullOrEmpty(_OptionalServiceRef))
                                            {
                                                compartmentsunitobj.servicechargefeeAmount = Convert.ToDecimal(Regex.Match(htPaidSeatPrice[_OptionalServiceRef].ToString(), @"\d+").Value);
                                            }
                                            else
                                                compartmentsunitobj.servicechargefeeAmount = 0M;
                                            compartmentsunitobj.type = Convert.ToInt32(0);
                                            compartmentsunitobj.travelClassCode = "0";
                                            compartmentsunitobj.set = 0;
                                            compartmentsunitobj.group = 1;
                                            compartmentsunitobj.priority = 0;
                                            compartmentsunitobj.text = "";
                                            compartmentsunitobj.angle = 0;
                                            compartmentsunitobj.width = 0;
                                            compartmentsunitobj.height = 0;
                                            compartmentsunitobj.zone = 0;
                                            compartmentsunitobj.x = 0;
                                            compartmentsunitobj.y = 0;
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
                                                else if (compartmentyproperties.value.Contains("PaidGeneralSeat") && mFacility.Groups["Availablity"].Value.Trim().ToLower() == "blocked")
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
                            dataobj.seatMap = Seatmapobj;
                            dataobj.seatMapfees = Fees;
                            datalist.Add(dataobj);
                            SeatMapResponceModel.datalist = datalist;
                            SeatMapResponceModel.htSeatlist = htseat;
                        }
                        string strseat = JsonConvert.SerializeObject(SeatMapResponceModel);
                        HttpContext.Session.SetString("Seatmap", JsonConvert.SerializeObject(SeatMapResponceModel));
                        HttpContext.Session.SetString("SeatResponseleft", JsonConvert.SerializeObject(SeatMapres));
                    }
                    #endregion
                }
                return RedirectToAction("GDSSaverTripsell", "GDSTripsell", new { Guid = Guid });
            }
        }
    }
}
