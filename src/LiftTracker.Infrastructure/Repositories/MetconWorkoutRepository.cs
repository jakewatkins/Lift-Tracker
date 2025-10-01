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
    /// Gets a metcon workout by ID for a specific user
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>Metcon workout or null if not found</returns>
    public async Task<MetconWorkout?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.MetconWorkouts
            .Include(mw => mw.MetconType)
            .Include(mw => mw.WorkoutSession)
            .Include(mw => mw.MetconMovements!)
                .ThenInclude(mm => mm.MovementType)
            .FirstOrDefaultAsync(mw => mw.Id == id && mw.WorkoutSession.UserId == userId);
    }

    /// <summary>
    /// Gets all metcon workouts for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>List of metcon workouts ordered by Order</returns>
    public async Task<IEnumerable<MetconWorkout>> GetByWorkoutSessionAsync(Guid workoutSessionId, Guid userId)
    {
        return await _context.MetconWorkouts
            .Include(mw => mw.MetconType)
            .Include(mw => mw.MetconMovements!)
                .ThenInclude(mm => mm.MovementType)
            .Where(mw => mw.WorkoutSessionId == workoutSessionId && mw.WorkoutSession.UserId == userId)
            .OrderBy(mw => mw.Order)
            .ToListAsync();
    }

    /// <summary>
    /// Gets metcon workouts for a user by metcon type with date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="metconTypeId">Metcon type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of metcon workouts ordered by date descending</returns>
    public async Task<IEnumerable<MetconWorkout>> GetByUserAndTypeAsync(Guid userId, int metconTypeId, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _context.MetconWorkouts
            .Include(mw => mw.MetconType)
            .Include(mw => mw.WorkoutSession)
            .Include(mw => mw.MetconMovements!)
                .ThenInclude(mm => mm.MovementType)
            .Where(mw => mw.WorkoutSession.UserId == userId && mw.MetconTypeId == metconTypeId);

        if (startDate.HasValue)
        {
            query = query.Where(mw => mw.WorkoutSession.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(mw => mw.WorkoutSession.Date <= endDate.Value);
        }

        return await query
            .OrderByDescending(mw => mw.WorkoutSession.Date)
            .ThenBy(mw => mw.Order)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new metcon workout
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to create</param>
    /// <returns>Created workout with generated ID</returns>
    public async Task<MetconWorkout> CreateAsync(MetconWorkout metconWorkout)
    {
        if (metconWorkout == null)
            throw new ArgumentNullException(nameof(metconWorkout));

        // Validate the workout session exists and belongs to the user
        var workoutSession = await _context.WorkoutSessions
            .FirstOrDefaultAsync(ws => ws.Id == metconWorkout.WorkoutSessionId);

        if (workoutSession == null)
            throw new InvalidOperationException($"Workout session {metconWorkout.WorkoutSessionId} not found");

        // Validate the metcon type exists
        var metconType = await _context.MetconTypes
            .FirstOrDefaultAsync(mt => mt.Id == metconWorkout.MetconTypeId);

        if (metconType == null)
            throw new InvalidOperationException($"Metcon type {metconWorkout.MetconTypeId} not found");

        // If order is not set, assign the next available order
        if (metconWorkout.Order == 0)
        {
            var maxOrder = await GetMaxOrderAsync(metconWorkout.WorkoutSessionId);
            metconWorkout.Order = maxOrder + 1;
        }

        // Validate business rules
        ValidateMetconWorkout(metconWorkout);

        _context.MetconWorkouts.Add(metconWorkout);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        return await GetByIdAsync(metconWorkout.Id, workoutSession.UserId)
               ?? throw new InvalidOperationException("Failed to retrieve created metcon workout");
    }

    /// <summary>
    /// Updates an existing metcon workout
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to update</param>
    /// <returns>Updated workout</returns>
    public async Task<MetconWorkout> UpdateAsync(MetconWorkout metconWorkout)
    {
        if (metconWorkout == null)
            throw new ArgumentNullException(nameof(metconWorkout));

        var existingWorkout = await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .FirstOrDefaultAsync(mw => mw.Id == metconWorkout.Id);

        if (existingWorkout == null)
            throw new InvalidOperationException($"Metcon workout {metconWorkout.Id} not found");

        // Validate the metcon type exists if it's being changed
        if (existingWorkout.MetconTypeId != metconWorkout.MetconTypeId)
        {
            var metconType = await _context.MetconTypes
                .FirstOrDefaultAsync(mt => mt.Id == metconWorkout.MetconTypeId);

            if (metconType == null)
                throw new InvalidOperationException($"Metcon type {metconWorkout.MetconTypeId} not found");
        }

        // Validate business rules
        ValidateMetconWorkout(metconWorkout);

        // Update properties
        existingWorkout.MetconTypeId = metconWorkout.MetconTypeId;
        existingWorkout.TotalTime = metconWorkout.TotalTime;
        existingWorkout.RoundsCompleted = metconWorkout.RoundsCompleted;
        existingWorkout.Notes = metconWorkout.Notes;
        existingWorkout.Order = metconWorkout.Order;

        await _context.SaveChangesAsync();

        // Reload with navigation properties
        return await GetByIdAsync(metconWorkout.Id, existingWorkout.WorkoutSession.UserId)
               ?? throw new InvalidOperationException("Failed to retrieve updated metcon workout");
    }

    /// <summary>
    /// Deletes a metcon workout by ID for a specific user
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var metconWorkout = await _context.MetconWorkouts
            .Include(mw => mw.WorkoutSession)
            .FirstOrDefaultAsync(mw => mw.Id == id && mw.WorkoutSession.UserId == userId);

        if (metconWorkout == null)
            return false;

        _context.MetconWorkouts.Remove(metconWorkout);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Gets the maximum order value for a workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <returns>Maximum order value or 0 if no metcon workouts exist</returns>
    public async Task<int> GetMaxOrderAsync(Guid workoutSessionId)
    {
        var maxOrder = await _context.MetconWorkouts
            .Where(mw => mw.WorkoutSessionId == workoutSessionId)
            .MaxAsync(mw => (int?)mw.Order);

        return maxOrder ?? 0;
    }

    /// <summary>
    /// Validates metcon workout business rules
    /// </summary>
    /// <param name="metconWorkout">Metcon workout to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    private static void ValidateMetconWorkout(MetconWorkout metconWorkout)
    {
        if (metconWorkout.Order < 1)
            throw new ArgumentException("Order must be greater than 0", nameof(metconWorkout.Order));

        if (metconWorkout.TotalTime.HasValue && metconWorkout.TotalTime.Value < 0)
            throw new ArgumentException("Total time cannot be negative", nameof(metconWorkout.TotalTime));

        if (metconWorkout.RoundsCompleted.HasValue && metconWorkout.RoundsCompleted.Value < 0)
            throw new ArgumentException("Rounds completed cannot be negative", nameof(metconWorkout.RoundsCompleted));

        if (!string.IsNullOrEmpty(metconWorkout.Notes) && metconWorkout.Notes.Length > 1000)
            throw new ArgumentException("Notes cannot exceed 1000 characters", nameof(metconWorkout.Notes));
    }
}
