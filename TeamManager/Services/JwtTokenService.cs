using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TeamManager.Models;

namespace TeamManager.Services;

public sealed class JwtTokenService(IConfiguration configuration)
{
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "SportManager";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "SportManager.Mobile";
    private readonly SymmetricSecurityKey _signingKey = GetSigningKey(configuration);

    public string CreateToken(User user, out string jwtId, out DateTime expiresAtUtc)
    {
        jwtId = Guid.NewGuid().ToString();
        expiresAtUtc = DateTime.UtcNow.AddHours(1);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name)
        };
        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static SymmetricSecurityKey GetSigningKey(IConfiguration configuration)
    {
        var key = configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "Configure Jwt:SigningKey with at least 32 bytes using user-secrets or an environment variable.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }
}
