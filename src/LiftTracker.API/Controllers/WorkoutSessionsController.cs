using LiftTracker.Application.DTOs;
using LiftTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiftTracker.API.Controllers;

/// <summary>
/// WorkoutSessions controller for workout session CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutSessionsController : ControllerBase
{
    private readonly IWorkoutSessionService _workoutSessionService;
    private readonly ILogger<WorkoutSessionsController> _logger;

    public WorkoutSessionsController(
        IWorkoutSessionService workoutSessionService,
        ILogger<WorkoutSessionsController> logger)
    {
        _workoutSessionService = workoutSessionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all workout sessions for the current user
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="limit">Maximum number of sessions to return (default: 50)</param>
    /// <returns>List of workout sessions</returns>
    [HttpGet]
    public async Task<IActionResult> GetWorkoutSessions(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] int limit = 50)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting workout sessions for user: {UserId}, StartDate: {StartDate}, EndDate: {EndDate}, Limit: {Limit}",
            userId, startDate, endDate, limit);

        try
        {
            var sessions = await _workoutSessionService.GetWorkoutSessionsByUserAsync(userId.Value, startDate, endDate);

            // Apply limit
            var limitedSessions = sessions.Take(limit);

            return Ok(limitedSessions.Select(session => new
            {
                id = session.Id,
                userId = session.UserId,
                date = session.Date,
                notes = session.Notes
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout sessions for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve workout sessions" });
        }
    }

    /// <summary>
    /// Get a specific workout session by ID
    /// </summary>
    /// <param name="id">Workout session ID</param>
    /// <returns>Workout session details</returns>
    [HttpGet("{id:guid}")]
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
    /// <param name="createSessionDto">Workout session data</param>
    /// <returns>Created workout session</returns>
    [HttpPost]
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
