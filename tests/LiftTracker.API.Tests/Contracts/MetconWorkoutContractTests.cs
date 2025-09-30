using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LiftTracker.API.Tests.Contracts;

/// <summary>
/// Contract tests for Metcon Workout endpoints
/// These tests verify the API contract matches the specification
/// IMPORTANT: These tests are EXPECTED to FAIL until implementation is complete
/// </summary>
public class MetconWorkoutContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MetconWorkoutContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetMetconWorkouts_ShouldReturnOkWithCorrectSchema()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedFields = new[] { "id", "workoutSessionId", "metconTypeId", "timeSeconds", "rounds", "notes", "createdDate" };

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync($"/api/workout-sessions/{sessionId}/metcon-workouts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var workouts = JsonSerializer.Deserialize<JsonElement[]>(content);

        if (workouts?.Length > 0)
        {
            var firstWorkout = workouts[0];
            foreach (var field in expectedFields)
            {
                Assert.True(firstWorkout.TryGetProperty(field, out _),
                    $"Response should contain field '{field}'");
            }
        }
    }

    [Fact]
    public async Task CreateMetconWorkout_ShouldReturnCreatedWithCorrectSchema()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var metconTypeId = Guid.NewGuid();
        var newWorkout = new
        {
            metconTypeId = metconTypeId,
            timeSeconds = 720, // 12 minutes
            rounds = 5,
            notes = "Great AMRAP workout"
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/metcon-workouts", newWorkout);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var createdWorkout = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(createdWorkout.TryGetProperty("id", out _));
        Assert.True(createdWorkout.TryGetProperty("workoutSessionId", out _));
        Assert.True(createdWorkout.TryGetProperty("metconTypeId", out _));
        Assert.True(createdWorkout.TryGetProperty("timeSeconds", out _));
        Assert.True(createdWorkout.TryGetProperty("rounds", out _));
        Assert.True(createdWorkout.TryGetProperty("notes", out _));
        Assert.True(createdWorkout.TryGetProperty("createdDate", out _));
    }

    [Fact]
    public async Task GetMetconWorkoutById_ShouldReturnOkWithCorrectSchema()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync($"/api/workout-sessions/{sessionId}/metcon-workouts/{workoutId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var workout = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(workout.TryGetProperty("id", out var idProperty));
        Assert.Equal(workoutId.ToString(), idProperty.GetString());
    }

    [Fact]
    public async Task UpdateMetconWorkout_ShouldReturnOkWithUpdatedData()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var updateData = new
        {
            timeSeconds = 900, // 15 minutes
            rounds = 6,
            notes = "Improved time and added extra round"
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PutAsJsonAsync($"/api/workout-sessions/{sessionId}/metcon-workouts/{workoutId}", updateData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var updatedWorkout = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(updatedWorkout.TryGetProperty("timeSeconds", out var timeProperty));
        Assert.Equal(900, timeProperty.GetInt32());
    }

    [Fact]
    public async Task DeleteMetconWorkout_ShouldReturnNoContent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();

        // Act - This will FAIL until API is implemented
        var response = await _client.DeleteAsync($"/api/workout-sessions/{sessionId}/metcon-workouts/{workoutId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateMetconWorkout_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var invalidWorkout = new
        {
            metconTypeId = Guid.Empty, // Invalid
            timeSeconds = -1, // Invalid
            rounds = -5, // Invalid
            notes = new string('x', 501) // Exceeds max length
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/metcon-workouts", invalidWorkout);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMetconTypes_ShouldReturnOkWithCorrectSchema()
    {
        // Arrange
        var expectedFields = new[] { "id", "name", "description", "scoringType" };

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync("/api/metcon-types");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var metconTypes = JsonSerializer.Deserialize<JsonElement[]>(content);

        if (metconTypes?.Length > 0)
        {
            var firstType = metconTypes[0];
            foreach (var field in expectedFields)
            {
                Assert.True(firstType.TryGetProperty(field, out _),
                    $"Response should contain field '{field}'");
            }
        }
    }

    [Fact]
    public async Task GetMetconMovements_ShouldReturnOkWithCorrectSchema()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var expectedFields = new[] { "id", "metconWorkoutId", "movementTypeId", "reps", "weightPounds", "orderIndex" };

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync($"/api/metcon-workouts/{workoutId}/movements");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var movements = JsonSerializer.Deserialize<JsonElement[]>(content);

        if (movements?.Length > 0)
        {
            var firstMovement = movements[0];
            foreach (var field in expectedFields)
            {
                Assert.True(firstMovement.TryGetProperty(field, out _),
                    $"Response should contain field '{field}'");
            }
        }
    }

    [Fact]
    public async Task CreateMetconMovement_ShouldReturnCreatedWithCorrectSchema()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var movementTypeId = Guid.NewGuid();
        var newMovement = new
        {
            movementTypeId = movementTypeId,
            reps = 10,
            weightPounds = 135.0m,
            orderIndex = 1
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PostAsJsonAsync($"/api/metcon-workouts/{workoutId}/movements", newMovement);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var createdMovement = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(createdMovement.TryGetProperty("id", out _));
        Assert.True(createdMovement.TryGetProperty("metconWorkoutId", out _));
        Assert.True(createdMovement.TryGetProperty("movementTypeId", out _));
        Assert.True(createdMovement.TryGetProperty("reps", out _));
        Assert.True(createdMovement.TryGetProperty("weightPounds", out _));
        Assert.True(createdMovement.TryGetProperty("orderIndex", out _));
    }

    [Fact]
    public async Task GetMetconWorkouts_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var anonymousClient = _factory.CreateClient();

        // Act - This will FAIL until API is implemented
        var response = await anonymousClient.GetAsync($"/api/workout-sessions/{sessionId}/metcon-workouts");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
