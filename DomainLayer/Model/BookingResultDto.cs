using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Model
{
    public class BookingResultDto
    {
        public string BookingID { get; set; }
        public string RecordLocator { get; set; }
        public DateTime BookedDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public int TotalAmount { get; set; }
        public int BookingStatus { get; set; }
        public string BookingType { get; set; }
        public string TripType { get; set; }
    }
    public class SegmentResultDto
    {
        public string BookingID { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string CarrierCode { get; set; }
        public int Identifier { get; set; }
        public string ArrivalTerminal { get; set; }
        public string DepartureTerminal { get; set; }
    }
    public class PassengerResultDto
    {
        public string BookingID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SeatNumber { get; set; }
        public string CarryBages { get; set; }
        public int TotalAmount { get; set; }
        public int TotalAmount_Tax { get; set; }

        public string MobileNumber { get; set; }
    }
}
