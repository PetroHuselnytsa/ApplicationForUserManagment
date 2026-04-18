namespace TestFirstProject.Exceptions
{
    /// <summary>
    /// Base application exception with HTTP status code support.
    /// </summary>
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Thrown when a requested resource is not found.
    /// </summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base(message, 404) { }
    }

    /// <summary>
    /// Thrown when the user does not have permission to access a resource.
    /// </summary>
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message)
            : base(message, 403) { }
    }

    /// <summary>
    /// Thrown when input validation fails.
    /// </summary>
    public class ValidationException : AppException
    {
        public ValidationException(string message)
            : base(message, 422) { }
    }

    /// <summary>
    /// Thrown when a rate limit is exceeded.
    /// </summary>
    public class RateLimitException : AppException
    {
        public RateLimitException(string message)
            : base(message, 429) { }
    }
}
