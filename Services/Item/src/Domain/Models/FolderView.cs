using Item.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Item.Domain.Models;

public class FolderView
{
    [BsonRepresentation(BsonType.Int32)]
    public SortBy SortBy { get; set; } = SortBy.Name;
    [BsonRepresentation(BsonType.Int32)]
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
}