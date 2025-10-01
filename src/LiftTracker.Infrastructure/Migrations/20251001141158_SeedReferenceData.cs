using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LiftTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExerciseTypes",
                columns: new[] { "Id", "Category", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Squat", true, "Back Squat" },
                    { 2, "Squat", true, "Front Squat" },
                    { 3, "Squat", true, "Overhead Squat" },
                    { 4, "Squat", true, "Box Squat" },
                    { 5, "Squat", true, "Goblet Squat" },
                    { 6, "Deadlift", true, "Deadlift" },
                    { 7, "Deadlift", true, "Sumo Deadlift" },
                    { 8, "Deadlift", true, "Romanian Deadlift" },
                    { 9, "Deadlift", true, "Stiff Leg Deadlift" },
                    { 10, "Deadlift", true, "Trap Bar Deadlift" },
                    { 11, "Press", true, "Bench Press" },
                    { 12, "Press", true, "Overhead Press" },
                    { 13, "Press", true, "Incline Bench Press" },
                    { 14, "Press", true, "Dumbbell Press" },
                    { 15, "Press", true, "Push Press" },
                    { 16, "Press", true, "Jerk" },
                    { 17, "Olympic", true, "Clean" },
                    { 18, "Olympic", true, "Snatch" },
                    { 19, "Olympic", true, "Clean and Jerk" },
                    { 20, "Olympic", true, "Power Clean" },
                    { 21, "Olympic", true, "Power Snatch" },
                    { 22, "Row", true, "Bent Over Row" },
                    { 23, "Row", true, "T-Bar Row" },
                    { 24, "Row", true, "Seated Row" },
                    { 25, "Row", true, "Pendlay Row" },
                    { 26, "Accessory", true, "Pull-ups" },
                    { 27, "Accessory", true, "Chin-ups" },
                    { 28, "Accessory", true, "Dips" },
                    { 29, "Accessory", true, "Lunges" },
                    { 30, "Accessory", true, "Hip Thrust" }
                });

            migrationBuilder.InsertData(
                table: "MetconTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "As Many Rounds As Possible - Complete as many rounds of the given movements within the time limit", true, "AMRAP" },
                    { 2, "Complete the prescribed work as fast as possible", true, "For Time" },
                    { 3, "Every Minute On the Minute - Perform specified work at the start of every minute", true, "EMOM" },
                    { 4, "20 seconds of work followed by 10 seconds of rest, repeated for 8 rounds (4 minutes)", true, "Tabata" },
                    { 5, "Work through a long list of exercises and repetitions in order, 'chipping away' at the work", true, "Chipper" },
                    { 6, "Increase or decrease reps with each round", true, "Ladder" },
                    { 7, "Start with 1 rep in minute 1, 2 reps in minute 2, continue until failure", true, "Death By" },
                    { 8, "Custom workout format not fitting standard categories", true, "Custom" }
                });

            migrationBuilder.InsertData(
                table: "MovementTypes",
                columns: new[] { "Id", "Category", "IsActive", "MeasurementType", "Name" },
                values: new object[,]
                {
                    { 1, "Bodyweight", true, "", "Burpees" },
                    { 2, "Bodyweight", true, "", "Push-ups" },
                    { 3, "Bodyweight", true, "", "Air Squats" },
                    { 4, "Bodyweight", true, "", "Mountain Climbers" },
                    { 5, "Bodyweight", true, "", "Jumping Jacks" },
                    { 6, "Bodyweight", true, "", "High Knees" },
                    { 7, "Bodyweight", true, "", "Butt Kickers" },
                    { 8, "Bodyweight", true, "", "Plank" },
                    { 9, "Bodyweight", true, "", "Sit-ups" },
                    { 10, "Bodyweight", true, "", "Lunges" },
                    { 11, "Cardio", true, "", "Running" },
                    { 12, "Cardio", true, "", "Rowing" },
                    { 13, "Cardio", true, "", "Biking" },
                    { 14, "Cardio", true, "", "Jump Rope" },
                    { 15, "Cardio", true, "", "Box Steps" },
                    { 16, "Weighted", true, "", "Thrusters" },
                    { 17, "Weighted", true, "", "Wall Balls" },
                    { 18, "Weighted", true, "", "Kettlebell Swings" },
                    { 19, "Weighted", true, "", "Dumbbell Snatches" },
                    { 20, "Weighted", true, "", "Deadlifts" },
                    { 21, "Weighted", true, "", "Box Jumps" },
                    { 22, "Weighted", true, "", "Pull-ups" },
                    { 23, "Weighted", true, "", "Ring Dips" },
                    { 24, "Weighted", true, "", "Handstand Push-ups" },
                    { 25, "Weighted", true, "", "Toes to Bar" },
                    { 26, "Core", true, "", "Russian Twists" },
                    { 27, "Core", true, "", "Bicycle Crunches" },
                    { 28, "Core", true, "", "Dead Bugs" },
                    { 29, "Core", true, "", "Bird Dogs" },
                    { 30, "Core", true, "", "Hollow Holds" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "MetconTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MetconTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MetconTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MetconTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MetconTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MetconTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MetconTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MetconTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 30);
        }
    }
}
