using AutoMapper;
using BlogApi.Data.Repositories.CommentRepo;
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

    public CommentService(ICommentRepository commentRepository, IPostRepository postRepository, IUserRepository userRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _mapper = mapper;
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
}