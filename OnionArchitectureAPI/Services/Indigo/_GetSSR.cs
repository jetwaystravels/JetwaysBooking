﻿using DomainLayer.Model;
using Indigo;
using IndigoBookingManager_;
using Newtonsoft.Json;
using Utility;

namespace OnionArchitectureAPI.Services.Indigo
{
    public class _GetSSR
    {
        Logs logs = new Logs();
        public async Task<List<GetSeatAvailabilityResponse>> GetseatAvailability(string Signature, AirAsiaTripResponceModel AirAsiaTripResponceobj,int p, string _AirlineWay = "")
        {

            GetSeatAvailabilityRequest _getseatAvailabilityRequest = new GetSeatAvailabilityRequest();
            GetSeatAvailabilityResponse _getSeatAvailabilityResponse = new GetSeatAvailabilityResponse();

            _getseatAvailabilityRequest.Signature = Signature;
            _getseatAvailabilityRequest.ContractVersion = 456;

            SeatAvailabilityRequest _seatRequest = new SeatAvailabilityRequest();
            List<GetSeatAvailabilityResponse> SeatGroup = new List<GetSeatAvailabilityResponse>();
            if (AirAsiaTripResponceobj.journeys.Count > 0)
            {
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
                    _seatRequest.EnforceSeatGroupRestrictions = false;
                    _getseatAvailabilityRequest.SeatAvailabilityRequest = _seatRequest;
                    _getapiIndigo _obj = new _getapiIndigo();
                    _getSeatAvailabilityResponse = await _obj.GetseatAvailability(_getseatAvailabilityRequest);
                    SeatGroup.Add(_getSeatAvailabilityResponse);

                }
            }

            string str1 = JsonConvert.SerializeObject(SeatGroup);
            if (_AirlineWay.ToLower() == "oneway")
            {
                //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_getseatAvailabilityRequest) + "\n\n Response: " + JsonConvert.SerializeObject(SeatGroup), "GetSeatAvailability", "IndigoOneWay", "oneway");
                logs.WriteLogs(JsonConvert.SerializeObject(_getseatAvailabilityRequest), "8-GetSeatAvailabilityReq", "IndigoOneWay", "oneway");
                logs.WriteLogs(JsonConvert.SerializeObject(SeatGroup), "8-GetSeatAvailabilityRes", "IndigoOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_getseatAvailabilityRequest) + "\n\n Response: " + JsonConvert.SerializeObject(SeatGroup), "GetSeatAvailability", "IndigoRT");
                if (p == 0)
                {
                    logs.WriteLogsR(JsonConvert.SerializeObject(_getseatAvailabilityRequest), "6-GetSeatAvailabilityReq_Left", "IndigoRT");
                    logs.WriteLogsR(JsonConvert.SerializeObject(SeatGroup), "6-GetSeatAvailabilityRes_Left", "IndigoRT");
                }
                else
                {
                    logs.WriteLogsR(JsonConvert.SerializeObject(_getseatAvailabilityRequest), "6-GetSeatAvailabilityReq_Right", "IndigoRT");
                    logs.WriteLogsR(JsonConvert.SerializeObject(SeatGroup), "6-GetSeatAvailabilityRes_Right", "IndigoRT");
                }
            }
            return SeatGroup;

        }

