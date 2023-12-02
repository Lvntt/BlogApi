using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Mappers;

public static class PostMapper
{
    public static PostDto MapToPostDto(Post post, bool hasLike, List<TagDto> tagDtos)
    {
        return new PostDto
        {
            Id = post.Id,
            CreateTime = post.CreateTime,
            Title = post.Title,
            Description = post.Description,
            ReadingTime = post.ReadingTime,
            Image = post.Image,
            AuthorId = post.AuthorId,
            Author = post.Author,
            CommunityId = post.CommunityId,
            CommunityName = post.CommunityName,
            AddressId = post.AddressId,
            Likes = post.Likes,
            HasLike = hasLike,
            CommentsCount = post.CommentsCount,
            Tags = tagDtos
        };
    }
    
    public static Post MapToPost(PostCreateDto postCreateDto, User user, List<Tag> tags)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            Title = postCreateDto.Title,
            Description = postCreateDto.Description,
            ReadingTime = postCreateDto.ReadingTime,
            Image = postCreateDto.Image,
            AuthorId = user.Id,
            Author = user.FullName,
            CommunityId = null,
            CommunityName = null,
            AddressId = postCreateDto.AddressId,
            Likes = 0,
            CommentsCount = 0,
            Tags = tags
        };
    }
    
    public static Post MapToCommunityPost(PostCreateDto postCreateDto, User user, Community community, List<Tag> tags)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            Title = postCreateDto.Title,
            Description = postCreateDto.Description,
            ReadingTime = postCreateDto.ReadingTime,
            Image = postCreateDto.Image,
            AuthorId = user.Id,
            Author = user.FullName,
            CommunityId = community.Id,
            CommunityName = community.Name,
            AddressId = postCreateDto.AddressId,
            Likes = 0,
            CommentsCount = 0,
            Tags = tags
        };
    }
    
    public static PostFullDto MapToPostFullDto(Post post, bool hasLike, List<TagDto> tagDtos, List<CommentDto> comments)
    {
        return new PostFullDto
        {
            Id = post.Id,
            CreateTime = post.CreateTime,
            Title = post.Title,
            Description = post.Description,
            ReadingTime = post.ReadingTime,
            Image = post.Image,
            AuthorId = post.AuthorId,
            Author = post.Author,
            CommunityId = post.CommunityId,
            CommunityName = post.CommunityName,
            AddressId = post.AddressId,
            Likes = post.Likes,
            HasLike = hasLike,
            CommentsCount = post.CommentsCount,
            Tags = tagDtos,
            Comments = comments
        };
    }
}