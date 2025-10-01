using LiftTracker.Application.Interfaces;
using LiftTracker.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiftTracker.API.Controllers;

/// <summary>
/// Authentication controller for Google OAuth login/logout operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        JwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Initiate Google OAuth login flow
    /// </summary>
    /// <param name="returnUrl">URL to redirect to after successful authentication</param>
    /// <returns>Redirect to Google OAuth</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        _logger.LogInformation("Initiating Google OAuth login flow");

        var redirectUrl = Url.Action(nameof(Callback), "Auth");
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl,
            Items = { { "returnUrl", returnUrl ?? "/" } }
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handle Google OAuth callback
    /// </summary>
    /// <returns>Redirect to application with auth token or error</returns>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback()
    {
        _logger.LogInformation("Processing Google OAuth callback");

        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning("Google OAuth authentication failed");
                return BadRequest(new { error = "Authentication failed" });
            }

            var claims = authenticateResult.Principal?.Claims;
            if (claims == null)
            {
                _logger.LogWarning("No claims received from Google OAuth");
                return BadRequest(new { error = "No user information received" });
            }

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(googleId))
            {
                _logger.LogWarning("Missing required user information from Google OAuth");
                return BadRequest(new { error = "Incomplete user information" });
            }

            // Create or update user
            var user = await _userService.CreateOrUpdateUserAsync(email, name);

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);

            _logger.LogInformation("User authenticated successfully: {UserId}", user.Id);

            // Get return URL from state
            var returnUrl = authenticateResult.Properties?.Items["returnUrl"] ?? "/";

            // For SPA, return JSON with token instead of redirect
            if (Request.Headers.Accept.Contains("application/json"))
            {
                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = user.Id,
                        name = user.Name,
                        email = user.Email
                    }
                });
            }

            // For traditional web app, redirect with token in query (not recommended for production)
            return Redirect($"{returnUrl}?token={token}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google OAuth callback processing");
            return StatusCode(500, new { error = "Authentication processing failed" });
        }
    }

    /// <summary>
    /// Get current user profile information
    /// </summary>
    /// <returns>User profile data</returns>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid user ID in token: {UserIdClaim}", userIdClaim);
            return Unauthorized(new { error = "Invalid user token" });
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return NotFound(new { error = "User not found" });
        }

        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email
        });
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    /// <returns>Success response</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User logging out: {UserId}", userIdClaim);

        await HttpContext.SignOutAsync();

        return Ok(new { message = "Logout successful" });
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <returns>New JWT token</returns>
    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        var newToken = _jwtTokenService.GenerateToken(user);

        return Ok(new { token = newToken });
    }
}
