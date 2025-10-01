using LiftTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiftTracker.API.Controllers;

/// <summary>
/// Progress controller for analytics and progress tracking endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;
    private readonly ILogger<ProgressController> _logger;

    public ProgressController(
        IProgressService progressService,
        ILogger<ProgressController> logger)
    {
        _progressService = progressService;
        _logger = logger;
    }

    /// <summary>
    /// Get user overview statistics
    /// </summary>
    /// <returns>Overall user statistics and achievements</returns>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting progress overview for user: {UserId}", userId);

        try
        {
            var overview = await _progressService.GetUserOverviewAsync(userId.Value);

            return Ok(new
            {
                totalWorkouts = overview.TotalWorkouts,
                totalWorkoutDays = overview.TotalWorkoutDays,
                totalStrengthLifts = overview.TotalStrengthLifts,
                totalMetconWorkouts = overview.TotalMetconWorkouts,
                firstWorkoutDate = overview.FirstWorkoutDate,
                lastWorkoutDate = overview.LastWorkoutDate,
                currentStreak = overview.CurrentStreak,
                longestStreak = overview.LongestStreak,
                averageWorkoutsPerWeek = overview.AverageWorkoutsPerWeek
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress overview for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve progress overview" });
        }
    }

    /// <summary>
    /// Get strength progress chart data for a specific exercise
    /// </summary>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="period">Time period (30, 90, 180, 365 days or 'all')</param>
    /// <param name="metric">Metric to track (weight, volume, or maxWeight)</param>
    /// <returns>Chart data points for strength progress</returns>
    [HttpGet("strength/{exerciseTypeId:int}/chart")]
    public async Task<IActionResult> GetStrengthProgressChart(
        int exerciseTypeId,
        [FromQuery] string period = "90",
        [FromQuery] string metric = "weight")
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        // Validate parameters
        if (!IsValidPeriod(period))
        {
            return BadRequest(new { error = "Invalid period. Must be 30, 90, 180, 365, or 'all'" });
        }

        if (!IsValidMetric(metric))
        {
            return BadRequest(new { error = "Invalid metric. Must be 'weight', 'volume', or 'maxWeight'" });
        }

        _logger.LogDebug("Getting strength progress chart for exercise: {ExerciseTypeId}, period: {Period}, metric: {Metric} for user: {UserId}",
            exerciseTypeId, period, metric, userId);

        try
        {
            var chartData = await _progressService.GetStrengthProgressChartAsync(userId.Value, exerciseTypeId, period, metric);

            return Ok(new
            {
                exerciseTypeId = exerciseTypeId,
                exerciseTypeName = chartData.ExerciseTypeName,
                period = period,
                metric = metric,
                dataPoints = chartData.DataPoints.Select(dp => new
                {
                    date = dp.Date,
                    value = dp.Value,
                    sets = dp.Sets,
                    reps = dp.Reps,
                    workoutSessionId = dp.WorkoutSessionId
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strength progress chart for exercise: {ExerciseTypeId} for user: {UserId}", exerciseTypeId, userId);
            return StatusCode(500, new { error = "Failed to retrieve strength progress chart" });
        }
    }

    /// <summary>
    /// Get workout frequency chart data
    /// </summary>
    /// <param name="period">Time period (30, 90, 180, 365 days or 'all')</param>
    /// <param name="groupBy">Group by period (day, week, month)</param>
    /// <returns>Chart data for workout frequency</returns>
    [HttpGet("frequency/chart")]
    public async Task<IActionResult> GetWorkoutFrequencyChart(
        [FromQuery] string period = "90",
        [FromQuery] string groupBy = "week")
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        // Validate parameters
        if (!IsValidPeriod(period))
        {
            return BadRequest(new { error = "Invalid period. Must be 30, 90, 180, 365, or 'all'" });
        }

        if (!IsValidGroupBy(groupBy))
        {
            return BadRequest(new { error = "Invalid groupBy. Must be 'day', 'week', or 'month'" });
        }

        _logger.LogDebug("Getting workout frequency chart for period: {Period}, groupBy: {GroupBy} for user: {UserId}",
            period, groupBy, userId);

        try
        {
            var chartData = await _progressService.GetWorkoutFrequencyChartAsync(userId.Value, period, groupBy);

            return Ok(new
            {
                period = period,
                groupBy = groupBy,
                dataPoints = chartData.Select(dp => new
                {
                    period = dp.Period,
                    workoutCount = dp.WorkoutCount,
                    strengthLiftsCount = dp.StrengthLiftsCount,
                    metconWorkoutsCount = dp.MetconWorkoutsCount
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout frequency chart for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve workout frequency chart" });
        }
    }

    /// <summary>
    /// Get personal records summary
    /// </summary>
    /// <param name="exerciseTypeIds">Optional list of exercise type IDs to filter</param>
    /// <param name="limit">Maximum number of PRs to return (default: 20)</param>
    /// <returns>List of personal records</returns>
    [HttpGet("personal-records")]
    public async Task<IActionResult> GetPersonalRecords(
        [FromQuery] int[]? exerciseTypeIds = null,
        [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting personal records for user: {UserId}, ExerciseTypes: {ExerciseTypeIds}, Limit: {Limit}",
            userId, exerciseTypeIds != null ? string.Join(",", exerciseTypeIds) : "all", limit);

        try
        {
            var personalRecords = await _progressService.GetPersonalRecordsAsync(userId.Value, exerciseTypeIds);

            // Apply limit
            var limitedRecords = personalRecords.Take(limit);

            return Ok(limitedRecords.Select(pr => new
            {
                exerciseTypeId = pr.ExerciseTypeId,
                exerciseTypeName = pr.ExerciseTypeName,
                maxWeight = pr.MaxWeight,
                sets = pr.Sets,
                reps = pr.Reps,
                workoutDate = pr.WorkoutDate,
                workoutSessionId = pr.WorkoutSessionId,
                isRecentPR = pr.IsRecentPR
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal records for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve personal records" });
        }
    }

    /// <summary>
    /// Get recent achievements and milestones
    /// </summary>
    /// <param name="days">Number of recent days to check (default: 30)</param>
    /// <param name="limit">Maximum number of achievements to return (default: 10)</param>
    /// <returns>List of recent achievements</returns>
    [HttpGet("achievements")]
    public async Task<IActionResult> GetRecentAchievements(
        [FromQuery] int days = 30,
        [FromQuery] int limit = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting recent achievements for user: {UserId}, Days: {Days}, Limit: {Limit}",
            userId, days, limit);

        try
        {
            var achievements = await _progressService.GetRecentAchievementsAsync(userId.Value, days);

            // Apply limit
            var limitedAchievements = achievements.Take(limit);

            return Ok(limitedAchievements.Select(achievement => new
            {
                type = achievement.Type,
                title = achievement.Title,
                description = achievement.Description,
                achievedDate = achievement.AchievedDate,
                value = achievement.Value,
                exerciseTypeId = achievement.ExerciseTypeId,
                exerciseTypeName = achievement.ExerciseTypeName,
                workoutSessionId = achievement.WorkoutSessionId
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent achievements for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve recent achievements" });
        }
    }

    /// <summary>
    /// Get volume progress trends for strength training
    /// </summary>
    /// <param name="period">Time period (30, 90, 180, 365 days or 'all')</param>
    /// <param name="exerciseTypeIds">Optional list of exercise type IDs to include</param>
    /// <returns>Volume trend data</returns>
    [HttpGet("volume/trends")]
    public async Task<IActionResult> GetVolumeTrends(
        [FromQuery] string period = "90",
        [FromQuery] int[]? exerciseTypeIds = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        // Validate parameters
        if (!IsValidPeriod(period))
        {
            return BadRequest(new { error = "Invalid period. Must be 30, 90, 180, 365, or 'all'" });
        }

        _logger.LogDebug("Getting volume trends for period: {Period}, ExerciseTypes: {ExerciseTypeIds} for user: {UserId}",
            period, exerciseTypeIds != null ? string.Join(",", exerciseTypeIds) : "all", userId);

        try
        {
            var volumeTrends = await _progressService.GetVolumeTrendsAsync(userId.Value, period, exerciseTypeIds);

            return Ok(new
            {
                period = period,
                exerciseTypeIds = exerciseTypeIds,
                trends = volumeTrends.Select(trend => new
                {
                    date = trend.Date,
                    totalVolume = trend.TotalVolume,
                    workoutCount = trend.WorkoutCount,
                    averageVolumePerWorkout = trend.AverageVolumePerWorkout,
                    exerciseBreakdown = trend.ExerciseBreakdown?.Select(eb => new
                    {
                        exerciseTypeId = eb.ExerciseTypeId,
                        exerciseTypeName = eb.ExerciseTypeName,
                        volume = eb.Volume
                    })
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting volume trends for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve volume trends" });
        }
    }

    /// <summary>
    /// Get metcon performance trends
    /// </summary>
    /// <param name="workoutTitle">Optional workout title to filter by</param>
    /// <param name="period">Time period (30, 90, 180, 365 days or 'all')</param>
    /// <returns>Metcon performance trend data</returns>
    [HttpGet("metcon/trends")]
    public async Task<IActionResult> GetMetconTrends(
        [FromQuery] string? workoutTitle = null,
        [FromQuery] string period = "90")
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        // Validate parameters
        if (!IsValidPeriod(period))
        {
            return BadRequest(new { error = "Invalid period. Must be 30, 90, 180, 365, or 'all'" });
        }

        _logger.LogDebug("Getting metcon trends for period: {Period}, WorkoutTitle: {WorkoutTitle} for user: {UserId}",
            period, workoutTitle ?? "all", userId);

        try
        {
            var metconTrends = await _progressService.GetMetconTrendsAsync(userId.Value, workoutTitle, period);

            return Ok(new
            {
                period = period,
                workoutTitle = workoutTitle,
                trends = metconTrends.Select(trend => new
                {
                    workoutTitle = trend.WorkoutTitle,
                    dataPoints = trend.DataPoints.Select(dp => new
                    {
                        date = dp.Date,
                        timeTakenMinutes = dp.TimeTakenMinutes,
                        rounds = dp.Rounds,
                        repsPerRound = dp.RepsPerRound,
                        workoutSessionId = dp.WorkoutSessionId
                    })
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metcon trends for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve metcon trends" });
        }
    }

    /// <summary>
    /// Helper method to validate period parameter
    /// </summary>
    private bool IsValidPeriod(string period)
    {
        return period == "all" || (int.TryParse(period, out var days) && new[] { 30, 90, 180, 365 }.Contains(days));
    }

    /// <summary>
    /// Helper method to validate metric parameter
    /// </summary>
    private bool IsValidMetric(string metric)
    {
        return new[] { "weight", "volume", "maxWeight" }.Contains(metric.ToLower());
    }

    /// <summary>
    /// Helper method to validate groupBy parameter
    /// </summary>
    private bool IsValidGroupBy(string groupBy)
    {
        return new[] { "day", "week", "month" }.Contains(groupBy.ToLower());
    }

    /// <summary>
    /// Helper method to extract current user ID from JWT claims
    /// </summary>
    /// <returns>Current user ID or null if not found/invalid</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
