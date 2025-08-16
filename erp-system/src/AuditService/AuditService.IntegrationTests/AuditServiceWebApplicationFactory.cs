using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using AuditService.Infrastructure.Data;
using Testcontainers.MsSql;

namespace AuditService.IntegrationTests;

public class AuditServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithPassword("Test123!")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<AuditDbContext>));
            services.RemoveAll(typeof(AuditDbContext));

            // Add test database
            services.AddDbContext<AuditDbContext>(options =>
            {
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            // Build service provider and create database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
            dbContext.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
