using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Grpc.Core;
using GrpcService;
using Item.Application.Features.CreateFile;
using Item.Application.Interfaces;
using Item.Application.Repositories;
using Item.Domain.Enums;
using Item.Domain.Models;
using Item.Tests.Common;
using MongoDB.Bson;
using Moq;
using Shared.Common.Exceptions;

namespace Item.Tests.Unit.Application.Features.CreateFile;

public class CreateFileHandlerTest
{
    private readonly Mock<IItemService> _itemServiceMock = new();
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<FileService.FileServiceClient> _fileServiceClientMock = new();
    private readonly Fixture _fixture = new();
    private readonly ObjectId _parentId = new();
    private readonly Guid _userId = Guid.NewGuid();
    
    public CreateFileHandlerTest()
    {
        _fixture.Customizations.Add(new ObjectIdSpecimenBuilder());
    }
    
    [Fact]
    public async Task Handle_WhenParentFolderIsNull_ThrowsHttpResponseException()
    {
        // Arrange
        var dto = _fixture.Create<CreateFileRequestDto>();
        
        var command = new CreateFileCommand(dto, _parentId, _userId);

        _itemRepositoryMock.Setup(x => x.IsItemExistAsync(It.IsAny<ObjectId>(), It.IsAny<Guid>(),
                It.IsAny<GetItemMode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        var handler = new CreateFileHandler(
            _itemServiceMock.Object,
            _itemRepositoryMock.Object, 
            _mapperMock.Object, 
            _fileServiceClientMock.Object);
        
        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<HttpResponseException>()
            .WithMessage("Parent folder not found.");
    }
    
    [Fact]
    public async Task Handle_WhenParentFolderIsExist_ReturnsCreateFileResponseDto()
    {
        // Arrange
        var dto = _fixture.Create<CreateFileRequestDto>();
        
        var command = new CreateFileCommand(dto, _parentId, _userId);
        
        var parentFolder = _fixture.Create<Domain.Models.Item>();
        
        _itemRepositoryMock.Setup(x => x.IsItemExistAsync(It.IsAny<ObjectId>(), It.IsAny<Guid>(), It.IsAny<GetItemMode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var file = _fixture.Build<Domain.Models.Item>()
            // .Without(x => x.Id)
            .With(x => x.UserId, _userId)
            .Create();
        
        _mapperMock.Setup(x => x.Map<Domain.Models.Item>(dto))
            .Returns(file);
        
        _itemServiceMock.Setup(x => x.CreateParentReferenceAsync(_parentId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<ItemReference>());
        
        var uploadSessionResponse = _fixture.Create<CreateUploadSessionResponse>();

        _fileServiceClientMock.Setup(x => x.CreateUploadSession(
                It.IsAny<CreateUploadSessionRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(uploadSessionResponse);
        
        var handler = new CreateFileHandler(
            _itemServiceMock.Object,
            _itemRepositoryMock.Object, 
            _mapperMock.Object, 
            _fileServiceClientMock.Object);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<CreateFileResponseDto>();
        result.UploadId.Should().Be(uploadSessionResponse.UploadId);
        
        _itemRepositoryMock.Verify(x => x.IsItemExistAsync(_parentId, _userId, GetItemMode.Folder, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<Domain.Models.Item>(dto), Times.Once);
        _itemServiceMock.Verify(x => x.CreateParentReferenceAsync(_parentId, _userId, It.IsAny<CancellationToken>()), Times.Once);
        _itemRepositoryMock.Verify(x => x.AddAsync(file, It.IsAny<CancellationToken>()), Times.Once);
        _fileServiceClientMock.Verify(x => x.CreateUploadSession(
            It.Is<CreateUploadSessionRequest>(r => r.FileId == file.Id.ToString()), 
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}