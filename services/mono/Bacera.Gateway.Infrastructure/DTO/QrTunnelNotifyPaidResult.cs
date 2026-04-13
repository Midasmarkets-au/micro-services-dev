namespace Bacera.Gateway.DTO;

public sealed class QrTunnelNotifyPaidResult
{
    public bool Success { get; init; }

    /// <summary>Upstream HTTP status, or 0 if transport failed.</summary>
    public int HttpStatusCode { get; init; }

    public string? Message { get; init; }

    public string? ResponseBody { get; init; }
}
