using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestFirstProject.Contexts;
using Xunit;

namespace TestFirstProject.Tests.Integration;

public class AuditEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string AdminKey = "test-admin-key-change-in-production";

    public AuditEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<AuditContext>) ||
                         d.ServiceType == typeof(DbContextOptions<PersonsContext>))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AuditContext>(options =>
                    options.UseInMemoryDatabase("TestAuditDb"));

                services.AddDbContext<PersonsContext>(options =>
                    options.UseInMemoryDatabase("TestPersonsDb"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SearchAuditLogs_NoAdminKey_Returns403()
    {
        var response = await _client.GetAsync("/api/audit/search");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SearchAuditLogs_ValidAdminKey_ReturnsOk()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var response = await _client.GetAsync("/api/audit/search");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchAuditLogs_WithFilters_ReturnsOk()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var response = await _client.GetAsync("/api/audit/search?userId=test&entityType=Person&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEntityHistory_ValidAdminKey_ReturnsOk()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var response = await _client.GetAsync("/api/audit/entity/Person/123/history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportAuditLogs_CsvFormat_ReturnsCsv()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var response = await _client.GetAsync("/api/audit/export?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
    }

    [Fact]
    public async Task ExportAuditLogs_JsonFormat_ReturnsJson()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var response = await _client.GetAsync("/api/audit/export?format=json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task VerifyIntegrity_ReturnsResult()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);
        var id = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/audit/{id}/verify");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("isValid");
    }

    [Fact]
    public async Task SearchLogs_NoAdminKey_Returns403()
    {
        var response = await _client.GetAsync("/api/logs/search");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SearchLogs_ValidAdminKey_ReturnsOk()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var response = await _client.GetAsync("/api/logs/search");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetErrorRate_ValidAdminKey_ReturnsOk()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var fromDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await _client.GetAsync($"/api/logs/analytics/error-rate?fromDate={fromDate}&toDate={toDate}&interval=day");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRequestVolume_ValidAdminKey_ReturnsOk()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var fromDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await _client.GetAsync($"/api/logs/analytics/request-volume?fromDate={fromDate}&toDate={toDate}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetResponseTimes_ValidAdminKey_ReturnsOk()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var fromDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await _client.GetAsync($"/api/logs/analytics/response-times?fromDate={fromDate}&toDate={toDate}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetErrorRate_InvalidInterval_ReturnsBadRequest()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var fromDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await _client.GetAsync($"/api/logs/analytics/error-rate?fromDate={fromDate}&toDate={toDate}&interval=invalid");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UsersEndpoint_NoAdminKey_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
