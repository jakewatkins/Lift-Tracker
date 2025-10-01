using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for MetconWorkout entity operations
/// </summary>
public interface IMetconWorkoutRepository
{
    /// <summary>
    /// Gets a metcon workout by ID for a specific user
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>Metcon workout or null if not found</returns>
    Task<MetconWorkout?> GetByIdAsync(Guid id, Guid userId);

    /// <summary>
    /// Gets all metcon workouts for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>List of metcon workouts ordered by Order</returns>
    Task<IEnumerable<MetconWorkout>> GetByWorkoutSessionAsync(Guid workoutSessionId, Guid userId);

    /// <summary>
    /// Gets metcon workouts for a user by metcon type with date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="metconTypeId">Metcon type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    Task<IEnumerable<MetconWorkout>> GetByUserAndTypeAsync(Guid userId, int metconTypeId, DateOnly? startDate = null, DateOnly? endDate = null);

    /// <summary>
    /// Creates a new metcon workout
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to create</param>
    /// <returns>Created workout with generated ID</returns>
    Task<MetconWorkout> CreateAsync(MetconWorkout metconWorkout);

    /// <summary>
    /// Updates an existing metcon workout
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to update</param>
    /// <returns>Updated workout</returns>
    Task<MetconWorkout> UpdateAsync(MetconWorkout metconWorkout);

    /// <summary>
    /// Deletes a metcon workout by ID for a specific user
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, Guid userId);

    /// <summary>
    /// Gets the maximum order value for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <returns>Maximum order value or 0 if no metcon workouts exist</returns>
    Task<int> GetMaxOrderAsync(Guid workoutSessionId);
}