using System.ComponentModel.DataAnnotations;

namespace User.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string Jwt = "Jwt";
    
    [Required]
    public required string SecretKey { get; init; }
    
    [Required]
    public required string Issuer { get; init; }
    
    [Required]
    public required string Audience { get; init; }
    
    [Required]
    public required int ExpiresInMinute { get; init; }
}