using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;
using Shared.Helpers.Interfaces;
using Microsoft.Extensions.Configuration;
namespace Shared.Helpers.Implementations;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(UserDto user)
    {
        // Fix: Access "JwtSettings:Secret" to match your JSON structure
        var secretKey = _config["JwtSettings:Secret"];

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("JWT Secret Key is missing from configuration (check User Secrets).");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"], // Matches your appsettings.json
            audience: _config["JwtSettings:Audience"], // Matches your appsettings.json
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpiryInMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}