using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LiftTracker.Application.Interfaces;

namespace LiftTracker.Application.Services;

/// <summary>
/// Sample service demonstrating performance optimization patterns
/// This is a demonstration class showing caching concepts
/// </summary>
public class PerformanceOptimizedService
{
    private readonly ILogger<PerformanceOptimizedService> _logger;
    private static readonly Dictionary<string, object> _simpleCache = new();
    private static readonly Dictionary<string, DateTime> _cacheExpiration = new();
    private static readonly object _cacheLock = new();

    public PerformanceOptimizedService(ILogger<PerformanceOptimizedService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets user progress data with simple in-memory caching
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="monthsBack">Number of months to look back</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached or computed progress data</returns>
    public async Task<List<ProgressDataPoint>> GetUserProgressAsync(
        Guid userId,
        Guid exerciseTypeId,
        int monthsBack = 12,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user_progress_{userId}_{exerciseTypeId}";

        // Check cache first
        if (TryGetFromCache<List<ProgressDataPoint>>(cacheKey, out var cachedValue))
        {
            _logger.LogDebug("Cache hit for user progress: {UserId}", userId);
            return cachedValue!;
        }

        // Compute and cache
        var result = await ComputeUserProgressAsync(userId, exerciseTypeId, monthsBack, cancellationToken);
        SetInCache(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    /// <summary>
    /// Gets cached user workout summary with fallback
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="daysBack">Number of days to look back</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workout summary data</returns>
    public async Task<PerformanceSummary> GetUserWorkoutSummaryAsync(
        Guid userId,
        int daysBack = 30,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"workout_summary_{userId}_{daysBack}";

        if (TryGetFromCache<PerformanceSummary>(cacheKey, out var cachedValue))
        {
            _logger.LogDebug("Cache hit for workout summary: {UserId}", userId);
            return cachedValue!;
        }

        var result = await ComputeWorkoutSummaryAsync(userId, daysBack, cancellationToken);
        SetInCache(cacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }

    /// <summary>
    /// Clears cache for specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    public void InvalidateUserCache(Guid userId)
    {
        lock (_cacheLock)
        {
            var keysToRemove = new List<string>();
            foreach (var key in _simpleCache.Keys)
            {
                if (key.Contains(userId.ToString()))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _simpleCache.Remove(key);
                _cacheExpiration.Remove(key);
            }
        }

        _logger.LogDebug("Invalidated cache for user: {UserId}", userId);
    }

    // Simple cache implementation
    private bool TryGetFromCache<T>(string key, out T? value)
    {
        lock (_cacheLock)
        {
            if (_simpleCache.TryGetValue(key, out var cachedObj) &&
                _cacheExpiration.TryGetValue(key, out var expiration) &&
                DateTime.UtcNow < expiration)
            {
                value = (T?)cachedObj;
                return value != null;
            }

            value = default;
            return false;
        }
    }

    private void SetInCache<T>(string key, T value, TimeSpan expiration)
    {
        lock (_cacheLock)
        {
            _simpleCache[key] = value!;
            _cacheExpiration[key] = DateTime.UtcNow.Add(expiration);
        }
    }

    // Private methods for computing data (simulated)
    private async Task<List<ProgressDataPoint>> ComputeUserProgressAsync(
        Guid userId,
        Guid exerciseTypeId,
        int monthsBack,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Computing progress data for user {UserId}, exercise {ExerciseTypeId}", userId, exerciseTypeId);

        // Simulate expensive computation
        await Task.Delay(100, cancellationToken);

        return new List<ProgressDataPoint>
        {
            new() { Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), Value = 100 },
            new() { Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)), Value = 110 },
            new() { Date = DateOnly.FromDateTime(DateTime.UtcNow), Value = 120 }
        };
    }

    private async Task<PerformanceSummary> ComputeWorkoutSummaryAsync(
        Guid userId,
        int daysBack,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Computing workout summary for user {UserId}, {DaysBack} days back", userId, daysBack);

        // Simulate expensive computation
        await Task.Delay(50, cancellationToken);

        return new PerformanceSummary
        {
            TotalWorkouts = 15,
            TotalDuration = TimeSpan.FromHours(20),
            AverageWorkoutDuration = TimeSpan.FromMinutes(80),
            LastWorkoutDate = DateTime.UtcNow.AddDays(-2)
        };
    }
}

// Supporting classes for the performance service
public class PerformanceSummary
{
    public int TotalWorkouts { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageWorkoutDuration { get; set; }
    public DateTime LastWorkoutDate { get; set; }
}
