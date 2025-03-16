using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using User.Infrastructure.Security;
using Xunit.Abstractions;
using PasswordOptions = User.Infrastructure.Options.PasswordOptions;

namespace User.Tests.Unit.Infrastructure.Security;

public class PasswordHasherTest
{
    private readonly PasswordOptions _options;
    private readonly ITestOutputHelper _output;
    
    public PasswordHasherTest(ITestOutputHelper output)
    {
        // _options = new PasswordOptions(4, 8 * 1024, 2, 128, 32);
        _options = new PasswordOptions
        {
            DegreeOfParallelism = 4,
            MemorySize = 8 * 1024,
            Iterations = 2,
            SaltSize = 128,
            HashSize = 32
        };
        _output = output;
    }
    
    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "password";
        var passwordHasher = new PasswordHasher(_options);
        
        // Act
        var hashedPassword = passwordHasher.HashPassword(password);
        _output.WriteLine(hashedPassword);

        // Assert
        hashedPassword.Should().NotBeEmpty();
    }
    
    [Fact]
    public void VerifyPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "password";
        var passwordHasher = new PasswordHasher(_options);
        var hashedPassword = passwordHasher.HashPassword(password);
        
        // Act
        var result = passwordHasher.VerifyPassword(password, hashedPassword);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void VerifyPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "password";
        var passwordHasher = new PasswordHasher(_options);
        var hashedPassword = passwordHasher.HashPassword(password);
        
        // Act
        var result = passwordHasher.VerifyPassword("wrongPassword", hashedPassword);
        
        // Assert
        result.Should().BeFalse();
    }
}