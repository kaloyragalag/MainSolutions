using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;

namespace MainSolutions.API.Services;

public class CustomerService : BaseService<Customer>, ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository) : base(customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _customerRepository.GetByUserIdAsync(userId, cancellationToken);
    }
}
