using LiftTracker.Application.DTOs;
using LiftTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiftTracker.API.Controllers;

/// <summary>
/// WorkoutSessions controller for workout session CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutSessionsController : BaseAuthenticatedController
{
    private readonly IWorkoutSessionService _workoutSessionService;

    public WorkoutSessionsController(
        IWorkoutSessionService workoutSessionService,
        ILogger<WorkoutSessionsController> logger) : base(logger)
    {
        _workoutSessionService = workoutSessionService;
    }

    /// <summary>
    /// Get all workout sessions for the current user
    /// </summary>
    /// <param name="startDate">Optional start date filter (format: YYYY-MM-DD)</param>
    /// <param name="endDate">Optional end date filter (format: YYYY-MM-DD)</param>
    /// <param name="limit">Maximum number of sessions to return (default: 50, max: 100)</param>
    /// <returns>List of workout sessions matching the filter criteria</returns>
    /// <response code="200">Returns the list of workout sessions</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkoutSessions(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] int limit = 50)
    {
        var authResult = ValidateAuthentication();
        if (authResult.Result != null) return authResult.Result;
        var userId = authResult.Value;

        _logger.LogDebug("Getting workout sessions for user: {UserId}, StartDate: {StartDate}, EndDate: {EndDate}, Limit: {Limit}",
            userId, startDate, endDate, limit);

        return await ExecuteWithErrorHandling(async () =>
        {
            var sessions = await _workoutSessionService.GetWorkoutSessionsByUserAsync(userId, startDate, endDate);

            // Apply limit
            var limitedSessions = sessions.Take(limit);

            return limitedSessions.Select(session => new
            {
                id = session.Id,
                userId = session.UserId,
                date = session.Date,
                notes = session.Notes
            });
        }, "get workout sessions", userId);
    }

    /// <summary>
    /// Get a specific workout session by ID
    /// </summary>
    /// <param name="id">Workout session unique identifier (GUID format)</param>
    /// <returns>Workout session details including associated exercises</returns>
    /// <response code="200">Returns the workout session details</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">Workout session not found or user doesn't have access</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkoutSession(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting workout session: {SessionId} for user: {UserId}", id, userId);

        try
        {
            var session = await _workoutSessionService.GetWorkoutSessionByIdAsync(id, userId.Value);
            if (session == null)
            {
                _logger.LogWarning("Workout session not found: {SessionId} for user: {UserId}", id, userId);
                return NotFound(new { error = "Workout session not found" });
            }

            return Ok(new
            {
                id = session.Id,
                userId = session.UserId,
                date = session.Date,
                notes = session.Notes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout session: {SessionId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to retrieve workout session" });
        }
    }

    /// <summary>
    /// Create a new workout session
    /// </summary>
    /// <param name="createSessionDto">Workout session creation data</param>
    /// <returns>Created workout session with generated ID</returns>
    /// <response code="201">Workout session created successfully</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="409">Workout session already exists for the specified date</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateWorkoutSession([FromBody] CreateWorkoutSessionDto createSessionDto)
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

        _logger.LogDebug("Creating workout session for user: {UserId} on date: {Date}", userId, createSessionDto.Date);

        try
        {
            var session = await _workoutSessionService.CreateWorkoutSessionAsync(userId.Value, createSessionDto);

            _logger.LogInformation("Workout session created successfully: {SessionId} for user: {UserId}", session.Id, userId);

            return CreatedAtAction(
                nameof(GetWorkoutSession),
                new { id = session.Id },
                new
                {
                    id = session.Id,
                    userId = session.UserId,
                    date = session.Date,
                    notes = session.Notes
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create workout session for user {UserId}: {Error}", userId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workout session for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to create workout session" });
        }
    }

    /// <summary>
    /// Update an existing workout session
    /// </summary>
    /// <param name="id">Workout session ID</param>
    /// <param name="updateSessionDto">Updated workout session data</param>
    /// <returns>Updated workout session</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWorkoutSession(Guid id, [FromBody] UpdateWorkoutSessionDto updateSessionDto)
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

        _logger.LogDebug("Updating workout session: {SessionId} for user: {UserId}", id, userId);

        try
        {
            var session = await _workoutSessionService.UpdateWorkoutSessionAsync(id, userId.Value, updateSessionDto);

            _logger.LogInformation("Workout session updated successfully: {SessionId} for user: {UserId}", id, userId);

            return Ok(new
            {
                id = session.Id,
                userId = session.UserId,
                date = session.Date,
                notes = session.Notes
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update workout session {SessionId} for user {UserId}: {Error}", id, userId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workout session: {SessionId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to update workout session" });
        }
    }

    /// <summary>
    /// Delete a workout session
    /// </summary>
    /// <param name="id">Workout session ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWorkoutSession(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Deleting workout session: {SessionId} for user: {UserId}", id, userId);

        try
        {
            var result = await _workoutSessionService.DeleteWorkoutSessionAsync(id, userId.Value);
            if (!result)
            {
                _logger.LogWarning("Failed to delete workout session: {SessionId} for user: {UserId}", id, userId);
                return NotFound(new { error = "Workout session not found" });
            }

            _logger.LogInformation("Workout session deleted successfully: {SessionId} for user: {UserId}", id, userId);
            return Ok(new { message = "Workout session deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workout session: {SessionId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to delete workout session" });
        }
    }

    /// <summary>
    /// Get workout session for a specific date
    /// </summary>
    /// <param name="date">Date to search for</param>
    /// <returns>Workout session for the specified date</returns>
    [HttpGet("by-date/{date}")]
    public async Task<IActionResult> GetWorkoutSessionByDate(DateOnly date)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting workout session for user: {UserId} on date: {Date}", userId, date);

        try
        {
            var session = await _workoutSessionService.GetWorkoutSessionByDateAsync(userId.Value, date);
            if (session == null)
            {
                return NotFound(new { error = "No workout session found for the specified date" });
            }

            return Ok(new
            {
                id = session.Id,
                userId = session.UserId,
                date = session.Date,
                notes = session.Notes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout session by date for user: {UserId} on date: {Date}", userId, date);
            return StatusCode(500, new { error = "Failed to retrieve workout session" });
        }
    }
}
