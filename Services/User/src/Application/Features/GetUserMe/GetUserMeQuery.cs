using MediatR;
using User.Application.DTOs;

namespace User.Application.Features.GetUserMe;

public sealed record GetUserMeQuery(Guid UserId) : IRequest<UserDto>;