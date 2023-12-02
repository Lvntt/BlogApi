using BlogApi.Models;
using BlogApi.Models.Types;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.CommunityRepository;

public class CommunityRepository : ICommunityRepository
{
    private readonly BlogDbContext _context;

    public CommunityRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Community?> GetCommunityByName(string name)
    {
        return await _context.Communities.FirstOrDefaultAsync(community => community.Name == name);
    }

    public async Task<Community?> GetCommunityById(Guid communityId)
    {
        return await _context.Communities
            .Include(community => community.Members)
            .FirstOrDefaultAsync(community => community.Id == communityId);
    }

    public List<CommunityMember> GetCommunityAdministrators(Community community)
    {
        return community.Members
            .Where(member => member.Role == CommunityRole.Administrator)
            .ToList();
    }

    public async Task AddCommunityMember(CommunityMember communityMember)
    {
        await _context.CommunityMembers.AddAsync(communityMember);
    }

    public void RemoveCommunityMember(CommunityMember communityMember)
    {
        _context.CommunityMembers.Remove(communityMember);
    }

    public async Task<Guid> CreateCommunity(Community community)
    {
        await _context.Communities.AddAsync(community);
        return community.Id;
    }

    public async Task<List<CommunityMember>> GetUserCommunities(Guid userId)
    {
        return await _context.CommunityMembers
            .Where(cm => cm.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<Community>> GetAllCommunities()
    {
        return await _context.Communities.ToListAsync();
    }

    public async Task<CommunityRole?> GetUserRoleInCommunity(Guid communityId, Guid userId)
    {
        var user = await _context.CommunityMembers
            .Where(cm => 
                cm.CommunityId == communityId 
                && cm.UserId == userId)
            .FirstOrDefaultAsync();
        return user?.Role;
    }

    public Task<CommunityMember?> GetSubscriber(Guid communityId, Guid userId)
    {
        return _context.CommunityMembers.FirstOrDefaultAsync(cm =>
            cm.CommunityId == communityId 
            && cm.UserId == userId 
            && cm.Role == CommunityRole.Subscriber);
    }

    public Task<CommunityMember?> GetAdministrator(Guid communityId, Guid userId)
    {
        return _context.CommunityMembers.FirstOrDefaultAsync(cm =>
            cm.CommunityId == communityId 
            && cm.UserId == userId 
            && cm.Role == CommunityRole.Administrator);
    }

    public Task<CommunityMember?> GetCommunityMember(Guid communityId, Guid userId)
    {
        return _context.CommunityMembers.FirstOrDefaultAsync(cm =>
            cm.CommunityId == communityId 
            && cm.UserId == userId);
    }

    public IQueryable<Post> GetCommunityPosts(Community community)
    {
        return _context.Posts
            .Where(post => post.CommunityId == community.Id)
            .Include(post => post.Tags)
            .Include(post => post.LikedPosts);
    }

    // TODO ВЕЗДЕ убрать async/await, заменить await на return 
    public Task Save()
    {
        return _context.SaveChangesAsync();
    }
}