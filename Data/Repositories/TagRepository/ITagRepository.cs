using BlogApi.Models;

namespace BlogApi.Data.Repositories.TagRepository;

public interface ITagRepository
{
    Task<Tag?> GetTagByName(string name);
    Task AddTag(Tag tag);
    Task<List<Tag>> GetTags();
    Task<Tag?> GetTagFromGuid(Guid id);
    Task Save();
}