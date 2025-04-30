namespace File.Infrastructure.Options;

public class AzureBlobStorageOptions
{
    public const string AzureBlobStorage = "AzureBlobStorage";
    
    public string ConnectionString { get; set; }
    public string ContainerName { get; set; }
}