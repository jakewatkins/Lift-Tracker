using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using LiftTracker.Infrastructure.Caching;
using LiftTracker.Infrastructure.Logging;

namespace LiftTracker.API.Controllers;

/// <summary>
/// Controller for performance monitoring and cache management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerformanceController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(
        ICacheService cacheService,
        ILogger<PerformanceController> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets basic performance metrics
    /// </summary>
    /// <returns>Performance metrics</returns>
    [HttpGet("metrics")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)] // Cache for 30 seconds
    public ActionResult<PerformanceMetrics> GetMetrics()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var metrics = new PerformanceMetrics
            {
                Timestamp = DateTime.UtcNow,
                MemoryUsage = GC.GetTotalMemory(false),
                Generation0Collections = GC.CollectionCount(0),
                Generation1Collections = GC.CollectionCount(1),
                Generation2Collections = GC.CollectionCount(2),
                ProcessorTime = Environment.TickCount64,
                WorkingSet = Environment.WorkingSet,
                ThreadCount = Environment.ProcessorCount
            };

            stopwatch.Stop();
            _logger.LogPerformanceMetric("GET_PERFORMANCE_METRICS", stopwatch.ElapsedMilliseconds);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error getting performance metrics in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return StatusCode(500, "Error retrieving performance metrics");
        }
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    /// <returns>Cache statistics</returns>
    [HttpGet("cache/stats")]
    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)] // Cache for 10 seconds
    public ActionResult<CacheStatistics> GetCacheStats()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Note: In a real implementation, you'd extend ICacheService to provide stats
            var stats = new CacheStatistics
            {
                Timestamp = DateTime.UtcNow,
                EstimatedCacheSize = "Varies", // IMemoryCache doesn't expose size directly
                CacheHits = "Not available", // Would need custom implementation
                CacheMisses = "Not available",
                EvictionCount = "Not available"
            };

            stopwatch.Stop();
            _logger.LogPerformanceMetric("GET_CACHE_STATS", stopwatch.ElapsedMilliseconds);

            return Ok(stats);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error getting cache statistics in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return StatusCode(500, "Error retrieving cache statistics");
        }
    }

    /// <summary>
    /// Clears cache by pattern (admin only)
    /// </summary>
    /// <param name="pattern">Cache key pattern to clear</param>
    /// <returns>Success message</returns>
    [HttpDelete("cache")]
    [Authorize(Roles = "Admin")] // Restrict to admin users
    public async Task<ActionResult> ClearCache([FromQuery] string pattern = ".*")
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _cacheService.RemoveByPatternAsync(pattern);

            stopwatch.Stop();
            _logger.LogInformation("Cache cleared with pattern: {Pattern} in {ElapsedMs}ms",
                pattern, stopwatch.ElapsedMilliseconds);

            return Ok(new { message = $"Cache cleared with pattern: {pattern}",
                           elapsedMs = stopwatch.ElapsedMilliseconds });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error clearing cache with pattern {Pattern} in {ElapsedMs}ms",
                pattern, stopwatch.ElapsedMilliseconds);
            return StatusCode(500, "Error clearing cache");
        }
    }

    /// <summary>
    /// Forces garbage collection (admin only)
    /// </summary>
    /// <returns>Garbage collection results</returns>
    [HttpPost("gc")]
    [Authorize(Roles = "Admin")]
    public ActionResult<GcResults> ForceGarbageCollection()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var beforeMemory = GC.GetTotalMemory(false);
            var beforeGen0 = GC.CollectionCount(0);
            var beforeGen1 = GC.CollectionCount(1);
            var beforeGen2 = GC.CollectionCount(2);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var afterMemory = GC.GetTotalMemory(false);
            var afterGen0 = GC.CollectionCount(0);
            var afterGen1 = GC.CollectionCount(1);
            var afterGen2 = GC.CollectionCount(2);

            var results = new GcResults
            {
                BeforeMemory = beforeMemory,
                AfterMemory = afterMemory,
                MemoryFreed = beforeMemory - afterMemory,
                Gen0Collections = afterGen0 - beforeGen0,
                Gen1Collections = afterGen1 - beforeGen1,
                Gen2Collections = afterGen2 - beforeGen2
            };

            stopwatch.Stop();
            _logger.LogInformation("Forced garbage collection completed in {ElapsedMs}ms. " +
                "Memory freed: {MemoryFreed} bytes", stopwatch.ElapsedMilliseconds, results.MemoryFreed);

            return Ok(results);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during forced garbage collection in {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
            return StatusCode(500, "Error during garbage collection");
        }
    }
}

// Supporting classes for performance monitoring
public class PerformanceMetrics
{
    public DateTime Timestamp { get; set; }
    public long MemoryUsage { get; set; }
    public int Generation0Collections { get; set; }
    public int Generation1Collections { get; set; }
    public int Generation2Collections { get; set; }
    public long ProcessorTime { get; set; }
    public long WorkingSet { get; set; }
    public int ThreadCount { get; set; }
}

public class CacheStatistics
{
    public DateTime Timestamp { get; set; }
    public string EstimatedCacheSize { get; set; } = string.Empty;
    public string CacheHits { get; set; } = string.Empty;
    public string CacheMisses { get; set; } = string.Empty;
    public string EvictionCount { get; set; } = string.Empty;
}

public class GcResults
{
    public long BeforeMemory { get; set; }
    public long AfterMemory { get; set; }
    public long MemoryFreed { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
}
