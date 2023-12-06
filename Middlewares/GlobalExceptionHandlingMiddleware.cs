using System.Net;
using System.Text.Json;
using BlogApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Middlewares;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private class ExceptionDetails
    {
        public int StatusCode { get; init; }
        public string Detail { get; init; } = string.Empty;
    }
    
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
        catch (Exception ex)
        {
            var exceptionDescription = GetExceptionDetails(ex);
            
            context.Response.StatusCode = exceptionDescription.StatusCode;
            var problem = new ProblemDetails
            {
                Status = exceptionDescription.StatusCode,
                Type = "Error",
                Detail = exceptionDescription.Detail
            };
            
            var jsonProblem = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonProblem);

        }
    }

    private ExceptionDetails GetExceptionDetails(Exception ex)
    {
        int statusCode;
        string detail;

        switch (ex)
        {
            case EntityNotFoundException notFoundEx:
                statusCode = (int)HttpStatusCode.NotFound;
                detail = notFoundEx.Message;
                break;
            case EntityExistsException entityExistsEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                detail = entityExistsEx.Message;
                break;
            case InvalidActionException invalidActionEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                detail = invalidActionEx.Message;
                break;
            case InvalidCredentialsException invalidCredentialsEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                detail = invalidCredentialsEx.Message;
                break;
            case ForbiddenActionException forbiddenActionEx:
                statusCode = (int)HttpStatusCode.Forbidden;
                detail = forbiddenActionEx.Message;
                break;
            default:
                _logger.LogError(ex, $"An unexpected error occurred. StackTrace: \n \t\t {ex.StackTrace}");
                statusCode = (int)HttpStatusCode.InternalServerError;
                detail = "An internal server error has occurred.";
                break;
        }

        return new ExceptionDetails
        {
            StatusCode = statusCode,
            Detail = detail
        };
    }
}