using File.Application.Features.CreateUploadSession;
using File.Application.Interfaces;
using File.Domain.Entities;
using FluentAssertions;
using Moq;

namespace File.Tests.Unit.Application.Features.CreateUploadSession;

public class CreateUploadSessionHandlerTest
{
    private readonly Mock<IUploadSessionService> _uploadSessionServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly CreateUploadSessionHandler _handler;

    public CreateUploadSessionHandlerTest()
    {
        _handler = new CreateUploadSessionHandler(_uploadSessionServiceMock.Object, _fileStorageServiceMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnUploadSession_WhenCalled()
    {
        // Arrange
        var command = new CreateUploadSessionCommand("test-file-id");
        var uploadId = "test-upload-id";
        
        _fileStorageServiceMock.Setup(x => x.InitiateMultiPartUploadAsync(command.FileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadId);
        
        _uploadSessionServiceMock.Setup(x => x.SetUploadSessionAsync(uploadId, It.IsAny<UploadSession>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UploadId.Should().Be(uploadId);
        result.Expires.Should().NotBeNull();
    }
}