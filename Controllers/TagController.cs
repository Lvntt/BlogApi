using BlogApi.Dtos;
using BlogApi.Models;
using BlogApi.Services.TagService;
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
    public async Task<ActionResult<List<Tag>>> GetTags()
    {
        return Ok(await _tagService.GetTags());
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag(TagCreateDto request)
    {
        await _tagService.CreateTag(request);
        
        return Ok();
    }
}