using AutoMapper;
using BlogApi.Data;
using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Exceptions;
using BlogApi.Mappers;
using BlogApi.Models;
using BlogApi.Models.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Extensions;

public static class DataExtensions
{
    public static async Task<User> GetUserById(this BlogDbContext context, Guid id)
    {
        return await context.Users.FirstOrDefaultAsync(user => user.Id == id) ??
               throw new EntityNotFoundException("User not found.");
    }
    
    public static async Task<User> GetUserByEmail(this BlogDbContext context, string email)
    {
        return await context.Users.FirstOrDefaultAsync(user => user.Email == email) ??
               throw new EntityExistsException("User with this email already exists.");
    }
    
    public static async Task<Tag> GetTagById(this BlogDbContext context, Guid id)
    {
        return await context.Tags.FirstOrDefaultAsync(tag => tag.Id == id) ??
            throw new EntityNotFoundException($"Tag with Guid={id} not found.");
    }
    
    public static async Task<Post> GetPostById(this BlogDbContext context, Guid id)
    {
        return await context.Posts
                       .Include(post => post.Tags)
                       .Include(post => post.Comments)
                       .Include(post => post.LikedPosts)
                       .FirstOrDefaultAsync(post => post.Id == id)
                   ?? throw new EntityNotFoundException($"Post with Guid={id} not found.");
    }
    
    public static Task<Author?> GetAuthorById(this BlogDbContext context, Guid id)
    {
        return context.Authors.FirstOrDefaultAsync(author => author.UserId == id);
    }
    
    public static async Task<Community> GetCommunityById(this BlogDbContext context, Guid id)
    {
        return await context.Communities
                            .Include(community => community.Members)
                            .FirstOrDefaultAsync(community => community.Id == id)
                        ?? throw new EntityNotFoundException($"Community with Guid={id} not found.");
    }

    public static async Task<PostPagedListDto> GetAllAvailablePosts(
        this BlogDbContext context,
        IMapper mapper,
        Guid? userId,
        Guid? communityId,
        List<Guid>? tags,
        string? author,
        int? min,
        int? max,
        SortingOption? sorting,
        bool onlyMyCommunities,
        int page,
        int size
    ) {
        if (communityId != null)
        {
            var community = await context.GetCommunityById((Guid)communityId);

            if (community.IsClosed && (userId == null
                                       || await context.CommunityMembers.FirstOrDefaultAsync(cm =>
                                           cm.CommunityId == communityId
                                           && cm.UserId == userId) == null))
                throw new ForbiddenActionException("This group is closed.");
        }
        
        var postsQueryable = context.GetPostsQueryable();

        if (communityId != null)
            postsQueryable = postsQueryable.Where(post => post.CommunityId == communityId);
        
        if (!tags.IsNullOrEmpty())
        {
            foreach (var guid in tags)
                await context.GetTagById(guid);
            
            postsQueryable = postsQueryable.GetPostsByTags(tags);
        }

        if (author != null)
            postsQueryable = postsQueryable.GetPostsByAuthor(author);

        if (min != null)
            postsQueryable = postsQueryable.GetPostsByMinTime((int)min);

        if (max != null)
            postsQueryable = postsQueryable.GetPostsByMaxTime((int)max);

        if (sorting != null)
            postsQueryable = postsQueryable.GetSortedPosts((SortingOption)sorting);

        if (communityId == null && onlyMyCommunities && userId != null)
        {
            var myCommunitiesIds = context.CommunityMembers
                .Where(cm => cm.UserId == userId)
                .Select(cm => cm.CommunityId);
        
            postsQueryable = postsQueryable.Where(post => post.CommunityId != null && myCommunitiesIds.Contains((Guid)post.CommunityId));
        }

        var postsCount = postsQueryable.Count();
        var paginationCount = (int)Math.Ceiling((double)postsCount / size);

        if (page < 1 || (paginationCount != 0 && page > paginationCount))
            throw new InvalidActionException("Invalid value for attribute page.");

        var pagination = new PageInfoModel
        {
            Size = size,
            Count = paginationCount,
            Current = page
        };

        
        var posts = await postsQueryable.GetPagedPosts(pagination);

        User? user = null;
        if (userId != null)
            user = await context.GetUserById((Guid)userId);

        var postsDto = posts.Select(post =>
            {
                var hasLike = false;
                if (user != null)
                    hasLike = post.LikedPosts.Any(liked => liked.UserId == user.Id);

                var tagDtos = post.Tags
                    .Select(tag => mapper.Map<TagDto>(tag))
                    .ToList();

                return PostMapper.MapToPostDto(post, hasLike, tagDtos);
            }
        ).ToList();

        return new PostPagedListDto
        {
            Posts = postsDto,
            Pagination = pagination
        };
    }
    
    public static string GetHouseText(this AsHouse asHouse)
    {
        var text = asHouse.Housenum!;

        if (asHouse.Addtype1 != null)
        {
            text += $" {ObjectLevelDescriptionMap.HouseDescriptionFromHouseType[(int)asHouse.Addtype1]}";
            text += $" {asHouse.Addnum1}";
        }

        if (asHouse.Addtype2 != null)
        {
            text += $" {ObjectLevelDescriptionMap.HouseDescriptionFromHouseType[(int)asHouse.Addtype2]}";
            text += $" {asHouse.Addnum2}";
        }

        return text;
    }
    
    private static IQueryable<Post> GetPostsQueryable(this BlogDbContext context)
    {
        return context.Posts;
    }
    
    private static IQueryable<Post> GetPostsByTags(this IQueryable<Post> posts, List<Guid> tagGuids)
    {
        return posts.Where(post => post.Tags
                .Count(tag => tagGuids.Contains(tag.Id)) == tagGuids.Count
        );
    }
    
    private static IQueryable<Post> GetPostsByAuthor(this IQueryable<Post> posts, string authorName)
    {
        return posts.Where(post => post.Author.Contains(authorName));
    }
    
    private static IQueryable<Post> GetPostsByMinTime(this IQueryable<Post> posts, int minReadingTime)
    {
        return posts.Where(post => post.ReadingTime >= minReadingTime);
    }
    
    private static IQueryable<Post> GetPostsByMaxTime(this IQueryable<Post> posts, int maxReadingTime)
    {
        return posts.Where(post => post.ReadingTime <= maxReadingTime);
    }
    
    private static IQueryable<Post> GetSortedPosts(this IQueryable<Post> posts, SortingOption sorting)
    {
        return sorting switch
        {
            SortingOption.CreateAsc => posts.OrderBy(post => post.CreateTime),
            SortingOption.CreateDesc => posts.OrderByDescending(post => post.CreateTime),
            SortingOption.LikeAsc => posts.OrderBy(post => post.Likes),
            SortingOption.LikeDesc => posts.OrderByDescending(post => post.Likes),
            _ => throw new ArgumentOutOfRangeException(nameof(sorting), sorting, null)
        };
    }
    
    private static Task<List<Post>> GetPagedPosts(this IQueryable<Post> posts, PageInfoModel pagination)
    {
        return posts
            .Skip((pagination.Current - 1) * pagination.Size)
            .Take(pagination.Size)
            .Include(post => post.Tags)
            .Include(post => post.LikedPosts)
            .ToListAsync();
    }
    
}