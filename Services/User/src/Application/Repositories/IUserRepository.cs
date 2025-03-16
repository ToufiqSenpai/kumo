namespace User.Application.Repositories;

public interface IUserRepository
{
    Task AddAsync(Domain.Entities.User user, CancellationToken cancellationToken);
    Task<Domain.Entities.User?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken);
}