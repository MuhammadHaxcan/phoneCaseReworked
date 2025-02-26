using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using System.Linq;
using System.Threading.Tasks;

namespace phoneCaseReworked.Controllers {
    public class ProductMetaController : Controller {
        private readonly PhoneCaseDbContext _context;

        public ProductMetaController(PhoneCaseDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var viewModel = new ProductMetaViewModel {
                CaseManufacturers = await _context.CaseManufacturers.ToListAsync(),
                PhoneModels = await _context.PhoneModels.ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddCaseManufacturer(string name) {
            if (!string.IsNullOrEmpty(name)) {
                var manufacturer = new CaseManufacturer { Name = name };
                _context.CaseManufacturers.Add(manufacturer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoneModel(string name) {
            if (!string.IsNullOrEmpty(name)) {
                var model = new PhoneModel { Name = name };
                _context.PhoneModels.Add(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }

    public class ProductMetaViewModel {
        public List<CaseManufacturer> CaseManufacturers { get; set; }
        public List<PhoneModel> PhoneModels { get; set; }
    }
}
