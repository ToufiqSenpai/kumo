using MediatR;

namespace Item.Application.Features.CreateRootFolder;

public record CreateRootFolderCommand(Guid UserId) : IRequest<Unit>;