using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Domain.Entities;

/// <summary>
/// Predefined catalog of strength exercises
/// </summary>
public class ExerciseType
{
    /// <summary>
    /// Unique exercise identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Exercise name (e.g., "Back Squat")
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Exercise category (e.g., "Squat", "Press")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Whether this exercise is available for selection
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for strength lifts using this exercise
    /// </summary>
    public virtual ICollection<StrengthLift> StrengthLifts { get; set; } = new List<StrengthLift>();

    /// <summary>
    /// Creates a new exercise type
    /// </summary>
    public ExerciseType()
    {
    }

    /// <summary>
    /// Creates a new exercise type with specified name and category
    /// </summary>
    /// <param name="name">Exercise name</param>
    /// <param name="category">Exercise category</param>
    public ExerciseType(string name, string category)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category ?? throw new ArgumentNullException(nameof(category));
    }
}
