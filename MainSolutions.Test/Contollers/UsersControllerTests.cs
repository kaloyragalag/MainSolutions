using MainSolutions.API.Controllers;
using MainSolutions.API.Models;
using MainSolutions.API.DTOs;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MainSolutions.Test.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _userRepoMock = new Mock<IUserRepository>();
        _controller = new UsersController(_userServiceMock.Object, _userRepoMock.Object, new ReflectionEntityPatcher());
    }

    #region GetAll

    [Fact]
    public async Task GetAll_ReturnsPagedResult()
    {
        var paged = new PagedResult<User>
        {
            Items = [CreateUser(1, "john@example.com"), CreateUser(2, "jane@example.com")],
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        _userServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<PaginationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _controller.GetAll(new PaginationQuery(), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(paged);
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_ExistingUser_Returns200()
    {
        var user = CreateUser(1, "john@example.com");
        _userServiceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _controller.GetById(1, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetById_NonExistentUser_Returns404()
    {
        _userServiceMock
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_NonExistentUser_Returns404()
    {
        _userServiceMock
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _controller.Update(999, new Dictionary<string, object?> { ["firstName"] = "Karl" }, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Update_SameEmail_DoesNotCheckForDuplicate()
    {
        var user = CreateUser(1, "john@example.com");
        _userServiceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _controller.Update(1, new Dictionary<string, object?> { ["email"] = "john@example.com" }, CancellationToken.None);

        _userRepoMock.Verify(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_DuplicateEmail_Returns409()
    {
        var user = CreateUser(1, "john@example.com");
        _userServiceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock
            .Setup(r => r.ExistsAsync("taken@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Update(1, new Dictionary<string, object?> { ["email"] = "taken@example.com" }, CancellationToken.None);

        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Update_ValidFields_Returns200WithUpdatedUser()
    {
        var user = CreateUser(1, "john@example.com");
        _userServiceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Update(1, new Dictionary<string, object?> { ["firstName"] = "Updated" }, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Update_ProtectedFields_AreIgnored()
    {
        var user = CreateUser(1, "john@example.com");
        var originalHash = user.PasswordHash;
        _userServiceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _controller.Update(1, new Dictionary<string, object?>
        {
            ["passwordHash"] = "hacked",
            ["id"] = 999,
            ["createdAt"] = "2000-01-01"
        }, CancellationToken.None);

        user.PasswordHash.Should().Be(originalHash);
        user.Id.Should().Be(1);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ExistingUser_Returns204()
    {
        var user = CreateUser(1, "john@example.com");
        _userServiceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _controller.Delete(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _userServiceMock.Verify(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistentUser_Returns404()
    {
        _userServiceMock
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _controller.Delete(999, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    #region Helpers

    private static User CreateUser(int id, string email) => new()
    {
        Id = id,
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
        Username = "john_doe",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    #endregion
}
