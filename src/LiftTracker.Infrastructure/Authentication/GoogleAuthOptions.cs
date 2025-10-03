namespace LiftTracker.Infrastructure.Authentication;

/// <summary>
/// Configuration options for Google OAuth authentication
/// </summary>
public class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    /// <summary>
    /// Google OAuth Client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Google OAuth Client Secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// JWT signing key
    /// </summary>
    public string JwtKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT issuer
    /// </summary>
    public string JwtIssuer { get; set; } = "LiftTracker";

    /// <summary>
    /// JWT audience
    /// </summary>
    public string JwtAudience { get; set; } = "LiftTracker";

    /// <summary>
    /// JWT expiration time in minutes
    /// </summary>
    public int JwtExpirationMinutes { get; set; } = 60;
}
