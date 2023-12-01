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
    
    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task AddUser(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public Task<User?> GetUserById(Guid id)
    {
        return _context.Users.FirstOrDefaultAsync(user => user.Id == id);
    }

    public void EditUserProfile(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }

    public async Task Save()
    {
        await _context.SaveChangesAsync();
    }
}