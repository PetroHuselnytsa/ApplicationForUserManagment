namespace TestFirstProject.Exceptions
{
    /// <summary>
    /// Thrown when a user exceeds the allowed message sending rate.
    /// </summary>
    public class RateLimitExceededException : Exception
    {
        public RateLimitExceededException()
            : base("Rate limit exceeded. Maximum 30 messages per minute.") { }
        public RateLimitExceededException(string message) : base(message) { }
    }
}
