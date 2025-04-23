using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using File.Domain.Entities;
using Moq;
using File.Infrastructure.Services;
using File.Infrastructure.Options;
using FluentAssertions;
using Xunit;

namespace File.Tests.Unit.Infrastructure.Services;

public class AwsS3ServiceTest
{
    private readonly Mock<IAmazonS3> _s3ClientMock = new();
    private readonly AwsS3Options _options = new AwsS3Options
    {
        AccessKey = "access-key",
        SecretKey = "secret-key",
        BucketName = "bucket-name",
        ServiceUrl = "http://localhost:9000"
    };
    private readonly AwsS3Service _awsS3Service;
    private readonly Fixture _fixture = new();

    public AwsS3ServiceTest()
    {
        _awsS3Service = new AwsS3Service(_s3ClientMock.Object, _options);
    }

    [Fact]
    public async Task InitiateMultipartUploadAsync_ShouldReturn_UploadId()
    {
        // Arrange
        var key = "key";
        _s3ClientMock.Setup(x => x.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InitiateMultipartUploadResponse
            {
                UploadId = "upload-id"
            });
        
        // Act
        var result = await _awsS3Service.InitiateMultiPartUploadAsync(key, CancellationToken.None);
        
        // Assert
        result.Should().Be("upload-id");
        
        _s3ClientMock.Verify(x => x.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UploadPartAsync_ShouldReturn_UploadPartResponse()
    {
        // Arrange
        var key = "key";
        var uploadId = "upload-id";
        var partNumber = 1;
        var partStream = new MemoryStream();
        _s3ClientMock.Setup(x => x.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UploadPartResponse());
        
        // Act
        var result = await _awsS3Service.UploadPartAsync(key, uploadId, partNumber, partStream, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        
        _s3ClientMock.Verify(x => x.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CompleteMultiPartUploadAsync_ShouldNotThrowException()
    {
        // Arrange
        var key = "key";
        var uploadId = "upload-id";
        var partETags = new List<FilePart>();
        _s3ClientMock.Setup(x => x.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompleteMultipartUploadResponse());
        
        // Act
        await _awsS3Service.CompleteMultiPartUploadAsync(key, uploadId, partETags, CancellationToken.None);
        
        // Assert
        _s3ClientMock.Verify(x => x.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AbortMultiPartUploadAsync_ShouldNotThrowException()
    {
        // Arrange
        var key = "key";
        var uploadId = "upload-id";
        _s3ClientMock.Setup(x => x.AbortMultipartUploadAsync(It.IsAny<AbortMultipartUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AbortMultipartUploadResponse());
        
        // Act
        await _awsS3Service.AbortMultiPartUploadAsync(key, uploadId, CancellationToken.None);
        
        // Assert
        _s3ClientMock.Verify(x => x.AbortMultipartUploadAsync(It.IsAny<AbortMultipartUploadRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFileAsync_ShouldReturn_StreamFile()
    {
        // Arrange
        _s3ClientMock.Setup(x =>
                x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = new MemoryStream() });
        
        // Act
        var result = await _awsS3Service.GetFileAsync("key", CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        
        _s3ClientMock.Verify(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetFileMetadataAsync_ShouldReturn_ObjectMetadata()
    {
        // Arrange
        var getObjectMetadataResponse = _fixture.Create<GetObjectMetadataResponse>();
        _s3ClientMock.Setup(x =>
                x.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(getObjectMetadataResponse);
        
        // Act
        var result = await _awsS3Service.GetFileMetadataAsync("key", CancellationToken.None);
        
        // Assert
        result.Should().BeEquivalentTo(getObjectMetadataResponse);
        
        _s3ClientMock.Verify(x => x.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}