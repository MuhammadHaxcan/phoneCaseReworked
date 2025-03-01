using phoneCaseReworked.Models;

namespace phoneCaseReworked.Repositories {
    public interface IVendorRepository {
        Task<List<Vendor>> GetAllVendorsAsync();
        Task<Vendor?> GetVendorByIdAsync(int VendorId);
        Task<bool> VendorExistsAsync(string name);
        Task AddVendorAsync(Vendor vendor);
        Task UpdateVendorAsync(Vendor vendor);
    }
}
