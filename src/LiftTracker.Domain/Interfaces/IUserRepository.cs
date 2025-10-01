using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">User to create</param>
    /// <returns>Created user with generated ID</returns>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">User to update</param>
    /// <returns>Updated user</returns>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Deletes a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a user exists by email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>True if user exists</returns>
    Task<bool> ExistsAsync(string email);
}
