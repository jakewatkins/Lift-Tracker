using LiftTracker.Application.DTOs;
using LiftTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiftTracker.API.Controllers;

/// <summary>
/// MetconWorkouts controller for metcon workout CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetconWorkoutsController : ControllerBase
{
    private readonly IMetconWorkoutService _metconWorkoutService;
    private readonly ILogger<MetconWorkoutsController> _logger;

    public MetconWorkoutsController(
        IMetconWorkoutService metconWorkoutService,
        ILogger<MetconWorkoutsController> logger)
    {
        _metconWorkoutService = metconWorkoutService;
        _logger = logger;
    }

    /// <summary>
    /// Get metcon workouts for a specific workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <returns>List of metcon workouts for the session</returns>
    [HttpGet("session/{workoutSessionId:guid}")]
    public async Task<IActionResult> GetMetconWorkoutsBySession(Guid workoutSessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting metcon workouts for session: {SessionId} for user: {UserId}", workoutSessionId, userId);

        try
        {
            var workouts = await _metconWorkoutService.GetMetconWorkoutsBySessionAsync(workoutSessionId, userId.Value);

            return Ok(workouts.Select(workout => new
            {
                id = workout.Id,
                workoutSessionId = workout.WorkoutSessionId,
                workoutTitle = workout.WorkoutTitle,
                description = workout.Description,
                timeCapMinutes = workout.TimeCapMinutes,
                timeTakenMinutes = workout.TimeTakenMinutes,
                rounds = workout.Rounds,
                repsPerRound = workout.RepsPerRound,
                comments = workout.Comments,
                order = workout.Order,
                movements = workout.Movements?.Select(m => new
                {
                    id = m.Id,
                    exerciseTypeId = m.ExerciseTypeId,
                    exerciseTypeName = m.ExerciseType?.Name,
                    reps = m.Reps,
                    weight = m.Weight,
                    distance = m.Distance,
                    order = m.Order
                })
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metcon workouts for session: {SessionId} for user: {UserId}", workoutSessionId, userId);
            return StatusCode(500, new { error = "Failed to retrieve metcon workouts" });
        }
    }

    /// <summary>
    /// Get a specific metcon workout by ID
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <returns>Metcon workout details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMetconWorkout(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting metcon workout: {WorkoutId} for user: {UserId}", id, userId);

        try
        {
            var workout = await _metconWorkoutService.GetMetconWorkoutByIdAsync(id, userId.Value);
            if (workout == null)
            {
                _logger.LogWarning("Metcon workout not found: {WorkoutId} for user: {UserId}", id, userId);
                return NotFound(new { error = "Metcon workout not found" });
            }

            return Ok(new
            {
                id = workout.Id,
                workoutSessionId = workout.WorkoutSessionId,
                workoutTitle = workout.WorkoutTitle,
                description = workout.Description,
                timeCapMinutes = workout.TimeCapMinutes,
                timeTakenMinutes = workout.TimeTakenMinutes,
                rounds = workout.Rounds,
                repsPerRound = workout.RepsPerRound,
                comments = workout.Comments,
                order = workout.Order,
                movements = workout.Movements?.Select(m => new
                {
                    id = m.Id,
                    exerciseTypeId = m.ExerciseTypeId,
                    exerciseTypeName = m.ExerciseType?.Name,
                    reps = m.Reps,
                    weight = m.Weight,
                    distance = m.Distance,
                    order = m.Order
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metcon workout: {WorkoutId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to retrieve metcon workout" });
        }
    }

    /// <summary>
    /// Get metcon workouts by title search with optional date filtering
    /// </summary>
    /// <param name="title">Workout title search term</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="limit">Maximum number of workouts to return (default: 20)</param>
    /// <returns>List of metcon workouts matching the search criteria</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchMetconWorkouts(
        [FromQuery] string? title = null,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Searching metcon workouts for user: {UserId}, Title: {Title}, StartDate: {StartDate}, EndDate: {EndDate}, Limit: {Limit}",
            userId, title, startDate, endDate, limit);

        try
        {
            var workouts = await _metconWorkoutService.SearchMetconWorkoutsAsync(userId.Value, title, startDate, endDate);

            // Apply limit
            var limitedWorkouts = workouts.Take(limit);

            return Ok(limitedWorkouts.Select(workout => new
            {
                id = workout.Id,
                workoutSessionId = workout.WorkoutSessionId,
                workoutDate = workout.WorkoutSession?.Date,
                workoutTitle = workout.WorkoutTitle,
                description = workout.Description,
                timeCapMinutes = workout.TimeCapMinutes,
                timeTakenMinutes = workout.TimeTakenMinutes,
                rounds = workout.Rounds,
                repsPerRound = workout.RepsPerRound,
                comments = workout.Comments,
                order = workout.Order,
                movementCount = workout.Movements?.Count ?? 0
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching metcon workouts for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to search metcon workouts" });
        }
    }

    /// <summary>
    /// Create a new metcon workout
    /// </summary>
    /// <param name="createWorkoutDto">Metcon workout data</param>
    /// <returns>Created metcon workout</returns>
    [HttpPost]
    public async Task<IActionResult> CreateMetconWorkout([FromBody] CreateMetconWorkoutDto createWorkoutDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogDebug("Creating metcon workout for user: {UserId} in session: {SessionId}", userId, createWorkoutDto.WorkoutSessionId);

        try
        {
            var workout = await _metconWorkoutService.CreateMetconWorkoutAsync(userId.Value, createWorkoutDto);

            _logger.LogInformation("Metcon workout created successfully: {WorkoutId} for user: {UserId}", workout.Id, userId);

            return CreatedAtAction(
                nameof(GetMetconWorkout),
                new { id = workout.Id },
                new
                {
                    id = workout.Id,
                    workoutSessionId = workout.WorkoutSessionId,
                    workoutTitle = workout.WorkoutTitle,
                    description = workout.Description,
                    timeCapMinutes = workout.TimeCapMinutes,
                    timeTakenMinutes = workout.TimeTakenMinutes,
                    rounds = workout.Rounds,
                    repsPerRound = workout.RepsPerRound,
                    comments = workout.Comments,
                    order = workout.Order,
                    movements = workout.Movements?.Select(m => new
                    {
                        id = m.Id,
                        exerciseTypeId = m.ExerciseTypeId,
                        exerciseTypeName = m.ExerciseType?.Name,
                        reps = m.Reps,
                        weight = m.Weight,
                        distance = m.Distance,
                        order = m.Order
                    })
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create metcon workout for user {UserId}: {Error}", userId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating metcon workout for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to create metcon workout" });
        }
    }

    /// <summary>
    /// Update an existing metcon workout
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <param name="updateWorkoutDto">Updated metcon workout data</param>
    /// <returns>Updated metcon workout</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMetconWorkout(Guid id, [FromBody] UpdateMetconWorkoutDto updateWorkoutDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogDebug("Updating metcon workout: {WorkoutId} for user: {UserId}", id, userId);

        try
        {
            var workout = await _metconWorkoutService.UpdateMetconWorkoutAsync(id, userId.Value, updateWorkoutDto);

            _logger.LogInformation("Metcon workout updated successfully: {WorkoutId} for user: {UserId}", id, userId);

            return Ok(new
            {
                id = workout.Id,
                workoutSessionId = workout.WorkoutSessionId,
                workoutTitle = workout.WorkoutTitle,
                description = workout.Description,
                timeCapMinutes = workout.TimeCapMinutes,
                timeTakenMinutes = workout.TimeTakenMinutes,
                rounds = workout.Rounds,
                repsPerRound = workout.RepsPerRound,
                comments = workout.Comments,
                order = workout.Order,
                movements = workout.Movements?.Select(m => new
                {
                    id = m.Id,
                    exerciseTypeId = m.ExerciseTypeId,
                    exerciseTypeName = m.ExerciseType?.Name,
                    reps = m.Reps,
                    weight = m.Weight,
                    distance = m.Distance,
                    order = m.Order
                })
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update metcon workout {WorkoutId} for user {UserId}: {Error}", id, userId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating metcon workout: {WorkoutId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to update metcon workout" });
        }
    }

    /// <summary>
    /// Delete a metcon workout
    /// </summary>
    /// <param name="id">Metcon workout ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMetconWorkout(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Deleting metcon workout: {WorkoutId} for user: {UserId}", id, userId);

        try
        {
            var result = await _metconWorkoutService.DeleteMetconWorkoutAsync(id, userId.Value);
            if (!result)
            {
                _logger.LogWarning("Failed to delete metcon workout: {WorkoutId} for user: {UserId}", id, userId);
                return NotFound(new { error = "Metcon workout not found" });
            }

            _logger.LogInformation("Metcon workout deleted successfully: {WorkoutId} for user: {UserId}", id, userId);
            return Ok(new { message = "Metcon workout deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting metcon workout: {WorkoutId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to delete metcon workout" });
        }
    }

    /// <summary>
    /// Get performance trends for a specific workout title
    /// </summary>
    /// <param name="workoutTitle">Workout title to analyze</param>
    /// <param name="limit">Maximum number of workouts to include in trend (default: 10)</param>
    /// <returns>Performance trend data</returns>
    [HttpGet("trends/{workoutTitle}")]
    public async Task<IActionResult> GetWorkoutTrends(string workoutTitle, [FromQuery] int limit = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting workout trends for title: {WorkoutTitle} for user: {UserId}, Limit: {Limit}", workoutTitle, userId, limit);

        try
        {
            var trends = await _metconWorkoutService.GetWorkoutTrendsAsync(userId.Value, workoutTitle, limit);

            return Ok(new
            {
                workoutTitle = workoutTitle,
                totalWorkouts = trends.Count(),
                trends = trends.Select(workout => new
                {
                    id = workout.Id,
                    workoutDate = workout.WorkoutSession?.Date,
                    timeTakenMinutes = workout.TimeTakenMinutes,
                    rounds = workout.Rounds,
                    repsPerRound = workout.RepsPerRound,
                    comments = workout.Comments
                }).OrderBy(w => w.workoutDate)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout trends for title: {WorkoutTitle} for user: {UserId}", workoutTitle, userId);
            return StatusCode(500, new { error = "Failed to retrieve workout trends" });
        }
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
