using BlogApi.Data.DbContext;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.TokenBlacklistRepo;

public class TokenBlacklistRepository : ITokenBlacklistRepository
{
    private readonly BlogDbContext _context;

    public TokenBlacklistRepository(BlogDbContext context)
    {
        _context = context;
    }
    
    
    public async Task BlacklistToken(TokenModel token)
    {
        await _context.InvalidTokens.AddAsync(token);
    }

    public Task<TokenModel?> GetTokenFromBlacklist(TokenModel token)
    {
        return _context.InvalidTokens.FirstOrDefaultAsync(t => t.Token == token.Token);
    }

    public Task Save()
    {
        return _context.SaveChangesAsync();
    }
}