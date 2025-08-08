namespace CoporateBooking.Models
{
    public class CustomerDealCodes
    {
        public int dealCodeID { get; set; }
        public string legalEntityCode { get; set; }
        public int supplierId { get; set; }
        public string dealCodeName { get; set; }
        public int iataGroup { get; set; }
        public string travelType { get; set; }
        public string associatedFareTypes { get; set; }
        public DateTime expiryDate { get; set; }
        public string dealPricingCode { get; set; }
        public string tourCode { get; set; }
        public string dealCodeType { get; set; }
        public string classOfSeats { get; set; }
        public bool gstMandatory { get; set; }
        public DateTime startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string bookingType { get; set; }
        public int status { get; set; }
    }
}
