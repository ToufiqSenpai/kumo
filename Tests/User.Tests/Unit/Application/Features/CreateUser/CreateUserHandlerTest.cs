using AutoFixture;
using AutoMapper;
using FluentAssertions;
using MassTransit;
using Moq;
using Shared.Events.Item;
using User.Application.Features.CreateUser;
using User.Application.Interfaces;
using User.Application.Repositories;
using Xunit.Abstractions;

namespace User.Tests.Unit.Application.Features.CreateUser;

public class CreateUserHandlerTest(ITestOutputHelper output)
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task Handler_ShouldCreateUser_Successfully()
    {
        // Arrange
        var createUserHandler = new CreateUserHandler(
            _userRepositoryMock.Object, 
            _passwordHasherMock.Object, 
            _mapperMock.Object, 
            _publishEndpointMock.Object);
        var userDummy = _fixture.Create<Domain.Entities.User>();
        var dtoDummy = _fixture.Create<CreateUserRequestDto>();
        string hashedPassword = "hashed-password";
        string? plainPassword = null;
        
        _mapperMock.Setup(mapper => mapper.Map<Domain.Entities.User>(It.IsAny<CreateUserRequestDto>()))
            .Returns(userDummy);
        _passwordHasherMock.Setup(passwordHasher => passwordHasher.HashPassword(It.IsAny<string>()))
            .Callback<string>(password => plainPassword = password)
            .Returns(hashedPassword);
        
        // Act
        var result = await createUserHandler.Handle(new CreateUserCommand(dtoDummy), CancellationToken.None);
        
        // Assert
        _mapperMock.Verify(mapper => mapper.Map<Domain.Entities.User>(dtoDummy), Times.Once);
        _passwordHasherMock.Verify(passwordHasher => passwordHasher.HashPassword(plainPassword), Times.Once);
        _userRepositoryMock.Verify(userRepository => userRepository.AddAsync(userDummy, CancellationToken.None), Times.Once);
        _publishEndpointMock.Verify(publishEndpoint => publishEndpoint.Publish(new CreateRootFolder(userDummy.Id), CancellationToken.None), Times.Once);

        result.Should().BeOfType<CreateUserResponseDto>();
    }
}