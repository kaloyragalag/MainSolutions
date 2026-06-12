using MainSolutions.API.Models;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController<T> : ControllerBase where T : class
{
    protected readonly IBaseService<T> _service;

    protected BaseController(IBaseService<T> service)
    {
        _service = service;
    }

    /// <summary>Get a paginated list of all records.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAll([FromQuery] PaginationQuery query)
    {
        var result = await _service.GetAllAsync(query);
        return Ok(result);
    }

    /// <summary>Get a single record by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        return entity is null ? NotFound() : Ok(entity);
    }

    /// <summary>Create a new record.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> Create([FromBody] T entity)
    {
        var created = await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = GetEntityId(created) }, created);
    }

    /// <summary>Partially update an existing record — only provided fields are applied.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Update(int id, [FromBody] Dictionary<string, object?> fields)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound(new { message = $"Record with id {id} was not found." });

        fields["updatedAt"] = DateTime.UtcNow.ToString("o");
        fields.Remove("createdAt");
        fields.Remove("id");

        ApplyFields(existing, fields);
        await _service.UpdateAsync(existing);
        return Ok(existing);
    }

    /// <summary>Delete a record by ID.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound(new { message = $"Record with id {id} was not found." });

        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Override in derived controllers to return the entity's ID for the Created response.</summary>
    protected virtual object GetEntityId(T entity) => 0;

    /// <summary>Applies only the provided fields from the request body onto the existing entity.</summary>
    private static void ApplyFields(T entity, Dictionary<string, object?> fields)
    {
        var entityType = typeof(T);
        foreach (var (key, value) in fields)
        {
            var property = entityType.GetProperty(
                key,
                System.Reflection.BindingFlags.IgnoreCase |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance
            );

            if (property is null || !property.CanWrite) continue;

            if (value is null)
            {
                property.SetValue(entity, null);
                continue;
            }

            try
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var converted = Convert.ChangeType(value.ToString(), targetType);
                property.SetValue(entity, converted);
            }
            catch
            {
                // skip fields that can't be converted
            }
        }
    }
}