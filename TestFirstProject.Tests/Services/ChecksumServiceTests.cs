using FluentAssertions;
using Microsoft.Extensions.Options;
using TestFirstProject.Models;
using TestFirstProject.Services.Audit;
using TestFirstProject.Services.Audit.Models;
using Xunit;

namespace TestFirstProject.Tests.Services;

public class ChecksumServiceTests
{
    private readonly IChecksumService _service;

    public ChecksumServiceTests()
    {
        var config = Options.Create(new AuditLoggingConfiguration
        {
            ChecksumSecretKey = "test-secret-key"
        });
        _service = new ChecksumService(config);
    }

    [Fact]
    public void GenerateChecksum_ShouldProduceConsistentHash()
    {
        var auditLog = CreateTestAuditLog();

        var hash1 = _service.GenerateChecksum(auditLog);
        var hash2 = _service.GenerateChecksum(auditLog);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GenerateChecksum_DifferentInputs_ShouldProduceDifferentHashes()
    {
        var auditLog1 = CreateTestAuditLog();
        var auditLog2 = CreateTestAuditLog();
        auditLog2.UserId = "different-user";

        var hash1 = _service.GenerateChecksum(auditLog1);
        var hash2 = _service.GenerateChecksum(auditLog2);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyChecksum_ValidChecksum_ShouldReturnTrue()
    {
        var auditLog = CreateTestAuditLog();
        auditLog.Checksum = _service.GenerateChecksum(auditLog);

        var result = _service.VerifyChecksum(auditLog);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyChecksum_ModifiedData_ShouldReturnFalse()
    {
        var auditLog = CreateTestAuditLog();
        auditLog.Checksum = _service.GenerateChecksum(auditLog);

        auditLog.NewValues = "tampered data";

        var result = _service.VerifyChecksum(auditLog);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyChecksum_InvalidChecksum_ShouldReturnFalse()
    {
        var auditLog = CreateTestAuditLog();
        auditLog.Checksum = "invalid-checksum";

        var result = _service.VerifyChecksum(auditLog);

        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateChecksum_ShouldProduceHexString()
    {
        var auditLog = CreateTestAuditLog();

        var checksum = _service.GenerateChecksum(auditLog);

        checksum.Should().MatchRegex("^[a-f0-9]{64}$");
    }

    [Fact]
    public void GenerateChecksum_NullOptionalFields_ShouldStillWork()
    {
        var auditLog = CreateTestAuditLog();
        auditLog.OldValues = null;
        auditLog.NewValues = null;

        var checksum = _service.GenerateChecksum(auditLog);

        checksum.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateChecksum_DifferentTimestamps_ShouldProduceDifferentHashes()
    {
        var auditLog1 = CreateTestAuditLog();
        var auditLog2 = CreateTestAuditLog();
        auditLog2.Timestamp = auditLog1.Timestamp.AddSeconds(1);

        var hash1 = _service.GenerateChecksum(auditLog1);
        var hash2 = _service.GenerateChecksum(auditLog2);

        hash1.Should().NotBe(hash2);
    }

    private static AuditLog CreateTestAuditLog()
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UserId = "test-user",
            UserName = "Test User",
            Action = "CREATE",
            EntityType = "TestEntity",
            EntityId = "123",
            OldValues = null,
            NewValues = """{"name":"test"}""",
            IpAddress = "127.0.0.1",
            CorrelationId = "correlation-123",
            Environment = "Test",
            Checksum = string.Empty
        };
    }
}
