using System.Security.Authentication;
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

    public async Task<User> Register(UserRegisterDto request)
    {
        var existingUser = await _userRepository.GetUserByEmail(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email {request.Email} already exists.");
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
            PhoneNumber = request.PhoneNumber,
            CreateTime = DateTime.UtcNow
        };

        await _userRepository.AddUser(user);
        return user;
    }

    public async Task<User> Login(LoginCredentialsDto request)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with email {request.Email} not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password.");
        }
        
        return user;
    }

    public async Task Logout(TokenModel token)
    {
        await _tokenBlacklistRepository.BlacklistToken(token);
    }

    public async Task<User> GetUserProfile(Guid id)
    {
        var user = await _userRepository.GetUserById(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        
        return user;
    }

    public async Task EditUserProfile(UserEditDto request, Guid id)
    {
        var user = await _userRepository.GetUserById(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        
        await _userRepository.EditUserProfile(id, request);
    }
}