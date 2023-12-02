using BlogApi.Models;

namespace BlogApi.Data.Repositories.AuthorRepo;

public interface IAuthorRepository
{
    Task<Author?> GetAuthorById(Guid id);
    Task AddAuthor(Author author);
    Task Save();
}