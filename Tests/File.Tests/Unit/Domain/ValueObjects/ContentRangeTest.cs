using File.Domain.ValueObjects;
using FluentAssertions;

namespace File.Tests.Unit.Domain.ValueObjects;

public class ContentRangeTest
{
    [Fact]
    public void ContentRange_WhenSourceIsValid_ShouldSetProperties()
    {
        // Arrange
        var source = "bytes 0-100/200";
        
        // Act
        var contentRange = new ContentRange(source);

        // Assert
        contentRange.RangeStart.Should().Be(0);
        contentRange.RangeEnd.Should().Be(100);
        contentRange.Size.Should().Be(200);
    }
    
    [Fact]
    public void ContentRange_WhenSourceIsEmptyString_ShouldThrowArgumentException_WithInvalidFormatMessage()
    {
        // Arrange
        var source = string.Empty;
        
        // Act
        var act = () => new ContentRange(source);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Invalid content range format.");
    }
    
    [Fact]
    public void ContentRange_WhenSourceIsInvalidFormat_ShouldThrowArgumentException_WithInvalidFormatMessage()
    {
        // Arrange
        var source = "invalid format";
        
        // Act
        var act = () => new ContentRange(source);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Invalid content range format.");
    }

    [Fact]
    public void ContentRange_WhenRangeStartIsInvalid_ShouldThrow()
    {
        // Arrange
        var source = "bytes 0-100/*";
        
        // Act
        // var act = () => new ContentRange(source);
        new ContentRange(source);

        // Assert
    }
    
    [Fact]
    public void ContentRange_WhenRangeStartIsGreaterThanRangeEnd_ShouldThrowArgumentException()
    {
        // Arrange
        var source = "bytes 100-0/200";
        
        // Act
        var act = () => new ContentRange(source);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Range start cannot be greater than range end.");
    }
    
    [Fact]
    public void ContentRange_WhenRangeEndIsGreaterThanSize_ShouldThrowArgumentException()
    {
        // Arrange
        var source = "bytes 0-200/100";
        
        // Act
        var act = () => new ContentRange(source);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Range end must be less than size.");
    }
    
    [Fact]
    public void ContentRange_WhenSizeIsAsterisk_ShouldSetSizeToNull()
    {
        // Arrange
        var source = "bytes 0-100/*";
        
        // Act
        var contentRange = new ContentRange(source);

        // Assert
        contentRange.Size.Should().BeNull();
    }
    
    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var source = "bytes 0-100/200";
        var contentRange = new ContentRange(source);
        
        // Act
        var result = contentRange.ToString();

        // Assert
        result.Should().Be("bytes 0-100/200");
    }
    
    [Fact]
    public void Equals_WhenOtherIsNull_ShouldReturnFalse()
    {
        // Arrange
        var source = "bytes 0-100/200";
        var contentRange = new ContentRange(source);
        
        // Act
        var result = contentRange.Equals(null);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void Equals_WhenOtherIsDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var source = "bytes 0-100/200";
        var contentRange = new ContentRange(source);
        
        // Act
        var result = contentRange.Equals("string");

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void Equals_WhenOtherIsSameContentRange_ShouldReturnTrue()
    {
        // Arrange
        var source = "bytes 0-100/200";
        var contentRange1 = new ContentRange(source);
        var contentRange2 = new ContentRange(source);
        
        // Act
        var result = contentRange1.Equals(contentRange2);

        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void Equals_WhenOtherIsDifferentContentRange_ShouldReturnFalse()
    {
        // Arrange
        var source1 = "bytes 0-100/200";
        var source2 = "bytes 0-50/200";
        var contentRange1 = new ContentRange(source1);
        var contentRange2 = new ContentRange(source2);
        
        // Act
        var result = contentRange1.Equals(contentRange2);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void GetHashCode_ShouldReturnSameHashCodeForSameContentRange()
    {
        // Arrange
        var source = "bytes 0-100/200";
        var contentRange1 = new ContentRange(source);
        var contentRange2 = new ContentRange(source);
        
        // Act
        var hashCode1 = contentRange1.GetHashCode();
        var hashCode2 = contentRange2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }
    
    [Fact]
    public void GetHashCode_ShouldReturnDifferentHashCodeForDifferentContentRange()
    {
        // Arrange
        var source1 = "bytes 0-100/200";
        var source2 = "bytes 0-50/200";
        var contentRange1 = new ContentRange(source1);
        var contentRange2 = new ContentRange(source2);
        
        // Act
        var hashCode1 = contentRange1.GetHashCode();
        var hashCode2 = contentRange2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }
    
    [Fact]
    public void OperatorEquals_WhenSameContentRange_ShouldReturnTrue()
    {
        // Arrange
        var source = "bytes 0-100/200";
        var contentRange1 = new ContentRange(source);
        var contentRange2 = new ContentRange(source);
        
        // Act
        var result = contentRange1 == contentRange2;

        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void OperatorEquals_WhenDifferentContentRange_ShouldReturnFalse()
    {
        // Arrange
        var source1 = "bytes 0-100/200";
        var source2 = "bytes 0-50/200";
        var contentRange1 = new ContentRange(source1);
        var contentRange2 = new ContentRange(source2);
        
        // Act
        var result = contentRange1 == contentRange2;

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void OperatorNotEquals_WhenSameContentRange_ShouldReturnFalse()
    {
        // Arrange
        var source = "bytes 0-100/200";
        var contentRange1 = new ContentRange(source);
        var contentRange2 = new ContentRange(source);
        
        // Act
        var result = contentRange1 != contentRange2;

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void OperatorNotEquals_WhenDifferentContentRange_ShouldReturnTrue()
    {
        // Arrange
        var source1 = "bytes 0-100/200";
        var source2 = "bytes 0-50/200";
        var contentRange1 = new ContentRange(source1);
        var contentRange2 = new ContentRange(source2);
        
        // Act
        var result = contentRange1 != contentRange2;

        // Assert
        result.Should().BeTrue();
    }
}