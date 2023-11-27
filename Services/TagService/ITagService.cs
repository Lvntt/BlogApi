using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Services.TagService;

public interface ITagService
{
    Task CreateTag(TagCreateDto request);
    Task<List<Tag>> GetTags();
}