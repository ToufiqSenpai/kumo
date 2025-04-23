namespace File.Application.Features.PutChunkFile;

public class PutChunkFileContinueUploadResponseDto
{
    public long NextStartRange { get; init; }
    public DateTime Expires { get; init; }
}