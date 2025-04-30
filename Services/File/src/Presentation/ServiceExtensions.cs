namespace File.Presentation;

public static class ServiceExtensions
{
    public static void AddPresentation(this IServiceCollection services)
    {
        services.AddGrpc();
    }
}