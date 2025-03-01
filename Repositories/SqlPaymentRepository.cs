using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task AddPaymentAsync(Payment payment) {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Payment>> GetPaymentHistoryByVendorAsync(int vendorId) {
            return await _context.Payments
                .Where(p => p.VendorId == vendorId)
                .Include(p => p.Vendor)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsByVendorAsync(int vendorId, DateTime date) {
            return await _context.Payments
                .Where(p => p.VendorId == vendorId && p.PaymentDate <= date)
                .SumAsync(p => p.Amount);
        }
    }
}
