using AutoFixture;
using Moq;
using User.Application.Features.LogoutUser;
using User.Application.Repositories;

namespace User.Tests.Unit.Application.Features.LogoutUser;

public class LogoutUserHandlerTest
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task Handle_ShouldDeleteRefreshToken()
    {
        // Arrange
        var refreshToken = _fixture.Create<string>();
        var command = new LogoutUserCommand(new LogoutUserRequestDto(refreshToken));
        var logoutUserHandler = new LogoutUserHandler(_refreshTokenRepositoryMock.Object);
        
        // Act
        await logoutUserHandler.Handle(command, CancellationToken.None);
        
        // Assert
        _refreshTokenRepositoryMock.Verify(x => x.DeleteByTokenAsync(refreshToken, It.IsAny<CancellationToken>()));
    }
}