using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Middlewares;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (InvalidOperationException ioe)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var problem = new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Type = "Error",
                Detail = ioe.Message
            };

            var jsonProblem = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonProblem);
        }
        catch (AuthenticationException ae)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var problem = new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Type = "Error",
                Detail = ae.Message
            };

            var jsonProblem = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonProblem);
        }
        catch (KeyNotFoundException nfe)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            var problem = new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Type = "Error",
                Detail = nfe.Message
            };
            
            var jsonProblem = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonProblem);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"An unexpected error occured. \n" +
                                $"StackTrace: {e.StackTrace}");
            
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            
            var problem = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "Internal server error",
                Detail = "An internal server error has occurred."
            };
            
            var jsonProblem = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonProblem);
        }
    }
}