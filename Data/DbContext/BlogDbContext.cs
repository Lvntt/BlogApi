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
        
        modelBuilder.Entity<Community>()
            .HasMany<User>()
            .WithMany()
            .UsingEntity<CommunityMember>(cm => cm.ToTable("CommunityMembers"));
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<TokenModel> InvalidTokens { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Community> Communities { get; set; }
    public DbSet<CommunityMember> CommunityMembers { get; set; }
    
}