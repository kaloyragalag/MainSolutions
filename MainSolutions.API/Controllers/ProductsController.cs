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

    public ProductsController(IProductService service, IProductRepository productRepository) : base(service)
    {
        _productRepository = productRepository;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<IActionResult> GetById(int id)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    public override async Task<IActionResult> Create([FromBody] Product entity)
    {
        if (!await _productRepository.CategoryExistsAsync(entity.CategoryId))
            return BadRequest(new { message = $"Category with id {entity.CategoryId} does not exist." });

        entity.CreatedAt = DateTime.UtcNow;
        return await base.Create(entity);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<IActionResult> Update(int id, [FromBody] Dictionary<string, object?> fields)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        if (fields.TryGetValue("categoryId", out var categoryIdValue) && categoryIdValue is not null)
        {
            if (int.TryParse(categoryIdValue.ToString(), out var categoryId))
            {
                if (!await _productRepository.CategoryExistsAsync(categoryId))
                    return BadRequest(new { message = $"Category with id {categoryId} does not exist." });
            }
        }

        await base.Update(id, fields);

        var updated = await _productRepository.GetByIdWithCategoryAsync(id);
        return Ok(updated);
    }

    protected override object GetEntityId(Product entity) => entity.Id;
}
