using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using User.Infrastructure.Persistence;

namespace User.Tests.Integration;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("user_db")
        .WithUsername("user")
        .WithPassword("password")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Re-setup database
            var dbDescriptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<UserDbContext>));

            if (dbDescriptor is not null)
            {
                services.Remove(dbDescriptor);
            }

            services.AddDbContext<UserDbContext>(options => { options.UseNpgsql(_dbContainer.GetConnectionString()); });
            
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            dbContext.Database.Migrate();

            // Setup mass transit test
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });

            base.ConfigureWebHost(builder);
        });
    }
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }
    
    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}