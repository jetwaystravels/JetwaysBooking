using static DomainLayer.Model.ReturnAirLineTicketBooking;

namespace CoporateBooking.Models
{
    public class BookingResponceview
    {
        public string BookingID { get; set; }
        public string RecordLocator { get; set; }
        public DateTime BookedDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string BookingStatus { get; set; }
        public string BookingType { get; set; }
        public string TripType { get; set; }
        public List<SegmentDto> Segments { get; set; }
        public List<PassengerDto> Passengers { get; set; }
    }
    public class SegmentDto
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string CarrierCode { get; set; }
        public string Identifier { get; set; }
        public string ArrivalTerminal { get; set; }
        public string DepartureTerminal { get; set; }
    }
    public class PassengerDto
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Seatnumber { get; set; }
        public string Carrybages { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmount_tax { get; set; }
    }
}
