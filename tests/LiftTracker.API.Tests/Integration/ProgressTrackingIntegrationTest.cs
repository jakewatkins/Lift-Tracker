using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiftTracker.API.Tests.Integration;

/// <summary>
/// Integration test for the progress tracking user story.
/// User Story: As a user, I want to view my progress over time so that I can see my improvements.
/// This test validates the complete end-to-end flow of progress tracking and analytics.
/// Test should fail until progress tracking implementation is complete.
/// </summary>
public class ProgressTrackingIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProgressTrackingIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task UserCanViewStrengthProgressOverTime()
    {
        // Arrange - Create authenticated user
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create historical strength data
        await CreateHistoricalStrengthData();

        // Act & Assert - Step 1: Get overall strength progress
        var progressResponse = await _client.GetAsync("/api/progress/strength");

        Assert.Equal(HttpStatusCode.OK, progressResponse.StatusCode);

        var progressContent = await progressResponse.Content.ReadAsStringAsync();
        var progressResult = JsonSerializer.Deserialize<JsonElement>(progressContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, progressResult.ValueKind);
        Assert.True(progressResult.GetArrayLength() > 0);

        // Verify progress data structure
        var firstProgress = progressResult[0];
        Assert.True(firstProgress.TryGetProperty("exerciseTypeId", out _));
        Assert.True(firstProgress.TryGetProperty("exerciseName", out _));
        Assert.True(firstProgress.TryGetProperty("progressData", out var progressData));

        Assert.Equal(JsonValueKind.Array, progressData.ValueKind);
        Assert.True(progressData.GetArrayLength() > 0);

        var firstDataPoint = progressData[0];
        Assert.True(firstDataPoint.TryGetProperty("date", out _));
        Assert.True(firstDataPoint.TryGetProperty("maxWeight", out _));
        Assert.True(firstDataPoint.TryGetProperty("totalVolume", out _));

        // Act & Assert - Step 2: Get progress for specific exercise
        var exerciseTypeId = firstProgress.GetProperty("exerciseTypeId").GetString();
        var specificProgressResponse = await _client.GetAsync($"/api/progress/strength/{exerciseTypeId}");

        Assert.Equal(HttpStatusCode.OK, specificProgressResponse.StatusCode);

        var specificProgressContent = await specificProgressResponse.Content.ReadAsStringAsync();
        var specificProgressResult = JsonSerializer.Deserialize<JsonElement>(specificProgressContent, _jsonOptions);

        Assert.True(specificProgressResult.TryGetProperty("exerciseTypeId", out _));
        Assert.True(specificProgressResult.TryGetProperty("progressData", out var specificProgressData));
        Assert.Equal(JsonValueKind.Array, specificProgressData.ValueKind);

        // Act & Assert - Step 3: Get progress with date range filter
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");

        var filteredProgressResponse = await _client.GetAsync($"/api/progress/strength?startDate={startDate}&endDate={endDate}");

        Assert.Equal(HttpStatusCode.OK, filteredProgressResponse.StatusCode);

        var filteredProgressContent = await filteredProgressResponse.Content.ReadAsStringAsync();
        var filteredProgressResult = JsonSerializer.Deserialize<JsonElement>(filteredProgressContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, filteredProgressResult.ValueKind);

        // Verify date filtering worked
        foreach (var exercise in filteredProgressResult.EnumerateArray())
        {
            Assert.True(exercise.TryGetProperty("progressData", out var exerciseProgressData));
            foreach (var dataPoint in exerciseProgressData.EnumerateArray())
            {
                Assert.True(dataPoint.TryGetProperty("date", out var date));
                var dataPointDate = DateTime.Parse(date.GetString()!);
                Assert.True(dataPointDate >= DateTime.Parse(startDate));
                Assert.True(dataPointDate <= DateTime.Parse(endDate));
            }
        }
    }

    [Fact]
    public async Task UserCanViewMetconProgressOverTime()
    {
        // Arrange - Create authenticated user
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create historical metcon data
        await CreateHistoricalMetconData();

        // Act & Assert - Step 1: Get overall metcon progress
        var progressResponse = await _client.GetAsync("/api/progress/metcon");

        Assert.Equal(HttpStatusCode.OK, progressResponse.StatusCode);

        var progressContent = await progressResponse.Content.ReadAsStringAsync();
        var progressResult = JsonSerializer.Deserialize<JsonElement>(progressContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, progressResult.ValueKind);
        Assert.True(progressResult.GetArrayLength() > 0);

        // Verify progress data structure
        var firstProgress = progressResult[0];
        Assert.True(firstProgress.TryGetProperty("metconType", out _));
        Assert.True(firstProgress.TryGetProperty("progressData", out var progressData));

        Assert.Equal(JsonValueKind.Array, progressData.ValueKind);
        Assert.True(progressData.GetArrayLength() > 0);

        var firstDataPoint = progressData[0];
        Assert.True(firstDataPoint.TryGetProperty("date", out _));
        Assert.True(firstDataPoint.TryGetProperty("result", out _));
        Assert.True(firstDataPoint.TryGetProperty("timeCapMinutes", out _));

        // Act & Assert - Step 2: Filter by metcon type
        var metconType = "AMRAP";
        var typeFilterResponse = await _client.GetAsync($"/api/progress/metcon?type={metconType}");

        Assert.Equal(HttpStatusCode.OK, typeFilterResponse.StatusCode);

        var typeFilterContent = await typeFilterResponse.Content.ReadAsStringAsync();
        var typeFilterResult = JsonSerializer.Deserialize<JsonElement>(typeFilterContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, typeFilterResult.ValueKind);

        // Verify all results are for specified type
        foreach (var metcon in typeFilterResult.EnumerateArray())
        {
            Assert.True(metcon.TryGetProperty("metconType", out var type));
            Assert.Equal(metconType, type.GetString());
        }
    }

    [Fact]
    public async Task UserCanViewVolumeProgress()
    {
        // Arrange - Create authenticated user and data
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await CreateHistoricalStrengthData();

        // Act & Assert - Step 1: Get overall volume progress
        var volumeResponse = await _client.GetAsync("/api/progress/volume");

        Assert.Equal(HttpStatusCode.OK, volumeResponse.StatusCode);

        var volumeContent = await volumeResponse.Content.ReadAsStringAsync();
        var volumeResult = JsonSerializer.Deserialize<JsonElement>(volumeContent, _jsonOptions);

        Assert.True(volumeResult.TryGetProperty("totalVolume", out _));
        Assert.True(volumeResult.TryGetProperty("periodData", out var periodData));

        Assert.Equal(JsonValueKind.Array, periodData.ValueKind);
        Assert.True(periodData.GetArrayLength() > 0);

        var firstPeriod = periodData[0];
        Assert.True(firstPeriod.TryGetProperty("period", out _));
        Assert.True(firstPeriod.TryGetProperty("volume", out _));
        Assert.True(firstPeriod.TryGetProperty("sessions", out _));

        // Act & Assert - Step 2: Get weekly volume breakdown
        var weeklyVolumeResponse = await _client.GetAsync("/api/progress/volume?period=weekly");

        Assert.Equal(HttpStatusCode.OK, weeklyVolumeResponse.StatusCode);

        var weeklyVolumeContent = await weeklyVolumeResponse.Content.ReadAsStringAsync();
        var weeklyVolumeResult = JsonSerializer.Deserialize<JsonElement>(weeklyVolumeContent, _jsonOptions);

        Assert.True(weeklyVolumeResult.TryGetProperty("periodData", out var weeklyPeriodData));
        Assert.Equal(JsonValueKind.Array, weeklyPeriodData.ValueKind);

        // Act & Assert - Step 3: Get monthly volume breakdown
        var monthlyVolumeResponse = await _client.GetAsync("/api/progress/volume?period=monthly");

        Assert.Equal(HttpStatusCode.OK, monthlyVolumeResponse.StatusCode);

        var monthlyVolumeContent = await monthlyVolumeResponse.Content.ReadAsStringAsync();
        var monthlyVolumeResult = JsonSerializer.Deserialize<JsonElement>(monthlyVolumeContent, _jsonOptions);

        Assert.True(monthlyVolumeResult.TryGetProperty("periodData", out var monthlyPeriodData));
        Assert.Equal(JsonValueKind.Array, monthlyPeriodData.ValueKind);
    }

    [Fact]
    public async Task UserCanViewPersonalRecords()
    {
        // Arrange - Create authenticated user and data
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await CreateHistoricalStrengthData();

        // Act & Assert - Step 1: Get all personal records
        var prResponse = await _client.GetAsync("/api/progress/personal-records");

        Assert.Equal(HttpStatusCode.OK, prResponse.StatusCode);

        var prContent = await prResponse.Content.ReadAsStringAsync();
        var prResult = JsonSerializer.Deserialize<JsonElement>(prContent, _jsonOptions);

        Assert.Equal(JsonValueKind.Array, prResult.ValueKind);
        Assert.True(prResult.GetArrayLength() > 0);

        var firstPR = prResult[0];
        Assert.True(firstPR.TryGetProperty("exerciseTypeId", out _));
        Assert.True(firstPR.TryGetProperty("exerciseName", out _));
        Assert.True(firstPR.TryGetProperty("maxWeight", out _));
        Assert.True(firstPR.TryGetProperty("maxReps", out _));
        Assert.True(firstPR.TryGetProperty("achievedDate", out _));

        // Act & Assert - Step 2: Get personal records for specific exercise
        var exerciseTypeId = firstPR.GetProperty("exerciseTypeId").GetString();
        var specificPRResponse = await _client.GetAsync($"/api/progress/personal-records/{exerciseTypeId}");

        Assert.Equal(HttpStatusCode.OK, specificPRResponse.StatusCode);

        var specificPRContent = await specificPRResponse.Content.ReadAsStringAsync();
        var specificPRResult = JsonSerializer.Deserialize<JsonElement>(specificPRContent, _jsonOptions);

        Assert.True(specificPRResult.TryGetProperty("exerciseTypeId", out var prExerciseId));
        Assert.Equal(exerciseTypeId, prExerciseId.GetString());
        Assert.True(specificPRResult.TryGetProperty("records", out var records));

        Assert.Equal(JsonValueKind.Array, records.ValueKind);
        Assert.True(records.GetArrayLength() > 0);

        var firstRecord = records[0];
        Assert.True(firstRecord.TryGetProperty("reps", out _));
        Assert.True(firstRecord.TryGetProperty("weight", out _));
        Assert.True(firstRecord.TryGetProperty("achievedDate", out _));
    }

    [Fact]
    public async Task UserCanViewProgressStatistics()
    {
        // Arrange - Create authenticated user and data
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await CreateHistoricalStrengthData();
        await CreateHistoricalMetconData();

        // Act & Assert - Step 1: Get overall statistics
        var statsResponse = await _client.GetAsync("/api/progress/stats");

        Assert.Equal(HttpStatusCode.OK, statsResponse.StatusCode);

        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        var statsResult = JsonSerializer.Deserialize<JsonElement>(statsContent, _jsonOptions);

        Assert.True(statsResult.TryGetProperty("totalWorkouts", out _));
        Assert.True(statsResult.TryGetProperty("totalVolume", out _));
        Assert.True(statsResult.TryGetProperty("strengthWorkouts", out _));
        Assert.True(statsResult.TryGetProperty("metconWorkouts", out _));
        Assert.True(statsResult.TryGetProperty("averageSessionDuration", out _));
        Assert.True(statsResult.TryGetProperty("personalRecords", out _));

        // Act & Assert - Step 2: Get statistics for specific period
        var periodStatsResponse = await _client.GetAsync("/api/progress/stats?period=30");

        Assert.Equal(HttpStatusCode.OK, periodStatsResponse.StatusCode);

        var periodStatsContent = await periodStatsResponse.Content.ReadAsStringAsync();
        var periodStatsResult = JsonSerializer.Deserialize<JsonElement>(periodStatsContent, _jsonOptions);

        // Should have same structure but potentially different values
        Assert.True(periodStatsResult.TryGetProperty("totalWorkouts", out _));
        Assert.True(periodStatsResult.TryGetProperty("totalVolume", out _));
    }

    [Fact]
    public async Task UserCanViewProgressTrends()
    {
        // Arrange - Create authenticated user and data
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await CreateHistoricalStrengthData();

        // Act & Assert - Step 1: Get overall trends
        var trendsResponse = await _client.GetAsync("/api/progress/trends");

        Assert.Equal(HttpStatusCode.OK, trendsResponse.StatusCode);

        var trendsContent = await trendsResponse.Content.ReadAsStringAsync();
        var trendsResult = JsonSerializer.Deserialize<JsonElement>(trendsContent, _jsonOptions);

        Assert.True(trendsResult.TryGetProperty("volumeTrend", out var volumeTrend));
        Assert.True(trendsResult.TryGetProperty("frequencyTrend", out var frequencyTrend));
        Assert.True(trendsResult.TryGetProperty("strengthTrend", out var strengthTrend));

        // Verify trend structure
        Assert.True(volumeTrend.TryGetProperty("direction", out _)); // "increasing", "decreasing", "stable"
        Assert.True(volumeTrend.TryGetProperty("percentage", out _));

        // Act & Assert - Step 2: Get specific metric trends
        var volumeMetricResponse = await _client.GetAsync("/api/progress/trends?metric=volume");

        Assert.Equal(HttpStatusCode.OK, volumeMetricResponse.StatusCode);

        var volumeMetricContent = await volumeMetricResponse.Content.ReadAsStringAsync();
        var volumeMetricResult = JsonSerializer.Deserialize<JsonElement>(volumeMetricContent, _jsonOptions);

        Assert.True(volumeMetricResult.TryGetProperty("metric", out var metric));
        Assert.Equal("volume", metric.GetString());
        Assert.True(volumeMetricResult.TryGetProperty("trendData", out var trendData));
        Assert.Equal(JsonValueKind.Array, trendData.ValueKind);
    }

    [Fact]
    public async Task UserCanCompareProgressBetweenPeriods()
    {
        // Arrange - Create authenticated user and data
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await CreateHistoricalStrengthData();

        // Act & Assert - Compare progress between two periods
        var period1 = DateTime.UtcNow.AddMonths(-2).ToString("yyyy-MM");
        var period2 = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM");

        var comparisonResponse = await _client.GetAsync($"/api/progress/comparison?period1={period1}&period2={period2}");

        Assert.Equal(HttpStatusCode.OK, comparisonResponse.StatusCode);

        var comparisonContent = await comparisonResponse.Content.ReadAsStringAsync();
        var comparisonResult = JsonSerializer.Deserialize<JsonElement>(comparisonContent, _jsonOptions);

        Assert.True(comparisonResult.TryGetProperty("period1", out var period1Data));
        Assert.True(comparisonResult.TryGetProperty("period2", out var period2Data));
        Assert.True(comparisonResult.TryGetProperty("comparison", out var comparison));

        // Verify comparison structure
        Assert.True(comparison.TryGetProperty("volumeChange", out _));
        Assert.True(comparison.TryGetProperty("workoutCountChange", out _));
        Assert.True(comparison.TryGetProperty("strengthImprovements", out _));

        // Verify period data structure
        Assert.True(period1Data.TryGetProperty("totalVolume", out _));
        Assert.True(period1Data.TryGetProperty("workoutCount", out _));
        Assert.True(period2Data.TryGetProperty("totalVolume", out _));
        Assert.True(period2Data.TryGetProperty("workoutCount", out _));
    }

    [Fact]
    public async Task UserCanExportProgressData()
    {
        // Arrange - Create authenticated user and data
        var (token, userId) = await CreateAuthenticatedUser();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await CreateHistoricalStrengthData();

        // Act & Assert - Step 1: Export all data as JSON
        var jsonExportResponse = await _client.GetAsync("/api/progress/export?format=json");

        Assert.Equal(HttpStatusCode.OK, jsonExportResponse.StatusCode);

        var jsonExportContent = await jsonExportResponse.Content.ReadAsStringAsync();
        var jsonExportResult = JsonSerializer.Deserialize<JsonElement>(jsonExportContent, _jsonOptions);

        Assert.True(jsonExportResult.TryGetProperty("exportDate", out _));
        Assert.True(jsonExportResult.TryGetProperty("user", out _));
        Assert.True(jsonExportResult.TryGetProperty("workoutSessions", out var sessions));
        Assert.True(jsonExportResult.TryGetProperty("strengthLifts", out var lifts));

        Assert.Equal(JsonValueKind.Array, sessions.ValueKind);
        Assert.Equal(JsonValueKind.Array, lifts.ValueKind);

        // Act & Assert - Step 2: Export CSV format
        var csvExportResponse = await _client.GetAsync("/api/progress/export?format=csv");

        Assert.Equal(HttpStatusCode.OK, csvExportResponse.StatusCode);
        Assert.Equal("text/csv", csvExportResponse.Content.Headers.ContentType?.MediaType);

        var csvContent = await csvExportResponse.Content.ReadAsStringAsync();
        Assert.Contains("Date,Exercise,Weight,Reps,Volume", csvContent); // CSV headers

        // Act & Assert - Step 3: Export with date range
        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var rangeExportResponse = await _client.GetAsync($"/api/progress/export?startDate={startDate}&endDate={endDate}&format=json");

        Assert.Equal(HttpStatusCode.OK, rangeExportResponse.StatusCode);

        var rangeExportContent = await rangeExportResponse.Content.ReadAsStringAsync();
        var rangeExportResult = JsonSerializer.Deserialize<JsonElement>(rangeExportContent, _jsonOptions);

        Assert.True(rangeExportResult.TryGetProperty("dateRange", out var dateRange));
        Assert.True(dateRange.TryGetProperty("startDate", out _));
        Assert.True(dateRange.TryGetProperty("endDate", out _));
    }

    private async Task<(string token, string userId)> CreateAuthenticatedUser()
    {
        var testEmail = $"progress.test.{Guid.NewGuid()}@example.com";
        var registration = new
        {
            email = testEmail,
            password = "SecurePassword123!",
            firstName = "Progress",
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

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var profileResponse = await _client.GetAsync("/api/auth/profile");
        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileResult = JsonSerializer.Deserialize<JsonElement>(profileContent, _jsonOptions);

        var userId = profileResult.GetProperty("id").GetString()!;

        return (token, userId);
    }

    private async Task CreateHistoricalStrengthData()
    {
        // Create workout sessions and strength lifts over the past 3 months
        var dates = new[]
        {
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow.AddDays(-14),
            DateTime.UtcNow.AddDays(-21),
            DateTime.UtcNow.AddDays(-35),
            DateTime.UtcNow.AddDays(-50),
            DateTime.UtcNow.AddDays(-65)
        };

        var exerciseTypeId = await GetFirstExerciseTypeId();

        foreach (var date in dates)
        {
            var sessionId = await CreateTestWorkoutSession("Strength", $"Session from {date:yyyy-MM-dd}", date);

            // Progressive overload - increasing weight over time
            var baseWeight = 135 + (int)((DateTime.UtcNow - date).TotalDays / 7) * 10;

            var strengthLift = new
            {
                workoutSessionId = sessionId,
                exerciseTypeId = exerciseTypeId,
                sets = new[]
                {
                    new { reps = 5, weight = baseWeight, restSeconds = 180 },
                    new { reps = 5, weight = baseWeight + 20, restSeconds = 180 },
                    new { reps = 5, weight = baseWeight + 40, restSeconds = 180 }
                },
                notes = $"Historical data from {date:yyyy-MM-dd}"
            };

            await _client.PostAsJsonAsync("/api/strength-lifts", strengthLift, _jsonOptions);
        }
    }

    private async Task CreateHistoricalMetconData()
    {
        var dates = new[]
        {
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-24),
            DateTime.UtcNow.AddDays(-38),
            DateTime.UtcNow.AddDays(-52)
        };

        var metconTypeId = await GetFirstMetconTypeId();

        foreach (var date in dates)
        {
            var sessionId = await CreateTestWorkoutSession("Metcon", $"Metcon from {date:yyyy-MM-dd}", date);

            // Improving performance over time
            var rounds = 5 + (int)((DateTime.UtcNow - date).TotalDays / 14);

            var metconWorkout = new
            {
                workoutSessionId = sessionId,
                metconTypeId = metconTypeId,
                timeCapMinutes = 15,
                result = $"{rounds} rounds",
                movements = new[]
                {
                    new {
                        name = "Burpees",
                        reps = 10,
                        weight = (decimal?)null,
                        distance = (decimal?)null,
                        notes = "Standard burpees"
                    }
                },
                notes = $"Historical metcon from {date:yyyy-MM-dd}"
            };

            await _client.PostAsJsonAsync("/api/metcon-workouts", metconWorkout, _jsonOptions);
        }
    }

    private async Task<string> CreateTestWorkoutSession(string type, string notes, DateTime? date = null)
    {
        var workoutSession = new
        {
            date = (date ?? DateTime.UtcNow).ToString("yyyy-MM-ddTHH:mm:ssZ"),
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
