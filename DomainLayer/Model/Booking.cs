using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Model
{
    public class Booking
    {
        public string ?FlightID { get; set; } = null!;
        public int AirLineID { get; set; }  
        public string? RecordLocator { get; set; }
        public string? TripType { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime? BookedDate { get; set; }
        public string? Segments { get; set; }
        public string? Passengers { get; set; }

        public string? BookingStatus { get; set; }

        public int cancelstatus { get; set; }

        public string? BookingRelationId { get; set; }


    }
}
