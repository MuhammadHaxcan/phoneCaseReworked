using Microsoft.AspNetCore.Mvc;
using phoneCaseReworked.Models;
using phoneCaseReworked.Repositories;
using phoneCaseReworked.ViewModels;

namespace phoneCaseReworked.Controllers {
    public class PaymentController : Controller {
        private readonly IVendorRepository _vendorRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPurchaseRepository _purchaseRepository;

        public PaymentController(IVendorRepository vendorRepository, IPaymentRepository paymentRepository, IPurchaseRepository purchaseRepository) {
            _vendorRepository = vendorRepository;
            _paymentRepository = paymentRepository;
            _purchaseRepository = purchaseRepository;
        }

        public async Task<IActionResult> MakePayment() {
            var viewModel = new PaymentViewModel {
                Vendors = await _vendorRepository.GetAllVendorsAsync(),
                Payment = new Payment { PaymentDate = DateTime.Now }
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetVendorCredit(int vendorId) {
            var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId);
            if (vendor != null) {
                return Json(new { success = true, totalCredit = vendor.TotalCredit });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> SavePayment(PaymentViewModel viewModel) {
            ModelState.Remove("Payment.Vendor");

            if (!ModelState.IsValid) {
                viewModel.Vendors = await _vendorRepository.GetAllVendorsAsync();
                return View("MakePayment", viewModel);
            }

            var vendor = await _vendorRepository.GetVendorByIdAsync(viewModel.Payment.VendorId);
            if (vendor == null) {
                ModelState.AddModelError("Payment.VendorId", "Vendor not found.");
                viewModel.Vendors = await _vendorRepository.GetAllVendorsAsync();
                return View("MakePayment", viewModel);
            }

            decimal remainingVendorCredit = vendor.TotalCredit;
            if (viewModel.Payment.Amount > remainingVendorCredit) {
                ModelState.AddModelError("Payment.Amount", $"Payment cannot exceed vendor's total credit of Rs. {remainingVendorCredit:N2}.");
                viewModel.Vendors = await _vendorRepository.GetAllVendorsAsync();
                return View("MakePayment", viewModel);
            }

            decimal totalPurchasesTillDate = await _purchaseRepository.GetTotalPurchasesByVendorAsync(viewModel.Payment.VendorId, viewModel.Payment.PaymentDate);
            decimal totalPaymentsTillDate = await _paymentRepository.GetTotalPaymentsByVendorAsync(viewModel.Payment.VendorId, viewModel.Payment.PaymentDate);
            decimal remainingAmountTillDate = totalPurchasesTillDate - totalPaymentsTillDate;

            if (viewModel.Payment.Amount > remainingAmountTillDate) {
                ModelState.AddModelError("Payment.Amount", $"Payment cannot exceed Rs. {remainingAmountTillDate:N2}, which is the remaining amount available till {viewModel.Payment.PaymentDate:MM/dd/yyyy}.");
                viewModel.Vendors = await _vendorRepository.GetAllVendorsAsync();
                return View("MakePayment", viewModel);
            }

            vendor.TotalCredit -= viewModel.Payment.Amount;
            await _paymentRepository.AddPaymentAsync(new Payment {
                VendorId = viewModel.Payment.VendorId,
                Amount = viewModel.Payment.Amount,
                PaymentDate = viewModel.Payment.PaymentDate
            });

            await _vendorRepository.UpdateVendorAsync(vendor);
            return RedirectToAction("MakePayment");
        }

        public async Task<IActionResult> ViewPaymentHistory(int? vendorId) {
            var vendors = await _vendorRepository.GetAllVendorsAsync();
            var paymentHistory = new List<Payment>();
            Vendor? selectedVendor = null;

            if (vendorId.HasValue && vendorId > 0) {
                selectedVendor = await _vendorRepository.GetVendorByIdAsync(vendorId.Value);
                paymentHistory = await _paymentRepository.GetPaymentHistoryByVendorAsync(vendorId.Value);
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
