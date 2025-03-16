using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using User.Application.Features.CreateUser;
using User.Application.Features.LoginUser;

namespace User.Tests.Integration.Features;

public class LoginUserTest : BaseIntegrationTest
{
    public LoginUserTest(IntegrationTestWebApplicationFactory factory) : base(factory)
    {
        
    }

    [Fact]
    public async Task LoginUser_WithValidCredentials_ShouldResponseOk_WithToken()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var password = Faker.Internet.Password();
        var createUserRequest = new CreateUserRequestDto(Faker.Person.FullName, email, password);
        await HttpClient.PostAsJsonAsync("/api/v1/users", createUserRequest);
        
        var loginUserRequest = new LoginUserRequestDto(email, password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/users/login", loginUserRequest);
        
        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponseDto>();
        content?.RefreshToken.Should().NotBeNullOrEmpty();
        content?.AccessToken.Should().NotBeNullOrEmpty();
        
        var refreshToken = await RefreshTokenRepository.GetByTokenAsync(content.RefreshToken, CancellationToken.None);
        refreshToken.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginUser_WithInvalidCredentials_ShouldResponseUnauthorized()
    {
        // Arrange
        var loginUserRequest = new LoginUserRequestDto(Faker.Internet.Email(), Faker.Internet.Password());
        
        // Act
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/users/login", loginUserRequest);
        
        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}