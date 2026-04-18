namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents an application user with authentication and authorization properties.
    /// </summary>
    public class User
    {
        protected User() { }

        public User(string email, string passwordHash, string name)
        {
            Email = email;
            PasswordHash = passwordHash;
            Name = name;
        }

        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Role Role { get; set; } = Role.User;
        public bool IsPremium { get; set; } = false;
        public bool IsEmailVerified { get; set; } = false;
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    }
}
