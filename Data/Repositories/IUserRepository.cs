using BlogApi.Models;

namespace BlogApi.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmail(string email);
    Task<bool> AddUser(User user);
}