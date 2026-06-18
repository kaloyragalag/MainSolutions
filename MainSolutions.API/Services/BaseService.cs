using MainSolutions.API.DTOs;
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

    public virtual Task<PagedResult<T>> GetAllAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        => _repository.GetAllAsync(query, cancellationToken);

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    public virtual Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
        => _repository.CreateAsync(entity, cancellationToken);

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        => _repository.UpdateAsync(entity, cancellationToken);

    public virtual Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);
}
