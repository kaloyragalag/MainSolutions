using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;

namespace MainSolutions.API.Services;

public class CategoryService : BaseService<Category>, ICategoryService
{
    public CategoryService(ICategoryRepository repository) : base(repository)
    {
    }
}
