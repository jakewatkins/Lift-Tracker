namespace LiftTracker.API.Configuration;

/// <summary>
/// Configuration options for the application
/// </summary>
public class ApplicationOptions
{
    public const string SectionName = "Application";

    /// <summary>
    /// Application name
    /// </summary>
    public string Name { get; set; } = "LiftTracker";

    /// <summary>
    /// Application version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Application environment
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Enable detailed error messages
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Maximum file upload size in bytes
    /// </summary>
    public long MaxFileUploadSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable request rate limiting
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;
}
