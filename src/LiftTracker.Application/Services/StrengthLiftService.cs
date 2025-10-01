using AutoMapper;
using LiftTracker.Application.Interfaces;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LiftTracker.Application.Services;

/// <summary>
/// Service implementation for strength lift management operations
/// </summary>
public class StrengthLiftService : IStrengthLiftService
{
    private readonly IStrengthLiftRepository _strengthLiftRepository;
    private readonly IWorkoutSessionRepository _sessionRepository;
    private readonly IExerciseTypeRepository _exerciseTypeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<StrengthLiftService> _logger;

    public StrengthLiftService(
        IStrengthLiftRepository strengthLiftRepository,
        IWorkoutSessionRepository sessionRepository,
        IExerciseTypeRepository exerciseTypeRepository,
        IMapper mapper,
        ILogger<StrengthLiftService> logger)
    {
        _strengthLiftRepository = strengthLiftRepository;
        _sessionRepository = sessionRepository;
        _exerciseTypeRepository = exerciseTypeRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<StrengthLift?> GetLiftByIdAsync(Guid liftId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting strength lift by ID: {LiftId}", liftId);

        var lift = await _strengthLiftRepository.GetByIdAsync(liftId, cancellationToken);

        if (lift == null)
        {
            _logger.LogDebug("Strength lift not found with ID: {LiftId}", liftId);
        }

        return lift;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StrengthLift>> GetLiftsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting strength lifts for session: {SessionId}", sessionId);

        var lifts = await _strengthLiftRepository.GetBySessionIdAsync(sessionId, cancellationToken);

        _logger.LogDebug("Found {Count} strength lifts for session: {SessionId}", lifts.Count(), sessionId);
        return lifts;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StrengthLift>> GetLiftsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all strength lifts for user: {UserId}", userId);

        var lifts = await _strengthLiftRepository.GetByUserIdAsync(userId, cancellationToken);

        _logger.LogDebug("Found {Count} strength lifts for user: {UserId}", lifts.Count(), userId);
        return lifts;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StrengthLift>> GetLiftsByUserAndExerciseTypeAsync(
        Guid userId,
        int exerciseTypeId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting strength lifts for user: {UserId} and exercise type: {ExerciseTypeId}",
            userId, exerciseTypeId);

        var lifts = await _strengthLiftRepository.GetByUserAndExerciseTypeAsync(userId, exerciseTypeId, cancellationToken);

        _logger.LogDebug("Found {Count} strength lifts for user: {UserId} and exercise type: {ExerciseTypeId}",
            lifts.Count(), userId, exerciseTypeId);
        return lifts;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StrengthLift>> GetLiftsByUserAndDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be after end date", nameof(startDate));
        }

        _logger.LogDebug("Getting strength lifts for user: {UserId} between {StartDate} and {EndDate}",
            userId, startDate, endDate);

        var lifts = await _strengthLiftRepository.GetByUserAndDateRangeAsync(userId, startDate, endDate, cancellationToken);

        _logger.LogDebug("Found {Count} strength lifts for user: {UserId} in date range", lifts.Count(), userId);
        return lifts;
    }

    /// <inheritdoc />
    public async Task<StrengthLift> CreateLiftAsync(
        Guid sessionId,
        int exerciseTypeId,
        string setStructure,
        int? sets = null,
        int? reps = null,
        decimal weight = 0,
        decimal? additionalWeight = null,
        decimal? duration = null,
        decimal? restPeriod = null,
        string? comments = null,
        int order = 0,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating strength lift for session: {SessionId}, exercise type: {ExerciseTypeId}",
            sessionId, exerciseTypeId);

