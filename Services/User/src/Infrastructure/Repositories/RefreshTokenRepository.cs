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
        var refreshTokens = await dbContext.RefreshTokens
            .Where(x => x.Token == token)
            .ToListAsync(cancellationToken);
        
        dbContext.RefreshTokens.RemoveRange(refreshTokens);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByExpiredTokensAsync(CancellationToken cancellationToken)
    {
        var expiredTokens =
            from refreshToken in dbContext.RefreshTokens
            where refreshToken.Expires <= DateTime.UtcNow
            select refreshToken;
        
        dbContext.RefreshTokens.RemoveRange(expiredTokens);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}