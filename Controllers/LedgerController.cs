using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using phoneCaseReworked.ViewModels;

namespace phoneCaseReworked.Controllers {
    public class LedgerController : Controller {
        private readonly PhoneCaseDbContext _context;

        public LedgerController(PhoneCaseDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VendorLedger(int vendorId, string filter) {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorId == vendorId);
            if (vendor == null) {
                ModelState.AddModelError("", "Vendor not found.");
                ViewBag.Vendors = await _context.Vendors.ToListAsync();
                return View("Index");
            }

            var purchases = await _context.Purchases
                .Where(p => p.VendorId == vendorId)
                .Select(p => new LedgerTransactionViewModel {
                    Date = p.PurchaseDate.Date,
                    Description = "Purchase",
                    Debit = p.Quantity * p.UnitPrice,
                    Credit = 0,
                    TransactionType = "purchase",
                    PurchaseIds = new List<int> { p.PurchaseId }
                })
                .ToListAsync();

            var payments = await _context.Payments
                .Where(p => p.VendorId == vendorId)
                .Select(p => new LedgerTransactionViewModel {
                    Date = p.PaymentDate.Date,
                    Description = "Payment",
                    Debit = 0,
                    Credit = p.Amount,
                    TransactionType = "payment",
                    PurchaseIds = new List<int>()
                })
                .ToListAsync();

            var transactions = purchases.Concat(payments)
                .OrderBy(t => t.Date)
                .ToList();

            // Group transactions by date
            var groupedTransactions = transactions
                .GroupBy(t => t.Date)
                .Select(g => new LedgerTransactionViewModel {
                    Date = g.Key,
                    Description = string.Join("|", g.Select(t => t.Description).Distinct()),
                    Debit = g.Where(t => t.TransactionType == "purchase").Sum(t => t.Debit),
                    Credit = g.Where(t => t.TransactionType == "payment").Sum(t => t.Credit),
                    TransactionType = string.Join("|", g.Select(t => t.TransactionType).Distinct()),
                    PurchaseIds = g.Where(t => t.TransactionType == "purchase").SelectMany(t => t.PurchaseIds).ToList()
                })
                .OrderBy(t => t.Date)
                .ToList();

            // Calculate running balance
            decimal runningBalance = 0;
            foreach (var transaction in groupedTransactions) {
                runningBalance += transaction.Debit - transaction.Credit;
                transaction.RemainingBalance = runningBalance;
            }

            // Apply filter
            DateTime filterStartDate = filter switch {
                "week" => DateTime.Now.AddDays(-7),
                "month" => DateTime.Now.AddMonths(-1),
                _ => DateTime.MinValue
            };

            var filteredTransactions = groupedTransactions.Where(t => t.Date >= filterStartDate).ToList();

            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.SelectedVendor = vendorId;
            ViewBag.SelectedFilter = filter;

            return View(filteredTransactions);
        }

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

}
