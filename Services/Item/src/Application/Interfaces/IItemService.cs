using Item.Domain.Models;
using MongoDB.Bson;

namespace Item.Application.Interfaces;

public interface IItemService
{
    public Task<ItemReference> CreateParentReferenceAsync(ObjectId parentId, Guid userId, CancellationToken cancellationToken);
}