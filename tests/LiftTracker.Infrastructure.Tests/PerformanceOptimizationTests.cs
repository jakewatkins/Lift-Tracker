using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using LiftTracker.Infrastructure.Caching;
using System;
using System.Threading.Tasks;

namespace LiftTracker.Infrastructure.Tests;

/// <summary>
/// Unit tests for performance optimization and caching infrastructure
/// </summary>
public class PerformanceOptimizationTests
{
    /// <summary>
    /// Tests that MemoryCacheService caches and retrieves values correctly
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_SetAndGet_ReturnsCorrectValue()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new LoggerFactory().CreateLogger<MemoryCacheService>();
        var cacheService = new MemoryCacheService(memoryCache, logger);

        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        await cacheService.SetAsync(key, value, expiration);
        var retrievedValue = await cacheService.GetAsync<string>(key);

        // Assert
        Assert.Equal(value, retrievedValue);
    }

    /// <summary>
    /// Tests that cache expiration works correctly
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_ExpiredEntry_ReturnsNull()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new LoggerFactory().CreateLogger<MemoryCacheService>();
        var cacheService = new MemoryCacheService(memoryCache, logger);

        var key = "expiring-key";
        var value = "expiring-value";
        var expiration = TimeSpan.FromMilliseconds(50);

        // Act
        await cacheService.SetAsync(key, value, expiration);
        await Task.Delay(100); // Wait for expiration
        var retrievedValue = await cacheService.GetAsync<string>(key);

        // Assert
        Assert.Null(retrievedValue);
    }

    /// <summary>
    /// Tests the GetOrSetAsync functionality
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_GetOrSetAsync_CallsFactoryOnlyOnce()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new LoggerFactory().CreateLogger<MemoryCacheService>();
        var cacheService = new MemoryCacheService(memoryCache, logger);

        var key = "factory-key";
        var value = "factory-value";
        var expiration = TimeSpan.FromMinutes(5);
        var factoryCallCount = 0;

        Func<Task<string>> factory = () =>
        {
            factoryCallCount++;
            return Task.FromResult(value);
        };

        // Act
        var firstResult = await cacheService.GetOrSetAsync(key, factory, expiration);
        var secondResult = await cacheService.GetOrSetAsync(key, factory, expiration);

        // Assert
        Assert.Equal(value, firstResult);
        Assert.Equal(value, secondResult);
        Assert.Equal(1, factoryCallCount); // Factory should only be called once
    }

    /// <summary>
    /// Tests cache removal functionality
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_RemoveAsync_RemovesEntry()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new LoggerFactory().CreateLogger<MemoryCacheService>();
        var cacheService = new MemoryCacheService(memoryCache, logger);

        var key = "remove-key";
        var value = "remove-value";
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        await cacheService.SetAsync(key, value, expiration);
        var beforeRemoval = await cacheService.GetAsync<string>(key);

        await cacheService.RemoveAsync(key);
        var afterRemoval = await cacheService.GetAsync<string>(key);

        // Assert
        Assert.Equal(value, beforeRemoval);
        Assert.Null(afterRemoval);
    }

    /// <summary>
    /// Tests cache key constants are properly formatted
    /// </summary>
    [Fact]
    public void CacheKeys_FormatCorrectly_ReturnsExpectedKeys()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";

        // Act
        var userIdKey = string.Format(CacheKeys.USER_BY_ID, userId);
        var emailKey = string.Format(CacheKeys.USER_BY_EMAIL, email);

        // Assert
        Assert.Contains(userId.ToString(), userIdKey);
        Assert.Contains(email, emailKey);
        Assert.StartsWith("user:", userIdKey);
        Assert.StartsWith("user:", emailKey);
    }

    /// <summary>
    /// Tests cache expiration times are reasonable
    /// </summary>
    [Fact]
    public void CacheKeys_Expiration_HasReasonableValues()
    {
        // Assert
        Assert.True(CacheKeys.Expiration.Short < CacheKeys.Expiration.Medium);
        Assert.True(CacheKeys.Expiration.Medium < CacheKeys.Expiration.Long);
        Assert.True(CacheKeys.Expiration.Long < CacheKeys.Expiration.ExtraLong);

        Assert.True(CacheKeys.Expiration.Short.TotalMinutes >= 1);
        Assert.True(CacheKeys.Expiration.ExtraLong.TotalHours <= 48);
    }

    /// <summary>
    /// Tests cache patterns for pattern matching
    /// </summary>
    [Fact]
    public void CacheKeys_Patterns_MatchExpectedFormat()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var userPattern = string.Format(CacheKeys.Patterns.USER_SPECIFIC, userId);

        // Assert
        Assert.Contains(userId.ToString(), userPattern);
        Assert.Contains(".*", userPattern);
    }
}
