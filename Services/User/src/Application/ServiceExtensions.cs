using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using User.Application.Features.CreateUser;

namespace User.Application;

public static class ServiceExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        // Add handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Add validators
        services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
        
        // Add mappers
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
    }
}