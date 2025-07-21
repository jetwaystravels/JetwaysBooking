namespace OnionConsumeWebAPI.Extensions

{
    public static class AppUrlConstant
    {
        public static string BaseURL = "http://localhost:5225/";
       // public static string BaseURL = "http://192.168.1.104/";
        public static string GDSURL = "https://apac.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/AirService";
        public static string GDSSeatURL = "https://apac.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/AirService";
        public static string GDSSSRURL = "https://apac.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/ReferenceDataLookupService";
        public static string GDSUniversalRecordURL = "https://apac.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/UniversalRecordService";
        public static string GDSEmdURL = "https://apac.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/AirService";
        public static string URLAirasia = "https://dotrezapi.test.I5.navitaire.com";
        public static string URLAkasaAir = "https://tbnk-reyalrb.qp.akasaair.com";
        public static string Corporatelogin = BaseURL + "api/Admin/LogIn";
        public static string AirlineLogin = "api/Login/GetAirlinelogin";

        #region AirAsia URLs
        public static string AirasiaTokan = URLAirasia + "/api/nsk/v1/token";
        public static string Airasiasearchsimple = URLAirasia + "/api/nsk/v4/availability/search/simple";
        public static string AirasiasearchsimpleR = URLAirasia + "/api/nsk/v4/availability/search/simple";
        public static string AirasiaTripsell = URLAirasia + "/api/nsk/v4/trip/sell";
        public static string Airasiainfantquote = URLAirasia + "/api/nsk/v2/bookings/quote";
        public static string Airasiaseatmap = URLAirasia + "/api/nsk/v3/booking/seatmaps/journey/";
        public static string Airasiassravailability = URLAirasia + "/api/nsk/v2/booking/ssrs/availability";
        public static string AirasiaContactDetail = URLAirasia + "/api/nsk/v1/booking/contacts";
        public static string AirasiaAddPassenger = URLAirasia + "/api/nsk/v3/booking/passengers/";
        public static string AirasiaAddPassengerInfant = URLAirasia + "/api/nsk/v3/booking/passengers/";
        public static string AirasiaGetBoking = URLAirasia + "/api/nsk/v1/booking";
        public static string AirasiaGstDetail = URLAirasia + "/api/nsk/v1/booking/contacts";
        public static string AirasiaAutoSeat = URLAirasia + "/api/nsk/v1/booking/seats/auto/";
        public static string AirasiaSeatSelect = URLAirasia + "/api/nsk/v2/booking/passengers/";
        public static string AirasiaMealSelect = URLAirasia + "/api/nsk/v2/booking/ssrs/";
        public static string AirasiaCommitBooking = URLAirasia + "/api/nsk/v3/booking";
        public static string AirasiaPNRBooking = URLAirasia + "/api/nsk/v1/booking/retrieve/byRecordLocator/";
        public static string AirasiaPayment = URLAirasia + "/api/nsk/v4/booking/payments";
        #endregion

        #region AkasaAir URLs
        public static string AkasaTokan = URLAkasaAir + "/api/nsk/v2/token";
        public static string AkasaAirSearchSimple = URLAkasaAir + "/api/nsk/v4/availability/search/simple";
        public static string AkasaAirTripsell = URLAkasaAir + "/api/nsk/v4/trip/sell";
        public static string AkasasearchsimpleR = URLAkasaAir + "/api/nsk/v4/availability/search/simple";
        public static string AkasaGetBoking = URLAkasaAir + "/api/nsk/v3/booking";
        public static string AkasaCommitBooking = URLAkasaAir + "/api/nsk/v3/booking";
        public static string AkasaPNRBooking = URLAkasaAir + "/api/nsk/v1/booking";
        public static string AkasaAirSeatMap = URLAkasaAir + "/api/nsk/v3/booking/seatmaps/journey/";
        //Contact
        public static string AkasaAirContactDetails = URLAkasaAir + "/api/nsk/v1/booking/contacts";//yes
                                                                                                   //Passenger

        public static string Akasainfant = URLAkasaAir + "/api/nsk/v2/bookings/quote";
        public static string AkasaAirPassengerDetails = URLAkasaAir + "/api/nsk/v3/booking/passengers/";
        //Infant
        public static string AkasaAirInfantDetails = URLAkasaAir + "/api/nsk/v3/booking/passengers/";
        //SeatAssign
        public static string AkasaAirSeatAssign = URLAkasaAir + "/api/nsk/v2/booking/passengers/";
        //Sell Ssr
        public static string AkasaAirMealBaggage = URLAkasaAir + "/api/nsk/v2/booking/ssrs/availability";
        // Post//Sell ssr
        public static string AkasaAirMealBaggagePost = URLAkasaAir + "/api/nsk/v2/booking/ssrs/";
        public static string AkasaAirCommitBooking = URLAkasaAir + "/api/nsk/v3/booking";
        public static string AkasaAirGetBooking = URLAkasaAir + "/api/nsk/v1/booking";
        public static string AkasaAirPayment = URLAkasaAir + "/api/nsk/v2/booking/payments";

        public static string AkasaAirPNRBooking = URLAkasaAir + "/api/nsk/v1/booking/retrieve/byRecordLocator/";
        #endregion

    }
}
