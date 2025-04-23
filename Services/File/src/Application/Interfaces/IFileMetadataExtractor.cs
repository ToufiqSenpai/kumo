namespace File.Application.Interfaces;

public interface IFileMetadataExtractor
{
    public Task<string> GetFileMimeTypeAsync(string fileId, CancellationToken cancellationToken);
    public void EnqueueFileMetadataExtractionAsync(string fileId, string mimeType, CancellationToken cancellationToken);
}