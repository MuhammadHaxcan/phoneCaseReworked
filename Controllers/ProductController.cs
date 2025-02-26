using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using System.Linq;
using System.Threading.Tasks;

namespace phoneCaseReworked.Controllers {
    public class ProductController : Controller {
        private readonly PhoneCaseDbContext _context;

        public ProductController(PhoneCaseDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id) {
            var viewModel = new ProductViewModel {
                Products = await _context.Products.Include(p => p.Model).Include(p => p.CaseManufacturer).ToListAsync(),
                PhoneModels = await _context.PhoneModels.ToListAsync(),
                CaseManufacturers = await _context.CaseManufacturers.ToListAsync(),
                Product = id == null ? new Product() : await _context.Products.FindAsync(id)
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProduct(Product product) {
            ModelState.Remove("product.Model");
            ModelState.Remove("product.CaseManufacturer");

            if (ModelState.IsValid) {
                if (product.ProductId == 0) {
                    _context.Products.Add(product);
                } else {
                    _context.Products.Update(product);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditProduct(int id) {
            return RedirectToAction("Index", new { id });
        }
    }

    public class ProductViewModel {
        public List<Product> Products { get; set; }
        public List<PhoneModel> PhoneModels { get; set; }
        public List<CaseManufacturer> CaseManufacturers { get; set; }
        public Product Product { get; set; } = new Product();
    }
}
