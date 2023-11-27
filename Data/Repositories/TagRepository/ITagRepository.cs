using BlogApi.Models;

namespace BlogApi.Data.Repositories.TagRepository;

public interface ITagRepository
{
    Task<Tag?> GetTagByName(string name);
    Task<bool> AddTag(Tag tag);
    Task<List<Tag>> GetTags();
    Tag? GetTagFromGuid(Guid id);
}