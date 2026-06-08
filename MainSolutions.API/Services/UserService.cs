using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;

namespace MainSolutions.API.Services;

public class UserService : BaseService<User>, IUserService
{
    public UserService(IUserRepository repository) : base(repository)
    {
    }
}
