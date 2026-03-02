namespace Bacera.Gateway.Web.BackgroundJobs;

public interface IPaymentCallbackJob
{
    /// <summary>
    /// Process ChinaPay callback asynchronously
    /// </summary>
    /// <param name="paymentNumber">Payment number to process</param>
    /// <param name="encryptedData">Encrypted callback data from ChinaPay</param>
    /// <param name="signature">Signature for verification</param>
    /// <param name="rawCallbackJson">Original callback JSON for logging</param>
    Task ProcessChinaPayCallbackAsync(string paymentNumber, string encryptedData, 
        string signature, string rawCallbackJson);
}
