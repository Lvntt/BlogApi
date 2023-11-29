using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.PostService;

public interface IPostService
{
    Task<PostPagedListDto> GetAllAvailablePosts(
        List<Guid>? tags,
        string? author,
        int? min,
        int? max,
        SortingOption? sorting,
        bool onlyMyCommunities,
        int page,
        int size
    );

    Task<Guid> CreatePost(PostCreateDto request, Guid authorId);
    Task<PostFullDto> GetPostInfo(Guid postId, Guid? userId);
    Task AddLikeToPost(Guid postId, Guid userId);
    Task RemoveLikeFromPost(Guid postId, Guid userId);
}