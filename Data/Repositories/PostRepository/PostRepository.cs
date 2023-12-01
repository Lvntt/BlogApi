using BlogApi.Dtos;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.PostRepository;

public class PostRepository : IPostRepository
{
    private readonly BlogDbContext _context;

    public PostRepository(BlogDbContext context)
    {
        _context = context;
    }

    public IQueryable<Post> GetAllPosts()
    {
        return _context.Posts
            .Include(post => post.Tags)
            .Include(post => post.LikedPosts);
    }

    public IQueryable<Post> GetPostsByTagsId(IQueryable<Post> posts, List<Guid> tagsId)
    {
        // TODO not working without ToList()?
        return posts.ToList().Where(post =>
            post.Tags != null && post.Tags.Select(tag => tag.Id)
                .Intersect(tagsId).Count() == tagsId.Count).AsQueryable();
        // return posts.Where(post => post.Tags != null && post.Tags.Intersect(tags).Count() == tags.Count);
        // return posts.Where(post => post.Tags != null && post.Tags.Count(tag => tags.Contains(tag)) == tags.Count);
    }

    public IQueryable<Post> GetPostsByAuthor(IQueryable<Post> posts, string query)
    {
        return posts.Where(post => post.Author.Contains(query));
    }

    public IQueryable<Post> GetPostsByMinReadingTime(IQueryable<Post> posts, int minTime)
    {
        return posts.Where(post => post.ReadingTime >= minTime);
    }

    public IQueryable<Post> GetPostsByMaxReadingTime(IQueryable<Post> posts, int maxTime)
    {
        return posts.Where(post => post.ReadingTime <= maxTime);
    }

    public IQueryable<Post> GetSortedPosts(IQueryable<Post> posts, SortingOption sortingOption)
    {
        return sortingOption switch
        {
            SortingOption.CreateAsc => posts.OrderBy(post => post.CreateTime),
            SortingOption.CreateDesc => posts.OrderByDescending(post => post.CreateTime),
            SortingOption.LikeAsc => posts.OrderBy(post => post.Likes),
            SortingOption.LikeDesc => posts.OrderByDescending(post => post.Likes),
            _ => throw new ArgumentOutOfRangeException(nameof(sortingOption), sortingOption, null)
        };
    }
    
    public IQueryable<Post> GetOnlyMyCommunitiesPosts(IQueryable<Post> posts, List<CommunityMember> communityMembers, Guid userId)
    {
        var communityIds = communityMembers
            .Where(cm => cm.UserId == userId)
            .Select(cm => cm.CommunityId)
            .ToList();
        
            return posts.Where(post => post.CommunityId != null && communityIds.Contains((Guid)post.CommunityId));
    }

    public List<Post> GetPagedPosts(IQueryable<Post> posts, PageInfoModel pagination)
    {
        return posts.Skip((pagination.Current - 1) * pagination.Size).Take(pagination.Size).ToList();
    }

    public async Task<Guid> AddPost(Post post)
    {
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
        return post.Id;
    }

    public async Task<Post?> GetPost(Guid id)
    {
        return await _context.Posts
            .Include(post => post.Tags)
            .Include(post => post.Comments)
            .Include(post => post.LikedPosts)
            .FirstOrDefaultAsync(post => post.Id == id);
    }

    public bool DidUserLikePost(Post post, User user)
    {
        return post.LikedPosts.Any(liked => liked.UserId == user.Id);
    }

    public async Task<Like?> GetExistingLike(Post post, User user)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(like => like.PostId == post.Id && like.UserId == user.Id);
    }

    public async Task AddLikeToPost(Post post, User user)
    {
        post.LikedPosts.Add(new Like { PostId = post.Id, UserId = user.Id });
        post.Likes++;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveLikeFromPost(Post post, Like like)
    {
        post.LikedPosts.Remove(like);
        post.Likes--;
        await _context.SaveChangesAsync();
    }
}