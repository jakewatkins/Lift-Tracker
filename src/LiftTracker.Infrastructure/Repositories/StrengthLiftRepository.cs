using Microsoft.EntityFrameworkCore;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using LiftTracker.Infrastructure.Data;

namespace LiftTracker.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for StrengthLift entity operations
/// </summary>
public class StrengthLiftRepository : IStrengthLiftRepository
{
    private readonly LiftTrackerDbContext _context;

    public StrengthLiftRepository(LiftTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a strength lift by ID for a specific user
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>Strength lift or null if not found</returns>
    public async Task<StrengthLift?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Include(sl => sl.WorkoutSession)
            .FirstOrDefaultAsync(sl => sl.Id == id && sl.WorkoutSession.UserId == userId);
    }

    /// <summary>
    /// Gets all strength lifts for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>List of strength lifts ordered by Order</returns>
    public async Task<IEnumerable<StrengthLift>> GetByWorkoutSessionAsync(Guid workoutSessionId, Guid userId)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Where(sl => sl.WorkoutSessionId == workoutSessionId && sl.WorkoutSession.UserId == userId)
            .OrderBy(sl => sl.Order)
            .ToListAsync();
    }

    /// <summary>
    /// Gets strength lifts for a user by exercise type with date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    public async Task<IEnumerable<StrengthLift>> GetByUserAndExerciseAsync(Guid userId, int exerciseTypeId, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Include(sl => sl.WorkoutSession)
            .Where(sl => sl.WorkoutSession.UserId == userId && sl.ExerciseTypeId == exerciseTypeId);

        if (startDate.HasValue)
            query = query.Where(sl => sl.WorkoutSession.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(sl => sl.WorkoutSession.Date <= endDate.Value);

        return await query
            .OrderByDescending(sl => sl.WorkoutSession.Date)
            .ThenBy(sl => sl.Order)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new strength lift
    /// </summary>
    /// <param name="strengthLift">Lift to create</param>
    /// <returns>Created lift with generated ID</returns>
    public async Task<StrengthLift> CreateAsync(StrengthLift strengthLift)
    {
        if (strengthLift == null)
            throw new ArgumentNullException(nameof(strengthLift));

        // Ensure ID is generated
        if (strengthLift.Id == Guid.Empty)
            strengthLift.Id = Guid.NewGuid();

        // Validate business rules
        if (!strengthLift.IsValidWeight())
            throw new ArgumentException("Weight must use fractional increments of 0.25", nameof(strengthLift));

        if (!strengthLift.IsValidAdditionalWeight())
            throw new ArgumentException("Additional weight must use fractional increments of 0.25", nameof(strengthLift));

        if (!strengthLift.IsValidDuration())
            throw new ArgumentException("Duration must use fractional increments of 0.25", nameof(strengthLift));

        if (!strengthLift.IsValidRestPeriod())
            throw new ArgumentException("Rest period must use fractional increments of 0.25", nameof(strengthLift));

        if (!strengthLift.IsValidSets())
            throw new ArgumentException("Sets must be between 1 and 50", nameof(strengthLift));

        if (!strengthLift.IsValidReps())
            throw new ArgumentException("Reps must be between 1 and 500", nameof(strengthLift));

        _context.StrengthLifts.Add(strengthLift);
        await _context.SaveChangesAsync();

        return strengthLift;
    }

    /// <summary>
    /// Updates an existing strength lift
    /// </summary>
    /// <param name="strengthLift">Lift to update</param>
    /// <returns>Updated lift</returns>
    public async Task<StrengthLift> UpdateAsync(StrengthLift strengthLift)
    {
        if (strengthLift == null)
            throw new ArgumentNullException(nameof(strengthLift));

        var existingLift = await _context.StrengthLifts
            .Include(sl => sl.WorkoutSession)
            .FirstOrDefaultAsync(sl => sl.Id == strengthLift.Id);

        if (existingLift == null)
            throw new InvalidOperationException($"Strength lift with ID {strengthLift.Id} not found");

        // Validate business rules
        if (!strengthLift.IsValidWeight())
            throw new ArgumentException("Weight must use fractional increments of 0.25", nameof(strengthLift));

        if (!strengthLift.IsValidAdditionalWeight())
            throw new ArgumentException("Additional weight must use fractional increments of 0.25", nameof(strengthLift));

        if (!strengthLift.IsValidDuration())
            throw new ArgumentException("Duration must use fractional increments of 0.25", nameof(strengthLift));

        if (!strengthLift.IsValidRestPeriod())
            throw new ArgumentException("Rest period must use fractional increments of 0.25", nameof(strengthLift));

        if (!strengthLift.IsValidSets())
            throw new ArgumentException("Sets must be between 1 and 50", nameof(strengthLift));

        if (!strengthLift.IsValidReps())
            throw new ArgumentException("Reps must be between 1 and 500", nameof(strengthLift));

        // Update properties
        existingLift.ExerciseTypeId = strengthLift.ExerciseTypeId;
        existingLift.SetStructure = strengthLift.SetStructure;
        existingLift.Sets = strengthLift.Sets;
        existingLift.Reps = strengthLift.Reps;
        existingLift.Weight = strengthLift.Weight;
        existingLift.AdditionalWeight = strengthLift.AdditionalWeight;
        existingLift.Duration = strengthLift.Duration;
        existingLift.RestPeriod = strengthLift.RestPeriod;
        existingLift.Comments = strengthLift.Comments;
        existingLift.Order = strengthLift.Order;

        _context.StrengthLifts.Update(existingLift);
        await _context.SaveChangesAsync();

        return existingLift;
    }

    /// <summary>
    /// Deletes a strength lift by ID for a specific user
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var strengthLift = await _context.StrengthLifts
            .Include(sl => sl.WorkoutSession)
            .FirstOrDefaultAsync(sl => sl.Id == id && sl.WorkoutSession.UserId == userId);

        if (strengthLift == null)
            return false;

        _context.StrengthLifts.Remove(strengthLift);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Gets the maximum order value for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <returns>Maximum order value or 0 if no lifts exist</returns>
    public async Task<int> GetMaxOrderAsync(Guid workoutSessionId)
    {
        var maxOrder = await _context.StrengthLifts
            .Where(sl => sl.WorkoutSessionId == workoutSessionId)
            .MaxAsync(sl => (int?)sl.Order);

        return maxOrder ?? 0;
    }
}
