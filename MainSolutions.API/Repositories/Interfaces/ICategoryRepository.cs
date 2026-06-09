using MainSolutions.API.Models;

namespace MainSolutions.API.Repositories.Interfaces;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<bool> ExistsAsync(string name);
    Task<bool> ExistsAsync(string name, int excludeId);
}
