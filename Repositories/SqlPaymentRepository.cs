using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;

namespace phoneCaseReworked.Repositories {
    public class SqlPaymentRepository : IPaymentRepository {
        private readonly PhoneCaseDbContext _context;

        public SqlPaymentRepository(PhoneCaseDbContext context) {
            _context = context;
        }

        public async Task<bool> HasPaymentsAfterDateAsync(int vendorId, DateTime purchaseDate) {
            return await _context.Payments
                .AnyAsync(p => p.VendorId == vendorId && p.PaymentDate >= purchaseDate);
        }
    }
}
