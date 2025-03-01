using phoneCaseReworked.Models;

namespace phoneCaseReworked.Repositories {
    public interface IPaymentRepository {
        Task<bool> HasPaymentsAfterDateAsync(int vendorId, DateTime purchaseDate);
        Task AddPaymentAsync(Payment payment);
        Task<List<Payment>> GetPaymentHistoryByVendorAsync(int vendorId);
        Task<decimal> GetTotalPaymentsByVendorAsync(int vendorId, DateTime date);
    }
}
