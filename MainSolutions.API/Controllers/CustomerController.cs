using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

[Authorize]
public class CustomerController : BaseController<Customer>
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerController(ICustomerService service, ICustomerRepository repository, IEntityPatcher patcher)
        : base(service, patcher)
    {
        _customerRepository = repository;
    }

    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Create([FromBody] Customer entity, CancellationToken cancellationToken)
    {
        if (!await _customerRepository.UserExistsAsync(entity.UserId, cancellationToken))
            return BadRequest(new { message = $"User with id {entity.UserId} does not exist." });

        if (await _customerRepository.GetByUserIdAsync(entity.UserId, cancellationToken) is not null)
            return Conflict(new { message = $"User with id {entity.UserId} is already associated with a customer." });

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
            return NotFound(new { message = $"Customer with id {id} was not found." });

        if (fields.TryGetValue("userId", out var userIdValue) && userIdValue is not null)
        {
            if (int.TryParse(userIdValue.ToString(), out var userId))
            {
                if (!await _customerRepository.UserExistsAsync(userId, cancellationToken))
                    return BadRequest(new { message = $"User with id {userId} does not exist." });
            }
        }

        await base.Update(id, fields, cancellationToken);

        var updated = await _customerRepository.GetByIdAsync(id, cancellationToken);
        return Ok(updated);
    }

    protected override object GetEntityId(Customer entity) => entity.Id;
}
