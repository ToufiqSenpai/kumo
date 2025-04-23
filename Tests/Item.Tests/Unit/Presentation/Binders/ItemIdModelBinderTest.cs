using FluentAssertions;
using Item.Application.Repositories;
using Item.Presentation.Binders;
using Item.Presentation.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using Moq;

namespace Item.Tests.Unit.Presentation.Binders;

public class ItemIdModelBinderTest
{
    private const string ModelName = "id";
    private readonly Guid _userId = Guid.NewGuid();
    private readonly ObjectId _objectId = new();
    private readonly DefaultHttpContext _httpContext = new();
    private readonly Mock<ModelBindingContext> _modelBindingContextMock = new();
    private readonly Mock<ModelStateDictionary> _modelStateDictionaryMock = new();
    private readonly Mock<IValueProvider> _valueProviderMock = new();
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    
    [Fact]
    public async Task BindModelAsync_WhenItemIdIsValid_ShouldSetItemIdInHttpContextItems()
    {
        // Arrange
        _valueProviderMock.Setup(x => x.GetValue(It.IsAny<string>())).Returns(ValueProviderResult.None);
        _valueProviderMock.Setup(x => x.GetValue(ModelName)).Returns(new ValueProviderResult(_objectId.ToString()));

        _modelBindingContextMock.Setup(x => x.HttpContext).Returns(_httpContext);
        _modelBindingContextMock.Setup(x => x.ValueProvider).Returns(_valueProviderMock.Object);
        _modelBindingContextMock.Setup(x => x.ModelName).Returns(ModelName);

        var objectIdModelBinder = new ItemIdModelBinder(_itemRepositoryMock.Object);

        // Act
        await objectIdModelBinder.BindModelAsync(_modelBindingContextMock.Object);

        // Assert
        _httpContext.Items.ContainsKey(HttpContextItemKey.ItemId).Should().BeTrue();
        _httpContext.Items[HttpContextItemKey.ItemId].Should().Be(_objectId);
        _modelBindingContextMock.VerifySet(x => x.Result = ModelBindingResult.Success(_objectId), Times.Once);
        _itemRepositoryMock.Verify(x => x.GetRootFolderIdByUserIdAsync(_userId, CancellationToken.None), Times.Never);
    }
    
    [Fact]
    public async Task BindModelAsync_WhenItemIdIsInvalid_ShouldAddModelError()
    {
        // Arrange
        _valueProviderMock.Setup(x => x.GetValue(It.IsAny<string>())).Returns(ValueProviderResult.None);
        _valueProviderMock.Setup(x => x.GetValue(ModelName)).Returns(new ValueProviderResult("invalid"));

        _modelBindingContextMock.Setup(x => x.HttpContext).Returns(_httpContext);
        _modelBindingContextMock.Setup(x => x.ValueProvider).Returns(_valueProviderMock.Object);
        _modelBindingContextMock.Setup(x => x.ModelName).Returns(ModelName);
        _modelBindingContextMock.Setup(x => x.ModelState).Returns(_modelStateDictionaryMock.Object);
        _modelStateDictionaryMock.Setup(x => x.AddModelError(It.IsAny<string>(), It.IsAny<string>()));

        var objectIdModelBinder = new ItemIdModelBinder(_itemRepositoryMock.Object);

        // Act
        await objectIdModelBinder.BindModelAsync(_modelBindingContextMock.Object);

        // Assert
        _httpContext.Items.ContainsKey(HttpContextItemKey.ItemId).Should().BeFalse();
        _modelStateDictionaryMock.Verify(x => x.AddModelError(ModelName, "Invalid ObjectId format."), Times.Once);
        _itemRepositoryMock.Verify(x => x.GetRootFolderIdByUserIdAsync(_userId, CancellationToken.None), Times.Never);
    }
}