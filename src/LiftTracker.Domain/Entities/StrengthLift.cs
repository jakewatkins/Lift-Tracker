using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Domain.Entities;

/// <summary>
/// Records individual strength training sets within a workout session
/// </summary>
public class StrengthLift
{
    /// <summary>
    /// Unique lift identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the workout session
    /// </summary>
    public Guid WorkoutSessionId { get; set; }

    /// <summary>
    /// Reference to the exercise type
    /// </summary>
    public int ExerciseTypeId { get; set; }

    /// <summary>
    /// Set type: "SetsReps", "EMOM", "AMRAP", "TimeBased"
    /// </summary>
    [Required]
    [StringLength(20)]
    public string SetStructure { get; set; } = string.Empty;

    /// <summary>
    /// Number of sets (null for AMRAP/TimeBased)
    /// </summary>
    public int? Sets { get; set; }

    /// <summary>
    /// Reps per set (null for time-based)
    /// </summary>
    public int? Reps { get; set; }

    /// <summary>
    /// Weight in pounds (0 for bodyweight)
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Extra weight for bodyweight exercises
    /// </summary>
    public decimal? AdditionalWeight { get; set; }

    /// <summary>
    /// Set duration in minutes
    /// </summary>
    public decimal? Duration { get; set; }

    /// <summary>
    /// Rest between sets in minutes
    /// </summary>
    public decimal? RestPeriod { get; set; }

    /// <summary>
    /// Set-specific notes
    /// </summary>
    [StringLength(500)]
    public string? Comments { get; set; }

    /// <summary>
    /// Order within workout session
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Navigation property to the workout session
    /// </summary>
    public virtual WorkoutSession WorkoutSession { get; set; } = null!;

    /// <summary>
    /// Navigation property to the exercise type
    /// </summary>
    public virtual ExerciseType ExerciseType { get; set; } = null!;

    /// <summary>
    /// Creates a new strength lift with generated ID
    /// </summary>
    public StrengthLift()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Validates that weight uses only fractional increments of 0.25
    /// </summary>
    /// <returns>True if weight is valid</returns>
    public bool IsValidWeight()
    {
        var fractionalPart = Weight - Math.Floor(Weight);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart) && Weight >= 0;
    }

    /// <summary>
    /// Validates that additional weight uses only fractional increments of 0.25
    /// </summary>
    /// <returns>True if additional weight is valid</returns>
    public bool IsValidAdditionalWeight()
    {
        if (AdditionalWeight == null) return true;
        
        var fractionalPart = AdditionalWeight.Value - Math.Floor(AdditionalWeight.Value);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart) && AdditionalWeight >= 0;
    }

    /// <summary>
    /// Validates that duration uses only fractional increments of 0.25
    /// </summary>
    /// <returns>True if duration is valid</returns>
    public bool IsValidDuration()
    {
        if (Duration == null) return true;
        
        var fractionalPart = Duration.Value - Math.Floor(Duration.Value);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart) && Duration >= 0;
    }

    /// <summary>
    /// Validates that rest period uses only fractional increments of 0.25
    /// </summary>
    /// <returns>True if rest period is valid</returns>
    public bool IsValidRestPeriod()
    {
        if (RestPeriod == null) return true;
        
        var fractionalPart = RestPeriod.Value - Math.Floor(RestPeriod.Value);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart) && RestPeriod >= 0;
    }

    /// <summary>
    /// Validates that sets are within valid range
    /// </summary>
    /// <returns>True if sets are valid</returns>
    public bool IsValidSets()
    {
        return Sets == null || (Sets >= 1 && Sets <= 50);
    }

    /// <summary>
    /// Validates that reps are within valid range
    /// </summary>
    /// <returns>True if reps are valid</returns>
    public bool IsValidReps()
    {
        return Reps == null || (Reps >= 1 && Reps <= 500);
    }
}