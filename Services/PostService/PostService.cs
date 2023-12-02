using AutoMapper;
using BlogApi.Data.Repositories.UserRepo;
using BlogApi.Data.Repositories.AuthorRepository;
using BlogApi.Data.Repositories.CommunityRepository;
using BlogApi.Data.Repositories.PostRepository;
using BlogApi.Data.Repositories.TagRepo;
using BlogApi.Dtos;
using BlogApi.Mappers;
using BlogApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services.PostService;

public class PostService : IPostService
{
    private readonly IMapper _mapper;
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ICommunityRepository _communityRepository;

    public PostService(IPostRepository postRepository, ITagRepository tagRepository, IUserRepository userRepository,
        IAuthorRepository authorRepository, ICommunityRepository communityRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _userRepository = userRepository;
        _authorRepository = authorRepository;
        _communityRepository = communityRepository;
        _mapper = mapper;
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
            foreach (var guid in tags)
            {
                if (await _tagRepository.GetTagFromGuid(guid) == null)
                {
                    throw new KeyNotFoundException($"Tag with Guid={guid} not found.");
                }
            }

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

        if (onlyMyCommunities && userId != null)
        {
            var communityMembers = await _communityRepository.GetUserCommunities((Guid)userId);
            postsQueryable = _postRepository.GetOnlyMyCommunitiesPosts(postsQueryable, communityMembers, (Guid)userId);
        }

        var postsCount = postsQueryable.Count();
        var paginationCount = !postsQueryable.IsNullOrEmpty() ? (int)Math.Ceiling((double)postsCount / size) : 0;

        if (page < 1 || (paginationCount != 0 && page > paginationCount))
        {
            throw new InvalidOperationException("Invalid value for attribute page.");
        }

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

        var postsDto = posts.Select(post =>
            {
                var hasLike = false;
                if (user != null)
                {
                    hasLike = _postRepository.DidUserLikePost(post, user);
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
        var user = await _userRepository.GetUserById(authorId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var tags = new List<Tag>();
        if (!postCreateDto.Tags.IsNullOrEmpty())
        {
            foreach (var tagGuid in postCreateDto.Tags)
            {
                var tag = await _tagRepository.GetTagFromGuid(tagGuid);
                if (tag == null)
                {
                    throw new KeyNotFoundException($"Tag with Guid={tagGuid} not found.");
                }

                tags.Add(tag);
            }
        }

        var newPost = PostMapper.MapToPost(postCreateDto, user, tags);
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
            existingAuthor.Posts++;
        }

        await _postRepository.Save();
        await _authorRepository.Save();

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
        
        var author = await _authorRepository.GetAuthorById(post.AuthorId);
        if (author == null)
        {
            throw new KeyNotFoundException("Author not found.");
        }

        if (_postRepository.DidUserLikePost(post, user))
        {
            throw new InvalidOperationException("User has already liked this post.");
        }

        post.LikedPosts.Add(
            new Like { PostId = post.Id, UserId = user.Id }
        );
        post.Likes++;
        author.Likes++;
        
        await _postRepository.Save();
        await _authorRepository.Save();
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
        
        var author = await _authorRepository.GetAuthorById(post.AuthorId);
        if (author == null)
        {
            throw new KeyNotFoundException("Author not found.");
        }

        post.LikedPosts.Remove(existingLike);
        post.Likes--;
        author.Likes--;
        
        await _postRepository.Save();
        await _authorRepository.Save();
    }
}