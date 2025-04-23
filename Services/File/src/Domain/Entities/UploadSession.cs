namespace File.Domain.Entities;

public class UploadSession
{
    public string FileId { get; set; }
    public string UploadId { get; set; }
    public DateTime Expires { get; set; }
    public long Size { get; set; }
    public List<FilePart> FileParts { get; set; }
}