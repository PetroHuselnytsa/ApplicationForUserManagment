namespace TestFirstProject.DTOs
{
    /// <summary>
    /// Structured error response returned by all auth endpoints.
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = null!;
        public IEnumerable<string>? Errors { get; set; }

        public ErrorResponse(int statusCode, string message, IEnumerable<string>? errors = null)
        {
            StatusCode = statusCode;
            Message = message;
            Errors = errors;
        }
    }
}
