using AutoMapper;
using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Exceptions;
using BlogApi.Mappers;
using BlogApi.Models;
using BlogApi.Models.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services.PostService;

public class PostService : IPostService
{
    private readonly BlogDbContext _context;
    private readonly IMapper _mapper;

    public PostService(IMapper mapper, BlogDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<PostPagedListDto> GetAllAvailablePosts(
        Guid? userId,
        List<Guid>? tags,
        string? author,
        int? min,
        int? max,
        SortingOption? sorting,
        bool onlyMyCommunities,
        int page,
        int size
    )
    {
        var postsQueryable = _context.Posts
            .Include(post => post.Tags)
            .Include(post => post.LikedPosts)
            .AsQueryable();

        if (!tags.IsNullOrEmpty())
        {
            foreach (var guid in tags)
            {
                if (await _context.Tags.FirstOrDefaultAsync(tag => tag.Id == guid) == null)
                    throw new EntityNotFoundException($"Tag with Guid={guid} not found.");
            }

            // TODO optimize
            postsQueryable = postsQueryable
                .ToList()
                .Where(post => post.Tags
                    .Select(tag => tag.Id)
                    .Intersect(tags).Count() == tags.Count)
                .AsQueryable();
        }

        if (author != null)
        {
            postsQueryable = postsQueryable.Where(post => post.Author.Contains(author));
        }

        if (min != null)
        {
            postsQueryable = postsQueryable.Where(post => post.ReadingTime >= (int)min);
        }

        if (max != null)
        {
            postsQueryable = postsQueryable.Where(post => post.ReadingTime <= (int)max);
        }

        if (sorting != null)
        {
            postsQueryable = (SortingOption)sorting switch
            {
                SortingOption.CreateAsc => postsQueryable.OrderBy(post => post.CreateTime),
                SortingOption.CreateDesc => postsQueryable.OrderByDescending(post => post.CreateTime),
                SortingOption.LikeAsc => postsQueryable.OrderBy(post => post.Likes),
                SortingOption.LikeDesc => postsQueryable.OrderByDescending(post => post.Likes),
                _ => throw new ArgumentOutOfRangeException(nameof(sorting), sorting, null)
            };
        }

        if (onlyMyCommunities && userId != null)
        {
            var communityMembers = await _context.CommunityMembers
                .Where(cm => cm.UserId == userId)
                .ToListAsync();
            
            var communityIds = communityMembers
                .Where(cm => cm.UserId == userId)
                .Select(cm => cm.CommunityId)
                .ToList();
        
            postsQueryable = postsQueryable.Where(post => post.CommunityId != null && communityIds.Contains((Guid)post.CommunityId));
        }

        var postsCount = postsQueryable.Count();
        var paginationCount = !postsQueryable.IsNullOrEmpty() ? (int)Math.Ceiling((double)postsCount / size) : 0;

        if (page < 1 || (paginationCount != 0 && page > paginationCount))
            throw new InvalidActionException("Invalid value for attribute page.");

        var pagination = new PageInfoModel
        {
            Size = size,
            Count = paginationCount,
            Current = page
        };

        var posts = postsQueryable
            .Skip((pagination.Current - 1) * pagination.Size)
            .Take(pagination.Size)
            .ToList();

        User? user = null;
        if (userId != null)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new EntityNotFoundException("User not found.");
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

                return PostMapper.MapToPostDto(post, hasLike, tagDtos);
            }
        ).ToList();

        return new PostPagedListDto
        {
            Posts = postsDto,
            Pagination = pagination
        };
    }

    public async Task<Guid> CreatePost(PostCreateDto postCreateDto, Guid authorId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == authorId)
            ?? throw new EntityNotFoundException("User not found.");

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

        var newPost = PostMapper.MapToPost(postCreateDto, user, tags);

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

    public async Task<PostFullDto> GetPostInfo(Guid postId, Guid? userId)
    {
        var post = await _context.Posts
            .Include(post => post.Tags)
            .Include(post => post.Comments)
            .Include(post => post.LikedPosts)
            .FirstOrDefaultAsync(post => post.Id == postId);
        if (post == null)
            throw new EntityNotFoundException($"Post with Guid={postId} not found.");

        var hasLike = false;
        if (userId != null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId)
                       ?? throw new EntityNotFoundException("User not found.");

            hasLike = post.LikedPosts.Any(liked => liked.UserId == user.Id);
        }

        var tagDtos = post.Tags
            .Select(tag => _mapper.Map<TagDto>(tag))
            .ToList();

        var comments = post.Comments
            .Select(comment => _mapper.Map<CommentDto>(comment))
            .ToList();

        var postFullDto = PostMapper.MapToPostFullDto(post, hasLike, tagDtos, comments);

        return postFullDto;
    }

    public async Task AddLikeToPost(Guid postId, Guid userId)
    {
        var post = await _context.Posts
                .Include(post => post.Tags)
                .Include(post => post.Comments)
                .Include(post => post.LikedPosts)
                .FirstOrDefaultAsync(post => post.Id == postId)
                   ?? throw new EntityNotFoundException($"Post with Guid={postId} not found.");


        var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId)
                   ?? throw new EntityNotFoundException("User not found.");

        var author = await _context.Authors.FirstOrDefaultAsync(author => author.UserId == post.AuthorId)
                     ?? throw new EntityNotFoundException("Author not found.");

        if (post.LikedPosts.Any(liked => liked.UserId == user.Id))
            throw new InvalidActionException("User has already liked this post.");

        post.LikedPosts.Add(
            new Like { PostId = post.Id, UserId = user.Id }
        );
        post.Likes++;
        author.Likes++;

        await _context.SaveChangesAsync();
    }

    public async Task RemoveLikeFromPost(Guid postId, Guid userId)
    {
        var post = await _context.Posts
                .Include(post => post.Tags)
                .Include(post => post.Comments)
                .Include(post => post.LikedPosts)
                .FirstOrDefaultAsync(post => post.Id == postId)
                   ?? throw new EntityNotFoundException($"Post with Guid={postId} not found.");

        var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId)
                   ?? throw new EntityNotFoundException("User not found.");

        var existingLike = await _context.Likes
                .FirstOrDefaultAsync(like => 
                    like.PostId == post.Id 
                    && like.UserId == user.Id)
                           ?? throw new InvalidActionException("User has not liked this post.");

        var author = await _context.Authors.FirstOrDefaultAsync(author => author.UserId == post.AuthorId)
                     ?? throw new EntityNotFoundException("Author not found.");

        post.LikedPosts.Remove(existingLike);
        post.Likes--;
        author.Likes--;

        await _context.SaveChangesAsync();
    }
}