using FluentAssertions;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MainSolutions.Test.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _configMock = new Mock<IConfiguration>();

        _configMock.Setup(c => c["Jwt:Key"]).Returns("super-secret-test-key-that-is-long-enough");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("MainSolutions.API");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("MainSolutions.React");

        _authService = new AuthService(_userRepoMock.Object, _configMock.Object);
    }

    #region Login

    [Fact]
    public async Task Login_ValidCredentials_ReturnsLoginResponse()
    {
        var user = CreateActiveUser("john@example.com", "password123");
        _userRepoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(user);

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "john@example.com",
            Password = "password123"
        });

        result.Should().NotBeNull();
        result.Email.Should().Be("john@example.com");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_UserNotFound_ThrowsUnauthorized()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var act = () => _authService.LoginAsync(new LoginRequest
        {
            Email = "ghost@example.com",
            Password = "password123"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        var user = CreateActiveUser("john@example.com", "password123");
        _userRepoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(user);

        var act = () => _authService.LoginAsync(new LoginRequest
        {
            Email = "john@example.com",
            Password = "wrongpassword"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Login_InactiveUser_ThrowsUnauthorized()
    {
        var user = CreateActiveUser("john@example.com", "password123");
        user.IsActive = false;
        _userRepoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(user);

        var act = () => _authService.LoginAsync(new LoginRequest
        {
            Email = "john@example.com",
            Password = "password123"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is disabled.");
    }

    [Fact]
    public async Task Login_ValidCredentials_UpdatesLastLoginAt()
    {
        var user = CreateActiveUser("john@example.com", "password123");
        _userRepoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(user);

        await _authService.LoginAsync(new LoginRequest
        {
            Email = "john@example.com",
            Password = "password123"
        });

        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.LastLoginAt != null)), Times.Once);
    }

    #endregion

    #region Register

    [Fact]
    public async Task Register_NewUser_ReturnsLoginResponse()
    {
        _userRepoMock.Setup(r => r.ExistsAsync("new@example.com")).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "new@example.com",
            Password = "password123",
            FirstName = "Jane",
            LastName = "Doe"
        });

        result.Should().NotBeNull();
        result.Email.Should().Be("new@example.com");
        result.FirstName.Should().Be("Jane");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ExistingEmail_ThrowsInvalidOperation()
    {
        _userRepoMock.Setup(r => r.ExistsAsync("existing@example.com")).ReturnsAsync(true);

        var act = () => _authService.RegisterAsync(new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123",
            FirstName = "Jane",
            LastName = "Doe"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("An account with this email already exists.");
    }

    [Fact]
    public async Task Register_NewUser_HashesPassword()
    {
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "new@example.com",
            Password = "plaintext",
            FirstName = "Jane",
            LastName = "Doe"
        });

        _userRepoMock.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.PasswordHash != "plaintext" && u.PasswordHash.StartsWith("$2"))),
            Times.Once);
    }

    #endregion

    #region Helpers

    private static User CreateActiveUser(string email, string plainPassword) => new()
    {
        Id = 1,
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
        FirstName = "John",
        LastName = "Doe",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    #endregion
}
