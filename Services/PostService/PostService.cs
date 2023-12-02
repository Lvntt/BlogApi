using BlogApi.Data.Repositories.UserRepo;
using BlogApi.Data.Repositories.AuthorRepository;
using BlogApi.Data.Repositories.CommunityRepository;
using BlogApi.Data.Repositories.PostRepository;
using BlogApi.Data.Repositories.TagRepository;
using BlogApi.Dtos;
using BlogApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services.PostService;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ICommunityRepository _communityRepository;

    public PostService(IPostRepository postRepository, ITagRepository tagRepository, IUserRepository userRepository,
        IAuthorRepository authorRepository, ICommunityRepository communityRepository)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _userRepository = userRepository;
        _authorRepository = authorRepository;
        _communityRepository = communityRepository;
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
        var postsQueryable = _postRepository.GetAllPosts();

        if (!tags.IsNullOrEmpty())
        {
            postsQueryable = _postRepository.GetPostsByTagsId(postsQueryable, tags);
        }

        if (author != null)
        {
            postsQueryable = _postRepository.GetPostsByAuthor(postsQueryable, author);
        }

        if (min != null)
        {
            postsQueryable = _postRepository.GetPostsByMinReadingTime(postsQueryable, (int)min);
        }

        if (max != null)
        {
            postsQueryable = _postRepository.GetPostsByMaxReadingTime(postsQueryable, (int)max);
        }

        if (sorting != null)
        {
            postsQueryable = _postRepository.GetSortedPosts(postsQueryable, (SortingOption)sorting);
        }

        if (onlyMyCommunities)
        {
            if (userId != null)
            {
                var communityMembers = await _communityRepository.GetUserCommunities((Guid)userId);
                postsQueryable = _postRepository.GetOnlyMyCommunitiesPosts(postsQueryable, communityMembers, (Guid)userId);
            }
        }
        
        var postsCount = postsQueryable.Count();
        var paginationCount = !postsQueryable.IsNullOrEmpty() ? (int)Math.Ceiling((double)postsCount / size) : 0;
        // TODO add page size validation (>0)
        var pagination = new PageInfoModel
        {
            Size = size,
            Count = paginationCount,
            Current = page
        };

        var posts = _postRepository.GetPagedPosts(postsQueryable, pagination);

        User? user = null;
        if (userId != null)
        {
            user = await _userRepository.GetUserById((Guid)userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
        }
        
        var postsDto = posts.Select((post, index) =>
            {
                var hasLike = false;
                if (user != null)
                {
                    hasLike = _postRepository.DidUserLikePost(post, user);
                }
                var tagDtos = post.Tags?.Select(tag =>
                    new TagDto
                    {
                        Id = tag.Id,
                        Name = tag.Name,
                        CreateTime = tag.CreateTime
                    }
                ).ToList();
                
                return new PostDto
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
                    Tags = tagDtos
                };
            }
        ).ToList();
        return new PostPagedListDto
        {
            Posts = postsDto,
            Pagination = pagination
        };
    }

    public async Task<Guid> CreatePost(PostCreateDto request, Guid authorId)
    {
        var user = await _userRepository.GetUserById(authorId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        
        // TODO replace exception with validation attribute?
        var tags = new List<Tag>();
        if (!request.Tags.IsNullOrEmpty())
        {
            foreach (var tagGuid in request.Tags)
            {
                var tag = await _tagRepository.GetTagFromGuid(tagGuid) 
                          ?? throw new KeyNotFoundException($"Tag with Guid={tagGuid} not found.");
                tags.Add(tag);
            }
        }

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
        // TODO add CommentsCount increment in Comments endpoints

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
            await _authorRepository.IncrementAuthorPosts(authorId);
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