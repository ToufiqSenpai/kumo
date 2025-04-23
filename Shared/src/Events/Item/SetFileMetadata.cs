namespace Shared.Events.Item;

public class SetFileMetadata
{
    public string Sha1 { get; set; } = string.Empty;
    public string Crc32 { get; set; } = string.Empty;
    public ImageMetadata? ImageMetadata { get; set; }
    public VideoMetadata? VideoMetadata { get; set; }
    public AudioMetadata? AudioMetadata { get; set; }
}

public class ImageMetadata
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
    public LocationMetadata? Location { get; set; }
    public double? MaxApertureValue { get; set; }
    public string? MeteringMode { get; set; }
    public double? SubjectDistance { get; set; }
    public string? WhiteBalance { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}

public class LocationMetadata
{
    public double? Altitude { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}


public class VideoMetadata
{
    public int? AudioBitsPerSample { get; set; }
    public int? AudioChannels { get; set; }
    public string? AudioFormat { get; set; }
    public int? AudioSamplesPerSecond { get; set; }
    public int? Bitrate { get; set; }
    public long? Duration { get; set; }
    public string? FourCC { get; set; }
    public double? FrameRate { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
}

public class AudioMetadata
{
    public string? Album { get; set; }
    
    public string? AlbumArtist { get; set; }
    
    public string? Artist { get; set; }
    
    public long? Bitrate { get; set; }
    
    public string? Composers { get; set; }
    
    public string? Copyright { get; set; }
    
    public short? Disc { get; set; }
    
    public short? DiscCount { get; set; }
    
    public long? Duration { get; set; }
    
    public string? Genre { get; set; }
    
    public string? Title { get; set; }
    
    public int? Track { get; set; }
    
    public int? TrackCount { get; set; }
    
    public int? Year { get; set; }
}