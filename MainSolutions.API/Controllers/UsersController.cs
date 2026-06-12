using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

[Authorize]
public class UsersController : BaseController<User>
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserService service, IUserRepository userRepository) : base(service)
    {
        _userRepository = userRepository;
    }

    [Authorize(Roles = "Admin,Editor")]
    public override async Task<IActionResult> Update(int id, [FromBody] Dictionary<string, object?> fields)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"User with id {id} was not found." });

        if (fields.TryGetValue("email", out var emailValue) && emailValue is not null)
        {
            var newEmail = emailValue.ToString()!;
            if (!string.Equals(existing.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _userRepository.ExistsAsync(newEmail);
                if (emailTaken)
                    return Conflict(new { message = $"Email '{newEmail}' is already in use." });
            }
        }

        // Protect sensitive fields from being updated via this endpoint
        fields.Remove("passwordHash");

        return await base.Update(id, fields);
    }
    
    protected override object GetEntityId(User entity) => entity.Id;
}