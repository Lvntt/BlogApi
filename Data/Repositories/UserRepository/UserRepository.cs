using BlogApi.Dtos;
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
        // TODO firstOrDefault
        return await _context.Users.SingleOrDefaultAsync(user => user.Email == email);
    }

    public async Task<bool> AddUser(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetUserById(Guid id)
    {
        return await _context.Users.SingleOrDefaultAsync(user => user.Id == id);
    }

    public async Task<bool> EditUserProfile(Guid id, UserEditDto editedUser)
    {
        var existingUser = await GetUserById(id);
        if (existingUser == null) return false;
        
        existingUser.FullName = editedUser.FullName;
        existingUser.Email = editedUser.Email;
        existingUser.BirthDate = editedUser.BirthDate;
        existingUser.Gender = editedUser.Gender;
        existingUser.PhoneNumber = editedUser.PhoneNumber;
        await _context.SaveChangesAsync();
        return true;
    }
}