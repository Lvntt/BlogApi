using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services;

public interface IUserService
{
    Task<ServiceResponse<User>> Register(UserRegisterDto request);
    Task<ServiceResponse<User>> Login(LoginCredentialsDto request);
}