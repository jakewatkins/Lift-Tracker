using Microsoft.Extensions.Logging;

namespace LiftTracker.Infrastructure.Logging;

public static class LoggerExtensions
{
    // User operations
    public static void LogUserOperation(this ILogger logger, string userId, string operation, object? data = null)
    {
        logger.LogInformation("User {UserId} performed {Operation}. Data: {@Data}", userId, operation, data);
    }

    public static void LogUserOperationFailed(this ILogger logger, string userId, string operation, Exception exception, object? data = null)
    {
        logger.LogError(exception, "User {UserId} failed to perform {Operation}. Data: {@Data}", userId, operation, data);
    }

    // Database operations
    public static void LogDatabaseOperation(this ILogger logger, string operation, string entityType, object? entityId = null)
    {
        logger.LogDebug("Database {Operation} for {EntityType}. EntityId: {EntityId}", operation, entityType, entityId);
    }

    public static void LogDatabaseOperationFailed(this ILogger logger, string operation, string entityType, Exception exception, object? entityId = null)
    {
        logger.LogError(exception, "Database {Operation} failed for {EntityType}. EntityId: {EntityId}", operation, entityType, entityId);
    }

    // Authentication operations
    public static void LogAuthenticationSuccess(this ILogger logger, string userId, string provider)
    {
        logger.LogInformation("Authentication successful for user {UserId} via {Provider}", userId, provider);
    }

    public static void LogAuthenticationFailed(this ILogger logger, string? userId, string provider, string reason)
    {
        logger.LogWarning("Authentication failed for user {UserId} via {Provider}. Reason: {Reason}", userId ?? "Unknown", provider, reason);
    }

    // Performance logging
    public static void LogPerformanceMetric(this ILogger logger, string operation, long durationMs, object? context = null)
    {
        if (durationMs > 1000) // Log slow operations
        {
            logger.LogWarning("Slow operation detected: {Operation} took {DurationMs}ms. Context: {@Context}", operation, durationMs, context);
        }
        else
        {
            logger.LogDebug("Operation {Operation} completed in {DurationMs}ms. Context: {@Context}", operation, durationMs, context);
        }
    }

    // Business rule violations
    public static void LogBusinessRuleViolation(this ILogger logger, string rule, string userId, object? context = null)
    {
        logger.LogWarning("Business rule violation: {Rule} by user {UserId}. Context: {@Context}", rule, userId, context);
    }

    // Security events
    public static void LogSecurityEvent(this ILogger logger, string eventType, string userId, object? details = null)
    {
        logger.LogWarning("Security event: {EventType} for user {UserId}. Details: {@Details}", eventType, userId, details);
    }
}
