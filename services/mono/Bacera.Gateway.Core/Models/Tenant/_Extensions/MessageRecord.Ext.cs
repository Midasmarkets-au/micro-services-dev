using System.Text.RegularExpressions;
using Newtonsoft.Json;
using M = Bacera.Gateway.MessageRecord;

namespace Bacera.Gateway;

public partial class MessageRecord
{
    public MQSource ToMQSource(long tenantId, string options) => new()
    {
        TenantId = tenantId,
        MessageRecordId = Id,
        Method = Method,
        Options = options
    };

    public sealed class MQSource
    {
        public long TenantId { get; set; }
        public long MessageRecordId { get; set; }
        public string Method { get; set; } = "";
        public string Options { get; set; } = "{}";

        public static bool TryParse(string json, out MQSource source)
        {
            source = new MQSource();
            try
            {
                source = JsonConvert.DeserializeObject<MQSource>(json)!;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public sealed class EmailSenderOptions
    {
        public string SenderEmailAddress { get; set; } = "";
        public string SenderDisplayName { get; set; } = "";
        public List<string>? Bcc { get; set; }

        public static bool TryParse(string json, out EmailSenderOptions source)
        {
            source = new EmailSenderOptions();
            try
            {
                source = JsonConvert.DeserializeObject<EmailSenderOptions>(json)!;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public static M Email(string email, string subject, string content, long partyId = 0, long eventId = 0) => new()
    {
        Method = "email",
        Receiver = email,
        ReceiverPartyId = partyId,
        Content = content,
        Event = subject,
        EventId = eventId,
        Status = (short)StatusTypes.Pending,
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow
    };


    public enum StatusTypes
    {
        Pending = 0,
        Sent = 1,
        Failed = 2
    }
}