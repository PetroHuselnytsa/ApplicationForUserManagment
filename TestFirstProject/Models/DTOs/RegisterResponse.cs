namespace TestFirstProject.Models.DTOs
{
    /// <summary>
    /// Response returned on successful user registration.
    /// </summary>
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool EmailVerificationRequired { get; set; }
    }
}
