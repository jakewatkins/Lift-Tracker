using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for MetconWorkout entity operations
/// </summary>
public interface IMetconWorkoutRepository
{
    /// <summary>
    /// Gets a metcon workout by ID (admin access)
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Metcon workout or null if not found</returns>
    Task<MetconWorkout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a metcon workout by ID for a specific user
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Metcon workout or null if not found</returns>
    Task<MetconWorkout?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all metcon workouts for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by Order</returns>
    Task<IEnumerable<MetconWorkout>> GetByWorkoutSessionAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all metcon workouts for a workout session (alias for service compatibility)
    /// </summary>
    /// <param name="sessionId">Workout session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by Order</returns>
    Task<IEnumerable<MetconWorkout>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all metcon workouts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    Task<IEnumerable<MetconWorkout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metcon workouts for a user by metcon type with date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="metconTypeId">Metcon type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    Task<IEnumerable<MetconWorkout>> GetByUserAndTypeAsync(Guid userId, int metconTypeId, DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metcon workouts for a user by metcon type (alias for service compatibility)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="metconTypeId">Metcon type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    Task<IEnumerable<MetconWorkout>> GetByUserAndMetconTypeAsync(Guid userId, int metconTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metcon workouts for a user within a date range
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    Task<IEnumerable<MetconWorkout>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent metcon workouts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Maximum number of workouts to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recent metcon workouts</returns>
    Task<IEnumerable<MetconWorkout>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new metcon workout
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created workout with generated ID</returns>
    Task<MetconWorkout> CreateAsync(MetconWorkout metconWorkout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new metcon workout (alias for service compatibility)
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created workout with generated ID</returns>
    Task<MetconWorkout> AddAsync(MetconWorkout metconWorkout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing metcon workout
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated workout</returns>
    Task<MetconWorkout> UpdateAsync(MetconWorkout metconWorkout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a metcon workout by ID for a specific user
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the maximum order value for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Maximum order value or 0 if no metcon workouts exist</returns>
    Task<int> GetMaxOrderAsync(Guid workoutSessionId, CancellationToken cancellationToken = default);
}
