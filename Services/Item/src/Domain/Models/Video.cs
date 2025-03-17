namespace Item.Domain.Models;

public class Video
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