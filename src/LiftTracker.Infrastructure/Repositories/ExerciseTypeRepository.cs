using Microsoft.EntityFrameworkCore;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using LiftTracker.Infrastructure.Data;

namespace LiftTracker.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ExerciseType entity operations
/// </summary>
public class ExerciseTypeRepository : IExerciseTypeRepository
{
    private readonly LiftTrackerDbContext _context;

    public ExerciseTypeRepository(LiftTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets an exercise type by ID
    /// </summary>
    /// <param name="id">Exercise type ID</param>
    /// <returns>Exercise type or null if not found</returns>
    public async Task<ExerciseType?> GetByIdAsync(int id)
    {
        return await _context.ExerciseTypes
            .FirstOrDefaultAsync(et => et.Id == id);
    }

    /// <summary>
    /// Gets all active exercise types
    /// </summary>
    /// <returns>List of active exercise types</returns>
    public async Task<IEnumerable<ExerciseType>> GetActiveAsync()
    {
        return await _context.ExerciseTypes
            .Where(et => et.IsActive)
            .OrderBy(et => et.Category)
            .ThenBy(et => et.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets exercise types by category
    /// </summary>
    /// <param name="category">Exercise category</param>
    /// <returns>List of exercise types in the category</returns>
    public async Task<IEnumerable<ExerciseType>> GetByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty", nameof(category));

        return await _context.ExerciseTypes
            .Where(et => et.Category == category && et.IsActive)
            .OrderBy(et => et.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all exercise types (including inactive)
    /// </summary>
    /// <returns>List of all exercise types</returns>
    public async Task<IEnumerable<ExerciseType>> GetAllAsync()
    {
        return await _context.ExerciseTypes
            .OrderBy(et => et.Category)
            .ThenBy(et => et.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new exercise type
    /// </summary>
    /// <param name="exerciseType">Exercise type to create</param>
    /// <returns>Created exercise type with generated ID</returns>
    public async Task<ExerciseType> CreateAsync(ExerciseType exerciseType)
    {
        if (exerciseType == null)
            throw new ArgumentNullException(nameof(exerciseType));

        // Validate business rules
        await ValidateExerciseTypeAsync(exerciseType);

        _context.ExerciseTypes.Add(exerciseType);
        await _context.SaveChangesAsync();

        return exerciseType;
    }

    /// <summary>
    /// Updates an existing exercise type
    /// </summary>
    /// <param name="exerciseType">Exercise type to update</param>
    /// <returns>Updated exercise type</returns>
    public async Task<ExerciseType> UpdateAsync(ExerciseType exerciseType)
    {
        if (exerciseType == null)
            throw new ArgumentNullException(nameof(exerciseType));

        var existingExerciseType = await _context.ExerciseTypes
            .FirstOrDefaultAsync(et => et.Id == exerciseType.Id);

        if (existingExerciseType == null)
            throw new InvalidOperationException($"Exercise type {exerciseType.Id} not found");

        // Validate business rules (only check name uniqueness if name changed)
        if (!string.Equals(existingExerciseType.Name, exerciseType.Name, StringComparison.OrdinalIgnoreCase))
        {
            await ValidateExerciseTypeAsync(exerciseType);
        }

        // Update properties
        existingExerciseType.Name = exerciseType.Name;
        existingExerciseType.Category = exerciseType.Category;
        existingExerciseType.IsActive = exerciseType.IsActive;

        await _context.SaveChangesAsync();

        return existingExerciseType;
    }

    /// <summary>
    /// Deactivates an exercise type (soft delete)
    /// </summary>
    /// <param name="id">Exercise type ID</param>
    /// <returns>True if deactivated, false if not found</returns>
    public async Task<bool> DeactivateAsync(int id)
    {
        var exerciseType = await _context.ExerciseTypes
            .FirstOrDefaultAsync(et => et.Id == id);

        if (exerciseType == null)
            return false;

        // Check if exercise type is used in any strength lifts
        var isUsed = await _context.StrengthLifts
            .AnyAsync(sl => sl.ExerciseTypeId == id);

        if (isUsed)
        {
            // Soft delete - deactivate instead of hard delete
            exerciseType.IsActive = false;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Hard delete if not used
            _context.ExerciseTypes.Remove(exerciseType);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    /// <summary>
    /// Checks if an exercise type exists by name
    /// </summary>
    /// <param name="name">Exercise type name</param>
    /// <returns>True if exercise type exists</returns>
    public async Task<bool> ExistsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _context.ExerciseTypes
            .AnyAsync(et => et.Name == name);
    }

    /// <summary>
    /// Validates exercise type business rules
    /// </summary>
    /// <param name="exerciseType">Exercise type to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    private async Task ValidateExerciseTypeAsync(ExerciseType exerciseType)
    {
        if (string.IsNullOrWhiteSpace(exerciseType.Name))
            throw new ArgumentException("Exercise type name is required", nameof(exerciseType.Name));

        if (exerciseType.Name.Length > 100)
            throw new ArgumentException("Exercise type name cannot exceed 100 characters", nameof(exerciseType.Name));

        if (string.IsNullOrWhiteSpace(exerciseType.Category))
            throw new ArgumentException("Exercise type category is required", nameof(exerciseType.Category));

        if (exerciseType.Category.Length > 50)
            throw new ArgumentException("Exercise type category cannot exceed 50 characters", nameof(exerciseType.Category));

        // Check for duplicate names
        var existingWithSameName = await _context.ExerciseTypes
            .FirstOrDefaultAsync(et => et.Name == exerciseType.Name && et.Id != exerciseType.Id);

        if (existingWithSameName != null)
            throw new ArgumentException($"An exercise type with the name '{exerciseType.Name}' already exists", nameof(exerciseType.Name));
    }
}
