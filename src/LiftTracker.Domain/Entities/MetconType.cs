using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Domain.Entities;

/// <summary>
/// Predefined catalog of metabolic conditioning workout types
/// </summary>
public class MetconType
{
    /// <summary>
    /// Unique metcon type identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Metcon type name
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type description
    /// </summary>
    [StringLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this metcon type is available for selection
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for metcon workouts using this type
    /// </summary>
    public virtual ICollection<MetconWorkout> MetconWorkouts { get; set; } = new List<MetconWorkout>();

    /// <summary>
    /// Creates a new metcon type
    /// </summary>
    public MetconType()
    {
    }

    /// <summary>
    /// Creates a new metcon type with specified name
    /// </summary>
    /// <param name="name">Metcon type name</param>
    public MetconType(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Creates a new metcon type with specified name and description
    /// </summary>
    /// <param name="name">Metcon type name</param>
    /// <param name="description">Type description</param>
    public MetconType(string name, string description) : this(name)
    {
        Description = description;
    }
}