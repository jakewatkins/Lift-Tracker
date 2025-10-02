using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Domain.Entities;

/// <summary>
/// Records metabolic conditioning workouts within a session
/// </summary>
public class MetconWorkout
{
    /// <summary>
    /// Unique metcon identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the workout session
    /// </summary>
    public Guid WorkoutSessionId { get; set; }

    /// <summary>
    /// Reference to the metcon type
    /// </summary>
    public int MetconTypeId { get; set; }

    /// <summary>
    /// Total workout time in minutes
    /// </summary>
    public decimal? TotalTime { get; set; }

    /// <summary>
    /// Rounds completed (for AMRAP)
    /// </summary>
    public int? RoundsCompleted { get; set; }

    /// <summary>
    /// Workout notes
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Order within workout session
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Navigation property to the workout session
    /// </summary>
    public virtual WorkoutSession WorkoutSession { get; set; } = null!;

    /// <summary>
    /// Navigation property to the metcon type
    /// </summary>
    public virtual MetconType MetconType { get; set; } = null!;

    /// <summary>
    /// Navigation property for movements in this metcon
    /// </summary>
    public virtual ICollection<MetconMovement> MetconMovements { get; set; } = new List<MetconMovement>();

    /// <summary>
    /// Number of rounds for the workout
    /// </summary>
    [Range(1, 100, ErrorMessage = "Rounds must be between 1 and 100")]
    public int? Rounds { get; set; }

    /// <summary>
    /// Time cap for the workout in minutes
    /// </summary>
    [Range(0.25, 240, ErrorMessage = "Time cap must be between 0.25 and 240 minutes")]
    public decimal? TimeCapMinutes { get; set; }

    /// <summary>
    /// Actual time taken to complete the workout in minutes
    /// </summary>
    [Range(0, 240, ErrorMessage = "Actual time must be between 0 and 240 minutes")]
    public decimal? ActualTimeMinutes { get; set; }

    /// <summary>
    /// Rest time between rounds in minutes
    /// </summary>
    [Range(0, 30, ErrorMessage = "Rest between rounds must be between 0 and 30 minutes")]
    public decimal? RestBetweenRounds { get; set; }

    /// <summary>
    /// Comments about the workout
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
    public string? Comments { get; set; }

    /// <summary>
    /// Creates a new metcon workout with generated ID
    /// </summary>
    public MetconWorkout()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Validates that total time uses only fractional increments of 0.25
    /// </summary>
    /// <returns>True if total time is valid</returns>
    public bool IsValidTotalTime()
    {
        if (TotalTime == null) return true;

        var fractionalPart = TotalTime.Value - Math.Floor(TotalTime.Value);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart) && TotalTime >= 0;
    }

    /// <summary>
    /// Validates that rounds completed are within valid range
    /// </summary>
    /// <returns>True if rounds completed are valid</returns>
    public bool IsValidRoundsCompleted()
    {
        return RoundsCompleted == null || (RoundsCompleted >= 0 && RoundsCompleted <= 1000);
    }

    /// <summary>
    /// Validates that rounds are within valid range
    /// </summary>
    /// <returns>True if rounds are valid</returns>
    public bool IsValidRounds()
    {
        return Rounds == null || (Rounds >= 1 && Rounds <= 100);
    }

    /// <summary>
    /// Validates that time cap uses only fractional increments of 0.25
    /// </summary>
    /// <returns>True if time cap is valid</returns>
    public bool IsValidTimeCapMinutes()
    {
        if (TimeCapMinutes == null) return true;

        var fractionalPart = TimeCapMinutes.Value - Math.Floor(TimeCapMinutes.Value);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart) && TimeCapMinutes >= 0.25m && TimeCapMinutes <= 240m;
    }

    /// <summary>
    /// Validates that actual time uses only fractional increments of 0.25
    /// </summary>
    /// <returns>True if actual time is valid</returns>
    public bool IsValidActualTimeMinutes()
    {
        if (ActualTimeMinutes == null) return true;

        var fractionalPart = ActualTimeMinutes.Value - Math.Floor(ActualTimeMinutes.Value);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart) && ActualTimeMinutes >= 0 && ActualTimeMinutes <= 240m;
    }

    /// <summary>
    /// Validates that rest between rounds uses only fractional increments of 0.25
    /// </summary>
    /// <returns>True if rest between rounds is valid</returns>
    public bool IsValidRestBetweenRounds()
    {
        if (RestBetweenRounds == null) return true;

        var fractionalPart = RestBetweenRounds.Value - Math.Floor(RestBetweenRounds.Value);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart) && RestBetweenRounds >= 0 && RestBetweenRounds <= 30m;
    }
}
