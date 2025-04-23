using System.Net;
using AutoMapper;
using Item.Application.Repositories;
using Item.Domain.Enums;
using MediatR;
using Shared.Common.Exceptions;
using GrpcService;
using Item.Application.Interfaces;

namespace Item.Application.Features.CreateFile;

public class CreateFileHandler(
    IItemService itemService,
    IItemRepository itemRepository, 
    IMapper mapper, 
    FileService.FileServiceClient fileServiceClient) : IRequestHandler<CreateFileCommand, CreateFileResponseDto>
{
    public async Task<CreateFileResponseDto> Handle(CreateFileCommand request, CancellationToken cancellationToken)
    {
        var isParentFolderExist = await itemRepository.IsItemExistAsync(
                request.ParentId, 
                request.UserId, 
                GetItemMode.Folder,
                cancellationToken);

        if (!isParentFolderExist)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound, "Parent folder not found.");
        }
        
        var file = mapper.Map<Domain.Models.Item>(request.Dto);
        file.UserId = request.UserId;
        file.ParentReference = await itemService.CreateParentReferenceAsync(request.ParentId, request.UserId, cancellationToken);
        
        await itemRepository.AddAsync(file, cancellationToken);

        var res = fileServiceClient.CreateUploadSession(
            new CreateUploadSessionRequest { FileId = file.Id.ToString() }, 
            cancellationToken: cancellationToken);

        return new CreateFileResponseDto(res.UploadId);
    }
}