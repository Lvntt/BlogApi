using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.TagRepository;

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

    public async Task<bool> AddTag(Tag tag)
    {
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Tag>> GetTags()
    {
        return await _context.Tags.ToListAsync();
    }

    public Tag? GetTagFromGuid(Guid id)
    {
        return _context.Tags.FirstOrDefault(tag => tag.Id == id);
    }
}