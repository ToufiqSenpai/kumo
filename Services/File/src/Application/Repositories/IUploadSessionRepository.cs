using File.Domain.Entities;

namespace File.Application.Repositories;

public interface IUploadSessionRepository
{
    public Task SetAsync(string uploadId, UploadSession uploadSession);
    public Task<UploadSession?> GetAsync(string uploadId);
    public Task DeleteAsync(string uploadId);
}