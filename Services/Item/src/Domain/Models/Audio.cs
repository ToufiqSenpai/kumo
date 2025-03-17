using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Item.Domain.Models;

public class Audio
{
    public string? Album { get; set; }
    
    public string? AlbumArtist { get; set; }
    
    public string? Artist { get; set; }
    
    [BsonRepresentation(BsonType.Int64)]
    public long? Bitrate { get; set; }
    
    public string? Composers { get; set; }
    
    public string? Copyright { get; set; }
    
    [BsonRepresentation(BsonType.Int32)]
    public short? Disc { get; set; }
    
    [BsonRepresentation(BsonType.Int32)]
    public short? DiscCount { get; set; }
    
    [BsonRepresentation(BsonType.Int64)]
    public long? Duration { get; set; }
    
    public string? Genre { get; set; }
    
    public string? Title { get; set; }
    
    public int? Track { get; set; }
    
    public int? TrackCount { get; set; }
    
    public int? Year { get; set; }
}