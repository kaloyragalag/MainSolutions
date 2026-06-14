namespace MainSolutions.API.Models.DTOs;

public class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
