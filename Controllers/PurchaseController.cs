using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using phoneCaseReworked.Models;
using phoneCaseReworked.Repositories;
using phoneCaseReworked.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace phoneCaseReworked.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IProductMetaRepository _productMetaRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IPaymentRepository _paymentRepository;

        public PurchaseController(
            IVendorRepository vendorRepository, IProductMetaRepository productMetaRepository,
            IPurchaseRepository purchaseRepository, IPaymentRepository paymentRepository)
        {
            _vendorRepository = vendorRepository;
            _productMetaRepository = productMetaRepository;
            _purchaseRepository = purchaseRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<IActionResult> RecordPurchase()
        {
            var vendors = await _vendorRepository.GetAllVendorsAsync();
            var products = await _productMetaRepository.GetAllProductAsync();

            var viewModel = new PurchaseViewModel
            {
                Vendors = vendors,
                Products = products,
                Purchase = new List<Purchase>() // Multiple purchases under one vendor
            };

            return View(viewModel);
        }

        public async Task<IActionResult> EditPurchase(int purchaseId)
        {
            var purchase = await _purchaseRepository.GetPurchaseByIdAsync(purchaseId);
            if (purchase == null)
            {
                return RedirectToAction("ViewPurchaseHistory");
            }

            var vendors = await _vendorRepository.GetAllVendorsAsync();
            var products = await _productMetaRepository.GetAllProductAsync();

            var viewModel = new PurchaseViewModel
            {
                SelectedVendorId = purchase.VendorId,
                PurchaseDate = purchase.PurchaseDate,
                Vendors = vendors,
                Products = products,
                Purchase = new List<Purchase> { purchase }
            };

            return View("EditPurchase", viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> SavePurchase(PurchaseViewModel viewModel)
        {
            foreach (var purchase in viewModel.Purchase)
            {
                ModelState.Remove($"Purchase[{viewModel.Purchase.IndexOf(purchase)}].Vendor");
                ModelState.Remove($"Purchase[{viewModel.Purchase.IndexOf(purchase)}].Product");
            }

            if (!ModelState.IsValid || viewModel.SelectedVendorId == 0 || viewModel.Purchase == null || !viewModel.Purchase.Any())
            {
                viewModel.Vendors = await _vendorRepository.GetAllVendorsAsync();
                viewModel.Products = await _productMetaRepository.GetAllProductAsync();
                return View("RecordPurchase", viewModel);
            }

            var vendor = await _vendorRepository.GetVendorByIdAsync(viewModel.SelectedVendorId);
            if (vendor == null)
            {
                return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
            }

            decimal totalAdjustment = 0;
            foreach (var purchase in viewModel.Purchase)
            {
                purchase.VendorId = viewModel.SelectedVendorId;
                purchase.PurchaseDate = viewModel.PurchaseDate;
                decimal newAmount = purchase.Quantity * purchase.UnitPrice;
                totalAdjustment += newAmount;
                await _purchaseRepository.AddPurchaseAsync(purchase);
            }

            vendor.TotalCredit += totalAdjustment;
            await _vendorRepository.UpdateVendorAsync(vendor);

            return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
        }
        public async Task<IActionResult> SaveEditPurchase(PurchaseViewModel viewModel)
        {

            ModelState.Remove($"Purchase[{0}].Vendor");
            ModelState.Remove($"Purchase[{0}].Product");
            
            if (!ModelState.IsValid || viewModel.SelectedVendorId == 0 || viewModel.Purchase == null)
            {
                viewModel.Vendors = await _vendorRepository.GetAllVendorsAsync();
                viewModel.Products = await _productMetaRepository.GetAllProductAsync();
                return View("EditPurchase", viewModel);
            }

            var purchase = viewModel.Purchase.First();
            purchase.VendorId = viewModel.SelectedVendorId;
            purchase.PurchaseDate = viewModel.PurchaseDate;

            var vendor = await _vendorRepository.GetVendorByIdAsync(viewModel.SelectedVendorId);
            if (vendor == null)
            {
                return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
            }

            var existingPurchase = await _purchaseRepository.GetPurchaseByIdAsync(purchase.PurchaseId);
            if (existingPurchase == null)
            {
                return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
            }

            bool hasPayments = await _paymentRepository.HasPaymentsAfterDateAsync(existingPurchase.VendorId, existingPurchase.PurchaseDate);
            if (hasPayments)
            {
                TempData["ErrorMessage"] = "This purchase cannot be edited because a payment has been recorded after this purchase date.";
                return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
            }

            decimal oldAmount = existingPurchase.Quantity * existingPurchase.UnitPrice;
            decimal newAmount = purchase.Quantity * purchase.UnitPrice;
            decimal totalAdjustment = newAmount - oldAmount;

            vendor.TotalCredit += totalAdjustment;

            existingPurchase.ProductId = purchase.ProductId;
            existingPurchase.Quantity = purchase.Quantity;
            existingPurchase.UnitPrice = purchase.UnitPrice;
            existingPurchase.PurchaseDate = purchase.PurchaseDate;

            await _purchaseRepository.UpdatePurchaseAsync(existingPurchase);
            await _vendorRepository.UpdateVendorAsync(vendor);

            return RedirectToAction("ViewPurchaseHistory", new { vendorId = viewModel.SelectedVendorId });
        }

        public async Task<IActionResult> ViewPurchaseHistory(int? vendorId)
        {
            var viewModel = new VendorPurchaseHistoryViewModel
            {
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
        public async Task<IActionResult> DeletePurchase(int purchaseId)
        {
            var purchase = await _purchaseRepository.GetPurchaseByIdAsync(purchaseId);
            if (purchase == null)
            {
                return RedirectToAction("ViewPurchaseHistory");
            }

            bool hasPayments = await _paymentRepository.HasPaymentsAfterDateAsync(purchase.VendorId, purchase.PurchaseDate);
            if (hasPayments)
            {
                TempData["ErrorMessage"] = "This purchase cannot be deleted because a payment has been recorded after this purchase date.";
                return RedirectToAction("ViewPurchaseHistory", new { vendorId = purchase.VendorId });
            }

            var vendor = await _vendorRepository.GetVendorByIdAsync(purchase.VendorId);
            if (vendor != null)
            {
                decimal amountToDeduct = purchase.Quantity * purchase.UnitPrice;
                vendor.TotalCredit -= amountToDeduct;
                await _vendorRepository.UpdateVendorAsync(vendor);
            }
            await _purchaseRepository.DeletePurchaseAsync(purchase);
            return RedirectToAction("ViewPurchaseHistory", new { vendorId = purchase.VendorId });
        }
    }
}
