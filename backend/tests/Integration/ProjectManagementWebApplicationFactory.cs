using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagement.Data;

namespace ProjectManagement.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that replaces SQL Server with an in-memory database
/// for fast, isolated integration tests.
/// </summary>
public class ProjectManagementWebApplicationFactory : WebApplicationFactory<Program>
{
    // A single, shared in-memory service provider that owns only the InMemory EF Core
    // services. Passed via UseInternalServiceProvider so EF Core never looks at the
    // application's DI container for its internal services, which would find both the
    // SqlServer services (registered by Program.cs) and the InMemory services, causing
    // the "two providers registered" InvalidOperationException.
    private static readonly IServiceProvider _inMemoryEfProvider =
        new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove the real SQL Server DbContext options
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Replace with an in-memory database that uses an isolated internal
            // service provider containing ONLY InMemory EF Core services.
            // Using UseInternalServiceProvider bypasses the application's DI container
            // for EF Core internals, avoiding the "two providers" conflict.
            // dbName is captured outside the lambda so all DbContext instances within
            // this factory share the SAME in-memory store (data persists across requests).
            var dbName = $"EPMTest_{Guid.NewGuid()}";
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName)
                       .UseInternalServiceProvider(_inMemoryEfProvider));
        });

        // Inject required configuration for JWT — must set before the host reads config
        builder.UseSetting("JwtSettings:SecretKey",
            "integration-test-secret-key-min-32-characters-ok");
        builder.UseSetting("JwtSettings:Issuer",   "ProjectManagementAPI");
        builder.UseSetting("JwtSettings:Audience", "ProjectManagementApp");
        builder.UseSetting("JwtSettings:ExpirationMinutes", "15");
        builder.UseSetting("JwtSettings:RefreshTokenExpireDays",   "7");
    }
}
