using phoneCaseReworked.Models;

namespace phoneCaseReworked.ViewModels
{
    public class PurchaseViewModel
    {
        public int SelectedVendorId { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public System.Collections.Generic.List<Vendor> Vendors { get; set; } = new();
        public System.Collections.Generic.List<Product> Products { get; set; } = new();
        public Purchase? Purchase { get; set; }
    }

    public class VendorPurchaseHistoryViewModel
    {
        public System.Collections.Generic.List<Vendor> Vendors { get; set; } = new();
        public int? SelectedVendorId { get; set; }
        public Vendor? SelectedVendor { get; set; }
        public System.Collections.Generic.List<Purchase> PurchaseHistory { get; set; } = new();
    }
}
