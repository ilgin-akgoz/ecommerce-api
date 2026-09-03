using System.Net;
using System.Text.Json;
using EcommerceApi.Api.Exceptions;

namespace EcommerceApi.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            BusinessRuleException => (HttpStatusCode.BadRequest, "Business rule violation"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        else 
            _logger.LogWarning("{ExceptionType} handled: {Message}", ex.GetType().Name, ex.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            type = $"https://tools.ietf.org/html/rfc7231#section-6.6.{(int)statusCode % 10}",
            title,
            status = (int)statusCode,
            detail = statusCode == HttpStatusCode.InternalServerError ? "An unexpected error occurred." : ex.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}