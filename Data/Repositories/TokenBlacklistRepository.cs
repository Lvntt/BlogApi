using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories;

public class TokenBlacklistRepository : ITokenBlacklistRepository
{
    private readonly BlogDbContext _context;

    public TokenBlacklistRepository(BlogDbContext context)
    {
        _context = context;
    }
    
    
    public async Task<bool> BlacklistToken(TokenModel token)
    {
        _context.InvalidTokens.Add(token);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TokenModel?> GetTokenFromBlacklist(TokenModel token)
    {
        return await _context.InvalidTokens.FirstOrDefaultAsync(t => t.Token == token.Token);
    }
    
}