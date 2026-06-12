using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

[Authorize]
public class CategoriesController : BaseController<Category>
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryService service, ICategoryRepository categoryRepository) : base(service)
    {
        _categoryRepository = categoryRepository;
    }

    public override async Task<IActionResult> Create([FromBody] Category entity)
    {
        if (await _categoryRepository.ExistsAsync(entity.Name))
            return Conflict(new { message = $"Category '{entity.Name}' already exists." });

        entity.CreatedAt = DateTime.UtcNow;
        return await base.Create(entity);
    }

    public override async Task<IActionResult> Update(int id, [FromBody] Dictionary<string, object?> fields)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Category with id {id} was not found." });

        if (fields.TryGetValue("name", out var nameValue) && nameValue is not null)
        {
            var newName = nameValue.ToString()!;
            if (!string.Equals(existing.Name, newName, StringComparison.OrdinalIgnoreCase))
            {
                if (await _categoryRepository.ExistsAsync(newName, id))
                    return Conflict(new { message = $"Category '{newName}' already exists." });
            }
        }

        return await base.Update(id, fields);
    }

    protected override object GetEntityId(Category entity) => entity.Id;
}
