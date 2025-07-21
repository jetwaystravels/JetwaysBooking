namespace CoporateBooking.Models
{
    public class EntityInfo
    {
   
            public string LegalEntityName { get; set; }
            public string LegalUserId { get; set; }
            public string BillingEntityName { get; set; }
            public double? Balance { get; set; }
            public string BillingEntityFullName { get; set; }
            public string LegalEntityFullName { get; set; }
            public string EmployeeFullName { get; set; }
            public string SearchOriginCode { get; set; }
            public string SearchdestCode { get; set; }
            public string SearchOrigin { get; set; }

        public string Searchdest { get; set; }

        

        public string SearchLogDeptDateTime { get; set; }
        public string SearchLogArrDateTime { get; set; }

    }
}
