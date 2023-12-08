using System.Security.Claims;
using BlogApi.Dtos;
using BlogApi.Models.Types;
using BlogApi.Services.CommunityService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Route("api/community")]
[ApiController]
public class CommunityController : ControllerBase
{
    private readonly ICommunityService _communityService;

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
        return Ok(await _communityService.GetUserCommunities((Guid)UserId!));
    }

    [AllowAnonymous]
    [HttpGet("{communityId}/post")]
    public async Task<ActionResult<Guid>> GetCommunityPosts(
        Guid communityId, 
        [FromQuery] List<Guid>? tags,
        [FromQuery] SortingOption? sorting,
        [FromQuery] int page = 1,
        [FromQuery] int size = 5
    )
    {
        return Ok(await _communityService.GetCommunityPosts(UserId, communityId, tags, sorting, page, size));
    }


    [HttpGet("{communityId}")]
    public async Task<ActionResult<CommunityFullDto>> GetCommunityInfo(Guid communityId)
    {
        return Ok(await _communityService.GetCommunityInfo(communityId));
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<ActionResult<Guid>> CreateCommunity(CommunityCreateDto communityCreateDto)
    {
        return Ok(await _communityService.CreateCommunity(communityCreateDto, (Guid)UserId!));
    }

    [Authorize]
    [HttpPost("{communityId}/post")]
    public async Task<ActionResult<Guid>> CreatePost(PostCreateDto postCreateDto, Guid communityId)
    {
        return Ok(await _communityService.CreatePost(postCreateDto, (Guid)UserId!, communityId));
    }

    [Authorize]
    [HttpGet("{communityId}/role")]
    public async Task<ActionResult<Guid>> GetUserRoleInCommunity(Guid communityId)
    {
        return Ok(await _communityService.GetUserRoleInCommunity(communityId, (Guid)UserId!));
    }

    [Authorize]
    [HttpPost("{communityId}/subscribe")]
    public async Task<IActionResult> SubscribeToCommunity(Guid communityId)
    {
        await _communityService.SubscribeToCommunity(communityId, (Guid)UserId!);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{communityId}/unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromCommunity(Guid communityId)
    {
        await _communityService.UnsubscribeFromCommunity(communityId, (Guid)UserId!);
        return Ok();
    }
}