using Microsoft.AspNetCore.Mvc;
using Shared.Common.Filters;

namespace Item.Presentation;

public static class ServiceExtensions
{
    public static void AddPresentation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

    services.AddOpenApi();
        services.AddControllers(options =>
        {
            options.Filters.Add<ModelStateFilter>();
            options.Filters.Add<ValidationFilter>();
        });
    }
}