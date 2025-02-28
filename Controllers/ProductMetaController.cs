using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using phoneCaseReworked.Repositories;
using phoneCaseReworked.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace phoneCaseReworked.Controllers
{
    public class ProductMetaController : Controller
    {
        private readonly IProductMetaRepository _repository;

        public ProductMetaController(IProductMetaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new ProductMetaViewModel
            {
                CaseManufacturers = await _repository.GetCaseManufacturerAsync(),
                PhoneModels = await _repository.GetPhoneModelAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddCaseManufacturer(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var manufacturer = new CaseManufacturer { Name = name };
                var result = await _repository.CreateCaseManufacturerAsync(manufacturer);

                if (result == null)
                {
                    TempData["ErrorMessage"] = "This manufacturer already exists!";
                }
                else
                {
                    TempData["SuccessMessage"] = "Manufacturer added successfully!";
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoneModel(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var model = new PhoneModel { Name = name };
                var result = await _repository.CreatePhoneModelAsync(model);

                if (result ==  null)
                {
                    TempData["ErrorMessage"] = "This phone model already exists!";
                }
                else
                {
                    TempData["SuccessMessage"] = "Phone model added successfully!";
                }
            }
            return RedirectToAction("Index");
        }
    }
}
