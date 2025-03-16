using System.ComponentModel.DataAnnotations;

namespace User.Infrastructure.Options;

public sealed class PasswordOptions
{
    public const string Password = "Password";
    
    [Required]
    [Range(1, 16)]
    public int DegreeOfParallelism { get; init; }
    
    [Required]
    [Range(8 * 1024, 64 * 1024)]
    public int MemorySize { get; init; }
    
    [Required]
    [Range(1, 5)]
    public int Iterations { get; init; }
    
    [Required]
    [Range(16, 128)]
    public int SaltSize { get; init; }
    
    [Required]
    [Range(16, 128)]
    public int HashSize { get; init; }
}