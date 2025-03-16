using Microsoft.EntityFrameworkCore;
using User.Application.Repositories;
using User.Domain.Entities;
using User.Infrastructure.Persistence;

namespace User.Infrastructure.Repositories;

public class RefreshTokenRepository(UserDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        dbContext.RefreshTokens.Add(refreshToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public async Task DeleteByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var refreshToken = await dbContext.RefreshTokens.AnyAsync(x => x.Token == token, cancellationToken);

        if (refreshToken)
        {
            dbContext.RefreshTokens.Remove(new RefreshToken { Token = token });
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}