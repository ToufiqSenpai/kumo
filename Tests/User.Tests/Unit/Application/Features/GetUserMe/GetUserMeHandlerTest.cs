using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Moq;
using Shared.Common.Exceptions;
using User.Application.DTOs;
using User.Application.Features.GetUserMe;
using User.Application.Repositories;

namespace User.Tests.Unit.Application.Features.GetUserMe;

public class GetUserMeHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task Handle_ShouldReturnUserDto()
    {
        // Arrange
        var user = _fixture.Build<Domain.Entities.User>()
            .Without(u => u.RefreshTokens)
            .Create();
        var userDto = new UserDto(user.Name, user.Email);
        var query = new GetUserMeQuery(user.Id);
        var getUserMeHandler = new GetUserMeHandler(_userRepositoryMock.Object, _mapperMock.Object);
        
        _userRepositoryMock.Setup(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(userDto);
        
        // Act
        var result = await getUserMeHandler.Handle(query, CancellationToken.None);
        
        // Assert
        result.Should().BeEquivalentTo(userDto);
        
        _userRepositoryMock.Verify(x => x.GetAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<UserDto>(user), Times.Once);
    }
    
    [Fact]
    public void Handle_UserNotFound_ShouldThrowHttpResponseException()
    {
        // Arrange
        var query = new GetUserMeQuery(_fixture.Create<Guid>());
        var getUserMeHandler = new GetUserMeHandler(_userRepositoryMock.Object, _mapperMock.Object);
        
        _userRepositoryMock.Setup(x => x.GetAsync(query.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Entities.User)null);
        
        // Act
        Func<Task> act = async () => await getUserMeHandler.Handle(query, CancellationToken.None);
        
        // Assert
        act.Should().ThrowAsync<HttpResponseException>().WithMessage("User not found.");
        
        _userRepositoryMock.Verify(x => x.GetAsync(query.UserId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
    }
}