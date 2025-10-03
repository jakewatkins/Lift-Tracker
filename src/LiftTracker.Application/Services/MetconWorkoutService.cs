using AutoMapper;
using LiftTracker.Application.Interfaces;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LiftTracker.Application.Services;

/// <summary>
/// Service implementation for metcon workout management operations
/// </summary>
public class MetconWorkoutService : IMetconWorkoutService
{
    private readonly IMetconWorkoutRepository _metconWorkoutRepository;
    private readonly IMetconMovementRepository _metconMovementRepository;
    private readonly IWorkoutSessionRepository _sessionRepository;
    private readonly IMetconTypeRepository _metconTypeRepository;
    private readonly IMovementTypeRepository _movementTypeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<MetconWorkoutService> _logger;

    public MetconWorkoutService(
        IMetconWorkoutRepository metconWorkoutRepository,
        IMetconMovementRepository metconMovementRepository,
        IWorkoutSessionRepository sessionRepository,
        IMetconTypeRepository metconTypeRepository,
        IMovementTypeRepository movementTypeRepository,
        IMapper mapper,
        ILogger<MetconWorkoutService> logger)
    {
        _metconWorkoutRepository = metconWorkoutRepository;
        _metconMovementRepository = metconMovementRepository;
        _sessionRepository = sessionRepository;
        _metconTypeRepository = metconTypeRepository;
        _movementTypeRepository = movementTypeRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<MetconWorkout?> GetWorkoutByIdAsync(Guid workoutId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting metcon workout by ID: {WorkoutId}", workoutId);

        var workout = await _metconWorkoutRepository.GetByIdAsync(workoutId, cancellationToken);

        if (workout == null)
        {
            _logger.LogDebug("Metcon workout not found with ID: {WorkoutId}", workoutId);
        }

        return workout;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MetconWorkout>> GetWorkoutsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting metcon workouts for session: {SessionId}", sessionId);

        var workouts = await _metconWorkoutRepository.GetBySessionIdAsync(sessionId, cancellationToken);

        _logger.LogDebug("Found {Count} metcon workouts for session: {SessionId}", workouts.Count(), sessionId);
        return workouts;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MetconWorkout>> GetWorkoutsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all metcon workouts for user: {UserId}", userId);

        var workouts = await _metconWorkoutRepository.GetByUserIdAsync(userId, cancellationToken);

        _logger.LogDebug("Found {Count} metcon workouts for user: {UserId}", workouts.Count(), userId);
        return workouts;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MetconWorkout>> GetWorkoutsByUserAndMetconTypeAsync(
        Guid userId,
        int metconTypeId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting metcon workouts for user: {UserId} and metcon type: {MetconTypeId}",
            userId, metconTypeId);

        var workouts = await _metconWorkoutRepository.GetByUserAndMetconTypeAsync(userId, metconTypeId, cancellationToken);

        _logger.LogDebug("Found {Count} metcon workouts for user: {UserId} and metcon type: {MetconTypeId}",
            workouts.Count(), userId, metconTypeId);
        return workouts;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MetconWorkout>> GetWorkoutsByUserAndDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be after end date", nameof(startDate));
        }

        _logger.LogDebug("Getting metcon workouts for user: {UserId} between {StartDate} and {EndDate}",
            userId, startDate, endDate);

        var workouts = await _metconWorkoutRepository.GetByUserAndDateRangeAsync(userId, startDate, endDate, cancellationToken);

        _logger.LogDebug("Found {Count} metcon workouts for user: {UserId} in date range", workouts.Count(), userId);
        return workouts;
    }

    /// <inheritdoc />
    public async Task<MetconWorkout> CreateWorkoutAsync(
        Guid sessionId,
        int metconTypeId,
        decimal? totalTime = null,
        int? roundsCompleted = null,
        string? notes = null,
        int order = 0,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating metcon workout for session: {SessionId}, metcon type: {MetconTypeId}",
            sessionId, metconTypeId);

