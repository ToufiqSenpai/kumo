using AutoFixture;
using FluentAssertions;
using Item.Application.Features.GetItem;
using Item.Application.Repositories;
using Item.Domain.Enums;
using Item.Tests.Common;
using MongoDB.Bson;
using Moq;
using Shared.Common.Exceptions;

namespace Item.Tests.Unit.Application.Features.GetItem;

public class GetItemHandlerTest
{
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly Fixture _fixture = new();
    private readonly ObjectId _itemId = new();
    private readonly Guid _userId = Guid.NewGuid();

    public GetItemHandlerTest()
    {
        _fixture.Customizations.Add(new ObjectIdSpecimenBuilder());
    }
    
    [Fact]
    public async Task Handle_WhenItemExists_ShouldReturnItem()
    {
        // Arrange
        var item = _fixture.Create<Item.Domain.Models.Item>();
        var query = new GetItemQuery(_itemId, _userId);
        _itemRepositoryMock.Setup(x => x.GetAsync(query.ItemId, query.UserId, GetItemMode.All, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        var handler = new GetItemHandler(_itemRepositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(item);
        
        _itemRepositoryMock.Verify(x => x.GetAsync(_itemId, _userId, GetItemMode.All, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenItemDoesNotExist_ShouldThrowsHttpResponseException()
    {
        // Arrange
        var query = new GetItemQuery(new ObjectId(), Guid.NewGuid());
        _itemRepositoryMock.Setup(x => x.GetAsync(query.ItemId, query.UserId, GetItemMode.All, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Models.Item)null);
        
        var handler = new GetItemHandler(_itemRepositoryMock.Object);
        
        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<HttpResponseException>().WithMessage("Item not found.");
    }
}