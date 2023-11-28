using BlogApi.Dtos;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.PostgresTypes;

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
            .Include(post => post.LikedPosts)
            .FirstOrDefaultAsync(post => post.Id == id);
    }
    
    public bool DidUserLikePost(Post post, User user)
    {
        return post.LikedPosts.Any(liked => liked.UserId == user.Id);
    }

    public async Task AddLikeToPost(Post post, User user)
    {
        post.LikedPosts.Add(new Likes { PostId = post.Id, UserId = user.Id });
        post.Likes++;
        await _context.SaveChangesAsync();
    }
}