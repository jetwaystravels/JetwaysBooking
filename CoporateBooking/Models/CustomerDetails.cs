namespace CoporateBooking.Models
{
    public class CustomerDetails
    {

        public string LegalEntityName { get; set; }
        public string ?EmployeeNames { get; set; }

        public decimal? PresentBalance { get; set; }
    }
}
