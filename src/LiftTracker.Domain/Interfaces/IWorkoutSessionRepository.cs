using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for WorkoutSession entity operations
/// </summary>
public interface IWorkoutSessionRepository
{
    /// <summary>
    /// Gets a workout session by ID for a specific user
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>Workout session or null if not found</returns>
    Task<WorkoutSession?> GetByIdAsync(Guid id, Guid userId);

    /// <summary>
    /// Gets all workout sessions for a user with optional date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of workout sessions</returns>
    Task<IEnumerable<WorkoutSession>> GetByUserAsync(Guid userId, DateOnly? startDate = null, DateOnly? endDate = null);

    /// <summary>
    /// Gets a workout session for a specific user and date
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="date">Workout date</param>
    /// <returns>Workout session or null if not found</returns>
    Task<WorkoutSession?> GetByUserAndDateAsync(Guid userId, DateOnly date);

    /// <summary>
    /// Creates a new workout session
    /// </summary>
    /// <param name="workoutSession">Session to create</param>
    /// <returns>Created session with generated ID</returns>
    Task<WorkoutSession> CreateAsync(WorkoutSession workoutSession);

    /// <summary>
    /// Updates an existing workout session
    /// </summary>
    /// <param name="workoutSession">Session to update</param>
    /// <returns>Updated session</returns>
    Task<WorkoutSession> UpdateAsync(WorkoutSession workoutSession);

    /// <summary>
    /// Deletes a workout session by ID for a specific user
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, Guid userId);

    /// <summary>
    /// Checks if a workout session exists for a user on a specific date
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="date">Workout date</param>
    /// <returns>True if session exists</returns>
    Task<bool> ExistsForDateAsync(Guid userId, DateOnly date);
}