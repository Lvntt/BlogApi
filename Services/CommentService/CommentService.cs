using AutoMapper;
using BlogApi.Data.Repositories.CommentRepo;
using BlogApi.Data.Repositories.CommunityRepo;
using BlogApi.Data.Repositories.PostRepo;
using BlogApi.Data.Repositories.UserRepo;
using BlogApi.Dtos;
using BlogApi.Mappers;

namespace BlogApi.Services.CommentService;

public class CommentService : ICommentService
{
    private readonly IMapper _mapper;
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICommunityRepository _communityRepository;

    public CommentService(ICommentRepository commentRepository, IPostRepository postRepository, IUserRepository userRepository, IMapper mapper, ICommunityRepository communityRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _communityRepository = communityRepository;
    }
    
    public async Task<List<CommentDto>> GetCommentTree(Guid commentId)
    {
        var parentComment = await _commentRepository.GetCommentById(commentId);
        if (parentComment == null)
        {
            throw new KeyNotFoundException($"Comment with Guid={commentId} not found.");
        }
        
        if (parentComment.ParentCommentId != null)
        {
            throw new InvalidOperationException($"Comment with Guid={commentId} is not a root comment.");
        }

        var commentTree = await _commentRepository.GetCommentTree(parentComment);
        return commentTree
            .OrderBy(comment => comment.CreateTime)
            .Select(comment => _mapper.Map<CommentDto>(comment))
            .ToList();
    }

    public async Task AddComment(Guid postId, Guid authorId, CreateCommentDto createCommentDto)
    {
        var post = await _postRepository.GetPost(postId);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with Guid={postId} not found.");
        }

        var author = await _userRepository.GetUserById(authorId);
        if (author == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        Guid? topLevelCommentId = null;
        
        if (createCommentDto.ParentId != null)
        {
            var parentComment = await _commentRepository.GetCommentById((Guid)createCommentDto.ParentId);
            if (parentComment == null || parentComment.PostId != postId)
            {
                throw new KeyNotFoundException($"Comment with Guid={createCommentDto.ParentId} not found.");
            }

            topLevelCommentId = parentComment.TopLevelCommentId ?? parentComment.Id;
            parentComment.SubComments++;
        }
        
        var newComment = CommentMapper.MapToComment(postId, topLevelCommentId, createCommentDto, author);

        post.CommentsCount++;
        await _commentRepository.AddComment(newComment);
        await _commentRepository.Save();
    }

    public async Task EditComment(Guid commentId, Guid authorId, UpdateCommentDto updateCommentDto)
    {
        var author = await _userRepository.GetUserById(authorId);
        if (author == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        
        var comment = await _commentRepository.GetCommentById(commentId);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with Guid={commentId} not found.");
        }

        var post = await _postRepository.GetPost(comment.PostId);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with Guid={comment.PostId} not found.");
        }

        if (post.CommunityId != null)
        {
            var community = await _communityRepository.GetCommunityById((Guid)post.CommunityId);
            if (community == null)
            {
                throw new KeyNotFoundException($"Community with Guid={post.CommunityId} not found.");
            }

            if (community.IsClosed && await _communityRepository.GetCommunityMember(community.Id, author.Id) == null)
            {
                throw new MemberAccessException($"This user can't interact with closed group with Guid={community.Id}");
            }
        }

        if (comment.AuthorId != author.Id)
        {
            throw new MemberAccessException("Users can only interact with their own comments.");
        }

        comment.Content = updateCommentDto.Content;
        comment.ModifiedDate = DateTime.UtcNow;
        await _commentRepository.Save();
    }

    public async Task DeleteComment(Guid commentId, Guid authorId)
    {
        var author = await _userRepository.GetUserById(authorId);
        if (author == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        
        var comment = await _commentRepository.GetCommentById(commentId);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with Guid={commentId} not found.");
        }

        var post = await _postRepository.GetPost(comment.PostId);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with Guid={comment.PostId} not found.");
        }
        
        if (post.CommunityId != null)
        {
            var community = await _communityRepository.GetCommunityById((Guid)post.CommunityId);
            if (community == null)
            {
                throw new KeyNotFoundException($"Community with Guid={post.CommunityId} not found.");
            }

            if (community.IsClosed && await _communityRepository.GetCommunityMember(community.Id, author.Id) == null)
            {
                throw new MemberAccessException($"This user can't interact with closed group with Guid={community.Id}");
            }
        }
        
        if (comment.AuthorId != author.Id)
        {
            throw new MemberAccessException("Users can only interact with their own comments.");
        }

        if (comment.DeleteDate != null)
        {
            throw new InvalidOperationException("This comment is already deleted.");
        }

        if (comment.SubComments == 0)
        {
            _commentRepository.DeleteComment(comment);

            if (comment.ParentCommentId != null)
            {
                var parentComment = await _commentRepository.GetCommentById((Guid)comment.ParentCommentId);
                if (parentComment == null)
                {
                    throw new KeyNotFoundException($"Comment with Guid={comment.ParentCommentId} not found.");
                }

                parentComment.SubComments--;
            }
        }
        else
        {
            comment.Content = "";
            comment.DeleteDate = DateTime.UtcNow;
        }

        await _commentRepository.Save();
    }
}