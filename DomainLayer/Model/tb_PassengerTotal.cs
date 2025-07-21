using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Model
{
    public class tb_PassengerTotal
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("tb_booking")]
        public string BookingID { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public int InfantCount { get; set; }
        public int TotalPax { get; set; }
        public double SpecialServicesAmount { get; set; }
        public double SpecialServicesAmount_Tax { get; set; }
        public double TotalSeatAmount { get; set; }
        public double TotalSeatAmount_Tax { get; set; }
        public double SeatAdjustment{ get; set; }
        public double TotalBookingAmount { get; set; }
        public double totalBookingAmount_Tax { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Createdby { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Modifyby { get; set; }
        public string Status { get; set; }
    }
}
