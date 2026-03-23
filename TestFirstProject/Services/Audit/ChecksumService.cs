using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TestFirstProject.Models;
using TestFirstProject.Services.Audit.Models;

namespace TestFirstProject.Services.Audit;

/// <summary>
/// Service for generating and verifying SHA256 checksums for audit log tamper detection.
/// </summary>
public class ChecksumService : IChecksumService
{
    private readonly string _secretKey;

    /// <summary>
    /// Initializes a new instance of the ChecksumService.
    /// </summary>
    /// <param name="options">Audit logging configuration options.</param>
    public ChecksumService(IOptions<AuditLoggingConfiguration> options)
    {
        _secretKey = options.Value.ChecksumSecretKey ?? "default-secret-key";
    }

    /// <inheritdoc />
    public string GenerateChecksum(AuditLog auditLog)
    {
        var dataToHash = BuildChecksumData(auditLog);
        return ComputeSha256Hash(dataToHash);
    }

    /// <inheritdoc />
    public bool VerifyChecksum(AuditLog auditLog)
    {
        var expectedChecksum = GenerateChecksum(auditLog);
        return string.Equals(auditLog.Checksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }

    private string BuildChecksumData(AuditLog auditLog)
    {
        var sb = new StringBuilder();
        sb.Append(auditLog.Timestamp.ToString("O"));
        sb.Append('|');
        sb.Append(auditLog.UserId);
        sb.Append('|');
        sb.Append(auditLog.Action);
        sb.Append('|');
        sb.Append(auditLog.EntityType);
        sb.Append('|');
        sb.Append(auditLog.EntityId);
        sb.Append('|');
        sb.Append(auditLog.OldValues ?? string.Empty);
        sb.Append('|');
        sb.Append(auditLog.NewValues ?? string.Empty);
        sb.Append('|');
        sb.Append(_secretKey);
        return sb.ToString();
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
