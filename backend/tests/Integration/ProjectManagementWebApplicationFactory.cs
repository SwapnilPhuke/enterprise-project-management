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
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove the real SQL Server DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Replace with in-memory database (unique per factory instance)
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase($"EPMTest_{Guid.NewGuid()}"));
        });

        // Inject required configuration for JWT
        builder.UseSetting("JwtSettings:SecretKey",
            "integration-test-secret-key-min-32-characters-ok");
        builder.UseSetting("JwtSettings:Issuer",   "ProjectManagementAPI");
        builder.UseSetting("JwtSettings:Audience", "ProjectManagementApp");
        builder.UseSetting("JwtSettings:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("JwtSettings:RefreshTokenExpirationDays",   "7");
    }
}
