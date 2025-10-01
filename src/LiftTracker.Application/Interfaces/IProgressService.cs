namespace LiftTracker.Application.Interfaces;

/// <summary>
/// Service interface for progress tracking and analytics operations
/// </summary>
public interface IProgressService
{
    /// <summary>
    /// Calculates strength progression data for a user and exercise type
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="exerciseTypeId">The exercise type identifier</param>
    /// <param name="periodDays">Number of days to look back for progression data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of progression data points</returns>
    Task<IEnumerable<ProgressDataPoint>> GetStrengthProgressionAsync(
        Guid userId,
        int exerciseTypeId,
        int periodDays = 90,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates metcon performance progression for a user and metcon type
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="metconTypeId">The metcon type identifier</param>
    /// <param name="periodDays">Number of days to look back for progression data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of progression data points</returns>
    Task<IEnumerable<ProgressDataPoint>> GetMetconProgressionAsync(
        Guid userId,
        int metconTypeId,
        int periodDays = 90,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workout frequency statistics for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="periodDays">Number of days to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workout frequency statistics</returns>
    Task<WorkoutFrequencyStats> GetWorkoutFrequencyAsync(
        Guid userId,
        int periodDays = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets personal records summary for all exercise types for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of personal records</returns>
    Task<IEnumerable<PersonalRecord>> GetPersonalRecordsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets volume statistics (total weight lifted) for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="periodDays">Number of days to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of volume data points by date</returns>
    Task<IEnumerable<VolumeDataPoint>> GetVolumeStatsAsync(
        Guid userId,
        int periodDays = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets dashboard summary statistics for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard summary with key metrics</returns>
    Task<DashboardSummary> GetDashboardSummaryAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent achievements and milestones for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="limit">Maximum number of achievements to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent achievements</returns>
    Task<IEnumerable<Achievement>> GetRecentAchievementsAsync(Guid userId, int limit = 5, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a data point for progress tracking charts
/// </summary>
public class ProgressDataPoint
{
    public DateOnly Date { get; set; }
    public decimal Value { get; set; }
    public string? ExerciseTypeName { get; set; }
    public string? MetconTypeName { get; set; }
}

/// <summary>
/// Represents workout frequency statistics
/// </summary>
public class WorkoutFrequencyStats
{
    public int TotalWorkouts { get; set; }
    public int TotalDays { get; set; }
    public decimal AverageWorkoutsPerWeek { get; set; }
    public int LongestStreak { get; set; }
    public int CurrentStreak { get; set; }
}

/// <summary>
/// Represents a personal record for an exercise
/// </summary>
public class PersonalRecord
{
    public int ExerciseTypeId { get; set; }
    public string ExerciseTypeName { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public int Reps { get; set; }
    public int Sets { get; set; }
    public DateOnly AchievedDate { get; set; }
}

/// <summary>
/// Represents volume (total weight lifted) data point
/// </summary>
public class VolumeDataPoint
{
    public DateOnly Date { get; set; }
    public decimal TotalVolume { get; set; }
    public int TotalLifts { get; set; }
}

/// <summary>
/// Represents dashboard summary statistics
/// </summary>
public class DashboardSummary
{
    public int TotalWorkouts { get; set; }
    public decimal TotalVolumeLifted { get; set; }
    public int PersonalRecords { get; set; }
    public int CurrentStreak { get; set; }
    public DateOnly? LastWorkoutDate { get; set; }
    public string? MostFrequentExercise { get; set; }
    public decimal? AverageWorkoutsPerWeek { get; set; }
}

/// <summary>
/// Represents an achievement or milestone
/// </summary>
public class Achievement
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly AchievedDate { get; set; }
    public string Type { get; set; } = string.Empty; // "PR", "Streak", "Volume", etc.
}
