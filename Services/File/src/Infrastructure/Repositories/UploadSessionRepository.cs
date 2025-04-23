using System.Text.Json;
using File.Application.Repositories;
using File.Domain.Entities;
using StackExchange.Redis;

namespace File.Infrastructure.Repositories;

public class UploadSessionRepository(IConnectionMultiplexer connectionMultiplexer) : IUploadSessionRepository
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private const string UploadSessionPrefix = "upload_session:";

    public async Task SetAsync(string uploadId, UploadSession uploadSession)
    {
        await _database.StringSetAsync(UploadSessionPrefix + uploadId, JsonSerializer.Serialize(uploadSession));
    }

    public async Task<UploadSession?> GetAsync(string uploadId)
    {
        var data = await _database.StringGetAsync(UploadSessionPrefix + uploadId);
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<UploadSession>(data);
    }

    public async Task DeleteAsync(string uploadId)
    {
        await _database.KeyDeleteAsync(UploadSessionPrefix + uploadId);
    }
}