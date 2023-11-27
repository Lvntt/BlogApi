using BlogApi.Data.Repositories.TagRepository;
using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.TagService;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task CreateTag(TagCreateDto request)
    {
        var existingTag = await _tagRepository.GetTagByName(request.Name);
        if (existingTag != null)
        {
            throw new InvalidOperationException($"Tag with name {request.Name} already exists.");
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreateTime = DateTime.UtcNow
        };
        await _tagRepository.AddTag(tag);
    }

    public async Task<List<Tag>> GetTags()
    {
        return await _tagRepository.GetTags();
    }
}