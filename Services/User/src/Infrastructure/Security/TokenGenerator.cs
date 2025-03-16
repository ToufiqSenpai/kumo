using System.Text;
using User.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using User.Infrastructure.Options;

namespace User.Infrastructure.Security;

public class TokenGenerator(JwtOptions options) : ITokenGenerator
{
    private const int RefreshTokenLength = 64;
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[RefreshTokenLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        return Convert.ToBase64String(randomNumber);
    }

    public string GenerateAccessToken(Domain.Entities.User user)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };
            
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(options.ExpiresInMinute),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}