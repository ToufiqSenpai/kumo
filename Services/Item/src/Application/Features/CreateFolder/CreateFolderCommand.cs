using MediatR;
using MongoDB.Bson;

namespace Item.Application.Features.CreateFolder;

public sealed record CreateFolderCommand(CreateFolderRequestDto Dto, ObjectId ParentId, Guid UserId) : IRequest<Domain.Models.Item> {}