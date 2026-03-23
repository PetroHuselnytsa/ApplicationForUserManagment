using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using TestFirstProject.Contexts;
using TestFirstProject.Models;
using TestFirstProject.Services.Audit;
using TestFirstProject.Services.Audit.Models;
using TestFirstProject.Services.Logging;
using Xunit;

namespace TestFirstProject.Tests.Services;

public class AuditServiceTests : IDisposable
{
    private readonly AuditContext _context;
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly Mock<IChecksumService> _mockChecksumService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly IAuditService _auditService;

    public AuditServiceTests()
    {
        var options = new DbContextOptionsBuilder<AuditContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestAuditContext(options);

        _mockLoggingService = new Mock<ILoggingService>();
        _mockLoggingService.Setup(x => x.GetCorrelationId()).Returns(Guid.NewGuid().ToString());

        _mockChecksumService = new Mock<IChecksumService>();
        _mockChecksumService.Setup(x => x.GenerateChecksum(It.IsAny<AuditLog>()))
            .Returns("test-checksum-12345");
        _mockChecksumService.Setup(x => x.VerifyChecksum(It.IsAny<AuditLog>()))
            .Returns(true);

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Test");

        var config = Options.Create(new AuditLoggingConfiguration
        {
            EnableAuditLogging = true,
            RetentionDays = 365,
            ChecksumSecretKey = "test-secret"
        });

        _auditService = new AuditService(
            _context,
            _mockLoggingService.Object,
            _mockChecksumService.Object,
            _mockHttpContextAccessor.Object,
            config,
            _mockEnvironment.Object);
    }

    [Fact]
    public async Task LogCreateAsync_ShouldCreateAuditRecord()
    {
        var entity = new TestEntity { Id = "1", Name = "Test" };

        await _auditService.LogCreateAsync("user-1", "Test User", entity);

        var logs = await _context.AuditLogs.ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].Action.Should().Be("CREATE");
        logs[0].EntityType.Should().Be("TestEntity");
        logs[0].UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task LogUpdateAsync_ShouldCaptureOldAndNewValues()
    {
        var oldEntity = new TestEntity { Id = "1", Name = "Old Name" };
        var newEntity = new TestEntity { Id = "1", Name = "New Name" };

        await _auditService.LogUpdateAsync("user-1", "Test User", oldEntity, newEntity);

        var log = await _context.AuditLogs.FirstAsync();
        log.Action.Should().Be("UPDATE");
        log.OldValues.Should().Contain("Old Name");
        log.NewValues.Should().Contain("New Name");
    }

    [Fact]
    public async Task LogDeleteAsync_ShouldCaptureDeletedEntity()
    {
        var entity = new TestEntity { Id = "1", Name = "Deleted Entity" };

        await _auditService.LogDeleteAsync("user-1", "Test User", entity);

        var log = await _context.AuditLogs.FirstAsync();
        log.Action.Should().Be("DELETE");
        log.OldValues.Should().Contain("Deleted Entity");
    }

    [Fact]
    public async Task LogReadAsync_ShouldCreateReadAuditRecord()
    {
        await _auditService.LogReadAsync("user-1", "Test User", "TestEntity", "123");

        var log = await _context.AuditLogs.FirstAsync();
        log.Action.Should().Be("READ");
        log.EntityType.Should().Be("TestEntity");
        log.EntityId.Should().Be("123");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByUserId()
    {
        await SeedTestData();

        var results = await _auditService.SearchAsync(new AuditLogSearchRequest { UserId = "user-1" });

        results.Items.Should().OnlyContain(x => x.UserId == "user-1");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByDateRange()
    {
        await SeedTestData();

        var results = await _auditService.SearchAsync(new AuditLogSearchRequest
        {
            FromDate = DateTime.UtcNow.AddDays(-1),
            ToDate = DateTime.UtcNow.AddDays(1)
        });

        results.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByEntityType()
    {
        await SeedTestData();

        var results = await _auditService.SearchAsync(new AuditLogSearchRequest { EntityType = "TestEntity" });

        results.Items.Should().OnlyContain(x => x.EntityType == "TestEntity");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByAction()
    {
        await SeedTestData();

        var results = await _auditService.SearchAsync(new AuditLogSearchRequest { Action = "CREATE" });

        results.Items.Should().OnlyContain(x => x.Action == "CREATE");
    }

    [Fact]
    public async Task SearchAsync_ShouldPaginateResults()
    {
        await SeedTestData(count: 25);

        var results = await _auditService.SearchAsync(new AuditLogSearchRequest { Page = 2, PageSize = 10 });

        results.Items.Should().HaveCount(10);
        results.TotalCount.Should().Be(25);
        results.Page.Should().Be(2);
        results.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetEntityHistoryAsync_ShouldReturnAllChanges()
    {
        await _auditService.LogCreateAsync("user-1", "Test User", new TestEntity { Id = "1", Name = "Initial" });
        await _auditService.LogUpdateAsync("user-1", "Test User",
            new TestEntity { Id = "1", Name = "Initial" },
            new TestEntity { Id = "1", Name = "Updated" });

        var history = await _auditService.GetEntityHistoryAsync("TestEntity", "1");

        history.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExportAsync_CsvFormat_ShouldGenerateValidCsv()
    {
        await SeedTestData();

        var stream = await _auditService.ExportAsync(new AuditLogSearchRequest(), ExportFormat.Csv);

        using var reader = new StreamReader(stream);
        var csv = await reader.ReadToEndAsync();
        csv.Should().Contain("Id,Timestamp,UserId,UserName,Action,EntityType,EntityId");
    }

    [Fact]
    public async Task ExportAsync_JsonFormat_ShouldGenerateValidJson()
    {
        await SeedTestData();

        var stream = await _auditService.ExportAsync(new AuditLogSearchRequest(), ExportFormat.Json);

        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        json.Should().StartWith("[");
        json.Should().EndWith("]");
    }

    [Fact]
    public async Task VerifyIntegrityAsync_ValidChecksum_ShouldReturnTrue()
    {
        await _auditService.LogCreateAsync("user-1", "Test User", new TestEntity { Id = "1", Name = "Test" });
        var log = await _context.AuditLogs.FirstAsync();

        var result = await _auditService.VerifyIntegrityAsync(log.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyIntegrityAsync_TamperedRecord_ShouldReturnFalse()
    {
        _mockChecksumService.Setup(x => x.VerifyChecksum(It.IsAny<AuditLog>()))
            .Returns(false);

        await _auditService.LogCreateAsync("user-1", "Test User", new TestEntity { Id = "1", Name = "Test" });
        var log = await _context.AuditLogs.FirstAsync();

        var result = await _auditService.VerifyIntegrityAsync(log.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyIntegrityAsync_NonExistentRecord_ShouldReturnFalse()
    {
        var result = await _auditService.VerifyIntegrityAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    private async Task SeedTestData(int count = 5)
    {
        for (int i = 0; i < count; i++)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                UserId = i % 2 == 0 ? "user-1" : "user-2",
                UserName = i % 2 == 0 ? "User One" : "User Two",
                Action = "CREATE",
                EntityType = "TestEntity",
                EntityId = i.ToString(),
                IpAddress = "127.0.0.1",
                CorrelationId = Guid.NewGuid().ToString(),
                Environment = "Test",
                Checksum = "test-checksum"
            });
        }
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private class TestEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private class TestAuditContext : AuditContext
    {
        private readonly DbContextOptions<AuditContext> _options;

        public TestAuditContext(DbContextOptions<AuditContext> options) : base()
        {
            _options = options;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
        }
    }
}
