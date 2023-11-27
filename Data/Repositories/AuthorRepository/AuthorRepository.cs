using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.AuthorRepository;

public class AuthorRepository : IAuthorRepository
{
    private readonly BlogDbContext _context;

    public AuthorRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Author?> GetAuthorById(Guid id)
    {
        return await _context.Authors.FirstOrDefaultAsync(author => author.UserId == id);
    }

    public async Task AddAuthor(Author author)
    {
        await _context.Authors.AddAsync(author);
        await _context.SaveChangesAsync();
    }

    public async Task IncreaseAuthorPosts(Guid id)
    {
        var author = await GetAuthorById(id);
        author!.Posts += 1;
        await _context.SaveChangesAsync();
    }
}