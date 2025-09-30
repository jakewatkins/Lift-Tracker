using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiftTracker.API.Tests.Integration;

/// <summary>
/// Integration test for the data editing user story.
/// User Story: As a user, I want to edit my workout data so that I can correct mistakes or update information.
/// This test validates the complete end-to-end flow of editing workout sessions, lifts, and metcon data.
/// Test should fail until data editing implementation is complete.
/// </summary>
public class DataEditingIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DataEditingIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task UserCanEditWorkoutSessionData()
    {
        // Arrange - Create authenticated user
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create initial workout session
        var initialSession = new
        {
            date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            type = "Strength",
            notes = "Initial notes"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/workout-sessions", initialSession, _jsonOptions);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<JsonElement>(createContent, _jsonOptions);
        var sessionId = createResult.GetProperty("id").GetString()!;

        // Act & Assert - Step 1: Update workout session basic information
        var sessionUpdate = new
        {
            date = DateTime.UtcNow.AddHours(-2).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            type = "Mixed",
            notes = "Updated notes - realized this was a mixed session"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/workout-sessions/{sessionId}", sessionUpdate, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Verify update was applied
        var getResponse = await _client.GetAsync($"/api/workout-sessions/{sessionId}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<JsonElement>(getContent, _jsonOptions);

        Assert.True(getResult.TryGetProperty("type", out var type));
        Assert.Equal("Mixed", type.GetString());
        Assert.True(getResult.TryGetProperty("notes", out var notes));
        Assert.Contains("mixed session", notes.GetString() ?? "");

        // Act & Assert - Step 2: Partially update workout session (only notes)
        var partialUpdate = new
        {
            notes = "Just updating the notes this time"
        };

        var partialUpdateResponse = await _client.PatchAsJsonAsync($"/api/workout-sessions/{sessionId}", partialUpdate, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, partialUpdateResponse.StatusCode);

        // Verify partial update preserved other fields
        var verifyResponse = await _client.GetAsync($"/api/workout-sessions/{sessionId}");
        var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
        var verifyResult = JsonSerializer.Deserialize<JsonElement>(verifyContent, _jsonOptions);

        Assert.True(verifyResult.TryGetProperty("type", out var preservedType));
        Assert.Equal("Mixed", preservedType.GetString()); // Should be preserved
        Assert.True(verifyResult.TryGetProperty("notes", out var updatedNotes));
        Assert.Equal("Just updating the notes this time", updatedNotes.GetString());
    }

    [Fact]
    public async Task UserCanEditStrengthLiftData()
    {
        // Arrange - Create authenticated user and workout session
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var sessionId = await CreateTestWorkoutSession("Strength", "Test session");

        // Create initial strength lift
        var exerciseTypeId = await GetFirstExerciseTypeId();
        var initialLift = new
        {
            workoutSessionId = sessionId,
            exerciseTypeId = exerciseTypeId,
            sets = new[]
            {
                new { reps = 5, weight = 135, restSeconds = 180 },
                new { reps = 5, weight = 155, restSeconds = 180 },
                new { reps = 5, weight = 175, restSeconds = 0 }
            },
            notes = "Initial lift notes"
        };

        var createLiftResponse = await _client.PostAsJsonAsync("/api/strength-lifts", initialLift, _jsonOptions);
        var createLiftContent = await createLiftResponse.Content.ReadAsStringAsync();
        var createLiftResult = JsonSerializer.Deserialize<JsonElement>(createLiftContent, _jsonOptions);
        var liftId = createLiftResult.GetProperty("id").GetString()!;

        // Act & Assert - Step 1: Update strength lift data (correct weights)
        var liftUpdate = new
        {
            exerciseTypeId = exerciseTypeId,
            sets = new[]
            {
                new { reps = 5, weight = 135, restSeconds = 180 },
                new { reps = 5, weight = 165, restSeconds = 180 }, // Corrected weight
                new { reps = 3, weight = 185, restSeconds = 180 }, // Corrected reps and weight
                new { reps = 1, weight = 205, restSeconds = 0 }    // Added new set
            },
            notes = "Corrected weights and added final set"
        };

        var updateLiftResponse = await _client.PutAsJsonAsync($"/api/strength-lifts/{liftId}", liftUpdate, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateLiftResponse.StatusCode);

        // Verify lift was updated
        var getLiftResponse = await _client.GetAsync($"/api/strength-lifts/{liftId}");
        var getLiftContent = await getLiftResponse.Content.ReadAsStringAsync();
        var getLiftResult = JsonSerializer.Deserialize<JsonElement>(getLiftContent, _jsonOptions);

        Assert.True(getLiftResult.TryGetProperty("sets", out var sets));
        Assert.Equal(JsonValueKind.Array, sets.ValueKind);
        Assert.Equal(4, sets.GetArrayLength()); // Should now have 4 sets

        var secondSet = sets[1];
        Assert.True(secondSet.TryGetProperty("weight", out var weight));
        Assert.Equal(165, weight.GetDecimal()); // Verify corrected weight

        // Act & Assert - Step 2: Update just the notes
        var notesOnlyUpdate = new
        {
            notes = "Updated notes only - felt stronger than expected"
        };

        var notesUpdateResponse = await _client.PatchAsJsonAsync($"/api/strength-lifts/{liftId}", notesOnlyUpdate, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, notesUpdateResponse.StatusCode);

        // Verify notes updated and sets preserved
        var verifyLiftResponse = await _client.GetAsync($"/api/strength-lifts/{liftId}");
        var verifyLiftContent = await verifyLiftResponse.Content.ReadAsStringAsync();
        var verifyLiftResult = JsonSerializer.Deserialize<JsonElement>(verifyLiftContent, _jsonOptions);

        Assert.True(verifyLiftResult.TryGetProperty("notes", out var updatedLiftNotes));
        Assert.Contains("stronger than expected", updatedLiftNotes.GetString() ?? "");
        Assert.True(verifyLiftResult.TryGetProperty("sets", out var preservedSets));
        Assert.Equal(4, preservedSets.GetArrayLength()); // Sets should be preserved
    }

    [Fact]
    public async Task UserCanEditMetconWorkoutData()
    {
        // Arrange - Create authenticated user and workout session
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var sessionId = await CreateTestWorkoutSession("Metcon", "Test session");

        // Create initial metcon workout
        var metconTypeId = await GetFirstMetconTypeId();
        var initialMetcon = new
        {
            workoutSessionId = sessionId,
            metconTypeId = metconTypeId,
            timeCapMinutes = 15,
            result = "7 rounds",
            movements = new[]
            {
                new {
                    name = "Burpees",
                    reps = 10,
                    weight = (decimal?)null,
                    distance = (decimal?)null,
                    notes = "Regular burpees"
                },
                new {
                    name = "Squats",
                    reps = 20,
                    weight = (decimal?)null,
                    distance = (decimal?)null,
                    notes = "Bodyweight"
                }
            },
            notes = "Initial metcon notes"
        };

        var createMetconResponse = await _client.PostAsJsonAsync("/api/metcon-workouts", initialMetcon, _jsonOptions);
        var createMetconContent = await createMetconResponse.Content.ReadAsStringAsync();
        var createMetconResult = JsonSerializer.Deserialize<JsonElement>(createMetconContent, _jsonOptions);
        var metconId = createMetconResult.GetProperty("id").GetString()!;

        // Act & Assert - Step 1: Update metcon result and movements
        var metconUpdate = new
        {
            metconTypeId = metconTypeId,
            timeCapMinutes = 15,
            result = "7 rounds + 5 burpees", // Corrected result
            movements = new[]
            {
                new {
                    name = "Burpees",
                    reps = 10,
                    weight = (decimal?)null,
                    distance = (decimal?)null,
                    notes = "Chest to floor - corrected form"
                },
                new {
                    name = "Air Squats",
                    reps = 20,
                    weight = (decimal?)null,
                    distance = (decimal?)null,
                    notes = "Below parallel"
                },
                new {
                    name = "Push-ups",
                    reps = 15,
                    weight = (decimal?)null,
                    distance = (decimal?)null,
                    notes = "Added third movement"
                }
            },
            notes = "Corrected result and added third movement"
        };

        var updateMetconResponse = await _client.PutAsJsonAsync($"/api/metcon-workouts/{metconId}", metconUpdate, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateMetconResponse.StatusCode);

        // Verify metcon was updated
        var getMetconResponse = await _client.GetAsync($"/api/metcon-workouts/{metconId}");
        var getMetconContent = await getMetconResponse.Content.ReadAsStringAsync();
        var getMetconResult = JsonSerializer.Deserialize<JsonElement>(getMetconContent, _jsonOptions);

        Assert.True(getMetconResult.TryGetProperty("result", out var result));
        Assert.Contains("+ 5 burpees", result.GetString() ?? "");

        Assert.True(getMetconResult.TryGetProperty("movements", out var movements));
        Assert.Equal(JsonValueKind.Array, movements.ValueKind);
        Assert.Equal(3, movements.GetArrayLength()); // Should now have 3 movements

        // Act & Assert - Step 2: Update time cap only
        var timeCapUpdate = new
        {
            timeCapMinutes = 12 // Realized it was actually 12 minutes
        };

        var timeCapUpdateResponse = await _client.PatchAsJsonAsync($"/api/metcon-workouts/{metconId}", timeCapUpdate, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, timeCapUpdateResponse.StatusCode);

        // Verify time cap updated and other data preserved
        var verifyMetconResponse = await _client.GetAsync($"/api/metcon-workouts/{metconId}");
        var verifyMetconContent = await verifyMetconResponse.Content.ReadAsStringAsync();
        var verifyMetconResult = JsonSerializer.Deserialize<JsonElement>(verifyMetconContent, _jsonOptions);

        Assert.True(verifyMetconResult.TryGetProperty("timeCapMinutes", out var timeCap));
        Assert.Equal(12, timeCap.GetInt32());
        Assert.True(verifyMetconResult.TryGetProperty("movements", out var preservedMovements));
        Assert.Equal(3, preservedMovements.GetArrayLength()); // Movements should be preserved
    }

    [Fact]
    public async Task UserCanDeleteWorkoutData()
    {
        // Arrange - Create authenticated user and test data
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var sessionId = await CreateTestWorkoutSession("Strength", "Test session for deletion");
        var exerciseTypeId = await GetFirstExerciseTypeId();

        var strengthLift = new
        {
            workoutSessionId = sessionId,
            exerciseTypeId = exerciseTypeId,
            sets = new[]
            {
                new { reps = 5, weight = 135, restSeconds = 180 }
            },
            notes = "Test lift for deletion"
        };

        var createLiftResponse = await _client.PostAsJsonAsync("/api/strength-lifts", strengthLift, _jsonOptions);
        var createLiftContent = await createLiftResponse.Content.ReadAsStringAsync();
        var createLiftResult = JsonSerializer.Deserialize<JsonElement>(createLiftContent, _jsonOptions);
        var liftId = createLiftResult.GetProperty("id").GetString()!;

        // Act & Assert - Step 1: Delete strength lift
        var deleteLiftResponse = await _client.DeleteAsync($"/api/strength-lifts/{liftId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteLiftResponse.StatusCode);

        // Verify lift was deleted
        var getLiftResponse = await _client.GetAsync($"/api/strength-lifts/{liftId}");
        Assert.Equal(HttpStatusCode.NotFound, getLiftResponse.StatusCode);

        // Act & Assert - Step 2: Delete workout session
        var deleteSessionResponse = await _client.DeleteAsync($"/api/workout-sessions/{sessionId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteSessionResponse.StatusCode);

        // Verify session was deleted
        var getSessionResponse = await _client.GetAsync($"/api/workout-sessions/{sessionId}");
        Assert.Equal(HttpStatusCode.NotFound, getSessionResponse.StatusCode);
    }

    [Fact]
    public async Task UserCannotEditAnotherUsersData()
    {
        // Arrange - Create two users
        var (token1, userId1) = await CreateAuthenticatedUser("user1");
        var (token2, userId2) = await CreateAuthenticatedUser("user2");

        // User 1 creates a workout session
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await CreateTestWorkoutSession("Strength", "User 1's session");

        // User 2 tries to edit User 1's session
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var unauthorizedUpdate = new
        {
            notes = "User 2 trying to edit User 1's data"
        };

        // Act & Assert - User 2 should not be able to edit User 1's data
        var updateResponse = await _client.PutAsJsonAsync($"/api/workout-sessions/{sessionId}", unauthorizedUpdate, _jsonOptions);

        Assert.True(updateResponse.StatusCode == HttpStatusCode.Forbidden ||
                   updateResponse.StatusCode == HttpStatusCode.NotFound);

        // Verify User 2 cannot delete User 1's data
        var deleteResponse = await _client.DeleteAsync($"/api/workout-sessions/{sessionId}");

        Assert.True(deleteResponse.StatusCode == HttpStatusCode.Forbidden ||
                   deleteResponse.StatusCode == HttpStatusCode.NotFound);
    }

    private async Task<(string token, string userId)> CreateAuthenticatedUser(string? suffix = null)
    {
        var testEmail = $"edit.test.{Guid.NewGuid()}{suffix}@example.com";
        var registration = new
        {
            email = testEmail,
            password = "SecurePassword123!",
            firstName = "Edit",
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

        var currentAuth = _client.DefaultRequestHeaders.Authorization;
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var profileResponse = await _client.GetAsync("/api/auth/profile");
        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileResult = JsonSerializer.Deserialize<JsonElement>(profileContent, _jsonOptions);

        var userId = profileResult.GetProperty("id").GetString()!;

        _client.DefaultRequestHeaders.Authorization = currentAuth;

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

    private async Task<string> GetFirstExerciseTypeId()
    {
        var response = await _client.GetAsync("/api/exercise-types");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        return result[0].GetProperty("id").GetString()!;
    }

    private async Task<string> GetFirstMetconTypeId()
    {
        var response = await _client.GetAsync("/api/metcon-types");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        return result[0].GetProperty("id").GetString()!;
    }
}
