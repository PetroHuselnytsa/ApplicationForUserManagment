namespace TestFirstProject.Services.ErrorTracking;

/// <summary>
/// Interface for error tracking operations (Sentry integration).
/// </summary>
public interface IErrorTrackingService
{
    /// <summary>
    /// Captures an exception and sends it to the error tracking service.
    /// </summary>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="context">Optional context data.</param>
    /// <returns>The event ID if captured successfully, null otherwise.</returns>
    string? CaptureException(Exception exception, Dictionary<string, object>? context = null);

    /// <summary>
    /// Captures a message and sends it to the error tracking service.
    /// </summary>
    /// <param name="message">The message to capture.</param>
    /// <param name="level">The severity level.</param>
    /// <returns>The event ID if captured successfully, null otherwise.</returns>
    string? CaptureMessage(string message, ErrorSeverityLevel level = ErrorSeverityLevel.Info);

    /// <summary>
    /// Adds a breadcrumb for tracking user actions.
    /// </summary>
    /// <param name="message">The breadcrumb message.</param>
    /// <param name="category">The category of the breadcrumb.</param>
    /// <param name="data">Optional data to associate with the breadcrumb.</param>
    void AddBreadcrumb(string message, string category, Dictionary<string, string>? data = null);

    /// <summary>
    /// Sets the current user context.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="username">The user's username.</param>
    void SetUser(string userId, string? email = null, string? username = null);

    /// <summary>
    /// Sets a tag on the current scope.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    void SetTag(string key, string value);
}

/// <summary>
/// Error severity levels for classification.
/// </summary>
public enum ErrorSeverityLevel
{
    /// <summary>
    /// Debug level - detailed information for debugging.
    /// </summary>
    Debug,

    /// <summary>
    /// Info level - general information.
    /// </summary>
    Info,

    /// <summary>
    /// Warning level - potential issues that should be monitored.
    /// </summary>
    Warning,

    /// <summary>
    /// Error level - errors that need attention.
    /// </summary>
    Error,

    /// <summary>
    /// Fatal level - critical errors that require immediate attention.
    /// </summary>
    Fatal
}
