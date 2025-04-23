using MediatR;

namespace Item.Application.Features.CreateRootFolder;

public sealed record CreateRootFolderCommand(Guid UserId) : IRequest<Unit>;