using AutoMapper;
using LiftTracker.Application.Interfaces;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LiftTracker.Application.Services;

/// <summary>
/// Service implementation for workout session management operations
/// </summary>
public class WorkoutSessionService : IWorkoutSessionService
{
    private readonly IWorkoutSessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<WorkoutSessionService> _logger;

    public WorkoutSessionService(
        IWorkoutSessionRepository sessionRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<WorkoutSessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<WorkoutSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting workout session by ID: {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session == null)
        {
            _logger.LogDebug("Workout session not found with ID: {SessionId}", sessionId);
        }

        return session;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutSession>> GetSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all workout sessions for user: {UserId}", userId);

        var sessions = await _sessionRepository.GetByUserIdAsync(userId, cancellationToken);

        _logger.LogDebug("Found {Count} workout sessions for user: {UserId}", sessions.Count(), userId);
        return sessions;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutSession>> GetSessionsByUserAndDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting workout sessions for user: {UserId} between {StartDate} and {EndDate}",
            userId, startDate, endDate);

        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be after end date", nameof(startDate));
        }

        var sessions = await _sessionRepository.GetByUserAndDateRangeAsync(userId, startDate, endDate, cancellationToken);

        _logger.LogDebug("Found {Count} workout sessions for user: {UserId} in date range", sessions.Count(), userId);
        return sessions;
    }

    /// <inheritdoc />
    public async Task<WorkoutSession?> GetSessionByUserAndDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting workout session for user: {UserId} on date: {Date}", userId, date);

        var session = await _sessionRepository.GetByUserAndDateAsync(userId, date, cancellationToken);

        if (session == null)
        {
            _logger.LogDebug("No workout session found for user: {UserId} on date: {Date}", userId, date);
        }

        return session;
    }

    /// <inheritdoc />
    public async Task<WorkoutSession> CreateSessionAsync(Guid userId, DateOnly date, string? notes = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating workout session for user: {UserId} on date: {Date}", userId, date);

        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Cannot create session - user not found: {UserId}", userId);
            throw new InvalidOperationException($"User not found with ID: {userId}");
        }

        // Check if session already exists for this user and date
        var existingSession = await _sessionRepository.GetByUserAndDateAsync(userId, date, cancellationToken);
        if (existingSession != null)
        {
            _logger.LogWarning("Session already exists for user: {UserId} on date: {Date}", userId, date);
            throw new InvalidOperationException($"A workout session already exists for user {userId} on {date}");
        }

        var session = new WorkoutSession
        {
            UserId = userId,
            Date = date,
            Notes = notes
        };

        await _sessionRepository.AddAsync(session, cancellationToken);

        _logger.LogInformation("Created workout session: {SessionId} for user: {UserId} on date: {Date}",
            session.Id, userId, date);

        return session;
    }

    /// <inheritdoc />
    public async Task<WorkoutSession> UpdateSessionAsync(Guid sessionId, DateOnly date, string? notes, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating workout session: {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            _logger.LogWarning("Cannot update - workout session not found: {SessionId}", sessionId);
            throw new InvalidOperationException($"Workout session not found with ID: {sessionId}");
        }

        // If date is changing, check for conflicts
        if (session.Date != date)
        {
            var conflictingSession = await _sessionRepository.GetByUserAndDateAsync(session.UserId, date, cancellationToken);
            if (conflictingSession != null && conflictingSession.Id != sessionId)
            {
                _logger.LogWarning("Cannot update session {SessionId} - date conflict with session {ConflictingSessionId}",
                    sessionId, conflictingSession.Id);
                throw new InvalidOperationException($"A workout session already exists for this user on {date}");
            }
        }

        session.Date = date;
        session.Notes = notes;

        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("Updated workout session: {SessionId}", sessionId);
        return session;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting workout session: {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            _logger.LogWarning("Cannot delete - workout session not found: {SessionId}", sessionId);
            return false;
        }

        await _sessionRepository.DeleteAsync(session, cancellationToken);

        _logger.LogInformation("Deleted workout session: {SessionId}", sessionId);
        return true;
    }

    /// <inheritdoc />
    public async Task<WorkoutSession?> GetMostRecentSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting most recent workout session for user: {UserId}", userId);

        var session = await _sessionRepository.GetMostRecentByUserAsync(userId, cancellationToken);

        if (session == null)
        {
            _logger.LogDebug("No workout sessions found for user: {UserId}", userId);
        }

        return session;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutSession>> GetRecentSessionsAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            throw new ArgumentException("Limit must be greater than zero", nameof(limit));
        }

        _logger.LogDebug("Getting {Limit} recent workout sessions for user: {UserId}", limit, userId);

        var sessions = await _sessionRepository.GetRecentByUserAsync(userId, limit, cancellationToken);

        _logger.LogDebug("Found {Count} recent workout sessions for user: {UserId}", sessions.Count(), userId);
        return sessions;
    }
}
