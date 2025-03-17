namespace Item.Domain.Models;

public class File
{
    public Hashes? Hashes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public bool ProcessingMetadata { get; set; } = true;
}