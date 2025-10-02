using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace LiftTracker.Infrastructure.Caching;

/// <summary>
/// In-memory cache service implementation using IMemoryCache
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lockObject = new();

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a cached value by key
    /// </summary>
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(key))
            return Task.FromResult<T?>(null);

        try
        {
            var value = _memoryCache.Get<T>(key);
            if (value != null)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
            }
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(key) || value == null)
            return Task.CompletedTask;

        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Priority = CacheItemPriority.Normal
            };

            // Set callback to remove from tracking when evicted
            options.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
            {
                lock (_lockObject)
                {
                    _cacheKeys.Remove(evictedKey.ToString() ?? string.Empty);
                }
                _logger.LogDebug("Cache entry evicted for key: {Key}, Reason: {Reason}", evictedKey, reason);
            });

            _memoryCache.Set(key, value, options);

            lock (_lockObject)
            {
                _cacheKeys.Add(key);
            }

            _logger.LogDebug("Cache entry set for key: {Key}, Expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            return Task.CompletedTask;

        try
        {
            _memoryCache.Remove(key);
            lock (_lockObject)
            {
                _cacheKeys.Remove(key);
            }
            _logger.LogDebug("Cache entry removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes all values matching a pattern from the cache
    /// </summary>
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(pattern))
            return Task.CompletedTask;

        try
        {
            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<string>();

            lock (_lockObject)
            {
                keysToRemove.AddRange(_cacheKeys.Where(key => regex.IsMatch(key)));
            }

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                lock (_lockObject)
                {
                    _cacheKeys.Remove(key);
                }
            }

            _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a cached value or sets it if not found
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        try
        {
            var value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, expiration, cancellationToken);
            }
            return value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing factory function for cache key: {Key}", key);
            throw;
        }
    }
}
