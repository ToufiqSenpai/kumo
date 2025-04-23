using File.Domain.Entities;

namespace File.Application.Interfaces;

public interface IUploadSessionService
{
    public Task SetUploadSessionAsync(string uploadId, UploadSession uploadSession);
    public Task<UploadSession?> GetUploadSessionAsync(string uploadId);
    public Task DeleteUploadSessionAsync(string uploadId);
}