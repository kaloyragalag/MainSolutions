using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

// MainSolutions.API/Controllers/ProductsController.cs
[Authorize]
public class ProductsController : BaseController<Product>
{
    private readonly IProductRepository _productRepository;
    private readonly IBlobStorageService _blobStorageService;

    private static readonly string[] AllowedImageContentTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    public ProductsController(
        IProductService service,
        IProductRepository productRepository,
        IEntityPatcher patcher,
        IBlobStorageService blobStorageService)
        : base(service, patcher)
    {
        _productRepository = productRepository;
        _blobStorageService = blobStorageService;
    }

    // ... existing GetById / Create / Update / GetEntityId unchanged ...

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost("{id:int}/image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No image file was provided." });

        if (file.Length > MaxImageSizeBytes)
            return BadRequest(new { message = "Image must be 5MB or smaller." });

        if (!AllowedImageContentTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Image must be JPEG, PNG, WEBP, or GIF." });

        var previousImagePath = product.ImagePath;

        using var stream = file.OpenReadStream();
        product.ImagePath = await _blobStorageService.UploadAsync(stream, file.FileName, file.ContentType, cancellationToken);
        product.UpdatedAt = DateTime.UtcNow;
        await _service.UpdateAsync(product, cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousImagePath))
            await _blobStorageService.DeleteAsync(previousImagePath, cancellationToken);

        var updated = await _productRepository.GetByIdWithCategoryAsync(id, cancellationToken);
        return Ok(updated);
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpDelete("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id, CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        if (!string.IsNullOrWhiteSpace(product.ImagePath))
        {
            await _blobStorageService.DeleteAsync(product.ImagePath, cancellationToken);
            product.ImagePath = null;
            product.UpdatedAt = DateTime.UtcNow;
            await _service.UpdateAsync(product, cancellationToken);
        }

        var updated = await _productRepository.GetByIdWithCategoryAsync(id, cancellationToken);
        return Ok(updated);
    }
}
