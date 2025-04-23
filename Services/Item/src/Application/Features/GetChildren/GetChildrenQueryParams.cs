using MongoDB.Bson;

namespace Item.Application.Features.GetChildren;

public class GetChildrenQueryParams
{
    public ObjectId? NextToken { get; set; }
    public uint? Limit { get; set; } = 200;
}