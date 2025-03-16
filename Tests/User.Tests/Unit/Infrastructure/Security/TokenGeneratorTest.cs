using AutoFixture;
using FluentAssertions;
using User.Infrastructure.Options;
using User.Infrastructure.Security;
using Xunit.Abstractions;

namespace User.Tests.Unit.Infrastructure.Security;

public class TokenGeneratorTest(ITestOutputHelper output)
{
    // private readonly JwtOptions _options = new("AVerySuperDuperSecretKeyAwokwkwkwkwkwkwkwkwkwkw", "example.com", "example.com", 60);
    private readonly JwtOptions _options = new JwtOptions
    {
        SecretKey = "AVerySuperDuperSecretKeyAwokwkwkwkwkwkwkwkwkwkw",
        Issuer = "example.com",
        Audience = "example.com",
        ExpiresInMinute = 60
    };
    private readonly Fixture _fixture = new();
    
    [Fact]
    public void GenerateAccessToken_Successfully()
    {
        // Arrange
        var user = _fixture.Create<Domain.Entities.User>();
        var tokenGenerator = new TokenGenerator(_options);
        
        // Act
        var token = tokenGenerator.GenerateAccessToken(user);
        
        output.WriteLine(token);
        
        // Assert
        token.Should().NotBeNull();
    }
    
    [Fact]
    public void GenerateRefreshToken_Successfully()
    {
        // Arrange
        var tokenGenerator = new TokenGenerator(_options);
        
        // Act
        var token = tokenGenerator.GenerateRefreshToken();
        
        output.WriteLine(token);
        
        // Assert
        token.Should().NotBeNull();
    }
}