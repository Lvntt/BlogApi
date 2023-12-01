using BlogApi.Dtos;
using BlogApi.Dtos.ValidationAttributes;
using BlogApi.Models;
using BlogApi.Services.UserService;
using BlogApi.Services.JwtService;
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
    public async Task<ActionResult<TokenModel>> Register([FromBody] UserRegisterDto userRegisterDto)
    {
        var user = await _userService.Register(userRegisterDto);
        
        return Ok(
            new TokenModel { Token = _jwtService.GenerateToken(user) }
        );
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenModel>> Login([FromBody] LoginCredentialsDto loginCredentialsDto)
    {
        var user = await _userService.Login(loginCredentialsDto);
        
        return Ok(
            new TokenModel { Token = _jwtService.GenerateToken(user) }
        );
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        await _userService.Logout(
            new TokenModel { Token = token }
        );
        return Ok();
    }

    [Authorize]
    [HttpGet("profile")]
    public ActionResult<UserDto> GetUserProfile()
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;
        
        return Ok(_userService.GetUserProfile(userId));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> EditUserProfile([FromBody] UserEditDto userEditDto)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;
        
        await _userService.EditUserProfile(userEditDto, userId);
        return Ok();
    }
}