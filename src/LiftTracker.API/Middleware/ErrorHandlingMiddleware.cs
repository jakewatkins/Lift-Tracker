using System.Net;
using System.Text.Json;

namespace LiftTracker.API.Middleware;

/// <summary>
/// Global error handling middleware for catching and formatting exceptions
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                CreateErrorResponse("Unauthorized access", "Authentication required", context)
            ),
            InvalidOperationException ex when ex.Message.Contains("not found") => (
                HttpStatusCode.NotFound,
                CreateErrorResponse("Resource not found", ex.Message, context)
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                CreateErrorResponse("Invalid operation", exception.Message, context)
            ),
            ArgumentException => (
                HttpStatusCode.BadRequest,
                CreateErrorResponse("Invalid argument", exception.Message, context)
            ),
            NotImplementedException => (
                HttpStatusCode.NotImplemented,
                CreateErrorResponse("Feature not implemented", "This feature is not yet available", context)
            ),
            TimeoutException => (
                HttpStatusCode.RequestTimeout,
                CreateErrorResponse("Request timeout", "The request took too long to process", context)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                CreateErrorResponse("Internal server error", "An unexpected error occurred", context, exception)
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private object CreateErrorResponse(
        string error,
        string message,
        HttpContext context,
        Exception? exception = null)
    {
        var response = new
        {
            error = error,
            message = message,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value,
            method = context.Request.Method,
            traceId = context.TraceIdentifier
        };

        // In development, include stack trace for debugging
        if (_environment.IsDevelopment() && exception != null)
        {
            return new
            {
                error = response.error,
                message = response.message,
                timestamp = response.timestamp,
                path = response.path,
                method = response.method,
                traceId = response.traceId,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException?.Message
            };
        }

        return response;
    }
}

/// <summary>
/// Extension methods for adding ErrorHandlingMiddleware to the pipeline
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds the ErrorHandlingMiddleware to the application pipeline
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
