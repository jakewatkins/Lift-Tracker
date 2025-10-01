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
    /// <returns>Metcon type or null if not found</returns>
    Task<MetconType?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all active metcon types
    /// </summary>
    /// <returns>List of active metcon types</returns>
    Task<IEnumerable<MetconType>> GetActiveAsync();

    /// <summary>
    /// Gets all metcon types (including inactive)
    /// </summary>
    /// <returns>List of all metcon types</returns>
    Task<IEnumerable<MetconType>> GetAllAsync();

    /// <summary>
    /// Creates a new metcon type
    /// </summary>
    /// <param name="metconType">Metcon type to create</param>
    /// <returns>Created metcon type with generated ID</returns>
    Task<MetconType> CreateAsync(MetconType metconType);

    /// <summary>
    /// Updates an existing metcon type
    /// </summary>
    /// <param name="metconType">Metcon type to update</param>
    /// <returns>Updated metcon type</returns>
    Task<MetconType> UpdateAsync(MetconType metconType);

    /// <summary>
    /// Deactivates a metcon type (soft delete)
    /// </summary>
    /// <param name="id">Metcon type ID</param>
    /// <returns>True if deactivated, false if not found</returns>
    Task<bool> DeactivateAsync(int id);

    /// <summary>
    /// Checks if a metcon type exists by name
    /// </summary>
    /// <param name="name">Metcon type name</param>
    /// <returns>True if metcon type exists</returns>
    Task<bool> ExistsByNameAsync(string name);
}
