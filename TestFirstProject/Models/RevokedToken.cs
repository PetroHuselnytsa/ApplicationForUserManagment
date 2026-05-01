namespace TestFirstProject.Models
{
    /// <summary>
    /// Stores revoked JWT IDs to support server-side token revocation (logout).
    /// </summary>
    public class RevokedToken
    {
        public Guid Id { get; set; }

        /// <summary>The unique JWT ID (jti claim) that has been revoked.</summary>
        public string Jti { get; set; } = null!;

        /// <summary>When the original token expires — used for cleanup queries.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>When this revocation was recorded.</summary>
        public DateTime RevokedAt { get; set; }
    }
}
