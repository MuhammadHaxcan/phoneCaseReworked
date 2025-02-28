using phoneCaseReworked.Models;

namespace phoneCaseReworked.Repositories {
    public interface IVendorRepository {
        Task<List<Vendor>> GetAllVendorsAsync();
        Task<Vendor?> GetVendorByIdAsync(int VendorId);
        Task AddVendorAsync(Vendor vendor);
        Task<bool> VendorExistsAsync(string name);
    }
}
