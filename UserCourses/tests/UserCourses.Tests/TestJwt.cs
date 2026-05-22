using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UserCourses.Tests;

public static class TestJwt
{
    public const string Issuer = "usercourses-tests";
    public const string Audience = "usercourses-tests";
    public const string Key = "test-only-signing-key-at-least-32-chars-long";

    // mintar ett test-JWT med samma sub- och role-claims som DevController
    public static string Create(Guid userId, string role)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("role", role)
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
