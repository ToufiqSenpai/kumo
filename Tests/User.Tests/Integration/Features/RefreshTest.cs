using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using User.Application.Features.CreateUser;
using User.Application.Features.LoginUser;
using User.Application.Features.Refresh;

namespace User.Tests.Integration.Features;

public class RefreshTest : BaseIntegrationTest
{
    public RefreshTest(IntegrationTestWebApplicationFactory factory) : base(factory) {}

    [Fact]
    public async Task Refresh_WithValidToken_ShouldResponseOk_WithNewToken()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var password = Faker.Internet.Password();
        await HttpClient.PostAsJsonAsync("/api/v1/users",
            new CreateUserRequestDto(Faker.Person.FullName, email, password));
        
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/users/login",
            new LoginUserRequestDto(email, password));
        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponseDto>();
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/users/refresh",
            new RefreshRequestDto(loginData.RefreshToken));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseData = await response.Content.ReadFromJsonAsync<RefreshResponseDto>();
        responseData?.AccessToken.Should().NotBeNullOrEmpty();
        responseData?.RefreshToken.Should().NotBeNullOrEmpty();
        
        var refreshToken = await RefreshTokenRepository.GetByTokenAsync(responseData.RefreshToken, CancellationToken.None);
        refreshToken.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Refresh_WithInvalidToken_ShouldResponseUnauthorized()
    {
        // Arrange
        var refreshToken = Faker.Random.String2(100);
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/users/refresh",
            new RefreshRequestDto(refreshToken));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}