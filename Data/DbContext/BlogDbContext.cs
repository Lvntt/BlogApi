using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
            .HasMany(p => p.LikedUsers)
            .WithMany(u => u.LikedPosts)
            .UsingEntity(j => j.ToTable("Likes"));
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<TokenModel> InvalidTokens { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Comment> Comments { get; set; }
    
}