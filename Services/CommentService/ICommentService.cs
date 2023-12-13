using BlogApi.Dtos;

namespace BlogApi.Services.CommentService;

public interface ICommentService
{
    Task<List<CommentDto>> GetCommentTree(Guid commentId, Guid? userId);
    Task AddComment(Guid postId, Guid authorId, CreateCommentDto createCommentDto);
    Task EditComment(Guid commentId, Guid authorId, UpdateCommentDto updateCommentDto);
    Task DeleteComment(Guid commentId, Guid authorId);
}