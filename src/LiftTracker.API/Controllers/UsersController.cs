using LiftTracker.Application.DTOs;
using LiftTracker.Application.Interfaces;
using LiftTracker.API.Controllers.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiftTracker.API.Controllers;

/// <summary>
/// Users controller for profile management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseAuthenticatedController
{
    private readonly IUserService _userService;
    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger) : base(logger)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>Current user profile information</returns>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting profile for user: {UserId}", userId);

        var user = await _userService.GetUserByIdAsync(userId.Value);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return NotFound(new { error = "User not found" });
        }

        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email,
            googleId = user.GoogleId
        });
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    /// <param name="updateUserDto">Updated user information</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDto updateUserDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Updating profile for user: {UserId}", userId);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userService.UpdateUserAsync(userId.Value, updateUserDto);

            _logger.LogInformation("User profile updated successfully: {UserId}", userId);

            return Ok(new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                googleId = user.GoogleId
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update user {UserId}: {Error}", userId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to update profile" });
        }
    }

    /// <summary>
    /// Delete current user account
    /// </summary>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogInformation("Deleting user account: {UserId}", userId);

        try
        {
            var result = await _userService.DeleteUserAsync(userId.Value);
            if (!result)
            {
                _logger.LogWarning("Failed to delete user: {UserId}", userId);
                return NotFound(new { error = "User not found" });
            }

            _logger.LogInformation("User account deleted successfully: {UserId}", userId);
            return Ok(new { message = "Account deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user account: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to delete account" });
        }
    }

    /// <summary>
    /// Check if email is available for registration
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>Availability status</returns>
    [HttpGet("check-email")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { error = "Email is required" });
        }

        _logger.LogDebug("Checking email availability: {Email}", email);

        try
        {
            var exists = await _userService.UserExistsAsync(email);
            return Ok(new { available = !exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability: {Email}", email);
            return StatusCode(500, new { error = "Failed to check email availability" });
        }
    }

    /// <summary>
    /// Get user statistics
    /// </summary>
    /// <returns>User account statistics</returns>
    [HttpGet("me/stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogDebug("Getting statistics for user: {UserId}", userId);

        try
        {
            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Basic stats for now - can be enhanced with workout counts, etc.
            return Ok(new
            {
                userId = user.Id,
                memberSince = user.Id.ToString(), // Could track creation date if added to entity
                profileComplete = !string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Email)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to get user statistics" });
        }
    }

}
