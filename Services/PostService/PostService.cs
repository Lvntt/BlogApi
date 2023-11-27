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

    public PostService(IPostRepository postRepository, ITagRepository tagRepository, IUserRepository userRepository, IAuthorRepository authorRepository)
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
                                                   ?? throw new KeyNotFoundException($"Tag with Guid={tagGuid} not found.")
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
            HasLike = false,
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
}