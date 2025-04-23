using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using File.Application.Interfaces;
using File.Domain.Entities;
using File.Infrastructure.Options;

namespace File.Infrastructure.Services;

public class AwsS3Service(IAmazonS3 s3Client, AwsS3Options options) : IFileStorageService
{
    public async Task<string> InitiateMultiPartUploadAsync(string key, CancellationToken cancellationToken)
    {
        var request = new InitiateMultipartUploadRequest
        {
            BucketName = options.BucketName,
            Key = key
        };
        
        var response = await s3Client.InitiateMultipartUploadAsync(request, cancellationToken);
        
        return response.UploadId;
    }

    public async Task<FilePart> UploadPartAsync(string key, string uploadId, int partNumber, Stream partStream, CancellationToken cancellationToken)
    {
        var request = new UploadPartRequest
        {
            BucketName = options.BucketName,
            Key = key,
            UploadId = uploadId,
            PartNumber = partNumber,
            PartSize = partStream.Length,
            InputStream = partStream,
            IsLastPart = false
        };
        
        var res = await s3Client.UploadPartAsync(request, cancellationToken);

        return new FilePart
        {
            Id = res.ETag,
            PartNumber = partNumber
        };
    }

    public async Task CompleteMultiPartUploadAsync(string key, string uploadId, List<FilePart> fileParts, CancellationToken cancellationToken)
    {
        var request = new CompleteMultipartUploadRequest
        {
            BucketName = options.BucketName,
            Key = key,
            UploadId = uploadId,
            PartETags = fileParts.Select(fp => new PartETag(fp.PartNumber, fp.Id)).ToList()
        };
        
        await s3Client.CompleteMultipartUploadAsync(request, cancellationToken);
    }

    public async Task AbortMultiPartUploadAsync(string key, string uploadId, CancellationToken cancellationToken)
    {
        var request = new AbortMultipartUploadRequest
        {
            BucketName = options.BucketName,
            Key = key,
            UploadId = uploadId
        };
        
        await s3Client.AbortMultipartUploadAsync(request, cancellationToken);
    }

    public async Task<Stream> GetFileAsync(string key, CancellationToken cancellationToken)
    {
        var request = new GetObjectRequest
        {
            BucketName = options.BucketName,
            Key = key
        };
        
        var response = await s3Client.GetObjectAsync(request, cancellationToken);
        
        return response.ResponseStream;
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string key, CancellationToken cancellationToken)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = options.BucketName,
            Key = key
        };
        
        var res = await s3Client.GetObjectMetadataAsync(request, cancellationToken);

        return new FileMetadata
        {
            Size = res.ContentLength,
            ContentType = "application/octet-stream",
            LastModified = res.LastModified,
            Metadata = res.Metadata.Keys.ToDictionary(
                key => key,
                key => res.Metadata[key])
        };
    }

    public async Task<bool> DeleteFileAsync(string key, CancellationToken cancellationToken)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = options.BucketName,
            Key = key
        };
        
        var response = await s3Client.DeleteObjectAsync(request, cancellationToken);
        
        return response.HttpStatusCode == HttpStatusCode.NoContent;
    }
}