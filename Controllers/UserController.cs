
using BlogApi.Dtos;
using BlogApi.Services;
using BlogApi.Services.JwtService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Route("api/account")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public UserController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<TokenDto>> Register([FromBody] UserRegisterDto request)
    {
        var response = await _userService.Register(request);
        if (response.IsSuccessful)
        {
            return Ok(
                new TokenDto { Token = _jwtService.GenerateToken(response.Data!) }
            );
        }
        
        return BadRequest(response.ErrorMessage);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginCredentialsDto request)
    {
        var response = await _userService.Login(request);
        if (response.IsSuccessful)
        {
            return Ok(
                new TokenDto { Token = _jwtService.GenerateToken(response.Data!) }
            );
        }
        
        return BadRequest(response.ErrorMessage);
    }
}