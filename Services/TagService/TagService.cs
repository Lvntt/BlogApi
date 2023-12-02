using AutoMapper;
using BlogApi.Data.Repositories.TagRepository;
using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.TagService;

public class TagService : ITagService
{
    private readonly IMapper _mapper;
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository, IMapper mapper)
    {
        _tagRepository = tagRepository;
        _mapper = mapper;
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
        await _tagRepository.Save();
    }

    public async Task<List<TagDto>> GetTags()
    {
        var tags = await _tagRepository.GetTags();
        var tagDtos = tags.Select(tag => _mapper.Map<TagDto>(tag)).ToList();

        return tagDtos;
    }
}