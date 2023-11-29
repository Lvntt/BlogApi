using BlogApi.Dtos;
using BlogApi.Dtos.ValidationAttributes;
using BlogApi.Models;
using BlogApi.Services.PostService;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Route("api/post")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;

    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    // TODO handle authorize (allow anonymous)
    [HttpGet]
    public async Task<ActionResult<PostPagedListDto>> GetAllAvailablePosts(
        [FromQuery] List<Guid>? tags,
        [FromQuery] string? author,
        [FromQuery] int? min,
        [FromQuery] int? max,
        [FromQuery] SortingOption? sorting,
        [FromQuery] bool onlyMyCommunities = false,
        [FromQuery] int page = 1,
        [FromQuery] int size = 5
    )
    {
        return Ok(
            await _postService.GetAllAvailablePosts(tags, author, min, max, sorting, onlyMyCommunities, page, size)
        );
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Guid>> CreatePost(PostCreateDto request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        return Ok(await _postService.CreatePost(request, userId));
    }

    [AllowAnonymous]
    [HttpGet("{postId}")]
    public async Task<ActionResult<PostFullDto>> GetPostInfo(Guid postId)
    {
        var userId = (Guid?)HttpContext.Items["UserId"];

        return Ok(await _postService.GetPostInfo(postId, userId));
    }

    [Authorize]
    [HttpPost("{postId}/like")]
    public async Task<IActionResult> AddLikeToPost(Guid postId)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        await _postService.AddLikeToPost(postId, userId);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{postId}/like")]
    public async Task<IActionResult> RemoveLikeFromPost(Guid postId)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        await _postService.RemoveLikeFromPost(postId, userId);
        return Ok();
    }
}