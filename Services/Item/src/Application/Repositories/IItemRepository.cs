using Item.Application.Features.GetChildren;
using Item.Domain.Enums;
using MongoDB.Bson;

namespace Item.Application.Repositories;

public interface IItemRepository
{
    public Task AddAsync(Domain.Models.Item item, CancellationToken cancellationToken);
    
    public Task<Domain.Models.Item?> GetAsync(ObjectId id, Guid userId, GetItemMode mode, CancellationToken cancellationToken);
    
    public Task<List<Domain.Models.Item>> GetChildrenAsync(ObjectId parentId, Guid userId, GetChildrenQueryParams query, CancellationToken cancellationToken);
    
    public Task<ObjectId> GetRootFolderIdByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    
    public Task<bool> IsItemExistAsync(ObjectId id, Guid userId, GetItemMode mode, CancellationToken cancellationToken);
}