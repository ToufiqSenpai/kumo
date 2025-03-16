using System.ComponentModel.DataAnnotations;

namespace User.Infrastructure.Options;

public sealed class RabbitmqOptions
{
    public const string Rabbitmq = "Rabbitmq";
    
    [Required]
    public required string Uri { get; init;  }
    
    [Required]
    public required string Username { get; init; }
    
    [Required]
    public required string Password { get; init; }
}