using BlogApi.Data.Repositories;
using BlogApi.Data.Repositories.AuthorRepository;
using BlogApi.Data.Repositories.PostRepository;
using BlogApi.Data.Repositories.TagRepository;
using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.PostService;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthorRepository _authorRepository;

    public PostService(IPostRepository postRepository, ITagRepository tagRepository, IUserRepository userRepository,
        IAuthorRepository authorRepository)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _userRepository = userRepository;
        _authorRepository = authorRepository;
    }

    public async Task<Guid> CreatePost(PostCreateDto request, Guid authorId)
    {
        var user = await _userRepository.GetUserById(authorId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // TODO get tags async?
        // TODO replace exception with validation attribute?
        var tags = request.Tags?.Select(tagGuid => _tagRepository.GetTagFromGuid(tagGuid)
                                                   ?? throw new KeyNotFoundException(
                                                       $"Tag with Guid={tagGuid} not found.")
        ).ToList();

        var newPost = new Post
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            Title = request.Title,
            Description = request.Description,
            ReadingTime = request.ReadingTime,
            Image = request.Image,
            AuthorId = user.Id,
            Author = user.FullName,
            CommunityId = null,
            CommunityName = null,
            AddressId = request.AddressId,
            Likes = 0,
            CommentsCount = 0,
            Tags = tags
        };

        var postId = await _postRepository.AddPost(newPost);
        var existingAuthor = await _authorRepository.GetAuthorById(authorId);

        if (existingAuthor == null)
        {
            var newAuthor = new Author
            {
                UserId = authorId,
                Likes = 0,
                Posts = 1
            };
            await _authorRepository.AddAuthor(newAuthor);
        }
        else
        {
            await _authorRepository.IncreaseAuthorPosts(authorId);
        }

        return postId;
    }

    public async Task<PostFullDto> GetPostInfo(Guid postId, Guid? userId)
    {
        var post = await _postRepository.GetPost(postId);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with Guid={postId} not found.");
        }

        var hasLike = false;
        if (userId != null)
        {
            var user = await _userRepository.GetUserById((Guid)userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            hasLike = _postRepository.DidUserLikePost(post, user);
        }

        var tags = post.Tags?.Select(tag =>
            new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                CreateTime = tag.CreateTime
            }
        ).ToList();

        var comments = post.Comments.Select(comment =>
            new CommentDto
            {
                Id = comment.Id,
                CreateTime = comment.CreateTime,
                Content = comment.Content,
                ModifiedDate = comment.ModifiedDate,
                DeleteDate = comment.DeleteDate,
                AuthorId = comment.AuthorId,
                Author = comment.Author,
                SubComments = comment.SubComments
            }
        ).ToList();
        
        var postFullDto = new PostFullDto
        {
            Id = post.Id,
            CreateTime = post.CreateTime,
            Title = post.Title,
            Description = post.Description,
            ReadingTime = post.ReadingTime,
            Image = post.Image,
            AuthorId = post.AuthorId,
            Author = post.Author,
            CommunityId = post.CommunityId,
            CommunityName = post.CommunityName,
            AddressId = post.AddressId,
            Likes = post.Likes,
            HasLike = hasLike,
            CommentsCount = post.CommentsCount,
            Tags = tags,
            Comments = comments
        };
        
        return postFullDto;
    }

    public async Task AddLikeToPost(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetPost(postId);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with Guid={postId} not found.");
        }
        
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (_postRepository.DidUserLikePost(post, user))
        {
            throw new InvalidOperationException("User has already liked this post.");
        }

        await _postRepository.AddLikeToPost(post, user);
    }

    public async Task RemoveLikeFromPost(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetPost(postId);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with Guid={postId} not found.");
        }
        
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var existingLike = await _postRepository.GetExistingLike(post, user);
        if (existingLike == null)
        {
            throw new InvalidOperationException("User has not liked this post.");
        }

        await _postRepository.RemoveLikeFromPost(post, existingLike);
    }
}