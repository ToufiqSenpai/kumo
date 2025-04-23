using System.ComponentModel.DataAnnotations;

namespace File.Infrastructure.Options;

public class AwsS3Options
{
    public const string AwsS3 = "AwsS3";
    
    [Required]
    public required string AccessKey { get; init; }
    
    [Required]
    public required string SecretKey { get; init; }
    
    [Required]
    public required string BucketName { get; init; }
    
    [Required]
    [Url]
    public required string ServiceUrl { get; init; }
}