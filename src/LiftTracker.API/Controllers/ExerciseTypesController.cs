using LiftTracker.Application.DTOs;
using LiftTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiftTracker.API.Controllers;

/// <summary>
/// ExerciseTypes controller for exercise type reference data management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExerciseTypesController : ControllerBase
{
    private readonly IExerciseTypeService _exerciseTypeService;
    private readonly ILogger<ExerciseTypesController> _logger;

    public ExerciseTypesController(
        IExerciseTypeService exerciseTypeService,
        ILogger<ExerciseTypesController> logger)
    {
        _exerciseTypeService = exerciseTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all exercise types with optional filtering
    /// </summary>
    /// <param name="category">Optional category filter (strength, cardio, etc.)</param>
    /// <param name="equipment">Optional equipment filter</param>
    /// <param name="muscleGroup">Optional muscle group filter</param>
    /// <param name="search">Optional search term for name/description</param>
    /// <returns>List of exercise types</returns>
    [HttpGet]
    public async Task<IActionResult> GetExerciseTypes(
        [FromQuery] string? category = null,
        [FromQuery] string? equipment = null,
        [FromQuery] string? muscleGroup = null,
        [FromQuery] string? search = null)
    {
        _logger.LogDebug("Getting exercise types with filters - Category: {Category}, Equipment: {Equipment}, MuscleGroup: {MuscleGroup}, Search: {Search}",
            category, equipment, muscleGroup, search);

        try
        {
            var exerciseTypes = await _exerciseTypeService.GetExerciseTypesAsync(category, equipment, muscleGroup, search);

            return Ok(exerciseTypes.Select(et => new
            {
                id = et.Id,
                name = et.Name,
                description = et.Description,
                category = et.Category,
                equipment = et.Equipment,
                muscleGroup = et.MuscleGroup,
                instructions = et.Instructions,
                isActive = et.IsActive,
                createdAt = et.CreatedAt,
                updatedAt = et.UpdatedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise types");
            return StatusCode(500, new { error = "Failed to retrieve exercise types" });
        }
    }

    /// <summary>
    /// Get exercise types by category
    /// </summary>
    /// <param name="category">Exercise category (strength, cardio, flexibility, etc.)</param>
    /// <returns>List of exercise types in the specified category</returns>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetExerciseTypesByCategory(string category)
    {
        _logger.LogDebug("Getting exercise types for category: {Category}", category);

        try
        {
            var exerciseTypes = await _exerciseTypeService.GetExerciseTypesByCategoryAsync(category);

            return Ok(exerciseTypes.Select(et => new
            {
                id = et.Id,
                name = et.Name,
                description = et.Description,
                equipment = et.Equipment,
                muscleGroup = et.MuscleGroup,
                instructions = et.Instructions,
                isActive = et.IsActive
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise types for category: {Category}", category);
            return StatusCode(500, new { error = "Failed to retrieve exercise types" });
        }
    }

    /// <summary>
    /// Get a specific exercise type by ID
    /// </summary>
    /// <param name="id">Exercise type ID</param>
    /// <returns>Exercise type details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetExerciseType(int id)
    {
        _logger.LogDebug("Getting exercise type: {ExerciseTypeId}", id);

        try
        {
            var exerciseType = await _exerciseTypeService.GetExerciseTypeByIdAsync(id);
            if (exerciseType == null)
            {
                _logger.LogWarning("Exercise type not found: {ExerciseTypeId}", id);
                return NotFound(new { error = "Exercise type not found" });
            }

            return Ok(new
            {
                id = exerciseType.Id,
                name = exerciseType.Name,
                description = exerciseType.Description,
                category = exerciseType.Category,
                equipment = exerciseType.Equipment,
                muscleGroup = exerciseType.MuscleGroup,
                instructions = exerciseType.Instructions,
                isActive = exerciseType.IsActive,
                createdAt = exerciseType.CreatedAt,
                updatedAt = exerciseType.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise type: {ExerciseTypeId}", id);
            return StatusCode(500, new { error = "Failed to retrieve exercise type" });
        }
    }

    /// <summary>
    /// Search exercise types by name or description
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results (default: 20)</param>
    /// <returns>List of matching exercise types</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchExerciseTypes([FromQuery] string query, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { error = "Search query is required" });
        }

        _logger.LogDebug("Searching exercise types with query: {Query}, Limit: {Limit}", query, limit);

        try
        {
            var exerciseTypes = await _exerciseTypeService.SearchExerciseTypesAsync(query);

            // Apply limit
            var limitedResults = exerciseTypes.Take(limit);

            return Ok(limitedResults.Select(et => new
            {
                id = et.Id,
                name = et.Name,
                description = et.Description,
                category = et.Category,
                equipment = et.Equipment,
                muscleGroup = et.MuscleGroup,
                isActive = et.IsActive
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching exercise types with query: {Query}", query);
            return StatusCode(500, new { error = "Failed to search exercise types" });
        }
    }

    /// <summary>
    /// Get available categories
    /// </summary>
    /// <returns>List of exercise categories</returns>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        _logger.LogDebug("Getting exercise categories");

        try
        {
            var categories = await _exerciseTypeService.GetCategoriesAsync();

            return Ok(categories.Select(category => new
            {
                name = category.Name,
                displayName = category.DisplayName,
                description = category.Description,
                exerciseCount = category.ExerciseCount
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise categories");
            return StatusCode(500, new { error = "Failed to retrieve exercise categories" });
        }
    }

    /// <summary>
    /// Get available equipment types
    /// </summary>
    /// <returns>List of equipment types</returns>
    [HttpGet("equipment")]
    public async Task<IActionResult> GetEquipment()
    {
        _logger.LogDebug("Getting exercise equipment types");

        try
        {
            var equipment = await _exerciseTypeService.GetEquipmentTypesAsync();

            return Ok(equipment.Select(eq => new
            {
                name = eq.Name,
                displayName = eq.DisplayName,
                description = eq.Description,
                exerciseCount = eq.ExerciseCount
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise equipment types");
            return StatusCode(500, new { error = "Failed to retrieve exercise equipment types" });
        }
    }

    /// <summary>
    /// Get available muscle groups
    /// </summary>
    /// <returns>List of muscle groups</returns>
    [HttpGet("muscle-groups")]
    public async Task<IActionResult> GetMuscleGroups()
    {
        _logger.LogDebug("Getting muscle groups");

        try
        {
            var muscleGroups = await _exerciseTypeService.GetMuscleGroupsAsync();

            return Ok(muscleGroups.Select(mg => new
            {
                name = mg.Name,
                displayName = mg.DisplayName,
                description = mg.Description,
                exerciseCount = mg.ExerciseCount
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting muscle groups");
            return StatusCode(500, new { error = "Failed to retrieve muscle groups" });
        }
    }

    /// <summary>
    /// Create a new custom exercise type (admin only)
    /// </summary>
    /// <param name="createExerciseTypeDto">Exercise type data</param>
    /// <returns>Created exercise type</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateExerciseType([FromBody] CreateExerciseTypeDto createExerciseTypeDto)
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

        _logger.LogDebug("Creating exercise type: {Name} by user: {UserId}", createExerciseTypeDto.Name, userId);

        try
        {
            var exerciseType = await _exerciseTypeService.CreateExerciseTypeAsync(createExerciseTypeDto);

            _logger.LogInformation("Exercise type created successfully: {ExerciseTypeId} by user: {UserId}", exerciseType.Id, userId);

            return CreatedAtAction(
                nameof(GetExerciseType),
                new { id = exerciseType.Id },
                new
                {
                    id = exerciseType.Id,
                    name = exerciseType.Name,
                    description = exerciseType.Description,
                    category = exerciseType.Category,
                    equipment = exerciseType.Equipment,
                    muscleGroup = exerciseType.MuscleGroup,
                    instructions = exerciseType.Instructions,
                    isActive = exerciseType.IsActive,
                    createdAt = exerciseType.CreatedAt
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create exercise type for user {UserId}: {Error}", userId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exercise type for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to create exercise type" });
        }
    }

    /// <summary>
    /// Update an existing exercise type (admin only)
    /// </summary>
    /// <param name="id">Exercise type ID</param>
    /// <param name="updateExerciseTypeDto">Updated exercise type data</param>
    /// <returns>Updated exercise type</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateExerciseType(int id, [FromBody] UpdateExerciseTypeDto updateExerciseTypeDto)
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

        _logger.LogDebug("Updating exercise type: {ExerciseTypeId} by user: {UserId}", id, userId);

        try
        {
            var exerciseType = await _exerciseTypeService.UpdateExerciseTypeAsync(id, updateExerciseTypeDto);

            _logger.LogInformation("Exercise type updated successfully: {ExerciseTypeId} by user: {UserId}", id, userId);

            return Ok(new
            {
                id = exerciseType.Id,
                name = exerciseType.Name,
                description = exerciseType.Description,
                category = exerciseType.Category,
                equipment = exerciseType.Equipment,
                muscleGroup = exerciseType.MuscleGroup,
                instructions = exerciseType.Instructions,
                isActive = exerciseType.IsActive,
                createdAt = exerciseType.CreatedAt,
                updatedAt = exerciseType.UpdatedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update exercise type {ExerciseTypeId} for user {UserId}: {Error}", id, userId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating exercise type: {ExerciseTypeId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to update exercise type" });
        }
    }

    /// <summary>
    /// Deactivate an exercise type (admin only)
    /// </summary>
    /// <param name="id">Exercise type ID</param>
    /// <returns>Deactivation confirmation</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateExerciseType(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Deactivating exercise type: {ExerciseTypeId} by user: {UserId}", id, userId);

        try
        {
            var result = await _exerciseTypeService.DeactivateExerciseTypeAsync(id);
            if (!result)
            {
                _logger.LogWarning("Failed to deactivate exercise type: {ExerciseTypeId} for user: {UserId}", id, userId);
                return NotFound(new { error = "Exercise type not found" });
            }

            _logger.LogInformation("Exercise type deactivated successfully: {ExerciseTypeId} by user: {UserId}", id, userId);
            return Ok(new { message = "Exercise type deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating exercise type: {ExerciseTypeId} for user: {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to deactivate exercise type" });
        }
    }

    /// <summary>
    /// Get popular exercise types based on usage
    /// </summary>
    /// <param name="limit">Maximum number of exercise types to return (default: 10)</param>
    /// <param name="period">Time period for popularity calculation in days (default: 30)</param>
    /// <returns>List of popular exercise types</returns>
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularExerciseTypes([FromQuery] int limit = 10, [FromQuery] int period = 30)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting popular exercise types for user: {UserId}, Limit: {Limit}, Period: {Period}", userId, limit, period);

        try
        {
            var popularExercises = await _exerciseTypeService.GetPopularExerciseTypesAsync(userId.Value, limit, period);

            return Ok(popularExercises.Select(pe => new
            {
                exerciseType = new
                {
                    id = pe.ExerciseType.Id,
                    name = pe.ExerciseType.Name,
                    description = pe.ExerciseType.Description,
                    category = pe.ExerciseType.Category,
                    equipment = pe.ExerciseType.Equipment,
                    muscleGroup = pe.ExerciseType.MuscleGroup
                },
                usageCount = pe.UsageCount,
                lastUsed = pe.LastUsed,
                rank = pe.Rank
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular exercise types for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve popular exercise types" });
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
