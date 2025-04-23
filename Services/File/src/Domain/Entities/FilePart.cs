namespace File.Domain.Entities;

public class FilePart
{
    public string Id { get; set; }
    public int PartNumber { get; set; }
    public int Start { get; set; }
    public int End { get; set; }
}