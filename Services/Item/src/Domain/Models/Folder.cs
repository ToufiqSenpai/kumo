namespace Item.Domain.Models;

public class Folder
{
    public int ChildCount { get; set; } = 0;
    public required FolderView View { get; set; }
}