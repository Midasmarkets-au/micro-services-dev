namespace Bacera.Gateway.Web.Response;

using M = MQMessage;

public class MQMessage
{
    public string Body { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string ReceiptHandle { get; set; } = string.Empty;
}

public static class MQMessageExtension
{
    public static List<M> ToMQMessage(this IEnumerable<Amazon.SQS.Model.Message> messages, string queueName = "")
        => messages.Select(m => new M
        {
            Body = m.Body,
            QueueName = queueName,
            ReceiptHandle = m.ReceiptHandle
        }).ToList();
}