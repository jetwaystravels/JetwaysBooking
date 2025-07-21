using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Model
{
    public class Trips
    {
        [Key]
        public int ID { get; set; }
        public string  OutboundFlightID { get; set; }
        public string ReturnFlightID { get; set; }
        public string TripType { get; set; }
        public string TripStatus { get; set; }
        public string UserID { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
