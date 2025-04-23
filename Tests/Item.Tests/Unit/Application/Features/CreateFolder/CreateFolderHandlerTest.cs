using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Item.Application.Features.CreateFolder;
using Item.Application.Interfaces;
using Item.Application.Repositories;
using Item.Domain.Enums;
using Item.Domain.Models;
using Item.Tests.Common;
using MongoDB.Bson;
using Moq;
using Shared.Common.Exceptions;

namespace Item.Tests.Unit.Application.Features.CreateFolder;

public class CreateFolderHandlerTest
{
    private readonly Mock<IItemService> _itemServiceMock = new();
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Fixture _fixture = new();
    private readonly ObjectId _parentId = new();
    private readonly Guid _userId = Guid.NewGuid();

    public CreateFolderHandlerTest()
    {
        _fixture.Customizations.Add(new ObjectIdSpecimenBuilder());
    }
    
    [Fact]
    public async Task Handle_WhenParentFolderNotExist_ThrowHttpResponseException()
    {
        // Arrange
        var dto = _fixture.Create<CreateFolderRequestDto>();
        var command = new CreateFolderCommand(dto, _parentId, _userId);
        
        _itemRepositoryMock.Setup(x => x.IsItemExistAsync(command.ParentId, command.UserId, GetItemMode.Folder, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        var handler = new CreateFolderHandler(_itemServiceMock.Object, _itemRepositoryMock.Object, _mapperMock.Object);
        
        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<HttpResponseException>().WithMessage("Parent folder not found.");
    }
    
    [Fact]
    public async Task Handle_WhenParentFolderExist_CreateFolder()
    {
        // Arrange
        var dto = _fixture.Create<CreateFolderRequestDto>();
        var command = new CreateFolderCommand(dto, _parentId, _userId);
        var item = _fixture.Create<Item.Domain.Models.Item>();
        
        _itemRepositoryMock.Setup(x => x.IsItemExistAsync(command.ParentId, command.UserId, GetItemMode.Folder, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _mapperMock.Setup(x => x.Map<Domain.Models.Item>(dto))
            .Returns(item);
        
        _itemServiceMock.Setup(x => x.CreateParentReferenceAsync(command.ParentId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<ItemReference>());
        
        var handler = new CreateFolderHandler(_itemServiceMock.Object, _itemRepositoryMock.Object, _mapperMock.Object);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().BeEquivalentTo(item);
        
        _itemRepositoryMock.Verify(x => x.IsItemExistAsync(_parentId, _userId, GetItemMode.Folder, It.IsAny<CancellationToken>()));
        _mapperMock.Verify(x => x.Map<Item.Domain.Models.Item>(dto));
        _itemServiceMock.Verify(x => x.CreateParentReferenceAsync(_parentId, _userId, It.IsAny<CancellationToken>()));
        _itemRepositoryMock.Verify(x => x.AddAsync(item, It.IsAny<CancellationToken>()));
    }
}