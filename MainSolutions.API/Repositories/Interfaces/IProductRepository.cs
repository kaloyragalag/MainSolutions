using MainSolutions.API.Models;

namespace MainSolutions.API.Repositories.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetByIdWithCategoryAsync(int id);
    Task<bool> CategoryExistsAsync(int categoryId);
}
