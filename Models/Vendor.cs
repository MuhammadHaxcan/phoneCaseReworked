using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace phoneCaseReworked.Models {
    public class Vendor {
        [Key]
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Vendor name is required.")]
        [StringLength(50, ErrorMessage = "Vendor name cannot exceed 50 characters.")] 
        [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = "Vendor name can only contain letters, numbers, spaces, and hyphens.")]
        [MinLength(3,ErrorMessage = "Minimum Vendor name length is 3 characters.")]
        public string Name { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "Contact info must be exactly 11 digits.")]
        public string ContactInfo { get; set; }

        [Required(ErrorMessage = "Total credit is required.")]
        [Range(0, 1000000, ErrorMessage = "Total credit must be between 0 and 1,000,000.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCredit { get; set; } = 0.00m;
    }
}
