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
}