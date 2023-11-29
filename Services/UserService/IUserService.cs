using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.UserService;

public interface IUserService
{
    Task<User> Register(UserRegisterDto request);
    Task<User> Login(LoginCredentialsDto request);
    Task Logout(TokenModel token);
    User GetUserProfile(Guid id);
    Task EditUserProfile(UserEditDto request, Guid id);
}