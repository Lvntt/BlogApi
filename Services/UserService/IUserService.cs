using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.UserService;

public interface IUserService
{
    Task<User> Register(UserRegisterDto userRegisterDto);
    Task<User> Login(LoginCredentialsDto loginCredentialsDto);
    Task Logout(TokenModel token);
    Task<bool> IsTokenInvalid(string token);
    Task<UserDto> GetUserProfile(Guid id);
    Task EditUserProfile(UserEditDto userEditDto, Guid id);
}