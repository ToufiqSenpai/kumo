using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using User.Application.Features.CreateUser;
using User.Application.Features.LoginUser;
using User.Application.Features.LogoutUser;

namespace User.Tests.Integration.Features;

public class LogoutUserTest : BaseIntegrationTest
{
    public LogoutUserTest(IntegrationTestWebApplicationFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task LogoutUser_WithRefreshToken_ShouldResponseNoContent()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var password = Faker.Internet.Password();
        var createUserRequest = new CreateUserRequestDto(Faker.Person.FullName, email, password);
        await HttpClient.PostAsJsonAsync("/api/v1/users", createUserRequest);
        
        var loginUserRequest = new LoginUserRequestDto(email, password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/users/login", loginUserRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponseDto>();
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/users/logout", new LogoutUserRequestDto(loginContent.RefreshToken));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var refreshToken = await RefreshTokenRepository.GetByTokenAsync(loginContent.RefreshToken, CancellationToken.None);
        refreshToken.Should().BeNull();
    }
}