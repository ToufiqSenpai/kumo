namespace File.Application.Features.GetUploadSession;

public class GetUploadSessionResponseDto
{
    public long NextStartRange { get; init; }
    public DateTime Expires { get; init; }
}