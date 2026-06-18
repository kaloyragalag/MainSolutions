using System.ComponentModel.DataAnnotations;

namespace MainSolutions.API.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; init; } = string.Empty;
}
