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
    [HttpPost("/api/post/{postId}/comment")]
    public async Task<IActionResult> AddComment(Guid postId, CreateCommentDto createCommentDto)
    {
        await _commentService.AddComment(postId, (Guid)UserId!, createCommentDto);
        return Ok();
    }
    
    [Authorize]
    [HttpGet("{commentId}/tree")]
    public async Task<ActionResult<List<CommentDto>>> GetCommentTree(Guid commentId)
    {
        return Ok(await _commentService.GetCommentTree(commentId));
    }
    
    [Authorize]
    [HttpPut("{commentId}")]
    public async Task<IActionResult> EditComment(Guid commentId, UpdateCommentDto updateCommentDto)
    {
        await _commentService.EditComment(commentId, (Guid)UserId!, updateCommentDto);
        return Ok();
    }
    
    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        await _commentService.DeleteComment(commentId, (Guid)UserId!);
        return Ok();
    }
}