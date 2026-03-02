namespace Bacera.Gateway.Services.Background;

public interface IBackgroundJob
{
    Task<(bool, string)> StartAsync();
    Task<(bool, string)> StopAsync();
    Task ExecuteAsync(CancellationToken cancellationToken);
}