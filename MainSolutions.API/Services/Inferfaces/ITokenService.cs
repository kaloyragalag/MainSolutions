using MainSolutions.API.Models;

namespace MainSolutions.API.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, DateTime expiresAt);
}
