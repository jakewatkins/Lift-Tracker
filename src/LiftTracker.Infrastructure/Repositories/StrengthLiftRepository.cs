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
    /// Gets a strength lift by ID (admin access)
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Strength lift or null if not found</returns>
    public async Task<StrengthLift?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Include(sl => sl.WorkoutSession)
            .FirstOrDefaultAsync(sl => sl.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets a strength lift by ID for a specific user
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Strength lift or null if not found</returns>
    public async Task<StrengthLift?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Include(sl => sl.WorkoutSession)
            .FirstOrDefaultAsync(sl => sl.Id == id && sl.WorkoutSession.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Gets all strength lifts for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by Order</returns>
    public async Task<IEnumerable<StrengthLift>> GetByWorkoutSessionAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Where(sl => sl.WorkoutSessionId == workoutSessionId && sl.WorkoutSession.UserId == userId)
            .OrderBy(sl => sl.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all strength lifts for a workout session (alias for service compatibility)
    /// </summary>
    /// <param name="sessionId">Workout session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by Order</returns>
    public async Task<IEnumerable<StrengthLift>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Include(sl => sl.WorkoutSession)
            .Where(sl => sl.WorkoutSessionId == sessionId)
            .OrderBy(sl => sl.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all strength lifts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    public async Task<IEnumerable<StrengthLift>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Include(sl => sl.WorkoutSession)
            .Where(sl => sl.WorkoutSession.UserId == userId)
            .OrderByDescending(sl => sl.WorkoutSession.Date)
            .ThenBy(sl => sl.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets strength lifts for a user by exercise type with date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    public async Task<IEnumerable<StrengthLift>> GetByUserAndExerciseAsync(Guid userId, int exerciseTypeId, DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken cancellationToken = default)
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
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets strength lifts for a user by exercise type (alias for service compatibility)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    public async Task<IEnumerable<StrengthLift>> GetByUserAndExerciseTypeAsync(Guid userId, int exerciseTypeId, CancellationToken cancellationToken = default)
    {
        return await GetByUserAndExerciseAsync(userId, exerciseTypeId, null, null, cancellationToken);
    }

    /// <summary>
    /// Gets strength lifts for a user within a date range
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of strength lifts ordered by date descending</returns>
    public async Task<IEnumerable<StrengthLift>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Include(sl => sl.WorkoutSession)
            .Where(sl => sl.WorkoutSession.UserId == userId &&
                        sl.WorkoutSession.Date >= startDate &&
                        sl.WorkoutSession.Date <= endDate)
            .OrderByDescending(sl => sl.WorkoutSession.Date)
            .ThenBy(sl => sl.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets personal record for a user and exercise type
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Personal record lift or null if none found</returns>
    public async Task<StrengthLift?> GetPersonalRecordAsync(Guid userId, int exerciseTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Include(sl => sl.WorkoutSession)
            .Where(sl => sl.WorkoutSession.UserId == userId && sl.ExerciseTypeId == exerciseTypeId)
            .OrderByDescending(sl => sl.Weight + sl.AdditionalWeight)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets recent strength lifts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Maximum number of lifts to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recent strength lifts</returns>
    public async Task<IEnumerable<StrengthLift>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken cancellationToken = default)
    {
        return await _context.StrengthLifts
            .Include(sl => sl.ExerciseType)
            .Include(sl => sl.WorkoutSession)
            .Where(sl => sl.WorkoutSession.UserId == userId)
            .OrderByDescending(sl => sl.WorkoutSession.Date)
            .ThenBy(sl => sl.Order)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new strength lift
    /// </summary>
    /// <param name="strengthLift">Lift to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created lift with generated ID</returns>
    public async Task<StrengthLift> CreateAsync(StrengthLift strengthLift, CancellationToken cancellationToken = default)
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
        await _context.SaveChangesAsync(cancellationToken);

        return strengthLift;
    }

    /// <summary>
    /// Creates a new strength lift (alias for service compatibility)
    /// </summary>
    /// <param name="strengthLift">Lift to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created lift with generated ID</returns>
    public async Task<StrengthLift> AddAsync(StrengthLift strengthLift, CancellationToken cancellationToken = default)
    {
        return await CreateAsync(strengthLift, cancellationToken);
    }

    /// <summary>
    /// Updates an existing strength lift
    /// </summary>
    /// <param name="strengthLift">Lift to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated lift</returns>
    public async Task<StrengthLift> UpdateAsync(StrengthLift strengthLift, CancellationToken cancellationToken = default)
    {
        if (strengthLift == null)
            throw new ArgumentNullException(nameof(strengthLift));

        var existingLift = await _context.StrengthLifts
            .Include(sl => sl.WorkoutSession)
            .FirstOrDefaultAsync(sl => sl.Id == strengthLift.Id, cancellationToken);

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
        await _context.SaveChangesAsync(cancellationToken);

        return existingLift;
    }

    /// <summary>
    /// Deletes a strength lift by ID for a specific user
    /// </summary>
    /// <param name="id">Lift ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var strengthLift = await _context.StrengthLifts
            .Include(sl => sl.WorkoutSession)
            .FirstOrDefaultAsync(sl => sl.Id == id && sl.WorkoutSession.UserId == userId, cancellationToken);

        if (strengthLift == null)
            return false;

        _context.StrengthLifts.Remove(strengthLift);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Gets the maximum order value for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Maximum order value or 0 if no lifts exist</returns>
    public async Task<int> GetMaxOrderAsync(Guid workoutSessionId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await _context.StrengthLifts
            .Where(sl => sl.WorkoutSessionId == workoutSessionId)
            .MaxAsync(sl => (int?)sl.Order, cancellationToken);

        return maxOrder ?? 0;
    }
}
