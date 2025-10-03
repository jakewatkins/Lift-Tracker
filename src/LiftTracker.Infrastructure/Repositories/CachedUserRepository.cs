using Microsoft.EntityFrameworkCore;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using LiftTracker.Infrastructure.Data;
using LiftTracker.Infrastructure.Caching;

namespace LiftTracker.Infrastructure.Repositories;

/// <summary>
/// Cached repository implementation for User entity operations
/// </summary>
public class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _baseRepository;
    private readonly ICacheService _cacheService;

    public CachedUserRepository(IUserRepository baseRepository, ICacheService cacheService)
    {
        _baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <summary>
    /// Gets a user by their unique identifier with caching
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = string.Format(CacheKeys.USER_BY_ID, id);

        var cachedUser = await _cacheService.GetAsync<User>(cacheKey, cancellationToken);
        if (cachedUser != null)
        {
            return cachedUser;
        }

        var user = await _baseRepository.GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            await _cacheService.SetAsync(cacheKey, user, CacheKeys.Expiration.UserData, cancellationToken);
        }

        return user;
    }

    /// <summary>
    /// Gets a user by their email address with caching
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var cacheKey = string.Format(CacheKeys.USER_BY_EMAIL, email.ToLowerInvariant());

        var cachedUser = await _cacheService.GetAsync<User>(cacheKey, cancellationToken);
        if (cachedUser != null)
        {
            return cachedUser;
        }

        var user = await _baseRepository.GetByEmailAsync(email, cancellationToken);
        if (user != null)
        {
            await _cacheService.SetAsync(cacheKey, user, CacheKeys.Expiration.UserData, cancellationToken);
        }

        return user;
    }

    /// <summary>
    /// Creates a new user and invalidates related cache
    /// </summary>
    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        var result = await _baseRepository.CreateAsync(user, cancellationToken);

        // Invalidate cache for this user
        await InvalidateUserCacheAsync(result.Id, result.Email, cancellationToken);

        return result;
    }

    /// <summary>
    /// Updates an existing user and invalidates related cache
    /// </summary>
    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        // Get original user to invalidate old email cache if email changed
        var originalUser = await _baseRepository.GetByIdAsync(user.Id, cancellationToken);

        var result = await _baseRepository.UpdateAsync(user, cancellationToken);

        // Invalidate cache for this user
        await InvalidateUserCacheAsync(result.Id, result.Email, cancellationToken);

        // If email changed, also invalidate old email cache
        if (originalUser != null && !string.Equals(originalUser.Email, result.Email, StringComparison.OrdinalIgnoreCase))
        {
            var oldEmailCacheKey = string.Format(CacheKeys.USER_BY_EMAIL, originalUser.Email.ToLowerInvariant());
            await _cacheService.RemoveAsync(oldEmailCacheKey, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Deletes a user and invalidates related cache
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Get user before deletion to invalidate email cache
        var user = await _baseRepository.GetByIdAsync(id, cancellationToken);

        var result = await _baseRepository.DeleteAsync(id, cancellationToken);

        if (result && user != null)
        {
            await InvalidateUserCacheAsync(id, user.Email, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Checks if a user exists by email with caching
    /// </summary>
    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        // Use the cached GetByEmailAsync method
        var user = await GetByEmailAsync(email, cancellationToken);
        return user != null;
    }

    /// <summary>
    /// Invalidates all cache entries for a specific user
    /// </summary>
    private async Task InvalidateUserCacheAsync(Guid userId, string email, CancellationToken cancellationToken)
    {
        // Remove specific cache entries
        var userIdCacheKey = string.Format(CacheKeys.USER_BY_ID, userId);
        var emailCacheKey = string.Format(CacheKeys.USER_BY_EMAIL, email.ToLowerInvariant());

        await _cacheService.RemoveAsync(userIdCacheKey, cancellationToken);
        await _cacheService.RemoveAsync(emailCacheKey, cancellationToken);

        // Remove user-specific pattern cache (workouts, progress, etc.)
        var userPattern = string.Format(CacheKeys.Patterns.USER_SPECIFIC, userId);
        await _cacheService.RemoveByPatternAsync(userPattern, cancellationToken);
    }
}
