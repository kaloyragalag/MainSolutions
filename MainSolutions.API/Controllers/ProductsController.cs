using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

[Authorize]
public class ProductsController : BaseImageController<Product>
{
    private readonly IProductRepository _productRepository;

    public ProductsController(
        IProductService service,
        IProductRepository repository,
        IEntityPatcher patcher,
        IBlobStorageService blobStorage,
        IEntityImageRepository imageRepository)
        : base(service, patcher, blobStorage, imageRepository)
    {
        _productRepository = repository;
    }

    /// <summary>Identifies this entity's images in the shared EntityImage table.</summary>
    protected override string EntityType => "product";

    protected override string EntityDisplayName => "Product";

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
}