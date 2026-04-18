namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents an authenticated user in the system.
    /// </summary>
    public class User
    {
        protected User() { }

        public User(string email, string passwordHash, string name)
        {
            Email = email;
            PasswordHash = passwordHash;
            Name = name;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Lockout fields for brute-force protection
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEndAt { get; set; }

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
