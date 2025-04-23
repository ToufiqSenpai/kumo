using MediatR;
using MongoDB.Bson;

namespace Item.Application.Features.CreateFile;

public sealed record CreateFileCommand(CreateFileRequestDto Dto, ObjectId ParentId, Guid UserId) : IRequest<CreateFileResponseDto>;