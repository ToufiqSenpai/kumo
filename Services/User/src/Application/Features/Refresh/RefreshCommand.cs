using MediatR;

namespace User.Application.Features.Refresh;

public sealed record RefreshCommand(RefreshRequestDto Dto) : IRequest<RefreshResponseDto>;