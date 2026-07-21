using System.Net;
using System.Text.Json;
using Boardsync.Api.Common.Models;

namespace Boardsync.Api.Common.Middleware;

/// <summary>
/// Catches any unhandled exception in the request pipeline.
/// 
/// Why this exists:
/// Without this, an unhandled exception would return a raw stack trace to the client,
/// which is both a security risk and a bad user experience.
/// 
/// This middleware sits at the top of the pipeline and wraps everything in a try/catch.
/// If anything throws, we log it and return a clean ApiResponse with a 500 status code.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception occurred");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                "An unexpected error occurred. Please try again later."
            );

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
