using MainSolutions.API.Models;
using MainSolutions.API.DTOs;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services;
using MainSolutions.API.Services.Interfaces;
using Moq;

namespace MainSolutions.Test.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();

        _tokenServiceMock
            .Setup(t => t.GenerateToken(It.IsAny<User>(), It.IsAny<DateTime>()))
            .Returns("jwt-token");

        _authService = new AuthService(_userRepoMock.Object, _tokenServiceMock.Object);
    }

    #region Login

    [Fact]
    public async Task Login_ValidCredentials_ReturnsLoginResponse()
    {
        var user = CreateActiveUser("john@example.com", "password123");
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

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
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

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
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

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
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

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
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _authService.LoginAsync(new LoginRequest
        {
            Email = "john@example.com",
            Password = "password123"
        });

        _userRepoMock.Verify(
            r => r.UpdateAsync(It.Is<User>(u => u.LastLoginAt != null), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Register

    [Fact]
    public async Task Register_NewUser_ReturnsLoginResponse()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "new@example.com",
            Password = "password123",
            Username = "jane_doe"
        });

        result.Should().NotBeNull();
        result.Email.Should().Be("new@example.com");
        result.Username.Should().Be("jane_doe");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ExistingEmail_ThrowsInvalidOperation()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _authService.RegisterAsync(new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123",
            Username = "jane_doe"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("An account with this email already exists.");
    }

    [Fact]
    public async Task Register_NewUser_HashesPassword()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "new@example.com",
            Password = "plaintext",
            Username = "jane_doe"
        });

        _userRepoMock.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.PasswordHash != "plaintext" && u.PasswordHash.StartsWith("$2")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helpers

    private static User CreateActiveUser(string email, string plainPassword) => new()
    {
        Id = 1,
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
        Username = "john_doe",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    #endregion
}