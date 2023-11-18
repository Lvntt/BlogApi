using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BlogDbContext _context;

    public UserRepository(BlogDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.SingleOrDefaultAsync(user => user.Email == email);
    }

    public async Task<bool> AddUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }
}