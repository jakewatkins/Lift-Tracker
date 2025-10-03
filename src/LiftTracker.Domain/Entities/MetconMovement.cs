using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Domain.Entities;

/// <summary>
/// Individual movements within a metcon workout
/// </summary>
public class MetconMovement
{
    /// <summary>
    /// Unique movement identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the metcon workout
    /// </summary>
    public Guid MetconWorkoutId { get; set; }

    /// <summary>
    /// Reference to the movement type
    /// </summary>
    public int MovementTypeId { get; set; }

    /// <summary>
    /// Number of reps (for rep-based movements)
    /// </summary>
    public int? Reps { get; set; }

    /// <summary>
    /// Distance in meters (for distance-based movements)
    /// </summary>
    public decimal? Distance { get; set; }

    /// <summary>
    /// Weight used in pounds
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Order within metcon
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Navigation property to the metcon workout
    /// </summary>
    public virtual MetconWorkout MetconWorkout { get; set; } = null!;

    /// <summary>
    /// Navigation property to the movement type
    /// </summary>
    public virtual MovementType MovementType { get; set; } = null!;

    /// <summary>
    /// Creates a new metcon movement with generated ID
    /// </summary>
    public MetconMovement()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Validates that reps are within valid range for rep-based movements
    /// </summary>
    /// <returns>True if reps are valid</returns>
    public bool IsValidReps()
    {
        return Reps == null || (Reps >= 1 && Reps <= 10000);
    }

    /// <summary>
    /// Validates that distance is positive for distance-based movements
    /// </summary>
    /// <returns>True if distance is valid</returns>
    public bool IsValidDistance()
    {
        return Distance == null || Distance > 0;
    }

    /// <summary>
    /// Validates that weight uses only fractional increments of 0.25
    /// </summary>
    /// <returns>True if weight is valid</returns>
    public bool IsValidWeight()
    {
        if (Weight == null) return true;

        var fractionalPart = Weight.Value - Math.Floor(Weight.Value);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart) && Weight >= 0;
    }

    /// <summary>
    /// Validates that the movement has the correct measurement based on MovementType
    /// </summary>
    /// <returns>True if measurement is valid for the movement type</returns>
    public bool HasValidMeasurement()
    {
        if (MovementType == null) return false;

        if (MovementType.IsRepBased)
        {
            return Reps.HasValue && !Distance.HasValue;
        }
        else if (MovementType.IsDistanceBased)
        {
            return Distance.HasValue && !Reps.HasValue;
        }

        return false;
    }
}
