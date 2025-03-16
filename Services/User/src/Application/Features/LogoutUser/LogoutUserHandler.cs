using MediatR;
using User.Application.Repositories;

namespace User.Application.Features.LogoutUser;

public sealed class LogoutUserHandler(IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<LogoutUserCommand, Unit>
{
    public async Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await refreshTokenRepository.DeleteByTokenAsync(request.Dto.RefreshToken, cancellationToken);
        
        return Unit.Value;
    }
}