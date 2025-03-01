
namespace phoneCaseReworked.Repositories {
    public interface IPaymentRepository {
        Task<bool> HasPaymentsAfterDateAsync(int vendorId, DateTime purchaseDate);
    }
}
