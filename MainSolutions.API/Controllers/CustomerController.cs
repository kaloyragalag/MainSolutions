using MainSolutions.API.Controllers;
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
    public CustomerController(ICustomerService service, ICustomerRepository repository) : base(service)
    {
        _customerRepository = repository;
    }

    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Create([FromBody] Customer entity)
    {
        if (!await _customerRepository.UserExistsAsync(entity.UserId))
            return BadRequest(new { message = $"User with id {entity.UserId} does not exist." });

        // Ensure user is not already a customer
        if (await _customerRepository.GetByUserIdAsync(entity.UserId) is not null)
            return Conflict(new { message = $"User with id {entity.UserId} is already associated with a customer." });

        entity.CreatedAt = DateTime.UtcNow;
        return await base.Create(entity);
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<IActionResult> Update(int id, [FromBody] Dictionary<string, object?> fields)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Customer with id {id} was not found." });

        if (fields.TryGetValue("userId", out var userIdValue) && userIdValue is not null)
        {
            if (int.TryParse(userIdValue.ToString(), out var userId))
            {
                if (!await _customerRepository.UserExistsAsync(userId))
                    return BadRequest(new { message = $"User with id {userId} does not exist." });
            }
        }

        await base.Update(id, fields);

        var updated = await _customerRepository.GetByIdAsync(id);
        return Ok(updated);
    }

    protected override object GetEntityId(Customer entity) => entity.Id;
}