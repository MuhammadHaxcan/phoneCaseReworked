using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;

namespace phoneCaseReworked.Repositories
{
    public class SqlProductMetaRepository : IProductMetaRepository
    {
        private readonly PhoneCaseDbContext _context;
        public SqlProductMetaRepository(PhoneCaseDbContext context)
        {
            _context = context;
        }

        // CREATE!
        public async Task<CaseManufacturer?> CreateCaseManufacturerAsync(CaseManufacturer caseManufacturer)
        {
            var existingManufacturer = await _context.CaseManufacturers
                .AnyAsync(m => m.Name == caseManufacturer.Name);

            if (existingManufacturer)
            {
                return null;
            }

            await _context.CaseManufacturers.AddAsync(caseManufacturer);
            await _context.SaveChangesAsync();
            return caseManufacturer;
        }

        public async Task<PhoneModel?> CreatePhoneModelAsync(PhoneModel phoneModel)
        {
            var existingModel = await _context.PhoneModels
                .AnyAsync(m => m.Name == phoneModel.Name);

            if (existingModel)
            {
                return null;
            }

            await _context.PhoneModels.AddAsync(phoneModel);
            await _context.SaveChangesAsync();
            return phoneModel;
        }


        public async Task<Product?> CreateProductAsync(Product product)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p =>
                p.CaseName == product.CaseName &&
                p.CaseManufacturerId == product.CaseManufacturerId &&
                p.ModelId == product.ModelId);

            if (existingProduct != null)
            {
                return null; // Duplicate product found
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        // READ [ GET ]
        public async Task<List<Product>> GetAllProductAsync()
        {
            return await _context.Products.Include(p => p.Model).Include(p => p.CaseManufacturer).ToListAsync();
        }

        public async Task<List<CaseManufacturer>> GetCaseManufacturerAsync()
        {
            return await _context.CaseManufacturers.ToListAsync();
        }

        public async Task<List<PhoneModel>> GetPhoneModelAsync()
        {
            return await _context.PhoneModels.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int? id)
        {
            return await _context.Products.FindAsync(id);
        }

        // UPDATE!
        public async Task<Product?> UpdateProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var existingProduct = await _context.Products.FirstOrDefaultAsync(p =>
                p.CaseName == product.CaseName &&
                p.CaseManufacturerId == product.CaseManufacturerId &&
                p.ModelId == product.ModelId &&
                p.ProductId != product.ProductId); 

            if (existingProduct != null)
            {
                return null; 
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }
    }
}
