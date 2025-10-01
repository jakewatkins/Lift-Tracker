using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for WorkoutSession entity operations
/// </summary>
public interface IWorkoutSessionRepository
{
    /// <summary>
    /// Gets a workout session by ID (admin access)
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workout session or null if not found</returns>
    Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workout session by ID for a specific user
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workout session or null if not found</returns>
    Task<WorkoutSession?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all workout sessions for a user with optional date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workout sessions</returns>
    Task<IEnumerable<WorkoutSession>> GetByUserAsync(Guid userId, DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all workout sessions for a user (alias for service compatibility)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workout sessions</returns>
    Task<IEnumerable<WorkoutSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workout sessions for a user within a date range
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workout sessions</returns>
    Task<IEnumerable<WorkoutSession>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workout session for a specific user and date
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="date">Workout date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workout session or null if not found</returns>
    Task<WorkoutSession?> GetByUserAndDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent workout session for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Most recent workout session or null if none found</returns>
    Task<WorkoutSession?> GetMostRecentByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent workout sessions for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Maximum number of sessions to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recent workout sessions</returns>
    Task<IEnumerable<WorkoutSession>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new workout session
    /// </summary>
    /// <param name="workoutSession">Session to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created session with generated ID</returns>
    Task<WorkoutSession> CreateAsync(WorkoutSession workoutSession, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new workout session (alias for service compatibility)
    /// </summary>
    /// <param name="workoutSession">Session to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created session with generated ID</returns>
    Task<WorkoutSession> AddAsync(WorkoutSession workoutSession, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing workout session
    /// </summary>
    /// <param name="workoutSession">Session to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated session</returns>
    Task<WorkoutSession> UpdateAsync(WorkoutSession workoutSession, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workout session by ID for a specific user
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a workout session exists for a user on a specific date
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="date">Workout date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if session exists</returns>
    Task<bool> ExistsForDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default);
}
