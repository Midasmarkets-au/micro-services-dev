using Api.V1;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.WebSocket;
using Grpc.Core;

namespace Bacera.Gateway.Web.Grpc;

/// <summary>
/// gRPC server implementation of MonoCallbackService (defined in scheduler.proto).
/// Called by the Rust scheduler service after a report is generated,
/// so mono can send the WebSocket popup notification to the requesting party.
/// </summary>
public class MonoCallbackGrpcService(
    WsMessageProcessor wsMessageProcessor,
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
}
