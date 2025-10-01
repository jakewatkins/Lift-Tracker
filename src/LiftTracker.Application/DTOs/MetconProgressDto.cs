namespace LiftTracker.Application.DTOs;

/// <summary>
/// Data transfer object for metcon workout progress data points
/// </summary>
public class MetconProgressDto
{
    public DateOnly Date { get; set; }
    public decimal Value { get; set; }
    public TimeSpan? Duration { get; set; }
    public int? Rounds { get; set; }
    public int? Reps { get; set; }
    public Guid WorkoutSessionId { get; set; }
    public string MetconTypeName { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty; // "time", "rounds", "reps"
}
