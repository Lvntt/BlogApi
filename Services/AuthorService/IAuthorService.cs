using BlogApi.Dtos;

namespace BlogApi.Services.AuthorService;

public interface IAuthorService
{
    Task<List<AuthorDto>> GetAllAuthors();
}