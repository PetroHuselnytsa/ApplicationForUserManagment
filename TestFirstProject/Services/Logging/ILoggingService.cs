using Microsoft.Extensions.Logging;

namespace TestFirstProject.Services.Logging;

/// <summary>
/// Interface for the centralized logging service with structured logging support.
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Logs an error message with optional exception.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="exception">Optional exception to log.</param>
    /// <param name="args">The message arguments.</param>
    void LogError(string message, Exception? exception = null, params object[] args);

    /// <summary>
    /// Logs a critical error message with optional exception.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="exception">Optional exception to log.</param>
    /// <param name="args">The message arguments.</param>
    void LogCritical(string message, Exception? exception = null, params object[] args);

    /// <summary>
    /// Logs a message with custom context properties.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The message template.</param>
    /// <param name="context">Optional dictionary of context properties.</param>
    void LogWithContext(LogLevel level, string message, Dictionary<string, object>? context = null);

    /// <summary>
    /// Gets the current correlation ID for request tracking.
    /// </summary>
    /// <returns>The correlation ID or an empty string if not set.</returns>
    string GetCorrelationId();

    /// <summary>
    /// Sets the correlation ID for the current context.
    /// </summary>
    /// <param name="correlationId">The correlation ID to set.</param>
    void SetCorrelationId(string correlationId);
}
