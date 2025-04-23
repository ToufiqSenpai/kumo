using MongoDB.Bson;

namespace Item.Application.Features.CopyFile;

public class CopyFileRequestDto
{
    public ObjectId ParentId { get; set; }
    public string Name { get; set; }
}