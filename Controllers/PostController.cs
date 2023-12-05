using System.Security.Claims;
using BlogApi.Dtos;
using BlogApi.Models.Types;
using BlogApi.Services.PostService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Route("api/post")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;

    private Guid? UserId
    {
        get
        {
            var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
            var claims = identity.Claims;
            var rawUserId = claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(rawUserId, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
    
    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    [AllowAnonymous]
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
            await _postService.GetAllAvailablePosts((Guid)UserId!, tags, author, min, max, sorting, onlyMyCommunities, page, size)
        );
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Guid>> CreatePost(PostCreateDto request)
    {
        return Ok(await _postService.CreatePost(request, (Guid)UserId!));
    }

    [AllowAnonymous]
    [HttpGet("{postId}")]
    public async Task<ActionResult<PostFullDto>> GetPostInfo(Guid postId)
    {
        return Ok(await _postService.GetPostInfo(postId, (Guid)UserId!));
    }

    [Authorize]
    [HttpPost("{postId}/like")]
    public async Task<IActionResult> AddLikeToPost(Guid postId)
    {
        await _postService.AddLikeToPost(postId, (Guid)UserId!);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{postId}/like")]
    public async Task<IActionResult> RemoveLikeFromPost(Guid postId)
    {
        await _postService.RemoveLikeFromPost(postId, (Guid)UserId!);
        return Ok();
    }
}