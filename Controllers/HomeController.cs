using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;


namespace phoneCaseReworked.Controllers
{
    public class HomeController : Controller
    {
        private readonly PhoneCaseDbContext _context;

        public HomeController(PhoneCaseDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Stats(int? selectedMonth, int? selectedYear)
        {
            int currentMonth = selectedMonth ?? DateTime.UtcNow.Month;
            int currentYear = selectedYear ?? DateTime.UtcNow.Year;

            DateTime startOfMonth = new DateTime(currentYear, currentMonth, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var vendorPurchases = await _context.Purchases
                .Where(p => p.PurchaseDate >= startOfMonth && p.PurchaseDate <= endOfMonth)
                .GroupBy(p => p.VendorId)
                .Select(g => new VendorStatsViewModel
                {
                    VendorName = g.First().Vendor.Name,
                    TotalPurchases = g.Sum(p => p.Quantity * p.UnitPrice)
                })
                .ToListAsync();

            var vendorPayments = await _context.Payments
                .Where(p => p.PaymentDate >= startOfMonth && p.PaymentDate <= endOfMonth)
                .GroupBy(p => p.VendorId)
                .Select(g => new VendorStatsViewModel
                {
                    VendorName = g.First().Vendor.Name,
                    TotalPayments = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            var vendorCredits = await _context.Vendors
                .Select(v => new VendorCreditViewModel
                {
                    VendorName = v.Name,
                    RemainingCredit = v.TotalCredit
                })
                .ToListAsync();

            var vendorData = vendorPurchases
                .GroupJoin(vendorPayments, p => p.VendorName, py => py.VendorName, (p, py) => new { p, py })
                .SelectMany(temp => temp.py.DefaultIfEmpty(), (temp, py) => new VendorStatsViewModel
                {
                    VendorName = temp.p.VendorName,
                    TotalPurchases = temp.p.TotalPurchases,
                    TotalPayments = py?.TotalPayments ?? 0,
                })
                .ToList();

            foreach (var vendor in vendorCredits)
            {
                var matchedVendor = vendorData.FirstOrDefault(v => v.VendorName == vendor.VendorName);
                if (matchedVendor != null)
                {
                    matchedVendor.RemainingCredit = vendor.RemainingCredit;
                }
                else
                {
                    vendorData.Add(new VendorStatsViewModel
                    {
                        VendorName = vendor.VendorName,
                        TotalPurchases = 0,
                        TotalPayments = 0,
                        RemainingCredit = vendor.RemainingCredit
                    });
                }
            }

            decimal cumulativeTotalPurchases = vendorData.Sum(v => v.TotalPurchases);
            decimal cumulativeTotalPayments = vendorData.Sum(v => v.TotalPayments);
            decimal cumulativeRemainingCredit = vendorCredits.Sum(v => v.RemainingCredit);

            var viewModel = new StatsViewModel
            {
                VendorData = vendorData,
                CumulativeTotalPurchases = cumulativeTotalPurchases,
                CumulativeTotalPayments = cumulativeTotalPayments,
                CumulativeRemainingCredit = cumulativeRemainingCredit,
                SelectedMonth = currentMonth,
                SelectedYear = currentYear
            };

            return View(viewModel);
        }
    }

    public class VendorStatsViewModel
    {
        public string VendorName { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal RemainingCredit { get; set; }
    }

    public class StatsViewModel
    {
        public List<VendorStatsViewModel> VendorData { get; set; } = new();
        public decimal CumulativeTotalPurchases { get; set; }
        public decimal CumulativeTotalPayments { get; set; }
        public decimal CumulativeRemainingCredit { get; set; }
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
    }
    public class VendorCreditViewModel
    {
        public string VendorName { get; set; } = string.Empty;
        public decimal RemainingCredit { get; set; }
    }
}
