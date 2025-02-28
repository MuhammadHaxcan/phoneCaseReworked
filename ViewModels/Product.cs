using phoneCaseReworked.Models;

namespace phoneCaseReworked.ViewModels
{
    public class ProductViewModel
    {
        public List<Product> Products { get; set; }
        public List<PhoneModel> PhoneModels { get; set; }
        public List<CaseManufacturer> CaseManufacturers { get; set; }
        public Product Product { get; set; } = new Product();
    }
}
