using Item.Domain.Models;

namespace Item.Application.Features.CreateFile;

public record CreateFileRequestDto()
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public FileSystem? FileSystem { get; init; }
}