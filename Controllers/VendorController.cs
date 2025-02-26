using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using System.Linq;
using System.Threading.Tasks;

namespace phoneCaseReworked.Controllers {
    public class VendorController : Controller {
        private readonly PhoneCaseDbContext _context;

        public VendorController(PhoneCaseDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var vendors = await _context.Vendors.ToListAsync();
            return View(new VendorViewModel { Vendors = vendors, NewVendor = new Vendor() });
        }

        [HttpPost]
        public async Task<IActionResult> AddVendor(Vendor newVendor) {
            if (!ModelState.IsValid) {
                var vendors = await _context.Vendors.ToListAsync();
                return View("Index", new VendorViewModel { Vendors = vendors, NewVendor = newVendor });
            }

            newVendor.TotalCredit = 0.00m;

            _context.Vendors.Add(newVendor);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVendor(int id) {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor != null) {
                _context.Vendors.Remove(vendor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }

    public class VendorViewModel {
        public List<Vendor> Vendors { get; set; }
        public Vendor NewVendor { get; set; }
    }
}
