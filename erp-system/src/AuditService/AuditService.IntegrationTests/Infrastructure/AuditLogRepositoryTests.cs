using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;
using AuditService.Domain.Entities;
using AuditService.Domain.Enums;
using AuditService.Infrastructure.Data;
using AuditService.Infrastructure.Repositories;

namespace AuditService.IntegrationTests.Infrastructure;

public class AuditLogRepositoryTests : IClassFixture<AuditServiceWebApplicationFactory>
{
    private readonly AuditServiceWebApplicationFactory _factory;

    public AuditLogRepositoryTests(AuditServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddAsync_WithValidAuditLog_ShouldPersist()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var repository = new AuditLogRepository(dbContext);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = "user123",
            Action = "CreateUser",
            ServiceName = "HRService",
            Resource = "User/123",
            Result = AuditResult.Success,
            Severity = AuditSeverity.Medium,
            Timestamp = DateTime.UtcNow,
            IpAddress = "192.168.1.1",
            UserAgent = "Test Agent",
            Details = "{\"userId\": \"123\"}"
        };

        // Act
        await repository.AddAsync(auditLog);

        // Assert
        var savedLog = await dbContext.AuditLogs.FindAsync(auditLog.Id);
        savedLog.Should().NotBeNull();
        savedLog!.UserId.Should().Be(auditLog.UserId);
        savedLog.Action.Should().Be(auditLog.Action);
        savedLog.ServiceName.Should().Be(auditLog.ServiceName);
    }

    [Fact]
    public async Task GetPagedAsync_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var repository = new AuditLogRepository(dbContext);

        await SeedTestDataAsync(dbContext);

        // Act
        var result = await repository.GetPagedAsync(
            userId: "user123",
            action: null,
            fromDate: null,
            toDate: null,
            page: 1,
            pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(log => log.UserId == "user123");
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPagedAsync_WithDateRange_ShouldReturnFilteredResults()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var repository = new AuditLogRepository(dbContext);

        await SeedTestDataAsync(dbContext);

        var fromDate = DateTime.UtcNow.AddHours(-1);
        var toDate = DateTime.UtcNow;

        // Act
        var result = await repository.GetPagedAsync(
            userId: null,
            action: null,
            fromDate: fromDate,
            toDate: toDate,
            page: 1,
            pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(log => 
            log.Timestamp >= fromDate && log.Timestamp <= toDate);
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var repository = new AuditLogRepository(dbContext);

        await SeedTestDataAsync(dbContext);

        // Act
        var result = await repository.GetPagedAsync(
            userId: null,
            action: null,
            fromDate: null,
            toDate: null,
            page: 2,
            pageSize: 2);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnAuditLog()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var repository = new AuditLogRepository(dbContext);

        var auditLogId = await SeedSingleAuditLogAsync(dbContext);

        // Act
        var result = await repository.GetByIdAsync(auditLogId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(auditLogId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var repository = new AuditLogRepository(dbContext);

        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task BulkAddAsync_WithMultipleAuditLogs_ShouldPersistAll()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var repository = new AuditLogRepository(dbContext);

        var auditLogs = new List<AuditLog>
        {
            CreateTestAuditLog("user1", "Action1"),
            CreateTestAuditLog("user2", "Action2"),
            CreateTestAuditLog("user3", "Action3")
        };

        // Act
        await repository.BulkAddAsync(auditLogs);

        // Assert
        var savedLogs = await dbContext.AuditLogs
            .Where(log => auditLogs.Select(al => al.Id).Contains(log.Id))
            .ToListAsync();
        
        savedLogs.Should().HaveCount(3);
    }

    private static async Task SeedTestDataAsync(AuditDbContext dbContext)
    {
        // Clear existing data
        dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs);
        await dbContext.SaveChangesAsync();

        var auditLogs = new List<AuditLog>
        {
            CreateTestAuditLog("user123", "CreateUser", DateTime.UtcNow.AddHours(-2)),
            CreateTestAuditLog("user456", "UpdateUser", DateTime.UtcNow.AddHours(-1)),
            CreateTestAuditLog("user123", "DeleteUser", DateTime.UtcNow.AddMinutes(-30)),
            CreateTestAuditLog("user789", "CreateUser", DateTime.UtcNow.AddDays(-2)),
            CreateTestAuditLog("user123", "ViewUser", DateTime.UtcNow.AddMinutes(-10))
        };

        dbContext.AuditLogs.AddRange(auditLogs);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<Guid> SeedSingleAuditLogAsync(AuditDbContext dbContext)
    {
        var auditLog = CreateTestAuditLog("testuser", "TestAction");
        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync();
        return auditLog.Id;
    }

    private static AuditLog CreateTestAuditLog(string userId, string action, DateTime? timestamp = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            ServiceName = "TestService",
            Resource = $"Resource/{Guid.NewGuid()}",
            Result = AuditResult.Success,
            Severity = AuditSeverity.Medium,
            Timestamp = timestamp ?? DateTime.UtcNow,
            IpAddress = "192.168.1.1",
            UserAgent = "Test Agent",
            Details = $"{{\"test\": \"data for {action}\"}}"
        };
    }
}
