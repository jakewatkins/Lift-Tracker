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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Movement type or null if not found</returns>
    Task<MovementType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active movement types
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active movement types</returns>
    Task<IEnumerable<MovementType>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets movement types by category
    /// </summary>
    /// <param name="category">Movement category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of movement types in the category</returns>
    Task<IEnumerable<MovementType>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets movement types by measurement type
    /// </summary>
    /// <param name="measurementType">Measurement type (Reps or Distance)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of movement types with the specified measurement type</returns>
    Task<IEnumerable<MovementType>> GetByMeasurementTypeAsync(string measurementType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all movement types (including inactive)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all movement types</returns>
    Task<IEnumerable<MovementType>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new movement type
    /// </summary>
    /// <param name="movementType">Movement type to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created movement type with generated ID</returns>
    Task<MovementType> CreateAsync(MovementType movementType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing movement type
    /// </summary>
    /// <param name="movementType">Movement type to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated movement type</returns>
    Task<MovementType> UpdateAsync(MovementType movementType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a movement type (soft delete)
    /// </summary>
    /// <param name="id">Movement type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deactivated, false if not found</returns>
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a movement type exists by name
    /// </summary>
    /// <param name="name">Movement type name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if movement type exists</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
