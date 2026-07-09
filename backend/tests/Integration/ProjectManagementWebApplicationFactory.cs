using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagement.Data;

namespace ProjectManagement.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that replaces SQL Server with an in-memory database
/// for fast, isolated integration tests.
/// </summary>
public class ProjectManagementWebApplicationFactory : WebApplicationFactory<Program>
{
    // Generated once per factory instance so all requests within a test class
    // share the same in-memory database, while different factory instances are isolated.
    private readonly string _dbName = $"EPMTest_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove the real SQL Server DbContext options descriptor.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // In EF Core 9, AddDbContext also registers IDbContextOptionsConfiguration<TContext>
            // which holds the SQL Server configuration action. If left in place it is applied
            // on top of the InMemory options registered below, causing the error:
            // "Services for database providers 'SqlServer', 'InMemory' have been registered".
            var configDescriptors = services
                .Where(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
                .ToList();
            foreach (var d in configDescriptors)
                services.Remove(d);

            // Replace with in-memory database.
            // Singleton options lifetime means the lambda is called only once, so the
            // InMemoryDatabaseRoot created lazily by EF Core is shared across all request
            // scopes — data written in one request is visible in subsequent requests.
            services.AddDbContext<AppDbContext>(
                options => options.UseInMemoryDatabase(_dbName),
                optionsLifetime: ServiceLifetime.Singleton);
        });

        // Inject required configuration for JWT — must be set before the host reads config
        builder.UseSetting("JwtSettings:SecretKey",
            "integration-test-secret-key-min-32-characters-ok");
        builder.UseSetting("JwtSettings:Issuer",   "ProjectManagementAPI");
        builder.UseSetting("JwtSettings:Audience", "ProjectManagementApp");
        builder.UseSetting("JwtSettings:ExpirationMinutes", "15");
        builder.UseSetting("JwtSettings:RefreshTokenExpireDays",   "7");
    }
}
