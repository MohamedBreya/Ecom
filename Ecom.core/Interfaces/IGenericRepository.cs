using Ecom.core.Entities;
using System.Linq.Expressions;

namespace Ecom.core.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity<int>
    {
        Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);

        /// <returns><c>true</c> when the entity existed and was deleted.</returns>
        Task<bool> DeleteAsync(int id);
    }
}
