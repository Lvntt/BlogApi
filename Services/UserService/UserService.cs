using AutoMapper;
using BlogApi.Data.Repositories.TokenBlacklistRepo;
using BlogApi.Dtos;
using BlogApi.Models;
using BlogApi.Data.Repositories.UserRepo;
using BlogApi.Exceptions;
using BlogApi.Mappers;

namespace BlogApi.Services.UserService;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly ITokenBlacklistRepository _tokenBlacklistRepository;

    public UserService(IUserRepository userRepository, ITokenBlacklistRepository tokenBlacklistRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _tokenBlacklistRepository = tokenBlacklistRepository;
        _mapper = mapper;
    }

    public async Task<User> Register(UserRegisterDto userRegisterDto)
    {
        if (await _userRepository.GetUserByEmail(userRegisterDto.Email) == null)
            throw new EntityExistsException("User with this email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password);

        var user = UserMapper.MapToUser(userRegisterDto, passwordHash);

        await _userRepository.AddUser(user);
        await _userRepository.Save();
        return user;
    }

    public async Task<User> Login(LoginCredentialsDto loginCredentialsDto)
    {
        var user = await _userRepository.GetUserByEmail(loginCredentialsDto.Email)
                   ?? throw new EntityNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(loginCredentialsDto.Password, user.PasswordHash))
            throw new InvalidCredentialsException("Invalid email or password.");

        return user;
    }

    public async Task Logout(TokenModel token)
    {
        await _tokenBlacklistRepository.BlacklistToken(token);
        await _tokenBlacklistRepository.Save();
    }

    public async Task<UserDto> GetUserProfile(Guid id)
    {
        var user = await _userRepository.GetUserById(id)
                   ?? throw new EntityNotFoundException("User not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task EditUserProfile(UserEditDto userEditDto, Guid id)
    {
        var user = await _userRepository.GetUserById(id)
                   ?? throw new EntityNotFoundException("User not found.");

        user.FullName = userEditDto.FullName;
        user.Email = userEditDto.Email;
        user.BirthDate = userEditDto.BirthDate;
        user.Gender = userEditDto.Gender;
        user.PhoneNumber = userEditDto.PhoneNumber;

        await _userRepository.Save();
    }
}