using BlogApi.Data.DbContext;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.AuthorRepo;

public class AuthorRepository : IAuthorRepository
{
    private readonly BlogDbContext _context;

    public AuthorRepository(BlogDbContext context)
    {
        _context = context;
    }

    public Task<List<Author>> GetAllAuthors()
    {
        return _context.Authors.ToListAsync();
    }

    public Task<Author?> GetAuthorById(Guid id)
    {
        return _context.Authors.FirstOrDefaultAsync(author => author.UserId == id);
    }

    public async Task AddAuthor(Author author)
    {
        await _context.Authors.AddAsync(author);
    }

    public Task Save()
    {
        return _context.SaveChangesAsync();
    }
}