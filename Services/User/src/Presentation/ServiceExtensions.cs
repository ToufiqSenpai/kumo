using Shared.Common.Filters;

namespace User.Presentation;

public static class ServiceExtensions
{
    public static void AddPresentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddControllers();

        services.AddScoped<ValidationFilter>();
    }
}