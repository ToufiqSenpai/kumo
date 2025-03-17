using MediatR;
using MongoDB.Bson;

namespace Item.Application.Features.CreateFile;

public record CreateFileCommand(CreateFileRequestDto Dto, ObjectId parentId) : IRequest<ObjectId>;