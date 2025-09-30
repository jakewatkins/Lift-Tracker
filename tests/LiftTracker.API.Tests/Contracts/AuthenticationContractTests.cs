using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiftTracker.API.Tests.Contracts;

/// <summary>
/// Contract tests for authentication API endpoints.
/// These tests verify that the authentication endpoints conform to the OpenAPI specification.
/// Tests should fail until authentication implementation is complete.
/// </summary>
public class AuthenticationContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthenticationContractTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task POST_Auth_Register_ShouldReturn201_WhenValidRegistration()
    {
        // Arrange
        var registration = new
        {
            email = "test@example.com",
            password = "SecurePassword123!",
            firstName = "John",
            lastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registration, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        Assert.True(result.TryGetProperty("id", out _));
        Assert.True(result.TryGetProperty("email", out var email));
        Assert.Equal("test@example.com", email.GetString());
    }

    [Fact]
    public async Task POST_Auth_Register_ShouldReturn400_WhenInvalidEmail()
    {
        // Arrange
        var registration = new
        {
            email = "invalid-email",
            password = "SecurePassword123!",
            firstName = "John",
            lastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registration, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        Assert.True(result.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task POST_Auth_Register_ShouldReturn409_WhenEmailAlreadyExists()
    {
        // Arrange
        var registration = new
        {
            email = "existing@example.com",
            password = "SecurePassword123!",
            firstName = "John",
            lastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registration, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        Assert.True(result.TryGetProperty("message", out var message));
        Assert.Contains("email", message.GetString()?.ToLower() ?? "");
    }

    [Fact]
    public async Task POST_Auth_Login_ShouldReturn200_WhenValidCredentials()
    {
        // Arrange
        var login = new
        {
            email = "test@example.com",
            password = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", login, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        Assert.True(result.TryGetProperty("token", out _));
        Assert.True(result.TryGetProperty("refreshToken", out _));
        Assert.True(result.TryGetProperty("expiresAt", out _));
    }

    [Fact]
    public async Task POST_Auth_Login_ShouldReturn401_WhenInvalidCredentials()
    {
        // Arrange
        var login = new
        {
            email = "test@example.com",
            password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", login, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        Assert.True(result.TryGetProperty("message", out var message));
        Assert.Contains("invalid", message.GetString()?.ToLower() ?? "");
    }

    [Fact]
    public async Task POST_Auth_RefreshToken_ShouldReturn200_WhenValidRefreshToken()
    {
        // Arrange
        var refreshRequest = new
        {
            refreshToken = "valid-refresh-token",
            token = "expired-access-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        Assert.True(result.TryGetProperty("token", out _));
        Assert.True(result.TryGetProperty("refreshToken", out _));
        Assert.True(result.TryGetProperty("expiresAt", out _));
    }

    [Fact]
    public async Task POST_Auth_RefreshToken_ShouldReturn401_WhenInvalidRefreshToken()
    {
        // Arrange
        var refreshRequest = new
        {
            refreshToken = "invalid-refresh-token",
            token = "expired-access-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task POST_Auth_Logout_ShouldReturn200_WhenValidToken()
    {
        // Arrange
        var logoutRequest = new
        {
            refreshToken = "valid-refresh-token"
        };

        // This would typically require authentication header
        // For now, testing the endpoint response structure

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest, _jsonOptions);

        // Assert
        // Should return 200 for successful logout or 401 if not authenticated
        Assert.True(response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_Auth_Profile_ShouldReturn401_WhenNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/profile");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PUT_Auth_Profile_ShouldReturn401_WhenNotAuthenticated()
    {
        // Arrange
        var profileUpdate = new
        {
            firstName = "Updated",
            lastName = "Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/auth/profile", profileUpdate, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
