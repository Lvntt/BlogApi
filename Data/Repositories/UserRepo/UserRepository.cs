using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.UserRepo;

public class UserRepository : IUserRepository
{
    private readonly BlogDbContext _context;

    public UserRepository(BlogDbContext context)
    {
        _context = context;
    }
    
    public Task<User?> GetUserByEmail(string email)
    {
        return  _context.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task AddUser(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public Task<User?> GetUserById(Guid id)
    {
        return _context.Users.FirstOrDefaultAsync(user => user.Id == id);
    }

    public Task Save()
    {
        return _context.SaveChangesAsync();
    }
}