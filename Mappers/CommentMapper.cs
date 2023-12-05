using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Mappers;

public static class CommentMapper
{
    public static Comment MapToComment(Guid postId, Guid? topLevelCommentId, CreateCommentDto createCommentDto, User author)
    {
        return new Comment
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            PostId = postId,
            ParentCommentId = createCommentDto.ParentId,
            TopLevelCommentId = topLevelCommentId,
            Content = createCommentDto.Content,
            ModifiedDate = null,
            DeleteDate = null,
            AuthorId = author.Id,
            Author = author.FullName,
            SubComments = 0
        };
    }
}