using Item.Application.DTOs;

namespace Item.Application.Features.CreateFolder;

public class CreateFolderRequestDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public FolderDto? Folder { get; init; }
}