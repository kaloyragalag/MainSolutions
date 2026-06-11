using MainSolutions.API.Data;
using MainSolutions.API.Models;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.Test.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProductRepository _repository;
    private Category _category = null!;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new ProductRepository(_context);
    }

    private async Task SeedCategoryAsync()
    {
        _category = new Category { Name = "Laptops", Description = "Portable computers", IsActive = true, CreatedAt = DateTime.UtcNow };
        await _context.Categories.AddAsync(_category);
        await _context.SaveChangesAsync();
    }

    #region GetAll

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await SeedCategoryAsync();
        await _context.Products.AddRangeAsync(
            CreateProduct("MacBook Pro", _category.Id),
            CreateProduct("Dell XPS", _category.Id),
            CreateProduct("Lenovo ThinkPad", _category.Id)
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Page = 1, PageSize = 2 });

        result.Items.Count().Should().Be(2);
        result.TotalCount.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_IncludesCategory()
    {
        await SeedCategoryAsync();
        await _context.Products.AddAsync(CreateProduct("MacBook Pro", _category.Id));
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery());

        result.Items.First().Category.Should().NotBeNull();
        result.Items.First().Category!.Name.Should().Be("Laptops");
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersByName()
    {
        await SeedCategoryAsync();
        await _context.Products.AddRangeAsync(
            CreateProduct("MacBook Pro", _category.Id),
            CreateProduct("Dell XPS", _category.Id)
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Search = "macbook" });

        result.Items.Count().Should().Be(1);
        result.Items.First().Name.Should().Be("MacBook Pro");
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersByDescription()
    {
        await SeedCategoryAsync();
        await _context.Products.AddRangeAsync(
            CreateProduct("MacBook Pro", _category.Id, "Apple silicon"),
            CreateProduct("Dell XPS", _category.Id, "Intel processor")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Search = "apple" });

        result.Items.Count().Should().Be(1);
        result.Items.First().Name.Should().Be("MacBook Pro");
    }

    [Fact]
    public async Task GetAllAsync_WithSortByPrice_SortsAscending()
    {
        await SeedCategoryAsync();
        await _context.Products.AddRangeAsync(
            CreateProduct("MacBook Pro", _category.Id, price: 2499.99m),
            CreateProduct("Dell XPS", _category.Id, price: 1999.99m),
            CreateProduct("Lenovo ThinkPad", _category.Id, price: 1599.99m)
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { SortBy = "price", SortOrder = "asc" });

        result.Items.First().Name.Should().Be("Lenovo ThinkPad");
    }

    [Fact]
    public async Task GetAllAsync_WithSortByPrice_SortsDescending()
    {
        await SeedCategoryAsync();
        await _context.Products.AddRangeAsync(
            CreateProduct("MacBook Pro", _category.Id, price: 2499.99m),
            CreateProduct("Dell XPS", _category.Id, price: 1999.99m)
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { SortBy = "price", SortOrder = "desc" });

        result.Items.First().Name.Should().Be("MacBook Pro");
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProduct()
    {
        await SeedCategoryAsync();
        var product = CreateProduct("MacBook Pro", _category.Id);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("MacBook Pro");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithCategoryAsync_ReturnsProductWithCategory()
    {
        await SeedCategoryAsync();
        var product = CreateProduct("MacBook Pro", _category.Id);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdWithCategoryAsync(product.Id);

        result.Should().NotBeNull();
        result!.Category.Should().NotBeNull();
        result.Category!.Name.Should().Be("Laptops");
    }

    [Fact]
    public async Task GetByIdWithCategoryAsync_NonExistentId_ReturnsNull()
    {
        var result = await _repository.GetByIdWithCategoryAsync(9999);
        result.Should().BeNull();
    }

    #endregion

    #region Create

    [Fact]
    public async Task CreateAsync_ValidProduct_PersistsToDatabase()
    {
        await SeedCategoryAsync();
        var product = CreateProduct("MacBook Pro", _category.Id);

        var created = await _repository.CreateAsync(product);

        created.Id.Should().BeGreaterThan(0);
        _context.Products.Should().ContainSingle(p => p.Name == "MacBook Pro");
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateAsync_ExistingProduct_SavesChanges()
    {
        await SeedCategoryAsync();
        var product = CreateProduct("MacBook Pro", _category.Id);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        product.Price = 1999.99m;
        await _repository.UpdateAsync(product);

        var updated = await _context.Products.FindAsync(product.Id);
        updated!.Price.Should().Be(1999.99m);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ExistingProduct_RemovesFromDatabase()
    {
        await SeedCategoryAsync();
        var product = CreateProduct("MacBook Pro", _category.Id);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(product.Id);

        var deleted = await _context.Products.FindAsync(product.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        var act = () => _repository.DeleteAsync(9999);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region CategoryExists

    [Fact]
    public async Task CategoryExistsAsync_ExistingId_ReturnsTrue()
    {
        await SeedCategoryAsync();

        var exists = await _repository.CategoryExistsAsync(_category.Id);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CategoryExistsAsync_NonExistentId_ReturnsFalse()
    {
        var exists = await _repository.CategoryExistsAsync(9999);
        exists.Should().BeFalse();
    }

    #endregion

    #region Helpers

    private static Product CreateProduct(string name, int categoryId, string description = "Test description", decimal price = 999.99m) => new()
    {
        Name = name,
        Description = description,
        Price = price,
        Stock = 10,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CategoryId = categoryId
    };

    #endregion

    public void Dispose() => _context.Dispose();
}
