using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Domain.Entities;

/// <summary>
/// Catalog of movements used in metcon workouts
/// </summary>
public class MovementType
{
    /// <summary>
    /// Unique movement identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Movement name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Movement category
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// "Reps" or "Distance"
    /// </summary>
    [Required]
    [StringLength(20)]
    public string MeasurementType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this movement is available for selection
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for metcon movements using this type
    /// </summary>
    public virtual ICollection<MetconMovement> MetconMovements { get; set; } = new List<MetconMovement>();

    /// <summary>
    /// Creates a new movement type
    /// </summary>
    public MovementType()
    {
    }

    /// <summary>
    /// Creates a new movement type with specified properties
    /// </summary>
    /// <param name="name">Movement name</param>
    /// <param name="category">Movement category</param>
    /// <param name="measurementType">Measurement type (Reps or Distance)</param>
    public MovementType(string name, string category, string measurementType)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        MeasurementType = measurementType ?? throw new ArgumentNullException(nameof(measurementType));

        if (measurementType != "Reps" && measurementType != "Distance")
        {
            throw new ArgumentException("MeasurementType must be 'Reps' or 'Distance'", nameof(measurementType));
        }
    }

    /// <summary>
    /// Checks if this movement is measured by repetitions
    /// </summary>
    public bool IsRepBased => MeasurementType == "Reps";

    /// <summary>
    /// Checks if this movement is measured by distance
    /// </summary>
    public bool IsDistanceBased => MeasurementType == "Distance";
}
