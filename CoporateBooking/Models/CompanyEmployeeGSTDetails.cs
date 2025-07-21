namespace CoporateBooking.Models
{
    public class CompanyEmployeeGSTDetails
    {

        public string ?LegalEntityCode { get; set; }
        public string? LegalEntityName { get; set; }

       // public string EmployeeID { get; set; }
        public string ? FirstName { get; set; }
        public string ?LastName { get; set; }
        public string? MobileNumber { get; set; }
        public string? BusinessEmail { get; set; }
        public string? FrequentFlyerNumbers { get; set; }

        public string? LocationName { get; set; }
        public string ?GSTNumber { get; set; }
        public string ?GSTName { get; set; }
        public string ?GSTEmail { get; set; }

        public string ?Country { get; set; }
        public string ?State { get; set; }
        public string ?City { get; set; }
        public string ?PostalCode { get; set; }
    }
}
