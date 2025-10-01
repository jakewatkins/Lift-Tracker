using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for MovementType entity operations
/// </summary>
public interface IMovementTypeRepository
{
    /// <summary>
    /// Gets a movement type by ID
    /// </summary>
    /// <param name="id">Movement type ID</param>
    /// <returns>Movement type or null if not found</returns>
    Task<MovementType?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all active movement types
    /// </summary>
    /// <returns>List of active movement types</returns>
    Task<IEnumerable<MovementType>> GetActiveAsync();

    /// <summary>
    /// Gets movement types by category
    /// </summary>
    /// <param name="category">Movement category</param>
    /// <returns>List of movement types in the category</returns>
    Task<IEnumerable<MovementType>> GetByCategoryAsync(string category);

    /// <summary>
    /// Gets movement types by measurement type
    /// </summary>
    /// <param name="measurementType">Measurement type (Reps or Distance)</param>
    /// <returns>List of movement types with the specified measurement type</returns>
    Task<IEnumerable<MovementType>> GetByMeasurementTypeAsync(string measurementType);

    /// <summary>
    /// Gets all movement types (including inactive)
    /// </summary>
    /// <returns>List of all movement types</returns>
    Task<IEnumerable<MovementType>> GetAllAsync();

    /// <summary>
    /// Creates a new movement type
    /// </summary>
    /// <param name="movementType">Movement type to create</param>
    /// <returns>Created movement type with generated ID</returns>
    Task<MovementType> CreateAsync(MovementType movementType);

    /// <summary>
    /// Updates an existing movement type
    /// </summary>
    /// <param name="movementType">Movement type to update</param>
    /// <returns>Updated movement type</returns>
    Task<MovementType> UpdateAsync(MovementType movementType);

    /// <summary>
    /// Deactivates a movement type (soft delete)
    /// </summary>
    /// <param name="id">Movement type ID</param>
    /// <returns>True if deactivated, false if not found</returns>
    Task<bool> DeactivateAsync(int id);

    /// <summary>
    /// Checks if a movement type exists by name
    /// </summary>
    /// <param name="name">Movement type name</param>
    /// <returns>True if movement type exists</returns>
    Task<bool> ExistsByNameAsync(string name);
}