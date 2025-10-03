using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Interfaces;

/// <summary>
/// Service interface for metcon workout management operations
/// </summary>
public interface IMetconWorkoutService
{
    /// <summary>
    /// Gets a metcon workout by its unique identifier
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The metcon workout if found, null otherwise</returns>
    Task<MetconWorkout?> GetWorkoutByIdAsync(Guid workoutId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all metcon workouts for a specific workout session
    /// </summary>
    /// <param name="sessionId">The session's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of metcon workouts</returns>
    Task<IEnumerable<MetconWorkout>> GetWorkoutsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all metcon workouts for a user across all sessions
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of metcon workouts</returns>
    Task<IEnumerable<MetconWorkout>> GetWorkoutsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metcon workouts for a specific metcon type and user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="metconTypeId">The metcon type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of metcon workouts for the metcon type</returns>
    Task<IEnumerable<MetconWorkout>> GetWorkoutsByUserAndMetconTypeAsync(
        Guid userId,
        int metconTypeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metcon workouts within a date range for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of metcon workouts within the date range</returns>
    Task<IEnumerable<MetconWorkout>> GetWorkoutsByUserAndDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new metcon workout
    /// </summary>
    /// <param name="sessionId">The workout session identifier</param>
    /// <param name="metconTypeId">The metcon type identifier</param>
    /// <param name="totalTime">Total time in minutes (optional for some types)</param>
    /// <param name="roundsCompleted">Number of rounds completed (optional for some types)</param>
    /// <param name="notes">Optional workout notes</param>
    /// <param name="order">Order within workout session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created metcon workout</returns>
    Task<MetconWorkout> CreateWorkoutAsync(
        Guid sessionId,
        int metconTypeId,
        decimal? totalTime = null,
        int? roundsCompleted = null,
        string? notes = null,
        int order = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing metcon workout
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier</param>
    /// <param name="metconTypeId">Updated metcon type identifier</param>
    /// <param name="totalTime">Updated total time in minutes</param>
    /// <param name="roundsCompleted">Updated number of rounds completed</param>
    /// <param name="notes">Updated workout notes</param>
    /// <param name="order">Updated order within session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated metcon workout</returns>
    Task<MetconWorkout> UpdateWorkoutAsync(
        Guid workoutId,
        int metconTypeId,
        decimal? totalTime,
        int? roundsCompleted,
        string? notes,
        int order,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a metcon workout and all associated movements
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if workout not found</returns>
    Task<bool> DeleteWorkoutAsync(Guid workoutId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a movement to a metcon workout
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier</param>
    /// <param name="movementTypeId">The movement type identifier</param>
    /// <param name="reps">Number of repetitions per round</param>
    /// <param name="weight">Weight used (optional)</param>
    /// <param name="distance">Distance covered (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created metcon movement</returns>
    Task<MetconMovement> AddMovementToWorkoutAsync(
        Guid workoutId,
        int movementTypeId,
        int reps,
        decimal? weight = null,
        decimal? distance = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all movements for a specific metcon workout
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of metcon movements</returns>
    Task<IEnumerable<MetconMovement>> GetMovementsByWorkoutIdAsync(Guid workoutId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent metcon workouts for a user for progress tracking
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="limit">Maximum number of workouts to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent metcon workouts</returns>
    Task<IEnumerable<MetconWorkout>> GetRecentWorkoutsAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default);
}
