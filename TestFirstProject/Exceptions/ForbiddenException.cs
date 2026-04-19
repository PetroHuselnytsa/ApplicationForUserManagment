namespace TestFirstProject.Exceptions
{
    /// <summary>
    /// Thrown when a user attempts to access a resource they do not own.
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
        public ForbiddenException()
            : base("You do not have permission to access this resource.") { }
    }
}
