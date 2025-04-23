using System.Text.RegularExpressions;

namespace File.Domain.ValueObjects;

public class ContentRange : IEquatable<ContentRange>
{
    private static readonly Regex ContentRangeRegex = new(
        @"^bytes (?<start>\d+)-(?<end>\d+)\/(?<size>\d+|\*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    
    public int RangeStart { get; }
    public int RangeEnd { get; }
    public long? Size { get; }
    
    public ContentRange(string source)
    {
        var match = ContentRangeRegex.Match(source);
        if (!match.Success)
        {
            throw new ArgumentException("Invalid content range format.");
        }
        
        RangeStart = int.Parse(match.Groups["start"].Value);
        RangeEnd = int.Parse(match.Groups["end"].Value);
        
        Size = match.Groups["size"].Value == "*"
            ? null
            : long.Parse(match.Groups["size"].Value);
        
        if (RangeStart > RangeEnd)
            throw new ArgumentException("Range start cannot be greater than range end.");
        
        if (Size.HasValue && RangeEnd >= Size.Value)
            throw new ArgumentException("Range end must be less than size.");
    }

    public bool Equals(ContentRange other)
    {
        if (other is null) return false;
        return RangeStart == other.RangeStart && 
               RangeEnd == other.RangeEnd && 
               Size == other.Size;
    }

    public override bool Equals(object obj) => Equals(obj as ContentRange);

    public override int GetHashCode() => HashCode.Combine(RangeStart, RangeEnd, Size);
    
    public override string ToString() => 
        $"bytes {RangeStart}-{RangeEnd}/{Size?.ToString() ?? "*"}";
    
    public static bool operator ==(ContentRange left, ContentRange right) => 
        EqualityComparer<ContentRange>.Default.Equals(left, right);

    public static bool operator !=(ContentRange left, ContentRange right) => 
        !(left == right);
}