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

public class ProductImageControllerTests
{
    private readonly Mock<IProductService>       _serviceMock       = new();
    private readonly Mock<IProductRepository>    _repositoryMock    = new();
    private readonly Mock<IBlobStorageService>   _blobStorageMock   = new();
    private readonly Mock<IEntityImageRepository> _imageRepoMock    = new();
    private readonly ProductsController          _controller;

    public ProductImageControllerTests()
    {
        _controller = new ProductsController(
            _serviceMock.Object,
            _repositoryMock.Object,
            new ReflectionEntityPatcher(),
            _blobStorageMock.Object,
            _imageRepoMock.Object);
    }

    // ── GetImages ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetImages_ExistingProduct_ReturnsImages()
    {
        var product = CreateProduct(1);
        var images  = new List<EntityImage>
        {
            new() { Id = 1, EntityType = "product", EntityId = 1, ImagePath = "https://cdn/a.png", SortOrder = 0 },
            new() { Id = 2, EntityType = "product", EntityId = 1, ImagePath = "https://cdn/b.png", SortOrder = 1 },
        };

        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _imageRepoMock.Setup(r => r.GetByEntityAsync("product", 1, It.IsAny<CancellationToken>())).ReturnsAsync(images);

        var result = await _controller.GetImages(1, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(images);
    }

    [Fact]
    public async Task GetImages_NonExistentProduct_Returns404()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var result = await _controller.GetImages(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── UploadImages ──────────────────────────────────────────────────────

    [Fact]
    public async Task UploadImages_ValidFiles_ReturnsCreatedImages()
    {
        var product  = CreateProduct(1);
        var existing = new List<EntityImage>();
        var saved    = new EntityImage { Id = 10, EntityType = "product", EntityId = 1, ImagePath = "https://cdn/x.png", SortOrder = 0 };

        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _imageRepoMock.Setup(r => r.GetByEntityAsync("product", 1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _blobStorageMock.Setup(b => b.UploadAsync(It.IsAny<Stream>(), "photo.png", "image/png", It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://cdn/x.png");
        _imageRepoMock.Setup(r => r.AddAsync(It.IsAny<EntityImage>(), It.IsAny<CancellationToken>())).ReturnsAsync(saved);

        var content = new byte[] { 1, 2, 3 };
        var file    = CreateFormFile(content, "photo.png", "image/png");
        var files   = new FormFileCollection { file };

        var result = await _controller.UploadImages(1, files, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var list = ok.Value.Should().BeOfType<List<EntityImage>>().Subject;
        list.Should().ContainSingle(i => i.ImagePath == "https://cdn/x.png");
    }

    [Fact]
    public async Task UploadImages_NoFiles_Returns400()
    {
        var product = CreateProduct(1);
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _controller.UploadImages(1, new FormFileCollection(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImages_AppendsSortOrderAfterExisting()
    {
        var product  = CreateProduct(1);
        var existing = new List<EntityImage>
        {
            new() { Id = 5, SortOrder = 0 },
            new() { Id = 6, SortOrder = 1 },
        };

        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _imageRepoMock.Setup(r => r.GetByEntityAsync("product", 1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _blobStorageMock.Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://cdn/new.png");

        EntityImage? captured = null;
        _imageRepoMock.Setup(r => r.AddAsync(It.IsAny<EntityImage>(), It.IsAny<CancellationToken>()))
            .Callback<EntityImage, CancellationToken>((img, _) => captured = img)
            .ReturnsAsync((EntityImage img, CancellationToken _) => img);

        var file  = CreateFormFile(new byte[] { 1 }, "new.png", "image/png");
        var files = new FormFileCollection { file };

        await _controller.UploadImages(1, files, CancellationToken.None);

        captured!.SortOrder.Should().Be(2); // max existing (1) + 1
    }

    // ── DeleteImage ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteImage_ValidImageId_Returns204()
    {
        var image = new EntityImage { Id = 7, EntityType = "product", EntityId = 1, ImagePath = "https://cdn/a.png" };
        _imageRepoMock.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(image);

        var result = await _controller.DeleteImage(1, 7, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _imageRepoMock.Verify(r => r.DeleteAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteImage_WrongProductId_Returns404()
    {
        // Image belongs to product 99, not product 1
        var image = new EntityImage { Id = 7, EntityType = "product", EntityId = 99, ImagePath = "https://cdn/a.png" };
        _imageRepoMock.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(image);

        var result = await _controller.DeleteImage(1, 7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── ReorderImages ─────────────────────────────────────────────────────

    [Fact]
    public async Task ReorderImages_ValidProduct_CallsRepositoryAndReturnsUpdated()
    {
        var product  = CreateProduct(1);
        var reordered = new List<EntityImage> { new() { Id = 2, SortOrder = 0 }, new() { Id = 1, SortOrder = 1 } };

        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _imageRepoMock.Setup(r => r.GetByEntityAsync("product", 1, It.IsAny<CancellationToken>())).ReturnsAsync(reordered);

        var items = new List<ImageSortItem> { new(2, 0), new(1, 1) };
        var result = await _controller.ReorderImages(1, items, CancellationToken.None);

        _imageRepoMock.Verify(r => r.ReorderAsync(It.IsAny<IEnumerable<(int, int)>>(), It.IsAny<CancellationToken>()), Times.Once);
        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Product CreateProduct(int id) => new()
    {
        Id = id, Name = "MacBook Pro", Description = "Test", Price = 999m,
        Stock = 10, IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1,
    };

    private static IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "files", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
    }
}
