using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace LiftTracker.API.Middleware;

/// <summary>
/// Middleware for handling input validation and model state validation
/// </summary>
public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continue to next middleware
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error occurred: {ValidationError}", ex.Message);
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Argument validation error occurred: {ArgumentError}", ex.Message);
            await HandleArgumentExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Validation failed",
            message = ex.Message,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static async Task HandleArgumentExceptionAsync(HttpContext context, ArgumentException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Invalid argument",
            message = ex.Message,
            parameter = ex.ParamName,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension methods for adding ValidationMiddleware to the pipeline
/// </summary>
public static class ValidationMiddlewareExtensions
{
    /// <summary>
    /// Adds the ValidationMiddleware to the application pipeline
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseValidationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ValidationMiddleware>();
    }
}
