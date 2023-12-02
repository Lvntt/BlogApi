using BlogApi.Dtos;

namespace BlogApi.Services.TagService;

public interface ITagService
{
    Task CreateTag(TagCreateDto request);
    Task<List<TagDto>> GetTags();
}