using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using phoneCaseReworked.Repositories;
using phoneCaseReworked.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace phoneCaseReworked.Controllers {
    public class ProductController : Controller {
        private readonly IProductMetaRepository _repository;

        public ProductController(IProductMetaRepository repository) {
            _repository = repository;
        }

        public async Task<IActionResult> Index(int? id) {
            var viewModel = new ProductViewModel {
                Products = await _repository.GetAllProductAsync(),
                PhoneModels = await _repository.GetPhoneModelAsync(),
                CaseManufacturers = await _repository.GetCaseManufacturerAsync(),
                Product = id == null ? new Product() : await _repository.GetProductByIdAsync(id)
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProduct(Product product) {
            ModelState.Remove("product.Model");
            ModelState.Remove("product.CaseManufacturer");

            if (ModelState.IsValid) {
                if (product.ProductId == 0) {
                    await _repository.CreateProductAsync(product);
                } else {
                    await _repository.UpdateProductAsync(product);
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult EditProduct(int id)
        {
            return RedirectToAction("Index", new { id });
        }
    }

}
