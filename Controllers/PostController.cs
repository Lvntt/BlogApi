using BlogApi.Dtos;
using BlogApi.Dtos.ValidationAttributes;
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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Guid>> CreatePost(PostCreateDto request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        return Ok(await _postService.CreatePost(request, userId));
    }
}