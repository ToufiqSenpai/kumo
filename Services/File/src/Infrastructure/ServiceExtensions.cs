using Azure.Storage.Blobs;
using File.Application.Interfaces;
using File.Application.Repositories;
using File.Infrastructure.Options;
using File.Infrastructure.Repositories;
using File.Infrastructure.Services;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace File.Infrastructure;

public static class ServiceExtensions
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<AzureBlobStorageOptions>()
            .BindConfiguration(AzureBlobStorageOptions.AzureBlobStorage)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<RedisOptions>()
            .BindConfiguration(RedisOptions.Redis)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AzureBlobStorageOptions>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RedisOptions>>().Value);
        
        // Add services
        services.AddSingleton<IFileStorageService, AzureBlobStorageService>();
        services.AddSingleton<IFileHasher, FileHasher>();
        services.AddSingleton<IFileMetadataExtractor, FileMetadataExtractor>();
        
        // Add repositories
        services.AddSingleton<IUploadSessionRepository, UploadSessionRepository>();

        // Add Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<RedisOptions>();
            var options = ConfigurationOptions.Parse(redisOptions.Host, true);
        
            return ConnectionMultiplexer.Connect(options);
        });
        
        // Add Azure Blob Storage
        services.AddSingleton(sp =>
        {
            var azureBlobStorageOptions = sp.GetRequiredService<AzureBlobStorageOptions>();
        
            return new BlobServiceClient(azureBlobStorageOptions.ConnectionString);
        });
    }
}