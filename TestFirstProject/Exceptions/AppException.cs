namespace TestFirstProject.Exceptions
{
    /// <summary>
    /// Base exception for application-level errors with HTTP status codes.
    /// </summary>
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404) { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message) : base(message, 403) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base(message, 401) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(message, 409) { }
    }

    public class RateLimitExceededException : AppException
    {
        public RateLimitExceededException(string message = "Rate limit exceeded. Please try again later.")
            : base(message, 429) { }
    }

    public class ValidationException : AppException
    {
        public ValidationException(string message) : base(message, 422) { }
    }
}
