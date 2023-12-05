using System.Security.Claims;
using BlogApi.Dtos;
using BlogApi.Services.CommentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Route("api/comment")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

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
    
    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [Authorize]
    [HttpPost("/api/post/{id}/comment")]
    public async Task<IActionResult> AddComment(Guid id, CreateCommentDto createCommentDto)
    {
        await _commentService.AddComment(id, (Guid)UserId!, createCommentDto);
        return Ok();
    }
    
    [Authorize]
    [HttpGet("{id}/tree")]
    public async Task<ActionResult<List<CommentDto>>> GetCommentTree(Guid id)
    {
        return Ok(await _commentService.GetCommentTree(id));
    }
}