using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;

namespace MainSolutions.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return BuildLoginResponse(user);
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.ExistsAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Username = request.Username,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.CreateAsync(user, cancellationToken);

        return BuildLoginResponse(user);
    }

    private LoginResponse BuildLoginResponse(User user)
    {
        var expiresAt = DateTime.UtcNow.AddHours(8);
        var token = _tokenService.GenerateToken(user, expiresAt);

        return new LoginResponse
        {
            Token = token,
            Email = user.Email,
            Username = user.Username,
            ExpiresAt = expiresAt
        };
    }
}
