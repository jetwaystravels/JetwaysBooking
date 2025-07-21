using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Model
{
    public class RefundRequest
    {
        [Key]
        public int RefundID { get; set; }
        public string? OrderID { get; set; }
        public string? CustomerID { get; set; }
        public string? BookingID { get; set; }
        public string? RecordLocator { get; set; }
        public decimal BookingAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal? DeductionAmount { get; set; }
        public string? RefundReason { get; set; }
        public string? RefundStatus { get; set; }
        public DateTime? RequestDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? RejectionReason { get; set; }
        public bool? PaymentReversed { get; set; }
        public DateTime? LastUpdated { get; set; }

    }
}
