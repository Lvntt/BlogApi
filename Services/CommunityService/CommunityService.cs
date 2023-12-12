using AutoMapper;
using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Exceptions;
using BlogApi.Extensions;
using BlogApi.Mappers;
using BlogApi.Models;
using BlogApi.Models.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services.CommunityService;

public class CommunityService : ICommunityService
{
    private readonly BlogDbContext _context;
    private readonly IMapper _mapper;

    public CommunityService(IMapper mapper, BlogDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<Guid> CreateCommunity(CommunityCreateDto communityCreateDto, Guid userId)
    {
        if (await _context.Communities.FirstOrDefaultAsync(community => community.Name == communityCreateDto.Name) != null)
            throw new InvalidActionException($"Community with name {communityCreateDto.Name} already exists.");

        var newCommunity = CommunityMapper.MapToCommunity(communityCreateDto);

        var newAdministrator = new CommunityMember
        {
            CommunityId = newCommunity.Id,
            UserId = userId,
            Role = CommunityRole.Administrator
        };

        await _context.Communities.AddAsync(newCommunity);
        await _context.CommunityMembers.AddAsync(newAdministrator);
        await _context.SaveChangesAsync();

        return newCommunity.Id;
    }

    public async Task<CommunityFullDto> GetCommunityInfo(Guid communityId)
    {
        var community = await _context.GetCommunityById(communityId);

        var administrators = community.Members
            .Where(member => member.Role == CommunityRole.Administrator)
            .ToList();
        var administratorUserDtos = new List<UserDto>();

        foreach (var administrator in administrators)
        {
            var user = await _context.GetUserById(administrator.UserId);
            var userDto = UserMapper.MapToUserDto(user);
            administratorUserDtos.Add(userDto);
        }

        var communityFullDto = CommunityMapper.MapToCommunityFullDto(community, administratorUserDtos);

        return communityFullDto;
    }

    public async Task<List<CommunityUserDto>> GetUserCommunities(Guid userId)
    {
        var communityUserDtos = await _context.CommunityMembers
            .Where(cm => cm.UserId == userId)
            .Select(community => _mapper.Map<CommunityUserDto>(community))
            .ToListAsync();

        return communityUserDtos;
    }

    public async Task<List<CommunityDto>> GetAllCommunities()
    {
        var communityDtos = await _context.Communities
            .Select(community => _mapper.Map<CommunityDto>(community))
            .ToListAsync();

        return communityDtos;
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

    public async Task SubscribeToCommunity(Guid communityId, Guid userId)
    {
        var existingCommunity = await _context.GetCommunityById(communityId);

        if (await _context.CommunityMembers.FirstOrDefaultAsync(cm =>
                cm.CommunityId == communityId
                && cm.UserId == userId) != null)
            throw new InvalidActionException("User is already a member of this community.");

        var newSubscriber = new CommunityMember
        {
            CommunityId = communityId,
            UserId = userId,
            Role = CommunityRole.Subscriber
        };

        await _context.CommunityMembers.AddAsync(newSubscriber);
        existingCommunity.SubscribersCount++;

        await _context.SaveChangesAsync();
    }

    public async Task UnsubscribeFromCommunity(Guid communityId, Guid userId)
    {
        var existingCommunity = await _context.GetCommunityById(communityId);

        var existingSubscriber = await _context.CommunityMembers.FirstOrDefaultAsync(cm =>
                                     cm.CommunityId == communityId
                                     && cm.UserId == userId
                                     && cm.Role == CommunityRole.Subscriber)
                                 ?? throw new InvalidActionException("User is not a subscriber of this community.");

        _context.CommunityMembers.Remove(existingSubscriber);
        existingCommunity.SubscribersCount--;

        await _context.SaveChangesAsync();
    }

    public async Task<Guid> CreatePost(PostCreateDto postCreateDto, Guid authorId, Guid communityId)
    {
        var user = await _context.GetUserById(authorId);
        var community = await _context.GetCommunityById(communityId);

        if (await _context.CommunityMembers.FirstOrDefaultAsync(cm =>
                cm.CommunityId == community.Id
                && cm.UserId == user.Id
                && cm.Role == CommunityRole.Administrator) == null)
            throw new ForbiddenActionException("Access denied");

        var tags = new List<Tag>();
        if (!postCreateDto.Tags.IsNullOrEmpty())
        {
            foreach (var tagGuid in postCreateDto.Tags)
            {
                var tag = await _context.GetTagById(tagGuid);
                tags.Add(tag);
            }
        }

        var newPost = PostMapper.MapToCommunityPost(postCreateDto, user, community, tags);

        await _context.Posts.AddAsync(newPost);
        var existingAuthor = await _context.GetAuthorById(authorId);

        if (existingAuthor == null)
        {
            var newAuthor = new Author
            {
                UserId = authorId,
                Likes = 0,
                Posts = 1
            };
            await _context.Authors.AddAsync(newAuthor);
        }
        else
        {
            existingAuthor.Posts++;
        }

        await _context.SaveChangesAsync();

        return newPost.Id;
    }

    public Task<PostPagedListDto> GetCommunityPosts(
        Guid? userId,
        Guid communityId,
        List<Guid>? tags,
        SortingOption? sorting,
        int page,
        int size
    )
    {
        return _context.GetAllAvailablePosts(_mapper, userId, communityId, tags, null, null, null, sorting, false,
            page, size);
    }
}