using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) {}
    
    public DbSet<User> Users { get; set; }
    public DbSet<TokenModel> InvalidTokens { get; set; }
    
}