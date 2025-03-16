using User.Application.Repositories;

namespace User.Infrastructure.BackgroundServices;

public class RefreshTokenCleanupService(
    ILogger<RefreshTokenCleanupService> logger, 
    IServiceProvider serviceProvider) : BackgroundService
{
    private const ushort refreshTokenExpirationDays = 7;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting refresh token cleanup service.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                    
                    logger.LogInformation("Deleting expired refresh tokens...");

                    await refreshTokenRepository.DeleteByExpiredTokensAsync(stoppingToken);
                    
                    logger.LogInformation("Refresh tokens deleted.");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error when cleanup refresh tokens.");
            }
            
            await Task.Delay(TimeSpan.FromDays(refreshTokenExpirationDays), stoppingToken);
        }
        
        logger.LogInformation("Refresh token cleanup service is stopping.");
    }
}