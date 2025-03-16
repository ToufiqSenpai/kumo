using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using User.Application.Features.CreateUser;

namespace User.Tests.Integration.Features;

public class CreateUserTest : BaseIntegrationTest
{
    public CreateUserTest(IntegrationTestWebApplicationFactory factory) : base(factory) {}
    
    [Fact]
    public async Task CreateUser_WithValidData_ShouldResponseCreated()
    {
        // Arrange
        var request = new CreateUserRequestDto(
            Faker.Person.FullName,
            Faker.Internet.Email(),
            Faker.Internet.Password());
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var json = await response.Content.ReadFromJsonAsync<CreateUserResponseDto>();
        json?.Message.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task CreateUser_WithEmptyData_ShouldResponseBadRequest()
    {
        // Arrange
        var request = new CreateUserRequestDto(string.Empty, string.Empty, string.Empty);
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var json = await response.Content.ReadFromJsonAsync<CreateUserBadRequest>();
        json?.Message.Should().Be("Bad Request");
        
        json?.Errors.Name.Should().Contain("Name is required.");
        json?.Errors.Email.Should().Contain("Email is required.");
        json?.Errors.Password.Should().Contain("Password is required.");
    }

    [Fact]
    public async Task CreateUser_WithDistinctEmail_ShouldResponseBadRequest()
    {
        // Arrange
        var request = new CreateUserRequestDto(
            Faker.Person.FullName,
            Faker.Internet.Email(),
            Faker.Internet.Password());
        
        // Act
        await HttpClient.PostAsJsonAsync("/api/v1/users", request);
        var response = await HttpClient.PostAsJsonAsync("/api/v1/users", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var json = await response.Content.ReadFromJsonAsync<CreateUserBadRequest>();
        json?.Message.Should().Be("Bad Request");
        json?.Errors.Email.Should().Contain("Email is already in use.");
    }

    private class CreateUserBadRequest
    {
        public string Message { get; set; }
        public ErrorsObj Errors { get; set; }
    }
    
    private class ErrorsObj
    {
        public string[] Name { get; set; }
        public string[] Email { get; set; }
        public string[] Password { get; set; }
    }
}