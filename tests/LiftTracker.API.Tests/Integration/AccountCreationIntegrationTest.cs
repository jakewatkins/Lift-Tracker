using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiftTracker.API.Tests.Integration;

/// <summary>
/// Integration test for the account creation user story.
/// User Story: As a user, I want to create an account so that I can track my workouts.
/// This test validates the complete end-to-end flow of user registration and initial setup.
/// Test should fail until authentication and user management implementation is complete.
/// </summary>
public class AccountCreationIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AccountCreationIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task UserCanCreateAccountAndAccessProfile()
    {
        // Arrange
        var testEmail = $"integration.test.{Guid.NewGuid()}@example.com";
        var registration = new
        {
            email = testEmail,
            password = "SecurePassword123!",
            firstName = "Integration",
            lastName = "Test"
        };

        // Act & Assert - Step 1: Register new user
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registration, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        Assert.NotNull(registerResponse.Headers.Location);

        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<JsonElement>(registerContent, _jsonOptions);

        Assert.True(registerResult.TryGetProperty("id", out var userId));
        Assert.True(registerResult.TryGetProperty("email", out var email));
        Assert.Equal(testEmail, email.GetString());

        // Act & Assert - Step 2: Login with new credentials
        var login = new
        {
            email = testEmail,
            password = "SecurePassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", login, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent, _jsonOptions);

        Assert.True(loginResult.TryGetProperty("token", out var token));
        Assert.True(loginResult.TryGetProperty("refreshToken", out var refreshToken));
        Assert.True(loginResult.TryGetProperty("expiresAt", out var expiresAt));

        // Act & Assert - Step 3: Access profile with authentication token
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.GetString());

        var profileResponse = await _client.GetAsync("/api/auth/profile");

        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileResult = JsonSerializer.Deserialize<JsonElement>(profileContent, _jsonOptions);

        Assert.True(profileResult.TryGetProperty("id", out var profileUserId));
        Assert.True(profileResult.TryGetProperty("email", out var profileEmail));
        Assert.True(profileResult.TryGetProperty("firstName", out var firstName));
        Assert.True(profileResult.TryGetProperty("lastName", out var lastName));

        Assert.Equal(userId.GetString(), profileUserId.GetString());
        Assert.Equal(testEmail, profileEmail.GetString());
        Assert.Equal("Integration", firstName.GetString());
        Assert.Equal("Test", lastName.GetString());

        // Act & Assert - Step 4: Update profile information
        var profileUpdate = new
        {
            firstName = "Updated",
            lastName = "Name"
        };

        var updateResponse = await _client.PutAsJsonAsync("/api/auth/profile", profileUpdate, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Act & Assert - Step 5: Verify profile was updated
        var updatedProfileResponse = await _client.GetAsync("/api/auth/profile");

        Assert.Equal(HttpStatusCode.OK, updatedProfileResponse.StatusCode);

        var updatedProfileContent = await updatedProfileResponse.Content.ReadAsStringAsync();
        var updatedProfileResult = JsonSerializer.Deserialize<JsonElement>(updatedProfileContent, _jsonOptions);

        Assert.True(updatedProfileResult.TryGetProperty("firstName", out var updatedFirstName));
        Assert.True(updatedProfileResult.TryGetProperty("lastName", out var updatedLastName));

        Assert.Equal("Updated", updatedFirstName.GetString());
        Assert.Equal("Name", updatedLastName.GetString());

        // Act & Assert - Step 6: Verify user can access workout-related endpoints
        var workoutSessionsResponse = await _client.GetAsync("/api/workout-sessions");

        Assert.Equal(HttpStatusCode.OK, workoutSessionsResponse.StatusCode);

        var workoutSessionsContent = await workoutSessionsResponse.Content.ReadAsStringAsync();
        var workoutSessionsResult = JsonSerializer.Deserialize<JsonElement>(workoutSessionsContent, _jsonOptions);

        // Should return empty array for new user
        Assert.Equal(JsonValueKind.Array, workoutSessionsResult.ValueKind);
        Assert.Equal(0, workoutSessionsResult.GetArrayLength());

        // Act & Assert - Step 7: Logout successfully
        var logoutRequest = new
        {
            refreshToken = refreshToken.GetString()
        };

        var logoutResponse = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        // Act & Assert - Step 8: Verify token is invalidated after logout
        var postLogoutProfileResponse = await _client.GetAsync("/api/auth/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, postLogoutProfileResponse.StatusCode);
    }

    [Fact]
    public async Task UserCannotCreateAccountWithDuplicateEmail()
    {
        // Arrange
        var testEmail = $"duplicate.test.{Guid.NewGuid()}@example.com";
        var registration = new
        {
            email = testEmail,
            password = "SecurePassword123!",
            firstName = "First",
            lastName = "User"
        };

        // Act & Assert - Step 1: Create first account
        var firstRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", registration, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, firstRegisterResponse.StatusCode);

        // Act & Assert - Step 2: Attempt to create second account with same email
        var duplicateRegistration = new
        {
            email = testEmail,
            password = "DifferentPassword456!",
            firstName = "Second",
            lastName = "User"
        };

        var secondRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", duplicateRegistration, _jsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, secondRegisterResponse.StatusCode);

        var errorContent = await secondRegisterResponse.Content.ReadAsStringAsync();
        var errorResult = JsonSerializer.Deserialize<JsonElement>(errorContent, _jsonOptions);

        Assert.True(errorResult.TryGetProperty("message", out var message));
        Assert.Contains("email", message.GetString()?.ToLower() ?? "");
    }

    [Fact]
    public async Task UserCannotAccessProtectedEndpointsWithoutAuthentication()
    {
        // Act & Assert - Profile endpoint
        var profileResponse = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, profileResponse.StatusCode);

        // Act & Assert - Workout sessions endpoint
        var workoutSessionsResponse = await _client.GetAsync("/api/workout-sessions");
        Assert.Equal(HttpStatusCode.Unauthorized, workoutSessionsResponse.StatusCode);

        // Act & Assert - Progress endpoint
        var progressResponse = await _client.GetAsync("/api/progress/strength");
        Assert.Equal(HttpStatusCode.Unauthorized, progressResponse.StatusCode);
    }
}
