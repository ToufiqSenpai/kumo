using MediatR;
using MongoDB.Bson;

namespace Item.Application.Features.GetItem;

public sealed record GetItemQuery(ObjectId ItemId, Guid UserId) : IRequest<Domain.Models.Item>;