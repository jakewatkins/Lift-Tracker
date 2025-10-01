using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for MetconMovement entity operations
/// </summary>
public interface IMetconMovementRepository
{
    /// <summary>
    /// Gets a metcon movement by ID for a specific user
    /// </summary>
    /// <param name="id">Movement ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>Metcon movement or null if not found</returns>
    Task<MetconMovement?> GetByIdAsync(Guid id, Guid userId);

    /// <summary>
    /// Gets all movements for a metcon workout
    /// </summary>
    /// <param name="metconWorkoutId">Metcon workout ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>List of movements ordered by Order</returns>
    Task<IEnumerable<MetconMovement>> GetByMetconWorkoutAsync(Guid metconWorkoutId, Guid userId);

    /// <summary>
    /// Gets metcon movements for a user by movement type with date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="movementTypeId">Movement type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of metcon movements ordered by date descending</returns>
    Task<IEnumerable<MetconMovement>> GetByUserAndMovementTypeAsync(Guid userId, int movementTypeId, DateOnly? startDate = null, DateOnly? endDate = null);

    /// <summary>
    /// Creates a new metcon movement
    /// </summary>
    /// <param name="metconMovement">Movement to create</param>
    /// <returns>Created movement with generated ID</returns>
    Task<MetconMovement> CreateAsync(MetconMovement metconMovement);

    /// <summary>
    /// Updates an existing metcon movement
    /// </summary>
    /// <param name="metconMovement">Movement to update</param>
    /// <returns>Updated movement</returns>
    Task<MetconMovement> UpdateAsync(MetconMovement metconMovement);

    /// <summary>
    /// Deletes a metcon movement by ID for a specific user
    /// </summary>
    /// <param name="id">Movement ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, Guid userId);

    /// <summary>
    /// Gets the maximum order value for a metcon workout
    /// </summary>
    /// <param name="metconWorkoutId">Metcon workout ID</param>
    /// <returns>Maximum order value or 0 if no movements exist</returns>
    Task<int> GetMaxOrderAsync(Guid metconWorkoutId);
}