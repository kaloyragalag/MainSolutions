using MainSolutions.API.Controllers;
using MainSolutions.API.Models;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MainSolutions.Test.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        _repositoryMock = new Mock<IProductRepository>();
        _controller = new ProductsController(_serviceMock.Object, _repositoryMock.Object);
    }

    #region GetAll

    [Fact]
    public async Task GetAll_ReturnsPagedResult()
    {
        var paged = new PagedResult<Product>
        {
            Items = [CreateProduct(1, "MacBook Pro", 1), CreateProduct(2, "Dell XPS", 1)],
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<PaginationQuery>())).ReturnsAsync(paged);

        var result = await _controller.GetAll(new PaginationQuery());

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(paged);
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_ExistingProduct_Returns200WithCategory()
    {
        var product = CreateProductWithCategory(1, "MacBook Pro", 1);
        _repositoryMock.Setup(r => r.GetByIdWithCategoryAsync(1)).ReturnsAsync(product);

        var result = await _controller.GetById(1);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        var value = ok.Value.Should().BeOfType<Product>().Subject;
        value.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_NonExistentProduct_Returns404()
    {
        _repositoryMock.Setup(r => r.GetByIdWithCategoryAsync(999)).ReturnsAsync((Product?)null);

        var result = await _controller.GetById(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_ValidProduct_Returns201()
    {
        var product = CreateProduct(1, "MacBook Pro", 1);
        _repositoryMock.Setup(r => r.CategoryExistsAsync(1)).ReturnsAsync(true);
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Product>())).ReturnsAsync(product);

        var result = await _controller.Create(product);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Create_InvalidCategoryId_Returns400()
    {
        var product = CreateProduct(1, "MacBook Pro", 999);
        _repositoryMock.Setup(r => r.CategoryExistsAsync(999)).ReturnsAsync(false);

        var result = await _controller.Create(product);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_NonExistentProduct_Returns404()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _controller.Update(999, new Dictionary<string, object?> { ["name"] = "Updated" });

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Update_InvalidCategoryId_Returns400()
    {
        var product = CreateProduct(1, "MacBook Pro", 1);
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);
        _repositoryMock.Setup(r => r.CategoryExistsAsync(999)).ReturnsAsync(false);

        var result = await _controller.Update(1, new Dictionary<string, object?> { ["categoryId"] = "999" });

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Update_ValidFields_Returns200WithCategory()
    {
        var product = CreateProduct(1, "MacBook Pro", 1);
        var productWithCategory = CreateProductWithCategory(1, "MacBook Pro", 1);
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);
        _repositoryMock.Setup(r => r.GetByIdWithCategoryAsync(1)).ReturnsAsync(productWithCategory);

        var result = await _controller.Update(1, new Dictionary<string, object?> { ["name"] = "MacBook Pro Updated" });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        var value = ok.Value.Should().BeOfType<Product>().Subject;
        value.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_SetsUpdatedAt()
    {
        var product = CreateProduct(1, "MacBook Pro", 1);
        var productWithCategory = CreateProductWithCategory(1, "MacBook Pro", 1);
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);
        _repositoryMock.Setup(r => r.GetByIdWithCategoryAsync(1)).ReturnsAsync(productWithCategory);

        await _controller.Update(1, new Dictionary<string, object?> { ["stock"] = "20" });

        _serviceMock.Verify(s => s.UpdateAsync(It.Is<Product>(p => p.UpdatedAt != null)), Times.Once);
    }

    [Fact]
    public async Task Update_ProtectedFields_AreIgnored()
    {
        var product = CreateProduct(1, "MacBook Pro", 1);
        var originalCreatedAt = product.CreatedAt;
        var productWithCategory = CreateProductWithCategory(1, "MacBook Pro", 1);
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);
        _repositoryMock.Setup(r => r.GetByIdWithCategoryAsync(1)).ReturnsAsync(productWithCategory);

        await _controller.Update(1, new Dictionary<string, object?>
        {
            ["id"] = 999,
            ["createdAt"] = "2000-01-01"
        });

        product.Id.Should().Be(1);
        product.CreatedAt.Should().Be(originalCreatedAt);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ExistingProduct_Returns204()
    {
        var product = CreateProduct(1, "MacBook Pro", 1);
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistentProduct_Returns404()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _controller.Delete(999);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    #region Helpers

    private static Product CreateProduct(int id, string name, int categoryId) => new()
    {
        Id = id,
        Name = name,
        Description = "Test description",
        Price = 999.99m,
        Stock = 10,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CategoryId = categoryId
    };

    private static Product CreateProductWithCategory(int id, string name, int categoryId) => new()
    {
        Id = id,
        Name = name,
        Description = "Test description",
        Price = 999.99m,
        Stock = 10,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CategoryId = categoryId,
        Category = new Category
        {
            Id = categoryId,
            Name = "Laptops",
            Description = "Portable computers",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    };

    #endregion
}
