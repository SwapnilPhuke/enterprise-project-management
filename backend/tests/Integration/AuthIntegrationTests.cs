using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProjectManagement.Tests.Integration;

/// <summary>
/// Integration tests that exercise the full HTTP pipeline:
/// routing → middleware → controllers → services → in-memory database.
/// </summary>
public class AuthIntegrationTests : IClassFixture<ProjectManagementWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthIntegrationTests(ProjectManagementWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ──────────────────────────────────────────────────────────────
    // POST /api/v1/auth/register
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidPayload_Returns201()
    {
        var payload = new
        {
            username = "integrationuser",
            email    = "integration@epm.test",
            password = "SecurePass1!",
            fullName = "Integration Test"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_Returns400()
    {
        var payload = new
        {
            username = "duplicateuser",
            email    = "dup1@epm.test",
            password = "SecurePass1!",
            fullName = "Duplicate User"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        var duplicate = new
        {
            username = "duplicateuser",
            email    = "dup2@epm.test",
            password = "SecurePass1!",
            fullName = "Duplicate User"
        };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", duplicate);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ──────────────────────────────────────────────────────────────
    // POST /api/v1/auth/login
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        // Register first
        var register = new
        {
            username = "logintest",
            email    = "logintest@epm.test",
            password = "SecurePass1!",
            fullName = "Login Test"
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", register);

        // Log in
        var login = new { username = "logintest", password = "SecurePass1!" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", login);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(body.TryGetProperty("token", out var token));
        Assert.NotEmpty(token.GetString()!);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var login = new { username = "nonexistent", password = "WrongPass1!" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", login);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ──────────────────────────────────────────────────────────────
    // GET /api/v1/projects — requires authentication
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProjects_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ──────────────────────────────────────────────────────────────
    // GET /health — health endpoint always responds
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task HealthEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
