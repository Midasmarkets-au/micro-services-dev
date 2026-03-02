using Bacera.Gateway.Web.Services.Interface;
using Bacera.Gateway.Web.Services;
using Microsoft.Extensions.Options;
using Amazon.SQS;
using Bacera.Gateway;
using Bacera.Gateway.Web.Response;

namespace Bacera.Gateway.Web.Services;

/// <summary>
/// Simple utility to test AWS SQS connectivity
/// </summary>
public class SqsConnectivityTester
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly AmazonSQSOptions _sqsOptions;

    public SqsConnectivityTester(IMessageQueueService messageQueueService, IOptions<AmazonSQSOptions> sqsOptions)
    {
        _messageQueueService = messageQueueService;
        _sqsOptions = sqsOptions.Value;
    }

    /// <summary>
    /// Tests SQS connectivity by sending and receiving a test message
    /// </summary>
    /// <param name="testMessage">Test message to send (optional)</param>
    /// <returns>Test results with success status and details</returns>
    public async Task<SqsTestResult> TestConnectivityAsync(string? testMessage = null)
    {
        var result = new SqsTestResult();
        
        try
        {
            // Default test message
            testMessage ??= $"SQS Test Message - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}";
            
            Console.WriteLine("=== AWS SQS Connectivity Test ===");
            Console.WriteLine($"Region: {_sqsOptions.Region}");
            Console.WriteLine($"Prefix: {_sqsOptions.Prefix}");
            Console.WriteLine($"Test Queue: {_sqsOptions.BCRSendMessage}");
            Console.WriteLine();            // Test 1: Send a message
            Console.WriteLine("Test 1: Sending test message...");
            // FIFO queues require MessageGroupId
            await _messageQueueService.SendAsync(testMessage, _sqsOptions.BCRSendMessage, "test-group");
            result.SendSuccess = true;
            Console.WriteLine("✓ Message sent successfully");
            
            // Wait a moment for the message to be available
            await Task.Delay(2000);
            
            // Test 2: Receive messages
            Console.WriteLine("\nTest 2: Receiving messages...");
            var messages = await _messageQueueService.ReceiveAsync(_sqsOptions.BCRSendMessage, 1);
            
            if (messages.Count > 0)
            {
                result.ReceiveSuccess = true;
                var receivedMessage = messages[0];
                Console.WriteLine($"✓ Received message: {receivedMessage.Body}");
                
                // Test 3: Delete the message
                Console.WriteLine("\nTest 3: Deleting message...");
                await _messageQueueService.DeleteAsync(_sqsOptions.BCRSendMessage, receivedMessage);
                result.DeleteSuccess = true;
                Console.WriteLine("✓ Message deleted successfully");
            }
            else
            {
                Console.WriteLine("⚠ No messages received (may be expected if queue is empty)");
            }

            result.OverallSuccess = result.SendSuccess && (result.ReceiveSuccess || messages.Count == 0);
            Console.WriteLine($"\n=== Test Results ===");
            Console.WriteLine($"Send: {(result.SendSuccess ? "✓ SUCCESS" : "✗ FAILED")}");
            Console.WriteLine($"Receive: {(result.ReceiveSuccess ? "✓ SUCCESS" : messages.Count == 0 ? "- N/A (no messages)" : "✗ FAILED")}");
            Console.WriteLine($"Delete: {(result.DeleteSuccess ? "✓ SUCCESS" : result.ReceiveSuccess ? "✗ FAILED" : "- N/A")}");
            Console.WriteLine($"Overall: {(result.OverallSuccess ? "✓ SUCCESS" : "✗ FAILED")}");
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            result.Exception = ex;
            Console.WriteLine($"✗ Test failed with error: {ex.Message}");
            
            // Additional details for common SQS errors
            if (ex.Message.Contains("InvalidUserID.NotFound"))
            {
                Console.WriteLine("  → Check AWS_SQS_ACCESS_KEY configuration");
            }
            else if (ex.Message.Contains("SignatureDoesNotMatch"))
            {
                Console.WriteLine("  → Check AWS_SQS_ACCESS_SECRET configuration");
            }
            else if (ex.Message.Contains("AWS.SimpleQueueService.NonExistentQueue"))
            {
                Console.WriteLine("  → Check if the SQS queue exists and queue name configuration");
            }
            else if (ex.Message.Contains("UnauthorizedOperation"))
            {
                Console.WriteLine("  → Check IAM permissions for SQS operations");
            }
        }

        return result;
    }

    /// <summary>
    /// Tests connectivity to all configured SQS queues
    /// </summary>
    public async Task<Dictionary<string, SqsTestResult>> TestAllQueuesAsync()
    {
        var results = new Dictionary<string, SqsTestResult>();
        
        var queues = new Dictionary<string, string>
        {
            ["BCRSendMessage"] = _sqsOptions.BCRSendMessage,
            ["BCREventTrade"] = _sqsOptions.BCREventTrade,
            ["BCRTrade"] = _sqsOptions.BCRTrade,
            ["BCRSalesRebateTrade"] = _sqsOptions.BCRSalesRebateTrade
        };

        foreach (var queue in queues.Where(q => !string.IsNullOrEmpty(q.Value)))
        {
            Console.WriteLine($"\n=== Testing Queue: {queue.Key} ({queue.Value}) ===");
              try
            {
                var testMessage = $"Test message for {queue.Key} - {DateTime.UtcNow:HH:mm:ss}";
                // FIFO queues require MessageGroupId
                await _messageQueueService.SendAsync(testMessage, queue.Value, "test-group");
                
                results[queue.Key] = new SqsTestResult 
                { 
                    SendSuccess = true, 
                    OverallSuccess = true 
                };
                Console.WriteLine($"✓ {queue.Key}: Send successful");
            }
            catch (Exception ex)
            {
                results[queue.Key] = new SqsTestResult 
                { 
                    ErrorMessage = ex.Message, 
                    Exception = ex 
                };
                Console.WriteLine($"✗ {queue.Key}: {ex.Message}");
            }
        }

        return results;
    }

    /// <summary>
    /// Simple configuration check without sending messages
    /// </summary>
    public SqsConfigurationStatus CheckConfiguration()
    {
        var status = new SqsConfigurationStatus();
        
        Console.WriteLine("=== SQS Configuration Check ===");
        
        status.HasAccessKey = !string.IsNullOrEmpty(_sqsOptions.AccessKey);
        Console.WriteLine($"Access Key: {(status.HasAccessKey ? "✓ Configured" : "✗ Missing")}");
        
        status.HasAccessSecret = !string.IsNullOrEmpty(_sqsOptions.AccessSecret);
        Console.WriteLine($"Access Secret: {(status.HasAccessSecret ? "✓ Configured" : "✗ Missing")}");
        
        status.HasRegion = !string.IsNullOrEmpty(_sqsOptions.Region);
        Console.WriteLine($"Region: {(status.HasRegion ? $"✓ {_sqsOptions.Region}" : "✗ Missing")}");
        
        status.HasPrefix = !string.IsNullOrEmpty(_sqsOptions.Prefix);
        Console.WriteLine($"Prefix: {(status.HasPrefix ? $"✓ {_sqsOptions.Prefix}" : "✗ Missing")}");

        var queues = new[]
        {
            ("BCRSendMessage", _sqsOptions.BCRSendMessage),
            ("BCREventTrade", _sqsOptions.BCREventTrade), 
            ("BCRTrade", _sqsOptions.BCRTrade),
            ("BCRSalesRebateTrade", _sqsOptions.BCRSalesRebateTrade)
        };

        Console.WriteLine("\nQueue Configuration:");
        foreach (var (name, value) in queues)
        {
            var configured = !string.IsNullOrEmpty(value);
            Console.WriteLine($"  {name}: {(configured ? $"✓ {value}" : "✗ Not configured")}");
            status.ConfiguredQueues.Add(name, configured);
        }

        status.IsFullyConfigured = status.HasAccessKey && status.HasAccessSecret && 
                                  status.HasRegion && status.HasPrefix && 
                                  status.ConfiguredQueues.Values.Any(v => v);

        Console.WriteLine($"\nOverall Configuration: {(status.IsFullyConfigured ? "✓ Complete" : "✗ Incomplete")}");
        
        return status;
    }
}

public class SqsTestResult
{
    public bool SendSuccess { get; set; }
    public bool ReceiveSuccess { get; set; }
    public bool DeleteSuccess { get; set; }
    public bool OverallSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
}

public class SqsConfigurationStatus
{
    public bool HasAccessKey { get; set; }
    public bool HasAccessSecret { get; set; }
    public bool HasRegion { get; set; }
    public bool HasPrefix { get; set; }
    public Dictionary<string, bool> ConfiguredQueues { get; set; } = new();
    public bool IsFullyConfigured { get; set; }
}
