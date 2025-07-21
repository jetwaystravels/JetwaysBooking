using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CoporateBooking.Models
{
    public class LegalEntity
    {
        public ObjectId _id;
        public string LegalName;
        public string Employee;
        public string BillingEntityName;
        public double? Balance;
        public string SuppId;
        public string Guid;

        public string UserId;

        public string Username;
        public string Password;
        public string Email;


        public string LegalFullName;
        public string BillingEntityFullName;
        public string EmployeeFullName;

        public DateTime CreatedDate;

    }
}
