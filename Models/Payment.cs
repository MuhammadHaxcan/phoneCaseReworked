using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace phoneCaseReworked.Models {
    public class Payment {
        [Key]
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "Vendor ID is required.")]
        [ForeignKey("Vendor")]
        public int VendorId { get; set; }

        public Vendor Vendor { get; set; } // Navigation Property

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1000, 100000, ErrorMessage = "Amount must be between 1000 and 100000.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        [Column(TypeName = "date")]
        public DateTime PaymentDate { get; set; }
    }
}