        // Validate session exists
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            _logger.LogWarning("Cannot create workout - session not found: {SessionId}", sessionId);
            throw new InvalidOperationException($"Workout session not found with ID: {sessionId}");
        }

        // Validate metcon type exists
        var metconType = await _metconTypeRepository.GetByIdAsync(metconTypeId, cancellationToken);
        if (metconType == null)
        {
            _logger.LogWarning("Cannot create workout - metcon type not found: {MetconTypeId}", metconTypeId);
            throw new InvalidOperationException($"Metcon type not found with ID: {metconTypeId}");
        }

        var workout = new MetconWorkout
        {
            WorkoutSessionId = sessionId,
            MetconTypeId = metconTypeId,
            TotalTime = totalTime,
            RoundsCompleted = roundsCompleted,
            Notes = notes,
            Order = order
        };

        await _metconWorkoutRepository.AddAsync(workout, cancellationToken);

        _logger.LogInformation("Created metcon workout: {WorkoutId} for session: {SessionId}", workout.Id, sessionId);
        return workout;
    }

    /// <inheritdoc />
    public async Task<MetconWorkout> UpdateWorkoutAsync(
        Guid workoutId,
        int metconTypeId,
        decimal? totalTime,
        int? roundsCompleted,
        string? notes,
        int order,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating metcon workout: {WorkoutId}", workoutId);

        var workout = await _metconWorkoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
        {
            _logger.LogWarning("Cannot update - metcon workout not found: {WorkoutId}", workoutId);
            throw new InvalidOperationException($"Metcon workout not found with ID: {workoutId}");
        }

        // Validate metcon type if it's changing
        if (workout.MetconTypeId != metconTypeId)
        {
            var metconType = await _metconTypeRepository.GetByIdAsync(metconTypeId, cancellationToken);
            if (metconType == null)
            {
                _logger.LogWarning("Cannot update workout - metcon type not found: {MetconTypeId}", metconTypeId);
                throw new InvalidOperationException($"Metcon type not found with ID: {metconTypeId}");
            }
        }

        workout.MetconTypeId = metconTypeId;
        workout.TotalTime = totalTime;
        workout.RoundsCompleted = roundsCompleted;
        workout.Notes = notes;
        workout.Order = order;

        await _metconWorkoutRepository.UpdateAsync(workout, cancellationToken);

        _logger.LogInformation("Updated metcon workout: {WorkoutId}", workoutId);
        return workout;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteWorkoutAsync(Guid workoutId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting metcon workout: {WorkoutId}", workoutId);

        var workout = await _metconWorkoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
        {
            _logger.LogWarning("Cannot delete - metcon workout not found: {WorkoutId}", workoutId);
            return false;
        }

        // Get the workout session to find the userId
        var session = await _sessionRepository.GetByIdAsync(workout.WorkoutSessionId, cancellationToken);
        if (session == null)
        {
            _logger.LogWarning("Cannot delete - associated session not found: {SessionId}", workout.WorkoutSessionId);
            return false;
        }

        await _metconWorkoutRepository.DeleteAsync(workout.Id, session.UserId, cancellationToken);

        _logger.LogInformation("Deleted metcon workout: {WorkoutId}", workoutId);
        return true;
    }

    /// <inheritdoc />
    public async Task<MetconMovement> AddMovementToWorkoutAsync(
        Guid workoutId,
        int movementTypeId,
        int reps,
        decimal? weight = null,
        decimal? distance = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding movement to metcon workout: {WorkoutId}, movement type: {MovementTypeId}",
            workoutId, movementTypeId);

        // Validate workout exists
        var workout = await _metconWorkoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
        {
            _logger.LogWarning("Cannot add movement - metcon workout not found: {WorkoutId}", workoutId);
            throw new InvalidOperationException($"Metcon workout not found with ID: {workoutId}");
        }

        // Validate movement type exists
        var movementType = await _movementTypeRepository.GetByIdAsync(movementTypeId, cancellationToken);
        if (movementType == null)
        {
            _logger.LogWarning("Cannot add movement - movement type not found: {MovementTypeId}", movementTypeId);
            throw new InvalidOperationException($"Movement type not found with ID: {movementTypeId}");
        }

        var movement = new MetconMovement
        {
            MetconWorkoutId = workoutId,
            MovementTypeId = movementTypeId,
            Reps = reps,
            Weight = weight,
            Distance = distance
        };

        await _metconMovementRepository.AddAsync(movement, cancellationToken);

        _logger.LogInformation("Added movement: {MovementId} to metcon workout: {WorkoutId}", movement.Id, workoutId);
        return movement;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MetconMovement>> GetMovementsByWorkoutIdAsync(Guid workoutId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting movements for metcon workout: {WorkoutId}", workoutId);

        var movements = await _metconMovementRepository.GetByWorkoutIdAsync(workoutId, cancellationToken);

        _logger.LogDebug("Found {Count} movements for metcon workout: {WorkoutId}", movements.Count(), workoutId);
        return movements;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MetconWorkout>> GetRecentWorkoutsAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            throw new ArgumentException("Limit must be greater than zero", nameof(limit));
        }

        _logger.LogDebug("Getting {Limit} recent metcon workouts for user: {UserId}", limit, userId);

        var workouts = await _metconWorkoutRepository.GetRecentByUserAsync(userId, limit, cancellationToken);

        _logger.LogDebug("Found {Count} recent metcon workouts for user: {UserId}", workouts.Count(), userId);
        return workouts;
    }
}
