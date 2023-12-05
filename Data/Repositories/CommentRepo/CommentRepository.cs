using BlogApi.Data.DbContext;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.CommentRepo;

public class CommentRepository : ICommentRepository
{
    private readonly BlogDbContext _context;

    public CommentRepository(BlogDbContext context)
    {
        _context = context;
    }

    public Task<Comment?> GetCommentById(Guid id)
    {
        return _context.Comments.FirstOrDefaultAsync(comment => comment.Id == id);
    }

    public async Task AddComment(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
    }

    public Task<List<Comment>> GetCommentTree(Comment comment)
    {
        return _context.Comments
            .Where(c => c.TopLevelCommentId == comment.Id)
            .ToListAsync();
    }

    public Task Save()
    {
        return _context.SaveChangesAsync();
    }
}