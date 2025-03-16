using MediatR;

namespace User.Application.Features.LogoutUser;

public sealed record LogoutUserCommand(LogoutUserRequestDto Dto) : IRequest<Unit>;