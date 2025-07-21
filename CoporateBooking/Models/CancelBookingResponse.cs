namespace CoporateBooking.Models
{
    public class CancelBookingResponse
    {
        public BookingData data { get; set; }

        public class BookingData
        {
            public string recordLocator { get; set; }
            public Breakdown breakdown { get; set; }
            public Dictionary<string, Passenger> passengers { get; set; }
            public List<Journey> journeys { get; set; }
        }

        public class Breakdown
        {
            public decimal totalAmount { get; set; }
            public decimal totalToCollect { get; set; }
        }

        public class Passenger
        {
            public string passengerKey { get; set; }
            public PassengerName name { get; set; }
        }

        public class PassengerName
        {
            public string first { get; set; }
            public string last { get; set; }
        }

        public class Journey
        {
            public JourneyDesignator designator { get; set; }
            public List<Segment> segments { get; set; }
        }
        public class Segment
        {
            public bool isStandby { get; set; }
            public SegmentDesignator designator { get; set; }
            public SegmentIdentifier identifier { get; set; }
            public List<Fare> fares { get; set; }
        }
        public class SegmentIdentifier
        {
            public string identifier { get; set; }
            public string carrierCode { get; set; }
        }
        public class Fare
        {
            public string fareBasisCode { get; set; }
            public string classOfService { get; set; }
            public string fareClassOfService { get; set; }
        }
        public class SegmentDesignator
        {
            public string origin { get; set; }
            public string destination { get; set; }
            public DateTime departure { get; set; }
            public DateTime arrival { get; set; }
        }

        public class JourneyDesignator
        {
            public string origin { get; set; }
            public string destination { get; set; }
            public DateTime departure { get; set; }
            public DateTime arrival { get; set; }
        }

    }
}
