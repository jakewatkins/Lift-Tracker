using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for MetconType entity operations
/// </summary>
public interface IMetconTypeRepository
{
    /// <summary>
    /// Gets a metcon type by ID
    /// </summary>
    /// <param name="id">Metcon type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Metcon type or null if not found</returns>
    Task<MetconType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active metcon types
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active metcon types</returns>
    Task<IEnumerable<MetconType>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all metcon types (including inactive)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all metcon types</returns>
    Task<IEnumerable<MetconType>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new metcon type
    /// </summary>
    /// <param name="metconType">Metcon type to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created metcon type with generated ID</returns>
    Task<MetconType> CreateAsync(MetconType metconType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing metcon type
    /// </summary>
    /// <param name="metconType">Metcon type to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated metcon type</returns>
    Task<MetconType> UpdateAsync(MetconType metconType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a metcon type (soft delete)
    /// </summary>
    /// <param name="id">Metcon type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deactivated, false if not found</returns>
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a metcon type exists by name
    /// </summary>
    /// <param name="name">Metcon type name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if metcon type exists</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
