using BlogApi.Dtos;
using BlogApi.Dtos.ValidationAttributes;
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

    [HttpGet("{id}")]
    public async Task<ActionResult<CommunityFullDto>> GetCommunityInfo(Guid id)
    {
        return Ok(await _communityService.GetCommunityInfo(id));
    }
    
    [Authorize]
    [HttpPost("create")]
    public async Task<ActionResult<Guid>> CreateCommunity(CommunityCreateDto request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;
        
        return Ok(await _communityService.CreateCommunity(request, userId));
    }

    [Authorize]
    [HttpPost("{id}/post")]
    public async Task<ActionResult<Guid>> CreatePost(PostCreateDto request, Guid id)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        return Ok(await _communityService.CreatePost(request, userId, id));
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