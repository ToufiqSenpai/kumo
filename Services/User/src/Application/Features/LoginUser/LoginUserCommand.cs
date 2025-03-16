using MediatR;

namespace User.Application.Features.LoginUser;

public sealed record LoginUserCommand(LoginUserRequestDto Dto) : IRequest<LoginUserResponseDto>;