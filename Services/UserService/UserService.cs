using AutoMapper;
using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Models;
using BlogApi.Exceptions;
using BlogApi.Extensions;
using BlogApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services.UserService;

public class UserService : IUserService
{
    private readonly BlogDbContext _context;
    private readonly IMapper _mapper;

    public UserService(IMapper mapper, BlogDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<User> Register(UserRegisterDto userRegisterDto)
    {
        await _context.GetUserByEmail(userRegisterDto.Email);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password);

        var user = UserMapper.MapToUser(userRegisterDto, passwordHash);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> Login(LoginCredentialsDto loginCredentialsDto)
    {
        var user = await _context.GetUserByEmail(loginCredentialsDto.Email);

        if (!BCrypt.Net.BCrypt.Verify(loginCredentialsDto.Password, user.PasswordHash))
            throw new InvalidCredentialsException("Invalid email or password.");

        return user;
    }

    public async Task Logout(TokenModel token)
    {
        await _context.InvalidTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<UserDto> GetUserProfile(Guid id)
    {
        var user = await _context.GetUserById(id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task EditUserProfile(UserEditDto userEditDto, Guid id)
    {
        var user = await _context.GetUserById(id);

        user.FullName = userEditDto.FullName;
        user.Email = userEditDto.Email;
        user.BirthDate = userEditDto.BirthDate;
        user.Gender = userEditDto.Gender;
        user.PhoneNumber = userEditDto.PhoneNumber;

        await _context.SaveChangesAsync();
    }
}