using System.ComponentModel.DataAnnotations;

namespace User.Infrastructure.Options;

public sealed class RefreshTokenOptions
{
    public const string RefreshToken = "RefreshToken";
    
    [Required]
    public int ExpiresInDays { get; init; }
}