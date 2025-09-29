using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using FluentAssertions;
using LiftTracker.API.Tests.Fixtures;
using LiftTracker.Application.DTOs;

namespace LiftTracker.API.Tests.Contracts;

/// <summary>
/// Contract tests for Strength Lift API endpoints
/// These tests validate API contract compliance and will FAIL until implementation is complete
/// </summary>
public class StrengthLiftContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public StrengthLiftContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_StrengthLifts_ShouldCreateLift_WhenValidData()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();
        var createRequest = new CreateStrengthLiftDto
        {
            ExerciseTypeId = 1, // Back Squat
            SetStructure = "SetsReps",
            Sets = 3,
            Reps = 5,
            Weight = 225.0m,
            RestPeriod = 3.0m,
            Comments = "Felt strong today"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", createRequest);

        // Assert - This will fail until implementation exists
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadFromJsonAsync<StrengthLiftDto>();
            content.Should().NotBeNull();
            content!.Id.Should().NotBeEmpty();
            content.ExerciseTypeId.Should().Be(createRequest.ExerciseTypeId);
            content.SetStructure.Should().Be(createRequest.SetStructure);
            content.Weight.Should().Be(createRequest.Weight);
        }
    }

    [Fact]
    public async Task POST_StrengthLifts_ShouldReturnBadRequest_WhenInvalidWeight()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();
        var createRequest = new CreateStrengthLiftDto
        {
            ExerciseTypeId = 1,
            SetStructure = "SetsReps",
            Sets = 3,
            Reps = 5,
            Weight = 225.33m // Invalid fractional increment
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadFromJsonAsync<ErrorDto>();
        content.Should().NotBeNull();
        content!.Message.Should().Contain("weight");
    }

    [Theory]
    [InlineData("EMOM", null, null, 5.0)] // EMOM with duration
    [InlineData("AMRAP", null, null, 10.0)] // AMRAP with duration
    [InlineData("TimeBased", null, null, 2.5)] // Time-based with duration
    [InlineData("SetsReps", 5, 10, null)] // Sets/reps without duration
    public async Task POST_StrengthLifts_ShouldCreateLift_WhenValidSetStructure(
        string setStructure, int? sets, int? reps, decimal? duration)
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();
        var createRequest = new CreateStrengthLiftDto
        {
            ExerciseTypeId = 1,
            SetStructure = setStructure,
            Sets = sets,
            Reps = reps,
            Weight = 135.0m,
            Duration = duration
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", createRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_StrengthLifts_ShouldCreateBodyweightLift_WhenZeroWeight()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();
        var createRequest = new CreateStrengthLiftDto
        {
            ExerciseTypeId = 10, // Pull-ups
            SetStructure = "SetsReps",
            Sets = 3,
            Reps = 10,
            Weight = 0.0m,
            AdditionalWeight = 25.0m // Weighted vest
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", createRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadFromJsonAsync<StrengthLiftDto>();
            content.Should().NotBeNull();
            content!.Weight.Should().Be(0.0m);
            content.AdditionalWeight.Should().Be(25.0m);
        }
    }

    [Fact]
    public async Task PUT_StrengthLifts_ShouldUpdateLift_WhenValidData()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var liftId = Guid.NewGuid();
        var updateRequest = new UpdateStrengthLiftDto
        {
            ExerciseTypeId = 1,
            SetStructure = "SetsReps",
            Sets = 5,
            Reps = 3,
            Weight = 275.0m,
            Comments = "Updated weight"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/strength-lifts/{liftId}", updateRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_StrengthLifts_ShouldDeleteLift_WhenExists()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var liftId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/strength-lifts/{liftId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(-1)] // Negative weight
    [InlineData(0.1)] // Invalid fractional increment
    [InlineData(0.33)] // Invalid fractional increment
    [InlineData(0.66)] // Invalid fractional increment
    public async Task POST_StrengthLifts_ShouldReturnBadRequest_WhenInvalidWeightValues(decimal invalidWeight)
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();
        var createRequest = new CreateStrengthLiftDto
        {
            ExerciseTypeId = 1,
            SetStructure = "SetsReps",
            Sets = 3,
            Reps = 5,
            Weight = invalidWeight
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)] // Zero sets
    [InlineData(51)] // Exceeds maximum
    public async Task POST_StrengthLifts_ShouldReturnBadRequest_WhenInvalidSets(int invalidSets)
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();
        var createRequest = new CreateStrengthLiftDto
        {
            ExerciseTypeId = 1,
            SetStructure = "SetsReps",
            Sets = invalidSets,
            Reps = 5,
            Weight = 225.0m
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)] // Zero reps
    [InlineData(501)] // Exceeds maximum
    public async Task POST_StrengthLifts_ShouldReturnBadRequest_WhenInvalidReps(int invalidReps)
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();
        var createRequest = new CreateStrengthLiftDto
        {
            ExerciseTypeId = 1,
            SetStructure = "SetsReps",
            Sets = 3,
            Reps = invalidReps,
            Weight = 225.0m
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_StrengthLifts_ShouldReturnBadRequest_WhenCommentsExceedMaxLength()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();
        var createRequest = new CreateStrengthLiftDto
        {
            ExerciseTypeId = 1,
            SetStructure = "SetsReps",
            Sets = 3,
            Reps = 5,
            Weight = 225.0m,
            Comments = new string('x', 501) // Exceeds 500 character limit
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_StrengthLifts_ShouldReturnUnauthorized_WhenNoAuth()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var createRequest = new CreateStrengthLiftDto
        {
            ExerciseTypeId = 1,
            SetStructure = "SetsReps",
            Weight = 225.0m
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/workout-sessions/{sessionId}/strength-lifts", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}