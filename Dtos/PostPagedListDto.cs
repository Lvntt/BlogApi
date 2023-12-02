using BlogApi.Models;

namespace BlogApi.Dtos;

public class PostPagedListDto
{
    public List<PostDto>? Posts { get; set; }
    public PageInfoModel Pagination { get; set; } = new();
}