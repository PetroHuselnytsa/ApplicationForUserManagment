namespace TestFirstProject.Services.Logging.Models;

/// <summary>
/// Configuration options for request/response logging middleware.
/// </summary>
public class RequestLoggingOptions
{
    /// <summary>
    /// Whether to log request bodies. Default is true.
    /// </summary>
    public bool LogRequestBody { get; set; } = true;

    /// <summary>
    /// Whether to log response bodies. Default is true.
    /// </summary>
    public bool LogResponseBody { get; set; } = true;

    /// <summary>
    /// Maximum length of body content to log in bytes. Default is 4096.
    /// </summary>
    public int MaxBodyLogLength { get; set; } = 4096;

    /// <summary>
    /// List of paths to exclude from request logging (e.g., health checks).
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/metrics",
        "/favicon.ico",
        "/swagger"
    };

    /// <summary>
    /// List of headers that should be masked in logs.
    /// </summary>
    public List<string> SensitiveHeaders { get; set; } = new()
    {
        "Authorization",
        "X-Api-Key",
        "Cookie",
        "Set-Cookie",
        "X-Auth-Token"
    };
}
