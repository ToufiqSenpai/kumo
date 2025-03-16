using Shared.Common.Filters;
using User.Application.Features.CreateUser;

namespace User.Presentation;

public static class ServiceExtensions
{
    public static void AddPresentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddControllers(options => { options.Filters.Add<ValidationFilter>(); });
    }
}