using File.Domain.Entities;

namespace File.Application.Interfaces;

public interface IFileStorageService
{
    public Task<string> InitiateMultiPartUploadAsync(string fileId, CancellationToken cancellationToken);
    public Task<FilePart> UploadPartAsync(string fileId, string uploadId, int partNumber, Stream partStream, CancellationToken cancellationToken);
    public Task CompleteMultiPartUploadAsync(string fileId, string uploadId, List<FilePart> partETags, CancellationToken cancellationToken);
    public Task AbortMultiPartUploadAsync(string fileId, string uploadId, CancellationToken cancellationToken);
    public Task<Stream> GetFileAsync(string fileId, CancellationToken cancellationToken);
    public Task<FileMetadata?> GetFileMetadataAsync(string fileId, CancellationToken cancellationToken);
    public Task<bool> DeleteFileAsync(string fileId, CancellationToken cancellationToken);
}