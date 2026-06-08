using MainSolutions.API.Models.DTOs;

namespace MainSolutions.API.Repositories.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task<PagedResult<T>> GetAllAsync(PaginationQuery query);
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
