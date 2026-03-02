using System.Text;
using System.Web;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.Bakong;
using Bacera.Gateway.Vendor.BigPay;
using Bacera.Gateway.Vendor.BigPay.Models;
using Bacera.Gateway.Vendor.BipiPay;
using Bacera.Gateway.Vendor.Buy365;
using Bacera.Gateway.Vendor.Buzipay;
using Bacera.Gateway.Vendor.ChinaPay;
using Bacera.Gateway.Vendor.ChipPay;
using Bacera.Gateway.Vendor.ExLink.Models;
using Bacera.Gateway.Vendor.ExLinkCashier;
using Bacera.Gateway.Vendor.FivePayF2F;
using Bacera.Gateway.Vendor.FivePayVA;
using Bacera.Gateway.Vendor.GPay;
using Bacera.Gateway.Vendor.Help2Pay;
using Bacera.Gateway.Vendor.Help2Pay.Models;
using Bacera.Gateway.Vendor.Long77Pay;
using Bacera.Gateway.Vendor.Monetix;
using Bacera.Gateway.Vendor.MonetixPay;
using Bacera.Gateway.Vendor.OFAPay;
using Bacera.Gateway.Vendor.PayPal;
using Bacera.Gateway.Vendor.UnionePay;
using Bacera.Gateway.Vendor.UniotcPay;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Services;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Controllers;

