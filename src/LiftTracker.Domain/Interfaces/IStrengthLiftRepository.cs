using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for StrengthLift entity operations
/// </summary>
public interface IStrengthLiftRepository
{
    /// <summary>
    /// Gets a strength lift by ID for a specific user
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>Strength lift or null if not found</returns>
    Task<StrengthLift?> GetByIdAsync(Guid id, Guid userId);

    /// <summary>
    /// Gets all strength lifts for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>List of strength lifts ordered by Order</returns>
    Task<IEnumerable<StrengthLift>> GetByWorkoutSessionAsync(Guid workoutSessionId, Guid userId);

    /// <summary>
    /// Gets strength lifts for a user by exercise type with date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    Task<IEnumerable<StrengthLift>> GetByUserAndExerciseAsync(Guid userId, int exerciseTypeId, DateOnly? startDate = null, DateOnly? endDate = null);

    /// <summary>
    /// Creates a new strength lift
    /// </summary>
    /// <param name="strengthLift">Lift to create</param>
    /// <returns>Created lift with generated ID</returns>
    Task<StrengthLift> CreateAsync(StrengthLift strengthLift);

    /// <summary>
    /// Updates an existing strength lift
    /// </summary>
    /// <param name="strengthLift">Lift to update</param>
    /// <returns>Updated lift</returns>
    Task<StrengthLift> UpdateAsync(StrengthLift strengthLift);

    /// <summary>
    /// Deletes a strength lift by ID for a specific user
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, Guid userId);

    /// <summary>
    /// Gets the maximum order value for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <returns>Maximum order value or 0 if no lifts exist</returns>
    Task<int> GetMaxOrderAsync(Guid workoutSessionId);
}