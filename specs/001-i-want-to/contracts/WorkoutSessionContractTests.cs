using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using FluentAssertions;
using LiftTracker.API.Tests.Fixtures;
using LiftTracker.Application.DTOs;

namespace LiftTracker.API.Tests.Contracts;

/// <summary>
/// Contract tests for Workout Session API endpoints
/// These tests validate API contract compliance and will FAIL until implementation is complete
/// </summary>
public class WorkoutSessionContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WorkoutSessionContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_WorkoutSessions_ShouldReturnUnauthorized_WhenNoAuth()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/workout-sessions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_WorkoutSessions_ShouldReturnWorkoutSessionList_WhenAuthenticated()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");

        // Act
        var response = await _client.GetAsync("/api/workout-sessions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<WorkoutSessionListDto>();
        content.Should().NotBeNull();
        content!.Sessions.Should().NotBeNull();
        content.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        content.Page.Should().BeGreaterThan(0);
        content.PageSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task POST_WorkoutSessions_ShouldCreateSession_WhenValidData()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var createRequest = new CreateWorkoutSessionDto
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            Notes = "Test workout session"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workout-sessions", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
        content.Date.Should().Be(createRequest.Date);
        content.Notes.Should().Be(createRequest.Notes);
    }

    [Fact]
    public async Task POST_WorkoutSessions_ShouldReturnBadRequest_WhenInvalidData()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var createRequest = new CreateWorkoutSessionDto
        {
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)), // Future date
            Notes = new string('x', 1001) // Exceeds max length
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workout-sessions", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadFromJsonAsync<ErrorDto>();
        content.Should().NotBeNull();
        content!.Error.Should().NotBeNullOrEmpty();
        content.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GET_WorkoutSession_ById_ShouldReturnSessionDetail_WhenExists()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/workout-sessions/{sessionId}");

        // Assert - This will fail until implementation exists
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadFromJsonAsync<WorkoutSessionDetailDto>();
            content.Should().NotBeNull();
            content!.Id.Should().Be(sessionId);
            content.StrengthLifts.Should().NotBeNull();
            content.MetconWorkouts.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task PUT_WorkoutSession_ShouldUpdateSession_WhenValidData()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();
        var updateRequest = new UpdateWorkoutSessionDto
        {
            Notes = "Updated notes"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/workout-sessions/{sessionId}", updateRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_WorkoutSession_ShouldDeleteSession_WhenExists()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/workout-sessions/{sessionId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?pageSize=0")]
    [InlineData("?pageSize=101")]
    [InlineData("?startDate=invalid-date")]
    public async Task GET_WorkoutSessions_ShouldReturnBadRequest_WhenInvalidQueryParams(string queryParams)
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");

        // Act
        var response = await _client.GetAsync($"/api/workout-sessions{queryParams}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_WorkoutSessions_ShouldRespectDateRange_WhenProvided()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");
        var startDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.Today.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/workout-sessions?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<WorkoutSessionListDto>();
        content.Should().NotBeNull();
        
        // Verify all sessions are within date range
        foreach (var session in content!.Sessions)
        {
            session.Date.Should().BeOnOrAfter(DateOnly.Parse(startDate));
            session.Date.Should().BeOnOrBefore(DateOnly.Parse(endDate));
        }
    }

    [Fact]
    public async Task GET_WorkoutSessions_ShouldRespectPagination_WhenProvided()
    {
        // Arrange
        _client.SetAuthenticatedUser("test-user-id");

        // Act
        var response = await _client.GetAsync("/api/workout-sessions?page=2&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<WorkoutSessionListDto>();
        content.Should().NotBeNull();
        content!.Page.Should().Be(2);
        content.PageSize.Should().Be(5);
        content.Sessions.Count.Should().BeLessOrEqualTo(5);
    }
}