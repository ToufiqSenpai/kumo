using AutoFixture;
using File.Application.Repositories;
using File.Application.Services;
using File.Domain.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Events.Item;

namespace File.Tests.Unit.Application.Services;

public class UploadSessionServiceTest
{
    private readonly Mock<IUploadSessionRepository> _uploadSessionRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ILogger<UploadSessionService>> _loggerMock = new();
    private readonly Fixture _fixture = new();
    
    private readonly UploadSessionService _uploadSessionService;

    public UploadSessionServiceTest()
    {
        _uploadSessionService = new UploadSessionService(_uploadSessionRepositoryMock.Object, _publishEndpointMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task SetUploadSessionAsync_Should_RunningDelayedTask()
    {
        // Arrange  
        var uploadId = Guid.NewGuid().ToString();
        var uploadSession = _fixture.Create<UploadSession>();
        
        // Act
        await _uploadSessionService.SetUploadSessionAsync(uploadId, uploadSession);
        
        // Assert
        _uploadSessionRepositoryMock.Verify(x => x.SetAsync(uploadId, uploadSession), Times.Once);
    }

    [Fact]
    public async Task SetUploadSessionAsync_Should_CancelTheTask()
    {
        // Arrange
        var uploadId = Guid.NewGuid().ToString();
        var uploadSession = _fixture.Create<UploadSession>();
        
        // Act
        Func<Task> act = async () =>
        {
            await _uploadSessionService.SetUploadSessionAsync(uploadId, uploadSession);
            await _uploadSessionService.DeleteUploadSessionAsync(uploadId);
        };
        
        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetUploadSessionAsync_WhenSessionExists_Should_ReplaceToNewSession()
    {
        // Arrange
        var uploadId = Guid.NewGuid().ToString();
        var uploadSession = _fixture.Create<UploadSession>();
        
        // Act
        await _uploadSessionService.SetUploadSessionAsync(uploadId, uploadSession);
        await _uploadSessionService.SetUploadSessionAsync(uploadId, uploadSession);
        
        // Assert
        _uploadSessionRepositoryMock.Verify(x => x.SetAsync(uploadId, uploadSession), Times.Exactly(2));
    }

    [Fact]
    public async Task GetUploadSessionAsync_Should_ReturnUploadSession()
    {
        // Arrange
        var uploadId = Guid.NewGuid().ToString();
        var uploadSession = _fixture.Create<UploadSession>();
        _uploadSessionRepositoryMock.Setup(x => x.GetAsync(uploadId)).ReturnsAsync(uploadSession);
        
        // Act
        var result = await _uploadSessionService.GetUploadSessionAsync(uploadId);
        
        // Assert
        result.Should().BeEquivalentTo(uploadSession);
        _uploadSessionRepositoryMock.Verify(x => x.GetAsync(uploadId), Times.Once);
    }

    [Fact]
    public async Task GetUploadSessionAsync_Should_ReturnNull()
    {
        // Arrange
        var uploadId = Guid.NewGuid().ToString();
        _uploadSessionRepositoryMock.Setup(x => x.GetAsync(uploadId)).ReturnsAsync((UploadSession?)null);
        
        // Act
        var result = await _uploadSessionService.GetUploadSessionAsync(uploadId);
        
        // Assert
        result.Should().BeNull();
        _uploadSessionRepositoryMock.Verify(x => x.GetAsync(uploadId), Times.Once);
    }
    
    [Fact]
    public async Task DeleteUploadSessionAsync_Should_DeleteUploadSession()
    {
        // Arrange
        var uploadId = Guid.NewGuid().ToString();
        var uploadSession = _fixture.Create<UploadSession>();
        _uploadSessionRepositoryMock.Setup(x => x.GetAsync(uploadId)).ReturnsAsync(uploadSession);
        
        // Act
        await _uploadSessionService.SetUploadSessionAsync(uploadId, uploadSession);
        await _uploadSessionService.DeleteUploadSessionAsync(uploadId);
        await Task.Delay(50);
        
        // Assert
        _uploadSessionRepositoryMock.Verify(x => x.SetAsync(uploadId, uploadSession), Times.Once);
        _uploadSessionRepositoryMock.Verify(x => x.GetAsync(uploadId), Times.Once);
        _uploadSessionRepositoryMock.Verify(x => x.DeleteAsync(uploadId), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<DeleteFile>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}