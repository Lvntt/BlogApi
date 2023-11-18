using BlogApi.Models;

namespace BlogApi.Services.JwtService;

public interface IJwtService
{
    string GenerateToken(User user);
}