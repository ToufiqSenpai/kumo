using System.Text.Json;
using System.Text.Json.Serialization;

namespace Item.Domain.Enums;

public enum SortBy
{
    Name,
    Size,
    LastModified,
    CreatedAt
}

public class SortByConverter : JsonConverter<SortBy>
{
    public override SortBy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()?.ToLower();
        return value switch
        {
            "name" => SortBy.Name,
            "size" => SortBy.Size,
            "last_modified" => SortBy.LastModified,
            "created_at" => SortBy.CreatedAt,
            _ => throw new JsonException($"Invalid SortBy value: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, SortBy value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLower());
    }
}
