using FluentAssertions;
using Item.Infrastructure.Utils;
using MongoDB.Bson;
using Xunit.Abstractions;

namespace Item.Tests.Unit.Infrastructure.Utils;

public class NextTokenHelperTest(ITestOutputHelper output)
{
    private readonly ObjectId _objectId = ObjectId.GenerateNewId();
    
    [Fact]
    public void EncodeNextToken_ShouldReturnBase64String()
    {
        // Act
        var result = NextTokenHelper.EncodeNextToken(_objectId);
        
        // Assert
        result.Should().NotBeNullOrEmpty();
        
        output.WriteLine(result);
    }
    
    [Fact]
    public void DecodeNextToken_ShouldReturnObjectId()
    {
        // Arrange
        var token = NextTokenHelper.EncodeNextToken(_objectId);
        
        // Act
        var result = NextTokenHelper.DecodeNextToken(token);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ObjectId>();
        result.Should().Be(_objectId);
        
        output.WriteLine(result.ToString());
    }
}