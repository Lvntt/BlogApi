using BlogApi.Dtos;
using BlogApi.Models;
using BlogApi.Models.Types;

namespace BlogApi.Services.CommunityService;

public interface ICommunityService
{
    Task<Guid> CreateCommunity(CommunityCreateDto communityCreateDto, Guid userId);
    Task<CommunityFullDto> GetCommunityInfo(Guid communityId);
    Task<List<CommunityUserDto>> GetUserCommunities(Guid userId);
    Task<List<CommunityDto>> GetAllCommunities();
    Task<CommunityRole?> GetUserRoleInCommunity(Guid communityId, Guid userId);
    Task SubscribeToCommunity(Guid communityId, Guid userId);
    Task UnsubscribeFromCommunity(Guid communityId, Guid userId);
    Task<Guid> CreatePost(PostCreateDto postCreateDto, Guid authorId, Guid communityId);

    Task<PostPagedListDto> GetCommunityPosts(
        Guid? userId,
        Guid communityId,
        List<Guid>? tags,
        SortingOption? sorting,
        int page,
        int size
    );
}