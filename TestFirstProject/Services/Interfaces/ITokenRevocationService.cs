namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Manages server-side JWT revocation by persisting and querying revoked token JTIs.
    /// </summary>
    public interface ITokenRevocationService
    {
        /// <summary>Revokes a token by storing its JTI in the database.</summary>
        Task RevokeAsync(string jti, DateTime expiry);

        /// <summary>Returns true if the given JTI has been revoked.</summary>
        Task<bool> IsRevokedAsync(string jti);
    }
}
