using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;

namespace MainSolutions.API.Services;

public class ProductService : BaseService<Product>, IProductService
{
    public ProductService(IProductRepository repository) : base(repository)
    {
    }
}
