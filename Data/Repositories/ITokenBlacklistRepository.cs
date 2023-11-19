using BlogApi.Models;

namespace BlogApi.Data.Repositories;

public interface ITokenBlacklistRepository
{
    Task<bool> BlacklistToken(TokenModel token);
    Task<TokenModel?> GetTokenFromBlacklist(TokenModel token);
}