using Microsoft.EntityFrameworkCore;
using User.Application.Repositories;
using User.Infrastructure.Persistence;

namespace User.Infrastructure.Repositories;

public class UserRepository(UserDbContext dbContext) : IUserRepository
{
    public async Task AddAsync(Domain.Entities.User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Users.FindAsync(id, cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken)
    {
        return !await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }
}