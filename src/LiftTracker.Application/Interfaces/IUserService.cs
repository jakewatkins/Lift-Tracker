using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Interfaces;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user or updates an existing user after Google OAuth authentication
    /// </summary>
    /// <param name="email">User's email from Google</param>
    /// <param name="name">User's display name from Google</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created or updated user</returns>
    Task<User> CreateOrUpdateUserAsync(string email, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user's last login date
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user's profile information
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="name">Updated display name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user</returns>
    Task<User> UpdateUserProfileAsync(Guid userId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user and all associated data
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if user not found</returns>
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
