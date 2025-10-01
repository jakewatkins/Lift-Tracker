using System.Security.Claims;
using System.Text.Json;

namespace LiftTracker.API.Middleware;

/// <summary>
/// Authentication middleware for JWT token validation and user context setup
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add user information to logs if authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = GetUserId(context.User);
            var userEmail = GetUserEmail(context.User);

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = userId ?? "unknown",
                ["UserEmail"] = userEmail ?? "unknown"
            }))
            {
                _logger.LogDebug("Processing request for authenticated user: {UserId}", userId);
                await _next(context);
            }
        }
        else
        {
            // Check if this is a protected endpoint
            var endpoint = context.GetEndpoint();
            var requiresAuth = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

            if (requiresAuth)
            {
                _logger.LogWarning("Unauthorized access attempt to protected endpoint: {Path}", context.Request.Path);
            }

            await _next(context);
        }
    }

    private static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private static string? GetUserEmail(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }
}

/// <summary>
/// Extension methods for adding AuthenticationMiddleware to the pipeline
/// </summary>
public static class AuthenticationMiddlewareExtensions
{
    /// <summary>
    /// Adds the AuthenticationMiddleware to the application pipeline
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}
