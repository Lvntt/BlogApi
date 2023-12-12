using BlogApi.Dtos;
using BlogApi.Services.TagService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Route("api/tag")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TagDto>>> GetTags()
    {
        return Ok(await _tagService.GetTags());
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateTag(TagCreateDto tagCreateDto)
    {
        await _tagService.CreateTag(tagCreateDto);
        
        return Ok();
    }
}