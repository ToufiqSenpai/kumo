using User.Domain.Entities;

namespace User.Application.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task DeleteByTokenAsync(string token, CancellationToken cancellationToken);
}