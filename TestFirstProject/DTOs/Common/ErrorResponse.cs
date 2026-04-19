namespace TestFirstProject.DTOs.Common
{
    /// <summary>
    /// Structured error response returned by all API endpoints on failure.
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = null!;
        public string? Detail { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
