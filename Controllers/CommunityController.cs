using System.Security.Claims;
using BlogApi.Dtos;
using BlogApi.Dtos.ValidationAttributes;
using BlogApi.Models;
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
    [HttpGet("{id}/post")]
    public async Task<ActionResult<Guid>> GetCommunityPosts(
        Guid id, 
        [FromQuery] List<Guid>? tags,
        [FromQuery] SortingOption? sorting,
        [FromQuery] int page = 1,
        [FromQuery] int size = 5
    )
    {
        return Ok(await _communityService.GetCommunityPosts((Guid)UserId!, id, tags, sorting, page, size));
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
        return Ok(await _communityService.CreateCommunity(communityCreateDto, (Guid)UserId!));
    }

    [Authorize]
    [HttpPost("{id}/post")]
    public async Task<ActionResult<Guid>> CreatePost(PostCreateDto postCreateDto, Guid id)
    {
        return Ok(await _communityService.CreatePost(postCreateDto, (Guid)UserId!, id));
    }

    [Authorize]
    [HttpGet("{id}/role")]
    public async Task<ActionResult<Guid>> GetUserRoleInCommunity(Guid id)
    {
        return Ok(await _communityService.GetUserRoleInCommunity(id, (Guid)UserId!));
    }

    [Authorize]
    [HttpPost("{id}/subscribe")]
    public async Task<IActionResult> SubscribeToCommunity(Guid id)
    {
        await _communityService.SubscribeToCommunity(id, (Guid)UserId!);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id}/unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromCommunity(Guid id)
    {
        await _communityService.UnsubscribeFromCommunity(id, (Guid)UserId!);
        return Ok();
    }
}