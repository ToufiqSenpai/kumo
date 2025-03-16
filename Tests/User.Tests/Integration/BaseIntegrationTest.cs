using Bogus;
using Microsoft.Extensions.DependencyInjection;
using User.Application.Repositories;

namespace User.Tests.Integration;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebApplicationFactory>, IDisposable
{
    private readonly IServiceScope _scope;
    protected readonly HttpClient HttpClient;
    protected readonly IUserRepository UserRepository;
    protected readonly Faker Faker = new();

    protected BaseIntegrationTest(IntegrationTestWebApplicationFactory factory)
    {
        _scope = factory.Services.CreateScope();
        HttpClient = factory.CreateClient();
        UserRepository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
    }
    
    public void Dispose()
    {
        _scope.Dispose();
    }
}