using MainSolutions.API.Models;

namespace MainSolutions.API.Repositories.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<bool> CategoryExistsAsync(int categoryId);
}
