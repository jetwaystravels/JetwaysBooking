using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Model
{
    public class Printticket
    {
        public int Id { get; set; }
        public string PNR { get; set; } // If available
        public string BookingDoc { get; set; } // JSON stored as string
    }
}
