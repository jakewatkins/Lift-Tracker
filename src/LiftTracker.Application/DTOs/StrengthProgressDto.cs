namespace LiftTracker.Application.DTOs;

/// <summary>
/// Data transfer object for strength exercise progress data points
/// </summary>
public class StrengthProgressDto
{
    public DateOnly Date { get; set; }
    public decimal Value { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal Weight { get; set; }
    public Guid WorkoutSessionId { get; set; }
    public string ExerciseTypeName { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty; // "weight", "volume", "maxWeight"
}
