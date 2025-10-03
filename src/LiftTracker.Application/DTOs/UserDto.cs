using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Application.DTOs;

/// <summary>
/// Data transfer object for user information
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public DateTime? LastLoginDate { get; set; }
}

/// <summary>
/// Data transfer object for creating a new user
/// </summary>
public class CreateUserDto
{
    [Required]
    [EmailAddress]
    [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for updating user profile
/// </summary>
public class UpdateUserDto
{
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for user login response
/// </summary>
public class UserLoginResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime TokenExpiration { get; set; }
    public bool IsNewUser { get; set; }
}
