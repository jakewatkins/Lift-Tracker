using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LiftTracker.API.Tests.Contracts;

/// <summary>
/// Contract tests for Strength Lift endpoints
/// These tests verify the API contract matches the specification
/// IMPORTANT: These tests are EXPECTED to FAIL until implementation is complete
/// </summary>
public class StrengthLiftContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public StrengthLiftContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetStrengthLifts_ShouldReturnOkWithCorrectSchema()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedFields = new[] { "id", "workoutSessionId", "exerciseTypeId", "sets", "reps", "weightPounds", "notes", "createdDate" };

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync($"/api/workout-sessions/{sessionId}/strength-lifts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var lifts = JsonSerializer.Deserialize<JsonElement[]>(content);

        if (lifts.Length > 0)
        {
            var firstLift = lifts[0];
            foreach (var field in expectedFields)
            {
                Assert.True(firstLift.TryGetProperty(field, out _),
                    $"Response should contain field '{field}'");
            }
        }
    }

    [Fact]
    public async Task CreateStrengthLift_ShouldReturnCreatedWithCorrectSchema()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var exerciseTypeId = Guid.NewGuid();
        var newLift = new
        {
            exerciseTypeId = exerciseTypeId,
            sets = 3,
            reps = 8,
            weightPounds = 185.5m,
            notes = "Felt strong today"
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", newLift);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var createdLift = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(createdLift.TryGetProperty("id", out _));
        Assert.True(createdLift.TryGetProperty("workoutSessionId", out _));
        Assert.True(createdLift.TryGetProperty("exerciseTypeId", out _));
        Assert.True(createdLift.TryGetProperty("sets", out _));
        Assert.True(createdLift.TryGetProperty("reps", out _));
        Assert.True(createdLift.TryGetProperty("weightPounds", out _));
        Assert.True(createdLift.TryGetProperty("notes", out _));
        Assert.True(createdLift.TryGetProperty("createdDate", out _));
    }

    [Fact]
    public async Task GetStrengthLiftById_ShouldReturnOkWithCorrectSchema()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var liftId = Guid.NewGuid();

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync($"/api/workout-sessions/{sessionId}/strength-lifts/{liftId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var lift = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(lift.TryGetProperty("id", out var idProperty));
        Assert.Equal(liftId.ToString(), idProperty.GetString());
    }

    [Fact]
    public async Task UpdateStrengthLift_ShouldReturnOkWithUpdatedData()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var liftId = Guid.NewGuid();
        var updateData = new
        {
            sets = 4,
            reps = 6,
            weightPounds = 200.0m,
            notes = "Increased weight successfully"
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PutAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts/{liftId}", updateData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var updatedLift = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(updatedLift.TryGetProperty("weightPounds", out var weightProperty));
        Assert.Equal(200.0m, weightProperty.GetDecimal());
    }

    [Fact]
    public async Task DeleteStrengthLift_ShouldReturnNoContent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var liftId = Guid.NewGuid();

        // Act - This will FAIL until API is implemented
        var response = await _client.DeleteAsync($"/api/workout-sessions/{sessionId}/strength-lifts/{liftId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateStrengthLift_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var invalidLift = new
        {
            exerciseTypeId = Guid.Empty, // Invalid
            sets = -1, // Invalid
            reps = 0, // Invalid
            weightPounds = -50.0m, // Invalid
            notes = new string('x', 501) // Exceeds max length
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", invalidLift);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStrengthLifts_WithInvalidSessionId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidSessionId = Guid.NewGuid();

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync($"/api/workout-sessions/{invalidSessionId}/strength-lifts");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStrengthLifts_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var anonymousClient = _factory.CreateClient();

        // Act - This will FAIL until API is implemented
        var response = await anonymousClient.GetAsync($"/api/workout-sessions/{sessionId}/strength-lifts");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetExerciseTypes_ShouldReturnOkWithCorrectSchema()
    {
        // Arrange
        var expectedFields = new[] { "id", "name", "category", "description", "isBodyweight" };

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync("/api/exercise-types");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var exerciseTypes = JsonSerializer.Deserialize<JsonElement[]>(content);

        if (exerciseTypes.Length > 0)
        {
            var firstType = exerciseTypes[0];
            foreach (var field in expectedFields)
            {
                Assert.True(firstType.TryGetProperty(field, out _),
                    $"Response should contain field '{field}'");
            }
        }
    }
}
