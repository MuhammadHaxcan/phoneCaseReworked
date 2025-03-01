using System.Collections.Generic;
using System.Threading.Tasks;
using phoneCaseReworked.Models;

namespace phoneCaseReworked.Repositories {
    public interface IPurchaseRepository {
        Task<Purchase?> GetPurchaseByIdAsync(int? purchaseId);
        Task<List<Purchase>> GetPurchaseHistoryByVendorAsync(int? vendorId);
        Task UpdatePurchaseAsync(Purchase purchase);
        Task DeletePurchaseAsync(Purchase purchase);
        Task AddPurchaseAsync(Purchase purchase);
        Task<decimal> GetTotalPurchasesByVendorAsync(int vendorId, DateTime date);
    }
}
