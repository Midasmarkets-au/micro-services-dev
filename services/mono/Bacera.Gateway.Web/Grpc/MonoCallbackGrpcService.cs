using Api.V1;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.WebSocket;
using Grpc.Core;
using Hangfire;

namespace Bacera.Gateway.Web.Grpc;

/// <summary>
/// gRPC server implementation of MonoCallbackService (defined in scheduler.proto).
/// Called by the Rust scheduler service after a report is generated,
/// so mono can send the WebSocket popup notification to the requesting party.
/// Also called by the Rust scheduler cron to trigger recurring jobs managed by Hangfire.
/// </summary>
public class MonoCallbackGrpcService(
    WsMessageProcessor wsMessageProcessor,
    IHangfireWrapper hangfire,
    ILogger<MonoCallbackGrpcService> logger)
    : MonoCallbackService.MonoCallbackServiceBase
{
    public override Task<NotifyReportDoneResponse> NotifyReportDone(
        NotifyReportDoneRequest request,
        ServerCallContext context)
    {
        logger.LogInformation(
            "gRPC NotifyReportDone: tenantId={TenantId} partyId={PartyId} name={Name}",
            request.TenantId, request.PartyId, request.ReportName);

        try
        {
            var partyGroup = NotificationHub.GetPartyGroupName(request.TenantId, request.PartyId);
            var popup = MessagePopupDTO.BuildInfo("Report Process Finished", request.ReportName);
            wsMessageProcessor.AddMessage(
                WsSendDTO.Build("SendMessageToGroup",
                    [partyGroup, "ReceivePopup", popup.ToJson()]));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WS notification for NotifyReportDone");
        }

        return Task.FromResult(new NotifyReportDoneResponse { Success = true });
    }

    public override Task<TriggerCalculateRebateResponse> TriggerCalculateRebate(
        TriggerCalculateRebateRequest request,
        ServerCallContext context)
    {
        logger.LogInformation("gRPC TriggerCalculateRebate");
        hangfire.BackgroundJobClient.Enqueue<IRebateJob>(x => x.CalculateRebate());
        return Task.FromResult(new TriggerCalculateRebateResponse { Success = true });
    }

    public override Task<TriggerReleaseRebateResponse> TriggerReleaseRebate(
        TriggerReleaseRebateRequest request,
        ServerCallContext context)
    {
        logger.LogInformation("gRPC TriggerReleaseRebate");
        hangfire.BackgroundJobClient.Enqueue<IRebateJob>(x => x.ReleaseRebateAsync());
        return Task.FromResult(new TriggerReleaseRebateResponse { Success = true });
    }

    public override Task<TriggerCryptoMonitorResponse> TriggerCryptoMonitor(
        TriggerCryptoMonitorRequest request,
        ServerCallContext context)
    {
        logger.LogInformation("gRPC TriggerCryptoMonitor");
        hangfire.BackgroundJobClient.Enqueue<CryptoJob>(x => x.MonitorAsync());
        return Task.FromResult(new TriggerCryptoMonitorResponse { Success = true });
    }
}
