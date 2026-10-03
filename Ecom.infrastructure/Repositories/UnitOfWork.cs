using Ecom.core.Interfaces;

namespace Ecom.infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository CategoryRepository { get; }
        public IPhotoRepository PhotoRepository { get; }
        public IProductRepository ProductRepository { get; }

        public UnitOfWork(
            ICategoryRepository categoryRepository,
            IPhotoRepository photoRepository,
            IProductRepository productRepository)
        {
            CategoryRepository = categoryRepository;
            PhotoRepository = photoRepository;
            ProductRepository = productRepository;
        }
    }
}
