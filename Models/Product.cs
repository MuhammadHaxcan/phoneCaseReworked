using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace phoneCaseReworked.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [ForeignKey("Model")]
        public int? ModelId { get; set; }
        public PhoneModel Model { get; set; }  // Navigation Property

        [ForeignKey("CaseManufacturer")]
        public int? CaseManufacturerId { get; set; }
        public CaseManufacturer CaseManufacturer { get; set; }  // Navigation Property

        [Required(ErrorMessage = "Case name is required.")]
        [StringLength(50, ErrorMessage = "Case name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = "Case name can only contain letters, numbers, spaces, and hyphens.")]
        public string CaseName { get; set; } // Unique constraint applied via Fluent API
    }
}
