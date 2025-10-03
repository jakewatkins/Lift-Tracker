using LiftTracker.Domain.Entities;

namespace LiftTracker.Infrastructure.Data;

/// <summary>
/// Contains seed data for reference entities
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Gets the seed data for exercise types
    /// </summary>
    public static readonly ExerciseType[] ExerciseTypes = new[]
    {
        // Squat variations
        new ExerciseType { Id = 1, Name = "Back Squat", Category = "Squat", IsActive = true },
        new ExerciseType { Id = 2, Name = "Front Squat", Category = "Squat", IsActive = true },
        new ExerciseType { Id = 3, Name = "Overhead Squat", Category = "Squat", IsActive = true },
        new ExerciseType { Id = 4, Name = "Box Squat", Category = "Squat", IsActive = true },
        new ExerciseType { Id = 5, Name = "Goblet Squat", Category = "Squat", IsActive = true },

        // Deadlift variations
        new ExerciseType { Id = 6, Name = "Deadlift", Category = "Deadlift", IsActive = true },
        new ExerciseType { Id = 7, Name = "Sumo Deadlift", Category = "Deadlift", IsActive = true },
        new ExerciseType { Id = 8, Name = "Romanian Deadlift", Category = "Deadlift", IsActive = true },
        new ExerciseType { Id = 9, Name = "Stiff Leg Deadlift", Category = "Deadlift", IsActive = true },
        new ExerciseType { Id = 10, Name = "Trap Bar Deadlift", Category = "Deadlift", IsActive = true },

        // Press variations
        new ExerciseType { Id = 11, Name = "Bench Press", Category = "Press", IsActive = true },
        new ExerciseType { Id = 12, Name = "Overhead Press", Category = "Press", IsActive = true },
        new ExerciseType { Id = 13, Name = "Incline Bench Press", Category = "Press", IsActive = true },
        new ExerciseType { Id = 14, Name = "Dumbbell Press", Category = "Press", IsActive = true },
        new ExerciseType { Id = 15, Name = "Push Press", Category = "Press", IsActive = true },
        new ExerciseType { Id = 16, Name = "Jerk", Category = "Press", IsActive = true },

        // Olympic lifts
        new ExerciseType { Id = 17, Name = "Clean", Category = "Olympic", IsActive = true },
        new ExerciseType { Id = 18, Name = "Snatch", Category = "Olympic", IsActive = true },
        new ExerciseType { Id = 19, Name = "Clean and Jerk", Category = "Olympic", IsActive = true },
        new ExerciseType { Id = 20, Name = "Power Clean", Category = "Olympic", IsActive = true },
        new ExerciseType { Id = 21, Name = "Power Snatch", Category = "Olympic", IsActive = true },

        // Row variations
        new ExerciseType { Id = 22, Name = "Bent Over Row", Category = "Row", IsActive = true },
        new ExerciseType { Id = 23, Name = "T-Bar Row", Category = "Row", IsActive = true },
        new ExerciseType { Id = 24, Name = "Seated Row", Category = "Row", IsActive = true },
        new ExerciseType { Id = 25, Name = "Pendlay Row", Category = "Row", IsActive = true },

        // Accessory movements
        new ExerciseType { Id = 26, Name = "Pull-ups", Category = "Accessory", IsActive = true },
        new ExerciseType { Id = 27, Name = "Chin-ups", Category = "Accessory", IsActive = true },
        new ExerciseType { Id = 28, Name = "Dips", Category = "Accessory", IsActive = true },
        new ExerciseType { Id = 29, Name = "Lunges", Category = "Accessory", IsActive = true },
        new ExerciseType { Id = 30, Name = "Hip Thrust", Category = "Accessory", IsActive = true }
    };

    /// <summary>
    /// Gets the seed data for metcon types
    /// </summary>
    public static readonly MetconType[] MetconTypes = new[]
    {
        new MetconType { Id = 1, Name = "AMRAP", Description = "As Many Rounds As Possible - Complete as many rounds of the given movements within the time limit", IsActive = true },
        new MetconType { Id = 2, Name = "For Time", Description = "Complete the prescribed work as fast as possible", IsActive = true },
        new MetconType { Id = 3, Name = "EMOM", Description = "Every Minute On the Minute - Perform specified work at the start of every minute", IsActive = true },
        new MetconType { Id = 4, Name = "Tabata", Description = "20 seconds of work followed by 10 seconds of rest, repeated for 8 rounds (4 minutes)", IsActive = true },
        new MetconType { Id = 5, Name = "Chipper", Description = "Work through a long list of exercises and repetitions in order, 'chipping away' at the work", IsActive = true },
        new MetconType { Id = 6, Name = "Ladder", Description = "Increase or decrease reps with each round", IsActive = true },
        new MetconType { Id = 7, Name = "Death By", Description = "Start with 1 rep in minute 1, 2 reps in minute 2, continue until failure", IsActive = true },
        new MetconType { Id = 8, Name = "Custom", Description = "Custom workout format not fitting standard categories", IsActive = true }
    };

    /// <summary>
    /// Gets the seed data for movement types
    /// </summary>
    public static readonly MovementType[] MovementTypes = new[]
    {
        // Bodyweight movements
        new MovementType { Id = 1, Name = "Burpees", Category = "Bodyweight", IsActive = true },
        new MovementType { Id = 2, Name = "Push-ups", Category = "Bodyweight", IsActive = true },
        new MovementType { Id = 3, Name = "Air Squats", Category = "Bodyweight", IsActive = true },
        new MovementType { Id = 4, Name = "Mountain Climbers", Category = "Bodyweight", IsActive = true },
        new MovementType { Id = 5, Name = "Jumping Jacks", Category = "Bodyweight", IsActive = true },
        new MovementType { Id = 6, Name = "High Knees", Category = "Bodyweight", IsActive = true },
        new MovementType { Id = 7, Name = "Butt Kickers", Category = "Bodyweight", IsActive = true },
        new MovementType { Id = 8, Name = "Plank", Category = "Bodyweight", IsActive = true },
        new MovementType { Id = 9, Name = "Sit-ups", Category = "Bodyweight", IsActive = true },
        new MovementType { Id = 10, Name = "Lunges", Category = "Bodyweight", IsActive = true },

        // Cardio movements
        new MovementType { Id = 11, Name = "Running", Category = "Cardio", IsActive = true },
        new MovementType { Id = 12, Name = "Rowing", Category = "Cardio", IsActive = true },
        new MovementType { Id = 13, Name = "Biking", Category = "Cardio", IsActive = true },
        new MovementType { Id = 14, Name = "Jump Rope", Category = "Cardio", IsActive = true },
        new MovementType { Id = 15, Name = "Box Steps", Category = "Cardio", IsActive = true },

        // Weighted movements
        new MovementType { Id = 16, Name = "Thrusters", Category = "Weighted", IsActive = true },
        new MovementType { Id = 17, Name = "Wall Balls", Category = "Weighted", IsActive = true },
        new MovementType { Id = 18, Name = "Kettlebell Swings", Category = "Weighted", IsActive = true },
        new MovementType { Id = 19, Name = "Dumbbell Snatches", Category = "Weighted", IsActive = true },
        new MovementType { Id = 20, Name = "Deadlifts", Category = "Weighted", IsActive = true },
        new MovementType { Id = 21, Name = "Box Jumps", Category = "Weighted", IsActive = true },
        new MovementType { Id = 22, Name = "Pull-ups", Category = "Weighted", IsActive = true },
        new MovementType { Id = 23, Name = "Ring Dips", Category = "Weighted", IsActive = true },
        new MovementType { Id = 24, Name = "Handstand Push-ups", Category = "Weighted", IsActive = true },
        new MovementType { Id = 25, Name = "Toes to Bar", Category = "Weighted", IsActive = true },

        // Core movements
        new MovementType { Id = 26, Name = "Russian Twists", Category = "Core", IsActive = true },
        new MovementType { Id = 27, Name = "Bicycle Crunches", Category = "Core", IsActive = true },
        new MovementType { Id = 28, Name = "Dead Bugs", Category = "Core", IsActive = true },
        new MovementType { Id = 29, Name = "Bird Dogs", Category = "Core", IsActive = true },
        new MovementType { Id = 30, Name = "Hollow Holds", Category = "Core", IsActive = true }
    };
}
