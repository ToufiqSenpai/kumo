using System.Net;
using Item.Application.Repositories;
using Item.Domain.Enums;
using MediatR;
using Shared.Common.Exceptions;

namespace Item.Application.Features.GetItem;

public sealed class GetItemHandler(IItemRepository itemRepository) : IRequestHandler<GetItemQuery, Domain.Models.Item>
{
    public async Task<Domain.Models.Item> Handle(GetItemQuery request, CancellationToken cancellationToken)
    {
        var item = await itemRepository.GetAsync(request.ItemId, request.UserId, GetItemMode.All, cancellationToken);

        if (item is null)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound, "Item not found.");
        }
        
        return item;
    }
}