        // Validate session exists
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            _logger.LogWarning("Cannot create lift - session not found: {SessionId}", sessionId);
            throw new InvalidOperationException($"Workout session not found with ID: {sessionId}");
        }

        // Validate exercise type exists
        var exerciseType = await _exerciseTypeRepository.GetByIdAsync(exerciseTypeId, cancellationToken);
        if (exerciseType == null)
        {
            _logger.LogWarning("Cannot create lift - exercise type not found: {ExerciseTypeId}", exerciseTypeId);
            throw new InvalidOperationException($"Exercise type not found with ID: {exerciseTypeId}");
        }

        var lift = new StrengthLift
        {
            WorkoutSessionId = sessionId,
            ExerciseTypeId = exerciseTypeId,
            SetStructure = setStructure,
            Sets = sets,
            Reps = reps,
            Weight = weight,
            AdditionalWeight = additionalWeight,
            Duration = duration,
            RestPeriod = restPeriod,
            Comments = comments,
            Order = order
        };

        // Validate business rules
        if (!lift.IsValidWeight())
        {
            throw new ArgumentException("Weight must use only 0.25 increments", nameof(weight));
        }

        if (!lift.IsValidAdditionalWeight())
        {
            throw new ArgumentException("Additional weight must use only 0.25 increments", nameof(additionalWeight));
        }

        if (!lift.IsValidDuration())
        {
            throw new ArgumentException("Duration must use only 0.25 increments", nameof(duration));
        }

        if (!lift.IsValidRestPeriod())
        {
            throw new ArgumentException("Rest period must use only 0.25 increments", nameof(restPeriod));
        }

        if (!lift.IsValidSets())
        {
            throw new ArgumentException("Sets must be between 1 and 50", nameof(sets));
        }

        if (!lift.IsValidReps())
        {
            throw new ArgumentException("Reps must be between 1 and 500", nameof(reps));
        }

        await _strengthLiftRepository.AddAsync(lift, cancellationToken);

        _logger.LogInformation("Created strength lift: {LiftId} for session: {SessionId}", lift.Id, sessionId);
        return lift;
    }

    /// <inheritdoc />
    public async Task<StrengthLift> UpdateLiftAsync(
        Guid liftId,
        int exerciseTypeId,
        string setStructure,
        int? sets,
        int? reps,
        decimal weight,
        decimal? additionalWeight,
        decimal? duration,
        decimal? restPeriod,
        string? comments,
        int order,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating strength lift: {LiftId}", liftId);

        var lift = await _strengthLiftRepository.GetByIdAsync(liftId, cancellationToken);
        if (lift == null)
        {
            _logger.LogWarning("Cannot update - strength lift not found: {LiftId}", liftId);
            throw new InvalidOperationException($"Strength lift not found with ID: {liftId}");
        }

        // Validate exercise type if it's changing
        if (lift.ExerciseTypeId != exerciseTypeId)
        {
            var exerciseType = await _exerciseTypeRepository.GetByIdAsync(exerciseTypeId, cancellationToken);
            if (exerciseType == null)
            {
                _logger.LogWarning("Cannot update lift - exercise type not found: {ExerciseTypeId}", exerciseTypeId);
                throw new InvalidOperationException($"Exercise type not found with ID: {exerciseTypeId}");
            }
        }

        lift.ExerciseTypeId = exerciseTypeId;
        lift.SetStructure = setStructure;
        lift.Sets = sets;
        lift.Reps = reps;
        lift.Weight = weight;
        lift.AdditionalWeight = additionalWeight;
        lift.Duration = duration;
        lift.RestPeriod = restPeriod;
        lift.Comments = comments;
        lift.Order = order;

        // Validate business rules
        if (!lift.IsValidWeight())
        {
            throw new ArgumentException("Weight must use only 0.25 increments", nameof(weight));
        }

        if (!lift.IsValidAdditionalWeight())
        {
            throw new ArgumentException("Additional weight must use only 0.25 increments", nameof(additionalWeight));
        }

        if (!lift.IsValidDuration())
        {
            throw new ArgumentException("Duration must use only 0.25 increments", nameof(duration));
        }

        if (!lift.IsValidRestPeriod())
        {
            throw new ArgumentException("Rest period must use only 0.25 increments", nameof(restPeriod));
        }

        if (!lift.IsValidSets())
        {
            throw new ArgumentException("Sets must be between 1 and 50", nameof(sets));
        }

        if (!lift.IsValidReps())
        {
            throw new ArgumentException("Reps must be between 1 and 500", nameof(reps));
        }

        await _strengthLiftRepository.UpdateAsync(lift, cancellationToken);

        _logger.LogInformation("Updated strength lift: {LiftId}", liftId);
        return lift;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteLiftAsync(Guid liftId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting strength lift: {LiftId}", liftId);

        var lift = await _strengthLiftRepository.GetByIdAsync(liftId, cancellationToken);
        if (lift == null)
        {
            _logger.LogWarning("Cannot delete - strength lift not found: {LiftId}", liftId);
            return false;
        }

        await _strengthLiftRepository.DeleteAsync(lift, cancellationToken);

        _logger.LogInformation("Deleted strength lift: {LiftId}", liftId);
        return true;
    }

    /// <inheritdoc />
    public async Task<StrengthLift?> GetPersonalRecordAsync(Guid userId, int exerciseTypeId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting personal record for user: {UserId}, exercise type: {ExerciseTypeId}",
            userId, exerciseTypeId);

        var personalRecord = await _strengthLiftRepository.GetPersonalRecordAsync(userId, exerciseTypeId, cancellationToken);

        if (personalRecord == null)
        {
            _logger.LogDebug("No personal record found for user: {UserId}, exercise type: {ExerciseTypeId}",
                userId, exerciseTypeId);
        }

        return personalRecord;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StrengthLift>> GetRecentLiftsAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            throw new ArgumentException("Limit must be greater than zero", nameof(limit));
        }

        _logger.LogDebug("Getting {Limit} recent strength lifts for user: {UserId}", limit, userId);

        var lifts = await _strengthLiftRepository.GetRecentByUserAsync(userId, limit, cancellationToken);

        _logger.LogDebug("Found {Count} recent strength lifts for user: {UserId}", lifts.Count(), userId);
        return lifts;
    }
}
