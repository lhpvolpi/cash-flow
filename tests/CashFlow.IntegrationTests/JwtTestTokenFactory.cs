using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CashFlow.IntegrationTests;

internal static class JwtTestTokenFactory
{
    public static string GenerateValidToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(IntegrationTestFixture.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: IntegrationTestFixture.JwtIssuer,
            audience: IntegrationTestFixture.JwtAudience,
            claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, "test-user") },
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
