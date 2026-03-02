using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = ApiLog;

public partial class ApiLog
{
    public CentralApiLog ToCentralApiLog()
        => new()
        {
            PartyId = PartyId,
            StatusCode = StatusCode,
            ConnectionId = ConnectionId,
            Method = Method,
            Action = Action,
            UserAgent = UserAgent,
            Referer = Referer,
            Parameters = Parameters,
            RequestContent = RequestContent,
            ResponseContent = ResponseContent,
            Ip = Ip,
            CreatedOn = CreatedOn,
            UpdatedOn = UpdatedOn
        };
}