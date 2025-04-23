using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Item.Domain.Models;

public class Item
{
    [BsonId] public ObjectId Id { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public File? File { get; set; }
    public FileSystem? FileSystem { get; set; }
    public Folder? Folder { get; set; }
    public Image? Image { get; set; }
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    public required string Name { get; set; }
    public ItemReference? ParentReference { get; set; }
    public bool Root { get; set; } = false;
    public long Size { get; set; }
    [BsonRepresentation(BsonType.String)]
    public required Guid UserId { get; set; }
    public Video? Video { get; set; }
}