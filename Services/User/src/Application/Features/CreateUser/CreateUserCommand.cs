using MediatR;

namespace User.Application.Features.CreateUser;

public sealed record CreateUserCommand(CreateUserRequestDto Dto) : IRequest<CreateUserResponseDto>;