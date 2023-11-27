using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Data.Repositories.PostRepository;

public class PostRepository : IPostRepository
{
    private readonly BlogDbContext _context;

    public PostRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> AddPost(Post post)
    {
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
        return post.Id;
    }
}