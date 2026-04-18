namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a refresh token stored in the database for secure token rotation.
    /// Revoked tokens cannot be reused; attempting to reuse a revoked token is a sign of token theft.
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The opaque refresh token string sent to the client.
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// Foreign key to the owning user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Navigation property to the owning user.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// UTC expiration timestamp. Expired tokens are rejected.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// UTC timestamp when this token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp when this token was revoked. Null means it is still active.
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Computed property: true if explicitly revoked.
        /// </summary>
        public bool IsRevoked => RevokedAt.HasValue;

        /// <summary>
        /// Computed property: true if the token has passed its expiration time.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Computed property: true if the token can still be used.
        /// </summary>
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
