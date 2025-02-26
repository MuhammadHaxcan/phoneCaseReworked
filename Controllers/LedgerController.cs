using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;

namespace phoneCaseReworked.Controllers {
    public class LedgerController : Controller {
        private readonly PhoneCaseDbContext _context;

        public LedgerController(PhoneCaseDbContext context) {
            _context = context;
        }

        // ✅ Show Vendors for Ledger View
        public async Task<IActionResult> Index() {
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            return View();
        }

        // ✅ Fetch Transactions for Selected Vendor
        [HttpPost]
        public async Task<IActionResult> VendorLedger(int vendorId, string filter) {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorId == vendorId);
            if (vendor == null) {
                ModelState.AddModelError("", "Vendor not found.");
                ViewBag.Vendors = await _context.Vendors.ToListAsync();
                return View("Index");
            }

            DateTime startDate = filter switch {
                "week" => DateTime.Now.AddDays(-7),
                "month" => DateTime.Now.AddMonths(-1),
                _ => DateTime.MinValue
            };

            // ✅ Fetch Purchases
            var purchases = await _context.Purchases
                .Where(p => p.VendorId == vendorId && p.PurchaseDate >= startDate)
                .Select(p => new LedgerTransactionViewModel {
                    Date = p.PurchaseDate.Date,
                    Description = "Purchase",
                    Debit = p.Quantity * p.UnitPrice,
                    Credit = 0,
                    TransactionType = "purchase",
                    PurchaseIds = new List<int> { p.PurchaseId }
                })
                .ToListAsync();

            // ✅ Fetch Payments
            var payments = await _context.Payments
                .Where(p => p.VendorId == vendorId && p.PaymentDate >= startDate)
                .Select(p => new LedgerTransactionViewModel {
                    Date = p.PaymentDate.Date,
                    Description = "Payment",
                    Debit = 0,
                    Credit = p.Amount,
                    TransactionType = "payment",
                    PurchaseIds = new List<int>()
                })
                .ToListAsync();

            // ✅ Combine Transactions and Order them Correctly
            var transactions = purchases.Concat(payments)
                .OrderBy(t => t.Date) // Ensure chronological order
                .ToList();

            // ✅ Fix Running Balance Calculation
            decimal runningBalance = 0; // Correct starting balance

            foreach (var transaction in transactions) {
                if (transaction.TransactionType == "purchase") {
                    runningBalance += transaction.Debit;
                } else if (transaction.TransactionType == "payment") {
                    runningBalance -= transaction.Credit;
                }
                transaction.RemainingBalance = runningBalance;
            }

            // ✅ Group Transactions by Date & Summarize
            var groupedTransactions = transactions
                .GroupBy(t => t.Date)
                .Select(g => new LedgerTransactionViewModel {
                    Date = g.Key,
                    Description = string.Join(" | ", g.Select(t => t.Description).Distinct()), // Remove duplicates
                    Debit = g.Sum(t => t.Debit),
                    Credit = g.Sum(t => t.Credit),
                    RemainingBalance = g.Last().RemainingBalance, // Ensure correct balance display
                    TransactionType = g.Any(t => t.TransactionType == "purchase") ? "purchase" : "payment",
                    PurchaseIds = g.SelectMany(t => t.PurchaseIds).Distinct().ToList() // Remove duplicate purchase IDs
                })
                .OrderBy(t => t.Date)
                .ToList();

            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.SelectedVendor = vendorId;
            ViewBag.SelectedFilter = filter;

            return View(groupedTransactions);
        }

        // ✅ Show Details for Purchases on a Specific Date
        public async Task<IActionResult> PurchaseDetails(string purchaseIds) {
            if (string.IsNullOrEmpty(purchaseIds)) {
                return NotFound("No purchase IDs provided.");
            }

            var purchaseIdList = purchaseIds.Split(',')
                                            .Select(id => int.TryParse(id, out int parsedId) ? parsedId : (int?)null)
                                            .Where(id => id.HasValue)
                                            .Select(id => id.Value)
                                            .ToList();

            if (!purchaseIdList.Any()) {
                return NotFound("Invalid purchase IDs.");
            }

            var purchases = await _context.Purchases
                .Where(p => purchaseIdList.Contains(p.PurchaseId))
                .Include(p => p.Product)
                .ThenInclude(m => m.Model)
                .Include(p => p.Product)
                .ThenInclude(cm => cm.CaseManufacturer)
                .ToListAsync();

            if (!purchases.Any()) {
                return NotFound("No purchases found for this date.");
            }

            return View(purchases);
        }
    }

    // ✅ Ledger Transaction ViewModel
    public class LedgerTransactionViewModel {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RemainingBalance { get; set; }
        public string TransactionType { get; set; }
        public List<int> PurchaseIds { get; set; } = new List<int>();
    }
}
