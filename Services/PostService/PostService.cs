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

    public Task<PostPagedListDto> GetAllAvailablePosts(
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
        return _context.GetAllAvailablePosts(_mapper, userId, null, tags, author, min, max, sorting, onlyMyCommunities,
            page, size);
    }

    public async Task<Guid> CreatePost(PostCreateDto postCreateDto, Guid authorId)
    {
        var user = await _context.GetUserById(authorId);

        var tags = new List<Tag>();
        if (!postCreateDto.Tags.IsNullOrEmpty())
        {
            foreach (var tagGuid in postCreateDto.Tags)
            {
                var tag = await _context.GetTagById(tagGuid);
                tags.Add(tag);
            }
        }

        var newPost = PostMapper.MapToPost(postCreateDto, user, tags);

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

    public async Task<PostFullDto> GetPostInfo(Guid postId, Guid? userId)
    {
        var post = await _context.GetPostById(postId);

        var hasLike = false;
        if (userId != null)
        {
            var user = await _context.GetUserById((Guid)userId);
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
        var post = await _context.GetPostById(postId);
        var user = await _context.GetUserById(userId);

        var author = await _context.GetAuthorById(post.AuthorId)
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
        var post = await _context.GetPostById(postId);
        var user = await _context.GetUserById(userId);

        var existingLike = await _context.Likes
                               .FirstOrDefaultAsync(like =>
                                   like.PostId == post.Id
                                   && like.UserId == user.Id)
                           ?? throw new InvalidActionException("User has not liked this post.");

        var author = await _context.GetAuthorById(post.AuthorId)
                     ?? throw new EntityNotFoundException("Author not found.");

        post.LikedPosts.Remove(existingLike);
        post.Likes--;
        author.Likes--;

        await _context.SaveChangesAsync();
    }
}