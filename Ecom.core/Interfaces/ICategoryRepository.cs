using Ecom.core.Entities.Product;

namespace Ecom.core.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<bool> HasProductsAsync(int categoryId);
    }
}
