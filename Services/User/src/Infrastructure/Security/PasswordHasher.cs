using System.Security.Cryptography;
using System.Text;
using User.Application.Interfaces;
using Konscious.Security.Cryptography;
using User.Infrastructure.Options;

namespace User.Infrastructure.Security;

public class PasswordHasher(PasswordOptions options) : IPasswordHasher
{
    public string HashPassword(string password)
    {
        byte[] salt = GenerateSalt();
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = options.MemorySize,
            DegreeOfParallelism = options.DegreeOfParallelism,
            Iterations = options.Iterations
        };
        
        byte[] hash = argon2.GetBytes(options.HashSize);
        
        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
    }
    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.');
        
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] hash = Convert.FromBase64String(parts[1]);
        
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = options.MemorySize,
            DegreeOfParallelism = options.DegreeOfParallelism,
            Iterations = options.Iterations
        };
        
        byte[] computedHash = argon2.GetBytes(options.HashSize);
        
        return hash.SequenceEqual(computedHash);
    }
    
    private byte[] GenerateSalt()
    {
        byte[] salt = new byte[options.SaltSize];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        
        return salt;
    }
}