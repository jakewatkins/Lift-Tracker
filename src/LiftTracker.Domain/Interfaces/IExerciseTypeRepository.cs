using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Interfaces;

/// <summary>
/// Repository interface for ExerciseType entity operations
/// </summary>
public interface IExerciseTypeRepository
{
    /// <summary>
    /// Gets an exercise type by ID
    /// </summary>
    /// <param name="id">Exercise type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exercise type or null if not found</returns>
    Task<ExerciseType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active exercise types
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active exercise types</returns>
    Task<IEnumerable<ExerciseType>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets exercise types by category
    /// </summary>
    /// <param name="category">Exercise category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of exercise types in the category</returns>
    Task<IEnumerable<ExerciseType>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all exercise types (including inactive)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all exercise types</returns>
    Task<IEnumerable<ExerciseType>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new exercise type
    /// </summary>
    /// <param name="exerciseType">Exercise type to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created exercise type with generated ID</returns>
    Task<ExerciseType> CreateAsync(ExerciseType exerciseType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing exercise type
    /// </summary>
    /// <param name="exerciseType">Exercise type to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated exercise type</returns>
    Task<ExerciseType> UpdateAsync(ExerciseType exerciseType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates an exercise type (soft delete)
    /// </summary>
    /// <param name="id">Exercise type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deactivated, false if not found</returns>
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an exercise type exists by name
    /// </summary>
    /// <param name="name">Exercise type name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exercise type exists</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
