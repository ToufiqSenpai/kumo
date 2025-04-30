namespace File.Infrastructure.Options;

public sealed class RedisOptions
{
    public const string Redis = "Redis";
    
    public string Host { get; set; } = string.Empty;
}