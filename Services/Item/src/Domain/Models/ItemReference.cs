using MongoDB.Bson;

namespace Item.Domain.Models;

public class ItemReference
{
    public required ObjectId Id { get; set; }
    
    public required string Name { get; set; }
    
    public required string Path { get; set; }
}