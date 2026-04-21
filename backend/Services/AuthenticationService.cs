using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;

namespace ProjectManagement.Services;

public interface IAuthenticationService
{
    Task<AuthResult> RegisterAsync(string username, string email, string password, string? fullName = null);
    Task<AuthResult> LoginAsync(string username, string password);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(int userId);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByUsernameAsync(string username);
}

/// <summary>
/// Authentication service implementing JWT token generation and BCrypt password hashing.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(AppDbContext context, IConfiguration configuration, ILogger<AuthenticationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password, string? fullName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
                return new AuthResult { Success = false, Message = "Username must be at least 3 characters" };

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return new AuthResult { Success = false, Message = "Password must be at least 6 characters" };

            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
                return new AuthResult { Success = false, Message = "Username or email already exists" };

            var user = new User
            {
                Username     = username,
                Email        = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName     = fullName,
                CreatedAt    = DateTime.UtcNow,
                IsActive     = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered: {Username}", username);
            return new AuthResult { Success = true, Message = "User registered successfully", UserId = user.Id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Username}", username);
            return new AuthResult { Success = false, Message = "An error occurred during registration" };
        }
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for unknown user: {Username}", username);
                return new AuthResult { Success = false, Message = "Invalid username or password" };
            }

            if (!user.IsActive)
                return new AuthResult { Success = false, Message = "User account is inactive" };

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", username);
                return new AuthResult { Success = false, Message = "Invalid username or password" };
            }

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var accessToken   = GenerateJwtToken(user);
            var refreshToken  = await CreateRefreshTokenAsync(user.Id);

            _logger.LogInformation("User logged in successfully: {Username}", username);

            return new AuthResult
            {
                Success      = true,
                Message      = "Login successful",
                Token        = accessToken,
                RefreshToken = refreshToken,
                UserId       = user.Id,
                Username     = user.Username,
                Role         = user.Role
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", username);
            return new AuthResult { Success = false, Message = "An error occurred during login" };
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }


    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (stored is null || !stored.IsActive)
            return new AuthResult { Success = false, Message = "Invalid or expired refresh token" };

        stored.IsRevoked = true;
        var newRefreshToken = await CreateRefreshTokenAsync(stored.UserId);
        var newAccessToken  = GenerateJwtToken(stored.User);

        return new AuthResult
        {
            Success      = true,
            Message      = "Token refreshed",
            Token        = newAccessToken,
            RefreshToken = newRefreshToken,
            UserId       = stored.UserId,
            Username     = stored.User.Username,
            Role         = stored.User.Role
        };
    }

    public async Task RevokeRefreshTokenAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync();

        foreach (var t in tokens)
            t.IsRevoked = true;

        await _context.SaveChangesAsync();
    }

    private async Task<string> CreateRefreshTokenAsync(int userId)
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        var token = Convert.ToBase64String(bytes);

        var jwtSettings   = _configuration.GetSection("JwtSettings");
        var expireDays    = int.Parse(jwtSettings["RefreshTokenExpireDays"] ?? "7");

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token     = token,
            UserId    = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expireDays)
        });
        await _context.SaveChangesAsync();
        return token;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "ProjectManagementAPI";
        var audience = jwtSettings["Audience"] ?? "ProjectManagementApp";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim("FullName",                user.FullName ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>Result class for authentication operations.</summary>
public class AuthResult
{
    public bool    Success      { get; set; }
    public string  Message      { get; set; } = string.Empty;
    public string? Token        { get; set; }
    public string? RefreshToken { get; set; }
    public int?    UserId       { get; set; }
    public string? Username     { get; set; }
    public string? Role         { get; set; }
}
