namespace TestFirstProject.Exceptions
{
    /// <summary>
    /// Thrown when the client sends invalid or malformed input.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}
