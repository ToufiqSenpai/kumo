using System.Security.Cryptography;
using System.IO.Pipelines;
using System.Text;
using File.Application.Interfaces;

namespace File.Infrastructure.Services;

public class FileHasher : IFileHasher
{
    public string GetSha1Hash(Stream fileStream)
    {
        using var sha1 = SHA1.Create();
        byte[] hashBytes = sha1.ComputeHash(fileStream);

        StringBuilder hashString = new StringBuilder();
        
        foreach (byte b in hashBytes)
        {
            hashString.Append(b.ToString("x2"));
        }
        
        return hashString.ToString();
    }

    public string GetCrc32Hash(Stream fileStream)
    {
        uint[] table = GenerateCrc32Table();
        uint crc = 0xFFFFFFFF;

        int byteRead;
        while ((byteRead = fileStream.ReadByte()) != -1)
        {
            byte b = (byte)byteRead;
            crc = (crc >> 8) ^ table[(crc ^ b) & 0xFF];
        }

        crc ^= 0xFFFFFFFF;

        return crc.ToString("X8"); // Return as an 8-character hexadecimal string
    }
    
    private uint[] GenerateCrc32Table()
    {
        const uint polynomial = 0xEDB88320;
        uint[] table = new uint[256];

        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 8; j > 0; j--)
            {
                if ((crc & 1) == 1)
                {
                    crc = (crc >> 1) ^ polynomial;
                }
                else
                {
                    crc >>= 1;
                }
            }
            table[i] = crc;
        }

        return table;
    }
}