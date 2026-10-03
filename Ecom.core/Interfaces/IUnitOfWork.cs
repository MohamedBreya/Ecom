namespace Ecom.core.Interfaces
{
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; }
        IPhotoRepository PhotoRepository { get; }
        IProductRepository ProductRepository { get; }
    }
}
