using BlogApi.Models;
using BlogApi.Models.Types;

namespace BlogApi.Data.Repositories.CommunityRepository;

public interface ICommunityRepository
{
    Task<Community?> GetCommunityByName(string name);
    Task<Community?> GetCommunityById(Guid communityId);
    List<CommunityMember> GetCommunityAdministrators(Community community);
    Task AddCommunityMember(CommunityMember communityMember);
    Task RemoveCommunityMember(CommunityMember communityMember);
    Task<Guid> CreateCommunity(Community community);
    Task<List<CommunityMember>> GetUserCommunities(Guid userId);
    Task<List<Community>> GetAllCommunities();
    Task<CommunityRole?> GetUserRoleInCommunity(Guid communityId, Guid userId);
    CommunityMember? GetSubscriber(Guid communityId, Guid userId);
    CommunityMember? GetAdministrator(Guid communityId, Guid userId);
    CommunityMember? GetCommunityMember(Guid communityId, Guid userId);
    Task SubscribeToCommunity(Guid communityId, Guid userId);
    Task IncrementCommunitySubscribers(Community community);
    Task DecrementCommunitySubscribers(Community community);
}