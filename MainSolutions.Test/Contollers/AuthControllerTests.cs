using MainSolutions.API.Controllers;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.Test.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    #region Login

    [Fact]
    public async Task Login_ValidRequest_Returns200WithToken()
    {
        var response = new LoginResponse
        {
            Token = "jwt-token",
            Email = "john@example.com",
            Username = "john_doe",
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Login(new LoginRequest
        {
            Email = "john@example.com",
            Password = "password123"
        }, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password."));

        var result = await _controller.Login(new LoginRequest
        {
            Email = "john@example.com",
            Password = "wrong"
        }, CancellationToken.None);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    #endregion

    #region Register

    [Fact]
    public async Task Register_ValidRequest_Returns201()
    {
        var response = new LoginResponse
        {
            Token = "jwt-token",
            Email = "new@example.com",
            Username = "jane_doe",
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };
        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Register(new RegisterRequest
        {
            Email = "new@example.com",
            Password = "password123",
            Username = "jane_doe"
        }, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("An account with this email already exists."));

        var result = await _controller.Register(new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123",
            Username = "jane_doe"
        }, CancellationToken.None);

        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    #endregion
}