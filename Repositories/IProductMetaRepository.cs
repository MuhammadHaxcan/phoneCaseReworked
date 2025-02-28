using phoneCaseReworked.Models;

namespace phoneCaseReworked.Repositories
{
    public interface IProductMetaRepository
    {
        // CaseManufacturer!!
        Task<CaseManufacturer> CreateCaseManufacturerAsync(CaseManufacturer caseManufacturer);
        Task<List<CaseManufacturer>> GetCaseManufacturerAsync();

        // PhoneModel!!
        Task<PhoneModel> CreatePhoneModelAsync(PhoneModel phoneModel);
        Task<List<PhoneModel>> GetPhoneModelAsync();

        // Product
        Task<List<Product>> GetAllProductAsync();
        Task<Product?> GetProductByIdAsync(int? id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(Product product);
    }
}
