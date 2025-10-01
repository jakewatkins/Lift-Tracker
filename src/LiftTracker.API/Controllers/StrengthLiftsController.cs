using LiftTracker.Application.DTOs;
using LiftTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiftTracker.API.Controllers;

/// <summary>
/// StrengthLifts controller for strength lift CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StrengthLiftsController : ControllerBase
{
    private readonly IStrengthLiftService _strengthLiftService;
    private readonly ILogger<StrengthLiftsController> _logger;

    public StrengthLiftsController(
        IStrengthLiftService strengthLiftService,
        ILogger<StrengthLiftsController> logger)
    {
        _strengthLiftService = strengthLiftService;
        _logger = logger;
    }

    /// <summary>
    /// Get strength lifts for a specific workout session
    /// </summary>
    /// <param name="workoutSessionId">Workout session ID</param>
    /// <returns>List of strength lifts for the session</returns>
    [HttpGet("session/{workoutSessionId:guid}")]
    public async Task<IActionResult> GetStrengthLiftsBySession(Guid workoutSessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting strength lifts for session: {SessionId} for user: {UserId}", workoutSessionId, userId);

        try
        {
            var lifts = await _strengthLiftService.GetStrengthLiftsBySessionAsync(workoutSessionId, userId.Value);

            return Ok(lifts.Select(lift => new
            {
                id = lift.Id,
                workoutSessionId = lift.WorkoutSessionId,
                exerciseTypeId = lift.ExerciseTypeId,
                exerciseTypeName = lift.ExerciseType?.Name,
                weight = lift.Weight,
                sets = lift.Sets,
                reps = lift.Reps,
                setStructure = lift.SetStructure,
                comments = lift.Comments,
                order = lift.Order
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strength lifts for session: {SessionId} for user: {UserId}", workoutSessionId, userId);
            return StatusCode(500, new { error = "Failed to retrieve strength lifts" });
        }
    }

    /// <summary>
    /// Get a specific strength lift by ID
    /// </summary>
    /// <param name="id">Strength lift ID</param>
    /// <returns>Strength lift details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStrengthLift(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting strength lift: {LiftId} for user: {UserId}", id, userId);

        try
        {
            var lift = await _strengthLiftService.GetStrengthLiftByIdAsync(id, userId.Value);
            if (lift == null)
            {
                _logger.LogWarning("Strength lift not found: {LiftId} for user: {UserId}", id, userId);
                return NotFound(new { error = "Strength lift not found" });
            }

            return Ok(new
            {
                id = lift.Id,
                workoutSessionId = lift.WorkoutSessionId,
                exerciseTypeId = lift.ExerciseTypeId,
                exerciseTypeName = lift.ExerciseType?.Name,
                weight = lift.Weight,
                sets = lift.Sets,
                reps = lift.Reps,
                setStructure = lift.SetStructure,
                comments = lift.Comments,
                order = lift.Order
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strength lift: {LiftId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to retrieve strength lift" });
        }
    }

    /// <summary>
    /// Get strength lifts by exercise type with optional date filtering
    /// </summary>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="limit">Maximum number of lifts to return (default: 50)</param>
    /// <returns>List of strength lifts for the exercise type</returns>
    [HttpGet("exercise/{exerciseTypeId:int}")]
    public async Task<IActionResult> GetStrengthLiftsByExercise(
        int exerciseTypeId,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] int limit = 50)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting strength lifts for exercise: {ExerciseTypeId} for user: {UserId}, StartDate: {StartDate}, EndDate: {EndDate}, Limit: {Limit}",
            exerciseTypeId, userId, startDate, endDate, limit);

        try
        {
            var lifts = await _strengthLiftService.GetStrengthLiftsByExerciseAsync(userId.Value, exerciseTypeId, startDate, endDate);

            // Apply limit
            var limitedLifts = lifts.Take(limit);

            return Ok(limitedLifts.Select(lift => new
            {
                id = lift.Id,
                workoutSessionId = lift.WorkoutSessionId,
                workoutDate = lift.WorkoutSession?.Date,
                exerciseTypeId = lift.ExerciseTypeId,
                exerciseTypeName = lift.ExerciseType?.Name,
                weight = lift.Weight,
                sets = lift.Sets,
                reps = lift.Reps,
                setStructure = lift.SetStructure,
                comments = lift.Comments,
                order = lift.Order
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strength lifts for exercise: {ExerciseTypeId} for user: {UserId}", exerciseTypeId, userId);
            return StatusCode(500, new { error = "Failed to retrieve strength lifts" });
        }
    }

    /// <summary>
    /// Create a new strength lift
    /// </summary>
    /// <param name="createLiftDto">Strength lift data</param>
    /// <returns>Created strength lift</returns>
    [HttpPost]
    public async Task<IActionResult> CreateStrengthLift([FromBody] CreateStrengthLiftDto createLiftDto)
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

        _logger.LogDebug("Creating strength lift for user: {UserId} in session: {SessionId}", userId, createLiftDto.WorkoutSessionId);

        try
        {
            var lift = await _strengthLiftService.CreateStrengthLiftAsync(userId.Value, createLiftDto);

            _logger.LogInformation("Strength lift created successfully: {LiftId} for user: {UserId}", lift.Id, userId);

            return CreatedAtAction(
                nameof(GetStrengthLift),
                new { id = lift.Id },
                new
                {
                    id = lift.Id,
                    workoutSessionId = lift.WorkoutSessionId,
                    exerciseTypeId = lift.ExerciseTypeId,
                    exerciseTypeName = lift.ExerciseType?.Name,
                    weight = lift.Weight,
                    sets = lift.Sets,
                    reps = lift.Reps,
                    setStructure = lift.SetStructure,
                    comments = lift.Comments,
                    order = lift.Order
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create strength lift for user {UserId}: {Error}", userId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating strength lift for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to create strength lift" });
        }
    }

    /// <summary>
    /// Update an existing strength lift
    /// </summary>
    /// <param name="id">Strength lift ID</param>
    /// <param name="updateLiftDto">Updated strength lift data</param>
    /// <returns>Updated strength lift</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStrengthLift(Guid id, [FromBody] UpdateStrengthLiftDto updateLiftDto)
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

        _logger.LogDebug("Updating strength lift: {LiftId} for user: {UserId}", id, userId);

        try
        {
            var lift = await _strengthLiftService.UpdateStrengthLiftAsync(id, userId.Value, updateLiftDto);

            _logger.LogInformation("Strength lift updated successfully: {LiftId} for user: {UserId}", id, userId);

            return Ok(new
            {
                id = lift.Id,
                workoutSessionId = lift.WorkoutSessionId,
                exerciseTypeId = lift.ExerciseTypeId,
                exerciseTypeName = lift.ExerciseType?.Name,
                weight = lift.Weight,
                sets = lift.Sets,
                reps = lift.Reps,
                setStructure = lift.SetStructure,
                comments = lift.Comments,
                order = lift.Order
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update strength lift {LiftId} for user {UserId}: {Error}", id, userId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating strength lift: {LiftId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to update strength lift" });
        }
    }

    /// <summary>
    /// Delete a strength lift
    /// </summary>
    /// <param name="id">Strength lift ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStrengthLift(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Deleting strength lift: {LiftId} for user: {UserId}", id, userId);

        try
        {
            var result = await _strengthLiftService.DeleteStrengthLiftAsync(id, userId.Value);
            if (!result)
            {
                _logger.LogWarning("Failed to delete strength lift: {LiftId} for user: {UserId}", id, userId);
                return NotFound(new { error = "Strength lift not found" });
            }

            _logger.LogInformation("Strength lift deleted successfully: {LiftId} for user: {UserId}", id, userId);
            return Ok(new { message = "Strength lift deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting strength lift: {LiftId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to delete strength lift" });
        }
    }

    /// <summary>
    /// Get personal records for an exercise type
    /// </summary>
    /// <param name="exerciseTypeId">Exercise type ID</param>
    /// <returns>Personal record information</returns>
    [HttpGet("exercise/{exerciseTypeId:int}/pr")]
    public async Task<IActionResult> GetPersonalRecord(int exerciseTypeId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting personal record for exercise: {ExerciseTypeId} for user: {UserId}", exerciseTypeId, userId);

        try
        {
            var personalRecord = await _strengthLiftService.GetPersonalRecordAsync(userId.Value, exerciseTypeId);
            if (personalRecord == null)
            {
                return NotFound(new { error = "No personal record found for this exercise" });
            }

            return Ok(new
            {
                exerciseTypeId = exerciseTypeId,
                exerciseTypeName = personalRecord.ExerciseType?.Name,
                maxWeight = personalRecord.Weight,
                sets = personalRecord.Sets,
                reps = personalRecord.Reps,
                workoutDate = personalRecord.WorkoutSession?.Date,
                workoutSessionId = personalRecord.WorkoutSessionId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal record for exercise: {ExerciseTypeId} for user: {UserId}", exerciseTypeId, userId);
            return StatusCode(500, new { error = "Failed to retrieve personal record" });
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
