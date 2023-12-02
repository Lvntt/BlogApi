using BlogApi.Dtos;
using BlogApi.Dtos.ValidationAttributes;
using BlogApi.Models;
using BlogApi.Services.CommunityService;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Route("api/community")]
[ApiController]
public class CommunityController : ControllerBase
{
    private readonly ICommunityService _communityService;

    public CommunityController(ICommunityService communityService)
    {
        _communityService = communityService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CommunityDto>>> GetAllCommunities()
    {
        return Ok(await _communityService.GetAllCommunities());
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<List<CommunityUserDto>>> GetUserCommunities()
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        return Ok(await _communityService.GetUserCommunities(userId));
    }

    [AllowAnonymous]
    [HttpGet("{id}/post")]
    public async Task<ActionResult<Guid>> GetCommunityPosts(
        Guid id, 
        [FromQuery] List<Guid>? tags,
        [FromQuery] SortingOption? sorting,
        [FromQuery] int page = 1,
        [FromQuery] int size = 5
    )
    {
        var userId = (Guid?)HttpContext.Items["UserId"];

        return Ok(await _communityService.GetCommunityPosts(userId, id, tags, sorting, page, size));
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<CommunityFullDto>> GetCommunityInfo(Guid id)
    {
        return Ok(await _communityService.GetCommunityInfo(id));
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<ActionResult<Guid>> CreateCommunity(CommunityCreateDto communityCreateDto)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        return Ok(await _communityService.CreateCommunity(communityCreateDto, userId));
    }

    [Authorize]
    [HttpPost("{id}/post")]
    public async Task<ActionResult<Guid>> CreatePost(PostCreateDto postCreateDto, Guid id)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        return Ok(await _communityService.CreatePost(postCreateDto, userId, id));
    }

    [Authorize]
    [HttpGet("{id}/role")]
    public async Task<ActionResult<Guid>> GetUserRoleInCommunity(Guid id)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        return Ok(await _communityService.GetUserRoleInCommunity(id, userId));
    }

    [Authorize]
    [HttpPost("{id}/subscribe")]
    public async Task<IActionResult> SubscribeToCommunity(Guid id)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        await _communityService.SubscribeToCommunity(id, userId);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id}/unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromCommunity(Guid id)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        await _communityService.UnsubscribeFromCommunity(id, userId);
        return Ok();
    }
}