using BlogApi.Data.Repositories.AuthorRepository;
using BlogApi.Data.Repositories.CommunityRepository;
using BlogApi.Data.Repositories.PostRepository;
using BlogApi.Data.Repositories.TagRepository;
using BlogApi.Data.Repositories.UserRepo;
using BlogApi.Dtos;
using BlogApi.Models;
using BlogApi.Models.Types;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services.CommunityService;

public class CommunityService : ICommunityService
{
    private readonly ICommunityRepository _communityRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IPostRepository _postRepository;
    private readonly IAuthorRepository _authorRepository;

    public CommunityService(ICommunityRepository communityRepository, IUserRepository userRepository,
        ITagRepository tagRepository, IPostRepository postRepository, IAuthorRepository authorRepository)
    {
        _communityRepository = communityRepository;
        _userRepository = userRepository;
        _tagRepository = tagRepository;
        _postRepository = postRepository;
        _authorRepository = authorRepository;
    }

    public async Task<Guid> CreateCommunity(CommunityCreateDto request, Guid userId)
    {
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var existingCommunity = await _communityRepository.GetCommunityByName(request.Name);
        if (existingCommunity != null)
        {
            throw new InvalidOperationException($"Community with name {request.Name} already exists.");
        }

        var newCommunity = new Community
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            Name = request.Name,
            Description = request.Description,
            IsClosed = request.IsClosed,
            SubscribersCount = 0
        };

        var newAdministrator = new CommunityMember
        {
            CommunityId = newCommunity.Id,
            UserId = userId,
            Role = CommunityRole.Administrator
        };

        var communityGuid = await _communityRepository.CreateCommunity(newCommunity);
        await _communityRepository.AddCommunityMember(newAdministrator);

        return communityGuid;
    }

    public async Task<CommunityFullDto> GetCommunityInfo(Guid communityId)
    {
        var community = await _communityRepository.GetCommunityById(communityId);
        if (community == null)
        {
            throw new KeyNotFoundException($"Community with Guid={communityId} not found.");
        }

        var administrators = _communityRepository.GetCommunityAdministrators(community);
        var administratorUserDtos = new List<UserDto>();
        
        foreach (var administrator in administrators)
        {
            var user = await _userRepository.GetUserById(administrator.UserId) ?? throw new KeyNotFoundException("User not found.");
    
            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                CreateTime = user.CreateTime
            };

            administratorUserDtos.Add(userDto);
        }

        var communityFullDto = new CommunityFullDto
        {
            Id = community.Id,
            CreateTime = community.CreateTime,
            Name = community.Name,
            Description = community.Description,
            IsClosed = community.IsClosed,
            SubscribersCount = community.SubscribersCount,
            Administrators = administratorUserDtos
        };

        return communityFullDto;
    }

    public async Task<List<CommunityUserDto>> GetUserCommunities(Guid userId)
    {
        var communities = await _communityRepository.GetUserCommunities(userId);
        var communityUserDtos = communities.Select(community => new CommunityUserDto
            {
                UserId = community.UserId,
                CommunityId = community.CommunityId,
                Role = community.Role
            }
        ).ToList();

        return communityUserDtos;
    }

    public async Task<List<CommunityDto>> GetAllCommunities()
    {
        var communities = await _communityRepository.GetAllCommunities();
        var communityDtos = communities.Select(community => new CommunityDto
            {
                Id = community.Id,
                CreateTime = community.CreateTime,
                Name = community.Name,
                Description = community.Description,
                IsClosed = community.IsClosed,
                SubscribersCount = community.SubscribersCount
            }
        ).ToList();

        return communityDtos;
    }

    public async Task<CommunityRole?> GetUserRoleInCommunity(Guid communityId, Guid userId)
    {
        return await _communityRepository.GetUserRoleInCommunity(communityId, userId);
    }

    public async Task SubscribeToCommunity(Guid communityId, Guid userId)
    {
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var existingCommunity = await _communityRepository.GetCommunityById(communityId);
        if (existingCommunity == null)
        {
            throw new KeyNotFoundException($"Community with Guid={communityId} not found.");
        }

        var existingMember = _communityRepository.GetCommunityMember(communityId, userId);
        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already a member of this community.");
        }

        var newSubscriber = new CommunityMember
        {
            CommunityId = communityId,
            UserId = userId,
            Role = CommunityRole.Subscriber
        };

        await _communityRepository.AddCommunityMember(newSubscriber);
        await _communityRepository.IncrementCommunitySubscribers(existingCommunity);
    }

    public async Task UnsubscribeFromCommunity(Guid communityId, Guid userId)
    {
        var user = _userRepository.GetUserById(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var existingCommunity = await _communityRepository.GetCommunityById(communityId);
        if (existingCommunity == null)
        {
            throw new KeyNotFoundException($"Community with Guid={communityId} not found.");
        }

        var existingSubscriber = _communityRepository.GetSubscriber(communityId, userId);
        if (existingSubscriber == null)
        {
            throw new InvalidOperationException("User is not a subscriber of this community.");
        }

        await _communityRepository.RemoveCommunityMember(existingSubscriber);
        await _communityRepository.DecrementCommunitySubscribers(existingCommunity);
    }

    public async Task<Guid> CreatePost(PostCreateDto request, Guid authorId, Guid communityId)
    {
        var user = await _userRepository.GetUserById(authorId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var community = await _communityRepository.GetCommunityById(communityId);
        if (community == null)
        {
            throw new KeyNotFoundException($"Community with Guid={communityId} not found.");
        }

        var administrator = _communityRepository.GetAdministrator(community.Id, user.Id);
        if (administrator == null)
        {
            throw new MemberAccessException("Access denied");
        }

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
            CommunityId = community.Id,
            CommunityName = community.Name,
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
            await _authorRepository.IncrementAuthorPosts(authorId);
        }

        return postId;
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
        var community = await _communityRepository.GetCommunityById(communityId);
        if (community == null)
        {
            throw new KeyNotFoundException($"Community with Guid={communityId} not found.");
        }

        if (community.IsClosed && (authorId == null
                                   || _communityRepository.GetCommunityMember(communityId, (Guid)authorId) == null))
        {
            throw new MemberAccessException("This group is closed.");
        }
        
        var communityPostsQueryable = _communityRepository.GetCommunityPosts(community);
        var postsCount = communityPostsQueryable.Count();
        var paginationCount = !communityPostsQueryable.IsNullOrEmpty() ? (int)Math.Ceiling((double)postsCount / size) : 0;
        // TODO add page size validation (>0)
        var pagination = new PageInfoModel
        {
            Size = size,
            Count = paginationCount,
            Current = page
        };
        
        if (!tags.IsNullOrEmpty())
        {
            communityPostsQueryable = _postRepository.GetPostsByTagsId(communityPostsQueryable, tags);
        }
        
        if (sorting != null)
        {
            communityPostsQueryable = _postRepository.GetSortedPosts(communityPostsQueryable, (SortingOption)sorting);
        }
        
        var posts = _postRepository.GetPagedPosts(communityPostsQueryable, pagination);

        User? user = null;
        if (authorId != null)
        {
            user = await _userRepository.GetUserById((Guid)authorId);
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
}