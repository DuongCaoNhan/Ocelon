using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Xunit;
using AuditService.Domain.Entities;
using AuditService.Domain.Enums;
using AuditService.Infrastructure.Data;
using AuditService.Application.DTOs;

namespace AuditService.IntegrationTests.Controllers;

public class AuditControllerTests : IClassFixture<AuditServiceWebApplicationFactory>
{
    private readonly AuditServiceWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuditControllerTests(AuditServiceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationSchemeHandler>(
                        "Test", options => { });
            });
        }).CreateClient();

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetAuditLogs_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/audit?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PagedResponseDto<AuditLogDto>>();
        content.Should().NotBeNull();
        content!.Data.Should().NotBeEmpty();
        content.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAuditLogs_WithUserIdFilter_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestDataAsync();
        var userId = "user123";

        // Act
        var response = await _client.GetAsync($"/api/audit?userId={userId}&page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PagedResponseDto<AuditLogDto>>();
        content.Should().NotBeNull();
        content!.Data.Should().OnlyContain(log => log.UserId == userId);
    }

    [Fact]
    public async Task GetAuditLogs_WithDateRangeFilter_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestDataAsync();
        var fromDate = DateTime.UtcNow.AddDays(-1);
        var toDate = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync(
            $"/api/audit?fromDate={fromDate:yyyy-MM-ddTHH:mm:ssZ}&toDate={toDate:yyyy-MM-ddTHH:mm:ssZ}&page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PagedResponseDto<AuditLogDto>>();
        content.Should().NotBeNull();
        content!.Data.Should().OnlyContain(log => 
            log.Timestamp >= fromDate && log.Timestamp <= toDate);
    }

    [Fact]
    public async Task GetAuditLogs_WithActionFilter_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestDataAsync();
        var action = "CreateUser";

        // Act
        var response = await _client.GetAsync($"/api/audit?action={action}&page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PagedResponseDto<AuditLogDto>>();
        content.Should().NotBeNull();
        content!.Data.Should().OnlyContain(log => log.Action == action);
    }

    [Fact]
    public async Task GetAuditLogs_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/audit?page=2&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PagedResponseDto<AuditLogDto>>();
        content.Should().NotBeNull();
        content!.Pagination.Page.Should().Be(2);
        content.Pagination.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetAuditLogs_WithUnauthorizedUser_ReturnsUnauthorized()
    {
        // Arrange
        var unauthorizedClient = _factory.CreateClient();

        // Act
        var response = await unauthorizedClient.GetAsync("/api/audit");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        // Clear existing data
        dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs);
        await dbContext.SaveChangesAsync();

        // Add test data
        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "user123",
                Action = "CreateUser",
                ServiceName = "HRService",
                Resource = "User/123",
                Result = AuditResult.Success,
                Severity = AuditSeverity.Medium,
                Timestamp = DateTime.UtcNow.AddHours(-2),
                IpAddress = "192.168.1.1",
                UserAgent = "Test Agent",
                Details = "{\"userId\": \"123\", \"userName\": \"testuser\"}"
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "user456",
                Action = "UpdateUser",
                ServiceName = "HRService",
                Resource = "User/456",
                Result = AuditResult.Success,
                Severity = AuditSeverity.Low,
                Timestamp = DateTime.UtcNow.AddHours(-1),
                IpAddress = "192.168.1.2",
                UserAgent = "Test Agent",
                Details = "{\"userId\": \"456\", \"changes\": [\"email\"]}"
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "user123",
                Action = "DeleteUser",
                ServiceName = "HRService",
                Resource = "User/789",
                Result = AuditResult.Failure,
                Severity = AuditSeverity.High,
                Timestamp = DateTime.UtcNow.AddMinutes(-30),
                IpAddress = "192.168.1.1",
                UserAgent = "Test Agent",
                Details = "{\"userId\": \"789\", \"error\": \"Unauthorized\"}"
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "user789",
                Action = "CreateUser",
                ServiceName = "AccountingService",
                Resource = "Account/101",
                Result = AuditResult.Success,
                Severity = AuditSeverity.Medium,
                Timestamp = DateTime.UtcNow.AddDays(-2),
                IpAddress = "192.168.1.3",
                UserAgent = "Test Agent",
                Details = "{\"accountId\": \"101\", \"accountType\": \"Assets\"}"
            }
        };

        dbContext.AuditLogs.AddRange(auditLogs);
        await dbContext.SaveChangesAsync();
    }
}

public class TestAuthenticationSchemeHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
#pragma warning disable CS0618 // Type or member is obsolete
    public TestAuthenticationSchemeHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
#pragma warning restore CS0618 // Type or member is obsolete
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("roles", "AuditReader")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
