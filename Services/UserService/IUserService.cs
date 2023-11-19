using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.UserService;

public interface IUserService
{
    Task<ServiceResponse<User>> Register(UserRegisterDto request);
    Task<ServiceResponse<User>> Login(LoginCredentialsDto request);
    Task<bool> Logout(TokenModel token);
}