using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Data.Repositories.PostRepository;

public interface IPostRepository
{
    Task<Guid> AddPost(Post post);
}