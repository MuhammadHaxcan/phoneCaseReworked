using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace phoneCaseReworked.Repositories {
    public class SqlVendorRepository : IVendorRepository {
        private readonly PhoneCaseDbContext _context;

        public SqlVendorRepository(PhoneCaseDbContext context) {
            _context = context;
        }

        public async Task<List<Vendor>> GetAllVendorsAsync() {
            return await _context.Vendors.ToListAsync();
        }

        public async Task<Vendor?> GetVendorByIdAsync(int VendorId) {
            return await _context.Vendors.FirstOrDefaultAsync(v => v.VendorId == VendorId);
        }

        public async Task<bool> VendorExistsAsync(string name) {
            return await _context.Vendors.AnyAsync(v => v.Name == name);
        }

        public async Task AddVendorAsync(Vendor vendor) {
            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVendorAsync(Vendor vendor) {
            _context.Vendors.Update(vendor);
            await _context.SaveChangesAsync();
        }
    }
}
