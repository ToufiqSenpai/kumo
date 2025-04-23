using System.Net;
using AutoMapper;
using Item.Application.Interfaces;
using Item.Application.Repositories;
using Item.Domain.Enums;
using MediatR;
using Shared.Common.Exceptions;

namespace Item.Application.Features.CreateFolder;

public sealed class CreateFolderHandler(IItemService itemService, IItemRepository itemRepository, IMapper mapper) : IRequestHandler<CreateFolderCommand, Domain.Models.Item>
{
    public async Task<Domain.Models.Item> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        var isParentFolderExist = await itemRepository.IsItemExistAsync(request.ParentId, request.UserId, GetItemMode.Folder, cancellationToken);

        if (!isParentFolderExist)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound, "Parent folder not found.");
        }
        
        var folder = mapper.Map<Domain.Models.Item>(request.Dto);
        folder.ParentReference = await itemService.CreateParentReferenceAsync(request.ParentId, request.UserId, cancellationToken);
        folder.Size = 0;
        folder.UserId = request.UserId;

        await itemRepository.AddAsync(folder, cancellationToken);
        
        return folder;
    }
}