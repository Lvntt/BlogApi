using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmail(string email);
    Task<bool> AddUser(User user);
    User? GetUserById(Guid id);
    Task<bool> EditUserProfile(Guid id, UserEditDto editedUser);
}