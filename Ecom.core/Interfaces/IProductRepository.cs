using Ecom.core.DTO;
using Ecom.core.Entities.Product;

namespace Ecom.core.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        /// <summary>Creates the product and stores its images. Images are removed again if the save fails.</summary>
        Task<Product> AddAsync(AddProductDTO addProductDTO);

        /// <exception cref="Exceptions.NotFoundException">The product does not exist.</exception>
        Task UpdateAsync(int id, UpdateProductDTO updateProductDTO);
    }
}
