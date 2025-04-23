using System.Collections.Concurrent;
using File.Application.Interfaces;
using File.Application.Repositories;
using File.Domain.Entities;
using MassTransit;
using Shared.Events.Item;

namespace File.Application.Services;

public class UploadSessionService(
    IUploadSessionRepository uploadSessionRepository, 
    IPublishEndpoint publishEndpoint, 
    ILogger<UploadSessionService> logger) : IUploadSessionService, IHostedService
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _jobs = new();
    private const int SessionTimeoutInHour = 1;
    
    public async Task SetUploadSessionAsync(string uploadId, UploadSession uploadSession)
    {
        if (_jobs.ContainsKey(uploadId))
        {
            var existCts = _jobs[uploadId];
            existCts.Cancel();
            existCts.Dispose();
            
            _jobs.TryRemove(uploadId, out _);
        }
        
        await uploadSessionRepository.SetAsync(uploadId, uploadSession);
        
        var cts = new CancellationTokenSource();
        _jobs[uploadId] = cts;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(SessionTimeoutInHour), cts.Token);
                
                if (!cts.Token.IsCancellationRequested)
                {
                    await DeleteUploadSessionAsync(uploadId);
                }
            }
            catch (TaskCanceledException e)
            {
                logger.LogInformation($"Upload session {uploadId} was cancelled.");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                throw;
            }
        }, cts.Token);
    }

    public async Task<UploadSession?> GetUploadSessionAsync(string uploadId)
    {
        return await uploadSessionRepository.GetAsync(uploadId);
    }

    public async Task DeleteUploadSessionAsync(string uploadId)
    {
        if (_jobs.TryRemove(uploadId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        var uploadSession = await uploadSessionRepository.GetAsync(uploadId);
        if (uploadSession != null)
        {
            await uploadSessionRepository.DeleteAsync(uploadId);
            await publishEndpoint.Publish(new DeleteFile(uploadSession.FileId));
        }
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping upload session jobs service.");

        foreach (var cts in _jobs.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        
        _jobs.Clear();
    }
}