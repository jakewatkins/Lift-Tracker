using Microsoft.EntityFrameworkCore;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using LiftTracker.Infrastructure.Data;

namespace LiftTracker.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for MetconWorkout entity operations
/// </summary>
public class MetconWorkoutRepository : IMetconWorkoutRepository
{
    private readonly LiftTrackerDbContext _context;

    public MetconWorkoutRepository(LiftTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a metcon workout by ID (admin access)
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Metcon workout or null if not found</returns>
    public async Task<MetconWorkout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .FirstOrDefaultAsync(mw => mw.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets a metcon workout by ID for a specific user
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Metcon workout or null if not found</returns>
    public async Task<MetconWorkout?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .FirstOrDefaultAsync(mw => mw.Id == id && mw.WorkoutSession.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Gets all metcon workouts for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by Order</returns>
    public async Task<IEnumerable<MetconWorkout>> GetByWorkoutSessionAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .Where(mw => mw.WorkoutSessionId == workoutSessionId && mw.WorkoutSession.UserId == userId)
            .OrderBy(mw => mw.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all metcon workouts for a workout session (alias for service compatibility)
    /// </summary>
    /// <param name="sessionId">Workout session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by Order</returns>
    public async Task<IEnumerable<MetconWorkout>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .Where(mw => mw.WorkoutSessionId == sessionId)
            .OrderBy(mw => mw.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all metcon workouts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    public async Task<IEnumerable<MetconWorkout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .Where(mw => mw.WorkoutSession.UserId == userId)
            .OrderByDescending(mw => mw.WorkoutSession.Date)
            .ThenBy(mw => mw.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets metcon workouts for a user by metcon type with date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="metconTypeId">Metcon type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    public async Task<IEnumerable<MetconWorkout>> GetByUserAndTypeAsync(Guid userId, int metconTypeId, DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .Where(mw => mw.WorkoutSession.UserId == userId && mw.MetconTypeId == metconTypeId);

        if (startDate.HasValue)
            query = query.Where(mw => mw.WorkoutSession.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(mw => mw.WorkoutSession.Date <= endDate.Value);

        return await query
            .OrderByDescending(mw => mw.WorkoutSession.Date)
            .ThenBy(mw => mw.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets metcon workouts for a user by metcon type (alias for service compatibility)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="metconTypeId">Metcon type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    public async Task<IEnumerable<MetconWorkout>> GetByUserAndMetconTypeAsync(Guid userId, int metconTypeId, CancellationToken cancellationToken = default)
    {
        return await GetByUserAndTypeAsync(userId, metconTypeId, null, null, cancellationToken);
    }

    /// <summary>
    /// Gets metcon workouts for a user within a date range
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    public async Task<IEnumerable<MetconWorkout>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        return await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .Where(mw => mw.WorkoutSession.UserId == userId &&
                        mw.WorkoutSession.Date >= startDate &&
                        mw.WorkoutSession.Date <= endDate)
            .OrderByDescending(mw => mw.WorkoutSession.Date)
            .ThenBy(mw => mw.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets recent metcon workouts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Maximum number of workouts to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recent metcon workouts</returns>
    public async Task<IEnumerable<MetconWorkout>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken cancellationToken = default)
    {
        return await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .Where(mw => mw.WorkoutSession.UserId == userId)
            .OrderByDescending(mw => mw.WorkoutSession.Date)
            .ThenBy(mw => mw.Order)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new metcon workout
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created workout with generated ID</returns>
    public async Task<MetconWorkout> CreateAsync(MetconWorkout metconWorkout, CancellationToken cancellationToken = default)
    {
        if (metconWorkout == null)
            throw new ArgumentNullException(nameof(metconWorkout));

        // Ensure ID is generated
        if (metconWorkout.Id == Guid.Empty)
            metconWorkout.Id = Guid.NewGuid();

        // Validate business rules
        if (!metconWorkout.IsValidRounds())
            throw new ArgumentException("Rounds must be between 1 and 100", nameof(metconWorkout));

        if (!metconWorkout.IsValidTimeCapMinutes())
            throw new ArgumentException("Time cap must use fractional increments of 0.25", nameof(metconWorkout));

        if (!metconWorkout.IsValidRestBetweenRounds())
            throw new ArgumentException("Rest between rounds must use fractional increments of 0.25", nameof(metconWorkout));

        if (!metconWorkout.IsValidActualTimeMinutes())
            throw new ArgumentException("Actual time must use fractional increments of 0.25", nameof(metconWorkout));

        _context.MetconWorkouts.Add(metconWorkout);
        await _context.SaveChangesAsync(cancellationToken);

        return metconWorkout;
    }

    /// <summary>
    /// Creates a new metcon workout (alias for service compatibility)
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created workout with generated ID</returns>
    public async Task<MetconWorkout> AddAsync(MetconWorkout metconWorkout, CancellationToken cancellationToken = default)
    {
        return await CreateAsync(metconWorkout, cancellationToken);
    }

    /// <summary>
    /// Updates an existing metcon workout
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated workout</returns>
    public async Task<MetconWorkout> UpdateAsync(MetconWorkout metconWorkout, CancellationToken cancellationToken = default)
    {
        if (metconWorkout == null)
            throw new ArgumentNullException(nameof(metconWorkout));

        var existingWorkout = await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .FirstOrDefaultAsync(mw => mw.Id == metconWorkout.Id, cancellationToken);

        if (existingWorkout == null)
            throw new InvalidOperationException($"Metcon workout with ID {metconWorkout.Id} not found");

        // Validate business rules
        if (!metconWorkout.IsValidRounds())
            throw new ArgumentException("Rounds must be between 1 and 100", nameof(metconWorkout));

        if (!metconWorkout.IsValidTimeCapMinutes())
            throw new ArgumentException("Time cap must use fractional increments of 0.25", nameof(metconWorkout));

        if (!metconWorkout.IsValidRestBetweenRounds())
            throw new ArgumentException("Rest between rounds must use fractional increments of 0.25", nameof(metconWorkout));

        if (!metconWorkout.IsValidActualTimeMinutes())
            throw new ArgumentException("Actual time must use fractional increments of 0.25", nameof(metconWorkout));

        // Update properties
        existingWorkout.MetconTypeId = metconWorkout.MetconTypeId;
        existingWorkout.Rounds = metconWorkout.Rounds;
        existingWorkout.TimeCapMinutes = metconWorkout.TimeCapMinutes;
        existingWorkout.ActualTimeMinutes = metconWorkout.ActualTimeMinutes;
        existingWorkout.RestBetweenRounds = metconWorkout.RestBetweenRounds;
        existingWorkout.Comments = metconWorkout.Comments;
        existingWorkout.Order = metconWorkout.Order;

        _context.MetconWorkouts.Update(existingWorkout);
        await _context.SaveChangesAsync(cancellationToken);

        return existingWorkout;
    }

    /// <summary>
    /// Deletes a metcon workout by ID for a specific user
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var metconWorkout = await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .FirstOrDefaultAsync(mw => mw.Id == id && mw.WorkoutSession.UserId == userId, cancellationToken);

        if (metconWorkout == null)
            return false;

        _context.MetconWorkouts.Remove(metconWorkout);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Gets the maximum order value for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Maximum order value or 0 if no metcon workouts exist</returns>
    public async Task<int> GetMaxOrderAsync(Guid workoutSessionId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await _context.MetconWorkouts
            .Where(mw => mw.WorkoutSessionId == workoutSessionId)
            .MaxAsync(mw => (int?)mw.Order, cancellationToken);

        return maxOrder ?? 0;
    }
}
