using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using phoneCaseReworked.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace phoneCaseReworked.Controllers {
    public class PaymentController : Controller {
        private readonly PhoneCaseDbContext _context;

        public PaymentController(PhoneCaseDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> MakePayment() {
            var viewModel = new PaymentViewModel {
                Vendors = await _context.Vendors.ToListAsync(),
                Payment = new Payment { PaymentDate = DateTime.UtcNow }
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetVendorCredit(int vendorId) {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor != null) {
                return Json(new { success = true, totalCredit = vendor.TotalCredit });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> SavePayment(PaymentViewModel viewModel) {
            ModelState.Remove("Payment.Vendor");

            if (!ModelState.IsValid) {
                viewModel.Vendors = await _context.Vendors.ToListAsync();
                return View("MakePayment", viewModel);
            }

            var vendor = await _context.Vendors.FindAsync(viewModel.Payment.VendorId);
            if (vendor == null) {
                ModelState.AddModelError("Payment.VendorId", "Vendor not found.");
                viewModel.Vendors = await _context.Vendors.ToListAsync();
                return View("MakePayment", viewModel);
            }

            decimal remainingVendorCredit = vendor.TotalCredit;
            if (viewModel.Payment.Amount > remainingVendorCredit) {
                ModelState.AddModelError("Payment.Amount", $"Payment cannot exceed vendor's total credit of Rs. {remainingVendorCredit:N2}.");
                viewModel.Vendors = await _context.Vendors.ToListAsync();
                return View("MakePayment", viewModel);
            }

            decimal totalPurchasesTillDate = await _context.Purchases
                .Where(p => p.VendorId == viewModel.Payment.VendorId && p.PurchaseDate <= viewModel.Payment.PaymentDate)
                .SumAsync(p => p.Quantity * p.UnitPrice);

            decimal totalPaymentsTillDate = await _context.Payments
                .Where(p => p.VendorId == viewModel.Payment.VendorId && p.PaymentDate <= viewModel.Payment.PaymentDate)
                .SumAsync(p => p.Amount);

            decimal remainingAmountTillDate = totalPurchasesTillDate - totalPaymentsTillDate;

            if (viewModel.Payment.Amount > remainingAmountTillDate) {
                ModelState.AddModelError("Payment.Amount", $"Payment cannot exceed Rs. {remainingAmountTillDate:N2}, which is the remaining amount available till {viewModel.Payment.PaymentDate:MM/dd/yyyy}.");
                viewModel.Vendors = await _context.Vendors.ToListAsync();
                return View("MakePayment", viewModel);
            }

            vendor.TotalCredit -= viewModel.Payment.Amount;

         
            _context.Payments.Add(new Payment {
                VendorId = viewModel.Payment.VendorId,
                Amount = viewModel.Payment.Amount,
                PaymentDate = viewModel.Payment.PaymentDate
            });

            _context.Vendors.Update(vendor);
            await _context.SaveChangesAsync();

            return RedirectToAction("MakePayment");
        }

        public async Task<IActionResult> ViewPaymentHistory(int? vendorId) {
            var vendors = await _context.Vendors.ToListAsync();
            var paymentHistory = new List<Payment>();
            Vendor selectedVendor = null;

            if (vendorId.HasValue && vendorId > 0) {
                selectedVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorId == vendorId);
                paymentHistory = await _context.Payments
                    .Where(p => p.VendorId == vendorId)
                    .Include(p => p.Vendor)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();
            }

            var viewModel = new PaymentHistoryViewModel {
                Vendors = vendors,
                SelectedVendor = selectedVendor,
                SelectedVendorId = vendorId ?? 0,
                PaymentHistory = paymentHistory
            };

            return View(viewModel);
        }
    }

}
