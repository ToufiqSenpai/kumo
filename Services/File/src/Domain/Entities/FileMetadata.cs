namespace File.Domain.Entities;

public class FileMetadata
{
    public long Size { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string ContentType { get; set; }
    public IDictionary<string, string> Metadata { get; set; }
}