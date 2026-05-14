using System.Net;
using System.Text.Json;
using LMS.Application.Common;

namespace LMS.API.Middleware;

/// <summary>
/// Global exception handler — catches all unhandled exceptions and returns
/// a consistent ApiResponse envelope. No raw stack traces reach the client.
/// Registered in Program.cs via app.UseMiddleware&lt;ExceptionHandlingMiddleware&gt;().
/// See Section 12 — Error Handling Strategy.
/// </summary>
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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, exception.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound, exception.Message),
            InvalidOperationException   => (HttpStatusCode.BadRequest, exception.Message),
            ArgumentException           => (HttpStatusCode.BadRequest, exception.Message),
            NotImplementedException     => (HttpStatusCode.NotImplemented, "This feature is not yet implemented."),
            _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
