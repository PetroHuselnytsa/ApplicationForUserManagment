namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents an application user with authentication credentials and account status.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }

        /// <summary>
        /// User's email address, used as the unique login identifier.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// BCrypt-hashed password. Never stored in plaintext.
        /// </summary>
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// Indicates whether the user has confirmed their email address.
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Email verification token sent during registration.
        /// </summary>
        public string? EmailVerificationToken { get; set; }

        /// <summary>
        /// Expiration time for the email verification token.
        /// </summary>
        public DateTime? EmailVerificationTokenExpiry { get; set; }

        /// <summary>
        /// UTC timestamp until which the account is locked out due to failed login attempts.
        /// Null means the account is not locked.
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// Count of consecutive failed login attempts. Resets on successful login.
        /// </summary>
        public int AccessFailedCount { get; set; }

        /// <summary>
        /// UTC timestamp of account creation.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp of last account update.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
