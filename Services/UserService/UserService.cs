using AutoMapper;
using BlogApi.Data.Repositories;
using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.UserService;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenBlacklistRepository _tokenBlacklistRepository;

    public UserService(IUserRepository userRepository, ITokenBlacklistRepository tokenBlacklistRepository)
    {
        _userRepository = userRepository;
        _tokenBlacklistRepository = tokenBlacklistRepository;
    }

    public async Task<ServiceResponse<User>> Register(UserRegisterDto request)
    {
        var existingUser = await _userRepository.GetUserByEmail(request.Email);
        if (existingUser != null)
        {
            return new ServiceResponse<User>
            {
                IsSuccessful = false,
                ErrorMessage = $"User with email {request.Email} already exists."
            };
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            PasswordHash = passwordHash,
            Email = request.Email,
            BirthDate = request.BirthDate,
            Gender = request.Gender,
            PhoneNumber = request.PhoneNumber
        };

        var isSuccess = await _userRepository.AddUser(user);
        if (!isSuccess)
        {
            return new ServiceResponse<User>
            {
                IsSuccessful = false,
                ErrorMessage = "Error while adding the user."
            };
        }

        return new ServiceResponse<User>
        {
            Data = user
        };
    }

    public async Task<ServiceResponse<User>> Login(LoginCredentialsDto request)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        return user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
            ? new ServiceResponse<User>
            {
                Data = user
            }
            : new ServiceResponse<User>
            {
                IsSuccessful = false,
                ErrorMessage = "Invalid email or password"
            };
    }

    public async Task<bool> Logout(TokenModel token)
    {
        if (await _tokenBlacklistRepository.GetTokenFromBlacklist(token) != null)
        {
            return false;
        }
        return await _tokenBlacklistRepository.BlacklistToken(token);
    }
}