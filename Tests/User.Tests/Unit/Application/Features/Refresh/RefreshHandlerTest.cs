using AutoFixture;
using FluentAssertions;
using Moq;
using Shared.Common.Exceptions;
using User.Application.Features.Refresh;
using User.Application.Interfaces;
using User.Application.Repositories;
using User.Domain.Entities;
using User.Infrastructure.Options;

namespace User.Tests.Unit.Application.Features.Refresh;

public class RefreshHandlerTest
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock = new();
    private readonly RefreshTokenOptions _refreshTokenOptions = new RefreshTokenOptions
    {
        ExpiresInDays = 30
    };
    private readonly Fixture _fixture = new();
    
    [Fact]
    public async Task Handle_WhenRefreshTokenIsExpires_ThrowsHttpResponseException()
    {
        // Arrange
        var refreshToken = _fixture.Create<Domain.Entities.RefreshToken>();
        refreshToken.Expires = DateTime.UtcNow.AddDays(-1);
        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        var handler = new RefreshHandler(_refreshTokenRepositoryMock.Object, _tokenGeneratorMock.Object, _refreshTokenOptions);
        var command = new RefreshCommand(_fixture.Create<RefreshRequestDto>());
        
        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<HttpResponseException>().WithMessage("Refresh token is invalid or expired.");
        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateAccessToken(It.IsAny<Domain.Entities.User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.DeleteByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenRefreshTokenIsNull_ThrowsHttpResponseException()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken) null);
        var handler = new RefreshHandler(_refreshTokenRepositoryMock.Object, _tokenGeneratorMock.Object, _refreshTokenOptions);
        var command = new RefreshCommand(_fixture.Create<RefreshRequestDto>());
        
        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<HttpResponseException>().WithMessage("Refresh token is invalid or expired.");
        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateAccessToken(It.IsAny<Domain.Entities.User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.DeleteByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsExists_ReturnRefreshResponseDto()
    {
        var refreshToken = _fixture.Create<Domain.Entities.RefreshToken>();
        refreshToken.Expires = DateTime.UtcNow.AddDays(_refreshTokenOptions.ExpiresInDays);
        
        var newRefreshToken = _fixture.Create<string>();
        var newAccessToken = _fixture.Create<string>();
        
        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _tokenGeneratorMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);
        _tokenGeneratorMock.Setup(x => x.GenerateAccessToken(It.IsAny<Domain.Entities.User>())).Returns(newAccessToken);
        
        var handler = new RefreshHandler(_refreshTokenRepositoryMock.Object, _tokenGeneratorMock.Object, _refreshTokenOptions);
        var command = new RefreshCommand(_fixture.Create<RefreshRequestDto>());
        
        var result = await handler.Handle(command, CancellationToken.None);
        
        result.RefreshToken.Should().Be(newRefreshToken);
        result.AccessToken.Should().Be(newAccessToken);
        
        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateAccessToken(It.IsAny<Domain.Entities.User>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.DeleteByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}