using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
            .HasMany<User>()
            .WithMany()
            .UsingEntity<Like>(j => j.ToTable("Likes"));
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<TokenModel> InvalidTokens { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    
}