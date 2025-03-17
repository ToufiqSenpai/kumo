using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Item.Domain.Models;

public class Image
{
    public double? Aperture { get; set; }
    public string? CameraMake { get; set; }
    public string? CameraModel { get; set; }
    public string? ColorSpace { get; set; }
    public string? ExposureMode { get; set; }
    public double? ExposureTime { get; set; }
    public double? FocalLength { get; set; }
    public int? Iso { get; set; }
    public string? Lens { get; set; }
    public Location? Location { get; set; }
    public double? MaxApertureValue { get; set; }
    public string? MeteringMode { get; set; }
    public double? SubjectDistance { get; set; }
    public string? WhiteBalance { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}