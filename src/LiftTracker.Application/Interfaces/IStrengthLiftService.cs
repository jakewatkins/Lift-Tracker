using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Interfaces;

/// <summary>
/// Service interface for strength lift management operations
/// </summary>
public interface IStrengthLiftService
{
    /// <summary>
    /// Gets a strength lift by its unique identifier
    /// </summary>
    /// <param name="liftId">The lift's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The strength lift if found, null otherwise</returns>
    Task<StrengthLift?> GetLiftByIdAsync(Guid liftId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all strength lifts for a specific workout session
    /// </summary>
    /// <param name="sessionId">The session's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of strength lifts</returns>
    Task<IEnumerable<StrengthLift>> GetLiftsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all strength lifts for a user across all sessions
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of strength lifts</returns>
    Task<IEnumerable<StrengthLift>> GetLiftsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets strength lifts for a specific exercise type and user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="exerciseTypeId">The exercise type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of strength lifts for the exercise type</returns>
    Task<IEnumerable<StrengthLift>> GetLiftsByUserAndExerciseTypeAsync(
        Guid userId,
        int exerciseTypeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets strength lifts within a date range for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of strength lifts within the date range</returns>
    Task<IEnumerable<StrengthLift>> GetLiftsByUserAndDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new strength lift
    /// </summary>
    /// <param name="sessionId">The workout session identifier</param>
    /// <param name="exerciseTypeId">The exercise type identifier</param>
    /// <param name="setStructure">The set structure type</param>
    /// <param name="sets">Number of sets (optional for some structures)</param>
    /// <param name="reps">Number of repetitions (optional for some structures)</param>
    /// <param name="weight">Weight lifted</param>
    /// <param name="additionalWeight">Additional weight for bodyweight exercises</param>
    /// <param name="duration">Set duration in minutes</param>
    /// <param name="restPeriod">Rest between sets in minutes</param>
    /// <param name="comments">Optional lift comments</param>
    /// <param name="order">Order within workout session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created strength lift</returns>
    Task<StrengthLift> CreateLiftAsync(
        Guid sessionId,
        int exerciseTypeId,
        string setStructure,
        int? sets = null,
        int? reps = null,
        decimal weight = 0,
        decimal? additionalWeight = null,
        decimal? duration = null,
        decimal? restPeriod = null,
        string? comments = null,
        int order = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing strength lift
    /// </summary>
    /// <param name="liftId">The lift's unique identifier</param>
    /// <param name="exerciseTypeId">Updated exercise type identifier</param>
    /// <param name="setStructure">Updated set structure type</param>
    /// <param name="sets">Updated number of sets</param>
    /// <param name="reps">Updated number of repetitions</param>
    /// <param name="weight">Updated weight lifted</param>
    /// <param name="additionalWeight">Updated additional weight</param>
    /// <param name="duration">Updated duration</param>
    /// <param name="restPeriod">Updated rest period</param>
    /// <param name="comments">Updated lift comments</param>
    /// <param name="order">Updated order within session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated strength lift</returns>
    Task<StrengthLift> UpdateLiftAsync(
        Guid liftId,
        int exerciseTypeId,
        string setStructure,
        int? sets,
        int? reps,
        decimal weight,
        decimal? additionalWeight,
        decimal? duration,
        decimal? restPeriod,
        string? comments,
        int order,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a strength lift
    /// </summary>
    /// <param name="liftId">The lift's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if lift not found</returns>
    Task<bool> DeleteLiftAsync(Guid liftId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the personal record (max weight) for a user and exercise type
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="exerciseTypeId">The exercise type identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The strength lift representing the personal record, null if none found</returns>
    Task<StrengthLift?> GetPersonalRecordAsync(Guid userId, int exerciseTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent strength lifts for a user for progress tracking
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="limit">Maximum number of lifts to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent strength lifts</returns>
    Task<IEnumerable<StrengthLift>> GetRecentLiftsAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default);
}
