using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.PostService;

public interface IPostService
{
    Task<Guid> CreatePost(PostCreateDto request, Guid authorId);
    Task<PostFullDto> GetPostInfo(Guid postId, Guid? userId);
    Task AddLikeToPost(Guid postId, Guid userId);
}