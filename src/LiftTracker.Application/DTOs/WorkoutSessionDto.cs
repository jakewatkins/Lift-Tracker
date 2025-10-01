using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Application.DTOs;

/// <summary>
/// Data transfer object for workout session information
/// </summary>
public class WorkoutSessionDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public List<StrengthLiftDto> StrengthLifts { get; set; } = new();

    public List<MetconWorkoutDto> MetconWorkouts { get; set; } = new();
}

/// <summary>
/// Data transfer object for creating a new workout session
/// </summary>
public class CreateWorkoutSessionDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Data transfer object for updating a workout session
/// </summary>
public class UpdateWorkoutSessionDto
{
    [Required]
    public DateOnly Date { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Data transfer object for workout session summary (without detailed lifts/workouts)
/// </summary>
public class WorkoutSessionSummaryDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public int StrengthLiftCount { get; set; }
    public int MetconWorkoutCount { get; set; }
    public decimal? TotalVolumeLifted { get; set; }
}
