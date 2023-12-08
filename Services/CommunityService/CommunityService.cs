using AutoMapper;
using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Exceptions;
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
        var community = await _context.Communities
                .Include(community => community.Members)
                .FirstOrDefaultAsync(community => community.Id == communityId)
                        ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

        var administrators = community.Members
            .Where(member => member.Role == CommunityRole.Administrator)
            .ToList();;
        var administratorUserDtos = new List<UserDto>();

        foreach (var administrator in administrators)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == administrator.UserId)
                       ?? throw new EntityNotFoundException("User not found.");

            var userDto = UserMapper.MapToUserDto(user);

            administratorUserDtos.Add(userDto);
        }

        var communityFullDto = CommunityMapper.MapToCommunityFullDto(community, administratorUserDtos);

        return communityFullDto;
    }

    public async Task<List<CommunityUserDto>> GetUserCommunities(Guid userId)
    {
        var communities = await _context.CommunityMembers
            .Where(cm => cm.UserId == userId)
            .ToListAsync();;
        var communityUserDtos = communities
            .Select(community => _mapper.Map<CommunityUserDto>(community))
            .ToList();

        return communityUserDtos;
    }

    public async Task<List<CommunityDto>> GetAllCommunities()
    {
        var communities = await _context.Communities.ToListAsync();
        var communityDtos = communities
            .Select(community => _mapper.Map<CommunityDto>(community))
            .ToList();

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
        var existingCommunity = await _context.Communities
                .Include(community => community.Members)
                .FirstOrDefaultAsync(community => community.Id == communityId)
                                ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

        var existingMember = await _context.CommunityMembers.FirstOrDefaultAsync(cm =>
                cm.CommunityId == communityId 
                && cm.UserId == userId)
                             ?? throw new InvalidActionException("User is already a member of this community.");

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
        var existingCommunity = await _context.Communities
                .Include(community => community.Members)
                .FirstOrDefaultAsync(community => community.Id == communityId)
                                ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

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
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == authorId)
                   ?? throw new EntityNotFoundException("User not found.");

        var community = await _context.Communities
                .Include(community => community.Members)
                .FirstOrDefaultAsync(community => community.Id == communityId)
                        ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

        var administrator = await _context.CommunityMembers.FirstOrDefaultAsync(cm =>
                cm.CommunityId == community.Id 
                && cm.UserId == user.Id 
                && cm.Role == CommunityRole.Administrator)
                            ?? throw new ForbiddenActionException("Access denied");

        var tags = new List<Tag>();
        if (!postCreateDto.Tags.IsNullOrEmpty())
        {
            foreach (var tagGuid in postCreateDto.Tags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(tag => tag.Id == tagGuid)
                          ?? throw new EntityNotFoundException($"Tag with Guid={tagGuid} not found.");

                tags.Add(tag);
            }
        }

        var newPost = PostMapper.MapToCommunityPost(postCreateDto, user, community, tags);

        await _context.Posts.AddAsync(newPost);
        var existingAuthor = await _context.Authors.FirstOrDefaultAsync(author => author.UserId == authorId);

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

    public async Task<PostPagedListDto> GetCommunityPosts(
        Guid? authorId,
        Guid communityId,
        List<Guid>? tags,
        SortingOption? sorting,
        int page,
        int size
    )
    {
        var community = await _context.Communities
                .Include(community => community.Members)
                .FirstOrDefaultAsync(community => community.Id == communityId)
                        ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

        if (community.IsClosed && (authorId == null
                                   || await _context.CommunityMembers.FirstOrDefaultAsync(cm =>
                                       cm.CommunityId == communityId 
                                       && cm.UserId == authorId) == null))
        {
            throw new ForbiddenActionException("This group is closed.");
        }

        var communityPostsQueryable = _context.Posts
            .Where(post => post.CommunityId == community.Id)
            .Include(post => post.Tags)
            .Include(post => post.LikedPosts)
            .AsQueryable();

        var postsCount = communityPostsQueryable.Count();
        var paginationCount =
            !communityPostsQueryable.IsNullOrEmpty() ? (int)Math.Ceiling((double)postsCount / size) : 0;

        if (page < 1 || (paginationCount != 0 && page > paginationCount))
        {
            throw new InvalidActionException("Invalid value for attribute page.");
        }

        var pagination = new PageInfoModel
        {
            Size = size,
            Count = paginationCount,
            Current = page
        };

        if (!tags.IsNullOrEmpty())
        {
            foreach (var guid in tags)
            {
                if (await _context.Tags.FirstOrDefaultAsync(tag => tag.Id == guid) == null)
                    throw new EntityNotFoundException($"Tag with Guid={guid} not found.");
            }
            
            communityPostsQueryable = communityPostsQueryable
                .ToList()
                .Where(post => post.Tags
                    .Select(tag => tag.Id)
                    .Intersect(tags).Count() == tags.Count)
                .AsQueryable();
        }

        if (sorting != null)
        {
            communityPostsQueryable = (SortingOption)sorting switch
            {
                SortingOption.CreateAsc => communityPostsQueryable.OrderBy(post => post.CreateTime),
                SortingOption.CreateDesc => communityPostsQueryable.OrderByDescending(post => post.CreateTime),
                SortingOption.LikeAsc => communityPostsQueryable.OrderBy(post => post.Likes),
                SortingOption.LikeDesc => communityPostsQueryable.OrderByDescending(post => post.Likes),
                _ => throw new ArgumentOutOfRangeException(nameof(sorting), sorting, null)
            };
        }

        var posts = communityPostsQueryable
            .Skip((pagination.Current - 1) * pagination.Size)
            .Take(pagination.Size)
            .ToList();

        User? user = null;
        if (authorId != null)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == authorId)
                   ?? throw new KeyNotFoundException("User not found.");
        }

        var postsDto = posts.Select(post =>
            {
                var hasLike = false;
                if (user != null)
                {
                    hasLike = post.LikedPosts.Any(liked => liked.UserId == user.Id);
                }

                var tagDtos = post.Tags
                    .Select(tag => _mapper.Map<TagDto>(tag))
                    .ToList();

                var postDto = PostMapper.MapToPostDto(post, hasLike, tagDtos);
                return postDto;
            }
        ).ToList();

        return new PostPagedListDto
        {
            Posts = postsDto,
            Pagination = pagination
        };
    }
}