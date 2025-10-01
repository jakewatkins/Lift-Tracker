using Microsoft.EntityFrameworkCore;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using LiftTracker.Infrastructure.Data;

namespace LiftTracker.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity operations
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly LiftTrackerDbContext _context;

    public UserRepository(LiftTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User entity or null if not found</returns>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>User entity or null if not found</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">User to create</param>
    /// <returns>Created user with generated ID</returns>
    public async Task<User> CreateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Ensure ID is generated
        if (user.Id == Guid.Empty)
            user.Id = Guid.NewGuid();

        // Set created date if not already set
        if (user.CreatedDate == default)
            user.CreatedDate = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">User to update</param>
    /// <returns>Updated user</returns>
    public async Task<User> UpdateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var existingUser = await _context.Users.FindAsync(user.Id);
        if (existingUser == null)
            throw new InvalidOperationException($"User with ID {user.Id} not found");

        // Update properties
        existingUser.Email = user.Email;
        existingUser.Name = user.Name;
        existingUser.LastLoginDate = user.LastLoginDate;

        _context.Users.Update(existingUser);
        await _context.SaveChangesAsync();

        return existingUser;
    }

    /// <summary>
    /// Deletes a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks if a user exists by email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>True if user exists</returns>
    public async Task<bool> ExistsAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }
}
