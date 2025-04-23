namespace File.Application.Interfaces;

public interface IFileHasher
{
    public string GetSha1Hash(Stream fileStream);
    public string GetCrc32Hash(Stream fileStream);
}