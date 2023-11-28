using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Data.Repositories.PostRepository;

public interface IPostRepository
{
    Task<Guid> AddPost(Post post);
    Task<Post?> GetPost(Guid id);
    bool DidUserLikePost(Post post, User user);
    Task AddLikeToPost(Post post, User user);
}