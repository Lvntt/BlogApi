using BlogApi.Models;
using BlogApi.Models.Types;

namespace BlogApi.Data.Repositories.CommunityRepo;

public interface ICommunityRepository
{
    Task<Community?> GetCommunityByName(string name);
    Task<Community?> GetCommunityById(Guid communityId);
    List<CommunityMember> GetCommunityAdministrators(Community community);
    Task AddCommunityMember(CommunityMember communityMember);
    void RemoveCommunityMember(CommunityMember communityMember);
    Task<Guid> CreateCommunity(Community community);
    Task<List<CommunityMember>> GetUserCommunities(Guid userId);
    Task<List<Community>> GetAllCommunities();
    Task<CommunityRole?> GetUserRoleInCommunity(Guid communityId, Guid userId);
    Task<CommunityMember?> GetSubscriber(Guid communityId, Guid userId);
    Task<CommunityMember?> GetAdministrator(Guid communityId, Guid userId);
    Task<CommunityMember?> GetCommunityMember(Guid communityId, Guid userId);
    IQueryable<Post> GetCommunityPosts(Community community);
    Task Save();
}