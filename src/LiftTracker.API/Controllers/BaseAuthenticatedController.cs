using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiftTracker.API.Controllers;

/// <summary>
/// Base controller providing common functionality for authenticated endpoints
/// </summary>
public abstract class BaseAuthenticatedController : ControllerBase
{
    protected readonly ILogger _logger;

    protected BaseAuthenticatedController(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the current authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>User ID if authenticated, null otherwise</returns>
    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim?.Value != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Validates user authentication and returns standardized error response if invalid
    /// </summary>
    /// <returns>Unauthorized response if authentication fails, null if valid</returns>
    protected IActionResult? ValidateUserAuthentication()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            _logger.LogWarning("Authentication failed - invalid or missing user token");
            return Unauthorized(new { error = "Invalid user token" });
        }

        return null;
    }

    /// <summary>
    /// Gets the authenticated user ID or returns error response
    /// </summary>
    /// <returns>Tuple with user ID and optional error response</returns>
    protected (Guid? userId, IActionResult? errorResponse) GetAuthenticatedUserId()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            _logger.LogWarning("Authentication failed - invalid or missing user token");
            return (null, Unauthorized(new { error = "Invalid user token" }));
        }

        return (userId, null);
    }
}
