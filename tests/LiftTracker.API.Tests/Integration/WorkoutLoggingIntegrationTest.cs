using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiftTracker.API.Tests.Integration;

/// <summary>
/// Integration test for the workout logging user story.
/// User Story: As a user, I want to log my workouts so that I can track my progress over time.
/// This test validates the complete end-to-end flow of logging strength and metcon workouts.
/// Test should fail until workout management implementation is complete.
/// </summary>
public class WorkoutLoggingIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public WorkoutLoggingIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task UserCanLogCompleteStrengthWorkout()
    {
        // Arrange - Create authenticated user
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act & Assert - Step 1: Create a new workout session
        var workoutSession = new
        {
            date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            type = "Strength",
            notes = "Monday strength session"
        };

        var sessionResponse = await _client.PostAsJsonAsync("/api/workout-sessions", workoutSession, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, sessionResponse.StatusCode);

        var sessionContent = await sessionResponse.Content.ReadAsStringAsync();
        var sessionResult = JsonSerializer.Deserialize<JsonElement>(sessionContent, _jsonOptions);

        Assert.True(sessionResult.TryGetProperty("id", out var sessionId));

        // Act & Assert - Step 2: Get available exercise types
        var exerciseTypesResponse = await _client.GetAsync("/api/exercise-types");

        Assert.Equal(HttpStatusCode.OK, exerciseTypesResponse.StatusCode);

        var exerciseTypesContent = await exerciseTypesResponse.Content.ReadAsStringAsync();
        var exerciseTypesResult = JsonSerializer.Deserialize<JsonElement>(exerciseTypesContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, exerciseTypesResult.ValueKind);
        Assert.True(exerciseTypesResult.GetArrayLength() > 0);

        // Use first exercise type for testing
        var firstExercise = exerciseTypesResult[0];
        Assert.True(firstExercise.TryGetProperty("id", out var exerciseTypeId));

        // Act & Assert - Step 3: Log first strength lift (back squat)
        var strengthLift1 = new
        {
            workoutSessionId = sessionId.GetString(),
            exerciseTypeId = exerciseTypeId.GetString(),
            sets = new[]
            {
                new { reps = 5, weight = 135, restSeconds = 180 },
                new { reps = 5, weight = 185, restSeconds = 180 },
                new { reps = 5, weight = 225, restSeconds = 180 },
                new { reps = 3, weight = 275, restSeconds = 240 },
                new { reps = 1, weight = 315, restSeconds = 0 }
            },
            notes = "Felt strong today, hit a new 1RM"
        };

        var lift1Response = await _client.PostAsJsonAsync("/api/strength-lifts", strengthLift1, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, lift1Response.StatusCode);

        var lift1Content = await lift1Response.Content.ReadAsStringAsync();
        var lift1Result = JsonSerializer.Deserialize<JsonElement>(lift1Content, _jsonOptions);

        Assert.True(lift1Result.TryGetProperty("id", out var lift1Id));

        // Act & Assert - Step 4: Log second strength lift (bench press)
        var strengthLift2 = new
        {
            workoutSessionId = sessionId.GetString(),
            exerciseTypeId = exerciseTypeId.GetString(), // In real scenario, would be different exercise
            sets = new[]
            {
                new { reps = 8, weight = 135, restSeconds = 120 },
                new { reps = 6, weight = 155, restSeconds = 120 },
                new { reps = 4, weight = 175, restSeconds = 150 },
                new { reps = 2, weight = 195, restSeconds = 0 }
            },
            notes = "Good pump, focused on form"
        };

        var lift2Response = await _client.PostAsJsonAsync("/api/strength-lifts", strengthLift2, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, lift2Response.StatusCode);

        // Act & Assert - Step 5: Verify workout session shows all lifts
        var sessionDetailsResponse = await _client.GetAsync($"/api/workout-sessions/{sessionId.GetString()}");

        Assert.Equal(HttpStatusCode.OK, sessionDetailsResponse.StatusCode);

        var sessionDetailsContent = await sessionDetailsResponse.Content.ReadAsStringAsync();
        var sessionDetailsResult = JsonSerializer.Deserialize<JsonElement>(sessionDetailsContent, _jsonOptions);

        Assert.True(sessionDetailsResult.TryGetProperty("strengthLifts", out var strengthLifts));
        Assert.Equal(JsonValueKind.Array, strengthLifts.ValueKind);
        Assert.Equal(2, strengthLifts.GetArrayLength());

        // Act & Assert - Step 6: Update workout session notes
        var sessionUpdate = new
        {
            notes = "Great strength session - new PR on squat!"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/workout-sessions/{sessionId.GetString()}", sessionUpdate, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Act & Assert - Step 7: Verify notes were updated
        var updatedSessionResponse = await _client.GetAsync($"/api/workout-sessions/{sessionId.GetString()}");

        Assert.Equal(HttpStatusCode.OK, updatedSessionResponse.StatusCode);

        var updatedSessionContent = await updatedSessionResponse.Content.ReadAsStringAsync();
        var updatedSessionResult = JsonSerializer.Deserialize<JsonElement>(updatedSessionContent, _jsonOptions);

        Assert.True(updatedSessionResult.TryGetProperty("notes", out var notes));
        Assert.Contains("PR", notes.GetString() ?? "");
    }

    [Fact]
    public async Task UserCanLogCompleteMetconWorkout()
    {
        // Arrange - Create authenticated user
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act & Assert - Step 1: Create a new workout session
        var workoutSession = new
        {
            date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            type = "Metcon",
            notes = "Tuesday conditioning"
        };

        var sessionResponse = await _client.PostAsJsonAsync("/api/workout-sessions", workoutSession, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, sessionResponse.StatusCode);

        var sessionContent = await sessionResponse.Content.ReadAsStringAsync();
        var sessionResult = JsonSerializer.Deserialize<JsonElement>(sessionContent, _jsonOptions);

        Assert.True(sessionResult.TryGetProperty("id", out var sessionId));

        // Act & Assert - Step 2: Get available metcon types
        var metconTypesResponse = await _client.GetAsync("/api/metcon-types");

        Assert.Equal(HttpStatusCode.OK, metconTypesResponse.StatusCode);

        var metconTypesContent = await metconTypesResponse.Content.ReadAsStringAsync();
        var metconTypesResult = JsonSerializer.Deserialize<JsonElement>(metconTypesContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, metconTypesResult.ValueKind);
        Assert.True(metconTypesResult.GetArrayLength() > 0);

        // Use AMRAP type for testing
        var amrapType = metconTypesResult.EnumerateArray()
            .FirstOrDefault(t => t.GetProperty("name").GetString() == "AMRAP");
        Assert.NotEqual(default, amrapType);
        Assert.True(amrapType.TryGetProperty("id", out var metconTypeId));

        // Act & Assert - Step 3: Log metcon workout
        var metconWorkout = new
        {
            workoutSessionId = sessionId.GetString(),
            metconTypeId = metconTypeId.GetString(),
            timeCapMinutes = 12,
            result = "8 rounds + 3 burpees",
            movements = new[]
            {
                new {
                    name = "Burpees",
                    reps = 10,
                    weight = (decimal?)null,
                    distance = (decimal?)null,
                    notes = "Chest to floor"
                },
                new {
                    name = "Air Squats",
                    reps = 15,
                    weight = (decimal?)null,
                    distance = (decimal?)null,
                    notes = "Below parallel"
                },
                new {
                    name = "Push-ups",
                    reps = 20,
                    weight = (decimal?)null,
                    distance = (decimal?)null,
                    notes = "Full range of motion"
                }
            },
            notes = "Maintained steady pace throughout"
        };

        var metconResponse = await _client.PostAsJsonAsync("/api/metcon-workouts", metconWorkout, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, metconResponse.StatusCode);

        var metconContent = await metconResponse.Content.ReadAsStringAsync();
        var metconResult = JsonSerializer.Deserialize<JsonElement>(metconContent, _jsonOptions);

        Assert.True(metconResult.TryGetProperty("id", out var metconId));

        // Act & Assert - Step 4: Verify workout session shows metcon
        var sessionDetailsResponse = await _client.GetAsync($"/api/workout-sessions/{sessionId.GetString()}");

        Assert.Equal(HttpStatusCode.OK, sessionDetailsResponse.StatusCode);

        var sessionDetailsContent = await sessionDetailsResponse.Content.ReadAsStringAsync();
        var sessionDetailsResult = JsonSerializer.Deserialize<JsonElement>(sessionDetailsContent, _jsonOptions);

        Assert.True(sessionDetailsResult.TryGetProperty("metconWorkouts", out var metconWorkouts));
        Assert.Equal(JsonValueKind.Array, metconWorkouts.ValueKind);
        Assert.Equal(1, metconWorkouts.GetArrayLength());

        var loggedMetcon = metconWorkouts[0];
        Assert.True(loggedMetcon.TryGetProperty("movements", out var movements));
        Assert.Equal(JsonValueKind.Array, movements.ValueKind);
        Assert.Equal(3, movements.GetArrayLength());
    }

    [Fact]
    public async Task UserCanViewWorkoutHistory()
    {
        // Arrange - Create authenticated user
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create multiple workout sessions
        await CreateTestWorkoutSession("Strength", "Day 1");
        await CreateTestWorkoutSession("Metcon", "Day 2");
        await CreateTestWorkoutSession("Strength", "Day 3");

        // Act & Assert - Get all workout sessions
        var allSessionsResponse = await _client.GetAsync("/api/workout-sessions");

        Assert.Equal(HttpStatusCode.OK, allSessionsResponse.StatusCode);

        var allSessionsContent = await allSessionsResponse.Content.ReadAsStringAsync();
        var allSessionsResult = JsonSerializer.Deserialize<JsonElement>(allSessionsContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, allSessionsResult.ValueKind);
        Assert.True(allSessionsResult.GetArrayLength() >= 3);

        // Act & Assert - Filter by workout type
        var strengthSessionsResponse = await _client.GetAsync("/api/workout-sessions?type=Strength");

        Assert.Equal(HttpStatusCode.OK, strengthSessionsResponse.StatusCode);

        var strengthSessionsContent = await strengthSessionsResponse.Content.ReadAsStringAsync();
        var strengthSessionsResult = JsonSerializer.Deserialize<JsonElement>(strengthSessionsContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, strengthSessionsResult.ValueKind);
        Assert.True(strengthSessionsResult.GetArrayLength() >= 2);

        // Verify all returned sessions are strength type
        foreach (var session in strengthSessionsResult.EnumerateArray())
        {
            Assert.True(session.TryGetProperty("type", out var type));
            Assert.Equal("Strength", type.GetString());
        }
    }

    private async Task<(string token, string userId)> CreateAuthenticatedUser()
    {
        var testEmail = $"workout.test.{Guid.NewGuid()}@example.com";
        var registration = new
        {
            email = testEmail,
            password = "SecurePassword123!",
            firstName = "Workout",
            lastName = "Tester"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registration, _jsonOptions);

        var login = new
        {
            email = testEmail,
            password = "SecurePassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", login, _jsonOptions);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent, _jsonOptions);

        var token = loginResult.GetProperty("token").GetString()!;

        // Get user ID from profile
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var profileResponse = await _client.GetAsync("/api/auth/profile");
        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileResult = JsonSerializer.Deserialize<JsonElement>(profileContent, _jsonOptions);

        var userId = profileResult.GetProperty("id").GetString()!;

        return (token, userId);
    }

    private async Task<string> CreateTestWorkoutSession(string type, string notes)
    {
        var workoutSession = new
        {
            date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            type = type,
            notes = notes
        };

        var response = await _client.PostAsJsonAsync("/api/workout-sessions", workoutSession, _jsonOptions);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        return result.GetProperty("id").GetString()!;
    }
}
