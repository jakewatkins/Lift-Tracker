namespace LiftTracker.Application.DTOs;

/// <summary>
/// Data transfer object for overall user progress summary
/// </summary>
public class ProgressDto
{
    public int TotalWorkouts { get; set; }
    public int TotalWorkoutDays { get; set; }
    public int TotalStrengthLifts { get; set; }
    public int TotalMetconWorkouts { get; set; }
    public decimal AverageWorkoutsPerWeek { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastWorkoutDate { get; set; }
    public List<PersonalRecordDto> RecentPersonalRecords { get; set; } = new();
}

/// <summary>
/// Data transfer object for personal record information
/// </summary>
public class PersonalRecordDto
{
    public int ExerciseTypeId { get; set; }
    public string ExerciseTypeName { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public int Reps { get; set; }
    public int Sets { get; set; }
    public DateOnly AchievedDate { get; set; }
}
