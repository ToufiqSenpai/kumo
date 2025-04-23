using AutoFixture;
using File.Application.Features.PutChunkFile;
using File.Application.Interfaces;
using File.Domain.Entities;
using File.Domain.ValueObjects;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Common.Exceptions;

namespace File.Tests.Unit.Application.Features.PutChunkFile;

public class PutChunkFileHandlerTest
{
    private readonly Mock<IUploadSessionService> _uploadSessionServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IFileMetadataExtractor> _fileMetadataExtractorMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Fixture _fixture = new();
    
    private readonly PutChunkFileHandler _handler;

    public PutChunkFileHandlerTest()
    {
        _handler = new PutChunkFileHandler(
            _uploadSessionServiceMock.Object,
            _fileStorageServiceMock.Object,
            _fileMetadataExtractorMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUploadSessionIsNotFound_ShouldThrow_HttpResponseException()
    {
        var header = new PutChunkFileHeader
        {
            ContentRange = new ContentRange("bytes 0-1024/8192"),
            ContentLength = 1024
        };
        var command = new PutChunkFileCommand(Guid.NewGuid().ToString(), new MemoryStream(), header);

        _uploadSessionServiceMock.Setup(x => x.GetUploadSessionAsync(command.UploadId))
            .ReturnsAsync((UploadSession)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HttpResponseException>().WithMessage("Upload session not found.");
        
        _uploadSessionServiceMock.Verify(x => x.GetUploadSessionAsync(command.UploadId), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenUploadSessionIsFound_ShouldUploadPartAndContinueUpload()
    {
        var header = new PutChunkFileHeader
        {
            ContentRange = new ContentRange("bytes 0-1024/8192"),
            ContentLength = 1024
        };
        var uploadSession = _fixture.Create<UploadSession>();
        uploadSession.Size = 8192;
        uploadSession.FileParts.Add(new FilePart { End = 1024 });
        
        var command = new PutChunkFileCommand(uploadSession.UploadId, new MemoryStream(), header);
        
        _uploadSessionServiceMock.Setup(x => x.GetUploadSessionAsync(command.UploadId))
            .ReturnsAsync(uploadSession);

        _fileStorageServiceMock.Setup(x => x.UploadPartAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<Stream>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FilePart());

        var result = await _handler.Handle(command, CancellationToken.None) as AcceptedResult;

        result.Should().BeOfType<AcceptedResult>();
        result.Value.Should().BeOfType<PutChunkFileContinueUploadResponseDto>();

        _uploadSessionServiceMock.Verify(x => x.SetUploadSessionAsync(command.UploadId, It.IsAny<UploadSession>()));
        _uploadSessionServiceMock.Verify(x => x.GetUploadSessionAsync(command.UploadId), Times.Once);
        _fileStorageServiceMock.Verify(x => x.UploadPartAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<Stream>(), 
            It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task Handle_WhenAllPartsAreUploaded_ShouldFinishUpload()
    {
        var header = new PutChunkFileHeader
        {
            ContentRange = new ContentRange("bytes 0-8191/8192"),
            ContentLength = 8192
        };
        var uploadSession = _fixture.Create<UploadSession>();
        uploadSession.Size = 8192;
        uploadSession.FileParts.Add(new FilePart { End = 8191 });
        
        var command = new PutChunkFileCommand(uploadSession.UploadId, new MemoryStream(), header);
        
        _uploadSessionServiceMock.Setup(x => x.GetUploadSessionAsync(command.UploadId))
            .ReturnsAsync(uploadSession);

        _fileStorageServiceMock.Setup(x => x.UploadPartAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<Stream>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FilePart());

        var result = await _handler.Handle(command, CancellationToken.None) as OkObjectResult;

        result.Should().BeOfType<OkObjectResult>();
        result.Value.Should().BeOfType<PutChunkFileFinishUploadResponseDto>();

        _uploadSessionServiceMock.Verify(x => x.SetUploadSessionAsync(command.UploadId, It.IsAny<UploadSession>()), Times.Never);
        _uploadSessionServiceMock.Verify(x => x.GetUploadSessionAsync(command.UploadId), Times.Once);
        _uploadSessionServiceMock.Verify(x => x.DeleteUploadSessionAsync(command.UploadId));
        _fileStorageServiceMock.Verify(x => x.CompleteMultiPartUploadAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<List<FilePart>>(), 
            It.IsAny<CancellationToken>()));
        _fileMetadataExtractorMock.Verify(x => x.GetFileMimeTypeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        _fileMetadataExtractorMock.Verify(x => x.EnqueueFileMetadataExtractionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        _fileStorageServiceMock.Verify(x => x.UploadPartAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<Stream>(), 
            It.IsAny<CancellationToken>()));
    }
}