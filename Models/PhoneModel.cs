using System.ComponentModel.DataAnnotations;

namespace phoneCaseReworked.Models {
    public class PhoneModel {
        [Key]
        public int ModelId { get; set; }

        [Required(ErrorMessage = "Model name is required.")]
        [StringLength(50, ErrorMessage = "Model name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = "Model name can only contain letters, numbers, spaces, and hyphens.")]
        public string Name { get; set; }
    }
}
