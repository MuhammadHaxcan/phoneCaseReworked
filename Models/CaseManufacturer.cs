using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace phoneCaseReworked.Models
{
    public class CaseManufacturer
    {
        [Key]
        public int CaseManufacturerId { get; set; }

        [Required(ErrorMessage = "Manufacturer name is required.")]
        [StringLength(30, ErrorMessage = "Manufacturer name cannot exceed 30 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = "Manufacturer name can only contain letters, numbers, spaces, and hyphens.")]
        public string Name { get; set; } // Unique constraint applied via Fluent API
    }
}
