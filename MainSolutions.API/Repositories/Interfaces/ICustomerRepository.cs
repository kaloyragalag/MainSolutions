using MainSolutions.API.Models;

namespace MainSolutions.API.Repositories.Interfaces;

public interface ICustomerRepository : IBaseRepository<Customer>
{
    Task<Customer?> GetByUserIdAsync(int userId);
    Task<bool> UserExistsAsync(int userId);
}