using BlogApi.Models;

namespace BlogApi.Data.Repositories.AuthorRepo;

public interface IAuthorRepository
{
    Task<List<Author>> GetAllAuthors();
    Task<Author?> GetAuthorById(Guid id);
    Task AddAuthor(Author author);
    Task Save();
}