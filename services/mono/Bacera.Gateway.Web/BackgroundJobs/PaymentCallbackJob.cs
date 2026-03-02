using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.ChinaPay;
using Bacera.Gateway.Web.Controllers;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs;

public class PaymentCallbackJob(
    IServiceProvider serviceProvider,
    ILogger<PaymentCallbackJob> logger,
    MyDbContextPool pool,
    IMyCache myCache) : IPaymentCallbackJob
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task ProcessChinaPayCallbackAsync(string paymentNumber, 
        string encryptedData, string signature, string rawCallbackJson)
    {
        logger.LogInformation("ChinaPay_Background_Started: PaymentNumber={PaymentNumber}", 
            paymentNumber);
        
        try
        {
            // 1. Create scope by payment number
            using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
            if (scope == null)
            {
                logger.LogWarning("ChinaPay_Background_TenantNotFound: {PaymentNumber}", 
                    paymentNumber);
                return; // Don't retry if tenant not found
            }
            
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            
            // 2. Get payment with service details
            var payment = await ctx.Payments
                .Include(x => x.PaymentMethod)
                .FirstOrDefaultAsync(x => x.Number == paymentNumber);
                
            if (payment == null)
            {
                logger.LogWarning("ChinaPay_Background_PaymentNotFound: {PaymentNumber}", 
                    paymentNumber);
                return; // Don't retry if payment not found
            }
            
            // 3. Idempotency check
            if (payment.Status == (int)PaymentStatusTypes.Completed)
            {
                logger.LogInformation("ChinaPay_Background_AlreadyCompleted: {PaymentNumber}", 
                    paymentNumber);
                return;
            }
            
            // 4. Load ChinaPay options
            var options = ChinaPayOptions.FromJson(payment.PaymentMethod.Configuration);
            
            // 5. Verify signature (again, in case of cache issues)
            if (!ChinaPay.VerifySign(encryptedData, options.ServerPublicKey, signature))
            {
                logger.LogError("ChinaPay_Background_InvalidSignature: {PaymentNumber}", 
                    paymentNumber);
                throw new Exception("Invalid signature"); // Retry in case of transient issue
            }
            
            // 6. Decrypt callback data
            string cbBodyJson = ChinaPay.DecryptJava(options.AppPrivateKey, encryptedData);
            dynamic cbBody = Utils.JsonDeserializeDynamic(cbBodyJson);
            
            // 7. Load existing callback body to preserve vendor data from controller
            var existingCallbackBody = string.IsNullOrEmpty(payment.CallbackBody)
                ? null
                : JsonConvert.DeserializeObject<Payment.CallbackBodyModel>(payment.CallbackBody);
            
            // Extract existing data safely
            dynamic? existingBody = existingCallbackBody?.Body;

            // 8. Update payment callback body with decrypted data, preserving vendor callback
            // 2nd time log CallbackBody
            payment.CallbackBody = JsonConvert.SerializeObject(new Payment.CallbackBodyModel
            {
                Status = "processing_by_hangfire",
                UpdatedOn = DateTime.UtcNow,
                Body = new
                {
                    // Preserve vendor callback from controller (encrypted data with signature)
                    vendorCallback = existingBody?.vendorCallback,
                    // Add decrypted callback data for easy access
                    decryptedData = cbBody,
                    // Add processing metadata
                    processingInfo = new
                    {
                        receivedAt = existingBody?.processingInfo?.receivedAt,
                        signatureVerified = existingBody?.processingInfo?.signatureVerified,
                        decryptedAt = DateTime.UtcNow,
                        hangfireJobStarted = true,
                        note = "Background job processing deposit"
                    }
                }
            });
            ctx.Payments.Update(payment);
            await ctx.SaveChangesAsync();
            
            logger.LogInformation("ChinaPay_Background_ProcessingStarted: {PaymentNumber}", 
                paymentNumber);
            
            // 8. Complete deposit (business logic)
            await PaymentCallbackController.TryCompleteDeposit(scope, payment, cbBody);
            
            logger.LogInformation("ChinaPay_Background_Completed: {PaymentNumber}", 
                paymentNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ChinaPay_Background_Error: {PaymentNumber}, Error: {Message}", 
                paymentNumber, ex.Message);
            throw; // Let Hangfire handle retry
        }
    }
    
    private async Task<IServiceScope?> CreateScopeByPaymentNumberAsync(string paymentNumber)
    {
        // Use cache first
        var cacheKey = Payment.GetPaymentNumberTenantIdKey(paymentNumber);
        var tidString = await myCache.GetStringAsync(cacheKey);
        
        if (tidString is { Length: 5 } && long.TryParse(tidString, out var t))
            return serviceProvider.CreateTenantScope(t);
        
        // Fallback: search all tenants
        var results = await Task.WhenAll(pool.GetTenantIds().Select(async tid =>
        {
            await using var ctx = pool.CreateTenantDbContext(tid);
            var exists = await ctx.Payments.AnyAsync(p => p.Number == paymentNumber);
            return exists ? tid : 0;
        }));
        
        var tenantId = results.FirstOrDefault(x => x != 0);
        return tenantId == 0 ? null : serviceProvider.CreateTenantScope(tenantId);
    }
}
