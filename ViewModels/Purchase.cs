using phoneCaseReworked.Models;

namespace phoneCaseReworked.ViewModels
{
    public class PurchaseViewModel
    {
        public int SelectedVendorId { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public List<Vendor> Vendors { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public List<Purchase> Purchase { get; set; } = new List<Purchase>();
    }

    public class VendorPurchaseHistoryViewModel
    {
        public List<Vendor> Vendors { get; set; } = new();
        public int? SelectedVendorId { get; set; }
        public Vendor? SelectedVendor { get; set; }
        public List<Purchase> PurchaseHistory { get; set; } = new();
    }
}
