using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.TagRepo;

public class TagRepository : ITagRepository
{
    private readonly BlogDbContext _context;

    public TagRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetTagByName(string name)
    {
        return await _context.Tags.FirstOrDefaultAsync(tag => tag.Name == name);
    }

    public async Task AddTag(Tag tag)
    {
        await _context.Tags.AddAsync(tag);
    }

    public async Task<List<Tag>> GetTags()
    {
        return await _context.Tags.ToListAsync();
    }

    public Task<Tag?> GetTagFromGuid(Guid id)
    {
        return _context.Tags.FirstOrDefaultAsync(tag => tag.Id == id);
    }

    public async Task Save()
    {
        await _context.SaveChangesAsync();
    }
}