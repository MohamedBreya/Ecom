using Ecom.core.Entities.Product;
using Ecom.core.Interfaces;
using Ecom.infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecom.infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public Task<bool> HasProductsAsync(int categoryId)
            => _context.Products.AnyAsync(p => p.CategoryId == categoryId);
    }
}
