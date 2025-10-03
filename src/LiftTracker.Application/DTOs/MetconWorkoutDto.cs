using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Application.DTOs;

/// <summary>
/// Data transfer object for metcon workout information
/// </summary>
public class MetconWorkoutDto
{
    public Guid Id { get; set; }

    public Guid WorkoutSessionId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Metcon type ID must be positive")]
    public int MetconTypeId { get; set; }

    public string? MetconTypeName { get; set; }

    [Range(0.01, 999.99, ErrorMessage = "Total time must be between 0.01 and 999.99 minutes")]
    public decimal? TotalTime { get; set; }

    [Range(1, 9999, ErrorMessage = "Rounds completed must be between 1 and 9999")]
    public int? RoundsCompleted { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public int Order { get; set; }

    public List<MetconMovementDto> Movements { get; set; } = new();
}

/// <summary>
/// Data transfer object for creating a new metcon workout
/// </summary>
public class CreateMetconWorkoutDto
{
    [Required]
    public Guid WorkoutSessionId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Metcon type ID must be positive")]
    public int MetconTypeId { get; set; }

    [Range(0.01, 999.99, ErrorMessage = "Total time must be between 0.01 and 999.99 minutes")]
    public decimal? TotalTime { get; set; }

    [Range(1, 9999, ErrorMessage = "Rounds completed must be between 1 and 9999")]
    public int? RoundsCompleted { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public int Order { get; set; }
}

/// <summary>
/// Data transfer object for updating a metcon workout
/// </summary>
public class UpdateMetconWorkoutDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Metcon type ID must be positive")]
    public int MetconTypeId { get; set; }

    [Range(0.01, 999.99, ErrorMessage = "Total time must be between 0.01 and 999.99 minutes")]
    public decimal? TotalTime { get; set; }

    [Range(1, 9999, ErrorMessage = "Rounds completed must be between 1 and 9999")]
    public int? RoundsCompleted { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public int Order { get; set; }
}
