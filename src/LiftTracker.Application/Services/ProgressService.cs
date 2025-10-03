using LiftTracker.Application.Interfaces;
using LiftTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LiftTracker.Application.Services;

/// <summary>
/// Service implementation for progress tracking and analytics operations
/// </summary>
public class ProgressService : IProgressService
{
    private readonly IStrengthLiftRepository _strengthLiftRepository;
    private readonly IMetconWorkoutRepository _metconWorkoutRepository;
    private readonly IWorkoutSessionRepository _sessionRepository;
    private readonly IExerciseTypeRepository _exerciseTypeRepository;
    private readonly IMetconTypeRepository _metconTypeRepository;
    private readonly ILogger<ProgressService> _logger;

    public ProgressService(
        IStrengthLiftRepository strengthLiftRepository,
        IMetconWorkoutRepository metconWorkoutRepository,
        IWorkoutSessionRepository sessionRepository,
        IExerciseTypeRepository exerciseTypeRepository,
        IMetconTypeRepository metconTypeRepository,
        ILogger<ProgressService> logger)
    {
        _strengthLiftRepository = strengthLiftRepository;
        _metconWorkoutRepository = metconWorkoutRepository;
        _sessionRepository = sessionRepository;
        _exerciseTypeRepository = exerciseTypeRepository;
        _metconTypeRepository = metconTypeRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProgressDataPoint>> GetStrengthProgressionAsync(
        Guid userId,
        int exerciseTypeId,
        int periodDays = 90,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting strength progression for user: {UserId}, exercise: {ExerciseTypeId}, period: {PeriodDays} days",
            userId, exerciseTypeId, periodDays);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-periodDays));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var lifts = await _strengthLiftRepository.GetByUserAndExerciseTypeAsync(userId, exerciseTypeId, cancellationToken);
        var exerciseType = await _exerciseTypeRepository.GetByIdAsync(exerciseTypeId, cancellationToken);

        var progressPoints = lifts
            .Where(l => l.WorkoutSession.Date >= startDate && l.WorkoutSession.Date <= endDate)
            .Where(l => l.SetStructure == "SetsReps" && l.Sets.HasValue && l.Reps.HasValue) // Only include standard sets/reps
            .GroupBy(l => l.WorkoutSession.Date)
            .Select(g => new ProgressDataPoint
            {
                Date = g.Key,
                Value = g.Max(l => l.Weight), // Max weight for that day
                ExerciseTypeName = exerciseType?.Name
            })
            .OrderBy(p => p.Date)
            .ToList();

        _logger.LogDebug("Found {Count} progression data points for user: {UserId}, exercise: {ExerciseTypeId}",
            progressPoints.Count, userId, exerciseTypeId);

        return progressPoints;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProgressDataPoint>> GetMetconProgressionAsync(
        Guid userId,
        int metconTypeId,
        int periodDays = 90,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting metcon progression for user: {UserId}, metcon type: {MetconTypeId}, period: {PeriodDays} days",
            userId, metconTypeId, periodDays);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-periodDays));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var workouts = await _metconWorkoutRepository.GetByUserAndMetconTypeAsync(userId, metconTypeId, cancellationToken);
        var metconType = await _metconTypeRepository.GetByIdAsync(metconTypeId, cancellationToken);

        var progressPoints = workouts
            .Where(w => w.WorkoutSession.Date >= startDate && w.WorkoutSession.Date <= endDate)
            .GroupBy(w => w.WorkoutSession.Date)
            .Select(g =>
            {
                var bestWorkout = g.OrderBy(w => w.TotalTime ?? decimal.MaxValue).First(); // Best time (lowest)
                return new ProgressDataPoint
                {
                    Date = g.Key,
                    Value = bestWorkout.TotalTime ?? bestWorkout.RoundsCompleted ?? 0,
                    MetconTypeName = metconType?.Name
                };
            })
            .OrderBy(p => p.Date)
            .ToList();

        _logger.LogDebug("Found {Count} metcon progression data points for user: {UserId}, metcon type: {MetconTypeId}",
            progressPoints.Count, userId, metconTypeId);

        return progressPoints;
    }

    /// <inheritdoc />
    public async Task<WorkoutFrequencyStats> GetWorkoutFrequencyAsync(
        Guid userId,
        int periodDays = 30,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting workout frequency stats for user: {UserId}, period: {PeriodDays} days",
            userId, periodDays);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-periodDays));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var sessions = await _sessionRepository.GetByUserAndDateRangeAsync(userId, startDate, endDate, cancellationToken);
        var sessionsByDate = sessions.OrderBy(s => s.Date).ToList();

        var totalWorkouts = sessionsByDate.Count;
        var averageWorkoutsPerWeek = totalWorkouts > 0 ? (decimal)totalWorkouts / periodDays * 7 : 0;

        // Calculate streaks
        var currentStreak = CalculateCurrentStreak(sessionsByDate, endDate);
        var longestStreak = CalculateLongestStreak(sessionsByDate);

        var stats = new WorkoutFrequencyStats
        {
            TotalWorkouts = totalWorkouts,
            TotalDays = periodDays,
            AverageWorkoutsPerWeek = Math.Round(averageWorkoutsPerWeek, 1),
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak
        };

        _logger.LogDebug("Calculated workout frequency stats for user: {UserId} - {TotalWorkouts} workouts, {CurrentStreak} current streak",
            userId, totalWorkouts, currentStreak);

        return stats;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PersonalRecord>> GetPersonalRecordsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting personal records for user: {UserId}", userId);

        var exerciseTypes = await _exerciseTypeRepository.GetAllAsync(cancellationToken);
        var personalRecords = new List<PersonalRecord>();

        foreach (var exerciseType in exerciseTypes)
        {
            var personalRecord = await _strengthLiftRepository.GetPersonalRecordAsync(userId, exerciseType.Id, cancellationToken);
            if (personalRecord != null && personalRecord.Sets.HasValue && personalRecord.Reps.HasValue)
            {
                personalRecords.Add(new PersonalRecord
                {
                    ExerciseTypeId = exerciseType.Id,
                    ExerciseTypeName = exerciseType.Name,
                    MaxWeight = personalRecord.Weight,
                    Reps = personalRecord.Reps.Value,
                    Sets = personalRecord.Sets.Value,
                    AchievedDate = personalRecord.WorkoutSession.Date
                });
            }
        }

        _logger.LogDebug("Found {Count} personal records for user: {UserId}", personalRecords.Count, userId);
        return personalRecords.OrderByDescending(pr => pr.MaxWeight);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<VolumeDataPoint>> GetVolumeStatsAsync(
        Guid userId,
        int periodDays = 30,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting volume stats for user: {UserId}, period: {PeriodDays} days",
            userId, periodDays);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-periodDays));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var lifts = await _strengthLiftRepository.GetByUserAndDateRangeAsync(userId, startDate, endDate, cancellationToken);

        var volumePoints = lifts
            .Where(l => l.SetStructure == "SetsReps" && l.Sets.HasValue && l.Reps.HasValue)
            .GroupBy(l => l.WorkoutSession.Date)
            .Select(g => new VolumeDataPoint
            {
                Date = g.Key,
                TotalVolume = g.Sum(l => l.Weight * l.Reps!.Value * l.Sets!.Value),
                TotalLifts = g.Count()
            })
            .OrderBy(v => v.Date)
            .ToList();

        _logger.LogDebug("Found {Count} volume data points for user: {UserId}", volumePoints.Count, userId);
        return volumePoints;
    }

    /// <inheritdoc />
    public async Task<DashboardSummary> GetDashboardSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting dashboard summary for user: {UserId}", userId);

        var allSessions = await _sessionRepository.GetByUserIdAsync(userId, cancellationToken);
        var allLifts = await _strengthLiftRepository.GetByUserIdAsync(userId, cancellationToken);
        var personalRecords = await GetPersonalRecordsAsync(userId, cancellationToken);

        var frequencyStats = await GetWorkoutFrequencyAsync(userId, 30, cancellationToken);

        var totalVolume = allLifts
            .Where(l => l.SetStructure == "SetsReps" && l.Sets.HasValue && l.Reps.HasValue)
            .Sum(l => l.Weight * l.Reps!.Value * l.Sets!.Value);

        var mostFrequentExercise = allLifts
            .GroupBy(l => l.ExerciseType.Name)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        var lastWorkoutDate = allSessions.OrderByDescending(s => s.Date).FirstOrDefault()?.Date;

        var summary = new DashboardSummary
        {
            TotalWorkouts = allSessions.Count(),
            TotalVolumeLifted = Math.Round(totalVolume, 2),
            PersonalRecords = personalRecords.Count(),
            CurrentStreak = frequencyStats.CurrentStreak,
            LastWorkoutDate = lastWorkoutDate,
            MostFrequentExercise = mostFrequentExercise,
            AverageWorkoutsPerWeek = frequencyStats.AverageWorkoutsPerWeek
        };

        _logger.LogDebug("Generated dashboard summary for user: {UserId}", userId);
        return summary;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Achievement>> GetRecentAchievementsAsync(Guid userId, int limit = 5, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting recent achievements for user: {UserId}", userId);

        var achievements = new List<Achievement>();
        var personalRecords = await GetPersonalRecordsAsync(userId, cancellationToken);

        // Add recent PRs
        var recentPRs = personalRecords
            .Where(pr => pr.AchievedDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)))
            .OrderByDescending(pr => pr.AchievedDate)
            .Take(limit);

        foreach (var pr in recentPRs)
        {
            achievements.Add(new Achievement
            {
                Title = $"Personal Record: {pr.ExerciseTypeName}",
                Description = $"New PR of {pr.MaxWeight}lbs for {pr.Reps} reps",
                AchievedDate = pr.AchievedDate,
                Type = "PR"
            });
        }

        _logger.LogDebug("Found {Count} recent achievements for user: {UserId}", achievements.Count, userId);
        return achievements.Take(limit);
    }

    private static int CalculateCurrentStreak(List<Domain.Entities.WorkoutSession> sessionsByDate, DateOnly currentDate)
    {
        if (!sessionsByDate.Any()) return 0;

        var streak = 0;
        var checkDate = currentDate;

        // Go backwards from current date looking for consecutive workout days
        while (true)
        {
            if (sessionsByDate.Any(s => s.Date == checkDate))
            {
                streak++;
                checkDate = checkDate.AddDays(-1);
            }
            else
            {
                // Allow one rest day before breaking streak
                checkDate = checkDate.AddDays(-1);
                if (sessionsByDate.Any(s => s.Date == checkDate))
                {
                    streak++;
                    checkDate = checkDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }
        }

        return streak;
    }

    private static int CalculateLongestStreak(List<Domain.Entities.WorkoutSession> sessionsByDate)
    {
        if (!sessionsByDate.Any()) return 0;

        var maxStreak = 0;
        var currentStreak = 1;
        var previousDate = sessionsByDate.First().Date;

        for (int i = 1; i < sessionsByDate.Count; i++)
        {
            var currentDate = sessionsByDate[i].Date;
            var daysDifference = currentDate.DayNumber - previousDate.DayNumber;

            if (daysDifference <= 2) // Allow one rest day
            {
                currentStreak++;
            }
            else
            {
                maxStreak = Math.Max(maxStreak, currentStreak);
                currentStreak = 1;
            }

            previousDate = currentDate;
        }

        return Math.Max(maxStreak, currentStreak);
    }
}
