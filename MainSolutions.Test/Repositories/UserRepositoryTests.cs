using MainSolutions.API.Data;
using MainSolutions.API.DTOs;
using MainSolutions.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.Test.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
    }

    #region GetAll

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await _context.Users.AddRangeAsync(
            CreateUser("user1@example.com"),
            CreateUser("user2@example.com"),
            CreateUser("user3@example.com")
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
    public async Task GetAllAsync_SecondPage_ReturnsRemainingItems()
    {
        await _context.Users.AddRangeAsync(
            CreateUser("user1@example.com"),
            CreateUser("user2@example.com"),
            CreateUser("user3@example.com")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(new PaginationQuery { Page = 2, PageSize = 2 });

        result.Items.Count().Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();
    }

    #endregion

    #region GetByEmail

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        var user = CreateUser("john@example.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync("john@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        var user = CreateUser("john@example.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync("JOHN@EXAMPLE.COM");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistentEmail_ReturnsNull()
    {
        var result = await _repository.GetByEmailAsync("ghost@example.com");
        result.Should().BeNull();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        var user = CreateUser("john@example.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
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
    public async Task CreateAsync_ValidUser_PersistsToDatabase()
    {
        var user = CreateUser("new@example.com");

        var created = await _repository.CreateAsync(user);

        created.Id.Should().BeGreaterThan(0);
        _context.Users.Should().ContainSingle(u => u.Email == "new@example.com");
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateAsync_ExistingUser_SavesChanges()
    {
        var user = CreateUser("john@example.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        user.Username = "updated_user";
        await _repository.UpdateAsync(user);

        var updated = await _context.Users.FindAsync(user.Id);
        updated!.Username.Should().Be("updated_user");
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ExistingUser_RemovesFromDatabase()
    {
        var user = CreateUser("john@example.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(user.Id);

        var deleted = await _context.Users.FindAsync(user.Id);
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
    public async Task ExistsAsync_ByEmail_ExistingEmail_ReturnsTrue()
    {
        var user = CreateUser("john@example.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsAsync("john@example.com");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ByEmail_NonExistentEmail_ReturnsFalse()
    {
        var exists = await _repository.ExistsAsync("ghost@example.com");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ById_ExistingId_ReturnsTrue()
    {
        var user = CreateUser("john@example.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsAsync(user.Id);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ById_NonExistentId_ReturnsFalse()
    {
        var exists = await _repository.ExistsAsync(9999);
        exists.Should().BeFalse();
    }

    #endregion

    #region Helpers

    private static User CreateUser(string email) => new()
    {
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
        Username = "john_doe",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    #endregion

    public void Dispose() => _context.Dispose();
}
