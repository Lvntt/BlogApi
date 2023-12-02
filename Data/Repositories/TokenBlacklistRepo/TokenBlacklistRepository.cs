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

    public async Task<TokenModel?> GetTokenFromBlacklist(TokenModel token)
    {
        return await _context.InvalidTokens.FirstOrDefaultAsync(t => t.Token == token.Token);
    }

    public async Task Save()
    {
        await _context.SaveChangesAsync();
    }
}