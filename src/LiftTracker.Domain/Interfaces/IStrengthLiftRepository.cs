using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for StrengthLift entity operations
/// </summary>
public interface IStrengthLiftRepository
{
    /// <summary>
    /// Gets a strength lift by ID (admin access)
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Strength lift or null if not found</returns>
    Task<StrengthLift?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a strength lift by ID for a specific user
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Strength lift or null if not found</returns>
    Task<StrengthLift?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all strength lifts for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by Order</returns>
    Task<IEnumerable<StrengthLift>> GetByWorkoutSessionAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all strength lifts for a workout session (alias for service compatibility)
    /// </summary>
    /// <param name="sessionId">Workout session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by Order</returns>
    Task<IEnumerable<StrengthLift>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all strength lifts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    Task<IEnumerable<StrengthLift>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets strength lifts for a user by exercise type with date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    Task<IEnumerable<StrengthLift>> GetByUserAndExerciseAsync(Guid userId, int exerciseTypeId, DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets strength lifts for a user by exercise type (alias for service compatibility)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    Task<IEnumerable<StrengthLift>> GetByUserAndExerciseTypeAsync(Guid userId, int exerciseTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets strength lifts for a user within a date range
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    Task<IEnumerable<StrengthLift>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets personal record for a user and exercise type
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Personal record lift or null if none found</returns>
    Task<StrengthLift?> GetPersonalRecordAsync(Guid userId, int exerciseTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent strength lifts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Maximum number of lifts to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recent strength lifts</returns>
    Task<IEnumerable<StrengthLift>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new strength lift
    /// </summary>
    /// <param name="strengthLift">Lift to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created lift with generated ID</returns>
    Task<StrengthLift> CreateAsync(StrengthLift strengthLift, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new strength lift (alias for service compatibility)
    /// </summary>
    /// <param name="strengthLift">Lift to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created lift with generated ID</returns>
    Task<StrengthLift> AddAsync(StrengthLift strengthLift, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing strength lift
    /// </summary>
    /// <param name="strengthLift">Lift to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated lift</returns>
    Task<StrengthLift> UpdateAsync(StrengthLift strengthLift, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a strength lift by ID for a specific user
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the maximum order value for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Maximum order value or 0 if no lifts exist</returns>
    Task<int> GetMaxOrderAsync(Guid workoutSessionId, CancellationToken cancellationToken = default);
}
