using Newtonsoft.Json;

namespace Bacera.Gateway;

// {"arguments":["ReceivePopup","gggggg","{}"],"invocationId":"0","target":"SendMessageToGroup","type":1}
public sealed class WsSendDTO
{
    public string[] Arguments { get; set; } = [];
    public string Target { get; set; } = "";
    public string InvocationId { get; set; } = "0";
    public int Type { get; set; } = 1;

    public static WsSendDTO Build(string target, string[] arguments) => new()
    {
        Arguments = arguments,
        Target = target
    };

    public string ToJson() => Utils.JsonSerializeObject(this);

    public string ToWsMessage() => $"{ToJson()}";

    public byte[] ToBytes() => System.Text.Encoding.UTF8.GetBytes(ToWsMessage());
}