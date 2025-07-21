using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Model
{
    public class FullBookingDetailsDto
    {
        public BookingResultDto Booking { get; set; }
        public List<SegmentResultDto> Segments { get; set; }
        public List<PassengerResultDto> Passengers { get; set; }
    }
}
