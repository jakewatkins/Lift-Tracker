namespace LiftTracker.Application.DTOs;

/// <summary>
/// Data transfer object for exercise type information
/// </summary>
public class ExerciseTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Equipment { get; set; }
}

/// <summary>
/// Data transfer object for metcon type information
/// </summary>
public class MetconTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsTimeBased { get; set; }
    public bool IsRoundBased { get; set; }
}

/// <summary>
/// Data transfer object for movement type information
/// </summary>
public class MovementTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool RequiresWeight { get; set; }
    public bool RequiresDistance { get; set; }
}
