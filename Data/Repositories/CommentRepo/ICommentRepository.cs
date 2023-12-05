using BlogApi.Models;

namespace BlogApi.Data.Repositories.CommentRepo;

public interface ICommentRepository
{
    Task<Comment?> GetCommentById(Guid id);
    Task AddComment(Comment comment);
    Task<List<Comment>> GetCommentTree(Comment comment);
    void DeleteComment(Comment comment);
    Task Save();
}