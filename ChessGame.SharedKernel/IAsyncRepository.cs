using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessGame.SharedKernel
{
    /// <summary>
    /// Contrat générique de dépôt asynchrone.
    /// </summary>
    public interface IAsyncRepository<T> where T : class, IAggregateRoot
    {
        Task<T?> GetByIdAsync(object id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
