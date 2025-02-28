using Microsoft.AspNetCore.Mvc;
using phoneCaseReworked.Models;
using phoneCaseReworked.Repositories;
using phoneCaseReworked.ViewModels;

namespace phoneCaseReworked.Controllers {
    public class VendorController : Controller {
        private readonly IVendorRepository _vendorRepository;

        public VendorController(IVendorRepository vendorRepository) {
            _vendorRepository = vendorRepository;
        }

        public async Task<IActionResult> Index() {
            var vendors = await _vendorRepository.GetAllVendorsAsync();
            return View(new VendorViewModel { Vendors = vendors, NewVendor = new Vendor() });
        }

        [HttpPost]
        public async Task<IActionResult> AddVendor(Vendor newVendor) {
            if (!ModelState.IsValid) {
                TempData["ErrorMessage"] = "Please correct the form errors.";
                return RedirectToAction("Index");
            }

            if (await _vendorRepository.VendorExistsAsync(newVendor.Name)) {
                TempData["ErrorMessage"] = "Vendor already exists in the database.";
                return RedirectToAction("Index");
            }

            newVendor.TotalCredit = 0.00m;
            await _vendorRepository.AddVendorAsync(newVendor);

            TempData["SuccessMessage"] = "Vendor added successfully!";
            return RedirectToAction("Index");
        }
    }
}
