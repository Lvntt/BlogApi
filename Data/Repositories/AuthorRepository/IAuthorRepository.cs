using BlogApi.Models;

namespace BlogApi.Data.Repositories.AuthorRepository;

public interface IAuthorRepository
{
    Task<Author?> GetAuthorById(Guid id);
    Task AddAuthor(Author author);
    Task IncrementAuthorPosts(Guid id);
    Task Save();
}