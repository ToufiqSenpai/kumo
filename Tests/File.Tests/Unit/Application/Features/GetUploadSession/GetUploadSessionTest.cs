using System.Net;
using File.Application.Features.GetUploadSession;
using File.Application.Interfaces;
using File.Domain.Entities;
using FluentAssertions;
using Moq;
using Shared.Common.Exceptions;

namespace File.Tests.Unit.Application.Features.GetUploadSession;

public class GetUploadSessionTest
{
    private readonly Mock<IUploadSessionService> _uploadSessionServiceMock;
    private readonly GetUploadSessionHandler _handler;

    public GetUploadSessionTest()
    {
        _uploadSessionServiceMock = new Mock<IUploadSessionService>();
        _handler = new GetUploadSessionHandler(_uploadSessionServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var uploadId = Guid.NewGuid().ToString();
        var request = new GetUploadSessionQuery(uploadId);
        var uploadSession = new UploadSession
        {
            Expires = DateTime.UtcNow.AddHours(1),
            FileParts = new List<FilePart>
            {
                new FilePart { End = 100 }
            }
        };

        _uploadSessionServiceMock.Setup(x => x.GetUploadSessionAsync(uploadId))
            .ReturnsAsync(uploadSession);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Expires.Should().Be(uploadSession.Expires);
        result.NextStartRange.Should().Be(101);
    }

    [Fact]
    public async Task Handle_UploadSessionNotFound_ThrowsHttpResponseException()
    {
        // Arrange
        var uploadId = Guid.NewGuid().ToString();
        var request = new GetUploadSessionQuery(uploadId);

        _uploadSessionServiceMock.Setup(x => x.GetUploadSessionAsync(uploadId))
            .ReturnsAsync((UploadSession)null);

        // Act
        Func<Task> act = () => _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpResponseException>()
            .WithMessage("Upload session not found.");
    }
}