using MediatR;
using MongoDB.Bson;

namespace Item.Application.Features.GetChildren;

public record GetChildrenQuery(ObjectId ParentId, Guid UserId, GetChildrenQueryParams Query) : IRequest<GetItemsResponseDto>;