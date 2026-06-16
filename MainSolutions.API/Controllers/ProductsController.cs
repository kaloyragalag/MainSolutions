using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

[Authorize]
public class ProductsController : BaseController<Product>
{
    private const string EntityType = "product";

    private readonly IProductRepository _productRepository;
    private readonly IBlobStorageService _blobStorage;
    private readonly IEntityImageRepository _imageRepository;

    public ProductsController(
        IProductService service,
        IProductRepository repository,
        IEntityPatcher patcher,
        IBlobStorageService blobStorage,
        IEntityImageRepository imageRepository)
        : base(service, patcher)
    {
        _productRepository = repository;
        _blobStorage = blobStorage;
        _imageRepository = imageRepository;
    }

    // ── BaseController overrides ───────────────────────────────────────────

    /// <summary>
    /// Returns the product with its Category eagerly loaded.
    /// Overrides the base GetById which returns a bare entity via FindAsync.
    /// </summary>
    [Authorize(Roles = "Admin,Editor,Viewer")]
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public override async Task<IActionResult> Create(
        [FromBody] Product entity, CancellationToken cancellationToken)
    {
        if (!await _productRepository.CategoryExistsAsync(entity.CategoryId, cancellationToken))
            return BadRequest(new { message = $"Category with id {entity.CategoryId} does not exist." });

        return await base.Create(entity, cancellationToken);
    }

    /// <summary>
    /// Validates the incoming categoryId (if present) before delegating to
    /// BaseController.Update, which applies the patcher and sets updatedAt.
    /// Returns the product with Category included.
    /// </summary>
    [Authorize(Roles = "Admin,Editor")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<IActionResult> Update(
        int id, [FromBody] Dictionary<string, object?> fields, CancellationToken cancellationToken)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        if (fields.TryGetValue("categoryId", out var catIdValue) && catIdValue is not null
            && int.TryParse(catIdValue.ToString(), out var categoryId))
        {
            if (!await _productRepository.CategoryExistsAsync(categoryId, cancellationToken))
                return BadRequest(new { message = $"Category with id {categoryId} does not exist." });
        }

        // Delegate to base: it injects updatedAt, strips id/createdAt, patches, and saves.
        await base.Update(id, fields, cancellationToken);

        // Return with Category populated (base would only return bare product).
        var updated = await _productRepository.GetByIdWithCategoryAsync(id, cancellationToken);
        return Ok(updated);
    }

    protected override object GetEntityId(Product entity) => entity.Id;

    // ── Multi-image endpoints ──────────────────────────────────────────────

    /// <summary>Returns all images for a product, ordered by SortOrder.</summary>
    [Authorize(Roles = "Admin,Editor,Viewer")]
    [HttpGet("{id:int}/images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImages(int id, CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        var images = await _imageRepository.GetByEntityAsync(EntityType, id, cancellationToken);
        return Ok(images);
    }

    /// <summary>
    /// Uploads one or more images for a product.
    /// Accepts multipart/form-data with field name "files".
    /// </summary>
    [Authorize(Roles = "Admin,Editor")]
    [HttpPost("{id:int}/images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImages(
        int id, IFormFileCollection files, CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        if (files is null || files.Count == 0)
            return BadRequest(new { message = "No files were provided." });

        var existing = await _imageRepository.GetByEntityAsync(EntityType, id, cancellationToken);
        var nextSort = existing.Count > 0 ? existing.Max(i => i.SortOrder) + 1 : 0;

        var uploaded = new List<EntityImage>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            await using var stream = file.OpenReadStream();
            var url = await _blobStorage.UploadAsync(
                stream, file.FileName, file.ContentType, cancellationToken);

            var image = await _imageRepository.AddAsync(new EntityImage
            {
                EntityType = EntityType,
                EntityId   = id,
                ImagePath  = url,
                SortOrder  = nextSort++,
            }, cancellationToken);

            uploaded.Add(image);
        }

        return Ok(uploaded);
    }

    /// <summary>Deletes a single image by its own id.</summary>
    [Authorize(Roles = "Admin,Editor")]
    [HttpDelete("{id:int}/images/{imageId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(
        int id, int imageId, CancellationToken cancellationToken)
    {
        var image = await _imageRepository.GetByIdAsync(imageId, cancellationToken);
        if (image is null || image.EntityType != EntityType || image.EntityId != id)
            return NotFound(new { message = $"Image with id {imageId} was not found for product {id}." });

        try { await _blobStorage.DeleteAsync(image.ImagePath, cancellationToken); }
        catch { /* best-effort; orphaned blobs handled by storage lifecycle policy */ }

        await _imageRepository.DeleteAsync(imageId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates the display order of images.
    /// Body: [{ "id": 3, "sortOrder": 0 }, { "id": 7, "sortOrder": 1 }, …]
    /// </summary>
    [Authorize(Roles = "Admin,Editor")]
    [HttpPatch("{id:int}/images/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderImages(
        int id, [FromBody] List<ImageSortItem> items, CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        await _imageRepository.ReorderAsync(
            items.Select(x => (x.Id, x.SortOrder)), cancellationToken);

        var updated = await _imageRepository.GetByEntityAsync(EntityType, id, cancellationToken);
        return Ok(updated);
    }
}

/// <param name="Id">EntityImage.Id</param>
/// <param name="SortOrder">Target zero-based display order.</param>
public record ImageSortItem(int Id, int SortOrder);