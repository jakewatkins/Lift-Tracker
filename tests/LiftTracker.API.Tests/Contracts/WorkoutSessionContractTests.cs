using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LiftTracker.API.Tests.Contracts;

/// <summary>
/// Contract tests for Workout Session endpoints
/// These tests verify the API contract matches the specification
/// IMPORTANT: These tests are EXPECTED to FAIL until implementation is complete
/// </summary>
public class WorkoutSessionContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WorkoutSessionContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetWorkoutSessions_ShouldReturnOkWithCorrectSchema()
    {
        // Arrange
        var expectedFields = new[] { "id", "userId", "date", "notes", "createdDate", "modifiedDate" };

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync("/api/workout-sessions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var sessions = JsonSerializer.Deserialize<JsonElement[]>(content);

        if (sessions.Length > 0)
        {
            var firstSession = sessions[0];
            foreach (var field in expectedFields)
            {
                Assert.True(firstSession.TryGetProperty(field, out _),
                    $"Response should contain field '{field}'");
            }
        }
    }

    [Fact]
    public async Task CreateWorkoutSession_ShouldReturnCreatedWithCorrectSchema()
    {
        // Arrange
        var newSession = new
        {
            date = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
            notes = "Test workout session"
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PostAsJsonAsync("/api/workout-sessions", newSession);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var createdSession = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(createdSession.TryGetProperty("id", out _));
        Assert.True(createdSession.TryGetProperty("userId", out _));
        Assert.True(createdSession.TryGetProperty("date", out _));
        Assert.True(createdSession.TryGetProperty("notes", out _));
        Assert.True(createdSession.TryGetProperty("createdDate", out _));
        Assert.True(createdSession.TryGetProperty("modifiedDate", out _));
    }

    [Fact]
    public async Task GetWorkoutSessionById_ShouldReturnOkWithCorrectSchema()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync($"/api/workout-sessions/{sessionId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(session.TryGetProperty("id", out var idProperty));
        Assert.Equal(sessionId.ToString(), idProperty.GetString());
    }

    [Fact]
    public async Task UpdateWorkoutSession_ShouldReturnOkWithUpdatedData()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var updateData = new
        {
            notes = "Updated workout notes"
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PutAsJsonAsync($"/api/workout-sessions/{sessionId}", updateData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var updatedSession = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(updatedSession.TryGetProperty("notes", out var notesProperty));
        Assert.Equal("Updated workout notes", notesProperty.GetString());
    }

    [Fact]
    public async Task DeleteWorkoutSession_ShouldReturnNoContent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - This will FAIL until API is implemented
        var response = await _client.DeleteAsync($"/api/workout-sessions/{sessionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkoutSessionById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act - This will FAIL until API is implemented
        var response = await _client.GetAsync($"/api/workout-sessions/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkoutSession_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidSession = new
        {
            date = "invalid-date",
            notes = new string('x', 1001) // Exceeds max length
        };

        // Act - This will FAIL until API is implemented
        var response = await _client.PostAsJsonAsync("/api/workout-sessions", invalidSession);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkoutSessions_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var anonymousClient = _factory.CreateClient();
        // Remove any authentication headers

        // Act - This will FAIL until API is implemented
        var response = await anonymousClient.GetAsync("/api/workout-sessions");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
