using Microsoft.AspNetCore.Mvc.Testing;

namespace File.Tests.Integration;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    
    
    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }
    
    public Task DisposeAsync()
    {
        throw new NotImplementedException();
    }
}