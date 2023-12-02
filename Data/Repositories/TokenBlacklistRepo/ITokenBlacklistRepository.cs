using BlogApi.Models;

namespace BlogApi.Data.Repositories.TokenBlacklistRepo;

public interface ITokenBlacklistRepository
{
    Task BlacklistToken(TokenModel token);
    Task<TokenModel?> GetTokenFromBlacklist(TokenModel token);
    Task Save();
}