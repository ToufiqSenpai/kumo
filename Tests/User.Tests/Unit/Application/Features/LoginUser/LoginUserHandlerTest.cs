using AutoFixture;
using FluentAssertions;
using Moq;
using Shared.Common.Exceptions;
using User.Application.Features.LoginUser;
using User.Application.Interfaces;
using User.Application.Repositories;
using User.Infrastructure.Options;
using Xunit.Abstractions;

namespace User.Tests.Unit.Application.Features.LoginUser;

public class LoginUserHandlerTest(ITestOutputHelper output)
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock = new();
    private readonly RefreshTokenOptions _refreshTokenOptions = new RefreshTokenOptions
    {
        ExpiresInDays = 30
    };
    private readonly Fixture _fixture = new();
    
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowHttpResponseException()
    {
        // Arrange
        var handler = new LoginUserHandler(
            _userRepositoryMock.Object, 
            _refreshTokenRepositoryMock.Object, 
            _tokenGeneratorMock.Object, 
            _refreshTokenOptions);
        var dtoDummy = _fixture.Create<LoginUserRequestDto>();
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User) null);
        
        // Act
        Func<Task> act = async () => await handler.Handle(new LoginUserCommand(dtoDummy), CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<HttpResponseException>().WithMessage("User not found");
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenUserFound_ShouldReturnLoginUserResponseDto()
    {
        // Arrange
        var handler = new LoginUserHandler(
            _userRepositoryMock.Object, 
            _refreshTokenRepositoryMock.Object, 
            _tokenGeneratorMock.Object,
            _refreshTokenOptions);
        var userDummy = _fixture.Create<Domain.Entities.User>();
        var dtoDummy = _fixture.Create<LoginUserRequestDto>();
        var refreshTokenDummy = _fixture.Create<string>();
        var accessTokenDummy = _fixture.Create<string>();
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDummy);
        _tokenGeneratorMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshTokenDummy);
        _tokenGeneratorMock.Setup(x => x.GenerateAccessToken(It.IsAny<Domain.Entities.User>())).Returns(accessTokenDummy);
        
        // Act
        var result = await handler.Handle(new LoginUserCommand(dtoDummy), CancellationToken.None);
        
        // Assert
        result.RefreshToken.Should().Be(refreshTokenDummy);
        result.AccessToken.Should().Be(accessTokenDummy);
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateAccessToken(It.IsAny<Domain.Entities.User>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}