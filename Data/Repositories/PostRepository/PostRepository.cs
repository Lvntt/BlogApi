using BlogApi.Dtos;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Post?> GetPost(Guid id)
    {
        return await _context.Posts
            .Include(post => post.Tags)
            .Include(post => post.Comments)
            .FirstOrDefaultAsync(post => post.Id == id);
    }

    // TODO not working for some reason bruhhhhhhhh
    public bool DidUserLikePost(Post post, User user)
    {
        return post.LikedUsers.Any(u => u.Id == user.Id);
    }

    public async Task AddLikeToPost(Post post, User user)
    {
        post.LikedUsers.Add(user);
        user.LikedPosts.Add(post);
        post.Likes++;
        await _context.SaveChangesAsync();
    }
}