using System.Net;
using MediatR;
using Shared.Common.Exceptions;
using User.Application.Interfaces;
using User.Application.Repositories;
using User.Domain.Entities;
using User.Infrastructure.Options;

namespace User.Application.Features.LoginUser;

public sealed class LoginUserHandler(
    IUserRepository userRepository, 
    IRefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    RefreshTokenOptions refreshTokenOptions) : IRequestHandler<LoginUserCommand, LoginUserResponseDto>
{
    public async Task<LoginUserResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Dto.Email, cancellationToken);
        
        if (user == null)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound, "User not found");
        }
        
        string refreshToken = tokenGenerator.GenerateRefreshToken();
        string accessToken = tokenGenerator.GenerateAccessToken(user);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(refreshTokenOptions.ExpiresInDays),
            UserId = user.Id,
        };
        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        return new LoginUserResponseDto(refreshToken, accessToken);
    }
}