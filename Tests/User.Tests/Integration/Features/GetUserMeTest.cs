using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using User.Application.DTOs;

namespace User.Tests.Integration.Features;

public class GetUserMeTest : BaseIntegrationTest
{
    private readonly Domain.Entities.User _userMock;
    
    public GetUserMeTest(IntegrationTestWebApplicationFactory factory) : base(factory)
    {
        var user = new Domain.Entities.User()
        {
            Name = Faker.Person.FullName,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.Password(),
        };
        
        UserRepository.AddAsync(user, CancellationToken.None).GetAwaiter();
        
        _userMock = user;
    }

    [Fact]
    public async Task GetUserMe_WithExistsUserId_ShouldResponseOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/users/me");
        request.Headers.Add("X-User-ID", _userMock.Id.ToString());
        
        // Act
        var response = await HttpClient.SendAsync(request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user?.Name.Should().Be(_userMock.Name);
        user?.Email.Should().Be(_userMock.Email);
    }
    
    [Fact]
    public async Task GetUserMe_WithNotExistsUserId_ShouldResponseNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/users/me");
        request.Headers.Add("X-User-ID", Guid.NewGuid().ToString());
        
        // Act
        var response = await HttpClient.SendAsync(request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}