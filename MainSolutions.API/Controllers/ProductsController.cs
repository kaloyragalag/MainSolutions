using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

[Authorize]
public class ProductsController : BaseController<Product>
{
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductService service, IProductRepository productRepository, IEntityPatcher patcher)
        : base(service, patcher)
    {
        _productRepository = productRepository;
    }

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
    public override async Task<IActionResult> Create([FromBody] Product entity, CancellationToken cancellationToken)
    {
        if (!await _productRepository.CategoryExistsAsync(entity.CategoryId, cancellationToken))
            return BadRequest(new { message = $"Category with id {entity.CategoryId} does not exist." });

        entity.CreatedAt = DateTime.UtcNow;
        return await base.Create(entity, cancellationToken);
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<IActionResult> Update(int id, [FromBody] Dictionary<string, object?> fields, CancellationToken cancellationToken)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        if (fields.TryGetValue("categoryId", out var categoryIdValue) && categoryIdValue is not null)
        {
            if (int.TryParse(categoryIdValue.ToString(), out var categoryId))
            {
                if (!await _productRepository.CategoryExistsAsync(categoryId, cancellationToken))
                    return BadRequest(new { message = $"Category with id {categoryId} does not exist." });
            }
        }

        await base.Update(id, fields, cancellationToken);

        var updated = await _productRepository.GetByIdWithCategoryAsync(id, cancellationToken);
        return Ok(updated);
    }

    protected override object GetEntityId(Product entity) => entity.Id;
}
