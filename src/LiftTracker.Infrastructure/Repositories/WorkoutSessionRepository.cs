using Microsoft.EntityFrameworkCore;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using LiftTracker.Infrastructure.Data;

namespace LiftTracker.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for WorkoutSession entity operations
/// </summary>
public class WorkoutSessionRepository : IWorkoutSessionRepository
{
    private readonly LiftTrackerDbContext _context;

    public WorkoutSessionRepository(LiftTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a workout session by ID for a specific user
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>Workout session or null if not found</returns>
    public async Task<WorkoutSession?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.WorkoutSessions
            .Include(ws => ws.StrengthLifts)
                .ThenInclude(sl => sl.ExerciseType)
            .Include(ws => ws.MetconWorkouts)
                .ThenInclude(mw => mw.MetconType)
            .Include(ws => ws.MetconWorkouts)
                .ThenInclude(mw => mw.MetconMovements)
                .ThenInclude(mm => mm.MovementType)
            .FirstOrDefaultAsync(ws => ws.Id == id && ws.UserId == userId);
    }

    /// <summary>
    /// Gets all workout sessions for a user with optional date filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of workout sessions</returns>
    public async Task<IEnumerable<WorkoutSession>> GetByUserAsync(Guid userId, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _context.WorkoutSessions
            .Where(ws => ws.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(ws => ws.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(ws => ws.Date <= endDate.Value);

        return await query
            .OrderByDescending(ws => ws.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a workout session for a specific user and date
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="date">Workout date</param>
    /// <returns>Workout session or null if not found</returns>
    public async Task<WorkoutSession?> GetByUserAndDateAsync(Guid userId, DateOnly date)
    {
        return await _context.WorkoutSessions
            .Include(ws => ws.StrengthLifts)
                .ThenInclude(sl => sl.ExerciseType)
            .Include(ws => ws.MetconWorkouts)
                .ThenInclude(mw => mw.MetconType)
            .Include(ws => ws.MetconWorkouts)
                .ThenInclude(mw => mw.MetconMovements)
                .ThenInclude(mm => mm.MovementType)
            .FirstOrDefaultAsync(ws => ws.UserId == userId && ws.Date == date);
    }

    /// <summary>
    /// Creates a new workout session
    /// </summary>
    /// <param name="workoutSession">Session to create</param>
    /// <returns>Created session with generated ID</returns>
    public async Task<WorkoutSession> CreateAsync(WorkoutSession workoutSession)
    {
        if (workoutSession == null)
            throw new ArgumentNullException(nameof(workoutSession));

        // Ensure ID is generated
        if (workoutSession.Id == Guid.Empty)
            workoutSession.Id = Guid.NewGuid();

        // Validate that the workout date is not in the future
        if (!workoutSession.IsValidDate())
            throw new ArgumentException("Workout date cannot be in the future", nameof(workoutSession));

        _context.WorkoutSessions.Add(workoutSession);
        await _context.SaveChangesAsync();

        return workoutSession;
    }

    /// <summary>
    /// Updates an existing workout session
    /// </summary>
    /// <param name="workoutSession">Session to update</param>
    /// <returns>Updated session</returns>
    public async Task<WorkoutSession> UpdateAsync(WorkoutSession workoutSession)
    {
        if (workoutSession == null)
            throw new ArgumentNullException(nameof(workoutSession));

        var existingSession = await _context.WorkoutSessions
            .FirstOrDefaultAsync(ws => ws.Id == workoutSession.Id && ws.UserId == workoutSession.UserId);

        if (existingSession == null)
            throw new InvalidOperationException($"Workout session with ID {workoutSession.Id} not found for user {workoutSession.UserId}");

        // Validate that the workout date is not in the future
        if (!workoutSession.IsValidDate())
            throw new ArgumentException("Workout date cannot be in the future", nameof(workoutSession));

        // Update properties
        existingSession.Date = workoutSession.Date;
        existingSession.Notes = workoutSession.Notes;

        _context.WorkoutSessions.Update(existingSession);
        await _context.SaveChangesAsync();

        return existingSession;
    }

    /// <summary>
    /// Deletes a workout session by ID for a specific user
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var workoutSession = await _context.WorkoutSessions
            .FirstOrDefaultAsync(ws => ws.Id == id && ws.UserId == userId);

        if (workoutSession == null)
            return false;

        _context.WorkoutSessions.Remove(workoutSession);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks if a workout session exists for a user on a specific date
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="date">Workout date</param>
    /// <returns>True if session exists</returns>
    public async Task<bool> ExistsForDateAsync(Guid userId, DateOnly date)
    {
        return await _context.WorkoutSessions
            .AnyAsync(ws => ws.UserId == userId && ws.Date == date);
    }
}