[Tags("Public/Payment")]
[AllowAnonymous]
[Route("api/" + VersionTypes.V1 + "/payment/callback/{tenantId:long}")]
public class PaymentCallbackController(
    ILogger<PaymentCallbackController> logger,
    CentralDbContext centralDbContext,
    IServiceProvider serviceProvider,
    IHttpClientFactory httpclientFactory,
    IMyCache myCache,
    MyDbContextPool pool,
    IBackgroundJobClient backgroundJobClient)
    : BaseController
{
    [HttpGet("ping")]
    public IActionResult Ping(long tenantId)
    {
        return Ok(tenantId + " - PONG");
    }

    /// <summary>
    /// [TEST ONLY] Generate test instructions for ChinaPay callback
    /// Since we cannot perfectly simulate vendor's encryption/signing without their private key,
    /// this endpoint provides guidance for testing
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="paymentNumber">Payment number (required)</param>
    /// <param name="amount">Amount in cents (optional)</param>
    /// <returns>Test instructions and sample data</returns>
    [HttpGet("chinapay/test")]
    public async Task<IActionResult> TestChinaPayCallback(
        long tenantId, 
        [FromQuery] string? paymentNumber = null,
        [FromQuery] long? amount = null)
    {
        try
        {
            if (string.IsNullOrEmpty(paymentNumber))
            {
                return Ok(new
                {
                    message = "ChinaPay Callback Test Endpoint",
                    usage = $"GET /api/v1/payment/callback/{tenantId}/chinapay/test?paymentNumber=pm-xxx&amount=366000",
                    note = "Provide a valid payment number to generate test callback data"
                });
            }

            using var scope = serviceProvider.CreateScope();
            var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
            tenancyResolver.SetTenantId(tenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            
            var payment = await ctx.Payments
                .Include(x => x.PaymentMethod)
                .FirstOrDefaultAsync(x => x.Number == paymentNumber);
                
            if (payment == null)
                return BadRequest($"Payment not found: {paymentNumber}");
            
            var options = ChinaPayOptions.FromJson(payment.PaymentMethod.Configuration);
            var (isValid, errorMsg) = options.Validate();
            if (!isValid)
                return BadRequest($"Invalid ChinaPay config: {errorMsg}");
            
            // Create unencrypted callback data for reference
            var callbackData = new
            {
                payCny = amount ?? payment.Amount,
                tradeNo = paymentNumber,
                status = "success",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            
            var callbackJson = JsonConvert.SerializeObject(callbackData);
            
            // Provide a simple test that bypasses encryption for initial testing
            var simpleTestCallback = new
            {
                code = 0,
                message = "success",
                body = new
                {
                    charset = "utf-8",
                    sign = "TEST_SIGNATURE_BYPASS",
                    method = "tigerpay.trade.wap",
                    version = "1.0.0",
                    data = Convert.ToBase64String(Encoding.UTF8.GetBytes(callbackJson)),
                    outTradeNo = paymentNumber
                }
            };
            
            var simpleJson = JsonConvert.SerializeObject(simpleTestCallback, Formatting.Indented);
            
            var instructions = $@"
===========================================
ChinaPay Callback Test Instructions
===========================================

Payment Number: {paymentNumber}
Amount: {amount ?? payment.Amount} cents ({(amount ?? payment.Amount) / 100m:F2} yuan)
Tenant ID: {tenantId}

⚠️ TESTING LIMITATION:
Real ChinaPay callbacks are encrypted with YOUR public key and signed with VENDOR's private key.
We cannot perfectly simulate this without vendor's private key.

RECOMMENDED TESTING APPROACHES:

1. TEST WITH REAL VENDOR (Best)
   - Use ChinaPay's sandbox/test environment
   - Create real payment and wait for callback
   
2. TEMPORARY BYPASS FOR LOCAL TESTING (Requires code modification)
   - Temporarily comment out signature verification
   - Use the simple test callback below

3. USE ACTUAL PRODUCTION CALLBACK DATA
   - Copy a real callback from production logs
   - Replay it against your test environment

-------------------------------------------
SIMPLE TEST CALLBACK (No encryption):
-------------------------------------------
{simpleJson}

TEST COMMAND (PowerShell):
$body = @'
{simpleJson}
'@
Invoke-RestMethod -Uri ""http://localhost:5001/api/v1/payment/callback/{tenantId}/chinapay"" -Method Post -ContentType ""application/json"" -Body $body

EXPECTED CALLBACK DATA FORMAT (Decrypted):
{callbackJson}

-------------------------------------------
TO USE THIS TEST:
-------------------------------------------
1. Temporarily modify ChinaPayCallback method:
   - Comment out: if (!ChinaPay.VerifySign(...)) {{ return... }}
   
2. Temporarily modify PaymentCallbackJob.ProcessChinaPayCallbackAsync:
   - Comment out signature verification
   - Replace: string cbBodyJson = ChinaPay.DecryptJava(...)
     With: string cbBodyJson = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedData))

3. Run the PowerShell command above

4. Verify:
   - Response should be: SUCCESS
   - Payment.Status: Pending -> Executing -> Completed
   - Check Hangfire dashboard
   - Check logs for ChinaPay_* messages

5. REMEMBER TO RESTORE the verification code after testing!

-------------------------------------------
CONFIGURATION CHECK:
-------------------------------------------
AppPrivateKey: {(options.AppPrivateKey?.Length > 50 ? "✓ Present (" + options.AppPrivateKey.Length + " chars)" : "✗ Missing or too short")}
ServerPublicKey: {(options.ServerPublicKey?.Length > 50 ? "✓ Present (" + options.ServerPublicKey.Length + " chars)" : "✗ Missing or too short")}
Callback URI: {options.CallbackUri}

NOTE: Keys should be in Java PKCS#8 format (base64, no PEM headers/footers)
If keys have '-----BEGIN' headers, remove them and keep only the base64 content.
";
            
            return Content(instructions, "text/plain", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TestChinaPay_Error: {Message}", ex.Message);
            return StatusCode(500, new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace,
                innerError = ex.InnerException?.Message
            });
        }
    }

    [HttpPost("chinapay/debug")]
    public async Task<IActionResult> DebugChinaPayCallback(long tenantId)
    {
        // Log all headers
        foreach (var header in Request.Headers)
        {
            logger.LogInformation("ChinaPay_Callback_Header: {Key}={Value}", header.Key, string.Join(", ", header.Value));
        }

        // Log query parameters
        foreach (var param in Request.Query)
        {
            logger.LogInformation("ChinaPay_Callback_Query: {Key}={Value}", param.Key, string.Join(", ", param.Value));
        }

        try
        {
            if (Request.HasFormContentType)
            {
                logger.LogInformation("ChinaPay_Callback_HasFormContent: true");
                foreach (var form in Request.Form)
                {
                    logger.LogInformation("ChinaPay_Callback_Form: {Key}={Value}", form.Key, string.Join(", ", form.Value));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("ChinaPay_Callback_FormParseError: {Error}", ex.Message);
        }

        var json = await GetRequestBody();
        logger.LogInformation("ChinaPay_Debug_RawCallback: TenantId={TenantId}, Json={Json}", tenantId, json);
        
        // Log headers too
        foreach (var header in Request.Headers)
        {
            logger.LogInformation("ChinaPay_Debug_Header: {Key}={Value}", header.Key, string.Join(", ", header.Value));
        }
        
        return Content("DEBUG_SUCCESS", "text/plain", Encoding.UTF8);
    }

    [HttpPost("chinapay/test")]
    public async Task<IActionResult> TestChinaPayCallback(long tenantId, [FromQuery] string? paymentNumber = null)
    {
        // Create a realistic test callback that will work with your actual callback logic
        paymentNumber ??= "pm-test" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var testCallbackData = new
        {
            payCny = 366000, // Amount in cents (3660.00)
            tradeNo = paymentNumber,
            status = "success"
        };
        
        var testCallbackJson = JsonConvert.SerializeObject(testCallbackData);
        logger.LogInformation("ChinaPay_Test_CallbackData: {Json}", testCallbackJson);
        
        // For real testing, you would encrypt this data properly
        // For now, let's create a simplified test
        var testCallback = JsonConvert.SerializeObject(new
        {
            code = 0,
            message = "success",
            body = new
            {
                sign = "test_signature_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                outTradeNo = paymentNumber,
                data = Convert.ToBase64String(Encoding.UTF8.GetBytes(testCallbackJson)) // Simple base64 instead of proper encryption for testing
            }
        });

        logger.LogInformation("ChinaPay_Test_SimulatedCallback: {Json}", testCallback);
        
        // Return instructions for manual testing
        var instructions = $@"
            Test Callback Created!
            Payment Number: {paymentNumber}

            To test manually, run this curl command:
            curl -X POST ""http://localhost:5001/api/v1/payment/callback/{tenantId}/chinapay"" \
              -H ""Content-Type: application/json"" \
              -d '{testCallback}'

            Or use this URL to test the ping endpoint:
            http://localhost:5001/api/v1/payment/callback/{tenantId}/ping
            ";
        
        return Content(instructions, "text/plain", Encoding.UTF8);
    }

    [HttpPost("chinapay/{token?}")]
    public async Task<IActionResult> ChinaPayCallback(long tenantId, string? token)
    {
        // ========== FAST PATH: Validation Only ==========
        
        logger.LogInformation("ChinaPay_Callback_Started: TenantId={TenantId}, Token={Token}", tenantId, token);
        
        var json = await GetRequestBody();
        logger.LogInformation("ChinaPay_Callback_RawBody: {Json}", json);
        
        // 1. Parse JSON
        dynamic body;
        try
        {
            body = Utils.JsonDeserializeDynamic(json);
        }
        catch (Exception ex)
        {
            logger.LogWarning("ChinaPay_ParseFailed: {Json}, Error: {Error}", json, ex.Message);
            return Content("SUCCESS", "text/plain", Encoding.UTF8); // Silent success to avoid retries
        }
        
        // 2. Check response code
        if ((int)body.code != 0 || (string)body.message != "success")
        {
            logger.LogWarning("ChinaPay_NotSuccess: {Json}", json);
            return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }
        
        // 3. Extract required fields
        string sign, paymentNumber, data;
        try
        {
            sign = (string)body.body.sign;
            paymentNumber = (string)body.body.outTradeNo;
            data = (string)body.body.data;
        }
        catch (Exception ex)
        {
            logger.LogWarning("ChinaPay_MissingFields: {Json}, Error: {Error}", json, ex.Message);
            return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }
        
        if (string.IsNullOrEmpty(paymentNumber))
        {
            logger.LogWarning("ChinaPay_MissingPaymentNumber: {Json}", json);
            return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }
        
        // 4. Create tenant scope and validate token from configuration
        using var scopeForConfig = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scopeForConfig == null)
        {
            logger.LogWarning("ChinaPay_TenantNotFound: PaymentNumber={PaymentNumber}", paymentNumber);
            return BadRequest("Tenant not found");
        }
        
        // 4a. Load enforcement configuration and validate callback token
        var configSvc = scopeForConfig.ServiceProvider.GetRequiredService<ConfigService>();
        var enforceTokenConfig = await configSvc.GetAsync<ApplicationConfigure.BoolValue>(
            ConfigCategoryTypes.Public.ToString(), 0, ConfigKeys.EnforcePaymentCallbackToken);
        
        bool enforceToken = enforceTokenConfig?.Value ?? false;
        
        // 4b. Validate callback token if enforcement is enabled
        if (enforceToken)
        {
            var fallbackToken = "8c9d0e1f-2a3b-4c5d-6e7f-8a9b0c1d2e3f"; // Default fallback
            try
            {
                var configuredToken = await configSvc.GetAsync<ApplicationConfigure.StringValue>(
                    ConfigCategoryTypes.Public.ToString(), 0, ConfigKeys.PaymentCallbackToken);
                
                var expectedToken = configuredToken?.Value ?? fallbackToken;
                
                if (string.IsNullOrWhiteSpace(token) || expectedToken != token)
                {
                    logger.LogWarning("ChinaPay_Callback_InvalidToken: ProvidedToken={Token}, ExpectedToken={Expected}", 
                        token ?? "(null)", expectedToken);
                    return NotFound();
                }
                
                logger.LogInformation("ChinaPay_Callback_TokenValidated: Token={Token}", token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ChinaPay_Callback_TokenValidationError: {Error}", ex.Message);
                // If token validation fails due to config error, use fallback and continue
                if (string.IsNullOrWhiteSpace(token) || fallbackToken != token)
                {
                    return NotFound();
                }
            }
        }
        else
        {
            logger.LogInformation("ChinaPay_Callback_TokenEnforcementDisabled: Token={Token}", token ?? "(optional)");
        }
        
        var ctxForConfig = scopeForConfig.ServiceProvider.GetRequiredService<TenantDbContext>();
        // Use common method to get payment and store original callback body
        var paymentForConfig = await PaymentGetWithServiceByNumberAsync(ctxForConfig, paymentNumber, body);
            
        if (paymentForConfig == null)
        {
            logger.LogWarning("ChinaPay_PaymentNotFound: {PaymentNumber}", paymentNumber);
            return BadRequest("Payment not found");
        }

        // 5. CRITICAL: Verify signature (after token validation)
        ChinaPayOptions options = ChinaPayOptions.FromJson(paymentForConfig.PaymentMethod.Configuration);
        var (isValid, errorMsg) = options.Validate();
        if (!isValid)
        {
            logger.LogError("ChinaPay_InvalidConfig: {ErrorMsg}", errorMsg);
            return StatusCode(500, "Configuration error");
        }
        
        if (!ChinaPay.VerifySign(data, options.ServerPublicKey, sign))
        {
            logger.LogWarning("ChinaPay_InvalidSignature: {PaymentNumber}", paymentNumber);
            return BadRequest("Invalid signature");
        }
        
        logger.LogInformation("ChinaPay_SignatureVerified: {PaymentNumber}", paymentNumber);
        
        // 5. Idempotency check via Redis (after signature verification)
        var idempotencyKey = $"chinapay_callback_{paymentNumber}";
        var existingValue = await myCache.GetStringAsync(idempotencyKey);
        if (existingValue != null)
        {
            logger.LogInformation("ChinaPay_DuplicateCallback: {PaymentNumber}", paymentNumber);
            return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }
        
        await myCache.SetStringAsync(idempotencyKey, "processing", TimeSpan.FromMinutes(10));
        
        // 6. Quick validation: Already completed?
        if (paymentForConfig.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("ChinaPay_AlreadyCompleted: {PaymentNumber}", paymentNumber);
            await myCache.KeyDeleteAsync(idempotencyKey);
            return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }
        
        // ========== SLOW PATH: Update Status & Enqueue to Hangfire ==========
        
        try
        {
            // 7. Ensure payment status is "Executing" so admin can manually complete if Hangfire fails, PaymentCompleteAsync() requires Status = Executing!
            paymentForConfig.Status = (short)PaymentStatusTypes.Executing;

            // Store complete callback information with processing metadata
            // 1st time log CallbackBody -> Enqueue 2nd time log CallbackBody -> TryUpdatePaymentAmount 3rd time log CallbackBody
            paymentForConfig.CallbackBody = JsonConvert.SerializeObject(new Payment.CallbackBodyModel
            {
                Status = "callback_received_queued",
                UpdatedOn = DateTime.UtcNow,
                Body = new
                {
                    // Store original vendor callback (the raw request from ChinaPay)
                    vendorCallback = body,
                    // Add processing metadata
                    processingInfo = new
                    {
                        receivedAt = DateTime.UtcNow,
                        signatureVerified = true,
                        note = "Payment set to Executing, queued for background processing"
                    }
                }
            });
            
            ctxForConfig.Payments.Update(paymentForConfig);
            await ctxForConfig.SaveChangesAsync();
            
            logger.LogInformation("ChinaPay_PaymentExecutingAndQueued: {PaymentNumber}", paymentNumber);
            
            // 8. Enqueue background job for async processing
            backgroundJobClient.Enqueue<IPaymentCallbackJob>(x => 
                x.ProcessChinaPayCallbackAsync(paymentNumber, data, sign, json));
            
            logger.LogInformation("ChinaPay_Enqueued: {PaymentNumber}", paymentNumber);
        }
        catch (Exception ex)
        {
            // If enqueue fails, payment is stuck in "Executing" status
            // Admin can see it in approval page and manually complete it
            logger.LogError(ex, "ChinaPay_EnqueueFailed: {PaymentNumber}, Error: {Message}", 
                paymentNumber, ex.Message);
            
            // Still return SUCCESS to ChinaPay to avoid retries
            // Payment is in "Executing" state, admin can manually complete via UI
        }
        
        // 9. Return SUCCESS immediately (fast response to ChinaPay)
        return Content("SUCCESS", "text/plain", Encoding.UTF8);
    }

    [HttpPost("ofa-pay")]
    public async Task<IActionResult> OFACallback([FromBody] Dictionary<string, string> spec, long tenantId)
    {
        logger.LogInformation("OFAPay Callback: {Spec}", spec);
        if (!spec.TryGetValue("orderid", out var paymentNumber))
            return BadRequest("orderid not found");

        if (!spec.TryGetValue("sign", out var sign))
            return BadRequest("sign not found");

        if (!spec.TryGetValue("status", out var status) || status != "1")
            return BadRequest("status not success");

        if (!spec.TryGetValue("amount", out var amount))
            return BadRequest("amount not found");

        using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scope == null) return BadRequest("Tenant not found");
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);
        if (payment == null) return BadRequest("Payment not found");

        if (payment.Status == (int)PaymentStatusTypes.Completed)
            return Ok("success");

        var options = OFAPayOptions.FromJson(payment.PaymentMethod.Configuration);

        if (OFAPay.GenerateSignature(spec, options.SecretKey, logger) != sign)
            return BadRequest("Invalid signature");

        await TryUpdatePaymentAmount(ctx, payment, decimal.Parse(amount));
        await TryCompleteDeposit(scope, payment, spec);
        return Ok("success");
    }


    [HttpPost("paypal")]
    public async Task<IActionResult> PayPalCallback([FromBody] Dictionary<string, string> spec, long tenantId)
    {
        var orderId = spec["orderID"];
        using var scope = await CreateScopeByReferenceNumberAsync(orderId);
        if (scope == null) return BadRequest("Tenant not found");
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByReferenceAsync(ctx, orderId, spec);
        if (payment == null) return BadRequest("Payment not found");
        
        if (payment.Status == (int)PaymentStatusTypes.Completed) 
            return Ok("Payment already completed");

        var options = PayPalOptions.FromJson(payment.PaymentMethod.Configuration);
        var paypalClient = new PayPal(options, logger, httpclientFactory.CreateClient());
        var order = await paypalClient.GetOrderAsync(orderId);
        logger.LogInformation("PayPal_Callback_{Order}", order);

        if (order == null) return BadRequest("Order not found");

        if (order.Status != "APPROVED" && order.Status != "COMPLETED")
            return BadRequest("Order not valid for completion");
        
        var unit = order.PurchaseUnits.FirstOrDefault();
        if (unit == null) return Ok("PurchaseUnit not found");

        if (!decimal.TryParse(unit.Amount.Value, out var amount))
            return Ok("Amount not valid");
        
        if (amount * 100 != payment.Amount) return Ok("Amount not match");
        
        var captureUrl = order.Links.FirstOrDefault(x => x.Rel == "capture")?.Href;
        var capture = await paypalClient.CaptureOrderAsync(orderId, captureUrl);
        if (capture == null) return Ok("Capture not found");
        
        var cbBody = payment.GetCallbackBodyModel();
        cbBody.Status = $"Paypal_user_cb_{order.Status}";
        payment.CallbackBody = cbBody.ToJson();
        await ctx.SaveChangesAsync();
        
        await TryCompleteDeposit(scope, payment, spec);
        return Ok();
    }
    
    [HttpPost("paypal-webhook")]
    public async Task<IActionResult> PayPalWebhook(long tenantId)
    {
        // get all content from request 
        var verifyDetails = await PayPal.WebhookRequestDetails.FromRequestAsync(Request);
        logger.LogInformation("PayPal Webhook: {Detail}", verifyDetails);
        if (!verifyDetails.IsValidRequest())
        {
            return BadRequest("Invalid request");
        }

        var certPem = await myCache.GetStringAsync(PayPal.CertPemCacheKey);
        var client = httpclientFactory.CreateClient();
        if (string.IsNullOrEmpty(certPem))
        {
            certPem = await client.GetStringAsync(verifyDetails.CertUrl);
            await myCache.SetStringAsync(PayPal.CertPemCacheKey, certPem);
        }

        logger.LogInformation("PayPal_Webhook_CertPem {CertPem}", certPem);

        var valid = PayPal.VerifyWebhookRequestAsync(verifyDetails, certPem);
        if (!valid) return BadRequest("Invalid signature");

        var spec = WebhookPayload.FromJson(verifyDetails.RawBody);
        if (spec.EventType != "CHECKOUT.ORDER.APPROVED")
            return Ok("Event not valid");

        var unit = spec.Resource.PurchaseUnits.FirstOrDefault();
        if (unit == null) return Ok("PurchaseUnit not found");

        if (!decimal.TryParse(unit.Amount.Value, out var amount))
            return Ok("Amount not valid");

        var paymentNumber = unit.ReferenceId;

        using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scope == null) return Ok("Tenant not found");

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);
        if (payment == null) return Ok("Payment not found");
        if (amount * 100 != payment.Amount) return Ok("Amount not match");
        if (payment.Status == (int)PaymentStatusTypes.Completed) 
            return Ok("Payment already completed");

        var options = PayPalOptions.FromJson(payment.PaymentMethod.Configuration);
        var service = new PayPal(options, logger, client);

        var captureUrl = spec.Resource.Links.FirstOrDefault(x => x.Rel == "capture")?.Href;
        if (string.IsNullOrEmpty(captureUrl)) return Ok("CaptureUrl not found");

        var capture = await service.CaptureOrderAsync(spec.Resource.Id, captureUrl);
        if (capture == null) return Ok("Capture not found");

        logger.LogInformation("PayPal_Webhook_Capture {Capture}", capture);

        if (capture.Status != "COMPLETED") return Ok("Capture not completed");
        await TryCompleteDeposit(scope, payment, spec);
        return Ok();
    }

    [HttpPost("usepay")]
    public async Task<IActionResult> UsePayCallback([FromBody] object spec)
    {
        await Task.Delay(0);
        return NoContent();
    }

    [HttpPost("monetix/success")]
    public async Task<IActionResult> MonetixSuccess2()
    {
        var json = await GetRequestBody();

        var model = MonetixPayCallbackResponse.Root.FromJson(json);
        if (model.Operation.Status != "success") return BadRequest();

        var paymentNumber = model.Payment.Id;
        using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scope == null) return BadRequest("Tenant not found");

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, model);
        if (payment == null) return BadRequest("Payment not found");

        if (payment.Status == (int)PaymentStatusTypes.Completed)
            return Ok();

        var options = MonetixOptions.FromJson(payment.PaymentMethod.Configuration);
        var result = Monetix.VerifySignatureFromCb(json, model.Signature, options.SecretKeyForSignature);
        if (!result) return BadRequest("Invalid signature");

        var amount = model.Operation.SumInitial.Amount * 100;
        if (amount != payment.Amount) return BadRequest("Amount not match");

        await TryCompleteDeposit(scope, payment, model);
        return NoContent();
    }

    [HttpPost("monetix/fail")]
    public async Task<IActionResult> MonetixFail()
    {
        return NoContent();
    }

    [HttpPost("bakong")]
    [HttpPost("dragon-pay")]
    public async Task<IActionResult> BakongSuccess(long tenantId, [FromForm] Dictionary<string, string> spec)
    {
        logger.LogInformation("BakongSuccess Callback: {Spec}", spec);

        if (!spec.TryGetValue("merchant_reference", out var paymentNumber))
        {
            logger.LogInformation("BakongSuccess Callback: Payment Number not found {Spec}", spec);
            return BadRequest();
        }

        using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scope == null)
        {
            logger.LogInformation("BakongSuccess Callback: Tenant Scope not found {Spec}", spec);
            return BadRequest();
        }

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);
        if (payment == null)
        {
            logger.LogInformation("BakongSuccess Callback: Payment Number not found {Spec}", spec);
            return BadRequest();
        }

        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("BakongSuccess Callback: Payment already completed {Spec}", spec);
            return Ok();
        }

        var options = BakongOptions.FromJson(payment.PaymentMethod.Configuration);
        var (result, msg) = Bakong.ValidateCallbackSpec(spec, options.MerchantSecret);
        if (!result)
        {
            logger.LogWarning("BakongSuccess Callback: Invalid signature {Request}, error_msg:{msg}", spec, msg);
            return BadRequest();
        }

        var amountInDecimal = decimal.Parse(spec["amount"]);
        await TryUpdatePaymentAmount(ctx, payment, amountInDecimal);

        switch (spec["status"])
        {
            case "1":
                logger.LogWarning("BakongSuccess Callback: Payment status is accepted {Spec}", spec);
                await TryCompleteDeposit(scope, payment, spec);
                return Ok("success");
            default:
                logger.LogWarning("BakongSuccess Callback: Payment status is not success {Spec}", spec);
                return BadRequest($"BakongSuccess Callback: Payment status is not success => {spec["status"]}");
        }
    }

    [HttpPost("help2pay")]
    public async Task<IActionResult> Help2PayCallback(long tenantId, [FromForm] Dictionary<string, string> spec)
    {
        logger.LogInformation("Help2Pay Callback: {Spec}", spec);
        // BcrLog.Slack($"Help2Pay Callback: {JsonConvert.SerializeObject(spec)}");
        if (!spec.TryGetValue("Reference", out var paymentNumber))
        {
            logger.LogInformation("Help2Pay Callback: Payment Number not found {Spec}", spec);
            return BadRequest();
        }

        using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scope == null)
        {
            logger.LogInformation("Help2Pay Callback: Tenant Scope not found {Spec}", spec);
            return BadRequest();
        }

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);
        if (payment == null)
        {
            logger.LogInformation("Help2Pay Callback: Payment Number not found {Spec}", spec);
            return BadRequest();
        }

        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("Help2Pay Callback: Payment already completed {Spec}", spec);
            return Ok();
        }

        var options = Help2PayOptions.FromJson(payment.PaymentMethod.Configuration);
        var (result, msg) = Help2Pay.ValidateCallbackSpec(spec, options.SecurityCode);
        if (!result)
        {
            logger.LogWarning("Help2Pay Callback: Invalid signature {msg}", msg);
            return BadRequest();
        }

        switch (spec["Status"])
        {
            case "009":
                logger.LogWarning("Help2Pay Callback: Payment status is pending {Spec}", spec);
                return Ok($"{paymentNumber} Pending");
            case "001":
                await ChangePaymentStatus(ctx, payment, PaymentStatusTypes.Failed);
                await TryCancelDeposit(scope, payment.Number);
                return Ok($"{paymentNumber} Failed");
            case "007":
                await ChangePaymentStatus(ctx, payment, PaymentStatusTypes.Rejected);
                await TryCancelDeposit(scope, payment.Number);
                return Ok($"{paymentNumber} Rejected");
            case "008":
                await ChangePaymentStatus(ctx, payment, PaymentStatusTypes.Cancelled);
                await TryCancelDeposit(scope, payment.Number);
                return Ok($"{paymentNumber} Cancelled");
            case "000":
            case "006":
                await TryCompleteDeposit(scope, payment, spec);
                return Ok();
            default:
                logger.LogWarning("Help2Pay Callback: Payment status is not success {Spec}", spec);
                return BadRequest($"Help2Pay Callback: Payment status is not success => {spec["Status"]}");
        }
    }

    [HttpPost("poli")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> PoliSuccessCallback(long tenantId, [FromForm] IFormCollection spec)
    {
        spec.TryGetValue("Token", out var tokenParameter);

        var token = HttpUtility.UrlDecode(tokenParameter.ToString().Trim());
        if (string.IsNullOrEmpty(token) || token.Length < 6)
        {
            logger.LogWarning("Poli Callback: Invalid token {Request}", spec);
            return BadRequest(Result.Error(ResultMessage.Common.InvalidToken));
        }

        var tenant = await centralDbContext.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            logger.LogWarning("Long77Pay Callback: Tenant not found {@Request}", spec);
            return BadRequest();
        }

        using var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenant.Id);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, token, spec);
        if (payment == null)
        {
            logger.LogWarning("Poli Callback: Payment not found {Request}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        await TryCompleteDeposit(scope, payment, spec);
        logger.LogInformation("Poli Callback: success {Request}", spec);
        return Content("SUCCESS", "text/plain", Encoding.UTF8);
    }


    [HttpPost("exlink")]
    public async Task<IActionResult> ExLinkCallback(long tenantId, [FromBody] Dictionary<string, string> spec)
    {
        // if (!ExLink.CallbackRequest.ValidateSignature(spec))
        if (!spec.ContainsKey("uniqueCode") || !long.TryParse(spec["uniqueCode"], out var accountUid))
        {
            logger.LogWarning("ExLink Callback: AccountUid not found {@Request}", spec);
            return BadRequest();
        }

        if (!spec.TryGetValue("apiOrderNo", out var paymentNumber))
        {
            logger.LogWarning("ExLink Callback: Payment Number not found {@Request}", spec);
            return BadRequest();
        }

        if (!spec.ContainsKey("signature"))
        {
            logger.LogWarning("ExLink Callback: Signature not found {@Request}", spec);
            return BadRequest();
        }

        var tenant = await centralDbContext.CentralAccounts
            .Where(x => x.Uid == accountUid)
            .Select(x => x.Tenant)
            .FirstOrDefaultAsync();

        if (tenant == null)
        {
            logger.LogWarning("ExLink Callback: Tenant not found {@Request}", spec);
            return BadRequest();
        }

        using var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenant.Id);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);

        if (payment == null)
        {
            logger.LogWarning("ExLink Callback: Payment not found {@Request}", spec);
            return BadRequest();
        }

        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("ExLink Callback: Payment already completed {@Request}", spec);
            return Ok();
        }

        var options = ExLinkOptions.FromJson(payment.PaymentMethod.Configuration);
        if (!ExLink.ValidateCallbackSignature(spec, options.CallbackSecretKey))
        {
            logger.LogWarning("ExLink Callback: Invalid signature {@Request}", spec);
            return BadRequest();
        }

        await TryCompleteDeposit(scope, payment, spec);
        return Ok();
    }

    [HttpPost("exlink-cashier")]
    public async Task<IActionResult> ExLinkCashierCallback(long tenantId, [FromBody] Dictionary<string, object> spec)
    {
        logger.LogInformation("ExLinkCashier Callback: {Spec}", spec);

        // Extract required parameters
        if (!spec.TryGetValue("signature", out var signatureRaw))
            return BadRequest("signature not found");
        if (!spec.TryGetValue("merchantOrderNo", out var paymentNumberRaw))
            return BadRequest("merchantOrderNo not found");
        if (!spec.TryGetValue("tradeStatus", out var tradeStatusRaw))
            return BadRequest("tradeStatus not found");
        if (!spec.TryGetValue("orderAmount", out var orderAmountRaw))
            return BadRequest("orderAmount not found");

        var signature = signatureRaw.ToString() ?? string.Empty;
        var paymentNumber = paymentNumberRaw.ToString() ?? string.Empty;
        var tradeStatus = (long)tradeStatusRaw;
        
        // Only process successful payments
        if (tradeStatus != 3)
        {
            logger.LogWarning("ExLinkCashier Callback: Trade status not success. Status={TradeStatus}, PaymentNumber={PaymentNumber}", 
                tradeStatus, paymentNumber);
            return BadRequest("tradeStatus not success");
        }
        
        var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scope == null)
        {
            logger.LogWarning("ExLinkCashier Callback: Tenant not found for PaymentNumber={PaymentNumber}", paymentNumber);
            return BadRequest("Tenant not found");
        }

        using (scope)
        {
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);
            if (payment == null)
            {
                logger.LogWarning("ExLinkCashier Callback: Payment not found. PaymentNumber={PaymentNumber}", paymentNumber);
                return BadRequest("Payment not found");
            }

            var successObj = new
            {
                code = 1,
                message = "成功",
                data = "success",
                success = true
            };

            // If already completed, return success to prevent retry
            if (payment.Status == (int)PaymentStatusTypes.Completed)
            {
                logger.LogInformation("ExLinkCashier Callback: Payment already completed. PaymentNumber={PaymentNumber}", paymentNumber);
                return Ok(successObj);
            }

            var options = ExLinkCashierOptions.FromJson(payment.PaymentMethod.Configuration);
            
            // SECURITY: Verify signature using CallbackSecretKey
            var sig = ExLinkCashier.GenerateSignature(spec, options.CallbackSecretKey, logger);
            if (sig != signature)
            {
                logger.LogWarning("ExLinkCashier Callback: Invalid signature. Expected={Expected}..., Received={Received}..., PaymentNumber={PaymentNumber}",
                    sig.Length > 8 ? sig[..8] : sig,
                    signature.Length > 8 ? signature[..8] : signature, 
                    paymentNumber);
                return BadRequest("Invalid signature");
            }

            // SECURITY: Verify orderAmount matches payment amount
            // orderAmount is in the currency's smallest unit (cents for USD, VND itself, etc.)
            var orderAmount = Convert.ToInt64(orderAmountRaw);
            var expectedAmount = payment.Amount; // Already in cents/smallest unit
            
            if ((orderAmount*100).ToScaledFromCents() != expectedAmount)
            {
                logger.LogWarning("ExLinkCashier Callback: Amount mismatch. Expected={Expected}, Received={Received}, PaymentNumber={PaymentNumber}", 
                    expectedAmount, orderAmount, paymentNumber);
                return BadRequest($"Amount mismatch: expected {expectedAmount}, received {orderAmount}");
            }

            logger.LogInformation("ExLinkCashier Callback: Validation passed. Processing payment. PaymentNumber={PaymentNumber}, Amount={Amount}", 
                paymentNumber, orderAmount);

            await TryCompleteDeposit(scope, payment, spec);
            return Ok(successObj);
        }
    }

    [HttpPost("uniotc-pay/{status}")]
    public async Task<IActionResult> UniotcCallback(long tenantId, [FromBody] Dictionary<string, string> spec,
        string status)
    {
        if (!spec.TryGetValue("signature", out _) && !spec.TryGetValue("sign", out _))
        {
            logger.LogInformation("Uniotc Callback: Sign not found {Spec}", spec);
            return BadRequest();
        }

        if (!spec.TryGetValue("apiOrderNo", out var paymentNumber))
        {
            logger.LogInformation("Uniotc Callback: Payment Number not set {Spec}", spec);
            return BadRequest();
        }

        var tenant = await centralDbContext.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            logger.LogWarning("Uniotc Callback: Tenant not found {@Request}", spec);
            return BadRequest();
        }

        using var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenant.Id);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);
        if (payment == null)
        {
            logger.LogInformation("Uniotc Callback: Payment Number not found {Spec}", spec);
            return BadRequest();
        }

        var options = JsonConvert.DeserializeObject<UniotcPayOptions>(payment.PaymentMethod.Configuration);

        if (options == null || !options.IsValid() || !payment.CanComplete())
        {
            logger.LogInformation("Uniotc Callback: Invalid option or payment {Spec}", spec);
            return BadRequest();
        }

        var isValid = UniotcPayRequestModel.VerifySign(spec, options.CallbackSecurityKey, true);
        if (!isValid)
        {
            logger.LogWarning("Uniotc Callback: Invalid signature {Request}", spec);
            return BadRequest();
        }

        if (status != "success")
        {
            logger.LogWarning("Uniotc Callback: Payment status is not success {Request}", spec);
            return Ok(new
            {
                code = -2,
                msg = "failure",
                success = false,
                data = new { }
            });
        }

        try
        {
            logger.LogInformation("Uniotc Callback: Success {Request}", spec);
            var response = new
            {
                code = 200,
                msg = "success",
                success = true,
                data = new
                {
                    apiOrderNo = spec["apiOrderNo"],
                    companyOrderNum = spec["companyOrderNum"],
                }
            };
            await TryCompleteDeposit(scope, payment, spec);
            return Ok(response);
        }
        catch
        {
            logger.LogWarning("Uniotc Callback: Payment complete fail {Request}", spec);
            return Ok(new
            {
                code = 1,
                msg = "fail",
                success = false,
                data = new
                {
                    apiOrderNo = spec["apiOrderNo"],
                    companyOrderNum = spec["companyOrderNum"],
                }
            });
        }
    }

    [HttpPost("chip-pay")]
    public async Task<IActionResult> ChipPayCallback(long tenantId, [FromBody] Dictionary<string, string> spec)
    {
        if (!spec.TryGetValue("sign", out var sign))
        {
            logger.LogInformation("ChipPay Callback: Sign not found {Spec}", spec);
            return BadRequest();
        }

        if (!spec.TryGetValue("companyOrderNum", out var paymentNumber))
        {
            logger.LogInformation("ChipPay Callback: Payment Number not found {Spec}", spec);
            return BadRequest();
        }

        if (!spec.TryGetValue("intentOrderNo", out var referenceNumber))
        {
            logger.LogInformation("ChipPay Callback: Payment RefNumber not found {Spec}", spec);
            return BadRequest();
        }

        using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scope == null)
        {
            logger.LogInformation("ChipPay Callback: Tenant Scope not found {Spec}", spec);
            return BadRequest();
        }

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);
        if (payment == null)
        {
            logger.LogInformation("ChipPay Callback: Payment Number not found {Spec}", spec);
            return BadRequest();
        }

        if (payment.ReferenceNumber != referenceNumber)
        {
            logger.LogInformation("ChipPay Callback: Payment RefNumber not found {Spec}", spec);
            return BadRequest();
        }

        var response = new
        {
            code = 200,
            msg = "success",
            success = true,
            data = new { intentOrderNo = referenceNumber, companyOrderNum = paymentNumber, }
        };

        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("ChipPay Callback: Payment already completed {Spec}", spec);
            return Ok(response);
        }

        var options = ChipPayOptions.FromJson(payment.PaymentMethod.Configuration);

        if (!ChipPay.ValidateCallbackDict(spec, options.PublicKey))
        {
            logger.LogWarning("ChipPay Callback: Invalid signature {Request}", spec);
            return BadRequest();
        }

        if (!spec.TryGetValue("tradeStatus", out var tradeStatus))
        {
            logger.LogInformation("ChipPay Callback: TradeStatus not found {Spec}", spec);
            return BadRequest();
        }

        if (tradeStatus != "1")
        {
            logger.LogInformation("ChipPay Callback: TradeStatus not success {Spec}", spec);
            await TryCancelDeposit(scope, payment.Number);
            return Ok(response);
        }

        logger.LogInformation("ChipPay Callback: Success {Request}", spec);
        await TryCompleteDeposit(scope, payment, spec);
        return Ok(response);
    }

    // TODO: create view model for callback request
    [HttpPost("gpay")]
    public async Task<IActionResult> GPayCallback(long tenantId, [FromForm] Dictionary<string, string> spec)
    {
        if (!spec.TryGetValue("company_order_num", out var paymentNumber))
        {
            logger.LogInformation("GPay Callback: Payment Number not found {Spec}", spec);
            return BadRequest();
        }

        using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scope == null)
        {
            logger.LogInformation("GPay Callback: Tenant Scope not found {Spec}", spec);
            return BadRequest();
        }

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);
        if (payment == null)
        {
            logger.LogInformation("GPay Callback: Payment Number not found {Spec}", spec);
            return BadRequest();
        }

        var options = GPayOptions.FromJson(payment.PaymentMethod.Configuration);
        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("GPay Callback: Payment already completed {Spec}", spec);
            return Ok();
        }

        if (!GPay.CallbackValidator(spec, options.MerchantSecret))
        {
            logger.LogWarning("GPay Callback: Invalid signature {Request}", spec);
            return BadRequest();
        }

        await TryUpdatePaymentAmount(ctx, payment, decimal.Parse(spec["amount"]));
        logger.LogInformation("GPay Callback: Success {Request}", spec);
        await TryCompleteDeposit(scope, payment, spec);
        return NotFound();
    }

    [HttpGet("test")]
    [HttpPost("test")]
    public async Task<IActionResult> TestCallback(long tenantId)
    {
        logger.LogInformation("Test_Callback_Received: TenantId={TenantId}, Method={Method}", 
            tenantId, Request.Method);
            
        foreach (var header in Request.Headers)
        {
            logger.LogInformation("Test_Callback_Header: {Key}={Value}", 
                header.Key, string.Join(", ", header.Value));
        }
        
        if (Request.Method == "POST")
        {
            var body = await GetRequestBody();
            logger.LogInformation("Test_Callback_Body: {Body}", body);
        }
        
        return Ok(new { 
            success = true, 
            message = "Test callback received successfully",
            tenantId = tenantId,
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("bipi-pay")]
    public async Task<IActionResult> BipiPayCallback(long tenantId, [FromQuery] BipiPayCallbackViewModel spec)
    {
        if (string.IsNullOrEmpty(spec.GePaymentNumber()))
        {
            logger.LogInformation("Bipi Pay Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        var tenant = await centralDbContext.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            logger.LogWarning("Long77Pay Callback: Tenant not found {@Request}", spec);
            return BadRequest();
        }

        using var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenant.Id);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, spec.GePaymentNumber(), spec);
        if (payment == null)
        {
            logger.LogInformation("Bipi Pay Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        var options = JsonConvert.DeserializeObject<BipiPayOptions>(payment.PaymentMethod.Configuration);

        if (options == null || !options.IsValid() || !payment.CanComplete())
        {
            logger.LogInformation("Bipi Pay Callback: Invalid option or payment {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        spec.ApplyOptions(options);
        if (!spec.IsValid())
        {
            logger.LogWarning("Bipi Pay Callback: Invalid signature {@Request}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        if (!spec.IsSuccess())
        {
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        payment.ReferenceNumber = spec.GePaymentNumber();
        ctx.Payments.Update(payment);
        await ctx.SaveChangesAsync();

        await TryCompleteDeposit(scope, payment, spec);
        return Content("SUCCESS", "text/plain", Encoding.UTF8);
    }

    [HttpPost("buy365")]
    public async Task<IActionResult> Buy365Callback(long tenantId, [FromBody] Dictionary<string, string> spec)
    {
        if (!spec.TryGetValue("bill_no", out var paymentNumber) || string.IsNullOrEmpty(paymentNumber))
        {
            logger.LogInformation("Buy365 Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        if (!spec.TryGetValue("amount", out var ams)
            || !decimal.TryParse(ams, out var amount)
            || amount < 1)
        {
            logger.LogInformation("Buy365 Callback: Payment amount {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
        if (scope == null)
        {
            logger.LogInformation("Buy365 Callback: Tenant Scope not found {@Spec}", spec);
            return BadRequest();
        }

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, spec);
        if (payment == null)
        {
            logger.LogInformation("Buy365 Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("Buy365 Callback: Payment already completed {@Spec}, {pm}", spec, payment.Number);
            return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }

        await TryUpdatePaymentAmount(ctx, payment, amount);
        var options = NPayOptions.FromJson(payment.PaymentMethod.Configuration);
        if (!NPay.ValidateCallbackSignature(spec, options.CallbackSecret))
        {
            logger.LogWarning("Buy365 Callback: Invalid signature {@Request}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        await TryCompleteDeposit(scope, payment, spec);
        return Content("SUCCESS", "text/plain", Encoding.UTF8);
    }


    [HttpPost("unione-pay")]
    public async Task<IActionResult> UnionePayXCallback(long tenantId, [FromForm] UnionePay.CallbackRequest spec)
    {
        if (string.IsNullOrEmpty(spec.OrderNo))
        {
            logger.LogInformation("UnioneXPay Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        var scope = await CreateScopeByPaymentNumberAsync(spec.OrderNo);
        if (scope == null)
        {
            logger.LogInformation("UnioneXPay Callback: Tenant Scope not found {@Spec}", spec);
            return BadRequest();
        }

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, spec.OrderNo, spec);
        if (payment == null)
        {
            logger.LogInformation("UnioneXPay Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("UnioneXPay Callback: Payment already completed {@Spec}, {pm}", spec,
                payment.Number);
            return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }

        await TryUpdatePaymentAmount(ctx, payment, spec.RealAmount);
        var options = UnionePayOptions.FromJson(payment.PaymentMethod.Configuration);
        spec.Options = options;
        if (!spec.ValidateSign())
        {
            logger.LogWarning("UnioneXPay Callback: Invalid signature {@Request}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        await TryCompleteDeposit(scope, payment, spec);
        return Content("SUCCESS", "text/plain", Encoding.UTF8);
    }

    [HttpPost("long77-pay")]
    public async Task<IActionResult> Long77PayCallback(long tenantId, [FromBody] Long77PayCallbackViewModel spec)
    {
        if (string.IsNullOrEmpty(spec.PartnerOrderCode))
        {
            logger.LogWarning("Long77Pay Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        var tenant = await centralDbContext.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            logger.LogWarning("Buy365 Callback: Tenant not found {@Request}", spec);
            return BadRequest();
        }

        using var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenant.Id);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, spec.PartnerOrderCode, spec);
        if (payment == null)
        {
            logger.LogWarning("Long77Pay Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        var options = JsonConvert.DeserializeObject<Long77PayOptions>(payment.PaymentMethod.Configuration);

        if (options == null || !options.IsValid() || !payment.CanComplete())
        {
            logger.LogWarning("Long77Pay Callback: Invalid option or payment {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        spec.ApplyOptions(options);

        if (!spec.IsValid())
        {
            logger.LogWarning("Long77Pay Callback: Invalid signature {@Request}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        await TryCompleteDeposit(scope, payment, spec);
        return Content("SUCCESS", "text/plain", Encoding.UTF8);
    }

    [HttpPost("long77-pay/usdt")]
    public async Task<IActionResult> Long77PayUsdtCallback(long tenantId,
        [FromBody] Long77PayUsdtCallbackViewModel spec)
    {
        if (string.IsNullOrEmpty(spec.PartnerOrderCode))
        {
            logger.LogWarning("Long77PayUSDT Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        var tenant = await centralDbContext.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            logger.LogWarning("Long77Pay Callback: Tenant not found {@Request}", spec);
            return BadRequest();
        }

        using var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenant.Id);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, spec.PartnerOrderCodeFormatted, spec);
        if (payment == null)
        {
            logger.LogWarning("Long77PayUSDT Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        var options = JsonConvert.DeserializeObject<Long77PayUsdtOptions>(payment.PaymentMethod.Configuration);

        if (options == null || !options.IsValid() || !payment.CanComplete())
        {
            logger.LogWarning("Long77PayUSDT Callback: Invalid option or payment {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        spec.ApplyOptions(options);

        if (!spec.IsValid())
        {
            logger.LogWarning("Long77PayUSDT Callback: Invalid signature {@Request}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        await TryCompleteDeposit(scope, payment, spec);
        return Content("SUCCESS", "text/plain", Encoding.UTF8);
    }

    [HttpPost("big-pay")]
    public async Task<IActionResult> BigPayCallback(long tenantId, [FromBody] BigPayCallbackViewModel spec)
    {
        logger.LogWarning("BigPay VND Callback:  {@Spec}", spec);
        if (spec.OrderType != "DEPOSIT")
        {
            return BadRequest(BigPay.CallbackFailureResponse(ResultMessage.Deposit.PaymentNotFound));
        }

        using var scope = await CreateScopeByPaymentNumberAsync(spec.OrderRef);
        if (scope == null)
        {
            logger.LogWarning("BigPay VND Callback: Tenant or PaymentNumber not found {@Request}", spec);
            return BadRequest(BigPay.CallbackFailureResponse("Tenant or PaymentNumber not found"));
        }

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var payment = await PaymentGetWithServiceByNumberAsync(ctx, spec.OrderRef, spec);
        if (payment == null)
        {
            logger.LogWarning("BigPay VND Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(BigPay.CallbackFailureResponse(ResultMessage.Deposit.PaymentNotFound));
        }

        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("BigPay VND Callback: Payment already completed {@Spec}", spec);
            return Ok(BigPay.CallbackSuccessResponse());
        }

        var options = BigPayOptions.FromJson(payment.PaymentMethod.Configuration);
        if (spec.SecretKey != options.CallbackSecretKey)
        {
            logger.LogWarning("BigPay VND Callback: Invalid signature {@Request}", spec);
            return BadRequest(BigPay.CallbackFailureResponse(ResultMessage.Deposit.InvalidParameters));
        }

        if (!spec.OrderStatus.Equals("SUCCEED"))
        {
            logger.LogWarning("BigPay VND payment failed, status is not SUCCESS  {@Request}", spec);
            return Ok(BigPay.CallbackSuccessResponse());
        }

        logger.LogInformation("BigPay VND payment success {@Request}", spec);
        await TryCompleteDeposit(scope, payment, spec);
        return Ok(BigPay.CallbackSuccessResponse());
    }

    [HttpPost("fivepay-f2f")]
    public async Task<IActionResult> FivePayF2FCallback(long tenantId, [FromBody] Dictionary<string, string> spec)
    {
        logger.LogWarning("FivePayF2F Callback:  {@Spec}", spec);
        if (!FivePayF2F.ValidateCallbackSignature(spec))
        {
            logger.LogWarning("FivePayF2F Callback: Invalid signature {@Request}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        if (!spec.TryGetValue("merchantOrderNo", out var referenceNumber))
        {
            logger.LogWarning("FivePayF2F Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        using var scope = await CreateScopeByReferenceNumberAsync(referenceNumber);
        if (scope == null)
        {
            logger.LogWarning("FivePayF2F Callback: Tenant or PaymentNumber not found {@Request}", spec);
            return BadRequest();
        }

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var payment = await PaymentGetWithServiceByReferenceAsync(ctx, referenceNumber, spec);
        if (payment == null)
        {
            logger.LogWarning("FivePayF2F Callback: Payment not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        var options = FivePayF2FOptions.FromJson(payment.PaymentMethod.Configuration);
        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("FivePayF2F Callback: Payment already completed {@Spec}", spec);
            return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }

        if (!spec.TryGetValue("orderAmount", out var amsEncrypted) ||
            !decimal.TryParse(Utils.TripleDESDecrypt(amsEncrypted, options.SecretKey), out var amountInDecimal))
        {
            logger.LogWarning("FivePayF2F Callback: Payment amount {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        if (!spec.TryGetValue("status", out var statusEncrypted) ||
            !int.TryParse(Utils.TripleDESDecrypt(statusEncrypted, options.SecretKey), out var status))
        {
            logger.LogWarning("FivePayF2F Callback: Payment status {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        switch (status)
        {
            case 1:
                logger.LogInformation("FivePayF2F Callback: Payment Created {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 2:
                logger.LogInformation("FivePayF2F Callback: Pending Customer Payment {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 3:
                logger.LogInformation("FivePayF2F Callback: Customer Payment Finished {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 4:
                logger.LogInformation("FivePayF2F Callback: Customer Payment Received {@Request}", spec);
                await TryUpdatePaymentAmount(ctx, payment, amountInDecimal);
                await TryCompleteDeposit(scope, payment, spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 6:
                logger.LogWarning("FivePayF2F Callback: Payment Expired {@Request}", spec);
                await TryCancelDeposit(scope, payment.Number);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 7:
                logger.LogWarning("FivePayF2F Callback: Payment Cancelled {@Request}", spec);
                await TryCancelDeposit(scope, payment.Number);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 8:
                logger.LogWarning("FivePayF2F Callback: Payment Unfrozen {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 9:
                logger.LogWarning("FivePayF2F Callback: Payment Sued {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 10:
                logger.LogWarning("FivePayF2F Callback: Payment Sued Failed {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            default:
                logger.LogInformation("FivePayF2F Callback: Status {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }
    }

    [HttpPost("fivepay-va")]
    public async Task<IActionResult> FivePayVACallback(long tenantId, [FromBody] Dictionary<string, string> spec)
    {
        logger.LogWarning("FivePayVA Callback:  {@Spec}", spec);
        if (!FivePayVA.ValidateCallbackSignature(spec))
        {
            logger.LogWarning("FivePayVA Callback: Invalid signature {@Request}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        if (!spec.TryGetValue("merchantOrderNo", out var referenceNumber))
        {
            logger.LogWarning("FivePayVA Callback: Payment RefNumber not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        using var scope = await CreateScopeByReferenceNumberAsync(referenceNumber);
        if (scope == null)
        {
            logger.LogWarning("FivePayVA Callback: Tenant or PaymentNumber not found {@Request}", spec);
            return BadRequest();
        }

        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var payment = await PaymentGetWithServiceByReferenceAsync(ctx, referenceNumber, spec);
        if (payment == null)
        {
            logger.LogWarning("FivePayVA Callback: Payment not found {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentNotFound));
        }

        var options = FivePayF2FOptions.FromJson(payment.PaymentMethod.Configuration);
        if (payment.Status == (int)PaymentStatusTypes.Completed)
        {
            logger.LogInformation("FivePayVA Callback: Payment already completed {@Spec}", spec);
            return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }


        if (!spec.TryGetValue("orderAmount", out var amsEncrypted) ||
            !decimal.TryParse(Utils.TripleDESDecrypt(amsEncrypted, options.SecretKey), out var amountInDecimal))
        {
            logger.LogWarning("FivePayVA Callback: Payment amount {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        if (!spec.TryGetValue("status", out var statusEncrypted) ||
            !int.TryParse(Utils.TripleDESDecrypt(statusEncrypted, options.SecretKey), out var status))
        {
            logger.LogWarning("FivePayVA Callback: Payment status {@Spec}", spec);
            return BadRequest(Result.Error(ResultMessage.Deposit.InvalidParameters));
        }

        switch (status)
        {
            case 1:
                logger.LogInformation("FivePayVA Callback: Payment Created {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 2:
                logger.LogInformation("FivePayVA Callback: Pending Customer Payment {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 3:
                logger.LogInformation("FivePayVA Callback: Customer Payment Finished {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 4:
                logger.LogInformation("FivePayVA Callback: Customer Payment Received {@Request}", spec);
                await TryUpdatePaymentAmount(ctx, payment, amountInDecimal);
                await TryCompleteDeposit(scope, payment, spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 6:
                logger.LogWarning("FivePayVA Callback: Payment Expired {@Request}", spec);
                await TryCancelDeposit(scope, payment.Number);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 7:
                logger.LogWarning("FivePayVA Callback: Payment Cancelled {@Request}", spec);
                await TryCancelDeposit(scope, payment.Number);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 8:
                logger.LogWarning("FivePayVA Callback: Payment Unfrozen {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 9:
                logger.LogWarning("FivePayVA Callback: Payment Sued {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            case 10:
                logger.LogWarning("FivePayVA Callback: Payment Sued Failed {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
            default:
                logger.LogInformation("FivePayVA Callback: Status {@Request}", spec);
                return Content("SUCCESS", "text/plain", Encoding.UTF8);
        }
    }

    private async Task<IServiceScope?> CreateScopeByReferenceNumberAsync(string referenceNumber)
    {
        var cacheKey = Payment.GetReferenceNumberTenantIdKey(referenceNumber);
        var tidString = await myCache.GetStringAsync(cacheKey);
        await myCache.KeyDeleteAsync(cacheKey);

        if (tidString is { Length: 5 } && long.TryParse(tidString, out var t))
            return serviceProvider.CreateTenantScope(t);

        var results = await Task.WhenAll(pool.GetTenantIds().Select(async tid =>
        {
            await using var ctx = pool.CreateTenantDbContext(tid);
            var exists = await ctx.Payments.AnyAsync(p => p.ReferenceNumber == referenceNumber);
            return exists ? tid : 0;
        }));

        var tenantId = results.FirstOrDefault(x => x != 0);
        return tenantId == 0 ? null : serviceProvider.CreateTenantScope(tenantId);
    }


    private async Task<IServiceScope?> CreateScopeByPaymentNumberAsync(string paymentNumber)
    {
        var cacheKey = Payment.GetPaymentNumberTenantIdKey(paymentNumber);
        var tidString = await myCache.GetStringAsync(cacheKey);
        await myCache.KeyDeleteAsync(cacheKey);

        if (tidString is { Length: 5 } && long.TryParse(tidString, out var t))
            return serviceProvider.CreateTenantScope(t);

        var results = await Task.WhenAll(pool.GetTenantIds().Select(async tid =>
        {
            await using var ctx = pool.CreateTenantDbContext(tid);
            var exists = await ctx.Payments.AnyAsync(p => p.Number == paymentNumber);
            return exists ? tid : 0;
        }));

        var tenantId = results.FirstOrDefault(x => x != 0);
        return tenantId == 0 ? null : serviceProvider.CreateTenantScope(tenantId);
    }

    private static async Task<Payment?> PaymentGetWithServiceByNumberAsync(TenantDbContext ctx, string number,
        object cbBody)
    {
        var payment = await ctx.Payments
            .Include(x => x.PaymentMethod)
            .FirstOrDefaultAsync(x => x.Number == number);

        if (payment == null) return payment;

        payment.CallbackBody = JsonConvert.SerializeObject(new Payment.CallbackBodyModel
        {
            Status = "received",
            UpdatedOn = DateTime.UtcNow,
            Body = cbBody
        });
        ctx.Payments.Update(payment);
        await ctx.SaveChangesAsync();

        return payment;
    }

    private static async Task<Payment?> PaymentGetWithServiceByReferenceAsync(TenantDbContext ctx, string @ref,
        object cbBody)
    {
        var payment = await ctx.Payments
            .Include(x => x.PaymentMethod)
            .FirstOrDefaultAsync(x => x.ReferenceNumber == @ref);

        if (payment == null) return payment;

        payment.CallbackBody = JsonConvert.SerializeObject(new Payment.CallbackBodyModel
        {
            Status = "received",
            UpdatedOn = DateTime.UtcNow,
            Body = cbBody
        });

        ctx.Payments.Update(payment);
        await ctx.SaveChangesAsync();

        return payment;
    }


    private static async Task ChangePaymentStatus(TenantDbContext ctx, Payment payment, PaymentStatusTypes status)
    {
        payment.Status = (short)status;
        payment.UpdatedOn = DateTime.UtcNow;
        ctx.Payments.Update(payment);
        await ctx.SaveChangesAsync();
    }

    /// <summary>
    /// Update Payment Amount with callback data
    /// </summary>
    /// <param name="amountInDecimal">The real amount, 需要扩大6位再保存进库</param>
    /// <returns></returns>
    private static async Task TryUpdatePaymentAmount(TenantDbContext ctx, Payment payment, decimal amountInDecimal)
    {
        var amount = (long)Math.Round((amountInDecimal * 100).ToScaledFromCents(), 0);
        if (amount == payment.Amount)
            return;

        payment.Amount = amount;
        payment.UpdatedOn = DateTime.UtcNow;
        ctx.Payments.Update(payment);
        await ctx.SaveChangesAsync();
    }

    private static async Task TryCancelDeposit(IServiceScope scope, string paymentNumber)
    {
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var acctSvc = scope.ServiceProvider.GetRequiredService<AcctService>();
        var depositId = await ctx.Deposits
            .Where(x => x.Payment.Number == paymentNumber)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (depositId == 0)
            return;

        await acctSvc.DepositCancelAsync(depositId, DefaultParty.System, "Cancel Deposit by Callback");
    }

    public static async Task TryCompleteDeposit(IServiceScope scope, Payment payment, object cbBody)
    {
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var acctSvc = scope.ServiceProvider.GetRequiredService<AcctService>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var depositId = await ctx.Deposits
            .Where(x => x.PaymentId == payment.Id)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (depositId == 0)
            return;

        // complete payment
        var (res0, _) =
            await acctSvc.DepositCompletePaymentAsync(depositId, DefaultParty.System, "Complete Payment by Callback");
        if (!res0) return;

        if (payment.PaymentMethod.IsAutoDepositEnabled != 1)
            return;

        var cfgSvc = scope.ServiceProvider.GetRequiredService<ConfigService>();
        var settings = await cfgSvc.GetAsync<PaymentMethod.CallbackSetting>(ConfigCategoryTypes.Public, 0,
            ConfigKeys.PaymentServiceCallbackSetting);

        if (settings != null && payment.CreatedOn < DateTime.UtcNow.AddMinutes(-settings.CallbackExpiredTimeInMinutes))
        {
            await acctSvc.DepositCallbackTimeoutAsync(depositId, DefaultParty.System, "Callback Timeout");
            return;
        }

        var (res1, _) = await acctSvc.DepositCallbackCompleteAsync(depositId, DefaultParty.System,
            "Deposit to Trade Account by Callback");
        if (!res1) return;

        // Load existing callback body to preserve all previous data
        var existingCallbackBody = string.IsNullOrEmpty(payment.CallbackBody)
            ? null
            : JsonConvert.DeserializeObject<Payment.CallbackBodyModel>(payment.CallbackBody);

        // Extract existing data safely
        dynamic? existingBody = existingCallbackBody?.Body;

        // Last time log CallbackBody
        payment.CallbackBody = JsonConvert.SerializeObject(new Payment.CallbackBodyModel
        {
            Status = "completed",
            UpdatedOn = DateTime.UtcNow,
            Body = new
            {
                // Preserve vendor callback from controller
                vendorCallback = existingBody?.vendorCallback,
                // Preserve decrypted data from Hangfire job
                decryptedData = existingBody?.decryptedData,
                // Final callback data for easy reference (same as decryptedData)
                finalCallbackData = cbBody,
                // Complete processing info with timeline
                processingInfo = new
                {
                    receivedAt = existingBody?.processingInfo?.receivedAt,
                    signatureVerified = existingBody?.processingInfo?.signatureVerified,
                    decryptedAt = existingBody?.processingInfo?.decryptedAt,
                    hangfireJobStarted = existingBody?.processingInfo?.hangfireJobStarted,
                    completedAt = DateTime.UtcNow,
                    depositCallbackCompleted = true,
                    note = "Deposit callback successfully completed"
                }
            }
        });

        ctx.Payments.Update(payment);
        await ctx.SaveChangesAsync();

        await mediator.Publish(new DepositCompletedEvent(depositId));


        // deposit = await accountingSvc.DepositGetAsync(deposit.Id);
        // var (isCompleted, _) = await tradingSvc.CompleteDepositToTradeAccount(deposit.Id);
        // if (isCompleted)
        // {
        //     await accountingSvc.DepositCallbackCompleteAsync(deposit, DefaultParty.System);
        //     await mediator.Publish(new DepositCompletedEvent(deposit));
        // }
    }
    
    /// <summary>
    /// Buzipay webhook callback
    /// Route: POST /api/v1/payment/callback/{tenantId}/buzipay
    /// </summary>
    [HttpPost("buzipay")]
    public async Task<IActionResult> BuzipayCallback(long tenantId)
    {
        try
        {
            // Read raw body (required for signature validation)
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            
            logger.LogInformation("Buzipay webhook received for tenant {TenantId}: {Body}", tenantId, rawBody);
            
            // Extract headers
            var timestamp = Request.Headers["Pay-Timestamp"].ToString();
            var signature = Request.Headers["Pay-Signature"].ToString();
            
            if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature))
            {
                logger.LogWarning("Buzipay webhook missing required headers");
                return BadRequest("Missing required headers");
            }
            
            // Parse webhook event
            var webhookEvent = JsonConvert.DeserializeObject<BuzipayWebhookEvent>(rawBody);
            if (webhookEvent?.Data?.Object == null)
            {
                logger.LogWarning("Buzipay webhook invalid format");
                return BadRequest("Invalid webhook format");
            }
            
            // Extract payment number from clientReferenceId
            var paymentNumber = webhookEvent.Data.Object.ClientReferenceId;
            if (string.IsNullOrEmpty(paymentNumber))
            {
                logger.LogWarning("Buzipay webhook missing clientReferenceId");
                return BadRequest("Missing payment reference");
            }
            
            // Resolve tenant scope
            using var scope = await CreateScopeByPaymentNumberAsync(paymentNumber);
            if (scope == null)
            {
                logger.LogWarning("Failed to resolve tenant for payment {PaymentNumber}", paymentNumber);
                return BadRequest("Invalid payment reference");
            }
            
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            
            // Load payment with method
            var payment = await PaymentGetWithServiceByNumberAsync(ctx, paymentNumber, new Dictionary<string, string>());
            if (payment == null)
            {
                logger.LogWarning("Payment not found: {PaymentNumber}", paymentNumber);
                return NotFound("Payment not found");
            }
            
            // Validate signature
            var options = BuzipayOptions.FromJson(payment.PaymentMethod.Configuration);
            if (!Buzipay.ValidateWebhookSignature(timestamp, signature, rawBody, options.WebhookSecretKey, logger))
            {
                logger.LogWarning("Buzipay webhook signature validation failed for payment {PaymentNumber}", paymentNumber);
                return BadRequest("Invalid signature");
            }
            
            // Check if already completed (idempotency)
            if (payment.Status == (short)PaymentStatusTypes.Completed)
            {
                logger.LogInformation("Payment {PaymentNumber} already completed, skipping", paymentNumber);
                return Ok("success");
            }
            
            // Only process payment_intent.succeeded events
            if (webhookEvent.Type != "payment_intent.succeeded")
            {
                logger.LogInformation("Ignoring webhook event type: {Type}", webhookEvent.Type);
                return Ok("success");
            }
            
            // Verify session status via API (optional but recommended)
            var session = await Buzipay.RetrieveSessionAsync(
                webhookEvent.Data.Object.SessionId,
                options.SecretKey,
                options.ApiUrl,
                scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(),
                logger
            );
            
            if (session?.Data?.Status != "complete")
            {
                logger.LogWarning("Session {SessionId} status is not complete: {Status}", 
                    webhookEvent.Data.Object.SessionId, session?.Data?.Status);
                return BadRequest("Session not complete");
            }
            
            // Update payment amount if needed (in case of currency conversion)
            //if (webhookEvent.Data.Object.Amount > 0)
            //{
            //    await TryUpdatePaymentAmount(ctx, payment, webhookEvent.Data.Object.Amount);
            //}
            
            // Complete deposit
            await TryCompleteDeposit(scope, payment, new Dictionary<string, string>
            {
                { "sessionId", webhookEvent.Data.Object.SessionId },
                { "paymentIntentId", webhookEvent.Data.Object.PaymentIntentId },
                { "amount", webhookEvent.Data.Object.Amount.ToString() },
                { "currency", webhookEvent.Data.Object.Currency }
            });
            
            logger.LogInformation("Buzipay payment {PaymentNumber} completed successfully", paymentNumber);
            return Ok("success");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Buzipay webhook processing failed");
            return StatusCode(500, "Internal error");
        }
    }
    
    // Buzipay webhook event models
    public class BuzipayWebhookEvent
    {
        [JsonProperty("eventId")]
        public string EventId { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("created")]
        public long Created { get; set; }
        
        [JsonProperty("data")]
        public BuzipayWebhookData Data { get; set; } = new();
    }
    
    public class BuzipayWebhookData
    {
        [JsonProperty("object")]
        public BuzipayPaymentIntent Object { get; set; } = new();
    }
    
    public class BuzipayPaymentIntent
    {
        [JsonProperty("paymentIntentId")]
        public string PaymentIntentId { get; set; } = string.Empty;
        
        [JsonProperty("sessionId")]
        public string SessionId { get; set; } = string.Empty;
        
        [JsonProperty("clientReferenceId")]
        public string ClientReferenceId { get; set; } = string.Empty;
        
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;
        
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonProperty("customerId")]
        public string CustomerId { get; set; } = string.Empty;
        
        [JsonProperty("paymentMethodId")]
        public string PaymentMethodId { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("created")]
        public long Created { get; set; }
        
        [JsonProperty("confirmAt")]
        public long ConfirmAt { get; set; }
        
        [JsonProperty("amountRefunded")]
        public decimal AmountRefunded { get; set; }
    }
}