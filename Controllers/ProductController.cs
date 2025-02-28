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
        public async Task<IActionResult> SaveProduct(Product product)
        {
            ModelState.Remove("product.Model");
            ModelState.Remove("product.CaseManufacturer");

            if (ModelState.IsValid)
            {
                Product? result = null;

                if (product.ProductId == 0)
                {
                    result = await _repository.CreateProductAsync(product);
                }
                else
                {
                    result = await _repository.UpdateProductAsync(product);
                }

                if (result == null)
                {
                    TempData["ErrorMessage"] = "A product with the same Case Name, Manufacturer, and Phone Model already exists.";
                    return RedirectToAction("Index");
                }

                TempData["SuccessMessage"] = "Product saved successfully!";
            }

            return RedirectToAction("Index");
        }


        public IActionResult EditProduct(int id)
        {
            return RedirectToAction("Index", new { id });
        }
    }

}
