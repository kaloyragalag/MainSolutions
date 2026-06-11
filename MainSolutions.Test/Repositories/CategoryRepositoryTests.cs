using MainSolutions.API.Data;
using MainSolutions.API.Models;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.Test.Repositories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new CategoryRepository(_context);
    }

    #region GetAll

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await _context.Categories.AddRangeAsync(
            CreateCategory("Laptops"),
            CreateCategory("Smartphones"),
            CreateCategory("Monitors")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Page = 1, PageSize = 2 });

        result.Items.Count().Should().Be(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersResults()
    {
        await _context.Categories.AddRangeAsync(
            CreateCategory("Laptops", "Portable computers"),
            CreateCategory("Smartphones", "Mobile devices"),
            CreateCategory("Monitors", "Display screens")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Search = "mobile" });

        result.Items.Count().Should().Be(1);
        result.Items.First().Name.Should().Be("Smartphones");
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_SearchesByName()
    {
        await _context.Categories.AddRangeAsync(
            CreateCategory("Laptops"),
            CreateCategory("Smartphones")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Search = "laptop" });

        result.Items.Count().Should().Be(1);
        result.Items.First().Name.Should().Be("Laptops");
    }

    [Fact]
    public async Task GetAllAsync_WithSortByName_SortsAscending()
    {
        await _context.Categories.AddRangeAsync(
            CreateCategory("Monitors"),
            CreateCategory("Laptops"),
            CreateCategory("Smartphones")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { SortBy = "name", SortOrder = "asc" });

        result.Items.First().Name.Should().Be("Laptops");
    }

    [Fact]
    public async Task GetAllAsync_WithSortByName_SortsDescending()
    {
        await _context.Categories.AddRangeAsync(
            CreateCategory("Monitors"),
            CreateCategory("Laptops"),
            CreateCategory("Smartphones")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { SortBy = "name", SortOrder = "desc" });

        result.Items.First().Name.Should().Be("Smartphones");
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCategory()
    {
        var category = CreateCategory("Laptops");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(category.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Laptops");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);
        result.Should().BeNull();
    }

    #endregion

    #region Create

    [Fact]
    public async Task CreateAsync_ValidCategory_PersistsToDatabase()
    {
        var category = CreateCategory("Laptops");

        var created = await _repository.CreateAsync(category);

        created.Id.Should().BeGreaterThan(0);
        _context.Categories.Should().ContainSingle(c => c.Name == "Laptops");
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateAsync_ExistingCategory_SavesChanges()
    {
        var category = CreateCategory("Laptops");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        category.Name = "Updated Laptops";
        await _repository.UpdateAsync(category);

        var updated = await _context.Categories.FindAsync(category.Id);
        updated!.Name.Should().Be("Updated Laptops");
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ExistingCategory_RemovesFromDatabase()
    {
        var category = CreateCategory("Laptops");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(category.Id);

        var deleted = await _context.Categories.FindAsync(category.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        var act = () => _repository.DeleteAsync(9999);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Exists

    [Fact]
    public async Task ExistsAsync_ByName_ExistingName_ReturnsTrue()
    {
        await _context.Categories.AddAsync(CreateCategory("Laptops"));
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsAsync("Laptops");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ByName_IsCaseInsensitive()
    {
        await _context.Categories.AddAsync(CreateCategory("Laptops"));
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsAsync("LAPTOPS");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ByName_NonExistentName_ReturnsFalse()
    {
        var exists = await _repository.ExistsAsync("NonExistent");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ByNameAndExcludeId_ReturnsFalse_WhenSameRecord()
    {
        var category = CreateCategory("Laptops");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsAsync("Laptops", category.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ByNameAndExcludeId_ReturnsTrue_WhenDifferentRecord()
    {
        var cat1 = CreateCategory("Laptops");
        var cat2 = CreateCategory("Monitors");
        await _context.Categories.AddRangeAsync(cat1, cat2);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsAsync("Laptops", cat2.Id);
        exists.Should().BeTrue();
    }

    #endregion

    #region Helpers

    private static Category CreateCategory(string name, string description = "Test description") => new()
    {
        Name = name,
        Description = description,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    #endregion

    public void Dispose() => _context.Dispose();
}
