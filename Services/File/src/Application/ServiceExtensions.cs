using System.Reflection;
using File.Application.Interfaces;
using File.Application.Services;
using FluentValidation;

namespace File.Application;

public static class ServiceExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        // Add handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Add validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Add mappers
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddSingleton<IUploadSessionService, UploadSessionService>();
    }
}