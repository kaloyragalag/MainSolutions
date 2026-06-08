using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;

namespace MainSolutions.API.Services;

public abstract class BaseService<T> : IBaseService<T> where T : class
{
    protected readonly IBaseRepository<T> _repository;

    protected BaseService(IBaseRepository<T> repository)
    {
        _repository = repository;
    }

    public virtual Task<PagedResult<T>> GetAllAsync(PaginationQuery query)
        => _repository.GetAllAsync(query);

    public virtual Task<T?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public virtual Task<T> CreateAsync(T entity)
        => _repository.CreateAsync(entity);

    public virtual Task UpdateAsync(T entity)
        => _repository.UpdateAsync(entity);

    public virtual Task DeleteAsync(int id)
        => _repository.DeleteAsync(id);
}
