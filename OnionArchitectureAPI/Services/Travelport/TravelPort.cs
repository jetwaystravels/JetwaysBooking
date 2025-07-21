using DomainLayer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using ServiceLayer.Service.Interface;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Utility;
using ZXing;
using ZXing.QrCode.Internal;
using static DomainLayer.Model.GDSResModel;
using static DomainLayer.Model.ReturnTicketBooking;
using static OnionArchitectureAPI.Services.Indigo._SellSSR;
using static OnionArchitectureAPI.Services.Indigo._updateContact;
using static System.Net.Mime.MediaTypeNames;

namespace OnionArchitectureAPI.Services.Travelport
{
    public class TravelPort : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TravelPort(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetSessionValue(string key, string value)
        {
            _httpContextAccessor.HttpContext.Session.SetString(key, value);
        }
        Logs logs = new Logs();
        //string _targetBranch = string.Empty;
        //string _userName = string.Empty;
        //string _password = string.Empty;
        //public TravelPort(string tragetBranch_, string userName_, string password_)
        //{
        //    _targetBranch = tragetBranch_;
        //    _userName = userName_;
        //    _password = password_;
        //}
        public string GetAvailabiltyRT(string _testURL, StringBuilder sbReq, TravelPort _objAvail, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, string flightclass, string _AirlineWay)
        {

            sbReq = new StringBuilder();
            sbReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            sbReq.Append("<soap:Body>");
            sbReq.Append("<LowFareSearchReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" SolutionResult=\"true\" TraceId=\"" + newGuid + "\" TargetBranch=\"" + _targetBranch + "\" ReturnUpsellFare =\"true\">");
            sbReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            sbReq.Append("<SearchAirLeg>");
            sbReq.Append("<SearchOrigin>");
            sbReq.Append("<CityOrAirport xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"" + _GetfligthModel.origin + "\" PreferCity=\"true\" />");
            sbReq.Append("</SearchOrigin>");
            sbReq.Append("<SearchDestination>");
            sbReq.Append("<CityOrAirport xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"" + _GetfligthModel.destination + "\" PreferCity=\"true\" />");
            sbReq.Append("</SearchDestination>");
            sbReq.Append("<SearchDepTime PreferredTime=\"" + _GetfligthModel.beginDate + "\"/>");
            sbReq.Append("</SearchAirLeg>");

            sbReq.Append("<SearchAirLeg>");
            sbReq.Append("<SearchOrigin>");
            sbReq.Append("<CityOrAirport xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"" + _GetfligthModel.destination + "\" PreferCity=\"true\" />");
            sbReq.Append("</SearchOrigin>");
            sbReq.Append("<SearchDestination>");
            sbReq.Append("<CityOrAirport xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"" + _GetfligthModel.origin + "\" PreferCity=\"true\" />");
            sbReq.Append("</SearchDestination>");
            sbReq.Append("<SearchDepTime PreferredTime=\"" + _GetfligthModel.endDate + "\"/>");
            sbReq.Append("</SearchAirLeg>");

            sbReq.Append("<AirSearchModifiers>");
            sbReq.Append("<PreferredProviders>");
            sbReq.Append("<Provider xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"1G\" />");
            sbReq.Append("</PreferredProviders>");

            // Start for prohibited carrier
            sbReq.Append("<ProhibitedCarriers>");
            sbReq.Append("<Carrier Code='H1' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            sbReq.Append("</ProhibitedCarriers>");
            //End  for prohibited carrier

            // Business class
            if (flightclass == "B")
            {
                sbReq.Append("<PermittedCabins>");
                sbReq.Append("<CabinClass Type=\"PremiumEconomy\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                sbReq.Append("</PermittedCabins>");
            }

            //Permitted Carrier
            //sbReq.Append("<air:PermittedCarriers xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
            //sbReq.Append("<Carrier Code='9W' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='AI' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='UK' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("</air:PermittedCarriers>");

            sbReq.Append("</AirSearchModifiers>");
            int pax = 0;
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                    }
                }

                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"true\" Age=\"01\"/>");
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"10\"/>");
                    }
                }
            }
            else
            {
                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                    }
                }
                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"true\" Age=\"01\"/>");
                    }
                }
                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"10\"/>");
                    }
                }



            }
            sbReq.Append("<AirPricingModifiers FaresIndicator=\"AllFares\" ETicketability=\"Required\">");
            sbReq.Append("<AccountCodes>");
            sbReq.Append("<AccountCode xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"-\" />");
            sbReq.Append("</AccountCodes>");
            sbReq.Append("<FlightType TripleInterlineCon=\"false\" DoubleInterlineCon=\"false\" SingleInterlineCon=\"true\" TripleOnlineCon=\"false\" DoubleOnlineCon=\"false\" SingleOnlineCon=\"true\" StopDirects=\"true\" NonStopDirects=\"true\" />");
            sbReq.Append("</AirPricingModifiers>");
            sbReq.Append("</LowFareSearchReq></soap:Body></soap:Envelope>");

            string res = Methodshit.HttpPost(_testURL, sbReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + sbReq + "\n\n Response: " + res, "GetAvailability", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(sbReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAvailability", "GDSRT");
            }
            return res;
        }

        public string GetAvailabiltyCorporate(string _testURL, StringBuilder sbReq, TravelPort _objAvail, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, string flightclass, string JourneyType, string _AirlineWay)
        {

            sbReq = new StringBuilder();
            sbReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            sbReq.Append("<soap:Body>");
            sbReq.Append("<air:LowFareSearchReq xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" xmlns:air=\"http://www.travelport.com/schema/air_v52_0\"  AuthorizedBy=\"Travelport\" SolutionResult=\"true\" TraceId=\"" + newGuid + "\" TargetBranch=\"" + _targetBranch + "\" ReturnUpsellFare =\"true\">");
            sbReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            sbReq.Append("<air:SearchAirLeg>");
            sbReq.Append("<air:SearchOrigin>");
            sbReq.Append("<com:CityOrAirport Code=\"" + _GetfligthModel.origin + "\"/>");
            sbReq.Append("</air:SearchOrigin>");
            sbReq.Append("<air:SearchDestination>");
            sbReq.Append("<com:CityOrAirport Code=\"" + _GetfligthModel.destination + "\"/>");
            sbReq.Append("</air:SearchDestination>");

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                sbReq.Append("<air:SearchDepTime PreferredTime=\"" + _GetfligthModel.beginDate + "\"/>");
            }
            else
            {
                sbReq.Append("<air:SearchDepTime PreferredTime=\"" + _GetfligthModel.endDate + "\"/>");

            }
            sbReq.Append("</air:SearchAirLeg>");
            sbReq.Append("<air:AirSearchModifiers>");
            sbReq.Append("<air:PreferredProviders>");
            sbReq.Append("<com:Provider Code=\"1G\" />");
            sbReq.Append("</air:PreferredProviders>");

            // Start for prohibited carrier
            //sbReq.Append("<ProhibitedCarriers>");
            //sbReq.Append("<Carrier Code='H1' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("</ProhibitedCarriers>");
            //End  for prohibited carrier

            // Business class
            if (flightclass == "B")
            {
                sbReq.Append("<air:PermittedCabins>");
                sbReq.Append("<com:CabinClass Type=\"Business\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                sbReq.Append("</air:PermittedCabins>");
            }

            // Economy Premium class
            if (flightclass == "P")
            {
                sbReq.Append("<air:PermittedCabins>");
                sbReq.Append("<com:CabinClass Type=\"PremiumEconomy\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                sbReq.Append("</air:PermittedCabins>");
            }

            //Permitted Carrier
            //sbReq.Append("<air:PermittedCarriers xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
            //sbReq.Append("<Carrier Code='9W' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='AI' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='UK' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("</air:PermittedCarriers>");

            sbReq.Append("</air:AirSearchModifiers>");
            int pax = 0;
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                        pax++;
                    }
                }

                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        //PricePTCOnly =\"True\" this is in certification log,2,2,2 cases flight is not showing in this case
                        sbReq.Append("<com:SearchPassenger Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"false\" Age=\"1\"/>");
                        pax++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {

                        sbReq.Append("<com:SearchPassenger  Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"11\"/>");
                        pax++;
                    }
                }
            }
            else
            {
                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {

                        sbReq.Append("<com:SearchPassenger  Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                        pax++;
                    }
                }
                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger  Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        pax++;
                    }
                }
                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"11\"/>");
                        pax++;
                    }
                }



            }
            sbReq.Append("<air:AirPricingModifiers FaresIndicator=\"AllFares\" ETicketability=\"Required\">");
            sbReq.Append("<FlightType TripleInterlineCon=\"false\" DoubleInterlineCon=\"false\" SingleInterlineCon=\"true\" TripleOnlineCon=\"false\" DoubleOnlineCon=\"false\" SingleOnlineCon=\"true\" StopDirects=\"true\" NonStopDirects=\"true\" />");
            sbReq.Append("<air:AccountCodes><com:AccountCode Code=\"SME2\" /></air:AccountCodes>");
            sbReq.Append("</air:AirPricingModifiers>");
            sbReq.Append("</air:LowFareSearchReq></soap:Body></soap:Envelope>");
            string res = Methodshit.HttpPost(_testURL, sbReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs("URL: " + _testURL + "\n\nRequest: " + sbReq, "1-GetAvailabilityReq", "GDSOneWay", JourneyType);
                logs.WriteLogs(res, "1-GetAvailabilityRes", "GDSOneWay", JourneyType);
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(sbReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAvailability", "GDSRT");
                logs.WriteLogsR("URL: " + _testURL + "\n\nRequest: " + sbReq, "1-GetAvailabilityReq", "GDSRT");
                logs.WriteLogsR(res, "1-GetAvailabilityRes", "GDSRT");
            }
            return res;
        }
        public string GetAvailabilty(string _testURL, StringBuilder sbReq, TravelPort _objAvail, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, string flightclass, string JourneyType, string _AirlineWay)
        {

            sbReq = new StringBuilder();
            sbReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            sbReq.Append("<soap:Body>");
            sbReq.Append("<air:LowFareSearchReq xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" xmlns:air=\"http://www.travelport.com/schema/air_v52_0\"  AuthorizedBy=\"Travelport\" SolutionResult=\"true\" TraceId=\"" + newGuid + "\" TargetBranch=\"" + _targetBranch + "\" ReturnUpsellFare =\"true\">");
            sbReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            sbReq.Append("<air:SearchAirLeg>");
            sbReq.Append("<air:SearchOrigin>");
            sbReq.Append("<com:CityOrAirport Code=\"" + _GetfligthModel.origin + "\"/>");
            sbReq.Append("</air:SearchOrigin>");
            sbReq.Append("<air:SearchDestination>");
            sbReq.Append("<com:CityOrAirport Code=\"" + _GetfligthModel.destination + "\"/>");
            sbReq.Append("</air:SearchDestination>");

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                sbReq.Append("<air:SearchDepTime PreferredTime=\"" + _GetfligthModel.beginDate + "\"/>");
            }
            else
            {
                sbReq.Append("<air:SearchDepTime PreferredTime=\"" + _GetfligthModel.endDate + "\"/>");

            }
            sbReq.Append("</air:SearchAirLeg>");
            sbReq.Append("<air:AirSearchModifiers>");
            sbReq.Append("<air:PreferredProviders>");
            sbReq.Append("<com:Provider Code=\"1G\" />");
            sbReq.Append("</air:PreferredProviders>");

            // Start for prohibited carrier
            //sbReq.Append("<ProhibitedCarriers>");
            //sbReq.Append("<Carrier Code='H1' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("</ProhibitedCarriers>");
            //End  for prohibited carrier

            // Business class
            if (flightclass == "B")
            {
                sbReq.Append("<air:PermittedCabins>");
                sbReq.Append("<com:CabinClass Type=\"Business\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                sbReq.Append("</air:PermittedCabins>");
            }

            // Economy Premium class
            if (flightclass == "P")
            {
                sbReq.Append("<air:PermittedCabins>");
                sbReq.Append("<com:CabinClass Type=\"PremiumEconomy\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                sbReq.Append("</air:PermittedCabins>");
            }

            //Permitted Carrier
            //sbReq.Append("<air:PermittedCarriers xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
            //sbReq.Append("<Carrier Code='9W' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='AI' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='UK' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("</air:PermittedCarriers>");

            sbReq.Append("</air:AirSearchModifiers>");
            int pax = 0;
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                        pax++;
                    }
                }

                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        //PricePTCOnly =\"True\" this is in certification log,2,2,2 cases flight is not showing in this case
                        sbReq.Append("<com:SearchPassenger Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"false\" Age=\"1\"/>");
                        pax++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {

                        sbReq.Append("<com:SearchPassenger  Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"11\"/>");
                        pax++;
                    }
                }
            }
            else
            {
                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {

                        sbReq.Append("<com:SearchPassenger  Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                        pax++;
                    }
                }
                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger  Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        pax++;
                    }
                }
                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"11\"/>");
                        pax++;
                    }
                }



            }
            sbReq.Append("<air:AirPricingModifiers FaresIndicator=\"AllFares\" ETicketability=\"Required\">");
            //sbReq.Append("<AccountCodes>");
            //sbReq.Append("<AccountCode xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"-\" />");
            //sbReq.Append("</AccountCodes>");
            sbReq.Append("<FlightType TripleInterlineCon=\"false\" DoubleInterlineCon=\"false\" SingleInterlineCon=\"true\" TripleOnlineCon=\"false\" DoubleOnlineCon=\"false\" SingleOnlineCon=\"true\" StopDirects=\"true\" NonStopDirects=\"true\" />");
            //sbReq.Append("<air:AccountCodes><com:AccountCode Code=\"SME2\" /></air:AccountCodes>");
            sbReq.Append("</air:AirPricingModifiers>");
            sbReq.Append("</air:LowFareSearchReq></soap:Body></soap:Envelope>");

            //sbReq.Append("<air:LowFareSearchReq xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" xmlns:air=\"http://www.travelport.com/schema/air_v52_0\" AuthorizedBy=\"ENDFARE\" ");
            //sbReq.Append("SolutionResult=\"true\" TraceId=\"" + newGuid + "\" TargetBranch=\"" + _objAvail._targetBranch + "\">");
            //sbReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            //sbReq.Append("<air:SearchAirLeg>");
            //sbReq.Append("<air:SearchOrigin><com:CityOrAirport Code=\"" + _GetfligthModel.origin + "\"/></air:SearchOrigin>");
            //sbReq.Append("<air:SearchDestination><com:CityOrAirport Code=\"" + _GetfligthModel.destination + "\"/></air:SearchDestination>");
            //sbReq.Append("<air:SearchDepTime PreferredTime=\"" + _GetfligthModel.beginDate + "\"/>");
            //sbReq.Append("<air:AirLegModifiers><air:PreferredCabins><com:CabinClass Type=\"Economy\"/></air:PreferredCabins></air:AirLegModifiers>");
            //sbReq.Append("</air:SearchAirLeg><air:AirSearchModifiers OrderBy=\"DepartureTime\">");
            //sbReq.Append("<air:PreferredProviders><com:Provider Code=\"1G\"/></air:PreferredProviders>");

            ////sbReq.Append("<air:PermittedCarriers xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
            ////sbReq.Append("<Carrier Code='9W' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            ////sbReq.Append("<Carrier Code='AI' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            ////sbReq.Append("<Carrier Code='UK' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            ////sbReq.Append("</air:PermittedCarriers>");
            //sbReq.Append("</air:AirSearchModifiers>");

            //if (_GetfligthModel.passengercount != null)
            //{
            //    if (_GetfligthModel.passengercount.adultcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"ADT\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUA01\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.passengercount.infantcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"INF\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUB02\" PricePTCOnly=\"true\" Age=\"01\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.passengercount.childcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"CNN\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUC03\" Age=\"10\"/>");
            //        }
            //    }
            //}
            //else
            //{

            //    if (_GetfligthModel.adultcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.adultcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"ADT\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUA01\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.infantcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.infantcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"INF\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUB02\" PricePTCOnly=\"true\" Age=\"01\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.childcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.childcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"CNN\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUC03\" Age=\"10\"/>");
            //        }
            //    }
            //}
            //sbReq.Append("<air:AirPricingModifiers FaresIndicator=\"AllFares\" ETicketability=\"Yes\" CurrencyType=\"INR\">");
            //sbReq.Append("<air:FlightType RequireSingleCarrier=\"true\" MaxConnections=\"1\" MaxStops=\"1\" NonStopDirects=\"true\" StopDirects=\"true\" SingleOnlineCon=\"true \"/></air:AirPricingModifiers>");
            //sbReq.Append("</air:LowFareSearchReq></soap:Body></soap:Envelope>");

            string res = Methodshit.HttpPost(_testURL, sbReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs("URL: " + _testURL + "\n\nRequest: " + sbReq, "1-GetAvailabilityReq", "GDSOneWay", JourneyType);
                logs.WriteLogs(res, "1-GetAvailabilityRes", "GDSOneWay", JourneyType);
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(sbReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAvailability", "GDSRT");
                logs.WriteLogsR("URL: " + _testURL + "\n\nRequest: " + sbReq, "1-GetAvailabilityReq", "GDSRT");
                logs.WriteLogsR(res, "1-GetAvailabilityRes", "GDSRT");
            }
            return res;
        }

        public string AirPriceGetRoundTrip(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, string farebasisdataL, int p, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do
            string segmentIdDataL = string.Empty;
            if (p == 0)
            {
                segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            }
            else
            {
                segmentIdDataL = AirfaredataL.SegmentidRightdata;
            }
            string FarebasisDataL = farebasisdataL;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }



            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                count++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"1\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }
            foreach (var segment in AirfaredataL.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;

                    FarebasisDataL = FarebasisDataL0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                    FarebasisDataL = FarebasisDataL1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                    FarebasisDataL = FarebasisDataL2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
                //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(fareRepriceReq.ToString(), "3-GetAirpriceReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "2-GetAirpriceRes", "GDSOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p == 0)
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Right", "GDSRT");
                }
            }
            return res;
        }

        public string AirPriceGet(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, string farebasisdataL, int p, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do
            string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            string FarebasisDataL = farebasisdataL;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }



            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                count++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"1\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }
            foreach (var segment in AirfaredataL.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;

                    FarebasisDataL = FarebasisDataL0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                    FarebasisDataL = FarebasisDataL1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                    FarebasisDataL = FarebasisDataL2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
                //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(fareRepriceReq.ToString(), "3-GetAirpriceReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "2-GetAirpriceRes", "GDSOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p == 0)
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Right", "GDSRT");
                }
            }
            return res;
        }
        //Same Airline RoundTrip 26-09-2024
        public string AirPriceGetCorporate(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, string farebasisdataL, int p, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<air:AirPriceReq xmlns:air=\"http://www.travelport.com/schema/air_v52_0\" xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<air:AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do
            string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            string FarebasisDataL = farebasisdataL;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }



            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<air:AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                //fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</air:AirSegment>");
                count++;
            }

            fareRepriceReq.Append("</air:AirItinerary>");
            fareRepriceReq.Append("<air:AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<air:AccountCodes>");
            fareRepriceReq.Append("<com:AccountCode Code=\"SME2\" ProviderCode=\"1G\"/>");
            fareRepriceReq.Append("</air:AccountCodes>");
            fareRepriceReq.Append("</air:AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"1\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            fareRepriceReq.Append("<air:AirPricingCommand/>");
            /*if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }
            foreach (var segment in AirfaredataL.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;

                    FarebasisDataL = FarebasisDataL0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                    FarebasisDataL = FarebasisDataL1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                    FarebasisDataL = FarebasisDataL2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
                //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");*/
            fareRepriceReq.Append("</air:AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(fareRepriceReq.ToString(), "3-GetAirpriceReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "2-GetAirpriceRes", "GDSOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p == 0)
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Right", "GDSRT");
                }
            }
            return res;
        }
        public string AirPriceGetRoundTripCorporate(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, string farebasisdataL, int p, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<air:AirPriceReq xmlns:air=\"http://www.travelport.com/schema/air_v52_0\" xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<air:AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do
            string segmentIdDataL = string.Empty;
            if (p == 0)
            {
                segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            }
            else
            {
                segmentIdDataL = AirfaredataL.SegmentidRightdata;
            }
            string FarebasisDataL = farebasisdataL;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }



            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<air:AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                //fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</air:AirSegment>");
                count++;
            }

            fareRepriceReq.Append("</air:AirItinerary>");
            fareRepriceReq.Append("<air:AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<air:AccountCodes>");
            fareRepriceReq.Append("<com:AccountCode Code=\"SME2\" ProviderCode=\"1G\"/>");
            fareRepriceReq.Append("</air:AccountCodes>");
            fareRepriceReq.Append("</air:AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"1\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<com:SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            fareRepriceReq.Append("<air:AirPricingCommand/>");
            /* if (segmentIdsL.Length == 3)
             {
                 segmentIdAtIndex0 = segmentIdsL[0];
                 segmentIdAtIndex1 = segmentIdsL[1];
                 segmentIdAtIndex2 = segmentIdsL[2];

                 FarebasisDataL0 = FarebasisL[0];
                 FarebasisDataL1 = FarebasisL[1];
                 FarebasisDataL2 = FarebasisL[2];

             }
             else if (segmentIdsL.Length == 2)
             {
                 // Accessing elements by index
                 segmentIdAtIndex0 = segmentIdsL[0];
                 segmentIdAtIndex1 = segmentIdsL[1];

                 FarebasisDataL0 = FarebasisL[0];
                 FarebasisDataL1 = FarebasisL[1];
             }
             else
             {
                 segmentIdAtIndex0 = segmentIdsL[0];
                 FarebasisDataL0 = FarebasisL[0];
             }
             foreach (var segment in AirfaredataL.segments)
             {
                 if (legKeyCounter == 0)
                 {
                     segmentIdAtIndex0 = segmentIdAtIndex0;

                     FarebasisDataL = FarebasisDataL0;
                 }
                 else if (legKeyCounter == 1)
                 {
                     segmentIdAtIndex0 = segmentIdAtIndex1;
                     FarebasisDataL = FarebasisDataL1;
                 }
                 else
                 {
                     segmentIdAtIndex0 = segmentIdAtIndex2;
                     FarebasisDataL = FarebasisDataL2;
                 }
                 fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
                 fareRepriceReq.Append("<PermittedBookingCodes>");
                 fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                 fareRepriceReq.Append("</PermittedBookingCodes>");
                 fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                 legKeyCounter++;
             }
             fareRepriceReq.Append("</AirPricingCommand>");
             fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");*/
            fareRepriceReq.Append("</air:AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(fareRepriceReq.ToString(), "3-GetAirpriceReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "2-GetAirpriceRes", "GDSOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p == 0)
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Right", "GDSRT");
                }
            }
            return res;
        }
        public string AirSSRGet(string _testURL, StringBuilder SSRReq, string _SSrType, string newGuid, string _targetBranch, string _userName, string _password, int p, string _AirlineWay)
        {

            SSRReq = new StringBuilder();
            SSRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            SSRReq.Append("<soap:Body>");
            SSRReq.Append("<ReferenceDataSearchReq AuthorizedBy=\"Travelport\" TargetBranch=\"" + _targetBranch + "\" TraceId=\"" + newGuid + "\" xmlns=\"http://www.travelport.com/schema/util_v48_0\" xmlns:common=\"http://www.travelport.com/schema/common_v48_0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.travelport.com/schema/util_v48_0 file:///C:/Users/mukil.kumar/Documents/Ecommerce/WSDL/Release-V19.1.0.53-V19.1/util_v48_0/Util.xsd\">");
            SSRReq.Append("<common:BillingPointOfSaleInfo OriginApplication=\"uAPI\"/>");
            SSRReq.Append("<ReferenceDataSearchItem Type=\"" + _SSrType + "\"/>");
            SSRReq.Append("</ReferenceDataSearchReq>");
            SSRReq.Append("</soap:Body></soap:Envelope>");
            string res = Methodshit.HttpPost(_testURL, SSRReq.ToString(), _userName, _password);

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(SSRReq.ToString(), "3-GetSSRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "2-GetSSRRes", "GDSOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p == 0)
                {
                    logs.WriteLogsR(SSRReq.ToString(), "3-GetSSRReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "2-GetSSRRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(SSRReq.ToString(), "3-GetSSRReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "2-GetSSRRes_Right", "GDSRT");
                }
            }
            return res;
        }

        public string GetSeatMap(string _testURL, StringBuilder SeatMapReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, string farebasisdataL, string hostTokenKey, string hostTokenValue, int p, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;
            //<SeatMapReq xmlns="http://www.travelport.com/schema/air_v52_0" TraceId="bd4398ec-5a0f-4918-82a9-7571c4536227" AuthorizedBy="Travelport" TargetBranch="P7087680" ReturnSeatPricing="true" ReturnBrandingInfo="true">
            SeatMapReq = new StringBuilder();
            SeatMapReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body> <SeatMapReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy=\"Travelport\" TargetBranch=\"" + _targetBranch + "\" ReturnSeatPricing=\"true\" ReturnBrandingInfo=\"true\">");
            SeatMapReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"uAPI\" />");

            // to do
            string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            string FarebasisDataL = farebasisdataL;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }



            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                SeatMapReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" HostTokenRef=\"" + hostTokenKey + "\" AvailabilitySource=\"" + segment.designator._AvailabilitySource + "\" ");
                SeatMapReq.Append("Equipment=\"" + segment.designator._Equipment + "\" AvailabilityDisplayType=\"Fare Specific Fare Quote Unbooked\" Group=\"" + segment.designator._Group + "\" Carrier=\"" + segment.identifier.carrierCode + "\" ");
                SeatMapReq.Append("FlightNumber=\"" + segment.identifier.identifier + "\" Origin=\"" + segment.designator.origin + "\" Destination=\"" + segment.designator.destination + "\" ");
                SeatMapReq.Append("DepartureTime=\"" + segment.designator._DepartureDate + "\" ArrivalTime=\"" + segment.designator._ArrivalDate + "\" FlightTime=\"" + segment.designator._FlightTime + "\" TravelTime=\"130\" Distance=\"" + segment.designator._Distance + "\" ProviderCode=\"" + segment.designator._ProviderCode + "\" ClassOfService=\"" + segment.designator._ClassOfService + "\">");
                SeatMapReq.Append("<CodeshareInfo OperatingCarrier=\"" + segment.identifier.carrierCode + "\" />");
                SeatMapReq.Append("</AirSegment>");
                count++;
            }
            foreach (var segment in AirfaredataL.segments)
            {
                SeatMapReq.Append("<HostToken xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + hostTokenKey + "\">" + hostTokenValue + "</HostToken>");
            }
            //fareRepriceReq.Append("</AirItinerary>");
            //fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            //fareRepriceReq.Append("<BrandModifiers>");
            //fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            //fareRepriceReq.Append("</BrandModifiers>");
            //fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"ADT\" Age=\"40\" Key=\"" + paxCount + "\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"Mr\" First=\"ADT\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"INF\"  PricePTCOnly=\"true\" Key=\"" + paxCount + "\" Age=\"1\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"MSTR\" First=\"INFT\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"CNN\" Key=\"" + paxCount + "\" Age=\"11\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"MSTR\" First=\"CHD\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"ADT\" Age=\"40\" Key=\"" + paxCount + "\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"Mr\" First=\"ADT\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"INF\"  PricePTCOnly=\"true\" Key=\"" + paxCount + "\" Age=\"1\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"MSTR\" First=\"INFT\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"CNN\" Key=\"" + paxCount + "\" Age=\"11\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"MSTR\" First=\"CHD\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }

            }
            SeatMapReq.Append("</SeatMapReq></soap:Body></soap:Envelope>");
            //if (segmentIdsL.Length == 3)
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //    segmentIdAtIndex2 = segmentIdsL[2];

            //    FarebasisDataL0 = FarebasisL[0];
            //    FarebasisDataL1 = FarebasisL[1];
            //    FarebasisDataL2 = FarebasisL[2];

            //}
            //else if (segmentIdsL.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];

            //    FarebasisDataL0 = FarebasisL[0];
            //    FarebasisDataL1 = FarebasisL[1];
            //}
            //else
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    FarebasisDataL0 = FarebasisL[0];
            //}
            //foreach (var segment in AirfaredataL.segments)
            //{
            //    if (legKeyCounter == 0)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex0;

            //        FarebasisDataL = FarebasisDataL0;
            //    }
            //    else if (legKeyCounter == 1)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex1;
            //        FarebasisDataL = FarebasisDataL1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex2;
            //        FarebasisDataL = FarebasisDataL2;
            //    }
            //    fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
            //    //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
            //    fareRepriceReq.Append("<PermittedBookingCodes>");
            //    fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
            //    //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
            //    fareRepriceReq.Append("</PermittedBookingCodes>");
            //    fareRepriceReq.Append("</AirSegmentPricingModifiers>");
            //    legKeyCounter++;
            //}
            //fareRepriceReq.Append("</AirPricingCommand>");
            //fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            //fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, SeatMapReq.ToString(), _userName, _password);

            //// Load XML into XmlDocument
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(res);

            // Convert XML to JSON
            //string json = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);

            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(SeatMapReq.ToString(), "3-GetSeatMapReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "2-GetSeatMapRes", "GDSOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p == 0)
                {
                    logs.WriteLogsR(SeatMapReq.ToString(), "3-GetSeatMapReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "2-GetSeatMapRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(SeatMapReq.ToString(), "3-GetSeatMapReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "2-GetSeatMapRes_Right", "GDSRT");
                }
            }
            return res;
        }

        public string GetSeatMapRoundTrip(string _testURL, StringBuilder SeatMapReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, string farebasisdataL, string hostTokenKey, string hostTokenValue, int p, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;
            //<SeatMapReq xmlns="http://www.travelport.com/schema/air_v52_0" TraceId="bd4398ec-5a0f-4918-82a9-7571c4536227" AuthorizedBy="Travelport" TargetBranch="P7087680" ReturnSeatPricing="true" ReturnBrandingInfo="true">
            SeatMapReq = new StringBuilder();
            SeatMapReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body> <SeatMapReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy=\"Travelport\" TargetBranch=\"" + _targetBranch + "\" ReturnSeatPricing=\"true\" ReturnBrandingInfo=\"true\">");
            SeatMapReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"uAPI\" />");

            // to do
            string segmentIdDataL = string.Empty;
            string FarebasisDataL = farebasisdataL;
            if (p == 0)
            {
                segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            }
            else
            {
                segmentIdDataL = AirfaredataL.SegmentidRightdata;
            }

            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }



            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                SeatMapReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" HostTokenRef=\"" + hostTokenKey + "\" AvailabilitySource=\"" + segment.designator._AvailabilitySource + "\" ");
                SeatMapReq.Append("Equipment=\"" + segment.designator._Equipment + "\" AvailabilityDisplayType=\"Fare Specific Fare Quote Unbooked\" Group=\"" + segment.designator._Group + "\" Carrier=\"" + segment.identifier.carrierCode + "\" ");
                SeatMapReq.Append("FlightNumber=\"" + segment.identifier.identifier + "\" Origin=\"" + segment.designator.origin + "\" Destination=\"" + segment.designator.destination + "\" ");
                SeatMapReq.Append("DepartureTime=\"" + segment.designator._DepartureDate + "\" ArrivalTime=\"" + segment.designator._ArrivalDate + "\" FlightTime=\"" + segment.designator._FlightTime + "\" TravelTime=\"130\" Distance=\"" + segment.designator._Distance + "\" ProviderCode=\"" + segment.designator._ProviderCode + "\" ClassOfService=\"" + segment.designator._ClassOfService + "\">");
                SeatMapReq.Append("<CodeshareInfo OperatingCarrier=\"" + segment.identifier.carrierCode + "\" />");
                SeatMapReq.Append("</AirSegment>");
                count++;
            }
            foreach (var segment in AirfaredataL.segments)
            {
                SeatMapReq.Append("<HostToken xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + hostTokenKey + "\">" + hostTokenValue + "</HostToken>");
            }
            //fareRepriceReq.Append("</AirItinerary>");
            //fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            //fareRepriceReq.Append("<BrandModifiers>");
            //fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            //fareRepriceReq.Append("</BrandModifiers>");
            //fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"ADT\" Age=\"40\" Key=\"" + paxCount + "\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"Mr\" First=\"ADT\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"INF\"  PricePTCOnly=\"true\" Key=\"" + paxCount + "\" Age=\"1\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"MSTR\" First=\"INFT\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"CNN\" Key=\"" + paxCount + "\" Age=\"11\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"MSTR\" First=\"CHD\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"ADT\" Age=\"40\" Key=\"" + paxCount + "\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"Mr\" First=\"ADT\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"INF\"  PricePTCOnly=\"true\" Key=\"" + paxCount + "\" Age=\"1\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"MSTR\" First=\"INFT\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        SeatMapReq.Append("<SearchTraveler Code=\"CNN\" Key=\"" + paxCount + "\" Age=\"11\">");
                        SeatMapReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" Prefix=\"MSTR\" First=\"CHD\" Last=\"One\" /></SearchTraveler>");
                        paxCount++;
                    }
                }

            }
            SeatMapReq.Append("</SeatMapReq></soap:Body></soap:Envelope>");
            //if (segmentIdsL.Length == 3)
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //    segmentIdAtIndex2 = segmentIdsL[2];

            //    FarebasisDataL0 = FarebasisL[0];
            //    FarebasisDataL1 = FarebasisL[1];
            //    FarebasisDataL2 = FarebasisL[2];

            //}
            //else if (segmentIdsL.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];

            //    FarebasisDataL0 = FarebasisL[0];
            //    FarebasisDataL1 = FarebasisL[1];
            //}
            //else
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    FarebasisDataL0 = FarebasisL[0];
            //}
            //foreach (var segment in AirfaredataL.segments)
            //{
            //    if (legKeyCounter == 0)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex0;

            //        FarebasisDataL = FarebasisDataL0;
            //    }
            //    else if (legKeyCounter == 1)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex1;
            //        FarebasisDataL = FarebasisDataL1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex2;
            //        FarebasisDataL = FarebasisDataL2;
            //    }
            //    fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
            //    //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
            //    fareRepriceReq.Append("<PermittedBookingCodes>");
            //    fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
            //    //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
            //    fareRepriceReq.Append("</PermittedBookingCodes>");
            //    fareRepriceReq.Append("</AirSegmentPricingModifiers>");
            //    legKeyCounter++;
            //}
            //fareRepriceReq.Append("</AirPricingCommand>");
            //fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            //fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, SeatMapReq.ToString(), _userName, _password);

            //// Load XML into XmlDocument
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(res);

            // Convert XML to JSON
            //string json = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);

            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(SeatMapReq.ToString(), "3-GetSeatMapReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "2-GetSeatMapRes", "GDSOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p == 0)
                {
                    logs.WriteLogsR(SeatMapReq.ToString(), "3-GetSeatMapReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "2-GetSeatMapRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(SeatMapReq.ToString(), "3-GetSeatMapReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "2-GetSeatMapRes_Right", "GDSRT");
                }
            }
            return res;
        }

        public string AirPriceGetRT(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, dynamic AirfaredataL, dynamic AirfaredataR, string _AirlineWay)
        {

            int count = 0;
            int countR = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;
            int legKeyCounterR = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do Left
            string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
            }


            //Right
            string segmentIdDataR = AirfaredataR.SegmentidRightdata;
            string[] segmentIdsR = segmentIdDataR.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string segmentIdAtIndexR0 = string.Empty;
            string segmentIdAtIndexR1 = string.Empty;
            string segmentIdAtIndexR2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsR.Length == 3)
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
                segmentIdAtIndexR2 = segmentIdsR[2];

            }
            else if (segmentIdsR.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
            }
            else
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
            }

            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                count++;
            }

            foreach (var segment in AirfaredataR.segments)
            {
                if (countR == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR0;
                }
                else if (countR == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                countR++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Yes\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"10\"/>");
                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"01\"/>");
                    }
                }
            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                    }
                }



                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"10\"/>");
                    }
                }
                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"01\"/>");
                    }
                }




            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
            }
            foreach (var segment in AirfaredataL.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }

            if (segmentIdsR.Length == 3)
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
                segmentIdAtIndexR2 = segmentIdsR[2];

            }
            else if (segmentIdsR.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
            }
            else
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
            }
            foreach (var segment in AirfaredataR.segments)
            {
                if (legKeyCounterR == 0)
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR0;
                }
                else if (legKeyCounterR == 1)
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR1;
                }
                else
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounterR++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
            }
            return res;
        }

        public string AirPriceGetRT_V2(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, SimpleAvailibilityaAddResponce AirfaredataR, string farebasisdataL, string farebasisdataR, string _AirlineWay)
        {

            int count = 0;
            int countR = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;
            int legKeyCounterR = 0;

            fareRepriceReq = new StringBuilder();

            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            //fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\"  AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\">");//According to demo
            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do Left
            string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            string FarebasisDataL = farebasisdataL;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }


            //Right
            string segmentIdDataR = AirfaredataR.SegmentidRightdata;
            string FarebasisDataR = farebasisdataR;
            string[] segmentIdsR = segmentIdDataR.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisR = FarebasisDataR.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataR0 = string.Empty;
            string FarebasisDataR1 = string.Empty;
            string FarebasisDataR2 = string.Empty;

            string segmentIdAtIndexR0 = string.Empty;
            string segmentIdAtIndexR1 = string.Empty;
            string segmentIdAtIndexR2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsR.Length == 3)
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
                segmentIdAtIndexR2 = segmentIdsR[2];

                FarebasisDataR0 = FarebasisR[0];
                FarebasisDataR1 = FarebasisR[1];
                FarebasisDataR2 = FarebasisR[2];

            }
            else if (segmentIdsR.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];

                FarebasisDataR0 = FarebasisR[0];
                FarebasisDataR1 = FarebasisR[1];
            }
            else
            {
                segmentIdAtIndexR0 = segmentIdsR[0];

                FarebasisDataR0 = FarebasisR[0];
            }

            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                count++;
            }

            foreach (var segment in AirfaredataR.segments)
            {
                if (countR == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR0;
                }
                else if (countR == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                countR++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            //fareRepriceReq.Append("<AirPricingModifiers  InventoryRequestType=\"DirectAccess\">");//According to demo
            fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"1\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"11\"/>");
                        paxCount++;
                    }
                }





            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }
            foreach (var segment in AirfaredataL.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;

                    FarebasisDataL = FarebasisDataL0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                    FarebasisDataL = FarebasisDataL1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                    FarebasisDataL = FarebasisDataL2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
                //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }

            if (segmentIdsR.Length == 3)
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
                segmentIdAtIndexR2 = segmentIdsR[2];

                FarebasisDataR0 = FarebasisR[0];
                FarebasisDataR1 = FarebasisR[1];
                FarebasisDataR2 = FarebasisR[2];

            }
            else if (segmentIdsR.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];

                FarebasisDataR0 = FarebasisR[0];
                FarebasisDataR1 = FarebasisR[1];
            }
            else
            {
                segmentIdAtIndexR0 = segmentIdsR[0];

                FarebasisDataR0 = FarebasisR[0];
            }
            foreach (var segment in AirfaredataR.segments)
            {
                if (legKeyCounterR == 0)
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR0;
                    FarebasisDataR = FarebasisR[0];
                }
                else if (legKeyCounterR == 1)
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR1;
                    FarebasisDataR = FarebasisR[1];
                }
                else
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR2;
                    FarebasisDataR = FarebasisR[2];
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\" FareBasisCode=\"" + FarebasisDataR + "\">");
                //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\"\">");

                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounterR++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");





            #region old
            //fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            //fareRepriceReq.Append("<soap:Body>");

            //fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            //fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            //fareRepriceReq.Append("<AirItinerary>");
            ////< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            //// to do Left
            //string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            //string FarebasisDataL = AirfaredataL.FareBasisLeftdata;
            //string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            //string segmentIdAtIndex0 = string.Empty;
            //string segmentIdAtIndex1 = string.Empty;
            //string segmentIdAtIndex2 = string.Empty;
            //// Checking if the array has at least two elements
            //if (segmentIdsL.Length == 3)
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //    segmentIdAtIndex2 = segmentIdsL[2];

            //}
            //else if (segmentIdsL.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //}
            //else
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //}


            ////Right
            //string segmentIdDataR = AirfaredataR.SegmentidRightdata;
            //string FarebasisDataR = AirfaredataR.FareBasisRightdata;
            //string[] segmentIdsR = segmentIdDataR.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            //string segmentIdAtIndexR0 = string.Empty;
            //string segmentIdAtIndexR1 = string.Empty;
            //string segmentIdAtIndexR2 = string.Empty;
            //// Checking if the array has at least two elements
            //if (segmentIdsR.Length == 3)
            //{
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //    segmentIdAtIndexR1 = segmentIdsR[1];
            //    segmentIdAtIndexR2 = segmentIdsR[2];

            //}
            //else if (segmentIdsR.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //    segmentIdAtIndexR1 = segmentIdsR[1];
            //}
            //else
            //{
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //}

            //foreach (var segment in AirfaredataL.segments)
            //{
            //    if (count == 0)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex0;
            //    }
            //    else if (count == 1)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex2;
            //    }
            //    fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
            //    fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
            //    fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
            //    fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
            //    fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
            //    fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
            //    fareRepriceReq.Append("<Connection />");
            //    fareRepriceReq.Append("</AirSegment>");
            //    count++;
            //}

            //foreach (var segment in AirfaredataR.segments)
            //{
            //    if (countR == 0)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndexR0;
            //    }
            //    else if (countR == 1)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndexR1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndexR2;
            //    }
            //    fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
            //    fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
            //    fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
            //    fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
            //    fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
            //    fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
            //    fareRepriceReq.Append("<Connection />");
            //    fareRepriceReq.Append("</AirSegment>");
            //    countR++;
            //}

            //fareRepriceReq.Append("</AirItinerary>");
            //fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Yes\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            //fareRepriceReq.Append("<BrandModifiers>");
            //fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            //fareRepriceReq.Append("</BrandModifiers>");
            //fareRepriceReq.Append("</AirPricingModifiers>");
            //if (_GetfligthModel.passengercount != null)
            //{
            //    if (_GetfligthModel.passengercount.adultcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.passengercount.childcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"10\"/>");
            //        }
            //    }
            //    if (_GetfligthModel.passengercount.infantcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"01\"/>");
            //        }
            //    }
            //}
            //else
            //{

            //    if (_GetfligthModel.adultcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.adultcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
            //        }
            //    }



            //    if (_GetfligthModel.childcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.childcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"10\"/>");
            //        }
            //    }
            //    if (_GetfligthModel.infantcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.infantcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"01\"/>");
            //        }
            //    }




            //}
            //fareRepriceReq.Append("<AirPricingCommand>");
            //if (segmentIdsL.Length == 3)
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //    segmentIdAtIndex2 = segmentIdsL[2];

            //}
            //else if (segmentIdsL.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //}
            //else
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //}
            //foreach (var segment in AirfaredataL.segments)
            //{
            //    if (legKeyCounter == 0)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex0;
            //    }
            //    else if (legKeyCounter == 1)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex2;
            //    }
            //    fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
            //    //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
            //    fareRepriceReq.Append("<PermittedBookingCodes>");
            //    fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
            //    fareRepriceReq.Append("</PermittedBookingCodes>");
            //    fareRepriceReq.Append("</AirSegmentPricingModifiers>");
            //    legKeyCounter++;
            //}

            //if (segmentIdsR.Length == 3)
            //{
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //    segmentIdAtIndexR1 = segmentIdsR[1];
            //    segmentIdAtIndexR2 = segmentIdsR[2];

            //}
            //else if (segmentIdsR.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //    segmentIdAtIndexR1 = segmentIdsR[1];
            //}
            //else
            //{
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //}
            //foreach (var segment in AirfaredataR.segments)
            //{
            //    if (legKeyCounterR == 0)
            //    {
            //        segmentIdAtIndexR0 = segmentIdAtIndexR0;
            //    }
            //    else if (legKeyCounterR == 1)
            //    {
            //        segmentIdAtIndexR0 = segmentIdAtIndexR1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndexR0 = segmentIdAtIndexR2;
            //    }
            //    fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\" FareBasisCode=\"" + FarebasisDataR + "\">");
            //    //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\"\">");

            //    fareRepriceReq.Append("<PermittedBookingCodes>");
            //    fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
            //    fareRepriceReq.Append("</PermittedBookingCodes>");
            //    fareRepriceReq.Append("</AirSegmentPricingModifiers>");
            //    legKeyCounterR++;
            //}
            //fareRepriceReq.Append("</AirPricingCommand>");
            //fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            //fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");
            #endregion

            //For certification Request
            //1) change the traceid i.e same as LowFare
            //2) airsegment from lowfareresponse
            //3) providercode="1G"
            //4) check segmentif and farebasiscode
            //5) Traceid will be same in all request further

            //string resp = string.Empty;
            //string path = "D:\\pcheck.txt";
            //using (StreamReader reader = new StreamReader(path))
            //{
            //resp = reader.ReadToEnd(); // Reads the entire file content into a string
            //}
            //fareRepriceReq = new StringBuilder();
            //fareRepriceReq.Append(resp);

            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirprice", "SameGDSRT");
            }
            return res;
        }

        public string AirPriceGet_old(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, dynamic Airfaredata, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do
            string segmentIdData = Airfaredata.Segmentiddata;
            string[] segmentIds = segmentIdData.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIds.Length == 3)
            {
                segmentIdAtIndex0 = segmentIds[0];
                segmentIdAtIndex1 = segmentIds[1];
                segmentIdAtIndex2 = segmentIds[2];

            }
            else if (segmentIds.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIds[0];
                segmentIdAtIndex1 = segmentIds[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIds[0];
            }


            foreach (var segment in Airfaredata.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" />");
                count++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            fareRepriceReq.Append("<AirPricingModifiers InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" Key=\"" + paxCount + "\"/>");
                        paxCount++;
                    }
                }
                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" Key=\"" + paxCount + "\" Age=\"10\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  Key=\"" + paxCount + "\" Age=\"01\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  Key=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + paxCount + "\" Code=\"CNN\" Age=\"10\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + paxCount + "\" Code=\"INF\" Age=\"01\"/>");
                        paxCount++;
                    }
                }


            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIds.Length == 3)
            {
                segmentIdAtIndex0 = segmentIds[0];
                segmentIdAtIndex1 = segmentIds[1];
                segmentIdAtIndex2 = segmentIds[2];
            }
            else if (segmentIds.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIds[0];
                segmentIdAtIndex1 = segmentIds[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIds[0];
            }
            foreach (var segment in Airfaredata.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
            }
            return res;
        }
        public string CreatePNRRoundTrip(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, int p1, List<string> _unitkey, List<string> _SSRkey, string? _pricesolution = null)
        {

            int count = 0;
            int icount = 100;

            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));

            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                int _id = 0;
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    string[] subParts = new string[2];
                    string[] _parts = passengerdetails[i].passengercombinedkey.Split('@');
                    for (int a = 0; a < _parts.Length; a++)
                    {
                        // Check if the part contains "Airasia" or "AirIndia"
                        if (_parts[a].ToLower().Trim().Contains("airindia"))
                        {
                            // Split the part at '^' and get the value before '^'
                            subParts = _parts[a].Split('^');
                        }
                    }
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + subParts[0] + "\"  TravelerType=\"ADT\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + subParts[0] + "\"  TravelerType=\"CNN\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + subParts[0] + "\" TravelerType=\"INF\">");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + subParts[0] + "\"  TravelerType=\"ADT\">");
                    }


                    //Title
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                    }
                    else
                    {
                        passengerdetails[i].title = "MSTR";
                    }
                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                        int seg = 0;

                        foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                        {

                            if (_SSRkey.Count > _id)
                            {
                                ssrsegmentwise _obj = new ssrsegmentwise();
                                _obj.SSRcodeOneWayI = new List<ssrsKey>();
                                _obj.SSRcodeOneWayII = new List<ssrsKey>();
                                _obj.SSRcodeRTI = new List<ssrsKey>();
                                _obj.SSRcodeRTII = new List<ssrsKey>();

                                for (int k = 0; k < _SSRkey.Count; k++)
                                {
                                    if (_SSRkey[k].Contains("_OneWay0") && _SSRkey[k].ToLower().Trim().Contains("airindia"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj0 = new ssrsKey();
                                            _obj0.key = _SSRkey[k];
                                            _obj.SSRcodeOneWayI.Add(_obj0);
                                        }

                                    }
                                    else if (_SSRkey[k].Contains("_OneWay1") && _SSRkey[k].ToLower().Trim().Contains("airindia"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj1 = new ssrsKey();
                                            _obj1.key = _SSRkey[k];
                                            _obj.SSRcodeOneWayII.Add(_obj1);
                                        }
                                    }
                                    else if (_SSRkey[k].Contains("_RT0") && _SSRkey[k].ToLower().Trim().Contains("airindia"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj2 = new ssrsKey();
                                            _obj2.key = _SSRkey[k];
                                            _obj.SSRcodeRTI.Add(_obj2);
                                        }
                                    }
                                    else if (_SSRkey[k].Contains("_RT1") && _SSRkey[k].ToLower().Trim().Contains("airindia"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj3 = new ssrsKey();
                                            _obj3.key = _SSRkey[k];
                                            _obj.SSRcodeRTII.Add(_obj3);
                                        }
                                    }
                                }


                                for (int k = 0; k < _obj.SSRcodeOneWayI.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeOneWayI[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);
                                    if (p1 == 0 && (_obj.SSRcodeOneWayI[k].key.Contains("_OneWay0") && seg == 0) && _obj.SSRcodeOneWayI[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)// || _SSRkey[_id].Contains("_OneWay1")))
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeOneWayI[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeOneWayII.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeOneWayII[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);
                                    if (p1 == 0 && (_obj.SSRcodeOneWayII[k].key.Contains("_OneWay1") && seg == 1) && _obj.SSRcodeOneWayII[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeOneWayII[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeRTI.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeRTI[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);

                                    if (p1 == 1 && (_obj.SSRcodeRTI[k].key.Contains("_RT0") && seg == 0) && _obj.SSRcodeRTI[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)//|| _SSRkey[_id].Contains("_RT1")))
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeRTI[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeRTII.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeRTII[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);

                                    if (p1 == 1 && (_obj.SSRcodeRTII[k].key.Contains("_RT1") && seg == 1) && _obj.SSRcodeRTII[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)//|| _SSRkey[_id].Contains("_RT1")))
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeRTII[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }

                            }
                            seg++;
                        }
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }
                    if (i == 0 && passengerdetails[i].passengertypecode == "ADT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mr")
                        {

                            //For Hardcoded DOB
                            /////30JAN98/M//Test/Umesh
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/01jan00/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////01jan00/M//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");


                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/01jan00/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////01jan00/F//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        //Domestic
                        //createPNRReq.Append("<Address>");
                        //createPNRReq.Append("<AddressName>Home</AddressName>");
                        //createPNRReq.Append("<Street>20th I Cross</Street>");
                        //createPNRReq.Append("<City>Bangalore</City>");
                        //createPNRReq.Append("<State>KA</State>");
                        //createPNRReq.Append("<PostalCode>560047</PostalCode>");
                        //createPNRReq.Append("<Country>IN</Country>");
                        //createPNRReq.Append("</Address>");
                        //International
                        createPNRReq.Append("<Address>");
                        createPNRReq.Append("<AddressName>DemoSiteAddress</AddressName>");
                        createPNRReq.Append("<Street>Via Augusta 59 5</Street>");
                        createPNRReq.Append("<City>Delhi</City>");
                        createPNRReq.Append("<State>DL</State>");
                        createPNRReq.Append("<PostalCode>111001</PostalCode>");
                        createPNRReq.Append("<Country>IN</Country>");
                        createPNRReq.Append("</Address>");

                    }
                    if (passengerdetails[i].passengertypecode == "CNN" || passengerdetails[i].passengertypecode == "CHD")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/01jan00/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////01jan00/M//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/01jan00/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////01jan00/F//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>P-C11 DOB11Dec13</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    string format = "11DEC23";// Convert.ToDateTime(passengerdetails[i].dateOfBirth).ToString("ddMMMyy");
                    if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/MI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////11DEC23/MI//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/FI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////11DEC23/FI//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>" + format + "</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                createPNRReq.Append("<ContinuityCheckOverride xmlns=\"http://www.travelport.com/schema/common_v52_0\">true</ContinuityCheckOverride>");
                createPNRReq.Append("<AgencyContactInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
                createPNRReq.Append("<PhoneNumber CountryCode=\"91\" AreaCode=\"011\" Number=\"46615790\" Location=\"DEL\" Type=\"Agency\"/>");
                createPNRReq.Append("</AgencyContactInfo>");
                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                Getdetails.PriceSolution = Getdetails.PriceSolution.Replace("</air:CancelPenalty>", "</air:CancelPenalty><air:AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\"> </air:AirPricingModifiers>");
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");

                #region seat
                int idx = 0;
                int _seg = 0;
                foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                {
                    List<string> oneway0List = _unitkey.Where(x => x.Contains("_OneWay0")).ToList();
                    List<string> oneway1List = _unitkey.Where(x => x.Contains("_OneWay1")).ToList();
                    List<string> rt0List = _unitkey.Where(x => x.Contains("_RT0")).ToList();
                    List<string> rt1List = _unitkey.Where(x => x.Contains("_RT1")).ToList();
                    string adultkey = string.Empty;
                    for (int a = 0; a < oneway0List.Count; a++)
                    {
                        if (oneway0List[a].Split('_')[0].Trim() == "0")
                        {
                            if (p1 == 0 && _seg == 0 && (oneway0List[a].Contains("_OneWay0")))
                            {
                                string before_at = passengerdetails[a].passengerkey.Split('@')[0];

                                adultkey = before_at.Split('^')[0];

                                string[] unitsubKey2 = oneway0List[a].Split('_');
                                string pas_unitKey = unitsubKey2[1];
                                //createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                //createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + passengerdetails[a].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + adultkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                //break;
                            }
                        }
                    }
                    for (int a = 0; a < oneway1List.Count; a++)
                    {
                        if (oneway1List[a].Split('_')[0].Trim() == "0")
                        {
                            if (p1 == 0 && _seg == 1 && (oneway1List[a].Contains("_OneWay1")))
                            {
                                string before_at = passengerdetails[a].passengerkey.Split('@')[1];
                                adultkey = before_at.Split('^')[0];

                                string[] unitsubKey2 = oneway1List[a].Split('_');
                                //string pas_unitKey = unitsubKey2[1].Insert(unitsubKey2[1].Length - 2, "-");
                                string pas_unitKey = unitsubKey2[1];
                                //createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + adultkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                //break;
                            }
                        }
                    }
                    for (int a = 0; a < rt0List.Count; a++)
                    {
                        if (rt0List[a].Split('_')[0].Trim() == "0")
                        {
                            if (p1 == 1 && _seg == 0 && (rt0List[a].Contains("_RT0")))
                            {
                                string before_at = passengerdetails[a].passengerkey.Split('@')[0];
                                adultkey = before_at.Split('^')[0];

                                string[] unitsubKey2 = rt0List[a].Split('_');
                                string pas_unitKey = unitsubKey2[1];
                                //createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + adultkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                //break;
                            }
                        }
                    }
                    for (int a = 0; a < rt1List.Count; a++)
                    {
                        if (rt1List[a].Split('_')[0].Trim() == "0")
                        {
                            if (p1 == 1 && _seg == 1 && (rt1List[a].Contains("_RT1")))
                            {
                                string before_at = passengerdetails[a].passengerkey.Split('@')[1];
                                adultkey = before_at.Split('^')[0];

                                string[] unitsubKey2 = rt1List[a].Split('_');
                                string pas_unitKey = unitsubKey2[1];
                                //createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + adultkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                //break;
                            }
                        }
                    }
                    _seg++;
                }
                #endregion

                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }
            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs(createPNRReq.ToString(), "3-GetPNRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "3-GetPNRRes", "GDSOneWay", "oneway");
            }
            else
            {
                if (p1 == 0)
                {
                    logs.WriteLogsR(createPNRReq.ToString(), "3-GetPNRReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "3-GetPNRRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(createPNRReq.ToString(), "3-GetPNRReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "3-GetPNRRes_Right", "GDSRT");
                }

            }
            return res;
        }

        public string CreatePNRRoundTrip_bkpnonstop(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, int p1, List<string> _unitkey, List<string> _SSRkey, string? _pricesolution = null)
        {

            int count = 0;
            int icount = 100;
            //int paxCount = 0;
            //int legcount = 0;
            //string origin = string.Empty;
            //int legKeyCounter = 0;

            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                int _id = 0;
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    string[] subParts = new string[2];
                    string[] _parts = passengerdetails[i].passengercombinedkey.Split('@');
                    for (int a = 0; a < _parts.Length; a++)
                    {
                        // Check if the part contains "Airasia" or "AirIndia"
                        if (_parts[a].ToLower().Trim().Contains("airindia"))
                        {
                            // Split the part at '^' and get the value before '^'
                            subParts = _parts[a].Split('^');
                        }
                    }
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + subParts[0] + "\"  TravelerType=\"ADT\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + subParts[0] + "\"  TravelerType=\"CNN\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + subParts[0] + "\" TravelerType=\"INF\">");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + subParts[0] + "\"  TravelerType=\"ADT\">");
                    }


                    //Title
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                    }
                    else
                    {
                        passengerdetails[i].title = "MSTR";
                    }
                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                        int seg = 0;

                        foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                        {

                            if (_SSRkey.Count > _id)
                            {
                                ssrsegmentwise _obj = new ssrsegmentwise();
                                _obj.SSRcodeOneWayI = new List<ssrsKey>();
                                _obj.SSRcodeOneWayII = new List<ssrsKey>();
                                _obj.SSRcodeRTI = new List<ssrsKey>();
                                _obj.SSRcodeRTII = new List<ssrsKey>();

                                for (int k = 0; k < _SSRkey.Count; k++)
                                {
                                    if (_SSRkey[k].Contains("_OneWay0") && _SSRkey[k].ToLower().Trim().Contains("airindia"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj0 = new ssrsKey();
                                            _obj0.key = _SSRkey[k];
                                            _obj.SSRcodeOneWayI.Add(_obj0);
                                        }

                                    }
                                    else if (_SSRkey[k].Contains("_OneWay1") && _SSRkey[k].ToLower().Trim().Contains("airindia"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj1 = new ssrsKey();
                                            _obj1.key = _SSRkey[k];
                                            _obj.SSRcodeOneWayII.Add(_obj1);
                                        }
                                    }
                                    else if (_SSRkey[k].Contains("_RT0") && _SSRkey[k].ToLower().Trim().Contains("airindia"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj2 = new ssrsKey();
                                            _obj2.key = _SSRkey[k];
                                            _obj.SSRcodeRTI.Add(_obj2);
                                        }
                                    }
                                    else if (_SSRkey[k].Contains("_RT1") && _SSRkey[k].ToLower().Trim().Contains("airindia"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj3 = new ssrsKey();
                                            _obj3.key = _SSRkey[k];
                                            _obj.SSRcodeRTII.Add(_obj3);
                                        }
                                    }
                                }


                                for (int k = 0; k < _obj.SSRcodeOneWayI.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeOneWayI[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);
                                    if (p1 == 0 && (_obj.SSRcodeOneWayI[k].key.Contains("_OneWay0") && seg == 0) && _obj.SSRcodeOneWayI[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)// || _SSRkey[_id].Contains("_OneWay1")))
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeOneWayI[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeOneWayII.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeOneWayII[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);
                                    if (p1 == 0 && (_obj.SSRcodeOneWayII[k].key.Contains("_OneWay1") && seg == 1) && _obj.SSRcodeOneWayII[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeOneWayII[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeRTI.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeRTI[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);

                                    if (p1 == 1 && (_obj.SSRcodeRTI[k].key.Contains("_RT0") && seg == 0) && _obj.SSRcodeRTI[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)//|| _SSRkey[_id].Contains("_RT1")))
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeRTI[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeRTII.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeRTII[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);

                                    if (p1 == 1 && (_obj.SSRcodeRTII[k].key.Contains("_RT1") && seg == 1) && _obj.SSRcodeRTII[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)//|| _SSRkey[_id].Contains("_RT1")))
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeRTII[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }

                            }
                            seg++;
                        }
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                        //foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                        //{
                        //    string[] unitsubKey2 = _SSRkey[i].Split('_');
                        //    string pas_unitKey = unitsubKey2[0];
                        //    createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                        //}
                    }

                    //if (!String.IsNullOrEmpty(paxDetail.FrequentFlierNumber) && paxDetail.FrequentFlierNumber.Length > 5)
                    //{
                    //if (segment_.Bonds[0].Legs[0].AirlineName.Equals("UK"))
                    //{
                    //createPNRReq.Append("<SSR  Key='" + count + "' Type='FQTV' Status='HK' Carrier='UK' FreeText='" + paxDetail.FrequentFlierNumber + "-" + paxDetail.LastName + "/" + paxDetail.FirstName + "" + paxDetail.Title.ToUpper() + "'/>");
                    //}
                    //else
                    //{
                    //  createPNRReq.Append("<com:LoyaltyCard SupplierCode='" + segment_.Bonds[0].Legs[0].AirlineName + "' CardNumber='" + paxDetail.FrequentFlierNumber + "'/>");
                    //}
                    //}
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    if (i == 0 && passengerdetails[i].passengertypecode == "ADT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/03Dec06/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                            //foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                            //{
                            //    string[] unitsubKey2 = _SSRkey[i].Split('_');
                            //    string pas_unitKey = unitsubKey2[0];
                            //    createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                            //}
                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/03Dec06/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                            //{
                            //    string[] unitsubKey2 = _SSRkey[i].Split('_');
                            //    string pas_unitKey = unitsubKey2[0];
                            //    createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                            //}
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        //Domestic
                        //createPNRReq.Append("<Address>");
                        //createPNRReq.Append("<AddressName>Home</AddressName>");
                        //createPNRReq.Append("<Street>20th I Cross</Street>");
                        //createPNRReq.Append("<City>Bangalore</City>");
                        //createPNRReq.Append("<State>KA</State>");
                        //createPNRReq.Append("<PostalCode>560047</PostalCode>");
                        //createPNRReq.Append("<Country>IN</Country>");
                        //createPNRReq.Append("</Address>");
                        //International
                        createPNRReq.Append("<Address>");
                        createPNRReq.Append("<AddressName>DemoSiteAddress</AddressName>");
                        createPNRReq.Append("<Street>Via Augusta 59 5</Street>");
                        createPNRReq.Append("<City>Delhi</City>");
                        createPNRReq.Append("<State>DL</State>");
                        createPNRReq.Append("<PostalCode>111001</PostalCode>");
                        createPNRReq.Append("<Country>IN</Country>");
                        createPNRReq.Append("</Address>");

                    }
                    if (passengerdetails[i].passengertypecode == "CNN" || passengerdetails[i].passengertypecode == "CHD")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11Dec13/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                            //{
                            //    string[] unitsubKey2 = _SSRkey[i].Split('_');
                            //    string pas_unitKey = unitsubKey2[0];
                            //    createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                            //}

                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11Dec13/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                            //{
                            //    string[] unitsubKey2 = _SSRkey[i].Split('_');
                            //    string pas_unitKey = unitsubKey2[0];
                            //    createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                            //}
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>P-C11 DOB11Dec13</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    string format = "11DEC23";// Convert.ToDateTime(passengerdetails[i].dateOfBirth).ToString("ddMMMyy");
                    if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/MI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //int indx = 0;
                            //foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                            //{
                            //    // Ensure each adult/child gets a unique seat per segment
                            //    foreach (Match mitem in Regex.Matches(Getdetails.PriceSolution, "PassengerType BookingTravelerRef=\'(?<Travllerref>[\\s\\S]*?)\'\\s*Code=\'(?<PaxType>[\\s\\S]*?)'", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                            //    {
                            //        if (mitem.Groups["PaxType"].Value == "INF" || mitem.Groups["PaxType"].Value == "INFT")
                            //        {
                            //            string[] unitsubKey2 = _SSRkey[indx].Split('_');
                            //            string pas_unitKey = unitsubKey2[0];
                            //            createPNRReq.Append("<SSR Type=\"BLML\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                            //            indx++;
                            //        }
                            //    }
                            //}
                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/FI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //int indx = 0;
                            //foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                            //{
                            //    // Ensure each adult/child gets a unique seat per segment
                            //    foreach (Match mitem in Regex.Matches(Getdetails.PriceSolution, "PassengerType BookingTravelerRef=\'(?<Travllerref>[\\s\\S]*?)\'\\s*Code=\'(?<PaxType>[\\s\\S]*?)'", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                            //    {
                            //        if (mitem.Groups["PaxType"].Value == "INF" || mitem.Groups["PaxType"].Value == "INFT")
                            //        {
                            //            string[] unitsubKey2 = _SSRkey[indx].Split('_');
                            //            string pas_unitKey = unitsubKey2[0];
                            //            createPNRReq.Append("<SSR Type=\"BLML\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                            //            indx++;
                            //        }
                            //    }
                            //}
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>" + format + "</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                //createPNRReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
                //createPNRReq.Append("</AirPricingModifiers>");
                createPNRReq.Append("<ContinuityCheckOverride xmlns=\"http://www.travelport.com/schema/common_v52_0\">true</ContinuityCheckOverride>");
                createPNRReq.Append("<AgencyContactInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
                createPNRReq.Append("<PhoneNumber CountryCode=\"91\" AreaCode=\"011\" Number=\"46615790\" Location=\"DEL\" Type=\"Agency\"/>");
                createPNRReq.Append("</AgencyContactInfo>");
                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                //createPNRReq.Append(Getdetails.PriceSolution.Replace("</air:CancelPenalty>","</air:CancelPenalty><air:AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\"> </air:AirPricingModifiers>"));
                Getdetails.PriceSolution = Getdetails.PriceSolution.Replace("</air:CancelPenalty>", "</air:CancelPenalty><air:AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\"> </air:AirPricingModifiers>");

                // Define the regex pattern to match any BookingTravelerRef value dynamically
                //string pattern = @"<air:PassengerType BookingTravelerRef='(\d+)' Code='INF' Age='(\d+)'/>";
                //// Define the replacement string (maintaining the BookingTravelerRef dynamically)
                //string replacement = @"<air:PassengerType BookingTravelerRef='$1' Code='INF' PricePTCOnly=""true"" Age='$1'/>";
                // Perform the replacement
                //Getdetails.PriceSolution = Regex.Replace(Getdetails.PriceSolution, pattern, replacement);
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");
                //if (passengerdetails.Count > 0)
                //{
                //    for (int i = 0; i < passengerdetails.Count; i++)
                //    {
                //        if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                //        {
                //            continue;
                //        }
                //        for (int j = 0; j < Getdetails.segmentid.Length; j++)
                //        {
                //            string [] unitsubKey2 = _unitkey[i].Split('_');
                //            string pas_unitKey = unitsubKey2[1];
                //            createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + passengerdetails[i].passengerkey + "\" SegmentRef=\"" + Getdetails.segmentid[j].Trim() + "\" SeatId=\"" + pas_unitKey + "\"/>");
                //        }

                //    }
                //}
                //if (passengerdetails.Count > 0)
                //{
                //    int adultIndex = 0;
                //    int childIndex = 0;

                //    // Loop through the passengers to assign seats, skipping infants
                //    for (int i = 0; i < passengerdetails.Count; i++)
                //    {
                //        if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                //        {
                //            // Skip infants
                //            continue;
                //        }

                //        // If we are processing an adult or child, assign seats
                //        for (int j = 0; j < Getdetails.segmentid.Length; j++)
                //        {
                //            // Ensure each adult/child gets a unique seat per segment
                //            string[] unitsubKey2 = _unitkey[i].Split('_');
                //            string pas_unitKey = unitsubKey2[1];

                //            // Assign the seat based on segment id and passenger type
                //            if (passengerdetails[i].passengertypecode == "ADT") // Adult
                //            {
                //                createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + passengerdetails[i].passengerkey + "\" SegmentRef=\"" + Getdetails.segmentid[j].Trim() + "\" SeatId=\"" + pas_unitKey + "\"/>");
                //                adultIndex++;
                //            }
                //            else if (passengerdetails[i].passengertypecode == "CHD") // Child
                //            {
                //                createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + passengerdetails[i].passengerkey + "\" SegmentRef=\"" + Getdetails.segmentid[j].Trim() + "\" SeatId=\"" + pas_unitKey + "\"/>");
                //                childIndex++;
                //            }
                //        }
                //    }
                //}

                #region seat
                int idx = 0;
                int _seg = 0;
                foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                {

                    // Ensure each adult/child gets a unique seat per segment
                    foreach (Match mitem in Regex.Matches(Getdetails.PriceSolution, "PassengerType BookingTravelerRef=\'(?<Travllerref>[\\s\\S]*?)\'\\s*Code=\'(?<PaxType>[\\s\\S]*?)'", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    {
                        //idx = 0;
                        if (mitem.Groups["PaxType"].Value == "INFT" || mitem.Groups["PaxType"].Value == "INF")
                        {  //idx++;
                            continue;
                        }


                        if (_unitkey.Count > 0)
                        {
                            for (int a = 0; a < _unitkey.Count; a++)
                            {
                                if (idx < _unitkey.Count)
                                {
                                    if (p1 == 0 && _seg == 0 && (_unitkey[idx].Contains("_OneWay0") || _unitkey[idx].Contains("_OneWay1")))
                                    {
                                        string[] unitsubKey2 = _unitkey[idx].Split('_');
                                        string pas_unitKey = unitsubKey2[1];
                                        createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                        break;
                                    }
                                    else if (p1 == 0 && _seg == 1 && (_unitkey[idx].Contains("_OneWay0") || _unitkey[idx].Contains("_OneWay1")))
                                    {
                                        string[] unitsubKey2 = _unitkey[idx].Split('_');
                                        string pas_unitKey = unitsubKey2[1];
                                        createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                        break;
                                    }
                                    else if (p1 == 1 && _seg == 0 && (_unitkey[idx].Contains("_RT0") || _unitkey[idx].Contains("_RT1")))
                                    {
                                        string[] unitsubKey2 = _unitkey[idx].Split('_');
                                        string pas_unitKey = unitsubKey2[1];
                                        createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                        break;
                                    }
                                    else if (p1 == 1 && _seg == 1 && (_unitkey[idx].Contains("_RT0") || _unitkey[idx].Contains("_RT1")))
                                    {
                                        string[] unitsubKey2 = _unitkey[idx].Split('_');
                                        string pas_unitKey = unitsubKey2[1];
                                        createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                        break;
                                    }
                                    else
                                    {
                                        //continue;
                                    }
                                }
                                idx++;
                            }


                        }
                        idx++;
                    }
                    _seg++;
                }
                #endregion

                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }
            //}
            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            //SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            //SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));
            //if (_AirlineWay.ToLower() == "gdsoneway")
            //{
            //    logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "GDSOneWay");
            //}
            //else
            //{
            //    logs.WriteLogsR("Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "SameGDSRT");
            //}
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(createPNRReq.ToString(), "3-GetPNRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "3-GetPNRRes", "GDSOneWay", "oneway");
            }
            else
            {
                //    //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p1 == 0)
                {
                    logs.WriteLogsR(createPNRReq.ToString(), "3-GetPNRReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "3-GetPNRRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(createPNRReq.ToString(), "3-GetPNRReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "3-GetPNRRes_Right", "GDSRT");
                }
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");

            }
            return res;
        }
        public string GetAirMerchandisingOfferAvailabilityReq(string _testURL, StringBuilder createBaggageReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _AirlineWay, string _pricesolution)
        {

            int count = 0;
            int icount = 100;
            string res = string.Empty;
            createBaggageReq = new StringBuilder();
            try
            {
                createBaggageReq.Append("<soap:Envelope xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                createBaggageReq.Append("<soap:Body>");
                createBaggageReq.Append("<AirMerchandisingOfferAvailabilityReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\">");
                createBaggageReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"uAPI\"/>");
                createBaggageReq.Append("<AirSolution xmlns=\"http://www.travelport.com/schema/air_v52_0\">");
                List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));
                AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
                Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

                if (passengerdetails.Count > 0)
                {
                    int _id = 0;
                    for (int i = 0; i < passengerdetails.Count; i++)
                    {
                        if (passengerdetails[i].passengertypecode == "ADT")
                        {
                            passengerdetails[i].title = "MR";
                            createBaggageReq.Append("<SearchTraveler xmlns=\"http://www.travelport.com/schema/air_v52_0\" Code=\"ADT\" Gender=\"M\" BookingTravelerRef=\"" + passengerdetails[i].passengerkey + "\">");
                            createBaggageReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" First=\"" + passengerdetails[i].first + "\"  Last=\"" + passengerdetails[i].last + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\"/>");
                            createBaggageReq.Append("</SearchTraveler>");
                        }
                        else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                        {
                            passengerdetails[i].title = "MSTR";
                            createBaggageReq.Append("<SearchTraveler xmlns=\"http://www.travelport.com/schema/air_v52_0\" Code=\"CNN\" Gender=\"M\" BookingTravelerRef=\"" + passengerdetails[i].passengerkey + "\">");
                            createBaggageReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" First=\"" + passengerdetails[i].first + "\"  Last=\"" + passengerdetails[i].last + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\"/>");
                            createBaggageReq.Append("</SearchTraveler>");
                        }
                        count++;
                    }
                    createBaggageReq.Append(Getdetails.PriceSolution);
                    createBaggageReq.Append("</AirSolution>");
                    //"<HostReservation xmlns=\"http://www.travelport.com/schema/air_v52_0\" Carrier=\"AI\" CarrierLocatorCode=\"" + supplierLocatorCode + "\" ProviderCode=\"1G\" ProviderLocatorCode=\"" + ProvidelocatorCode + "\" />");
                    createBaggageReq.Append("</AirMerchandisingOfferAvailabilityReq></soap:Body></soap:Envelope>");
                }
                res = Methodshit.HttpPost(_testURL, createBaggageReq.ToString(), _userName, _password);
                if (_AirlineWay.ToLower() == "gdsoneway")
                {
                    logs.WriteLogs(createBaggageReq.ToString(), "3-GetAirMerchandisingOfferAvailabilityReq", "GDSOneWay", "oneway");
                    logs.WriteLogs(res, "3-getAirMerchandisingOfferAvailabilityRes", "GDSOneWay", "oneway");
                }
                else
                {
                    logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createBaggageReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirMerchandisingOfferAvailability", "GDSRT");
                }

            }
            catch (Exception ex)
            {

            }
            return res;
        }

        public string AirMerchandisingFulfillmentReq(string _testURL, StringBuilder createSSRReq, string newGuid, string _targetBranch, string _userName, string _password, string _AirlineWay, List<string> _unitkey, List<string> _SSRkey, List<string> BaggageSSrkey, SimpleAvailabilityRequestModel _GetfligthModel, List<passkeytype> passengerdetails, Hashtable htbaggagedata, string strSeatResponseleft, string? Segmentblock = null)
        {

            int count = 0;
            int icount = 100;

            string UniversallocatorCode = string.Empty;
            string supplierLocatorCode = string.Empty;
            string ProvidelocatorCode = string.Empty;
            //string Segmentblock = string.Empty;
            string BookingRefkey = string.Empty;
            UniversallocatorCode = Segmentblock.Split('@')[3];
            supplierLocatorCode = Segmentblock.Split('@')[2];
            ProvidelocatorCode = Segmentblock.Split('@')[1];

            createSSRReq = new StringBuilder();
            createSSRReq.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createSSRReq.Append(" <soapenv:Body>");
            createSSRReq.Append("<univ:AirMerchandisingFulfillmentReq xmlns:air=\"http://www.travelport.com/schema/air_v52_0\" xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" xmlns:univ=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\"  TargetBranch=\"" + _targetBranch + "\">");
            createSSRReq.Append("<com:BillingPointOfSaleInfo OriginApplication=\"UAPI\"/>");
            createSSRReq.Append("<air:HostReservation Carrier=\"AI\" CarrierLocatorCode=\"" + supplierLocatorCode + "\" ProviderCode=\"1G\" ProviderLocatorCode=\"" + ProvidelocatorCode + "\" UniversalLocatorCode=\"" + UniversallocatorCode + "\"/>");
            createSSRReq.Append("<air:AirSolution xmlns=\'http://www.travelport.com/schema/air_v52_0\'>");
            if (passengerdetails.Count > 0)
            {
                int _id = 0;
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                        createSSRReq.Append("<air:SearchTraveler xmlns=\"http://www.travelport.com/schema/air_v52_0\" Code=\"ADT\" Gender=\"M\" Key=\"" + passengerdetails[i].passengerkey + "\">");
                        createSSRReq.Append("<com:Name First=\"" + passengerdetails[i].first + "\"  Last=\"" + passengerdetails[i].last + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\"/>");
                        createSSRReq.Append("</air:SearchTraveler>");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        passengerdetails[i].title = "MSTR";
                        createSSRReq.Append("<SearchTraveler xmlns=\"http://www.travelport.com/schema/air_v52_0\" Code=\"CNN\" Gender=\"M\" Key=\"" + passengerdetails[i].passengerkey + "\">");
                        createSSRReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" First=\"" + passengerdetails[i].first + "\"  Last=\"" + passengerdetails[i].last + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\"/>");
                        createSSRReq.Append("</SearchTraveler>");
                    }
                    count++;
                }
                createSSRReq.Append(Segmentblock.Split('@')[0].Replace("common_v52_0", "com"));
                createSSRReq.Append("</air:AirSolution><air:OptionalServices><air:OptionalServicesTotal/>");

                string referencekey = string.Empty;
                string BookingTravellerref = string.Empty;
                string AirSegmentref = string.Empty;
                //Baggage
                for (int i = 0; i < BaggageSSrkey.Count; i++)
                {
                    string _data = BaggageSSrkey[i].ToString().Split('₹')[0].ToString().Trim();
                    //_data = _data.Split('*')[0].ToString();
                    referencekey = htbaggagedata[_data].ToString().Replace("common_v52_0", "com");
                    BookingTravellerref = Regex.Match(referencekey, "BookingTravelerRef=\"(?<Travellerref>[\\s\\S]*?)\"").Groups["Travellerref"].Value.Trim();
                    AirSegmentref = Regex.Match(referencekey, "AirSegmentRef=\"(?<Airsegmentref>[\\s\\S]*?)\"").Groups["Airsegmentref"].Value.Trim();
                    createSSRReq.Append(htbaggagedata[_data].ToString().Replace("common_v52_0", "com").Replace("SSRCode=\"XBAG\"", "SSRCode=\"XBAG\" SSRFreeText=\"TTL" + _data.Split("_")[0] + "1PC\""));
                }


                List<string> oneway0List = _unitkey.Where(x => x.Contains("_OneWay0")).ToList();
                List<string> oneway1List = _unitkey.Where(x => x.Contains("_OneWay1")).ToList();
                Hashtable htseat = new Hashtable();
                strSeatResponseleft = strSeatResponseleft.Replace("\\\"", "\"");
                foreach (Match mSeat in Regex.Matches(strSeatResponseleft, @"<air:OptionalService Type=""PreReservedSeatAssignment[\s\S]*?TotalPrice=""(?<Price>[\s\S]*?)""[\s\S]*?Key=""(?<Key>[\s\S]*?)""[\s\S]*?</air:OptionalService>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {

                    if (!htseat.Contains(mSeat.Groups["Price"].Value.Trim().Replace("INR", "")))
                    {
                        htseat.Add(mSeat.Groups["Price"].Value.Trim().Replace("INR", ""), mSeat.Value.Trim());
                    }
                }
                //Seat
                for (int a = 0; a < oneway0List.Count; a++)
                {
                    if (oneway0List[a].ToString().Split("_")[0].ToString().Trim() == "0")
                        continue;
                    if (oneway0List[a].ToString().Contains("_OneWay0"))
                    {
                        string SeatNum = "Data=\"" + oneway0List[a].ToString().Split("_")[1].ToString() + "\"";
                        //string _data = oneway0List[a].ToString().Split("__")[1].ToString().Replace("common_v52_0", "com");
                        string _data = htseat[oneway0List[a].ToString().Split("_")[0].Trim()].ToString().Replace("common_v52_0", "com");
                        //_data=_data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim() + );
                        string NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey + "\" AirSegmentRef=\"" + oneway0List[a].Split("_")[4].ToString().Trim() + "\">";
                        //string NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey + "\" AirSegmentRef=\"" + AirSegmentref + "\">";
                        _data = _data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim());
                        _data = Regex.Replace(_data, "Key=\"", "Key=\"" + a + "");
                        _data = Regex.Replace(_data, "<air:TaxInfo Category=[\\s\\S]*?/>", "");
                        createSSRReq.Append(Regex.Replace(_data, "<com:ServiceData[\\s\\S]*?>", NewValue));
                    }
                }
                int paxcounter = oneway0List.Count + 1;
                for (int a = 0; a < oneway1List.Count; a++)
                {
                    if (oneway1List[a].ToString().Split("_")[0].ToString().Trim() == "0")
                        continue;
                    if (oneway1List[a].ToString().Contains("_OneWay1"))
                    {
                        string SeatNum = "Data=\"" + oneway1List[a].ToString().Split("_")[1].ToString() + "\"";
                        //string _data = oneway1List[a].ToString().Split("__")[1].ToString().Replace("common_v52_0", "com");
                        string _data = htseat[oneway1List[a].ToString().Split("_")[0].Trim()].ToString().Replace("common_v52_0", "com");
                        //_data=_data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim() + );
                        string NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey + "\" AirSegmentRef=\"" + oneway1List[a].Split("_")[4].ToString().Trim() + "\">";
                        _data = _data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim());
                        _data = Regex.Replace(_data, "Key=\"", "Key=\"" + paxcounter + "");
                        _data = Regex.Replace(_data, "<air:TaxInfo Category=[\\s\\S]*?/>", "");
                        createSSRReq.Append(Regex.Replace(_data, "<com:ServiceData[\\s\\S]*?>", NewValue));
                        paxcounter++;
                    }

                }

                createSSRReq.Append("</air:OptionalServices></univ:AirMerchandisingFulfillmentReq></soapenv:Body></soapenv:Envelope>");
            }

            string res = Methodshit.HttpPost(_testURL, createSSRReq.ToString(), _userName, _password);
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs(createSSRReq.ToString(), "3-GetAirMerchandisingFulfillmentReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "3-GetAirMerchandisingFulfillmentRes", "GDSOneWay", "oneway");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createSSRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirMerchandisingFulfillment", "GDSRT");
            }
            return res;
        }
        public string AirMerchandisingFulfillmentReqRoundTrip(string _testURL, StringBuilder createSSRReq, string newGuid, string _targetBranch, string _userName, string _password, string _AirlineWay, List<string> _unitkey, List<string> _SSRkey, List<string> BaggageSSrkey, SimpleAvailabilityRequestModel _GetfligthModel, List<passkeytype> passengerdetails, string _htbaggagedataStringL, string _htbaggagedataStringR, string strSeatResponseleft, string strSeatResponseright, int p1, string? Segmentblock = null)
        {
            int count = 0;
            int icount = 100;

            string UniversallocatorCode = string.Empty;
            string supplierLocatorCode = string.Empty;
            string ProvidelocatorCode = string.Empty;
            //string Segmentblock = string.Empty;
            string BookingRefkey = string.Empty;
            UniversallocatorCode = Segmentblock.Split('@')[3];
            supplierLocatorCode = Segmentblock.Split('@')[2];
            ProvidelocatorCode = Segmentblock.Split('@')[1];
            if (BaggageSSrkey.Count > 0)
            {
                //UniversallocatorCode = BaggageSSrkey[0].ToString().Split('@')[2];
                //supplierLocatorCode = BaggageSSrkey[0].ToString().Split('@')[3];
                //ProvidelocatorCode = BaggageSSrkey[0].ToString().Split('@')[4];
                ////Segmentblock = BaggageSSrkey[0].ToString().Split('@')[5];
                //BookingRefkey = BaggageSSrkey[0].ToString().Split('@')[6].Split('_')[0];
            }

            createSSRReq = new StringBuilder();
            createSSRReq.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createSSRReq.Append(" <soapenv:Body>");
            createSSRReq.Append("<univ:AirMerchandisingFulfillmentReq xmlns:air=\"http://www.travelport.com/schema/air_v52_0\" xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" xmlns:univ=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\"  TargetBranch=\"" + _targetBranch + "\">");
            createSSRReq.Append("<com:BillingPointOfSaleInfo OriginApplication=\"UAPI\"/>");
            createSSRReq.Append("<air:HostReservation Carrier=\"AI\" CarrierLocatorCode=\"" + supplierLocatorCode + "\" ProviderCode=\"1G\" ProviderLocatorCode=\"" + ProvidelocatorCode + "\" UniversalLocatorCode=\"" + UniversallocatorCode + "\"/>");
            createSSRReq.Append("<air:AirSolution xmlns=\'http://www.travelport.com/schema/air_v52_0\'>");
            string paxkey = string.Empty;
            if (passengerdetails.Count > 0)
            {
                int _id = 0;
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        if (p1 == 0)
                        {
                            if (passengerdetails[i].passengerkey.Contains("**"))
                            {
                                paxkey = passengerdetails[i].passengerkey.Split("**")[0].Trim();
                            }
                            else
                            {
                                paxkey = passengerdetails[i].passengerkey.Trim();
                            }
                        }
                        else
                        {
                            if (passengerdetails[i].passengerkey.Contains("**"))
                            {
                                paxkey = passengerdetails[i].passengerkey.Split("**")[1].Trim();
                            }
                            else
                            {
                                paxkey = passengerdetails[i].passengerkey.Trim();
                            }
                        }
                        passengerdetails[i].title = "MR";
                        createSSRReq.Append("<air:SearchTraveler xmlns=\"http://www.travelport.com/schema/air_v52_0\" Code=\"ADT\" Gender=\"M\" Key=\"" + paxkey + "\">");
                        createSSRReq.Append("<com:Name First=\"" + passengerdetails[i].first + "\"  Last=\"" + passengerdetails[i].last + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\"/>");
                        createSSRReq.Append("</air:SearchTraveler>");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        if (p1 == 0)
                        {
                            if (passengerdetails[i].passengerkey.Contains("**"))
                            {
                                paxkey = passengerdetails[i].passengerkey.Split("**")[0].Trim();
                            }
                            else
                            {
                                paxkey = passengerdetails[i].passengerkey.Trim();
                            }
                        }
                        else
                        {
                            if (passengerdetails[i].passengerkey.Contains("**"))
                            {
                                paxkey = passengerdetails[i].passengerkey.Split("**")[1].Trim();
                            }
                            else
                            {
                                paxkey = passengerdetails[i].passengerkey.Trim();
                            }
                        }
                        passengerdetails[i].title = "MSTR";
                        createSSRReq.Append("<SearchTraveler xmlns=\"http://www.travelport.com/schema/air_v52_0\" Code=\"CNN\" Gender=\"M\" Key=\"" + paxkey + "\">");
                        createSSRReq.Append("<Name xmlns=\"http://www.travelport.com/schema/common_v52_0\" First=\"" + passengerdetails[i].first + "\"  Last=\"" + passengerdetails[i].last + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\"/>");
                        createSSRReq.Append("</SearchTraveler>");
                    }
                    count++;
                }
                
                createSSRReq.Append(Segmentblock.Split('@')[0].Replace("common_v52_0", "com"));
                createSSRReq.Append("</air:AirSolution><air:OptionalServices><air:OptionalServicesTotal/>");

                string referencekey = string.Empty;
                string BookingTravellerref = string.Empty;
                string AirSegmentref = string.Empty;

                Hashtable htbaggagedata = new Hashtable();
                //Baggage
                // to do p1 case
                for (int i = 0; i < BaggageSSrkey.Count; i++)
                {
                    if (p1 == 0 && (BaggageSSrkey[i].ToString().Contains("_OneWay0") || BaggageSSrkey[i].ToString().Contains("_OneWay1")))
                    {
                        htbaggagedata = JsonConvert.DeserializeObject<Hashtable>(_htbaggagedataStringL);
                        string _data = BaggageSSrkey[i].ToString().Replace("_OneWay0", "").Replace("_OneWay1", "").Trim(); ;
                        int index = _data.IndexOf('_');
                        //if (index != -1 && (_data.Contains("AirIndia") || BaggageSSrkey[i].ToString().Contains("<air")))
                        //{
                        //    //string getpaxkey = Regex.Match(_data.Split('_')[1], @"(\d+)\^AirIndia").Groups[1].Value;
                        //    string getpaxkey = Regex.Match(BaggageSSrkey[i].ToString().Split('*')[1].ToString(), @"BookingTravelerRef=""(?<paxid>[\s\S]*?)""").Groups["paxid"].Value;
                        //    //_data = "\"" + _data.Split('_')[0] + "_" + htpaxid[getpaxkey] + _data.Split('_')[2] + "\""; 
                        //    _data = _data.Split('_')[0] + "_" + getpaxkey + "_" + _data.Split('_')[2];
                        //}
                        referencekey = htbaggagedata[_data].ToString().Replace("common_v52_0", "com");
                        BookingTravellerref = Regex.Match(referencekey, "BookingTravelerRef=\"(?<Travellerref>[\\s\\S]*?)\"").Groups["Travellerref"].Value.Trim();
                        AirSegmentref = Regex.Match(referencekey, "AirSegmentRef=\"(?<Airsegmentref>[\\s\\S]*?)\"").Groups["Airsegmentref"].Value.Trim();
                        createSSRReq.Append(htbaggagedata[_data].ToString().Replace("common_v52_0", "com").Replace("SSRCode=\"XBAG\"", "SSRCode=\"XBAG\" SSRFreeText=\"TTL" + _data.Split("_")[0] + "1PC\""));
                    }
                    else
                    {
                        if (p1 == 1 && BaggageSSrkey[i].ToString().Contains("_RT0") || BaggageSSrkey[i].ToString().Contains("_RT1"))
                        {
                            htbaggagedata = JsonConvert.DeserializeObject<Hashtable>(_htbaggagedataStringR);
                            //string _data = BaggageSSrkey[i].ToString().Split('@')[0].ToString();
                            //_data = _data.Split('*')[0].ToString();
                            //string[] parts = BaggageSSrkey[i].ToString().Split('_');
                            //string result = parts.Length >= 3
                            //    ? string.Join("_", parts[0], parts[1])
                            //    : BaggageSSrkey[i].ToString();

                            string _data = BaggageSSrkey[i].ToString().Replace("_RT0", "").Replace("_RT1", "").Trim(); ; ;
                            referencekey = htbaggagedata[_data].ToString().Replace("common_v52_0", "com");
                            BookingTravellerref = Regex.Match(referencekey, "BookingTravelerRef=\"(?<Travellerref>[\\s\\S]*?)\"").Groups["Travellerref"].Value.Trim();
                            AirSegmentref = Regex.Match(referencekey, "AirSegmentRef=\"(?<Airsegmentref>[\\s\\S]*?)\"").Groups["Airsegmentref"].Value.Trim();
                            createSSRReq.Append(htbaggagedata[_data].ToString().Replace("common_v52_0", "com").Replace("SSRCode=\"XBAG\"", "SSRCode=\"XBAG\" SSRFreeText=\"TTL" + _data.Split("_")[0] + "1PC\""));
                        }
                    }
                }


                List<string> oneway0List = _unitkey.Where(x => x.Contains("_OneWay0")).ToList();
                List<string> oneway1List = _unitkey.Where(x => x.Contains("_OneWay1")).ToList();
                List<string> rt0List = _unitkey.Where(x => x.Contains("_RT0")).ToList();
                List<string> rt1List = _unitkey.Where(x => x.Contains("_RT1")).ToList();

                //Seat
                Hashtable htseat = new Hashtable();
                if (p1 == 0)
                {
                    strSeatResponseleft = strSeatResponseleft.Replace("\\\"", "\"");
                    foreach (Match mSeat in Regex.Matches(strSeatResponseleft, @"<air:OptionalService Type=""PreReservedSeatAssignment[\s\S]*?TotalPrice=""(?<Price>[\s\S]*?)""[\s\S]*?Key=""(?<Key>[\s\S]*?)""[\s\S]*?</air:OptionalService>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    {

                        if (!htseat.Contains(mSeat.Groups["Price"].Value.Trim().Replace("INR", "")))
                        {
                            htseat.Add(mSeat.Groups["Price"].Value.Trim().Replace("INR", ""), mSeat.Value.Trim());
                        }
                    }
                    //string Airsegmentid = Regex.Match(strSeatResponseleft, @"AirSegment Key=""(?<segmentid>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["segmentid"].Value.Trim();
                    List<string> lstsegment = new List<string>();
                    foreach (Match mSegmentid in Regex.Matches(Segmentblock, @"AirSegment Key=""(?<segmentid>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    {
                        lstsegment.Add(mSegmentid.Groups["segmentid"].Value.Trim());
                    }
                    for (int a = 0; a < oneway0List.Count; a++)
                    {
                        if (oneway0List[a].ToString().Split("_")[0].ToString().Trim() == "0")
                            continue;
                        if (oneway0List[a].ToString().Contains("_OneWay0"))
                        {
                            string SeatNum = "Data=\"" + oneway0List[a].ToString().Split("_")[1].ToString() + "\"";
                            string _data = htseat[oneway0List[a].ToString().Split("_")[0].Trim()].ToString().Replace("common_v52_0", "com");
                            //_data=_data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim() + );
                            //BookingTravellerref = Regex.Match(_data, @"BookingTravelerRef=""(?<BookingTravelerRef>[\s\S]*?)""").Groups["BookingTravelerRef"].Value;
                            string NewValue = "";
                            if (passengerdetails[a].passengerkey.Contains("**"))
                            {
                                NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey.Split("**")[0].Trim() + "\" AirSegmentRef=\"" + AirSegmentref + "\">";
                            }
                            else
                            {
                                NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey.Trim() + "\" AirSegmentRef=\"" + AirSegmentref + "\">";
                            }
                            AirSegmentref = lstsegment[0].Trim();
                            _data = _data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim());
                            _data = Regex.Replace(_data, "Key=\"", "Key=\"" + a + "");
                            _data = Regex.Replace(_data, "<air:TaxInfo Category=[\\s\\S]*?/>", "");
                            createSSRReq.Append(Regex.Replace(_data, "<com:ServiceData[\\s\\S]*?>", NewValue));
                        }
                    }
                    int paxcounter = oneway0List.Count + 1;
                    for (int a = 0; a < oneway1List.Count; a++)
                    {
                        if (oneway1List[a].ToString().Split("_")[0].ToString().Trim() == "0")
                            continue;
                        if (oneway1List[a].ToString().Contains("_OneWay1"))
                        {
                            string SeatNum = "Data=\"" + oneway1List[a].ToString().Split("_")[1].ToString() + "\"";
                            string _data = htseat[oneway1List[a].ToString().Split("_")[0].Trim()].ToString().Replace("common_v52_0", "com");
                            //_data=_data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim() + );
                            //BookingTravellerref = Regex.Match(_data, @"BookingTravelerRef=""(?<BookingTravelerRef>[\s\S]*?)""").Groups["BookingTravelerRef"].Value;
                            string NewValue = "";
                            if (passengerdetails[a].passengerkey.Contains("**"))
                            {
                                NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey.Split("**")[0].Trim() + "\" AirSegmentRef=\"" + AirSegmentref + "\">";
                            }
                            else
                            {
                                NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey.Trim() + "\" AirSegmentRef=\"" + AirSegmentref + "\">";
                            }
                            AirSegmentref = lstsegment[1].Trim();
                            _data = _data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim());
                            _data = Regex.Replace(_data, "Key=\"", "Key=\"" + paxcounter + "");
                            _data = Regex.Replace(_data, "<air:TaxInfo Category=[\\s\\S]*?/>", "");
                            createSSRReq.Append(Regex.Replace(_data, "<com:ServiceData[\\s\\S]*?>", NewValue));
                            paxcounter++;
                        }

                    }
                }
                else if (p1 == 1)
                {
                    strSeatResponseright = strSeatResponseright.Replace("\\\"", "\"");
                    //string Airsegmentid = Regex.Match(strSeatResponseright, @"AirSegment Key=""(?<segmentid>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups["segmentid"].Value.Trim();
                    List<string> lstsegment = new List<string>();
                    foreach (Match mSegmentid in Regex.Matches(Segmentblock, @"AirSegment Key=""(?<segmentid>[\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    {
                        lstsegment.Add(mSegmentid.Groups["segmentid"].Value.Trim());
                    }

                    htseat = new Hashtable();
                    foreach (Match mSeat in Regex.Matches(strSeatResponseright, @"<air:OptionalService Type=""PreReservedSeatAssignment[\s\S]*?TotalPrice=""(?<Price>[\s\S]*?)""[\s\S]*?Key=""(?<Key>[\s\S]*?)""[\s\S]*?</air:OptionalService>", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    {

                        if (!htseat.Contains(mSeat.Groups["Price"].Value.Trim().Replace("INR", "")))
                        {
                            htseat.Add(mSeat.Groups["Price"].Value.Trim().Replace("INR", ""), mSeat.Value.Trim());
                        }
                    }
                    for (int a = 0; a < rt0List.Count; a++)
                    {
                        if (rt0List[a].ToString().Split("_")[0].ToString().Trim() == "0")
                            continue;
                        if (rt0List[a].ToString().Contains("_RT0"))
                        {
                            string SeatNum = "Data=\"" + rt0List[a].ToString().Split("_")[1].ToString() + "\"";
                            string _data = htseat[rt0List[a].ToString().Split("_")[0].Trim()].ToString().Replace("common_v52_0", "com");
                            //_data=_data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim() + );
                            AirSegmentref = lstsegment[0].Trim();
                            //BookingTravellerref = Regex.Match(_data, @"BookingTravelerRef=""(?<BookingTravelerRef>[\s\S]*?)""").Groups["BookingTravelerRef"].Value;
                            string NewValue = "";
                            if (passengerdetails[a].passengerkey.Contains("**"))
                            {
                                NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey.Split("**")[1].Trim() + "\" AirSegmentRef=\"" + AirSegmentref + "\">";
                            }
                            else
                            {
                                NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey.Trim() + "\" AirSegmentRef=\"" + AirSegmentref + "\">";
                            }
                            _data = _data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim());
                            _data = Regex.Replace(_data, "Key=\"", "Key=\"" + a + "");
                            _data = Regex.Replace(_data, "<air:TaxInfo Category=[\\s\\S]*?/>", "");
                            createSSRReq.Append(Regex.Replace(_data, "<com:ServiceData[\\s\\S]*?>", NewValue));
                        }
                    }
                    int paxcounter = rt0List.Count + 1;
                    for (int a = 0; a < rt1List.Count; a++)
                    {
                        if (rt1List[a].ToString().Split("_")[0].ToString().Trim() == "0")
                            continue;
                        if (rt1List[a].ToString().Contains("_RT1"))
                        {
                            string SeatNum = "Data=\"" + rt1List[a].ToString().Split("_")[1].ToString() + "\"";
                            string _data = htseat[rt1List[a].ToString().Split("_")[0].Trim()].ToString().Replace("common_v52_0", "com");
                            //_data=_data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim() + );
                            //BookingTravellerref = Regex.Match(_data, @"BookingTravelerRef=""(?<BookingTravelerRef>[\s\S]*?)""").Groups["BookingTravelerRef"].Value;
                            AirSegmentref = lstsegment[1].Trim();
                            string NewValue = "";
                            if (passengerdetails[a].passengerkey.Contains("**"))
                            {
                                NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey.Split("**")[1].Trim() + "\" AirSegmentRef=\"" + AirSegmentref + "\">";
                            }
                            else
                            {
                                NewValue = "<com:ServiceData " + SeatNum.Trim() + " BookingTravelerRef=\"" + passengerdetails[a].passengerkey.Trim() + "\" AirSegmentRef=\"" + AirSegmentref + "\">";
                            }
                            _data = _data.Replace("<com:ServiceData", "<com:ServiceData " + SeatNum.Trim());
                            _data = Regex.Replace(_data, "Key=\"", "Key=\"" + paxcounter + "");
                            _data = Regex.Replace(_data, "<air:TaxInfo Category=[\\s\\S]*?/>", "");
                            createSSRReq.Append(Regex.Replace(_data, "<com:ServiceData[\\s\\S]*?>", NewValue));
                            paxcounter++;
                        }

                    }
                }

                createSSRReq.Append("</air:OptionalServices></univ:AirMerchandisingFulfillmentReq></soapenv:Body></soapenv:Envelope>");
            }

            string res = Methodshit.HttpPost(_testURL, createSSRReq.ToString(), _userName, _password);
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs(createSSRReq.ToString(), "3-GetAirMerchandisingFulfillmentReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "3-GetAirMerchandisingFulfillmentRes", "GDSOneWay", "oneway");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createSSRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirMerchandisingFulfillment", "GDSRT");
            }
            return res;
        }
        public string CreatePNR_bkpnonstop(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, List<string> _unitkey, List<string> _SSRkey, string? _pricesolution = null)
        {

            int count = 0;
            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                int _id = 0;
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + passengerdetails[i].passengerkey + "\"  TravelerType=\"ADT\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + passengerdetails[i].passengerkey + "\"  TravelerType=\"CNN\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + passengerdetails[i].passengerkey + "\" TravelerType=\"INF\">");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + passengerdetails[i].passengerkey + "\"  TravelerType=\"ADT\">");
                    }


                    //Title
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                    }
                    else
                    {
                        passengerdetails[i].title = "MSTR";
                    }
                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                        int seg = 0;
                        foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                        {
                            if (_SSRkey.Count > _id)
                            {
                                ssrsegmentwise _obj = new ssrsegmentwise();
                                _obj.SSRcodeOneWayI = new List<ssrsKey>();
                                _obj.SSRcodeOneWayII = new List<ssrsKey>();
                                for (int k = 0; k < _SSRkey.Count; k++)
                                {
                                    if (_SSRkey[k].Contains("_OneWay0"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj0 = new ssrsKey();
                                            _obj0.key = _SSRkey[k];
                                            _obj.SSRcodeOneWayI.Add(_obj0);
                                        }

                                    }
                                    else if (_SSRkey[k].Contains("_OneWay1"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj1 = new ssrsKey();
                                            _obj1.key = _SSRkey[k];
                                            _obj.SSRcodeOneWayII.Add(_obj1);
                                        }
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeOneWayI.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeOneWayI[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);
                                    if (_obj.SSRcodeOneWayI[k].key.Contains("_OneWay0") && seg == 0 && _obj.SSRcodeOneWayI[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)// || _SSRkey[_id].Contains("_OneWay1")))
                                    {
                                        //}
                                        string[] unitsubKey2 = _obj.SSRcodeOneWayI[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeOneWayII.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeOneWayII[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);
                                    if (_obj.SSRcodeOneWayII[k].key.Contains("_OneWay1") && seg == 1 && _obj.SSRcodeOneWayII[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeOneWayII[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }

                            }
                            seg++;
                        }
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }

                    if (i == 0 && passengerdetails[i].passengertypecode == "ADT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/03Dec06/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/03Dec06/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        //Domestic
                        //createPNRReq.Append("<Address>");
                        //createPNRReq.Append("<AddressName>Home</AddressName>");
                        //createPNRReq.Append("<Street>20th I Cross</Street>");
                        //createPNRReq.Append("<City>Bangalore</City>");
                        //createPNRReq.Append("<State>KA</State>");
                        //createPNRReq.Append("<PostalCode>560047</PostalCode>");
                        //createPNRReq.Append("<Country>IN</Country>");
                        //createPNRReq.Append("</Address>");
                        //International
                        createPNRReq.Append("<Address>");
                        createPNRReq.Append("<AddressName>DemoSiteAddress</AddressName>");
                        createPNRReq.Append("<Street>Via Augusta 59 5</Street>");
                        createPNRReq.Append("<City>Delhi</City>");
                        createPNRReq.Append("<State>DL</State>");
                        createPNRReq.Append("<PostalCode>111001</PostalCode>");
                        createPNRReq.Append("<Country>IN</Country>");
                        createPNRReq.Append("</Address>");

                    }
                    if (passengerdetails[i].passengertypecode == "CNN" || passengerdetails[i].passengertypecode == "CHD")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11Dec13/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11Dec13/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>P-C11 DOB11Dec13</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    string format = "11DEC23";// Convert.ToDateTime(passengerdetails[i].dateOfBirth).ToString("ddMMMyy");
                    if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/MI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/FI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>" + format + "</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }

                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                createPNRReq.Append("<ContinuityCheckOverride xmlns=\"http://www.travelport.com/schema/common_v52_0\">true</ContinuityCheckOverride>");
                createPNRReq.Append("<AgencyContactInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
                createPNRReq.Append("<PhoneNumber CountryCode=\"91\" AreaCode=\"011\" Number=\"46615790\" Location=\"DEL\" Type=\"Agency\"/>");
                createPNRReq.Append("</AgencyContactInfo>");
                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                Getdetails.PriceSolution = Getdetails.PriceSolution.Replace("</air:CancelPenalty>", "</air:CancelPenalty><air:AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\"> </air:AirPricingModifiers>");
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");

                #region seat
                int idx = 0;
                int _seg = 0;
                foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                {

                    // Ensure each adult/child gets a unique seat per segment
                    //foreach (Match mitem in Regex.Matches(Getdetails.PriceSolution, "PassengerType BookingTravelerRef=\'(?<Travllerref>[\\s\\S]*?)\'\\s*Code=\'(?<PaxType>[\\s\\S]*?)'", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    //{
                    //idx = 0;
                    //if (mitem.Groups["PaxType"].Value == "INFT" || mitem.Groups["PaxType"].Value == "INF")
                    //{  //idx++;
                    //    continue;
                    //}


                    if (_unitkey.Count > 0 && idx < _unitkey.Count)
                    {
                        for (int a = 0; a < _unitkey.Count && idx < _unitkey.Count; a++)
                        {
                            if (_unitkey[idx].Split('_')[0].Trim() == "0")
                            {
                                if (_seg == 0 && (_unitkey[idx].Contains("_OneWay0") || _unitkey[idx].Contains("_OneWay1")))
                                {
                                    string[] unitsubKey2 = _unitkey[idx].Split('_');
                                    string pas_unitKey = unitsubKey2[1].Insert(unitsubKey2[1].Length - 2, "-");
                                    //createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                    createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + passengerdetails[a].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                    //break;
                                }
                                else if (_seg == 1 && (_unitkey[idx].Contains("_OneWay0") || _unitkey[idx].Contains("_OneWay1")))
                                {
                                    string[] unitsubKey2 = _unitkey[idx].Split('_');
                                    string pas_unitKey = unitsubKey2[1].Insert(unitsubKey2[1].Length - 2, "-");
                                    //createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                    createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + passengerdetails[a].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                    //break;
                                }
                                else
                                {
                                    //continue;
                                }
                            }
                            idx++;
                        }


                    }
                    idx++;
                    //}
                    _seg++;
                }
                #endregion

                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }
            //}
            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs(createPNRReq.ToString(), "3-GetPNRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "3-GetPNRRes", "GDSOneWay", "oneway");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");

            }
            return res;
        }

        public string CreatePNR(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, List<string> _unitkey, List<string> _SSRkey, string? _pricesolution = null)
        {

            int count = 0;
            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                int _id = 0;
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + passengerdetails[i].passengerkey + "\"  TravelerType=\"ADT\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + passengerdetails[i].passengerkey + "\"  TravelerType=\"CNN\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + passengerdetails[i].passengerkey + "\" TravelerType=\"INF\">");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + passengerdetails[i].passengerkey + "\"  TravelerType=\"ADT\">");
                    }


                    //Title
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                    }
                    else
                    {
                        passengerdetails[i].title = "MSTR";
                    }
                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                        int seg = 0;
                        foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                        {
                            if (_SSRkey.Count > _id)
                            {
                                ssrsegmentwise _obj = new ssrsegmentwise();
                                _obj.SSRcodeOneWayI = new List<ssrsKey>();
                                _obj.SSRcodeOneWayII = new List<ssrsKey>();
                                for (int k = 0; k < _SSRkey.Count; k++)
                                {
                                    if (_SSRkey[k].Contains("_OneWay0"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj0 = new ssrsKey();
                                            _obj0.key = _SSRkey[k];
                                            _obj.SSRcodeOneWayI.Add(_obj0);
                                        }

                                    }
                                    else if (_SSRkey[k].Contains("_OneWay1"))
                                    {
                                        string[] wordsArray = _SSRkey[k].ToString().Split('_');
                                        if (wordsArray.Length > 1 && !string.IsNullOrEmpty(wordsArray[0]))
                                        {
                                            ssrsKey _obj1 = new ssrsKey();
                                            _obj1.key = _SSRkey[k];
                                            _obj.SSRcodeOneWayII.Add(_obj1);
                                        }
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeOneWayI.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeOneWayI[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);
                                    if (_obj.SSRcodeOneWayI[k].key.Contains("_OneWay0") && seg == 0 && _obj.SSRcodeOneWayI[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)// || _SSRkey[_id].Contains("_OneWay1")))
                                    {
                                        //}
                                        string[] unitsubKey2 = _obj.SSRcodeOneWayI[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }
                                for (int k = 0; k < _obj.SSRcodeOneWayII.Count; k++)
                                {
                                    string[] parts = _obj.SSRcodeOneWayII[k].key.Split('/');
                                    string result = parts[parts.Length - 2] + "/" + parts[0].Substring(parts[0].LastIndexOf('_') + 1);
                                    if (_obj.SSRcodeOneWayII[k].key.Contains("_OneWay1") && seg == 1 && _obj.SSRcodeOneWayII[k].key.Split('/').Last() == passengerdetails[i].passengertypecode && result == passengerdetails[i].last + "/" + passengerdetails[i].first)
                                    {
                                        string[] unitsubKey2 = _obj.SSRcodeOneWayII[k].key.Split('_');
                                        string pas_unitKey = unitsubKey2[0];
                                        createPNRReq.Append("<SSR Type=\"" + pas_unitKey + "\" Status=\"NN\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" Key=\"" + passengerdetails[i].passengerkey + "_" + _id + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\"/>");
                                        _id++;
                                    }
                                }

                            }
                            seg++;
                        }
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }

                    if (i == 0 && passengerdetails[i].passengertypecode == "ADT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mr")
                        {
                            //for DEFAULT DOB
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/01Jan00/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////01Jan00/M//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                            //for Hardcoded DOB
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN//M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"//////M//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                        }
                        else
                        {
                            //for DEFAULT DOB
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/01Jan00/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////01Jan00/F//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                            //for Hardcoded DOB
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN//F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"//////F//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        //Domestic
                        //createPNRReq.Append("<Address>");
                        //createPNRReq.Append("<AddressName>Home</AddressName>");
                        //createPNRReq.Append("<Street>20th I Cross</Street>");
                        //createPNRReq.Append("<City>Bangalore</City>");
                        //createPNRReq.Append("<State>KA</State>");
                        //createPNRReq.Append("<PostalCode>560047</PostalCode>");
                        //createPNRReq.Append("<Country>IN</Country>");
                        //createPNRReq.Append("</Address>");
                        //International
                        createPNRReq.Append("<Address>");
                        createPNRReq.Append("<AddressName>DemoSiteAddress</AddressName>");
                        createPNRReq.Append("<Street>Via Augusta 59 5</Street>");
                        createPNRReq.Append("<City>Delhi</City>");
                        createPNRReq.Append("<State>DL</State>");
                        createPNRReq.Append("<PostalCode>111001</PostalCode>");
                        createPNRReq.Append("<Country>IN</Country>");
                        createPNRReq.Append("</Address>");

                    }
                    if (passengerdetails[i].passengertypecode == "CNN" || passengerdetails[i].passengertypecode == "CHD")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            //for DEFAULT DOB
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/01Jan00/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////01Jan00/M//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                            //for Hardcoded DOB
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN//M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"//////M//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                        }
                        else
                        {
                            //for DEFAULT DOB
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/01Jan00/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////01Jan00/F//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                            //for Hardcoded DOB
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN//F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"//////F//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>P-C11 DOB11Dec13</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    string format = "11DEC23";// Convert.ToDateTime(passengerdetails[i].dateOfBirth).ToString("ddMMMyy");
                    if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/MI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////11DEC23/MI//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN//MI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"//////MI//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/FI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"/////11DEC23/FI//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN//FI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                            //createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"//////FI//" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");

                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>" + format + "</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }

                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                createPNRReq.Append("<ContinuityCheckOverride xmlns=\"http://www.travelport.com/schema/common_v52_0\">true</ContinuityCheckOverride>");
                createPNRReq.Append("<AgencyContactInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
                createPNRReq.Append("<PhoneNumber CountryCode=\"91\" AreaCode=\"011\" Number=\"46615790\" Location=\"DEL\" Type=\"Agency\"/>");
                createPNRReq.Append("</AgencyContactInfo>");
                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                Getdetails.PriceSolution = Getdetails.PriceSolution.Replace("</air:CancelPenalty>", "</air:CancelPenalty><air:AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\"> </air:AirPricingModifiers>");
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");

                #region seat
                int idx = 0;
                int _seg = 0;
                foreach (Match itemsegment in Regex.Matches(Getdetails.PriceSolution, "AirSegment Key=\"(?<Segmentid>[\\s\\S]*?)\""))
                {

                    // Ensure each adult/child gets a unique seat per segment
                    //foreach (Match mitem in Regex.Matches(Getdetails.PriceSolution, "PassengerType BookingTravelerRef=\'(?<Travllerref>[\\s\\S]*?)\'\\s*Code=\'(?<PaxType>[\\s\\S]*?)'", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    //{
                    //idx = 0;
                    //if (mitem.Groups["PaxType"].Value == "INFT" || mitem.Groups["PaxType"].Value == "INF")
                    //{  //idx++;
                    //    continue;
                    //}
                    List<string> oneway0List = _unitkey.Where(x => x.Contains("_OneWay0")).ToList();
                    List<string> oneway1List = _unitkey.Where(x => x.Contains("_OneWay1")).ToList();

                    for (int a = 0; a < oneway0List.Count; a++)
                    {
                        if (oneway0List[a].Split('_')[0].Trim() == "0")
                        {
                            if (_seg == 0 && (oneway0List[a].Contains("_OneWay0")))
                            {
                                string[] unitsubKey2 = oneway0List[a].Split('_');
                                string pas_unitKey = unitsubKey2[1].Insert(unitsubKey2[1].Length - 2, "-");
                                //createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + passengerdetails[a].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                //break;
                            }
                        }
                    }

                    for (int a = 0; a < oneway1List.Count; a++)
                    {
                        if (oneway1List[a].Split('_')[0].Trim() == "0")
                        {
                            if (_seg == 1 && (oneway1List[a].Contains("_OneWay1")))
                            {
                                string[] unitsubKey2 = oneway1List[a].Split('_');
                                string pas_unitKey = unitsubKey2[1].Insert(unitsubKey2[1].Length - 2, "-");
                                //createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + mitem.Groups["Travllerref"].Value + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                createPNRReq.Append("<SpecificSeatAssignment xmlns=\"http://www.travelport.com/schema/air_v52_0\" BookingTravelerRef=\"" + passengerdetails[a].passengerkey + "\" SegmentRef=\"" + itemsegment.Groups["Segmentid"].Value.Trim() + "\" SeatId=\"" + pas_unitKey.Trim() + "\"/>");
                                //break;
                            }
                        }
                    }
                    _seg++;
                }
                #endregion

                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }
            //}
            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs(createPNRReq.ToString(), "3-GetPNRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "3-GetPNRRes", "GDSOneWay", "oneway");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");

            }
            return res;
        }

        public string CreatePNR_abhi(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, string? _pricesolution = null)
        {

            int count = 0;
            int icount = 100;
            //int paxCount = 0;
            //int legcount = 0;
            //string origin = string.Empty;
            //int legKeyCounter = 0;

            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"CNN\" Age=\"10\" DOB=\"2014-07-25\" >");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\" TravelerType=\"INF\" Age=\"1\" DOB=\"2023-08-25\" >");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    //Title
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                    }
                    else
                    {
                        passengerdetails[i].title = "MSTR";
                    }
                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }

                    //if (!String.IsNullOrEmpty(paxDetail.FrequentFlierNumber) && paxDetail.FrequentFlierNumber.Length > 5)
                    //{
                    //if (segment_.Bonds[0].Legs[0].AirlineName.Equals("UK"))
                    //{
                    //createPNRReq.Append("<SSR  Key='" + count + "' Type='FQTV' Status='HK' Carrier='UK' FreeText='" + paxDetail.FrequentFlierNumber + "-" + paxDetail.LastName + "/" + paxDetail.FirstName + "" + paxDetail.Title.ToUpper() + "'/>");
                    //}
                    //else
                    //{
                    //  createPNRReq.Append("<com:LoyaltyCard SupplierCode='" + segment_.Bonds[0].Legs[0].AirlineName + "' CardNumber='" + paxDetail.FrequentFlierNumber + "'/>");
                    //}
                    //}
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("<SSR  Key=\"" + count + "\" Type=\"DOCS\"  FreeText=\"P/GB/S12345678/GB/20JUL76/M/01JAN16/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                    string contractNo = string.Empty;
                    if (string.IsNullOrEmpty(contractNo))
                    {
                        contractNo = "CTCM " + passengerdetails[i].mobile + " PAX";
                    }


                    if (i == 0 && passengerdetails[i].passengertypecode == "ADT")
                    {
                        icount++;
                        createPNRReq.Append("<SSR  Key=\"" + icount + "\" Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        icount++;
                        createPNRReq.Append("<SSR  Key=\"" + icount + "\" Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<Address>");
                        createPNRReq.Append("<AddressName>Home</AddressName>");
                        createPNRReq.Append("<Street>20th I Cross</Street>");
                        createPNRReq.Append("<City>Bangalore</City>");
                        createPNRReq.Append("<State>KA</State>");
                        createPNRReq.Append("<PostalCode>560047</PostalCode>");
                        createPNRReq.Append("<Country>IN</Country>");
                        createPNRReq.Append("</Address>");
                    }

                    if (passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>P-C04</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    string format = Convert.ToDateTime(passengerdetails[i].dateOfBirth).ToString("ddMMMyy");
                    if (passengerdetails[i].passengertypecode == "INF")
                    {
                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>" + format + "</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                createPNRReq.Append("<ContinuityCheckOverride xmlns=\"http://www.travelport.com/schema/common_v52_0\">true</ContinuityCheckOverride>");
                createPNRReq.Append("<AgencyContactInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
                createPNRReq.Append("<PhoneNumber CountryCode=\"91\" AreaCode=\"011\" Number=\"46615790\" Location=\"DEL\" Type=\"Agency\"/>");
                createPNRReq.Append("</AgencyContactInfo>");
                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");
                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }
            //}
            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            //SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            //SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));
            //if (_AirlineWay.ToLower() == "gdsoneway")
            //{
            //    //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "GDSOneWay");
            //}
            //else
            //{
            //    //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");
            //}

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(createPNRReq.ToString(), "3-GetPNRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "3-GetPNRRes", "GDSOneWay", "oneway");
            }
            else
            {
                //    //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                //    if (p == 0)
                //    {
                //        logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Left", "GDSRT");
                //        logs.WriteLogsR(res, "3-GetAirpriceRes_Left", "GDSRT");
                //    }
                //    else
                //    {
                //        logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Right", "GDSRT");
                //        logs.WriteLogsR(res, "3-GetAirpriceRes_Right", "GDSRT");
                //    }
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");

            }
            return res;
        }
        public string CreatePNR_old(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, string? _pricesolution = null)
        {

            int count = 0;
            //int paxCount = 0;
            //int legcount = 0;
            //string origin = string.Empty;
            //int legKeyCounter = 0;

            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" key=\"" + count + "\" TravelerType=\"INF\" Age=\"01\" DOB=\"2023-08-25\" >");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"CNN\" Age=\"10\" DOB=\"2014-07-25\" >");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }

                    //Title
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                    }
                    else
                    {
                        passengerdetails[i].title = "MSTR";
                    }

                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }

                    //if (!String.IsNullOrEmpty(paxDetail.FrequentFlierNumber) && paxDetail.FrequentFlierNumber.Length > 5)
                    //{
                    //if (segment_.Bonds[0].Legs[0].AirlineName.Equals("UK"))
                    //{
                    //createPNRReq.Append("<SSR  Key='" + count + "' Type='FQTV' Status='HK' Carrier='UK' FreeText='" + paxDetail.FrequentFlierNumber + "-" + paxDetail.LastName + "/" + paxDetail.FirstName + "" + paxDetail.Title.ToUpper() + "'/>");
                    //}
                    //else
                    //{
                    //  createPNRReq.Append("<com:LoyaltyCard SupplierCode='" + segment_.Bonds[0].Legs[0].AirlineName + "' CardNumber='" + paxDetail.FrequentFlierNumber + "'/>");
                    //}
                    //}
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    // createPNRReq.Append("<SSR  Key=\"" + count + "\" Type=\"DOCS\"  FreeText=\"P/GB/S12345678/GB/20JUL76/M/01JAN16/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                    string contractNo = string.Empty;
                    if (string.IsNullOrEmpty(contractNo))
                    {
                        contractNo = "CTCM " + passengerdetails[i].mobile + " PAX";
                    }


                    if (i == 0 && passengerdetails[i].passengertypecode == "ADT")
                    {
                        //createPNRReq.Append("<SSR  Key=\"" + count + "\" Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        //createPNRReq.Append("<SSR  Key=\"" + count+ "\" Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<Address>");
                        createPNRReq.Append("<AddressName>Home</AddressName>");
                        createPNRReq.Append("<Street>20th I Cross</Street>");
                        createPNRReq.Append("<City>Bangalore</City>");
                        createPNRReq.Append("<State>KA</State>");
                        createPNRReq.Append("<PostalCode>560047</PostalCode>");
                        createPNRReq.Append("<Country>IN</Country>");
                        createPNRReq.Append("</Address>");
                    }

                    if (passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>P-C04</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    string format = Convert.ToDateTime(passengerdetails[i].dateOfBirth).ToString("ddMMMyy");
                    if (passengerdetails[i].passengertypecode == "INF")
                    {
                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>" + format + "</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                createPNRReq.Append("<ContinuityCheckOverride xmlns=\"http://www.travelport.com/schema/common_v52_0\">true</ContinuityCheckOverride>");
                createPNRReq.Append("<AgencyContactInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
                createPNRReq.Append("<PhoneNumber CountryCode=\"91\" AreaCode=\"011\" Number=\"46615790\" Location=\"DEL\" Type=\"Agency\"/>");
                createPNRReq.Append("</AgencyContactInfo>");

                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");
                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }

            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            //SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            //SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");
            }
            return res;
        }
        public string CreatePNR_1(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, string? _pricesolution = null)
        {

            int count = 0;
            //int paxCount = 0;
            //int legcount = 0;
            //string origin = string.Empty;
            //int legKeyCounter = 0;

            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"CNN\" Age=\"10\" DOB=\"2014-07-25\" >");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\" TravelerType=\"INF\" Age=\"1\" DOB=\"2023-08-25\" >");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }

                    //if (!String.IsNullOrEmpty(paxDetail.FrequentFlierNumber) && paxDetail.FrequentFlierNumber.Length > 5)
                    //{
                    //if (segment_.Bonds[0].Legs[0].AirlineName.Equals("UK"))
                    //{
                    //createPNRReq.Append("<SSR  Key='" + count + "' Type='FQTV' Status='HK' Carrier='UK' FreeText='" + paxDetail.FrequentFlierNumber + "-" + paxDetail.LastName + "/" + paxDetail.FirstName + "" + paxDetail.Title.ToUpper() + "'/>");
                    //}
                    //else
                    //{
                    //  createPNRReq.Append("<com:LoyaltyCard SupplierCode='" + segment_.Bonds[0].Legs[0].AirlineName + "' CardNumber='" + paxDetail.FrequentFlierNumber + "'/>");
                    //}
                    //}
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("<SSR  Key=\"" + count + "\" Type=\"DOCS\"  FreeText=\"P/GB/S12345678/GB/20JUL76/M/01JAN16/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                    string contractNo = string.Empty;
                    if (string.IsNullOrEmpty(contractNo))
                    {
                        contractNo = "CTCM " + passengerdetails[i].mobile + " PAX";
                    }
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");
                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }
            //}
            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            //SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            //SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");
            }
            return res;
        }
        public string RetrivePnr(string universalRlcode_, string _testURL, string newGuid, string _targetBranch, string _userName, string _password, string _AirlineWay)
        {
            StringBuilder retrivePnrReq = null;
            string pnrretriveRes = string.Empty;
            try
            {
                retrivePnrReq = new StringBuilder();
                //retrivePnrReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
                //retrivePnrReq.Append("<soap:Body>");
                //retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq xmlns:univ=\"http://www.travelport.com/schema/universal_v52_0\" AuthorizedBy=\"ENDFARE\" TargetBranch=\"" + _targetBranch + "\" TraceId=\"" + newGuid + "\">");
                //retrivePnrReq.Append("<com:BillingPointOfSaleInfo xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
                //retrivePnrReq.Append("<univ:ProviderReservationInfo ProviderLocatorCode=\"" + universalRlcode_ + "\" ProviderCode=\"1G\"/>");
                //retrivePnrReq.Append("</univ:UniversalRecordRetrieveReq>");
                //retrivePnrReq.Append("</soap:Body>");
                //retrivePnrReq.Append("</soap:Envelope>");

                retrivePnrReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
                retrivePnrReq.Append("<soap:Body>");
                retrivePnrReq.Append("<UniversalRecordRetrieveReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy=\"Travelport\" TargetBranch=\"" + _targetBranch + "\">");
                retrivePnrReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"uAPI\" />");
                retrivePnrReq.Append("<UniversalRecordLocatorCode>" + universalRlcode_ + "</UniversalRecordLocatorCode>");
                retrivePnrReq.Append("</UniversalRecordRetrieveReq>");
                retrivePnrReq.Append("</soap:Body>");
                retrivePnrReq.Append("</soap:Envelope>");


                //retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq xmlns:univ=\"http://www.travelport.com/schema/universal_v52_0\" AuthorizedBy=\"ENDFARE\" TargetBranch=\"" + _targetBranch + "\" TraceId=\"" + newGuid + "\">");
                //retrivePnrReq.Append("<com:BillingPointOfSaleInfo xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
                //retrivePnrReq.Append("<univ:ProviderReservationInfo ProviderLocatorCode=\"" + universalRlcode_ + "\" ProviderCode=\"1G\"/>");
                //retrivePnrReq.Append("</univ:UniversalRecordRetrieveReq>");




                //retrivePnrReq.Append("<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>");
                //retrivePnrReq.Append("<s:Header>");
                //retrivePnrReq.Append("<Action s:mustUnderstand='1' xmlns='http://schemas.microsoft.com/ws/2005/05/addressing/none'/>");
                //retrivePnrReq.Append("</s:Header>");
                //retrivePnrReq.Append("<s:Body xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>");
                ////if (IsDomestic)
                ////{
                ////retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq TraceId='" + GetTId(5) + "' TargetBranch='" + _ticketingCredential.Split('|')[0] + "' AuthorizedBy='user'  xmlns:univ='http://www.travelport.com/schema/universal_v46_0'>");
                //retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq xmlns:univ=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" TargetBranch=\"" + _targetBranch + "\" AuthorizedBy=\"ENDFARE\">");
                ////}
                ////else
                ////{
                ////retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq TraceId='" + GetTId(5) + "' TargetBranch='" + _ticketingCredential.Split('|')[0] + "' AuthorizedBy='user'  xmlns:univ='http://www.travelport.com/schema/universal_v46_0'>");
                ////}
                //retrivePnrReq.Append("<com:BillingPointOfSaleInfo OriginApplication=\"UAPI\" xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" />");
                //retrivePnrReq.Append("<univ:ProviderReservationInfo ProviderCode=\"1G\" ProviderLocatorCode=\"" + universalRlcode_ + "\" />");
                //retrivePnrReq.Append("</univ:UniversalRecordRetrieveReq>");
                //retrivePnrReq.Append("</s:Body>");
                //retrivePnrReq.Append("</s:Envelope>");
                pnrretriveRes = Methodshit.HttpPost(_testURL, retrivePnrReq.ToString(), _userName, _password);
            }
            catch (SystemException ex_)
            {
                //Utility.BookingTracker.LogTrackBooking(TransactionId, "[Cloud][TravelPortAPI][PnrRetriveResErr]", pnrretriveRes + "_" + sex_.Message + "_" + sex_.StackTrace, false, "", "");
            }

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs(retrivePnrReq.ToString(), "4-GetRetrievePNRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(pnrretriveRes, "4-GetRetrievePNRRes", "GDSOneWay", "oneway");

                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + retrivePnrReq + "\n\n Response: " + pnrretriveRes, "RetrivePnr", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(retrivePnrReq) + "\n\n Response: " + JsonConvert.SerializeObject(pnrretriveRes), "RetrivePnr", "GDSRT");
            }
            return pnrretriveRes;
        }

        public string RetriveEmdIssuranceBag(string EndUrl, string newGuid, string _targetBranch, string _userName, string _password, string _ProviderLocatorCode, string _UniversalLocatorCode, string _TktNum, string _Segmentkey, string _AirlineWay)
        {
            StringBuilder retriveEmdReq = null;
            string EmdretriveRes = string.Empty;
            try
            {
                //RFIC= C(Baggage) ,A(PaidSeat)
                retriveEmdReq = new StringBuilder();
                retriveEmdReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
                retriveEmdReq.Append("<soap:Body>");
                retriveEmdReq.Append("<air:EMDIssuanceReq xmlns:air=\"http://www.travelport.com/schema/air_v51_0\" xmlns:com=\"http://www.travelport.com/schema/common_v51_0\" TraceId =\"" + newGuid + "\" AuthorizedBy=\"Travelport\" ShowDetails=\"true\" TargetBranch=\"" + _targetBranch + "\" UniversalRecordLocatorCode=\"" + _UniversalLocatorCode + "\">");
                retriveEmdReq.Append("<com:BillingPointOfSaleInfo OriginApplication=\"UAPI\"/>");
                retriveEmdReq.Append("<com:ProviderReservationDetail ProviderCode=\"1G\" ProviderLocatorCode=\"" + _ProviderLocatorCode + "\" />");
                retriveEmdReq.Append("<com:TicketNumber>" + _TktNum.Trim() + "</com:TicketNumber>");
                retriveEmdReq.Append("<air:IssuanceModifiers>");
                retriveEmdReq.Append("<com:FormOfPayment Type=\"Cash\"/>");
                retriveEmdReq.Append("</air:IssuanceModifiers>");
                retriveEmdReq.Append("<air:SelectionModifiers RFIC=\"C\" SupplierCode=\"AI\">");
                int a = _Segmentkey.Split(' ').Length;
                if (a > 1)
                {
                    retriveEmdReq.Append("<air:AirSegmentRef Key=\"" + _Segmentkey.Split(' ')[0].Trim() + "\" />");
                    retriveEmdReq.Append("<air:AirSegmentRef Key=\"" + _Segmentkey.Split(' ')[1].Trim() + "\" />");

                }
                else
                {
                    retriveEmdReq.Append("<air:AirSegmentRef Key=\"" + _Segmentkey.Trim() + "\" />");
                }
                retriveEmdReq.Append("</air:SelectionModifiers></air:EMDIssuanceReq>");
                retriveEmdReq.Append("</soap:Body>");
                retriveEmdReq.Append("</soap:Envelope>");


                EmdretriveRes = Methodshit.HttpPost(EndUrl, retriveEmdReq.ToString(), _userName, _password);
            }
            catch (SystemException ex_)
            {
                //Utility.BookingTracker.LogTrackBooking(TransactionId, "[Cloud][TravelPortAPI][PnrRetriveResErr]", pnrretriveRes + "_" + sex_.Message + "_" + sex_.StackTrace, false, "", "");
            }

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs(retriveEmdReq.ToString(), "4-GetRetrieveEmdBagReq", "GDSOneWay", "oneway");
                logs.WriteLogs(EmdretriveRes, "4-GetRetrieveEmdBages", "GDSOneWay", "oneway");

                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + retrivePnrReq + "\n\n Response: " + pnrretriveRes, "RetrivePnr", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(retriveEmdReq) + "\n\n Response: " + JsonConvert.SerializeObject(EmdretriveRes), "RetrivePnr", "GDSRT");
            }
            return EmdretriveRes;
        }
        public string RetriveEmdIssuranceSeat(string EndUrl, string newGuid, string _targetBranch, string _userName, string _password, string _ProviderLocatorCode, string _UniversalLocatorCode, string _TktNum, string _Segmentkey, string _AirlineWay)
        {
            StringBuilder retriveEmdReq = null;
            string EmdretriveRes = string.Empty;
            try
            {
                //RFIC= C(Baggage) ,A(PaidSeat)
                retriveEmdReq = new StringBuilder();
                retriveEmdReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
                retriveEmdReq.Append("<soap:Body>");
                retriveEmdReq.Append("<air:EMDIssuanceReq xmlns:air=\"http://www.travelport.com/schema/air_v51_0\" xmlns:com=\"http://www.travelport.com/schema/common_v51_0\" TraceId =\"" + newGuid + "\" AuthorizedBy=\"Travelport\" ShowDetails=\"true\" TargetBranch=\"" + _targetBranch + "\" UniversalRecordLocatorCode=\"" + _UniversalLocatorCode + "\">");
                retriveEmdReq.Append("<com:BillingPointOfSaleInfo OriginApplication=\"UAPI\"/>");
                retriveEmdReq.Append("<com:ProviderReservationDetail ProviderCode=\"1G\" ProviderLocatorCode=\"" + _ProviderLocatorCode + "\" />");
                retriveEmdReq.Append("<com:TicketNumber>" + _TktNum.Trim() + "</com:TicketNumber>");
                retriveEmdReq.Append("<air:IssuanceModifiers>");
                retriveEmdReq.Append("<com:FormOfPayment Type=\"Cash\"/>");
                retriveEmdReq.Append("</air:IssuanceModifiers>");
                retriveEmdReq.Append("<air:SelectionModifiers RFIC=\"A\" SupplierCode=\"AI\">");
                int a = _Segmentkey.Split(' ').Length;
                if (a > 1)
                {
                    retriveEmdReq.Append("<air:AirSegmentRef Key=\"" + _Segmentkey.Split(' ')[0].Trim() + "\" />");
                    retriveEmdReq.Append("<air:AirSegmentRef Key=\"" + _Segmentkey.Split(' ')[1].Trim() + "\" />");

                }
                else
                {
                    retriveEmdReq.Append("<air:AirSegmentRef Key=\"" + _Segmentkey.Trim() + "\" />");
                }
                retriveEmdReq.Append("</air:SelectionModifiers></air:EMDIssuanceReq>");
                retriveEmdReq.Append("</soap:Body>");
                retriveEmdReq.Append("</soap:Envelope>");


                EmdretriveRes = Methodshit.HttpPost(EndUrl, retriveEmdReq.ToString(), _userName, _password);
            }
            catch (SystemException ex_)
            {
                //Utility.BookingTracker.LogTrackBooking(TransactionId, "[Cloud][TravelPortAPI][PnrRetriveResErr]", pnrretriveRes + "_" + sex_.Message + "_" + sex_.StackTrace, false, "", "");
            }

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs(retriveEmdReq.ToString(), "4-GetRetrieveEmdSeatReq", "GDSOneWay", "oneway");
                logs.WriteLogs(EmdretriveRes, "4-GetRetrieveEmdSeatRes", "GDSOneWay", "oneway");

                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + retrivePnrReq + "\n\n Response: " + pnrretriveRes, "RetrivePnr", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(retriveEmdReq) + "\n\n Response: " + JsonConvert.SerializeObject(EmdretriveRes), "RetrivePnr", "GDSRT");
            }
            return EmdretriveRes;
        }

        public string GetTicketdata(string universalRlcode_, string _testURL, string newGuid, string _targetBranch, string _userName, string _password, string _AirlineWay)
        {
            StringBuilder retriveTicketPnrReq = null;
            string pnrticketretriveRes = string.Empty;
            try
            {
                retriveTicketPnrReq = new StringBuilder();
                retriveTicketPnrReq.Append("<soap:Envelope xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                retriveTicketPnrReq.Append("<soap:Body>");
                //ReturnInfoOnFail =\"true\" BulkTicket=\"false\"
                retriveTicketPnrReq.Append("<AirTicketingReq TargetBranch=\"" + _targetBranch + "\" TraceId=\"" + newGuid + "\" AuthorizedBy=\"test\"  xmlns=\"http://www.travelport.com/schema/air_v52_0\">");
                retriveTicketPnrReq.Append("<BillingPointOfSaleInfo OriginApplication=\"UAPI\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                retriveTicketPnrReq.Append("<AirReservationLocatorCode>" + universalRlcode_ + "</AirReservationLocatorCode>");
                retriveTicketPnrReq.Append("</AirTicketingReq>");
                retriveTicketPnrReq.Append("</soap:Body>");
                retriveTicketPnrReq.Append("</soap:Envelope>");

                pnrticketretriveRes = Methodshit.HttpPost(_testURL, retriveTicketPnrReq.ToString(), _userName, _password);
            }
            catch (SystemException ex_)
            {
                //Utility.BookingTracker.LogTrackBooking(TransactionId, "[Cloud][TravelPortAPI][PnrRetriveResErr]", pnrretriveRes + "_" + sex_.Message + "_" + sex_.StackTrace, false, "", "");
            }

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + retriveTicketPnrReq + "\n\n Response: " + pnrticketretriveRes, "RetriveTicketPnr", "GDSOneWay");
                logs.WriteLogs(retriveTicketPnrReq.ToString(), "5-GetRetrieveTicketReq", "GDSOneWay", "oneway");
                logs.WriteLogs(pnrticketretriveRes, "5-GetRetrieveTicketRes", "GDSOneWay", "oneway");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(retriveTicketPnrReq) + "\n\n Response: " + JsonConvert.SerializeObject(pnrticketretriveRes), "RetriveTicketPnr", "GDSRT");
            }
            return pnrticketretriveRes;
        }


        //}
    }
}
