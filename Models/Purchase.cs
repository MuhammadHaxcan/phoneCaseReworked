using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace phoneCaseReworked.Models {
    public class Purchase {
        [Key]
        public int PurchaseId { get; set; }

        //[Required(ErrorMessage = "Vendor ID is required.")]
        [ForeignKey("Vendor")]
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }  // Navigation Property

        //[Required(ErrorMessage = "Product ID is required.")]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }  // Navigation Property

        //[Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 100, ErrorMessage = "Quantity must be at least 1 and at most 100.")]
        public int Quantity { get; set; }

        //[Required(ErrorMessage = "Unit price is required.")]
        [Range(50, 20000, ErrorMessage = "Unit price must be between 50 and 20,000.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(10,2)")]
        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;  // Computed property

        //[Required(ErrorMessage = "Purchase date is required.")]
        [Column(TypeName = "date")]
        public DateTime PurchaseDate { get; set; }
    }
}
