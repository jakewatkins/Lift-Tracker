using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiftTracker.API.Controllers.Helpers;

/// <summary>
/// Helper class for common controller operations to reduce code duplication
/// </summary>
public static class ControllerHelpers
{
    /// <summary>
    /// Extracts the current user ID from JWT claims
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <returns>User ID if found and valid, null otherwise</returns>
    public static Guid? GetCurrentUserId(this ControllerBase controller)
    {
        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim?.Value != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Validates user authentication and returns standardized error if invalid
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="logger">Logger instance for warnings</param>
    /// <returns>Tuple with user ID and optional error response</returns>
    public static (Guid? userId, IActionResult? errorResponse) ValidateAuthentication(
        this ControllerBase controller,
        ILogger logger)
    {
        var userId = controller.GetCurrentUserId();
        if (userId == null)
        {
            logger.LogWarning("Authentication failed - invalid or missing user token");
            return (null, controller.Unauthorized(new { error = "Invalid user token" }));
        }

        return (userId, null);
    }

    /// <summary>
    /// Creates a standardized error response for exceptions
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="userId">User ID for context</param>
    /// <returns>Appropriate error response</returns>
    public static IActionResult HandleException(
        this ControllerBase controller,
        Exception ex,
        ILogger logger,
        string operationName,
        Guid? userId = null)
    {
        switch (ex)
        {
            case ArgumentException argEx:
                logger.LogWarning(argEx, "Invalid argument in {OperationName} for user: {UserId}", operationName, userId);
                return controller.BadRequest(new { error = argEx.Message });

            case InvalidOperationException invEx:
                logger.LogWarning(invEx, "Invalid operation in {OperationName} for user: {UserId}", operationName, userId);
                return controller.Conflict(new { error = invEx.Message });

            case KeyNotFoundException notFoundEx:
                logger.LogWarning(notFoundEx, "Resource not found in {OperationName} for user: {UserId}", operationName, userId);
                return controller.NotFound(new { error = notFoundEx.Message });

            case UnauthorizedAccessException authEx:
                logger.LogWarning(authEx, "Unauthorized access in {OperationName} for user: {UserId}", operationName, userId);
                return controller.Forbid();

            default:
                logger.LogError(ex, "Error in {OperationName} for user: {UserId}", operationName, userId);
                return controller.StatusCode(500, new { error = $"Failed to {operationName.ToLowerInvariant()}" });
        }
    }

    /// <summary>
    /// Validates model state and returns standardized validation error response
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <returns>BadRequest with validation errors if invalid, null if valid</returns>
    public static IActionResult? ValidateModel(this ControllerBase controller)
    {
        if (!controller.ModelState.IsValid)
        {
            var errors = controller.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            return controller.BadRequest(new
            {
                error = "Validation failed",
                details = errors
            });
        }

        return null;
    }

    /// <summary>
    /// Creates a standardized success response with optional metadata
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="controller">The controller instance</param>
    /// <param name="data">Response data</param>
    /// <param name="includeMetadata">Whether to include metadata like timestamp</param>
    /// <returns>OK response with data and optional metadata</returns>
    public static IActionResult SuccessResponse<T>(
        this ControllerBase controller,
        T data,
        bool includeMetadata = false)
    {
        if (includeMetadata)
        {
            return controller.Ok(new
            {
                data = data,
                meta = new
                {
                    timestamp = DateTime.UtcNow,
                    requestId = Guid.NewGuid().ToString()
                }
            });
        }

        return controller.Ok(data);
    }

    /// <summary>
    /// Executes an operation with standardized error handling
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="controller">The controller instance</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="userId">User ID for context</param>
    /// <returns>ActionResult with operation result or error response</returns>
    public static async Task<IActionResult> ExecuteWithErrorHandling<T>(
        this ControllerBase controller,
        Func<Task<T>> operation,
        ILogger logger,
        string operationName,
        Guid? userId = null)
    {
        try
        {
            var result = await operation();
            return controller.Ok(result);
        }
        catch (Exception ex)
        {
            return controller.HandleException(ex, logger, operationName, userId);
        }
    }
}
