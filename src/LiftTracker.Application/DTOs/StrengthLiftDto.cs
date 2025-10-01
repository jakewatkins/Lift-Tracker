using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Application.DTOs;

/// <summary>
/// Data transfer object for strength lift information
/// </summary>
public class StrengthLiftDto
{
    public Guid Id { get; set; }

    public Guid WorkoutSessionId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Exercise type ID must be positive")]
    public int ExerciseTypeId { get; set; }

    public string? ExerciseTypeName { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "Set structure cannot exceed 20 characters")]
    public string SetStructure { get; set; } = string.Empty;

    [Range(1, 50, ErrorMessage = "Sets must be between 1 and 50")]
    public int? Sets { get; set; }

    [Range(1, 500, ErrorMessage = "Reps must be between 1 and 500")]
    public int? Reps { get; set; }

    [Required]
    [Range(0, 9999.99, ErrorMessage = "Weight must be between 0 and 9999.99")]
    public decimal Weight { get; set; }

    [Range(0, 9999.99, ErrorMessage = "Additional weight must be between 0 and 9999.99")]
    public decimal? AdditionalWeight { get; set; }

    [Range(0.01, 999.99, ErrorMessage = "Duration must be between 0.01 and 999.99 minutes")]
    public decimal? Duration { get; set; }

    [Range(0.01, 99.99, ErrorMessage = "Rest period must be between 0.01 and 99.99 minutes")]
    public decimal? RestPeriod { get; set; }

    [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
    public string? Comments { get; set; }

    public int Order { get; set; }

    /// <summary>
    /// Calculated total volume (Weight * Reps * Sets) - only applies to SetsReps structure
    /// </summary>
    public decimal? TotalVolume => SetStructure == "SetsReps" && Sets.HasValue && Reps.HasValue ? Weight * Reps.Value * Sets.Value : null;
}

/// <summary>
/// Data transfer object for creating a new strength lift
/// </summary>
public class CreateStrengthLiftDto
{
    [Required]
    public Guid WorkoutSessionId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Exercise type ID must be positive")]
    public int ExerciseTypeId { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "Set structure cannot exceed 20 characters")]
    public string SetStructure { get; set; } = string.Empty;

    [Range(1, 50, ErrorMessage = "Sets must be between 1 and 50")]
    public int? Sets { get; set; }

    [Range(1, 500, ErrorMessage = "Reps must be between 1 and 500")]
    public int? Reps { get; set; }

    [Required]
    [Range(0, 9999.99, ErrorMessage = "Weight must be between 0 and 9999.99")]
    public decimal Weight { get; set; }

    [Range(0, 9999.99, ErrorMessage = "Additional weight must be between 0 and 9999.99")]
    public decimal? AdditionalWeight { get; set; }

    [Range(0.01, 999.99, ErrorMessage = "Duration must be between 0.01 and 999.99 minutes")]
    public decimal? Duration { get; set; }

    [Range(0.01, 99.99, ErrorMessage = "Rest period must be between 0.01 and 99.99 minutes")]
    public decimal? RestPeriod { get; set; }

    [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
    public string? Comments { get; set; }

    public int Order { get; set; }
}

/// <summary>
/// Data transfer object for updating a strength lift
/// </summary>
public class UpdateStrengthLiftDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Exercise type ID must be positive")]
    public int ExerciseTypeId { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "Set structure cannot exceed 20 characters")]
    public string SetStructure { get; set; } = string.Empty;

    [Range(1, 50, ErrorMessage = "Sets must be between 1 and 50")]
    public int? Sets { get; set; }

    [Range(1, 500, ErrorMessage = "Reps must be between 1 and 500")]
    public int? Reps { get; set; }

    [Required]
    [Range(0, 9999.99, ErrorMessage = "Weight must be between 0 and 9999.99")]
    public decimal Weight { get; set; }

    [Range(0, 9999.99, ErrorMessage = "Additional weight must be between 0 and 9999.99")]
    public decimal? AdditionalWeight { get; set; }

    [Range(0.01, 999.99, ErrorMessage = "Duration must be between 0.01 and 999.99 minutes")]
    public decimal? Duration { get; set; }

    [Range(0.01, 99.99, ErrorMessage = "Rest period must be between 0.01 and 99.99 minutes")]
    public decimal? RestPeriod { get; set; }

    [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
    public string? Comments { get; set; }

    public int Order { get; set; }
}
