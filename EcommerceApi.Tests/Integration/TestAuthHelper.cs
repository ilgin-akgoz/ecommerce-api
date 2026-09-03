using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceApi.Tests.Integration;

/// Generates JWTs for integration tests, matching the Jwt:* settings configured
/// via builder.UseSetting(...) in CustomWebApplicationFactory. If those settings
/// ever change, update the constants below to match.
public static class TestAuthHelper
{
    private const string TestSigningKey = "test-only-signing-key-not-used-anywhere-real-32chars+";
    private const string TestIssuer = "EcommerceApi";
    private const string TestAudience = "EcommerceApiClients";

    
    /// Builds a signed JWT for a fake test user with the given role(s).
    /// Pass no roles for a token that's authenticated but has no specific role
    public static string GenerateToken(
        string email = "testuser@example.com",
        string fullName = "Test User",
        params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// Shortcut for a token with the "Admin" role.
    public static string GenerateAdminToken(string email = "admin@example.com") =>
        GenerateToken(email, "Test Admin", "Admin");

    /// Shortcut for a token with the "Customer" role.
    public static string GenerateCustomerToken(string email = "customer@example.com") =>
        GenerateToken(email, "Test Customer", "Customer");

    /// Attaches a Bearer token to an HttpClient's default request headers,
    /// so every subsequent request from this client is authenticated.
    public static void AuthenticateAs(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
