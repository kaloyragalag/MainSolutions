using MainSolutions.API.Models;

namespace MainSolutions.API.Repositories.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetByIdWithCategoryAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);
}
