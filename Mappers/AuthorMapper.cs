using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Mappers;

public static class AuthorMapper
{
    public static AuthorDto MapToAuthorDto(User user, Author author)
    {
        return new AuthorDto
        {
            FullName = user.FullName,
            BirthDate = user.BirthDate,
            Gender = user.Gender,
            Posts = author.Posts,
            Likes = author.Likes,
            Created = user.CreateTime
        };
    }
}