using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiftTracker.API.Tests.Integration;

/// <summary>
/// Integration test for the data isolation user story.
/// User Story: As a user, I want my workout data to be private so that only I can see and modify it.
/// This test validates the complete end-to-end flow of data isolation and security.
/// Test should fail until proper authentication and authorization implementation is complete.
/// </summary>
public class DataIsolationIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DataIsolationIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task UsersCanOnlyAccessTheirOwnWorkoutSessions()
    {
        // Arrange - Create two separate users
        var (token1, userId1) = await CreateAuthenticatedUser("user1");
        var (token2, userId2) = await CreateAuthenticatedUser("user2");

        // User 1 creates workout sessions
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1Session1Id = await CreateTestWorkoutSession("Strength", "User 1 strength session");
        var user1Session2Id = await CreateTestWorkoutSession("Metcon", "User 1 metcon session");

        // User 2 creates workout sessions
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var user2Session1Id = await CreateTestWorkoutSession("Strength", "User 2 strength session");
        var user2Session2Id = await CreateTestWorkoutSession("Metcon", "User 2 metcon session");

        // Act & Assert - User 1 can only see their own sessions
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1SessionsResponse = await _client.GetAsync("/api/workout-sessions");

        Assert.Equal(HttpStatusCode.OK, user1SessionsResponse.StatusCode);

        var user1SessionsContent = await user1SessionsResponse.Content.ReadAsStringAsync();
        var user1SessionsResult = JsonSerializer.Deserialize<JsonElement>(user1SessionsContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, user1SessionsResult.ValueKind);
        Assert.Equal(2, user1SessionsResult.GetArrayLength());

        // Verify all sessions belong to user 1
        foreach (var session in user1SessionsResult.EnumerateArray())
        {
            Assert.True(session.TryGetProperty("id", out var sessionId));
            var id = sessionId.GetString();
            Assert.True(id == user1Session1Id || id == user1Session2Id);
        }

        // Act & Assert - User 2 can only see their own sessions
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var user2SessionsResponse = await _client.GetAsync("/api/workout-sessions");

        Assert.Equal(HttpStatusCode.OK, user2SessionsResponse.StatusCode);

        var user2SessionsContent = await user2SessionsResponse.Content.ReadAsStringAsync();
        var user2SessionsResult = JsonSerializer.Deserialize<JsonElement>(user2SessionsContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, user2SessionsResult.ValueKind);
        Assert.Equal(2, user2SessionsResult.GetArrayLength());

        // Verify all sessions belong to user 2
        foreach (var session in user2SessionsResult.EnumerateArray())
        {
            Assert.True(session.TryGetProperty("id", out var sessionId));
            var id = sessionId.GetString();
            Assert.True(id == user2Session1Id || id == user2Session2Id);
        }

        // Act & Assert - User 1 cannot access user 2's specific session
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var unauthorizedAccessResponse = await _client.GetAsync($"/api/workout-sessions/{user2Session1Id}");

        Assert.True(unauthorizedAccessResponse.StatusCode == HttpStatusCode.NotFound ||
                   unauthorizedAccessResponse.StatusCode == HttpStatusCode.Forbidden);

        // Act & Assert - User 2 cannot access user 1's specific session
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var unauthorizedAccessResponse2 = await _client.GetAsync($"/api/workout-sessions/{user1Session1Id}");

        Assert.True(unauthorizedAccessResponse2.StatusCode == HttpStatusCode.NotFound ||
                   unauthorizedAccessResponse2.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UsersCanOnlyAccessTheirOwnStrengthLifts()
    {
        // Arrange - Create two users with workout sessions and strength lifts
        var (token1, userId1) = await CreateAuthenticatedUser("strength1");
        var (token2, userId2) = await CreateAuthenticatedUser("strength2");

        // User 1 creates strength data
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1SessionId = await CreateTestWorkoutSession("Strength", "User 1 session");
        var user1LiftId = await CreateTestStrengthLift(user1SessionId, "User 1 lift");

        // User 2 creates strength data
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var user2SessionId = await CreateTestWorkoutSession("Strength", "User 2 session");
        var user2LiftId = await CreateTestStrengthLift(user2SessionId, "User 2 lift");

        // Act & Assert - User 1 can only see their own strength lifts
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1LiftsResponse = await _client.GetAsync("/api/strength-lifts");

        Assert.Equal(HttpStatusCode.OK, user1LiftsResponse.StatusCode);

        var user1LiftsContent = await user1LiftsResponse.Content.ReadAsStringAsync();
        var user1LiftsResult = JsonSerializer.Deserialize<JsonElement>(user1LiftsContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, user1LiftsResult.ValueKind);
        Assert.Equal(1, user1LiftsResult.GetArrayLength());

        var user1Lift = user1LiftsResult[0];
        Assert.True(user1Lift.TryGetProperty("id", out var liftId));
        Assert.Equal(user1LiftId, liftId.GetString());

        // Act & Assert - User 1 cannot access user 2's strength lift
        var unauthorizedLiftResponse = await _client.GetAsync($"/api/strength-lifts/{user2LiftId}");

        Assert.True(unauthorizedLiftResponse.StatusCode == HttpStatusCode.NotFound ||
                   unauthorizedLiftResponse.StatusCode == HttpStatusCode.Forbidden);

        // Act & Assert - User 2 can access their own strength lift
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var user2LiftResponse = await _client.GetAsync($"/api/strength-lifts/{user2LiftId}");

        Assert.Equal(HttpStatusCode.OK, user2LiftResponse.StatusCode);

        var user2LiftContent = await user2LiftResponse.Content.ReadAsStringAsync();
        var user2LiftResult = JsonSerializer.Deserialize<JsonElement>(user2LiftContent, _jsonOptions);

        Assert.True(user2LiftResult.TryGetProperty("id", out var user2LiftIdResult));
        Assert.Equal(user2LiftId, user2LiftIdResult.GetString());
    }

    [Fact]
    public async Task UsersCanOnlyAccessTheirOwnMetconWorkouts()
    {
        // Arrange - Create two users with metcon data
        var (token1, userId1) = await CreateAuthenticatedUser("metcon1");
        var (token2, userId2) = await CreateAuthenticatedUser("metcon2");

        // User 1 creates metcon data
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1SessionId = await CreateTestWorkoutSession("Metcon", "User 1 metcon session");
        var user1MetconId = await CreateTestMetconWorkout(user1SessionId, "User 1 metcon");

        // User 2 creates metcon data
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var user2SessionId = await CreateTestWorkoutSession("Metcon", "User 2 metcon session");
        var user2MetconId = await CreateTestMetconWorkout(user2SessionId, "User 2 metcon");

        // Act & Assert - User 1 can only see their own metcon workouts
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1MetconsResponse = await _client.GetAsync("/api/metcon-workouts");

        Assert.Equal(HttpStatusCode.OK, user1MetconsResponse.StatusCode);

        var user1MetconsContent = await user1MetconsResponse.Content.ReadAsStringAsync();
        var user1MetconsResult = JsonSerializer.Deserialize<JsonElement>(user1MetconsContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, user1MetconsResult.ValueKind);
        Assert.Equal(1, user1MetconsResult.GetArrayLength());

        var user1Metcon = user1MetconsResult[0];
        Assert.True(user1Metcon.TryGetProperty("id", out var metconId));
        Assert.Equal(user1MetconId, metconId.GetString());

        // Act & Assert - User 1 cannot access user 2's metcon workout
        var unauthorizedMetconResponse = await _client.GetAsync($"/api/metcon-workouts/{user2MetconId}");

        Assert.True(unauthorizedMetconResponse.StatusCode == HttpStatusCode.NotFound ||
                   unauthorizedMetconResponse.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UsersCanOnlyAccessTheirOwnProgressData()
    {
        // Arrange - Create two users with historical data
        var (token1, userId1) = await CreateAuthenticatedUser("progress1");
        var (token2, userId2) = await CreateAuthenticatedUser("progress2");

        // User 1 creates workout history
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1SessionId = await CreateTestWorkoutSession("Strength", "User 1 progress session");
        await CreateTestStrengthLift(user1SessionId, "User 1 progress lift");

        // User 2 creates workout history
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var user2SessionId = await CreateTestWorkoutSession("Strength", "User 2 progress session");
        await CreateTestStrengthLift(user2SessionId, "User 2 progress lift");

        // Act & Assert - User 1 can only see their own strength progress
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1ProgressResponse = await _client.GetAsync("/api/progress/strength");

        Assert.Equal(HttpStatusCode.OK, user1ProgressResponse.StatusCode);

        var user1ProgressContent = await user1ProgressResponse.Content.ReadAsStringAsync();
        var user1ProgressResult = JsonSerializer.Deserialize<JsonElement>(user1ProgressContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, user1ProgressResult.ValueKind);
        // Progress should only include data from user 1's workouts

        // Act & Assert - User 2 can only see their own strength progress
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var user2ProgressResponse = await _client.GetAsync("/api/progress/strength");

        Assert.Equal(HttpStatusCode.OK, user2ProgressResponse.StatusCode);

        var user2ProgressContent = await user2ProgressResponse.Content.ReadAsStringAsync();
        var user2ProgressResult = JsonSerializer.Deserialize<JsonElement>(user2ProgressContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, user2ProgressResult.ValueKind);
        // Progress should only include data from user 2's workouts

        // Act & Assert - User 1 personal records don't include user 2's data
        var user1PRResponse = await _client.GetAsync("/api/progress/personal-records");

        Assert.Equal(HttpStatusCode.OK, user1PRResponse.StatusCode);
        // Personal records should be isolated per user

        // Act & Assert - User 1 statistics don't include user 2's data
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1StatsResponse = await _client.GetAsync("/api/progress/stats");

        Assert.Equal(HttpStatusCode.OK, user1StatsResponse.StatusCode);

        var user1StatsContent = await user1StatsResponse.Content.ReadAsStringAsync();
        var user1StatsResult = JsonSerializer.Deserialize<JsonElement>(user1StatsContent, _jsonOptions);

        Assert.True(user1StatsResult.TryGetProperty("totalWorkouts", out var user1TotalWorkouts));
        Assert.Equal(1, user1TotalWorkouts.GetInt32()); // Should only count user 1's workouts
    }

    [Fact]
    public async Task UsersCannotModifyAnotherUsersData()
    {
        // Arrange - Create two users with data
        var (token1, userId1) = await CreateAuthenticatedUser("modify1");
        var (token2, userId2) = await CreateAuthenticatedUser("modify2");

        // User 1 creates data
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var user1SessionId = await CreateTestWorkoutSession("Strength", "User 1 session");
        var user1LiftId = await CreateTestStrengthLift(user1SessionId, "User 1 lift");

        // Act & Assert - User 2 cannot modify user 1's workout session
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var sessionUpdate = new
        {
            notes = "User 2 trying to modify user 1's session"
        };

        var unauthorizedUpdateResponse = await _client.PutAsJsonAsync($"/api/workout-sessions/{user1SessionId}", sessionUpdate, _jsonOptions);

        Assert.True(unauthorizedUpdateResponse.StatusCode == HttpStatusCode.NotFound ||
                   unauthorizedUpdateResponse.StatusCode == HttpStatusCode.Forbidden);

        // Act & Assert - User 2 cannot modify user 1's strength lift
        var liftUpdate = new
        {
            notes = "User 2 trying to modify user 1's lift"
        };

        var unauthorizedLiftUpdateResponse = await _client.PutAsJsonAsync($"/api/strength-lifts/{user1LiftId}", liftUpdate, _jsonOptions);

        Assert.True(unauthorizedLiftUpdateResponse.StatusCode == HttpStatusCode.NotFound ||
                   unauthorizedLiftUpdateResponse.StatusCode == HttpStatusCode.Forbidden);

        // Act & Assert - User 2 cannot delete user 1's workout session
        var unauthorizedDeleteResponse = await _client.DeleteAsync($"/api/workout-sessions/{user1SessionId}");

        Assert.True(unauthorizedDeleteResponse.StatusCode == HttpStatusCode.NotFound ||
                   unauthorizedDeleteResponse.StatusCode == HttpStatusCode.Forbidden);

        // Act & Assert - User 2 cannot delete user 1's strength lift
        var unauthorizedLiftDeleteResponse = await _client.DeleteAsync($"/api/strength-lifts/{user1LiftId}");

        Assert.True(unauthorizedLiftDeleteResponse.StatusCode == HttpStatusCode.NotFound ||
                   unauthorizedLiftDeleteResponse.StatusCode == HttpStatusCode.Forbidden);

        // Act & Assert - Verify user 1's data is still intact
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);

        var verifySessionResponse = await _client.GetAsync($"/api/workout-sessions/{user1SessionId}");
        Assert.Equal(HttpStatusCode.OK, verifySessionResponse.StatusCode);

        var verifyLiftResponse = await _client.GetAsync($"/api/strength-lifts/{user1LiftId}");
        Assert.Equal(HttpStatusCode.OK, verifyLiftResponse.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedUsersCannotAccessAnyData()
    {
        // Arrange - Create user with data
        var (token, userId) = await CreateAuthenticatedUser("auth-test");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var sessionId = await CreateTestWorkoutSession("Strength", "Protected session");
        var liftId = await CreateTestStrengthLift(sessionId, "Protected lift");

        // Remove authentication
        _client.DefaultRequestHeaders.Authorization = null;

        // Act & Assert - Unauthenticated access should be denied
        var sessionListResponse = await _client.GetAsync("/api/workout-sessions");
        Assert.Equal(HttpStatusCode.Unauthorized, sessionListResponse.StatusCode);

        var specificSessionResponse = await _client.GetAsync($"/api/workout-sessions/{sessionId}");
        Assert.Equal(HttpStatusCode.Unauthorized, specificSessionResponse.StatusCode);

        var liftListResponse = await _client.GetAsync("/api/strength-lifts");
        Assert.Equal(HttpStatusCode.Unauthorized, liftListResponse.StatusCode);

        var specificLiftResponse = await _client.GetAsync($"/api/strength-lifts/{liftId}");
        Assert.Equal(HttpStatusCode.Unauthorized, specificLiftResponse.StatusCode);

        var progressResponse = await _client.GetAsync("/api/progress/strength");
        Assert.Equal(HttpStatusCode.Unauthorized, progressResponse.StatusCode);

        var statsResponse = await _client.GetAsync("/api/progress/stats");
        Assert.Equal(HttpStatusCode.Unauthorized, statsResponse.StatusCode);

        var profileResponse = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, profileResponse.StatusCode);
    }

    [Fact]
    public async Task InvalidTokensAreRejected()
    {
        // Act & Assert - Invalid token format
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        var response1 = await _client.GetAsync("/api/workout-sessions");
        Assert.Equal(HttpStatusCode.Unauthorized, response1.StatusCode);

        // Act & Assert - Expired or malformed token
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid");

        var response2 = await _client.GetAsync("/api/workout-sessions");
        Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);

        // Act & Assert - Empty token
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "");

        var response3 = await _client.GetAsync("/api/workout-sessions");
        Assert.Equal(HttpStatusCode.Unauthorized, response3.StatusCode);
    }

    private async Task<(string token, string userId)> CreateAuthenticatedUser(string suffix)
    {
        var testEmail = $"isolation.test.{suffix}.{Guid.NewGuid()}@example.com";
        var registration = new
        {
            email = testEmail,
            password = "SecurePassword123!",
            firstName = "Isolation",
            lastName = $"Tester{suffix}"
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

    private async Task<string> CreateTestStrengthLift(string sessionId, string notes)
    {
        var exerciseTypeId = await GetFirstExerciseTypeId();

        var strengthLift = new
        {
            workoutSessionId = sessionId,
            exerciseTypeId = exerciseTypeId,
            sets = new[]
            {
                new { reps = 5, weight = 135, restSeconds = 180 }
            },
            notes = notes
        };

        var response = await _client.PostAsJsonAsync("/api/strength-lifts", strengthLift, _jsonOptions);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        return result.GetProperty("id").GetString()!;
    }

    private async Task<string> CreateTestMetconWorkout(string sessionId, string notes)
    {
        var metconTypeId = await GetFirstMetconTypeId();

        var metconWorkout = new
        {
            workoutSessionId = sessionId,
            metconTypeId = metconTypeId,
            timeCapMinutes = 15,
            result = "5 rounds",
            movements = new[]
            {
                new {
                    name = "Burpees",
                    reps = 10,
                    weight = (decimal?)null,
                    distance = (decimal?)null,
                    notes = "Standard"
                }
            },
            notes = notes
        };

        var response = await _client.PostAsJsonAsync("/api/metcon-workouts", metconWorkout, _jsonOptions);
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
