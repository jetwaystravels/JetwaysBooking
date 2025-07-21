using DomainLayer.Model;
using Spicejet;
using SpicejetSessionManager_;
using SpicejetBookingManager_;
using Newtonsoft.Json;
using Utility;
using Microsoft.AspNetCore.Mvc;

namespace OnionArchitectureAPI.Services.Spicejet
{
    public class _GetAvailability : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public _GetAvailability(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetSessionValue(string key, string value)
        {
            _httpContextAccessor.HttpContext.Session.SetString(key, value);
        }
        Logs logs = new Logs();
        public async Task<GetAvailabilityVer2Response> GetTripAvailability(SimpleAvailabilityRequestModel _GetfligthModel, LogonResponse _SpicejetlogonResponseobj, int TotalCount, int adultcount, int childcount, int infantcount, string flightclass, string JourneyType, string _AirlineWay = "", string Guid = "")
        {
            #region Availability
            
            
            GetAvailabilityRequest _getAvailabilityRQ = new GetAvailabilityRequest();
            _getAvailabilityRQ.Signature = _SpicejetlogonResponseobj.Signature;
            _getAvailabilityRQ.ContractVersion = 420;// _SpicejetlogonResponseobj.ContractVersion;
            _getAvailabilityRQ.TripAvailabilityRequest = new TripAvailabilityRequest();
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests = new AvailabilityRequest[1];
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0] = new AvailabilityRequest();

            if (_AirlineWay.ToLower() == "spicejetoneway" || _AirlineWay.ToLower() == "spicejetonewaycorporate")
            {
                //TempData["origin"] = _GetfligthModel.origin;
                //TempData["destination"] = _GetfligthModel.destination;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].DepartureStation = _GetfligthModel.origin;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ArrivalStation = _GetfligthModel.destination;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDateSpecified = true;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDate = Convert.ToDateTime(_GetfligthModel.beginDate);

                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDateSpecified = true;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDate = Convert.ToDateTime(_GetfligthModel.beginDate);
            }
            else
            {
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].DepartureStation = _GetfligthModel.destination;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ArrivalStation = _GetfligthModel.origin;
                //TempData["originR"] = _GetfligthModel.origin;
                //TempData["destinationR"] = _GetfligthModel.destination;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDateSpecified = true;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDate = Convert.ToDateTime(_GetfligthModel.endDate);

                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDateSpecified = true;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDate = Convert.ToDateTime(_GetfligthModel.endDate);
            }
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FlightTypeSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FlightType = FlightType.All;

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCountSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCount = Convert.ToInt16(TotalCount); //Total Travell Count

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].DowSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].Dow = DOW.Daily;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].CurrencyCode = "INR";

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].AvailabilityFilter = default;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].AvailabilityFilterSpecified = true;


            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxPriceTypes = new PaxPriceType[0];
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxPriceTypes = getPaxdetails(adultcount, childcount, infantcount); //Pax Count 1 always Default Set.
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].CarrierCode = "SG";
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareClassControlSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareClassControl = FareClassControl.CompressByProductClass;

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BookingStatusSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BookingStatus = BookingStatus.Default;
            // Different Product Class
            //string[] faretypes = { "R", "MX", "IO", "SF" };
            string[] faretypes = null;
            string[] productclasses = null;
            string[][] productclassesCorp = new string[8][];
            if (_AirlineWay.ToLower() == "spicejetonewaycorporate")
            {
                faretypes = new string[] { "R", "MX", "SF", "IO", "F", "IO", "C", "MX" };
                productclassesCorp = new string[8][];
                productclassesCorp[0] = new string[] { "RS", "SS", "SR", "SU" };
                productclassesCorp[1] = new string[] { "SC", "CS" };
                productclassesCorp[2] = new string[] { "FS", "SF" };
                productclassesCorp[3] = new string[] { "NF", "FN" }; //For IO Statutory taxes Refundable but changeable
                productclassesCorp[4] = new string[] { "XB", "BX" }; //Family Fare
                productclassesCorp[5] = new string[] { "NN" };       //For IO  Refundable and Non Changeable
                productclassesCorp[6] = new string[] { "CP", "PC" }; //Corporate 
                productclassesCorp[7] = new string[] { "CM", "MC" }; //Corporate
                var ProductClasses = productclassesCorp.SelectMany(x => x).ToArray();
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ProductClasses = ProductClasses;
            }
            else
            {
                faretypes = new string[] { "R", "MX", "SF" };
                productclasses = new string[1];
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ProductClasses = productclasses;
            }
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareTypes = faretypes;

            //string[] productclasses = new string[1];
            //string[] productclasses = {"R"};

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].MaximumConnectingFlights = 20;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].MaximumConnectingFlightsSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].LoyaltyFilterSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].LoyaltyFilter = LoyaltyFilter.MonetaryOnly;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].IncludeTaxesAndFees = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].IncludeTaxesAndFeesSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareRuleFilterSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareRuleFilter = FareRuleFilter.Default;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ServiceBundleControlSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ServiceBundleControl = ServiceBundleControl.Disabled;

            _getapi objSpicejet = new _getapi();
            GetAvailabilityVer2Response _getAvailabilityVer2Response = await objSpicejet.GetTripAvailability(_getAvailabilityRQ);
            if (_AirlineWay.ToLower() == "spicejetoneway" || _AirlineWay.ToLower() == "spicejetonewaycorporate")
            {
                SetSessionValue("SpicejetAvailibilityRequest", JsonConvert.SerializeObject(_getAvailabilityRQ));
                SetSessionValue("SpicejetSignature", JsonConvert.SerializeObject(_getAvailabilityRQ.Signature));

                logs.WriteLogs(JsonConvert.SerializeObject(_getAvailabilityRQ), "2-GetAvailabilityReq", "SpicejetOneWay", JourneyType);
                logs.WriteLogs(JsonConvert.SerializeObject(_getAvailabilityVer2Response), "2-GetAvailabilityRes", "SpicejetOneWay", JourneyType);
            }
            else
            {
                SetSessionValue("SpicejetAvailibilityRequest", JsonConvert.SerializeObject(_getAvailabilityRQ));
                SetSessionValue("SpicejetReturnSignature", JsonConvert.SerializeObject(_getAvailabilityRQ.Signature));

                logs.WriteLogsR(JsonConvert.SerializeObject(_getAvailabilityRQ), "2-GetAvailabilityReq", "SpicejetRT");
                logs.WriteLogsR(JsonConvert.SerializeObject(_getAvailabilityVer2Response), "2-GetAvailabilityRes", "SpicejetRT");
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_getAvailabilityRQ) + "\n\n Response: " + JsonConvert.SerializeObject(_getAvailabilityVer2Response), "GetAvailability", "SpicejetRT");
            }
            return (GetAvailabilityVer2Response)_getAvailabilityVer2Response;
            #endregion

        }

        public async Task<GetAvailabilityVer2Response> GetTripAvailabilityCorporate(SimpleAvailabilityRequestModel _GetfligthModel, LogonResponse _SpicejetlogonResponseobj, int TotalCount, int adultcount, int childcount, int infantcount, string flightclass, string JourneyType, string _AirlineWay = "", string Guid = "")
        {
            #region Availability


            GetAvailabilityRequest _getAvailabilityRQ = new GetAvailabilityRequest();
            _getAvailabilityRQ.Signature = _SpicejetlogonResponseobj.Signature;
            _getAvailabilityRQ.ContractVersion = 420;// _SpicejetlogonResponseobj.ContractVersion;
            _getAvailabilityRQ.TripAvailabilityRequest = new TripAvailabilityRequest();
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests = new AvailabilityRequest[1];
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0] = new AvailabilityRequest();

            if (_AirlineWay.ToLower() == "spicejetoneway" || _AirlineWay.ToLower() == "spicejetonewaycorporate")
            {
                //TempData["origin"] = _GetfligthModel.origin;
                //TempData["destination"] = _GetfligthModel.destination;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].DepartureStation = _GetfligthModel.origin;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ArrivalStation = _GetfligthModel.destination;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDateSpecified = true;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDate = Convert.ToDateTime(_GetfligthModel.beginDate);

                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDateSpecified = true;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDate = Convert.ToDateTime(_GetfligthModel.beginDate);
            }
            else
            {
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].DepartureStation = _GetfligthModel.destination;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ArrivalStation = _GetfligthModel.origin;
                //TempData["originR"] = _GetfligthModel.origin;
                //TempData["destinationR"] = _GetfligthModel.destination;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDateSpecified = true;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDate = Convert.ToDateTime(_GetfligthModel.endDate);

                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDateSpecified = true;
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDate = Convert.ToDateTime(_GetfligthModel.endDate);
            }
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FlightTypeSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FlightType = FlightType.All;

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCountSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCount = Convert.ToInt16(TotalCount); //Total Travell Count

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].DowSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].Dow = DOW.Daily;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].CurrencyCode = "INR";

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].AvailabilityFilter = default;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].AvailabilityFilterSpecified = true;


            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxPriceTypes = new PaxPriceType[0];
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxPriceTypes = getPaxdetails(adultcount, childcount, infantcount); //Pax Count 1 always Default Set.
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].CarrierCode = "SG";
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareClassControlSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareClassControl = FareClassControl.CompressByProductClass;

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BookingStatusSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BookingStatus = BookingStatus.Default;
            // Different Product Class
            //string[] faretypes = { "R", "MX", "IO", "SF" };
            string[] faretypes = null;
            string[] productclasses = null;
            string[][] productclassesCorp = new string[8][];
            if (_AirlineWay.ToLower() == "spicejetonewaycorporate")
            {
                faretypes = new string[] { "R", "MX", "SF", "IO", "F", "IO", "C", "MX" };
                productclassesCorp = new string[8][];
                productclassesCorp[0] = new string[] { "RS", "SS", "SR", "SU" };
                productclassesCorp[1] = new string[] { "SC", "CS" };
                productclassesCorp[2] = new string[] { "FS", "SF" };
                productclassesCorp[3] = new string[] { "NF", "FN" }; //For IO Statutory taxes Refundable but changeable
                productclassesCorp[4] = new string[] { "XB", "BX" }; //Family Fare
                productclassesCorp[5] = new string[] { "NN" };       //For IO  Refundable and Non Changeable
                productclassesCorp[6] = new string[] { "CP", "PC" }; //Corporate 
                productclassesCorp[7] = new string[] { "CM", "MC" }; //Corporate
                var ProductClasses = productclassesCorp.SelectMany(x => x).ToArray();
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ProductClasses = ProductClasses;
            }

            else
            {
                faretypes = new string[] { "R", "MX", "SF", "IO", "F", "IO", "C", "MX" };
                productclassesCorp = new string[8][];
                productclassesCorp[0] = new string[] { "RS", "SS", "SR", "SU" };
                productclassesCorp[1] = new string[] { "SC", "CS" };
                productclassesCorp[2] = new string[] { "FS", "SF" };
                productclassesCorp[3] = new string[] { "NF", "FN" }; //For IO Statutory taxes Refundable but changeable
                productclassesCorp[4] = new string[] { "XB", "BX" }; //Family Fare
                productclassesCorp[5] = new string[] { "NN" };       //For IO  Refundable and Non Changeable
                productclassesCorp[6] = new string[] { "CP", "PC" }; //Corporate 
                productclassesCorp[7] = new string[] { "CM", "MC" }; //Corporate
                var ProductClasses = productclassesCorp.SelectMany(x => x).ToArray();
                _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ProductClasses = ProductClasses;
                //faretypes = new string[] { "R", "MX", "SF" };
                //productclasses = new string[1];
                //_getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ProductClasses = productclasses;
            }
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareTypes = faretypes;

            //string[] productclasses = new string[1];
            //string[] productclasses = {"R"};

            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].MaximumConnectingFlights = 20;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].MaximumConnectingFlightsSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].LoyaltyFilterSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].LoyaltyFilter = LoyaltyFilter.MonetaryOnly;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].IncludeTaxesAndFees = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].IncludeTaxesAndFeesSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareRuleFilterSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareRuleFilter = FareRuleFilter.Default;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ServiceBundleControlSpecified = true;
            _getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].ServiceBundleControl = ServiceBundleControl.Disabled;

            _getapi objSpicejet = new _getapi();
            GetAvailabilityVer2Response _getAvailabilityVer2Response = await objSpicejet.GetTripAvailability(_getAvailabilityRQ);
            if (_AirlineWay.ToLower() == "spicejetoneway" || _AirlineWay.ToLower() == "spicejetonewaycorporate")
            {
                SetSessionValue("SpicejetAvailibilityRequest", JsonConvert.SerializeObject(_getAvailabilityRQ));
                SetSessionValue("SpicejetSignature", JsonConvert.SerializeObject(_getAvailabilityRQ.Signature));

                logs.WriteLogs(JsonConvert.SerializeObject(_getAvailabilityRQ), "2-GetAvailabilityReq", "SpicejetOneWay", JourneyType);
                logs.WriteLogs(JsonConvert.SerializeObject(_getAvailabilityVer2Response), "2-GetAvailabilityRes", "SpicejetOneWay", JourneyType);
            }
            else
            {
                SetSessionValue("SpicejetAvailibilityRequest", JsonConvert.SerializeObject(_getAvailabilityRQ));
                SetSessionValue("SpicejetReturnSignature", JsonConvert.SerializeObject(_getAvailabilityRQ.Signature));

                logs.WriteLogsR(JsonConvert.SerializeObject(_getAvailabilityRQ), "2-GetAvailabilityReq", "SpicejetRT");
                logs.WriteLogsR(JsonConvert.SerializeObject(_getAvailabilityVer2Response), "2-GetAvailabilityRes", "SpicejetRT");
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_getAvailabilityRQ) + "\n\n Response: " + JsonConvert.SerializeObject(_getAvailabilityVer2Response), "GetAvailability", "SpicejetRT");
            }
            return (GetAvailabilityVer2Response)_getAvailabilityVer2Response;
            #endregion

        }



        PaxPriceType[] getPaxdetails(int adult_, int child_, int infant_)
        {
            PaxPriceType[] paxPriceTypes = null;
            try
            {
                //int tcount = adult_ + child_ + infant_;
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
                    //paxPriceTypes[j].PaxCount = Convert.ToInt16(0);
                    j++;
                }

                if (child_ > 0)
                {
                    paxPriceTypes[j] = new PaxPriceType();
                    paxPriceTypes[j].PaxType = "CHD";
                    paxPriceTypes[j].PaxCountSpecified = true;
                    paxPriceTypes[j].PaxCount = Convert.ToInt16(child_);
                    //paxPriceTypes[j].PaxCount = Convert.ToInt16(0);
                    j++;
                }

                if (infant_ > 0)
                {
                    paxPriceTypes[j] = new PaxPriceType();
                    paxPriceTypes[j].PaxType = "INFT";
                    paxPriceTypes[j].PaxCountSpecified = true;
                    paxPriceTypes[j].PaxCount = Convert.ToInt16(infant_);
                    //paxPriceTypes[j].PaxCount = Convert.ToInt16(0);
                    j++;
                }
            }
            catch (Exception e)
            {
            }

            return paxPriceTypes;
        }
    }
}
