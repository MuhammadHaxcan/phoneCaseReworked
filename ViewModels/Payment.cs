using phoneCaseReworked.Models;

namespace phoneCaseReworked.ViewModels
{
    public class PaymentViewModel
    {
        public List<Vendor> Vendors { get; set; } = new();
        public Payment Payment { get; set; }
    }
    public class PaymentHistoryViewModel
    {
        public List<Vendor> Vendors { get; set; } = new();
        public int? SelectedVendorId { get; set; }
        public Vendor SelectedVendor { get; set; }
        public List<Payment> PaymentHistory { get; set; } = new();
    }
}
