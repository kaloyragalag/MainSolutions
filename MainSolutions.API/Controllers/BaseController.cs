using MainSolutions.API.DTOs;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController<T> : ControllerBase where T : class
{
    protected readonly IBaseService<T> _service;
    protected readonly IEntityPatcher _patcher;

    protected BaseController(IBaseService<T> service, IEntityPatcher patcher)
    {
        _service = service;
        _patcher = patcher;
    }

    /// <summary>Get a paginated list of all records.</summary>
    [Authorize(Roles = "Admin,Editor,Viewer")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAll([FromQuery] PaginationQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single record by ID.</summary>
    [Authorize(Roles = "Admin,Editor,Viewer")]
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await _service.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    /// <summary>Create a new record.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> Create([FromBody] T entity, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = GetEntityId(created) }, created);
    }

    /// <summary>Partially update an existing record — only provided fields are applied.</summary>
    [Authorize(Roles = "Admin,Editor")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Update(int id, [FromBody] Dictionary<string, object?> fields, CancellationToken cancellationToken)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken);
        if (existing is null) return NotFound(new { message = $"Record with id {id} was not found." });

        fields["updatedAt"] = DateTime.UtcNow.ToString("o");
        fields.Remove("createdAt");
        fields.Remove("id");

        _patcher.Apply(existing, fields);
        await _service.UpdateAsync(existing, cancellationToken);
        return Ok(existing);
    }

    /// <summary>Delete a record by ID.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken);
        if (existing is null) return NotFound(new { message = $"Record with id {id} was not found." });

        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Override in derived controllers to return the entity's ID for the Created response.</summary>
    protected virtual object GetEntityId(T entity) => 0;
}
