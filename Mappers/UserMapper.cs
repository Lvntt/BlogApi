using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Mappers;

public static class UserMapper
{
    public static User MapToUser(UserRegisterDto userRegisterDto, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = userRegisterDto.FullName,
            PasswordHash = passwordHash,
            Email = userRegisterDto.Email,
            BirthDate = userRegisterDto.BirthDate,
            Gender = userRegisterDto.Gender,
            PhoneNumber = userRegisterDto.PhoneNumber,
            CreateTime = DateTime.UtcNow
        };
    }
}