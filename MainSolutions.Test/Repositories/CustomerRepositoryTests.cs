using FluentAssertions;
using MainSolutions.API.Data;
using MainSolutions.API.Models;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MainSolutions.Test.Repositories;

public class CustomerRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new CustomerRepository(_context);
    }

    #region GetByUserId

    [Fact]
    public async Task GetByUserIdAsync_ReturnsCustomer_WhenUserIdMatches()
    {
        var customer = CreateCustomer(userId: 1, firstName: "Juan", lastName: "Dela Cruz");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(1);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Juan");
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsNull_WhenNoMatch()
    {
        var result = await _repository.GetByUserIdAsync(999);
        result.Should().BeNull();
    }

    #endregion

    #region UserExists

    [Fact]
    public async Task UserExistsAsync_ReturnsTrue_WhenUserExists()
    {
        await _context.Users.AddAsync(new User { Id = 1, Email = "juan@example.com" });
        await _context.SaveChangesAsync();

        var exists = await _repository.UserExistsAsync(1);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UserExistsAsync_ReturnsFalse_WhenUserDoesNotExist()
    {
        var exists = await _repository.UserExistsAsync(999);
        exists.Should().BeFalse();
    }

    #endregion

    #region GetAll

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await _context.Customers.AddRangeAsync(
            CreateCustomer(1, "Juan", "Dela Cruz"),
            CreateCustomer(2, "Maria", "Santos"),
            CreateCustomer(3, "Pedro", "Reyes")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Page = 1, PageSize = 2 });

        result.Items.Count().Should().Be(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Theory]
    [InlineData("juan")]   // FirstName
    [InlineData("CRUZ")]   // LastName, case-insensitive
    [InlineData("cebu")]   // City
    [InlineData("6000")]   // PostalCode
    public async Task GetAllAsync_WithSearch_FiltersAcrossMultipleFields(string search)
    {
        await _context.Customers.AddRangeAsync(
            CreateCustomer(1, "Juan", "Dela Cruz", city: "Cebu City", country: "Philippines",
                province: "Cebu", postalCode: "6000", phone: "09171234567"),
            CreateCustomer(2, "Maria", "Santos", city: "Manila", country: "Philippines",
                province: "Metro Manila", postalCode: "1000", phone: "09181234567")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Search = search });

        result.Items.Count().Should().Be(1);
        result.Items.First().UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_NoMatches_ReturnsEmpty()
    {
        await _context.Customers.AddRangeAsync(
            CreateCustomer(1, "Juan", "Dela Cruz"),
            CreateCustomer(2, "Maria", "Santos")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Search = "nonexistent" });

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_NoSearch_ReturnsAllCustomers()
    {
        await _context.Customers.AddRangeAsync(
            CreateCustomer(1, "Juan", "Dela Cruz"),
            CreateCustomer(2, "Maria", "Santos")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery());

        result.Items.Count().Should().Be(2);
        result.TotalCount.Should().Be(2);
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCustomer()
    {
        var customer = CreateCustomer(1, "Juan", "Dela Cruz");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(customer.Id);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Juan");
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
    public async Task CreateAsync_ValidCustomer_PersistsToDatabase()
    {
        var customer = CreateCustomer(1, "Juan", "Dela Cruz");

        var created = await _repository.CreateAsync(customer);

        created.Id.Should().BeGreaterThan(0);
        _context.Customers.Should().ContainSingle(c => c.FirstName == "Juan" && c.LastName == "Dela Cruz");
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateAsync_ExistingCustomer_SavesChanges()
    {
        var customer = CreateCustomer(1, "Juan", "Dela Cruz");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        customer.LastName = "Updated";
        await _repository.UpdateAsync(customer);

        var updated = await _context.Customers.FindAsync(customer.Id);
        updated!.LastName.Should().Be("Updated");
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ExistingCustomer_RemovesFromDatabase()
    {
        var customer = CreateCustomer(1, "Juan", "Dela Cruz");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(customer.Id);

        var deleted = await _context.Customers.FindAsync(customer.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        var act = () => _repository.DeleteAsync(9999);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Helpers

    private static Customer CreateCustomer(
        int userId,
        string firstName,
        string lastName,
        string? city = null,
        string? country = null,
        string? province = null,
        string? postalCode = null,
        string? phone = null) => new()
    {
        UserId = userId,
        FirstName = firstName,
        LastName = lastName,
        City = city,
        Country = country,
        Province = province,
        PostalCode = postalCode,
        Phone = phone,
        CreatedAt = DateTime.UtcNow
    };

    #endregion

    public void Dispose() => _context.Dispose();
}