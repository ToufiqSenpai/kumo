using System.Net;
using Item.Application.Repositories;
using Item.Domain.Enums;
using Item.Infrastructure.Utils;
using MediatR;
using Shared.Common.Exceptions;

namespace Item.Application.Features.GetChildren;

public class GetChildrenHandler(IItemRepository itemRepository) : IRequestHandler<GetChildrenQuery, GetItemsResponseDto>
{
    public async Task<GetItemsResponseDto> Handle(GetChildrenQuery request, CancellationToken cancellationToken)
    {
        var parentFolder = await itemRepository.GetAsync(request.ParentId, request.UserId, GetItemMode.Folder, cancellationToken);
        
        if (parentFolder is null)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound, "Parent folder not found.");
        }
        
        var sortBy = parentFolder.Folder?.View.SortBy;
        var sortOrder = parentFolder.Folder?.View.SortOrder;
        var children = await itemRepository.GetChildrenAsync(request.ParentId, request.UserId, request.Query, cancellationToken);

        switch (sortBy)
        {
            case SortBy.Name:
                children = sortOrder == SortOrder.Ascending ? children.OrderBy(x => x.Name).ToList() : children.OrderByDescending(x => x.Name).ToList();
                break;
            case SortBy.Size:
                children = sortOrder == SortOrder.Ascending ? children.OrderBy(x => x.Size).ToList() : children.OrderByDescending(x => x.Size).ToList();
                break;
            case SortBy.CreatedAt:
                children = sortOrder == SortOrder.Ascending ? children.OrderBy(x => x.CreatedAt).ToList() : children.OrderByDescending(x => x.CreatedAt).ToList();
                break;
            case SortBy.LastModifiedAt:
                children = sortOrder == SortOrder.Ascending ? children.OrderBy(x => x.LastModifiedAt).ToList() : children.OrderByDescending(x => x.LastModifiedAt).ToList();
                break;
            default:
                throw new HttpResponseException(HttpStatusCode.BadRequest, "Invalid sort by value.");
        }

        return new GetItemsResponseDto
        {
            Data = children,
            NextToken = children.Count == request.Query.Limit ? children.Last().Id.ToString() : null
        };
    }
}