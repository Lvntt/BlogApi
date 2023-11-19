using BlogApi.Dtos;
using BlogApi.Models;
using BlogApi.Services.UserService;
using BlogApi.Services.JwtService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
        var response = await _userService.Register(request);
        if (response.IsSuccessful)
        {
            return Ok(
                new TokenModel { Token = _jwtService.GenerateToken(response.Data!) }
            );
        }

        return BadRequest(response.ErrorMessage);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<TokenModel>> Login([FromBody] LoginCredentialsDto request)
    {
        var response = await _userService.Login(request);
        if (response.IsSuccessful)
        {
            return Ok(
                new TokenModel { Token = _jwtService.GenerateToken(response.Data!) }
            );
        }

        return BadRequest(response.ErrorMessage);
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (token.IsNullOrEmpty())
        {
            return Unauthorized();
        }

        var response = await _userService.Logout(
            new TokenModel { Token = token }
        );

        if (response)
        {
            return Ok();
        }

        return Unauthorized();
    }
}