using MainSolutions.API.Models.DTOs;

namespace MainSolutions.API.Services.Interfaces;

public interface IBaseService<T> where T : class
{
    Task<PagedResult<T>> GetAllAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
