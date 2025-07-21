using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Model
{
    public class AkasaSSRavailRequest
    {
        public List<TripAA> trips { get; set; }
        public string[] passengerKeys { get; set; }
        public string currencyCode { get; set; } = "INR";

    }
    

    public class TripAA
    {
        //public List<TripIdentifier> identifier { get; set; }
        public TripIdentifier identifier { get; set; }
        public string origin { get; set; }
        public string destination { get; set; }
      
        public string departureDate { get; set; }
    }

    //public class TripIdentifierAA
    //{
    //    public string identifier { get; set; }
    //    public string carrierCode { get; set; }
    //}

}
