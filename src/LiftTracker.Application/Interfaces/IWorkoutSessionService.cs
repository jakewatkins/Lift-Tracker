using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Interfaces;

/// <summary>
/// Service interface for workout session management operations
/// </summary>
public interface IWorkoutSessionService
{
    /// <summary>
    /// Gets a workout session by its unique identifier
    /// </summary>
    /// <param name="sessionId">The session's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workout session if found, null otherwise</returns>
    Task<WorkoutSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all workout sessions for a specific user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of workout sessions</returns>
    Task<IEnumerable<WorkoutSession>> GetSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workout sessions for a user within a date range
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of workout sessions within the date range</returns>
    Task<IEnumerable<WorkoutSession>> GetSessionsByUserAndDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workout session for a user on a specific date
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="date">The workout date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workout session if found, null otherwise</returns>
    Task<WorkoutSession?> GetSessionByUserAndDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new workout session
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="date">The workout date</param>
    /// <param name="notes">Optional session notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created workout session</returns>
    Task<WorkoutSession> CreateSessionAsync(Guid userId, DateOnly date, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing workout session
    /// </summary>
    /// <param name="sessionId">The session's unique identifier</param>
    /// <param name="date">Updated workout date</param>
    /// <param name="notes">Updated session notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated workout session</returns>
    Task<WorkoutSession> UpdateSessionAsync(Guid sessionId, DateOnly date, string? notes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workout session and all associated lifts and workouts
    /// </summary>
    /// <param name="sessionId">The session's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if session not found</returns>
    Task<bool> DeleteSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent workout session for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The most recent workout session if found, null otherwise</returns>
    Task<WorkoutSession?> GetMostRecentSessionAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workout sessions with statistics for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="limit">Maximum number of sessions to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent workout sessions</returns>
    Task<IEnumerable<WorkoutSession>> GetRecentSessionsAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default);
}
