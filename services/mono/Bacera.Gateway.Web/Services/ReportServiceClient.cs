using Api.V1;

namespace Bacera.Gateway.Web.Services;

/// <summary>
/// gRPC client for the Rust scheduler service.
/// Replaces Hangfire Enqueue calls for report job dispatch.
/// </summary>
public interface IReportServiceClient
{
    Task EnqueueProcessReportRequestAsync(long tenantId, long requestId);
}

public class ReportServiceClient(SchedulerService.SchedulerServiceClient grpcClient,
    ILogger<ReportServiceClient> logger)
    : IReportServiceClient
{
    public async Task EnqueueProcessReportRequestAsync(long tenantId, long requestId)
    {
        try
        {
            var response = await grpcClient.EnqueueReportRequestAsync(new EnqueueReportRequestRequest
            {
                TenantId  = tenantId,
                RequestId = requestId,
            });

            if (!response.Success)
                throw new InvalidOperationException($"Scheduler rejected job: {response.Message}");

            logger.LogInformation(
                "Enqueued report request via scheduler gRPC: tenantId={TenantId} requestId={RequestId}",
                tenantId, requestId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to enqueue report request via scheduler gRPC: tenantId={TenantId} requestId={RequestId}",
                tenantId, requestId);
            throw;
        }
    }
}
