using System.ComponentModel.DataAnnotations;

namespace User.Infrastructure.Options;

public class DatabaseOptions
{
    public const string Database = "Database";
    
    [Required]
    public required string ConnectionString { get; init; }
}