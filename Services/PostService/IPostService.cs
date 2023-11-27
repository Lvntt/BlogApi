using BlogApi.Dtos;

namespace BlogApi.Services.PostService;

public interface IPostService
{
    Task<Guid> CreatePost(PostCreateDto request, Guid authorId);
}