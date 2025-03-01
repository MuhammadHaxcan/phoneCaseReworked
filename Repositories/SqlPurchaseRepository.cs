using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;

namespace phoneCaseReworked.Repositories {
    public class SqlPurchaseRepository : IPurchaseRepository {
        private readonly PhoneCaseDbContext _context;

        public SqlPurchaseRepository(PhoneCaseDbContext context) {
            _context = context;
        }

        public async Task<Purchase?> GetPurchaseByIdAsync(int? purchaseId) {
            return await _context.Purchases.FindAsync(purchaseId);
        }

        public async Task<List<Purchase>> GetPurchaseHistoryByVendorAsync(int? vendorId) {
            return await _context.Purchases
                .Where(p => p.VendorId == vendorId)
                .Include(p => p.Product)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task UpdatePurchaseAsync(Purchase purchase) {
            _context.Purchases.Update(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePurchaseAsync(Purchase purchase) {
            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task AddPurchaseAsync(Purchase purchase) {
            await _context.Purchases.AddAsync(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalPurchasesByVendorAsync(int vendorId, DateTime date) {
            return await _context.Purchases
                .Where(p => p.VendorId == vendorId && p.PurchaseDate <= date)
                .SumAsync(p => p.Quantity * p.UnitPrice);
        }

    }
}
