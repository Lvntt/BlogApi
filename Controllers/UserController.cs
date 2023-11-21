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
    public async Task<ActionResult<TokenModel>> Register([FromBody] UserRegisterDto request)
    {
        var user = await _userService.Register(request);
        return Ok(
            new TokenModel { Token = _jwtService.GenerateToken(user) }
        );
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenModel>> Login([FromBody] LoginCredentialsDto request)
    {
        var user = await _userService.Login(request);
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
    public async Task<ActionResult<UserDto>> GetUserProfile()
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;
        var user = await _userService.GetUserProfile(userId);
            
        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            BirthDate = user.BirthDate,
            Gender = user.Gender,
            PhoneNumber = user.PhoneNumber,
            CreateTime = user.CreateTime
        });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> EditUserProfile([FromBody] UserEditDto request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;
        await _userService.EditUserProfile(request, userId);

        return Ok();
    }
}