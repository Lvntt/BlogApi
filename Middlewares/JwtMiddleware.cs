using BlogApi.Data.Repositories;
using BlogApi.Models;
using BlogApi.Services.JwtService;

namespace BlogApi.Middlewares;

public class JwtMiddleware : IMiddleware
{
    private readonly IJwtService _jwtService;
    private readonly ITokenBlacklistRepository _tokenBlacklistRepository;

    public JwtMiddleware(IJwtService jwtService, ITokenBlacklistRepository tokenBlacklistRepository)
    {
        _jwtService = jwtService;
        _tokenBlacklistRepository = tokenBlacklistRepository;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = _jwtService.ValidateToken(token);
        var isTokenInBlacklist =
            await _tokenBlacklistRepository.GetTokenFromBlacklist(new TokenModel { Token = token }) != null;
        if (userId != null && !isTokenInBlacklist)
        {
            context.Items["UserId"] = userId;
        }

        await next(context);
    }
}