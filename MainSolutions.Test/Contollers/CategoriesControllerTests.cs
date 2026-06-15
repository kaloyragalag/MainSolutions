using MainSolutions.API.Controllers;
using MainSolutions.API.Models;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MainSolutions.Test.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _serviceMock;
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _serviceMock = new Mock<ICategoryService>();
        _repositoryMock = new Mock<ICategoryRepository>();
        _controller = new CategoriesController(_serviceMock.Object, _repositoryMock.Object, new ReflectionEntityPatcher());
    }

    #region GetAll

    [Fact]
    public async Task GetAll_ReturnsPagedResult()
    {
        var paged = new PagedResult<Category>
        {
            Items = [CreateCategory(1, "Laptops"), CreateCategory(2, "Monitors")],
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        _serviceMock
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
    public async Task GetById_ExistingCategory_Returns200()
    {
        var category = CreateCategory(1, "Laptops");
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _controller.GetById(1, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetById_NonExistentCategory_Returns404()
    {
        _serviceMock
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_UniqueCategory_Returns201()
    {
        var category = CreateCategory(1, "Laptops");
        _repositoryMock
            .Setup(r => r.ExistsAsync("Laptops", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _serviceMock
            .Setup(s => s.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _controller.Create(category, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Create_DuplicateName_Returns409()
    {
        var category = CreateCategory(1, "Laptops");
        _repositoryMock
            .Setup(r => r.ExistsAsync("Laptops", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Create(category, CancellationToken.None);

        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_NonExistentCategory_Returns404()
    {
        _serviceMock
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _controller.Update(999, new Dictionary<string, object?> { ["name"] = "Updated" }, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Update_SameName_DoesNotCheckForDuplicate()
    {
        var category = CreateCategory(1, "Laptops");
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _controller.Update(1, new Dictionary<string, object?> { ["name"] = "Laptops" }, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_DuplicateName_Returns409()
    {
        var category = CreateCategory(1, "Laptops");
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _repositoryMock
            .Setup(r => r.ExistsAsync("Monitors", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Update(1, new Dictionary<string, object?> { ["name"] = "Monitors" }, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Update_ValidFields_Returns200()
    {
        var category = CreateCategory(1, "Laptops");
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Update(1, new Dictionary<string, object?> { ["description"] = "Updated description" }, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Update_ProtectedFields_AreIgnored()
    {
        var category = CreateCategory(1, "Laptops");
        var originalCreatedAt = category.CreatedAt;
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await _controller.Update(1, new Dictionary<string, object?>
        {
            ["id"] = 999,
            ["createdAt"] = "2000-01-01"
        }, CancellationToken.None);

        category.Id.Should().Be(1);
        category.CreatedAt.Should().Be(originalCreatedAt);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ExistingCategory_Returns204()
    {
        var category = CreateCategory(1, "Laptops");
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _controller.Delete(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistentCategory_Returns404()
    {
        _serviceMock
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _controller.Delete(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    #region Helpers

    private static Category CreateCategory(int id, string name) => new()
    {
        Id = id,
        Name = name,
        Description = "Test description",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    #endregion
}
