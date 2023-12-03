using BlogApi.Dtos;
using BlogApi.Services.AuthorService;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Route("api/author")]
[ApiController]
public class AuthorController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<AuthorDto>>> GetAllAuthors()
    {
        return Ok(await _authorService.GetAllAuthors());
    }
}