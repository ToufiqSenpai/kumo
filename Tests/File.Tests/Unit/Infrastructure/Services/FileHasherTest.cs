using File.Infrastructure.Services;
using FluentAssertions;
using Xunit.Abstractions;

namespace File.Tests.Unit.Infrastructure.Services;

public class FileHasherTest(ITestOutputHelper output)
{
    [Fact]
    public void GetSha1Hash_ValidStream_ReturnsCorrectHash()
    {
        // Arrange
        var fileHasher = new FileHasher();
        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName!, "Assets", "Audio", "NamelessFaces.mp3");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        // Act
        var hash = fileHasher.GetSha1Hash(fileStream);

        // Assert
        hash.Should().NotBeEmpty();
        
        output.WriteLine(hash);
    }
    
    [Fact]
    public void GetCRC32Hash_ValidStream_ReturnsCorrectHash()
    {
        // Arrange
        var fileHasher = new FileHasher();
        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName!, "Assets", "Audio", "NamelessFaces.mp3");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        // Act
        var hash = fileHasher.GetCrc32Hash(fileStream);

        // Assert
        hash.Should().NotBeEmpty();
        
        output.WriteLine(hash);
    }
}