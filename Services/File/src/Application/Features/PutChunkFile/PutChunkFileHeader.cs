using File.Domain.ValueObjects;

namespace File.Application.Features.PutChunkFile;

public class PutChunkFileHeader
{
    public ContentRange ContentRange { get; init; }
    public ulong ContentLength { get; init; }
}