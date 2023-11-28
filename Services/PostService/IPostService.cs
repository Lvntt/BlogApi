using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.PostService;

public interface IPostService
{
    Task<Guid> CreatePost(PostCreateDto request, Guid authorId);
    Task<PostFullDto> GetPostInfo(Guid id);
    Task AddLikeToPost(Guid postId, Guid userId);
}