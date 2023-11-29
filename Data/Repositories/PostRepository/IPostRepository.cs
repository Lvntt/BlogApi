using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Data.Repositories.PostRepository;

public interface IPostRepository
{
    IQueryable<Post> GetAllPosts();
    IQueryable<Post> GetPostsByTagsId(IQueryable<Post> posts, List<Guid> tagsId);
    IQueryable<Post> GetPostsByAuthor(IQueryable<Post> posts, string query);
    IQueryable<Post> GetPostsByMinReadingTime(IQueryable<Post> posts, int minTime);
    IQueryable<Post> GetPostsByMaxReadingTime(IQueryable<Post> posts, int maxTime);
    IQueryable<Post> GetSortedPosts(IQueryable<Post> posts, SortingOption sortingOption);
    IQueryable<Post> GetOnlyMyCommunitiesPosts(IQueryable<Post> posts, Guid userId);
    List<Post> GetPagedPosts(IQueryable<Post> posts, PageInfoModel pagination);
    Task<Guid> AddPost(Post post);
    Task<Post?> GetPost(Guid id);
    bool DidUserLikePost(Post post, User user);
    Task<Like?> GetExistingLike(Post post, User user);
    Task AddLikeToPost(Post post, User user);
    Task RemoveLikeFromPost(Post post, Like like);
}