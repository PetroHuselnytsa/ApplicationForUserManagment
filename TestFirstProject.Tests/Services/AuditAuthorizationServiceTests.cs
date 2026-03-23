using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using TestFirstProject.Services.Authorization;
using Xunit;

namespace TestFirstProject.Tests.Services;

public class AuditAuthorizationServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private const string AdminKey = "test-admin-key";

    public AuditAuthorizationServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x["Audit:AdminKey"]).Returns(AdminKey);
    }

    [Fact]
    public void CanViewAuditLogs_ValidAdminKey_ReturnsTrue()
    {
        var context = CreateHttpContext(AdminKey);
        var service = new AuditAuthorizationService(_mockConfiguration.Object);

        var result = service.CanViewAuditLogs(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanViewAuditLogs_InvalidAdminKey_ReturnsFalse()
    {
        var context = CreateHttpContext("invalid-key");
        var service = new AuditAuthorizationService(_mockConfiguration.Object);

        var result = service.CanViewAuditLogs(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanViewAuditLogs_NoAdminKey_ReturnsFalse()
    {
        var context = new DefaultHttpContext();
        var service = new AuditAuthorizationService(_mockConfiguration.Object);

        var result = service.CanViewAuditLogs(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanExportAuditLogs_ValidAdminKey_ReturnsTrue()
    {
        var context = CreateHttpContext(AdminKey);
        var service = new AuditAuthorizationService(_mockConfiguration.Object);

        var result = service.CanExportAuditLogs(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanExportAuditLogs_InvalidAdminKey_ReturnsFalse()
    {
        var context = CreateHttpContext("invalid-key");
        var service = new AuditAuthorizationService(_mockConfiguration.Object);

        var result = service.CanExportAuditLogs(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanViewAllUsers_ValidAdminKey_ReturnsTrue()
    {
        var context = CreateHttpContext(AdminKey);
        var service = new AuditAuthorizationService(_mockConfiguration.Object);

        var result = service.CanViewAllUsers(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanViewAllUsers_InvalidAdminKey_ReturnsFalse()
    {
        var context = CreateHttpContext("invalid-key");
        var service = new AuditAuthorizationService(_mockConfiguration.Object);

        var result = service.CanViewAllUsers(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_NullConfiguration_UsesDefaultKey()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["Audit:AdminKey"]).Returns((string?)null);

        var service = new AuditAuthorizationService(mockConfig.Object);
        var context = CreateHttpContext("default-admin-key-change-me");

        var result = service.CanViewAuditLogs(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanViewAuditLogs_EmptyAdminKey_ReturnsFalse()
    {
        var context = CreateHttpContext("");
        var service = new AuditAuthorizationService(_mockConfiguration.Object);

        var result = service.CanViewAuditLogs(context);

        result.Should().BeFalse();
    }

    private static DefaultHttpContext CreateHttpContext(string adminKey)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Admin-Key"] = adminKey;
        return context;
    }
}
