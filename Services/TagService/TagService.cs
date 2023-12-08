using AutoMapper;
using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Exceptions;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services.TagService;

public class TagService : ITagService
{
    private readonly BlogDbContext _context;
    private readonly IMapper _mapper;

    public TagService(IMapper mapper, BlogDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task CreateTag(TagCreateDto request)
    {
        
        if (await _context.Tags.FirstOrDefaultAsync(tag => tag.Name == request.Name) != null)
            throw new EntityExistsException($"Tag with name {request.Name} already exists.");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreateTime = DateTime.UtcNow
        };
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TagDto>> GetTags()
    {
        // TODO to one query (single select)
        var tags = await _context.Tags.ToListAsync();
        var tagDtos = tags.Select(tag => _mapper.Map<TagDto>(tag)).ToList();

        return tagDtos;
    }
}