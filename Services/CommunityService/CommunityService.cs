using AutoMapper;
using BlogApi.Data.Repositories.AuthorRepo;
using BlogApi.Data.Repositories.CommunityRepo;
using BlogApi.Data.Repositories.PostRepo;
using BlogApi.Data.Repositories.TagRepo;
using BlogApi.Data.Repositories.UserRepo;
using BlogApi.Dtos;
using BlogApi.Exceptions;
using BlogApi.Mappers;
using BlogApi.Models;
using BlogApi.Models.Types;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services.CommunityService;

public class CommunityService : ICommunityService
{
    private readonly IMapper _mapper;
    private readonly ICommunityRepository _communityRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IPostRepository _postRepository;
    private readonly IAuthorRepository _authorRepository;

    public CommunityService(ICommunityRepository communityRepository, IUserRepository userRepository,
        ITagRepository tagRepository, IPostRepository postRepository, IAuthorRepository authorRepository,
        IMapper mapper)
    {
        _communityRepository = communityRepository;
        _userRepository = userRepository;
        _tagRepository = tagRepository;
        _postRepository = postRepository;
        _authorRepository = authorRepository;
        _mapper = mapper;
    }

    public async Task<Guid> CreateCommunity(CommunityCreateDto communityCreateDto, Guid userId)
    {
        var existingCommunity = await _communityRepository.GetCommunityByName(communityCreateDto.Name)
                                ?? throw new InvalidActionException(
                                    $"Community with name {communityCreateDto.Name} already exists.");

        var newCommunity = CommunityMapper.MapToCommunity(communityCreateDto);

        var newAdministrator = new CommunityMember
        {
            CommunityId = newCommunity.Id,
            UserId = userId,
            Role = CommunityRole.Administrator
        };

        var communityGuid = await _communityRepository.CreateCommunity(newCommunity);
        await _communityRepository.AddCommunityMember(newAdministrator);
        await _communityRepository.Save();

        return communityGuid;
    }

    public async Task<CommunityFullDto> GetCommunityInfo(Guid communityId)
    {
        var community = await _communityRepository.GetCommunityById(communityId)
                        ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

        var administrators = _communityRepository.GetCommunityAdministrators(community);
        var administratorUserDtos = new List<UserDto>();

        foreach (var administrator in administrators)
        {
            var user = await _userRepository.GetUserById(administrator.UserId) ??
                       throw new EntityNotFoundException("User not found.");

            var userDto = UserMapper.MapToUserDto(user);

            administratorUserDtos.Add(userDto);
        }

        var communityFullDto = CommunityMapper.MapToCommunityFullDto(community, administratorUserDtos);

        return communityFullDto;
    }

    public async Task<List<CommunityUserDto>> GetUserCommunities(Guid userId)
    {
        var communities = await _communityRepository.GetUserCommunities(userId);
        var communityUserDtos = communities
            .Select(community => _mapper.Map<CommunityUserDto>(community))
            .ToList();

        return communityUserDtos;
    }

    public async Task<List<CommunityDto>> GetAllCommunities()
    {
        var communities = await _communityRepository.GetAllCommunities();
        var communityDtos = communities
            .Select(community => _mapper.Map<CommunityDto>(community))
            .ToList();

        return communityDtos;
    }

    public async Task<CommunityRole?> GetUserRoleInCommunity(Guid communityId, Guid userId)
    {
        return await _communityRepository.GetUserRoleInCommunity(communityId, userId);
    }

    public async Task SubscribeToCommunity(Guid communityId, Guid userId)
    {
        var existingCommunity = await _communityRepository.GetCommunityById(communityId)
                                ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

        var existingMember = await _communityRepository.GetCommunityMember(communityId, userId)
                             ?? throw new InvalidActionException("User is already a member of this community.");

        var newSubscriber = new CommunityMember
        {
            CommunityId = communityId,
            UserId = userId,
            Role = CommunityRole.Subscriber
        };

        await _communityRepository.AddCommunityMember(newSubscriber);
        existingCommunity.SubscribersCount++;

        await _communityRepository.Save();
    }

    public async Task UnsubscribeFromCommunity(Guid communityId, Guid userId)
    {
        var existingCommunity = await _communityRepository.GetCommunityById(communityId)
                                ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

        var existingSubscriber = await _communityRepository.GetSubscriber(communityId, userId)
                                 ?? throw new InvalidActionException("User is not a subscriber of this community.");

        _communityRepository.RemoveCommunityMember(existingSubscriber);
        existingCommunity.SubscribersCount--;

        await _communityRepository.Save();
    }

    public async Task<Guid> CreatePost(PostCreateDto postCreateDto, Guid authorId, Guid communityId)
    {
        var user = await _userRepository.GetUserById(authorId)
                   ?? throw new EntityNotFoundException("User not found.");

        var community = await _communityRepository.GetCommunityById(communityId)
                        ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

        var administrator = await _communityRepository.GetAdministrator(community.Id, user.Id)
                            ?? throw new ForbiddenActionException("Access denied");

        var tags = new List<Tag>();
        if (!postCreateDto.Tags.IsNullOrEmpty())
        {
            foreach (var tagGuid in postCreateDto.Tags)
            {
                var tag = await _tagRepository.GetTagFromGuid(tagGuid)
                          ?? throw new EntityNotFoundException($"Tag with Guid={tagGuid} not found.");

                tags.Add(tag);
            }
        }

        var newPost = PostMapper.MapToCommunityPost(postCreateDto, user, community, tags);

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

    public async Task<PostPagedListDto> GetCommunityPosts(
        Guid? authorId,
        Guid communityId,
        List<Guid>? tags,
        SortingOption? sorting,
        int page,
        int size
    )
    {
        var community = await _communityRepository.GetCommunityById(communityId)
                        ?? throw new EntityNotFoundException($"Community with Guid={communityId} not found.");

        if (community.IsClosed && (authorId == null
                                   || await _communityRepository.GetCommunityMember(communityId, (Guid)authorId) ==
                                   null))
        {
            throw new ForbiddenActionException("This group is closed.");
        }

        var communityPostsQueryable = _communityRepository.GetCommunityPosts(community);

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
                if (await _tagRepository.GetTagFromGuid(guid) == null)
                    throw new EntityNotFoundException($"Tag with Guid={guid} not found.");
            }

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
            user = await _userRepository.GetUserById((Guid)authorId)
                   ?? throw new KeyNotFoundException("User not found.");
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