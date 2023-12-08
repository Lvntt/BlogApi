using AutoMapper;
using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Exceptions;
using BlogApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services.CommentService;

public class CommentService : ICommentService
{
    private readonly BlogDbContext _context;
    private readonly IMapper _mapper;

    public CommentService(IMapper mapper, BlogDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<List<CommentDto>> GetCommentTree(Guid commentId)
    {
        var parentComment = await _context.Comments.FirstOrDefaultAsync(comment => comment.Id == commentId)
                            ?? throw new EntityNotFoundException($"Comment with Guid={commentId} not found.");

        if (parentComment.ParentCommentId != null)
            throw new InvalidActionException($"Comment with Guid={commentId} is not a root comment.");

        var commentTree = await _context.Comments
            .Where(c => c.TopLevelCommentId == parentComment.Id)
            .ToListAsync();
        return commentTree
            .OrderBy(comment => comment.CreateTime)
            .Select(comment => _mapper.Map<CommentDto>(comment))
            .ToList();
    }

    public async Task AddComment(Guid postId, Guid authorId, CreateCommentDto createCommentDto)
    {
        var post = await _context.Posts
                .Include(post => post.Tags)
                .Include(post => post.Comments)
                .Include(post => post.LikedPosts)
                .FirstOrDefaultAsync(post => post.Id == postId)
                   ?? throw new EntityNotFoundException($"Post with Guid={postId} not found.");

        var author = await _context.Users.FirstOrDefaultAsync(user => user.Id == authorId)
                     ?? throw new EntityNotFoundException("User not found.");

        Guid? topLevelCommentId = null;

        if (createCommentDto.ParentId != null)
        {
            var parentComment = await _context.Comments.FirstOrDefaultAsync(comment => comment.Id == createCommentDto.ParentId);;
            if (parentComment == null || parentComment.PostId != postId)
                throw new EntityNotFoundException($"Comment with Guid={createCommentDto.ParentId} not found.");

            topLevelCommentId = parentComment.TopLevelCommentId ?? parentComment.Id;
            parentComment.SubComments++;
        }

        var newComment = CommentMapper.MapToComment(postId, topLevelCommentId, createCommentDto, author);

        post.CommentsCount++;
        await _context.Comments.AddAsync(newComment);
        await _context.SaveChangesAsync();
    }

    public async Task EditComment(Guid commentId, Guid authorId, UpdateCommentDto updateCommentDto)
    {
        var author = await _context.Users.FirstOrDefaultAsync(user => user.Id == authorId)
                     ?? throw new EntityNotFoundException("User not found.");

        var comment = await _context.Comments.FirstOrDefaultAsync(comment => comment.Id == commentId)
                      ?? throw new EntityNotFoundException($"Comment with Guid={commentId} not found.");

        var post = await _context.Posts
                .Include(post => post.Tags)
                .Include(post => post.Comments)
                .Include(post => post.LikedPosts)
                .FirstOrDefaultAsync(post => post.Id == comment.PostId)
                   ?? throw new EntityNotFoundException($"Post with Guid={comment.PostId} not found.");

        if (post.CommunityId != null)
        {
            var community = await _context.Communities
                    .Include(community => community.Members)
                    .FirstOrDefaultAsync(community => community.Id == post.CommunityId)
                            ?? throw new EntityNotFoundException($"Community with Guid={post.CommunityId} not found.");

            if (community.IsClosed && await _context.CommunityMembers.FirstOrDefaultAsync(cm =>
                    cm.CommunityId == community.Id 
                    && cm.UserId == author.Id) == null)
                throw new ForbiddenActionException(
                    $"This user can't interact with closed group with Guid={community.Id}");
        }

        if (comment.AuthorId != author.Id)
            throw new ForbiddenActionException("Users can only interact with their own comments.");

        comment.Content = updateCommentDto.Content;
        comment.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteComment(Guid commentId, Guid authorId)
    {
        var author = await _context.Users.FirstOrDefaultAsync(user => user.Id == authorId)
                     ?? throw new EntityNotFoundException("User not found.");

        var comment = await _context.Comments.FirstOrDefaultAsync(comment => comment.Id == commentId)
                      ?? throw new EntityNotFoundException($"Comment with Guid={commentId} not found.");

        var post = await _context.Posts
                .Include(post => post.Tags)
                .Include(post => post.Comments)
                .Include(post => post.LikedPosts)
                .FirstOrDefaultAsync(post => post.Id == comment.PostId)
                   ?? throw new EntityNotFoundException($"Post with Guid={comment.PostId} not found.");

        if (post.CommunityId != null)
        {
            var community = await _context.Communities
                    .Include(community => community.Members)
                    .FirstOrDefaultAsync(community => community.Id == post.CommunityId)
                            ?? throw new EntityNotFoundException($"Community with Guid={post.CommunityId} not found.");

            if (community.IsClosed && await _context.CommunityMembers.FirstOrDefaultAsync(cm =>
                    cm.CommunityId == community.Id 
                    && cm.UserId == author.Id) == null)
                throw new ForbiddenActionException(
                    $"This user can't interact with closed group with Guid={community.Id}");
        }

        if (comment.AuthorId != author.Id)
            throw new ForbiddenActionException("Users can only interact with their own comments.");

        if (comment.DeleteDate != null)
            throw new InvalidActionException("This comment is already deleted.");

        if (comment.SubComments == 0)
        {
            _context.Comments.Remove(comment);

            if (comment.ParentCommentId != null)
            {
                var parentComment = await _context.Comments.FirstOrDefaultAsync(comment => comment.Id == comment.ParentCommentId)
                                    ?? throw new EntityNotFoundException(
                                        $"Comment with Guid={comment.ParentCommentId} not found.");

                parentComment.SubComments--;
            }
        }
        else
        {
            comment.Content = "";
            comment.DeleteDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}