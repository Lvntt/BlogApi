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
    }

    public async Task Save()
    {
        await _context.SaveChangesAsync();
    }
}