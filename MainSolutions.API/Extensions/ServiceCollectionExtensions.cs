using MainSolutions.API.Data;
using MainSolutions.API.Options;
using MainSolutions.API.Repositories;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services;
using MainSolutions.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.API.Extensions;

/// <summary>
/// Centralizes DI registrations so Program.cs stays a thin composition root.
/// Extracted from Program.cs as part of the folder-structure cleanup —
/// add new entity registrations here rather than in Program.cs directly.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IEntityPatcher, ReflectionEntityPatcher>();
        services.AddScoped<IEntityImageRepository, EntityImageRepository>();

        services.Configure<AzureStorageOptions>(
            configuration.GetSection(AzureStorageOptions.SectionName));
        services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

        return services;
    }
}
