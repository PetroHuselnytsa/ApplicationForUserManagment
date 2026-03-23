using TestFirstProject.Models;

namespace TestFirstProject.Services.Audit;

/// <summary>
/// Interface for generating and verifying checksums for tamper detection.
/// </summary>
public interface IChecksumService
{
    /// <summary>
    /// Generates a SHA256 checksum for an audit log entry.
    /// </summary>
    /// <param name="auditLog">The audit log entry to generate a checksum for.</param>
    /// <returns>The hexadecimal checksum string.</returns>
    string GenerateChecksum(AuditLog auditLog);

    /// <summary>
    /// Verifies the integrity of an audit log entry by comparing its checksum.
    /// </summary>
    /// <param name="auditLog">The audit log entry to verify.</param>
    /// <returns>True if the checksum is valid, false if tampered.</returns>
    bool VerifyChecksum(AuditLog auditLog);
}
