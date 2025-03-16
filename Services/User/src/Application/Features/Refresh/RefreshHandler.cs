using System.Net;
using MediatR;
using Shared.Common.Exceptions;
using User.Application.Interfaces;
using User.Application.Repositories;
using User.Infrastructure.Options;

namespace User.Application.Features.Refresh;

public sealed class RefreshHandler(
    IRefreshTokenRepository refreshTokenRepository, 
    ITokenGenerator tokenGenerator,
    RefreshTokenOptions refreshTokenOptions) : IRequestHandler<RefreshCommand, RefreshResponseDto>
{
    public async Task<RefreshResponseDto> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await refreshTokenRepository.GetByTokenAsync(request.Dto.RefreshToken, cancellationToken);
        
        if (refreshToken is null || refreshToken.Expires < DateTime.UtcNow)
        {
            throw new HttpResponseException(HttpStatusCode.Unauthorized, "Refresh token is invalid or expired.");
        }

        var newRefreshToken = tokenGenerator.GenerateRefreshToken();
        var newAccessToken = tokenGenerator.GenerateAccessToken(new Domain.Entities.User { Id = refreshToken.UserId });
        
        var newRefreshTokenEntity = new Domain.Entities.RefreshToken
        {
            Token = newRefreshToken,
            UserId = refreshToken.UserId,
            Expires = DateTime.UtcNow.AddDays(refreshTokenOptions.ExpiresInDays)
        };
        await refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
        await refreshTokenRepository.DeleteByTokenAsync(refreshToken.Token, cancellationToken);

        return new RefreshResponseDto(newRefreshToken, newAccessToken);
    }
}