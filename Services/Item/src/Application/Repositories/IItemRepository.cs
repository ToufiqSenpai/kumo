namespace Item.Application.Repositories;

public interface IItemRepository
{
    public Task AddAsync(Domain.Models.Item item, CancellationToken cancellationToken);
}