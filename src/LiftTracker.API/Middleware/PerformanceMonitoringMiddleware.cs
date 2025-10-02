using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using LiftTracker.Infrastructure.Logging;

namespace LiftTracker.API.Middleware;

/// <summary>
/// Middleware for monitoring API performance and logging slow requests
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly int _slowRequestThresholdMs;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        int slowRequestThresholdMs = 1000)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _slowRequestThresholdMs = slowRequestThresholdMs;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start timing the request
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        var requestPath = $"{request.Method} {request.Path}{request.QueryString}";

        try
        {
            // Add request ID for tracking
            if (!context.Request.Headers.ContainsKey("X-Request-ID"))
            {
                context.Request.Headers.Add("X-Request-ID", Guid.NewGuid().ToString());
            }

            // Log request start
            _logger.LogDebug("Starting request: {RequestPath}", requestPath);

            // Call the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log errors with timing information
            stopwatch.Stop();
            _logger.LogError(ex, "Request failed: {RequestPath} in {ElapsedMs}ms",
                requestPath, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            // Stop timing and log performance metrics
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Create context for performance logging
            var performanceContext = new
            {
                RequestPath = requestPath,
                StatusCode = context.Response.StatusCode,
                RequestId = context.Request.Headers["X-Request-ID"].FirstOrDefault(),
                UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                ClientIP = GetClientIP(context),
                RequestSize = GetRequestSize(context),
                ResponseSize = GetResponseSize(context)
            };

            // Log performance metrics
            _logger.LogPerformanceMetric("HTTP_REQUEST", elapsedMs, performanceContext);

            // Add response headers for monitoring
            context.Response.Headers.Add("X-Response-Time", $"{elapsedMs}ms");
            context.Response.Headers.Add("X-Request-ID",
                context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? "unknown");

            // Log slow requests with additional detail
            if (elapsedMs > _slowRequestThresholdMs)
            {
                _logger.LogWarning("Slow request detected: {RequestPath} took {ElapsedMs}ms. " +
                    "Status: {StatusCode}, Client: {ClientIP}, Size: {RequestSize}b/{ResponseSize}b",
                    requestPath, elapsedMs, context.Response.StatusCode,
                    GetClientIP(context), GetRequestSize(context), GetResponseSize(context));
            }
        }
    }

    private static string GetClientIP(HttpContext context)
    {
        // Check various headers for the real client IP
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIP))
        {
            return realIP;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static long GetRequestSize(HttpContext context)
    {
        var contentLength = context.Request.Headers["Content-Length"].FirstOrDefault();
        return long.TryParse(contentLength, out var size) ? size : 0;
    }

    private static long GetResponseSize(HttpContext context)
    {
        // This is an approximation since response might be streamed
        var contentLength = context.Response.Headers["Content-Length"].FirstOrDefault();
        return long.TryParse(contentLength, out var size) ? size : 0;
    }
}
