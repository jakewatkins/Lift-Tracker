using System.ComponentModel.DataAnnotations;

namespace LiftTracker.Domain.Entities;

/// <summary>
/// Represents a user in the lift tracking system
/// </summary>
public class User
{
    /// <summary>
    /// Unique user identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address from Google OAuth
    /// </summary>
    [Required]
    [StringLength(254)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name from Google profile
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When the user account was first created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Last time the user logged into the system
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Navigation property for user's workout sessions
    /// </summary>
    public virtual ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();

    /// <summary>
    /// Creates a new user with generated ID and current timestamp
    /// </summary>
    public User()
    {
        Id = Guid.NewGuid();
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new user with specified email and name
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="name">User's display name</param>
    public User(string email, string name) : this()
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Updates the last login timestamp
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginDate = DateTime.UtcNow;
    }
}
