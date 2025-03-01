using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using phoneCaseReworked.Repositories;
using phoneCaseReworked.ViewModels;

namespace phoneCaseReworked.Controllers {
    public class PurchaseController : Controller {
        private readonly IVendorRepository _vendorRepository;
        private readonly PhoneCaseDbContext _context;

        public PurchaseController(IVendorRepository vendorRepository, PhoneCaseDbContext context) {
            _vendorRepository = vendorRepository;
            _context = context;
        }

        public async Task<IActionResult> RecordPurchase(int? purchaseId) {
            var vendors = await _vendorRepository.GetAllVendorsAsync();
            var products = await _context.Products
                .Include(p => p.Model)
                .Include(p => p.CaseManufacturer)
                .ToListAsync();

            var viewModel = new PurchaseViewModel {
                Vendors = vendors,
                Products = products,
                Purchase = purchaseId.HasValue
                    ? await _context.Purchases.FindAsync(purchaseId) ?? new Purchase()
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
                viewModel.Products = await _context.Products.ToListAsync();
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
                _context.Purchases.Add(purchase);
            } else {
                var existingPurchase = await _context.Purchases.FindAsync(purchase.PurchaseId);
                if (existingPurchase == null) {
                    return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
                }

                bool hasPayments = await _context.Payments
                    .AnyAsync(p => p.VendorId == existingPurchase.VendorId && p.PaymentDate >= existingPurchase.PurchaseDate);

                if (hasPayments) {
                    ModelState.AddModelError("", "This purchase cannot be edited because a payment has been recorded after this purchase date.");
                    var errorViewModel = new VendorPurchaseHistoryViewModel {
                        Vendors = await _vendorRepository.GetAllVendorsAsync(),
                        SelectedVendor = vendor,
                        SelectedVendorId = viewModel.SelectedVendorId,
                        PurchaseHistory = await _context.Purchases
                            .Where(p => p.VendorId == viewModel.SelectedVendorId)
                            .Include(p => p.Product)
                            .OrderByDescending(p => p.PurchaseDate)
                            .ToListAsync()
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

                _context.Purchases.Update(existingPurchase);
            }

            await _vendorRepository.UpdateVendorAsync(vendor);
            await _context.SaveChangesAsync();
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
                    ? await _context.Purchases
                        .Where(p => p.VendorId == vendorId)
                        .Include(p => p.Product)
                        .OrderByDescending(p => p.PurchaseDate)
                        .ToListAsync()
                    : new()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePurchase(int purchaseId) {
            var purchase = await _context.Purchases.FindAsync(purchaseId);
            if (purchase == null) {
                return RedirectToAction("ViewPurchaseHistory");
            }

            bool hasPayments = await _context.Payments
                .AnyAsync(p => p.VendorId == purchase.VendorId && p.PaymentDate >= purchase.PurchaseDate);

            if (hasPayments) {
                ModelState.AddModelError("", "This purchase cannot be deleted because a payment has been recorded after this purchase date.");

                var errorViewModel = new VendorPurchaseHistoryViewModel {
                    Vendors = await _vendorRepository.GetAllVendorsAsync(),
                    SelectedVendor = await _vendorRepository.GetVendorByIdAsync(purchase.VendorId),
                    SelectedVendorId = purchase.VendorId,
                    PurchaseHistory = await _context.Purchases
                        .Where(p => p.VendorId == purchase.VendorId)
                        .Include(p => p.Product)
                        .OrderByDescending(p => p.PurchaseDate)
                        .ToListAsync()
                };

                return View("ViewPurchaseHistory", errorViewModel);
            }

            var vendor = await _vendorRepository.GetVendorByIdAsync(purchase.VendorId);
            if (vendor != null) {
                decimal amountToDeduct = purchase.Quantity * purchase.UnitPrice;
                vendor.TotalCredit -= amountToDeduct;
                await _vendorRepository.UpdateVendorAsync(vendor);
            }

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewPurchaseHistory", new { vendorId = purchase.VendorId });
        }
    }
}
