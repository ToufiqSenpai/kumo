using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using File.Application.Interfaces;
using File.Domain.Entities;
using File.Infrastructure.Options;

namespace File.Infrastructure.Services;

public class AzureBlobStorageService(BlobServiceClient blobServiceClient, AzureBlobStorageOptions options) : IFileStorageService
{
    private readonly BlobContainerClient _containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);
    // private const string Prefix
    
    public async Task<string> InitiateMultiPartUploadAsync(string fileId, CancellationToken cancellationToken)
    {
        return Guid.NewGuid().ToString();
    }

    public async Task<FilePart> UploadPartAsync(string fileId, string uploadId, int partNumber, Stream partStream,
        CancellationToken cancellationToken)
    {
        var blockBlobClient = _containerClient.GetBlockBlobClient(fileId);
        string blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{partNumber:D6}"));
        
        var result = await blockBlobClient.StageBlockAsync(
            base64BlockId: blockId,
            content: partStream,
            cancellationToken: cancellationToken);

        return new FilePart
        {
            Id = blockId,
            PartNumber = partNumber
        };
    }

    public async Task CompleteMultiPartUploadAsync(string fileId, string uploadId, List<FilePart> fileParts, CancellationToken cancellationToken)
    {
        var blockBlobClient = _containerClient.GetBlockBlobClient(fileId);
        var blockList = fileParts
            .OrderBy(fp => fp.PartNumber)
            .Select(fp => fp.Id)
            .ToList();
        
        await blockBlobClient.CommitBlockListAsync(blockList, cancellationToken: cancellationToken);
    }

    public async Task AbortMultiPartUploadAsync(string fileId, string uploadId, CancellationToken cancellationToken)
    {
        await DeleteFileAsync(fileId, cancellationToken);
    }

    public async Task<Stream> GetFileAsync(string fileId, CancellationToken cancellationToken)
    {
        var blockBlobClient = _containerClient.GetBlockBlobClient(fileId);
        var response = await blockBlobClient.DownloadAsync(cancellationToken: cancellationToken);

        return response.Value.Content;
    }

    public async Task<FileMetadata?> GetFileMetadataAsync(string fileId, CancellationToken cancellationToken)
    {
        var blobClient = _containerClient.GetBlobClient(fileId);
        var blobProperties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        if (!blobProperties.HasValue)
            return null;

        return new FileMetadata
        {
            Size = blobProperties.Value.ContentLength,
        };
    }

    public async Task<bool> DeleteFileAsync(string fileId, CancellationToken cancellationToken)
    {
        var blobClient = _containerClient.GetBlobClient(fileId);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        return response.Value;
    }
}