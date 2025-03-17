using AutoFixture;
using Item.Application.Features.CreateRootFolder;
using Item.Application.Repositories;
using Moq;

namespace Item.Tests.Unit.Application.Features.CreateRootFolder;

public class CreateRootFolderHandlerTest
{
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly Fixture _fixture = new();
    
    [Fact]
    public async Task Handle_ShouldCreateRootFolder()
    {
        // Arrange
        var handler = new CreateRootFolderHandler(_itemRepositoryMock.Object);
        var command = _fixture.Create<CreateRootFolderCommand>();
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        // Assert
        _itemRepositoryMock.Verify(x => x.AddAsync(It.Is<Domain.Models.Item>(i => i.Name == "root"), CancellationToken.None), Times.Once);
    }
}