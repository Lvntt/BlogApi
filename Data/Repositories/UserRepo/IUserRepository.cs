using BlogApi.Models;

namespace BlogApi.Data.Repositories.UserRepo;

public interface IUserRepository
{
    Task<User?> GetUserByEmail(string email);
    Task AddUser(User user);
    Task<User?> GetUserById(Guid id);
    void EditUserProfile(User user);
    Task Save();
}