        public async Task<List<IndigoBookingManager_.GetSeatAvailabilityResponse>> GetseatAvailability(string Signature, AirAsiaTripResponceModel AirAsiaTripResponceobj, string _AirlineWay = "")
        {
            IndigoBookingManager_.GetSeatAvailabilityRequest _getseatAvailabilityRequest = new IndigoBookingManager_.GetSeatAvailabilityRequest();
            IndigoBookingManager_.GetSeatAvailabilityResponse _getSeatAvailabilityResponse = new IndigoBookingManager_.GetSeatAvailabilityResponse();

            _getseatAvailabilityRequest.Signature = Signature;
            _getseatAvailabilityRequest.ContractVersion = 452;

            IndigoBookingManager_.SeatAvailabilityRequest _seatRequest = new IndigoBookingManager_.SeatAvailabilityRequest();
            List<IndigoBookingManager_.GetSeatAvailabilityResponse> SeatGroup = new List<IndigoBookingManager_.GetSeatAvailabilityResponse>();
            if (AirAsiaTripResponceobj.journeys.Count > 0)
            {
                for (int a = 0; a < AirAsiaTripResponceobj.journeys.Count; a++)
                {
                    for (int i = 0; i < AirAsiaTripResponceobj.journeys[a].segments.Count; i++)
                    {
                        _seatRequest = new IndigoBookingManager_.SeatAvailabilityRequest();
                        _seatRequest.STDSpecified = true;
                        _seatRequest.STD = AirAsiaTripResponceobj.journeys[a].segments[i].designator.departure;
                        _seatRequest.DepartureStation = AirAsiaTripResponceobj.journeys[a].segments[i].designator.origin;
                        _seatRequest.ArrivalStation = AirAsiaTripResponceobj.journeys[a].segments[i].designator.destination;
                        _seatRequest.IncludeSeatFees = true;
                        _seatRequest.IncludeSeatFeesSpecified = true;
                        _seatRequest.SeatAssignmentModeSpecified = true;
                        _seatRequest.SeatAssignmentMode = IndigoBookingManager_.SeatAssignmentMode.PreSeatAssignment;
                        _seatRequest.FlightNumber = AirAsiaTripResponceobj.journeys[a].segments[i].identifier.identifier;
                        _seatRequest.OverrideSTDSpecified = true;
                        _seatRequest.OverrideSTD = AirAsiaTripResponceobj.journeys[a].segments[i].designator.departure;
                        _seatRequest.CarrierCode = AirAsiaTripResponceobj.journeys[a].segments[i].identifier.carrierCode;
                        _seatRequest.EnforceSeatGroupRestrictions = false;
                        _getseatAvailabilityRequest.SeatAvailabilityRequest = _seatRequest;
                        _getapiIndigo _obj = new _getapiIndigo();
                        _getSeatAvailabilityResponse = await _obj.GetseatAvailability(_getseatAvailabilityRequest, a, i);
                        SeatGroup.Add(_getSeatAvailabilityResponse);

                    }
                }

            }

            string str1 = JsonConvert.SerializeObject(SeatGroup);
            if (_AirlineWay.ToLower() == "oneway")
            {
                logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_getseatAvailabilityRequest) + "\n\n Response: " + JsonConvert.SerializeObject(SeatGroup), "GetSeatAvailability", "IndigoOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_getseatAvailabilityRequest) + "\n\n Response: " + JsonConvert.SerializeObject(SeatGroup), "GetSeatAvailability", "SameIndigoRT");
            }
            return SeatGroup;

        }
        public async Task<List<IndigoBookingManager_.GetSSRAvailabilityForBookingResponse>> GetSSRAvailabilityForBooking(string Signature, AirAsiaTripResponceModel passeengerlist, int TotalCount, string _AirlineWay = "")
        {
            List<IndigoBookingManager_.GetSSRAvailabilityForBookingResponse> SSRGroup = new List<IndigoBookingManager_.GetSSRAvailabilityForBookingResponse>();
            IndigoBookingManager_.GetSSRAvailabilityForBookingRequest _req = new IndigoBookingManager_.GetSSRAvailabilityForBookingRequest();
            IndigoBookingManager_.GetSSRAvailabilityForBookingResponse _res = new IndigoBookingManager_.GetSSRAvailabilityForBookingResponse();
            try
            {
                int segmentcount = 0;
                int journeyscount = passeengerlist.journeys.Count;
                _req.Signature = Signature;
                _req.ContractVersion = 456;
                IndigoBookingManager_.SSRAvailabilityForBookingRequest _SSRAvailabilityForBookingRequest = new IndigoBookingManager_.SSRAvailabilityForBookingRequest();

                for (int i = 0; i < journeyscount; i++)
                {
                    int segmentscount = passeengerlist.journeys[i].segments.Count;
                    _SSRAvailabilityForBookingRequest.SegmentKeyList = new IndigoBookingManager_.LegKey[segmentscount];
                    for (int j = 0; j < segmentscount; j++)
                    {
                        _SSRAvailabilityForBookingRequest.SegmentKeyList[j] = new IndigoBookingManager_.LegKey();
                        int legcount = passeengerlist.journeys[i].segments[j].legs.Count;
                        for (int n = 0; n < legcount; n++)
                        {
                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].CarrierCode = passeengerlist.journeys[i].segments[j].identifier.carrierCode;
                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].FlightNumber = passeengerlist.journeys[i].segments[j].identifier.identifier;
                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].DepartureDateSpecified = true;
                            //string strdate = Convert.ToDateTime(passengerdetails.departure).ToString("yyyy-MM-dd");
                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].DepartureDate = Convert.ToDateTime(passeengerlist.journeys[i].segments[j].designator.departure);//DateTime.ParseExact(strdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].ArrivalStation = passeengerlist.journeys[i].segments[j].designator.destination;
                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].DepartureStation = passeengerlist.journeys[i].segments[j].designator.origin;
                            segmentcount++;
                        }
                    }

                    _SSRAvailabilityForBookingRequest.PassengerNumberList = new short[Convert.ToInt16(TotalCount)];//new short[1];
                    int paxCount = _SSRAvailabilityForBookingRequest.PassengerNumberList.Length;//passeengerlist.passengerscount;
                    for (int a = 0; a < paxCount; a++)
                    {
                        if (a > 0)
                            continue;
                        _SSRAvailabilityForBookingRequest.PassengerNumberList[a] = Convert.ToInt16(a);
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
                    _SSRAvailabilityForBookingRequest.SSRAvailabilityMode = IndigoBookingManager_.SSRAvailabilityMode.NonBundledSSRs;
                    _SSRAvailabilityForBookingRequest.SSRAvailabilityModeSpecified = true;
                    _req.SSRAvailabilityForBookingRequest = _SSRAvailabilityForBookingRequest;
                    _getapiIndigo _obj = new _getapiIndigo();
                    _res = await _obj.GetMealAvailabilityForBooking(_req);
                    SSRGroup.Add(_res);
                    string Str2 = JsonConvert.SerializeObject(SSRGroup);
                    if (_AirlineWay.ToLower() == "oneway")
                    {
                        logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_req) + "\n\n Response: " + JsonConvert.SerializeObject(_res), "GetSSRAvailabilityForBooking", "IndigoOneWay");
                    }
                    else
                    {
                        logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_req) + "\n\n Response: " + JsonConvert.SerializeObject(_res), "GetSSRAvailabilityForBooking", "SameIndigoRT");
                    }
                }


                return SSRGroup;

            }
            catch (Exception ex)
            {

            }
            return SSRGroup;
        }

        public async Task<IndigoBookingManager_.GetSSRAvailabilityForBookingResponse> GetSSRAvailabilityForBooking(string Signature, AirAsiaTripResponceModel passeengerlist, int TotalCount, string _journeySide, string _AirlineWay = "")
        {
            IndigoBookingManager_.GetSSRAvailabilityForBookingRequest _req = new IndigoBookingManager_.GetSSRAvailabilityForBookingRequest();
            IndigoBookingManager_.GetSSRAvailabilityForBookingResponse _res = new IndigoBookingManager_.GetSSRAvailabilityForBookingResponse();
            try
            {
                int segmentcount = 0;
                int journeyscount = passeengerlist.journeys.Count;
                _req.Signature = Signature;
                _req.ContractVersion = 456;
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
                            //string strdate = Convert.ToDateTime(passengerdetails.departure).ToString("yyyy-MM-dd");
                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].DepartureDate = Convert.ToDateTime(passeengerlist.journeys[i].segments[j].designator.departure);//DateTime.ParseExact(strdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].ArrivalStation = passeengerlist.journeys[i].segments[j].designator.destination;
                            _SSRAvailabilityForBookingRequest.SegmentKeyList[j].DepartureStation = passeengerlist.journeys[i].segments[j].designator.origin;
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
                _getapiIndigo _obj = new _getapiIndigo();
                _res = await _obj.GetMealAvailabilityForBooking(_req);

                //string Str2 = JsonConvert.SerializeObject(_res);
                if (_AirlineWay.ToLower() == "oneway")
                {
                    //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_req) + "\n\n Response: " + JsonConvert.SerializeObject(_res), "GetSSRAvailabilityForBooking", "IndigoOneWay", "oneway");
                    logs.WriteLogs(JsonConvert.SerializeObject(_req), "7-GetSSRAvailabilityForBookingReq", "IndigoOneWay", "oneway");
                    logs.WriteLogs(JsonConvert.SerializeObject(_res), "7-GetSSRAvailabilityForBookingRes", "IndigoOneWay", "oneway");
                }
                else
                {
                    //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_req) + "\n\n Response: " + JsonConvert.SerializeObject(_res), "GetSSRAvailabilityForBooking", "IndigoRT");
                    if (_journeySide == "0j")
                    {
                        logs.WriteLogsR(JsonConvert.SerializeObject(_req), "7-GetSSRAvailabilityForBookingReq_Left", "IndigoRT");
                        logs.WriteLogsR(JsonConvert.SerializeObject(_res), "7-GetSSRAvailabilityForBookingRes_Left", "IndigoRT");
                    }
                    else
                    {
                        logs.WriteLogsR(JsonConvert.SerializeObject(_req), "7-GetSSRAvailabilityForBookingReq_Right", "IndigoRT");
                        logs.WriteLogsR(JsonConvert.SerializeObject(_res), "7-GetSSRAvailabilityForBookingRes_Right", "IndigoRT");
                    }
                }
                return (GetSSRAvailabilityForBookingResponse)_res;

            }
            catch (Exception ex)
            {

            }
            return (GetSSRAvailabilityForBookingResponse)_res;
        }

    }
}

