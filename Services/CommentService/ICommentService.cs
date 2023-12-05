using BlogApi.Dtos;

namespace BlogApi.Services.CommentService;

public interface ICommentService
{
    Task<List<CommentDto>> GetCommentTree(Guid commentId);
    Task AddComment(Guid postId, Guid authorId, CreateCommentDto createCommentDto);
}