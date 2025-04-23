using System.Reflection;
using File.Application.Interfaces;
using File.Infrastructure.Services;
using FluentAssertions;
using MassTransit;
using Moq;
using Shared.Events.Item;
using Xunit.Abstractions;

namespace File.Tests.Unit.Infrastructure.Services;

public class FileMetadataExtractorTest(ITestOutputHelper output)
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    // private readonly Mock<IFileHasher> _fileHasherMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

    [Fact]
    public async Task GetFileMimeTypeAsync_ShouldReturn_AudioMpegMimeType()
    {
        // Arrange
        var fileId = "test-file-id";
        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName!, "Assets", "Audio", "NamelessFaces.mp3");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        
        // Act
        var fileMetadataExtractor = new FileMetadataExtractor(_fileStorageServiceMock.Object, new FileHasher(), _publishEndpointMock.Object);
        var mimeType = await fileMetadataExtractor.GetFileMimeTypeAsync(fileId, CancellationToken.None);
        
        // Assert
        mimeType.Should().Be("audio/mpeg3");
    }
    
    [Fact]
    public async Task GetFileMimeTypeAsync_ShouldReturn_ImagePngMimeType()
    {
        // Arrange
        var fileId = "test-file-id";
        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName!, "Assets", "Image", "nilou-heart-pose.png");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        
        // Act
        var fileMetadataExtractor = new FileMetadataExtractor(_fileStorageServiceMock.Object, new FileHasher(), _publishEndpointMock.Object);
        var mimeType = await fileMetadataExtractor.GetFileMimeTypeAsync(fileId, CancellationToken.None);
        
        // Assert
        mimeType.Should().Be("image/png");
    }
    
    [Fact]
    public async Task GetFileMimeTypeAsync_ShouldThrow_InvalidOperationException_WhenMimeTypeCannotBeDetermined()
    {
        // Arrange
        var fileId = "test-file-id";
        var random = new Random();
        var buffer = new byte[5 * 1024];
        random.NextBytes(buffer);
        using var fileStream = new MemoryStream(buffer);
        
        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        
        // Act
        var fileMetadataExtractor = new FileMetadataExtractor(_fileStorageServiceMock.Object, new FileHasher(), _publishEndpointMock.Object);
        var mimeType = await fileMetadataExtractor.GetFileMimeTypeAsync(fileId, CancellationToken.None);
        
        // Assert
        mimeType.Should().Be("application/octet-stream");
    }

    [Fact]
    public async Task ExtractFileMetadataAsync_ShouldExtractMetadata_WhenFileIsHeicImage()
    {
        // Arrange
        var fileId = "test-file-id";
        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName!, "Assets", "Image", "image.HEIC");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        SetFileMetadata? fileMetadata = null;
        
        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<SetFileMetadata, CancellationToken>((metadata, _) => fileMetadata = metadata);
        
        // Act
        var fileMetadataExtractor = new FileMetadataExtractor(_fileStorageServiceMock.Object, new FileHasher(), _publishEndpointMock.Object);
        var method = typeof(FileMetadataExtractor).GetMethod("ExtractFileMetadataAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task)method?.Invoke(fileMetadataExtractor, new object[] { fileId, "image/heic" });
        await task;
        
        // Assert
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()));
        fileMetadata.Should().NotBeNull();
        fileMetadata.ImageMetadata.Should().NotBeNull();

        fileMetadata.ImageMetadata.CameraMake.Should().Be("Apple");
        fileMetadata.ImageMetadata.CameraModel.Should().Be("iPhone 13 Pro");
        fileMetadata.ImageMetadata.Lens.Should().Be("iPhone 13 Pro back triple camera 5.7mm f/1.5");
    }
    
    [Fact]
    public async Task ExtractFileMetadataAsync_ShouldExtractMetadata_WhenFileIsPngImage()
    {
        // Arrange
        var fileId = "test-file-id";
        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName!, "Assets", "Image", "nilou-heart-pose.png");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        SetFileMetadata? fileMetadata = null;
        
        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<SetFileMetadata, CancellationToken>((metadata, _) => fileMetadata = metadata);
        
        // Act
        var fileMetadataExtractor = new FileMetadataExtractor(_fileStorageServiceMock.Object, new FileHasher(), _publishEndpointMock.Object);
        var method = typeof(FileMetadataExtractor).GetMethod("ExtractFileMetadataAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task)method?.Invoke(fileMetadataExtractor, new object[] { fileId, "image/png" });
        await task;
        
        // Assert
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()));
        fileMetadata.Should().NotBeNull();
        fileMetadata.ImageMetadata.Should().NotBeNull();
    }
    
    [Fact]
    public async Task ExtractFileMetadataAsync_ShouldExtractMetadata_WhenFileIsWebpImage()
    {
        // Arrange
        var fileId = "test-file-id";
        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName!, "Assets", "Image", "image.webp");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        SetFileMetadata? fileMetadata = null;
        
        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<SetFileMetadata, CancellationToken>((metadata, _) => fileMetadata = metadata);
        
        // Act
        var fileMetadataExtractor = new FileMetadataExtractor(_fileStorageServiceMock.Object, new FileHasher(), _publishEndpointMock.Object);
        var method = typeof(FileMetadataExtractor).GetMethod("ExtractFileMetadataAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task)method?.Invoke(fileMetadataExtractor, new object[] { fileId, "image/webp" });
        await task;
        
        // Assert
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()));
        fileMetadata.Should().NotBeNull();
        fileMetadata.ImageMetadata.Should().NotBeNull();
    }
    
    [Fact]
    public async Task ExtractFileMetadataAsync_ShouldExtractMetadata_WhenFileIsVideo()
    {
        // Arrange
        var fileId = "test-file-id";
        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName!, "Assets", "Video", "mv-test.mp4");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        SetFileMetadata? fileMetadata = null;
        
        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<SetFileMetadata, CancellationToken>((metadata, _) => fileMetadata = metadata);
        
        // Act
        var fileMetadataExtractor = new FileMetadataExtractor(_fileStorageServiceMock.Object, new FileHasher(), _publishEndpointMock.Object);
        // await fileMetadataExtractor.ExtractFileMetadataAsync(fileId, "image/png");
        var method = typeof(FileMetadataExtractor).GetMethod("ExtractFileMetadataAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task)method?.Invoke(fileMetadataExtractor, new object[] { fileId, "video/mpeg" });
        await task;
        
        
        // Assert
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()));
        fileMetadata.Should().NotBeNull();
        fileMetadata.VideoMetadata.Should().NotBeNull();
        
        output.WriteLine($"SHA1: {fileMetadata?.Sha1}");
        output.WriteLine($"CRC32: {fileMetadata?.Crc32}");
        output.WriteLine($"VideoMetadata.AudioBitsPerSample: {fileMetadata?.VideoMetadata?.AudioBitsPerSample?.ToString()}");
        output.WriteLine($"VideoMetadata.AudioChannels: {fileMetadata?.VideoMetadata?.AudioChannels?.ToString()}");
        output.WriteLine($"VideoMetadata.AudioFormat: {fileMetadata?.VideoMetadata?.AudioFormat}");
        output.WriteLine($"VideoMetadata.AudioSamplesPerSecond: {fileMetadata?.VideoMetadata?.AudioSamplesPerSecond?.ToString()}");
        output.WriteLine($"VideoMetadata.Bitrate: {fileMetadata?.VideoMetadata?.Bitrate?.ToString()}");
        output.WriteLine($"VideoMetadata.Duration: {fileMetadata?.VideoMetadata?.Duration?.ToString()}");
        output.WriteLine($"VideoMetadata.FourCC: {fileMetadata?.VideoMetadata?.FourCC}");
        output.WriteLine($"VideoMetadata.FrameRate: {fileMetadata?.VideoMetadata?.FrameRate?.ToString()}");
        output.WriteLine($"VideoMetadata.Height: {fileMetadata?.VideoMetadata?.Height?.ToString()}");
        output.WriteLine($"VideoMetadata.Width: {fileMetadata?.VideoMetadata?.Width?.ToString()}");
    }
    
    [Fact]
    public async Task ExtractFileMetadataAsync_ShouldExtractMetadata_WhenFileIsAudio()
    {
        // Arrange
        var fileId = "test-file-id";
        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName!, "Assets", "Audio", "Sign.mp3");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        SetFileMetadata? fileMetadata = null;
        
        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<SetFileMetadata, CancellationToken>((metadata, _) => fileMetadata = metadata);
        
        // Act
        var fileMetadataExtractor = new FileMetadataExtractor(_fileStorageServiceMock.Object, new FileHasher(), _publishEndpointMock.Object);
        var method = typeof(FileMetadataExtractor).GetMethod("ExtractFileMetadataAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task)method?.Invoke(fileMetadataExtractor, new object[] { fileId, "audio/mpeg3" });
        await task;
        
        // Assert
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<SetFileMetadata>(), It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.GetFileAsync(fileId, It.IsAny<CancellationToken>()));
        fileMetadata.Should().NotBeNull();
        fileMetadata.AudioMetadata.Should().NotBeNull();

        var audioMetadata = fileMetadata.AudioMetadata;

        audioMetadata.Album.Should().Be("Sign");
        audioMetadata.AlbumArtist.Should().Be("Lilas");
        audioMetadata.Artist.Should().Be("Lilas");
        audioMetadata.Bitrate.Should().Be(160);
        audioMetadata.Composers.Should().BeNull();
        audioMetadata.Copyright.Should().Be("(C) 2024 Lilas Ikuta");
        audioMetadata.Disc.Should().Be(1);
        audioMetadata.DiscCount.Should().Be(1);
        audioMetadata.Duration.Should().NotBeNull();
        audioMetadata.Genre.Should().Be("J-Pop");
        audioMetadata.Title.Should().Be("Sign");
        audioMetadata.Track.Should().Be(1);
        audioMetadata.TrackCount.Should().Be(1);
        audioMetadata.Year.Should().Be(2024);
    }
}