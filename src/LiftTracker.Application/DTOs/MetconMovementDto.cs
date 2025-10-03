using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Application.DTOs;

/// <summary>
/// Data transfer object for metcon movement information
/// </summary>
public class MetconMovementDto
{
    public Guid Id { get; set; }

    public Guid MetconWorkoutId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Movement type ID must be positive")]
    public int MovementTypeId { get; set; }

    public string? MovementTypeName { get; set; }

    [Required]
    [Range(1, 9999, ErrorMessage = "Reps must be between 1 and 9999")]
    public int Reps { get; set; }

    [Range(0.01, 9999.99, ErrorMessage = "Weight must be between 0.01 and 9999.99")]
    public decimal? Weight { get; set; }

    [Range(0.01, 9999.99, ErrorMessage = "Distance must be between 0.01 and 9999.99")]
    public decimal? Distance { get; set; }
}

/// <summary>
/// Data transfer object for creating a new metcon movement
/// </summary>
public class CreateMetconMovementDto
{
    [Required]
    public Guid MetconWorkoutId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Movement type ID must be positive")]
    public int MovementTypeId { get; set; }

    [Required]
    [Range(1, 9999, ErrorMessage = "Reps must be between 1 and 9999")]
    public int Reps { get; set; }

    [Range(0.01, 9999.99, ErrorMessage = "Weight must be between 0.01 and 9999.99")]
    public decimal? Weight { get; set; }

    [Range(0.01, 9999.99, ErrorMessage = "Distance must be between 0.01 and 9999.99")]
    public decimal? Distance { get; set; }
}

/// <summary>
/// Data transfer object for updating a metcon movement
/// </summary>
public class UpdateMetconMovementDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Movement type ID must be positive")]
    public int MovementTypeId { get; set; }

    [Required]
    [Range(1, 9999, ErrorMessage = "Reps must be between 1 and 9999")]
    public int Reps { get; set; }

    [Range(0.01, 9999.99, ErrorMessage = "Weight must be between 0.01 and 9999.99")]
    public decimal? Weight { get; set; }

    [Range(0.01, 9999.99, ErrorMessage = "Distance must be between 0.01 and 9999.99")]
    public decimal? Distance { get; set; }
}
