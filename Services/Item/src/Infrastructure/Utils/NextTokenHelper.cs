using MongoDB.Bson;

namespace Item.Infrastructure.Utils;

public static class NextTokenHelper
{
    public static string EncodeNextToken(ObjectId id)
    {
        return Convert.ToBase64String(id.ToByteArray())
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static ObjectId? DecodeNextToken(string token)
    {
        var base64 = token
            .Replace('-', '+')
            .Replace('_', '/');
        var bytes = Convert.FromBase64String(base64).ToString();

        if (ObjectId.TryParse(bytes, out ObjectId result))
        {
            return result;
        } else
        {
            return null;
        }
    }
}