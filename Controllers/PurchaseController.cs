using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using phoneCaseReworked.Repositories;
using phoneCaseReworked.ViewModels;

namespace phoneCaseReworked.Controllers {
    public class PurchaseController : Controller {
        private readonly IVendorRepository _vendorRepository;
        private readonly IProductMetaRepository _productMetaRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IPaymentRepository _paymentRepository;

        public PurchaseController(
            IVendorRepository vendorRepository,IProductMetaRepository productMetaRepository,
            IPurchaseRepository purchaseRepository,IPaymentRepository paymentRepository) {
            _vendorRepository = vendorRepository;
            _productMetaRepository = productMetaRepository;
            _purchaseRepository = purchaseRepository;
            _paymentRepository = paymentRepository;
        }


        public async Task<IActionResult> RecordPurchase(int? purchaseId) {
            var vendors = await _vendorRepository.GetAllVendorsAsync();
            var products = await _productMetaRepository.GetAllProductAsync();

            var viewModel = new PurchaseViewModel {
                Vendors = vendors,
                Products = products,
                Purchase = purchaseId.HasValue
                    ? await _purchaseRepository.GetPurchaseByIdAsync(purchaseId) ?? new Purchase()
                    : new Purchase()
            };

            if (purchaseId.HasValue && viewModel.Purchase.PurchaseId > 0) {
                viewModel.SelectedVendorId = viewModel.Purchase.VendorId;
                viewModel.PurchaseDate = viewModel.Purchase.PurchaseDate;
            }

            return View(viewModel);
        }

        public async Task<IActionResult> SavePurchase(PurchaseViewModel viewModel) {
            ModelState.Remove("Purchase.Vendor");
            ModelState.Remove("Purchase.Product");

            if (!ModelState.IsValid || viewModel.SelectedVendorId == 0 || viewModel.Purchase == null) {
                viewModel.Vendors = await _vendorRepository.GetAllVendorsAsync();
                viewModel.Products = await _productMetaRepository.GetAllProductAsync();
                return View("RecordPurchase", viewModel);
            }

            var purchase = viewModel.Purchase;
            purchase.VendorId = viewModel.SelectedVendorId;
            purchase.PurchaseDate = viewModel.PurchaseDate;

            var vendor = await _vendorRepository.GetVendorByIdAsync(viewModel.SelectedVendorId);
            if (vendor == null) {
                return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
            }

            decimal totalAdjustment = 0;

            if (purchase.PurchaseId == 0) {
                decimal newAmount = purchase.Quantity * purchase.UnitPrice;
                totalAdjustment = newAmount;
                vendor.TotalCredit += totalAdjustment;
                await _purchaseRepository.AddPurchaseAsync(purchase);
            } else {
                var existingPurchase = await _purchaseRepository.GetPurchaseByIdAsync(purchase.PurchaseId);
                if (existingPurchase == null) {
                    return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
                }

                bool hasPayments = await _paymentRepository.HasPaymentsAfterDateAsync(existingPurchase.VendorId, existingPurchase.PurchaseDate);

                if (hasPayments) {
                    ModelState.AddModelError("", "This purchase cannot be edited because a payment has been recorded after this purchase date.");
                    var errorViewModel = new VendorPurchaseHistoryViewModel {
                        Vendors = await _vendorRepository.GetAllVendorsAsync(),
                        SelectedVendor = vendor,
                        SelectedVendorId = viewModel.SelectedVendorId,
                        PurchaseHistory = await _purchaseRepository.GetPurchaseHistoryByVendorAsync(viewModel.SelectedVendorId),
                    };
                    return View("ViewPurchaseHistory", errorViewModel);
                }

                decimal oldAmount = existingPurchase.Quantity * existingPurchase.UnitPrice;
                decimal newAmount = purchase.Quantity * purchase.UnitPrice;
                totalAdjustment = newAmount - oldAmount;

                vendor.TotalCredit += totalAdjustment;

                existingPurchase.ProductId = purchase.ProductId;
                existingPurchase.Quantity = purchase.Quantity;
                existingPurchase.UnitPrice = purchase.UnitPrice;
                existingPurchase.PurchaseDate = purchase.PurchaseDate;

                await _purchaseRepository.UpdatePurchaseAsync(existingPurchase);
            }
            await _vendorRepository.UpdateVendorAsync(vendor);

            return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
        }



        public async Task<IActionResult> ViewPurchaseHistory(int? vendorId) {
            var viewModel = new VendorPurchaseHistoryViewModel {
                Vendors = await _vendorRepository.GetAllVendorsAsync(),
                SelectedVendor = vendorId.HasValue
                    ? await _vendorRepository.GetVendorByIdAsync(vendorId.Value)
                    : null,
                SelectedVendorId = vendorId ?? 0,
                PurchaseHistory = vendorId.HasValue
                    ? await _purchaseRepository.GetPurchaseHistoryByVendorAsync(vendorId)
                    : new()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePurchase(int purchaseId) {
            var purchase = await _purchaseRepository.GetPurchaseByIdAsync(purchaseId);
            if (purchase == null) {
                return RedirectToAction("ViewPurchaseHistory");
            }

            bool hasPayments = await _paymentRepository.HasPaymentsAfterDateAsync(purchase.VendorId, purchase.PurchaseDate);

            if (hasPayments) {
                ModelState.AddModelError("", "This purchase cannot be deleted because a payment has been recorded after this purchase date.");

                var errorViewModel = new VendorPurchaseHistoryViewModel {
                    Vendors = await _vendorRepository.GetAllVendorsAsync(),
                    SelectedVendor = await _vendorRepository.GetVendorByIdAsync(purchase.VendorId),
                    SelectedVendorId = purchase.VendorId,
                    PurchaseHistory = await _purchaseRepository.GetPurchaseHistoryByVendorAsync(purchase.VendorId),
                };

                return View("ViewPurchaseHistory", errorViewModel);
            }

            var vendor = await _vendorRepository.GetVendorByIdAsync(purchase.VendorId);
            if (vendor != null) {
                decimal amountToDeduct = purchase.Quantity * purchase.UnitPrice;
                vendor.TotalCredit -= amountToDeduct;
                await _vendorRepository.UpdateVendorAsync(vendor);
            }
            await _purchaseRepository.DeletePurchaseAsync(purchase);            
            return RedirectToAction("ViewPurchaseHistory", new { vendorId = purchase.VendorId });
        }
    }
}
