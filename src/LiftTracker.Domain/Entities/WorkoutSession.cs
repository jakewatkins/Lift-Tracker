using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Domain.Entities;

/// <summary>
/// Represents a workout session containing strength lifts and metcon workouts
/// </summary>
public class WorkoutSession
{
    /// <summary>
    /// Unique session identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the user who owns this session
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Date when the workout occurred (no time component)
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Optional notes about the workout session
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Navigation property for strength lifts in this session
    /// </summary>
    public virtual ICollection<StrengthLift> StrengthLifts { get; set; } = new List<StrengthLift>();

    /// <summary>
    /// Navigation property for metcon workouts in this session
    /// </summary>
    public virtual ICollection<MetconWorkout> MetconWorkouts { get; set; } = new List<MetconWorkout>();

    /// <summary>
    /// Creates a new workout session with generated ID
    /// </summary>
    public WorkoutSession()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Creates a new workout session for a specific user and date
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="date">Date of the workout</param>
    public WorkoutSession(Guid userId, DateOnly date) : this()
    {
        UserId = userId;
        Date = date;
    }

    /// <summary>
    /// Validates that the workout date is not in the future
    /// </summary>
    /// <returns>True if the date is valid</returns>
    public bool IsValidDate()
    {
        return Date <= DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
