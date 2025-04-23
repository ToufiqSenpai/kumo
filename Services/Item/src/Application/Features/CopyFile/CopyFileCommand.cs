using MediatR;
using MongoDB.Bson;

namespace Item.Application.Features.CopyFile;

public record CopyFileCommand(CopyFileRequestDto Dto, ObjectId ItemId) : IRequest<string>;