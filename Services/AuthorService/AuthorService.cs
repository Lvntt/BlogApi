using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services.AuthorService;

public class AuthorService : IAuthorService
{
    private readonly BlogDbContext _context;

    public AuthorService(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuthorDto>> GetAllAuthors()
    {
        var authors = await _context.Authors.ToListAsync();

        var authorDtos = new List<AuthorDto>();
        foreach (var author in authors)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == author.UserId);

            var authorDto = AuthorMapper.MapToAuthorDto(user!, author);
            
            authorDtos.Add(authorDto);
        }

        return authorDtos;
    }
}