using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using User.Infrastructure.Options;
using User.Application.Interfaces;
using User.Application.Repositories;
using User.Infrastructure.BackgroundServices;
using User.Infrastructure.Persistence;
using User.Infrastructure.Repositories;
using User.Infrastructure.Security;

namespace User.Infrastructure;

public static class ServiceExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add options
        Console.WriteLine(Environment.GetEnvironmentVariable("Rabbitmq__Uri"));
        
        services.AddOptions<PasswordOptions>()
            .BindConfiguration(PasswordOptions.Password)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.Jwt)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<RefreshTokenOptions>()
            .BindConfiguration(RefreshTokenOptions.RefreshToken)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.Database)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<RabbitmqOptions>()
            .BindConfiguration(RabbitmqOptions.Rabbitmq)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<PasswordOptions>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtOptions>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RefreshTokenOptions>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<DatabaseOptions>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitmqOptions>>().Value);
        
        // Add security
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        
        // Add database context
        services.AddDbContext<UserDbContext>(options =>
        {
            options.UseNpgsql(configuration["Database:ConnectionString"]);
        });
        
        // Add repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        
        // Add mass transit
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<RabbitmqOptions>();
                
                cfg.Host(options.Uri, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                    
                    h.Heartbeat(60);
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });
        
        // Add background services
        services.AddHostedService<RefreshTokenCleanupService>();
    }
}