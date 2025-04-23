using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using File.Application.Interfaces;
using MassTransit;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.WebP;
using MimeDetective;
using MimeDetective.Definitions;
using Shared.Events.Item;

namespace File.Infrastructure.Services;

public class FileMetadataExtractor(
    IFileStorageService fileStorageService,
    IFileHasher fileHasher,
    IPublishEndpoint publishEndpoint)
    : IFileMetadataExtractor
{
    private readonly object _lock = new();
    private readonly ConcurrentQueue<(string fileId, string mimeType)> _metadataExtractionQueue = new();

    private readonly IContentInspector _mimeTypeInspector = new ContentInspectorBuilder
    {
        // Definitions = new ExhaustiveBuilder
        // {
        //     UsageType = UsageType.PersonalNonCommercial
        // }.Build().TrimCategories().TrimDescription().TrimMeta().ToImmutableArray()
        Definitions = DefaultDefinitions.All()
    }.Build();

    private bool _isProcessingMetadataExtraction;

    public async Task<string> GetFileMimeTypeAsync(string fileId, CancellationToken cancellationToken)
    {
        var fileStream = await fileStorageService.GetFileAsync(fileId, cancellationToken);
        var mimeTypeResult = _mimeTypeInspector.Inspect(fileStream);

        if (mimeTypeResult == null || mimeTypeResult.ByMimeType().IsEmpty) return "application/octet-stream";

        return mimeTypeResult.ByMimeType().First().MimeType.ToLower();
    }

    public void EnqueueFileMetadataExtractionAsync(string fileId, string mimeType, CancellationToken cancellationToken)
    {
        _metadataExtractionQueue.Enqueue((fileId, mimeType));

        lock (_lock)
        {
            if (!_isProcessingMetadataExtraction)
            {
                _isProcessingMetadataExtraction = true;
                _ = ProcessFileMetadataExtractionAsync();
            }
        }
    }

    private async Task ProcessFileMetadataExtractionAsync()
    {
        while (_metadataExtractionQueue.TryDequeue(out var fileMetadata))
        {
            await ExtractFileMetadataAsync(fileMetadata.fileId, fileMetadata.mimeType);
        }

        lock (_lock)
        {
            _isProcessingMetadataExtraction = false;
        }
    }

    private async Task ExtractFileMetadataAsync(string fileId, string mimeType)
    {
        var fileStream = await fileStorageService.GetFileAsync(fileId, CancellationToken.None);
        fileStream.Position = 0;

        var fileMetadata = new SetFileMetadata();
        
        fileMetadata.Crc32 = fileHasher.GetCrc32Hash(fileStream);
        fileStream.Position = 0;

        fileMetadata.Sha1 = fileHasher.GetSha1Hash(fileStream);
        fileStream.Position = 0;

        if (mimeType.StartsWith("image"))
            fileMetadata.ImageMetadata = GetImageMetadata(fileStream);
        else if (mimeType.StartsWith("video"))
            fileMetadata.VideoMetadata = await GetVideoMetadata(fileStream);
        else if (mimeType.StartsWith("audio")) fileMetadata.AudioMetadata = await GetAudioMetadata(fileStream);

        await publishEndpoint.Publish(fileMetadata);
    }

    private ImageMetadata GetImageMetadata(Stream fileStream)
    {
        var imageMetadata = new ImageMetadata();

        var directories = ImageMetadataReader.ReadMetadata(fileStream);
        var exif = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
        var jpeg = directories.OfType<JpegDirectory>().FirstOrDefault();
        var png = directories.OfType<PngDirectory>().FirstOrDefault();
        var webp = directories.OfType<WebPDirectory>().FirstOrDefault();

        if (exif != null)
        {
            if (exif.ContainsTag(ExifDirectoryBase.TagFNumber))
                imageMetadata.Aperture = exif.GetDouble(ExifDirectoryBase.TagFNumber);

            if (exif.ContainsTag(ExifDirectoryBase.TagExposureTime))
                imageMetadata.ExposureTime = exif.GetDouble(ExifDirectoryBase.TagExposureTime);

            if (exif.ContainsTag(ExifDirectoryBase.TagFocalLength))
                imageMetadata.FocalLength = exif.GetDouble(ExifDirectoryBase.TagFocalLength);

            if (exif.ContainsTag(ExifDirectoryBase.TagIsoEquivalent))
                imageMetadata.Iso = exif.GetInt32(ExifDirectoryBase.TagIsoEquivalent);

            if (exif.ContainsTag(ExifDirectoryBase.TagMeteringMode))
                imageMetadata.MeteringMode = exif.GetDescription(ExifDirectoryBase.TagMeteringMode);

            if (exif.ContainsTag(ExifDirectoryBase.TagWhiteBalance))
                imageMetadata.WhiteBalance = exif.GetDescription(ExifDirectoryBase.TagWhiteBalance);

            if (exif.ContainsTag(ExifDirectoryBase.TagMaxAperture))
                imageMetadata.MaxApertureValue = exif.GetDouble(ExifDirectoryBase.TagMaxAperture);

            if (exif.ContainsTag(ExifDirectoryBase.TagSubjectDistance))
                imageMetadata.SubjectDistance = exif.GetDouble(ExifDirectoryBase.TagSubjectDistance);

            if (exif.ContainsTag(ExifDirectoryBase.TagColorSpace))
                imageMetadata.ColorSpace = exif.GetDescription(ExifDirectoryBase.TagColorSpace);

            if (exif.ContainsTag(ExifDirectoryBase.TagExposureMode))
                imageMetadata.ExposureMode = exif.GetDescription(ExifDirectoryBase.TagExposureMode);

            if (exif.ContainsTag(ExifDirectoryBase.TagLensModel))
                imageMetadata.Lens = exif.GetDescription(ExifDirectoryBase.TagLensModel);

            if (exif.ContainsTag(ExifDirectoryBase.TagExifImageWidth))
                imageMetadata.Width = exif.GetInt32(ExifDirectoryBase.TagExifImageWidth);

            if (exif.ContainsTag(ExifDirectoryBase.TagExifImageHeight))
                imageMetadata.Height = exif.GetInt32(ExifDirectoryBase.TagExifImageHeight);
        }

        if (ifd0 != null)
        {
            if (ifd0.ContainsTag(ExifDirectoryBase.TagMake))
                imageMetadata.CameraMake = ifd0.GetDescription(ExifDirectoryBase.TagMake);

            if (ifd0.ContainsTag(ExifDirectoryBase.TagModel))
                imageMetadata.CameraModel = ifd0.GetDescription(ExifDirectoryBase.TagModel);
        }

        if (jpeg != null)
        {
            imageMetadata.Width = jpeg.GetImageWidth();
            imageMetadata.Height = jpeg.GetImageHeight();
        }

        if (png != null)
        {
            if (png.ContainsTag(PngDirectory.TagImageWidth))
                imageMetadata.Width = png.GetInt32(PngDirectory.TagImageWidth);

            if (png.ContainsTag(PngDirectory.TagImageHeight))
                imageMetadata.Width = png.GetInt32(PngDirectory.TagImageHeight);
        }

        if (webp != null)
        {
            if (webp.ContainsTag(WebPDirectory.TagImageWidth))
                imageMetadata.Width = webp.GetInt32(WebPDirectory.TagImageWidth);

            if (webp.ContainsTag(WebPDirectory.TagImageHeight))
                imageMetadata.Width = webp.GetInt32(WebPDirectory.TagImageHeight);
        }

        if (gps != null)
        {
            var location = new LocationMetadata
            {
                Latitude = gps.GetGeoLocation()?.Latitude,
                Longitude = gps.GetGeoLocation()?.Longitude
            };

            if (gps.TryGetSingle(GpsDirectory.TagAltitude, out var altitude)) location.Altitude = altitude;

            imageMetadata.Location = location;
        }

        return imageMetadata;
    }

    private async Task<VideoMetadata> GetVideoMetadata(Stream fileStream)
    {
        var videoMetadata = new VideoMetadata();
        var mediaMetadata = await GetFfprobeMetadataAsync(fileStream);

        using var doc = JsonDocument.Parse(mediaMetadata);
        var root = doc.RootElement;
        var format = root.GetProperty("format");

        if (format.TryGetProperty("duration", out var durationProp) &&
            double.TryParse(durationProp.GetString(), out var duration))
            videoMetadata.Duration = (long)(duration * 1000); // convert to ms

        foreach (var stream in root.GetProperty("streams").EnumerateArray())
        {
            var codecType = stream.GetProperty("codec_type").GetString();

            if (codecType == "video")
            {
                videoMetadata.Width = stream.TryGetProperty("width", out var w) ? w.GetInt32() : null;
                videoMetadata.Height = stream.TryGetProperty("height", out var h) ? h.GetInt32() : null;
                videoMetadata.FourCC = stream.TryGetProperty("codec_tag_string", out var tag)
                    ? tag.GetString()?.ToUpper()
                    : null;

                if (stream.TryGetProperty("r_frame_rate", out var frProp))
                {
                    var fr = frProp.GetString(); // e.g., "30/1"
                    if (fr != null && fr.Contains("/"))
                    {
                        var parts = fr.Split('/');
                        if (double.TryParse(parts[0], out var num) && double.TryParse(parts[1], out var den) &&
                            den != 0)
                            videoMetadata.FrameRate = Math.Round(num / den);
                    }
                }

                if (stream.TryGetProperty("bit_rate", out var brProp) &&
                    int.TryParse(brProp.GetString(), out var bitrate))
                    videoMetadata.Bitrate = bitrate;
            }
            else if (codecType == "audio")
            {
                videoMetadata.AudioFormat =
                    stream.TryGetProperty("codec_name", out var af) ? af.GetString()?.ToUpper() : null;
                videoMetadata.AudioChannels = stream.TryGetProperty("channels", out var ch) ? ch.GetInt32() : null;

                if (stream.TryGetProperty("sample_rate", out var sr) && int.TryParse(sr.GetString(), out var srInt))
                    videoMetadata.AudioSamplesPerSecond = srInt;

                videoMetadata.AudioBitsPerSample =
                    stream.TryGetProperty("bits_per_sample", out var bps) ? bps.GetInt32() : null;
            }
        }

        return videoMetadata;
    }

    private async Task<AudioMetadata?> GetAudioMetadata(Stream fileStream)
    {
        var audioMetadata = new AudioMetadata();
        var mediaMetadata = await GetFfprobeMetadataAsync(fileStream);

        using var doc = JsonDocument.Parse(mediaMetadata);
        var root = doc.RootElement;
        var audioStream = root.GetProperty("streams").EnumerateArray()
            .FirstOrDefault(s => s.GetProperty("codec_type").GetString() == "audio");
        var audioTags = root.GetProperty("format").GetProperty("tags");

        if (audioStream.ValueKind == JsonValueKind.Undefined) return null;

        audioMetadata.Album = audioTags.TryGetProperty("album", out var album) ? album.GetString() : null;
        audioMetadata.AlbumArtist = audioTags.TryGetProperty("album_artist", out var albumArtist)
            ? albumArtist.GetString()
            : null;
        audioMetadata.Artist = audioTags.TryGetProperty("artist", out var artist) ? artist.GetString() : null;
        audioMetadata.Bitrate = audioStream.TryGetProperty("bit_rate", out var bitrate)
            ? long.TryParse(bitrate.GetString(), out var bitrateLong) ? bitrateLong / 1000 : null
            : null;
        audioMetadata.Composers = audioTags.TryGetProperty("composers", out var composer) ? composer.GetString() : null;
        audioMetadata.Copyright =
            audioTags.TryGetProperty("copyright", out var copyright) ? copyright.GetString() : null;

        if (audioTags.TryGetProperty("disc", out var disc))
        {
            var discParts = disc.GetString()?.Split('/');

            if (discParts != null && discParts.Length == 2)
            {
                audioMetadata.Disc = short.TryParse(discParts[0], out var discNumber) ? discNumber : null;
                audioMetadata.DiscCount = short.TryParse(discParts[1], out var discCount) ? discCount : null;
            }
        }

        audioMetadata.Duration = audioStream.TryGetProperty("duration", out var duration)
            ? double.TryParse(duration.GetString(), out var durationDouble) ? (long)durationDouble * 1000 : null
            : null;
        audioMetadata.Genre = audioTags.TryGetProperty("genre", out var genre) ? genre.GetString() : null;
        audioMetadata.Title = audioTags.TryGetProperty("title", out var title) ? title.GetString() : null;

        if (audioTags.TryGetProperty("track", out var track))
        {
            var trackParts = track.GetString()?.Split('/');

            if (trackParts != null && trackParts.Length == 2)
            {
                audioMetadata.Track = int.TryParse(trackParts[0], out var trackNumber) ? trackNumber : null;
                audioMetadata.TrackCount = int.TryParse(trackParts[1], out var trackCount) ? trackCount : null;
            }
        }

        audioMetadata.Year = audioTags.TryGetProperty("year", out var year)
            ? int.TryParse(year.GetString(), out var yearInt) ? yearInt : null
            : null;
        audioMetadata.Year = audioTags.TryGetProperty("date", out var date)
            ? int.TryParse(date.GetString(), out var dateInt) ? dateInt : null
            : null;

        return audioMetadata;
    }

    public async Task<string> GetFfprobeMetadataAsync(Stream fileStream)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = "-v error -show_format -show_streams -print_format json -i -",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = Process.Start(processStartInfo);

        if (process is null)
            throw new InvalidOperationException("Unable to open ffprobe process.");
        
        var inputTask = Task.Run(async () =>
        {
            try
            {
                await fileStream.CopyToAsync(process.StandardInput.BaseStream);
            }
            catch (IOException)
            {
                // Ignore broken pipe if ffprobe has already exited
            }
            finally
            {
                process.StandardInput.Close();
            }
        });
        
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        
        await Task.WhenAll(inputTask, outputTask, errorTask);
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"ffprobe exited with code {process.ExitCode}. Error: {await errorTask}");
        
        return await outputTask;
    }
}