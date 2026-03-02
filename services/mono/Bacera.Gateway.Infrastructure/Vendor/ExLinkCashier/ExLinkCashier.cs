using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.ExLinkCashier;

public class ExLinkCashier
{
    public static async Task<object> TestAsync(HttpClient client, string config, ILogger logger)
    {
        var options = ExLinkCashierOptions.FromJson(config);
        var request = new RequestClient
        {
            PaymentNumber = Payment.GenerateNumber(),
            Amount = 500000,
            Options = options,
            Client = client,
            Logger = logger
        };
        return await request.RequestAsync();
    }

    public class RequestClient
    {
        public string PaymentNumber { get; set; } = null!;
        public long Amount { get; set; }
        public ExLinkCashierOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        private Dictionary<string, object> BuildForm()
        {
            // B.银行台待收 (Cashier)
            // Note: callback_url is configured in ExLink admin portal, not in request body
            long uid = long.Parse(Options.Uid);
            return new Dictionary<string, object>
            {
                { "uid", uid },
                { "merchantOrderNo", PaymentNumber },
                { "currencyCoinName", Options.Currency },
                { "amount", Amount }
            };
        }

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var form = BuildForm();
            Logger.LogInformation("ExLinkCashier.RequestAsync - Form before signature:");
            foreach (var kvp in form)
            {
                Logger.LogInformation("  {Key} = {Value} (type: {Type})", kvp.Key, kvp.Value, kvp.Value?.GetType().Name ?? "null");
            }

            var signature = GenerateSignature(form, Options.SecretKey, Logger);
            form.Add("signature", signature);

            var jsonBody = JsonConvert.SerializeObject(form);
            Logger.LogInformation("ExLinkCashier.RequestAsync - Final JSON body: {jsonBody}", jsonBody);
            Logger.LogInformation("ExLinkCashier.RequestAsync - Posting to: {RequestUrl}", Options.RequestUrl);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            
            // Add Language header as shown in documentation examples
            var request = new HttpRequestMessage(HttpMethod.Post, Options.RequestUrl)
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");
            
            Logger.LogInformation("ExLinkCashier.RequestAsync - Request headers: Content-Type=application/json, Language=en_us");
            
            var response = await Client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("ExLinkCashier.RequestAsync ({Currency}) - Response: {json}", Options.Currency, json);

            var obj = Utils.JsonDeserializeObjectWithDefault<dynamic>(json);
            
            // Check if request was successful
            if (obj.success != true || obj.code != 1)
            {
                string errorMessage = obj.message?.ToString() ?? "Unknown error";
                int code = obj.code != null ? (int)obj.code : -1;
                Logger.LogError("ExLinkCashier.RequestAsync - API returned error: code={Code}, message={Message}", 
                    code, errorMessage);
                
                return DepositCreatedResponseModel.Fail(
                    $"ExLink error ({code}): {errorMessage}");
            }
            
            // Extract redirect URL
            string redirectUrl = obj.data?.url?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(redirectUrl))
            {
                Logger.LogError("ExLinkCashier.RequestAsync - No redirect URL in successful response");
                return DepositCreatedResponseModel.Fail("ExLink did not return a payment URL");
            }
            
            return new DepositCreatedResponseModel
            {
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = redirectUrl,
                IsSuccess = true,
                PaymentNumber = PaymentNumber
            };
        }
    }

    /// <summary>
    /// Withdrawal/Payout Request Client for ExLink Cashier
    /// API: https://api.exlinked.global/coin/pay/withdraw/order/create
    /// </summary>
    public class WithdrawalRequestClient
    {
        public string PaymentNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string PaymentType { get; set; } = "BankDirect";
        public string BankName { get; set; } = null!;
        public string BankCode { get; set; } = null!;
        public string BankBranchName { get; set; } = null!;
        public string AccountName { get; set; } = null!;
        public string BankNumber { get; set; } = null!;
        public string Memo { get; set; } = string.Empty;
        public ExLinkCashierOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;

        private Dictionary<string, object> BuildForm()
        {
            // Build form for withdrawal request
            // Note: memo does NOT participate in signature
            long uid = long.Parse(Options.Uid);
            return new Dictionary<string, object>
            {
                { "uid", uid },
                { "merchantOrderNo", PaymentNumber },
                { "currencyCoinName", Currency },
                { "paymentType", PaymentType },
                { "amount", Amount.ToString("F2") },
                { "bankName", BankName },
                { "bankCode", BankCode },
                { "bankBranchName", BankBranchName },
                { "bankUserName", AccountName },
                { "bankAccount", BankNumber }
            };
        }

        public async Task<PayoutResponseModel> RequestAsync()
        {
            var form = BuildForm();
            Logger.LogInformation("ExLinkCashier.WithdrawalRequestClient - Form before signature:");
            foreach (var kvp in form)
            {
                Logger.LogInformation("  {Key} = {Value} (type: {Type})", kvp.Key, kvp.Value, kvp.Value?.GetType().Name ?? "null");
            }

            var signature = GenerateSignature(form, Options.SecretKey, Logger);
            form.Add("signature", signature);
            
            // Add memo AFTER signature (does not participate in signature)
            if (!string.IsNullOrEmpty(Memo))
            {
                form.Add("memo", Memo);
            }

            var jsonBody = JsonConvert.SerializeObject(form);
            Logger.LogInformation("ExLinkCashier.WithdrawalRequestClient - Final JSON body: {jsonBody}", jsonBody);
            
            var withdrawalUrl = string.IsNullOrEmpty(Options.WithdrawalUrl) 
                ? "https://api.exlinked.global/coin/pay/withdraw/order/create" 
                : Options.WithdrawalUrl;
            
            Logger.LogInformation("ExLinkCashier.WithdrawalRequestClient - Posting to: {WithdrawalUrl}", withdrawalUrl);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            
            // Add Language header as shown in documentation examples
            var request = new HttpRequestMessage(HttpMethod.Post, withdrawalUrl)
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");
            
            Logger.LogInformation("ExLinkCashier.WithdrawalRequestClient - Request headers: Content-Type=application/json, Language=en_us");
            
            var response = await Client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("ExLinkCashier.WithdrawalRequestClient ({Currency}) - Response: {json}", Currency, json);

            var obj = Utils.JsonDeserializeObjectWithDefault<dynamic>(json);
            
            // Check if request was successful
            bool isSuccess = obj.success == true && obj.code == 1;
            string message = obj.message?.ToString() ?? "Unknown response";
            int code = obj.code != null ? (int)obj.code : -1;
            
            // Try to extract recordId safely - data might be null, a simple value, or an object
            string recordId = string.Empty;
            try
            {
                if (obj.data != null && obj.data is Newtonsoft.Json.Linq.JObject)
                {
                    recordId = obj.data.recordId?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                // If we can't extract recordId, just leave it empty
            }
            
            if (!isSuccess)
            {
                Logger.LogError("ExLinkCashier.WithdrawalRequestClient - API returned error: code={Code}, message={Message}", 
                    code, message);
            }
            else
            {
                Logger.LogInformation("ExLinkCashier.WithdrawalRequestClient - Withdrawal created successfully. RecordId: {RecordId}", recordId);
            }
            
            return new PayoutResponseModel
            {
                IsSuccess = isSuccess,
                Message = message,
                ResponseJson = json
            };
        }
    }

    public static string GenerateSignature(Dictionary<string, object> spec, string secretKey, ILogger? logger = null)
    {
        // Sort parameters alphabetically and build the signature string
        var sortedParams = spec
            .Where(x => x.Key != "signature")
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .ToList();

        logger?.LogInformation("ExLinkCashier.GenerateSignature - Parameters to sign:");
        foreach (var param in sortedParams)
        {
            logger?.LogInformation("  {Key} = {Value} (type: {Type})", param.Key, param.Value, param.Value?.GetType().Name ?? "null");
        }

        var aggregatedString = sortedParams
            .Select(x => $"{x.Key}={x.Value}")
            .Aggregate((x, y) => $"{x}&{y}");

        logger?.LogInformation("ExLinkCashier.GenerateSignature - String before key: {aggregatedString}", aggregatedString);

        // Use &key= format as shown in Java documentation
        aggregatedString = aggregatedString.Trim() + $"&key={secretKey}";
        logger?.LogInformation("ExLinkCashier.GenerateSignature - String with key: {aggregatedString}", aggregatedString);

        var hash = Utils.Md5Hash(aggregatedString).ToLower();
        logger?.LogInformation("ExLinkCashier.GenerateSignature - Final signature (MD5 lowercase): {hash}", hash);

        return hash;
    }

    /// <summary>
    /// Query exchange rate from ExLink API
    /// API: https://api.exlinked.global/coin/pay/query/market-price-rate
    /// Returns rates for USDT pairs (e.g., VND/USDT, THB/USDT)
    /// </summary>
    public static async Task<ExchangeRateResponse?> QueryExchangeRateAsync(
        string uid, 
        string secretKey, 
        HttpClient client, 
        ILogger? logger = null)
    {
        try
        {
            var form = new Dictionary<string, object>
            {
                { "uid", long.Parse(uid) }
            };

            var signature = GenerateSignature(form, secretKey, logger);
            form.Add("signature", signature);

            var jsonBody = JsonConvert.SerializeObject(form);
            logger?.LogInformation("ExLinkCashier.QueryExchangeRateAsync - Request body: {jsonBody}", jsonBody);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/query/market-price-rate")
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            logger?.LogInformation("ExLinkCashier.QueryExchangeRateAsync - Response: {json}", json);

            var result = JsonConvert.DeserializeObject<ExchangeRateResponse>(json);
            if (result?.Success == true && result.Code == 1)
            {
                return result;
            }

            logger?.LogWarning("ExLinkCashier.QueryExchangeRateAsync - Failed: {json}", json);
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ExLinkCashier.QueryExchangeRateAsync - Exception: {Message}", ex.Message);
            return null;
        }
    }

    public class ExchangeRateResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public ExchangeRateDataWrapper Data { get; set; } = new();
    }

    public class ExchangeRateDataWrapper
    {
        [JsonProperty("marketPriceList")]
        public List<ExchangeRateData> MarketPriceList { get; set; } = new();
    }

    public class ExchangeRateData
    {
        [JsonProperty("marketOutPrice")]
        public decimal MarketOutPrice { get; set; }

        [JsonProperty("marketInPrice")]
        public decimal MarketInPrice { get; set; }

        [JsonProperty("targetCoinId")]
        public long TargetCoinId { get; set; }

        [JsonProperty("targetCoinName")]
        public string TargetCoinName { get; set; } = string.Empty;

        [JsonProperty("sourceCoinName")]
        public string SourceCoinName { get; set; } = string.Empty;

        [JsonProperty("sourceCoinId")]
        public long SourceCoinId { get; set; }
    }

    /// <summary>
    /// Query bank list for payout (withdrawal)
    /// API: https://api.exlinked.global/coin/pay/route/query/queryPayoutBankList
    /// </summary>
    public static async Task<BankListResponse?> QueryPayoutBankListAsync(
        string uid,
        string secretKey,
        string coinName,
        string paymentType,
        HttpClient client,
        ILogger? logger = null)
    {
        try
        {
            var form = new Dictionary<string, object>
            {
                { "uid", long.Parse(uid) }
            };

            var signature = GenerateSignature(form, secretKey, logger);
            form.Add("signature", signature);
            
            // coinName and paymentType are added AFTER signature
            form.Add("coinName", coinName);
            form.Add("paymentType", paymentType);

            var jsonBody = JsonConvert.SerializeObject(form);
            logger?.LogInformation("ExLinkCashier.QueryPayoutBankListAsync - Request body: {jsonBody}", jsonBody);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/route/query/queryPayoutBankList")
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            logger?.LogInformation("ExLinkCashier.QueryPayoutBankListAsync - Response: {json}", json);

            var result = JsonConvert.DeserializeObject<BankListResponse>(json);
            if (result?.Success == true && result.Code == 1)
            {
                return result;
            }

            logger?.LogWarning("ExLinkCashier.QueryPayoutBankListAsync - Failed: {json}", json);
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ExLinkCashier.QueryPayoutBankListAsync - Exception: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Query withdrawal order status
    /// API: https://api.exlinked.global/coin/pay/withdraw/order/status
    /// </summary>
    public static async Task<WithdrawalStatusResponse?> QueryWithdrawalStatusAsync(
        string uid,
        string secretKey,
        string merchantOrderNo,
        HttpClient client,
        ILogger? logger = null)
    {
        try
        {
            var form = new Dictionary<string, object>
            {
                { "uid", long.Parse(uid) },
                { "merchantOrderNo", merchantOrderNo }
            };

            var signature = GenerateSignature(form, secretKey, logger);
            form.Add("signature", signature);

            var jsonBody = JsonConvert.SerializeObject(form);
            logger?.LogInformation("ExLinkCashier.QueryWithdrawalStatusAsync - Request body: {jsonBody}", jsonBody);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/withdraw/order/status")
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            logger?.LogInformation("ExLinkCashier.QueryWithdrawalStatusAsync - Response: {json}", json);

            var result = JsonConvert.DeserializeObject<WithdrawalStatusResponse>(json);
            if (result?.Success == true && result.Code == 1)
            {
                return result;
            }

            logger?.LogWarning("ExLinkCashier.QueryWithdrawalStatusAsync - Failed: {json}", json);
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ExLinkCashier.QueryWithdrawalStatusAsync - Exception: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Query deposit/recharge order status
    /// API: https://api.exlinked.global/coin/pay/recharge/order/status
    /// </summary>
    public static async Task<DepositStatusResponse?> QueryDepositStatusAsync(
        string uid,
        string secretKey,
        string merchantOrderNo,
        HttpClient client,
        ILogger? logger = null)
    {
        try
        {
            var form = new Dictionary<string, object>
            {
                { "uid", long.Parse(uid) },
                { "merchantOrderNo", merchantOrderNo }
            };

            var signature = GenerateSignature(form, secretKey, logger);
            form.Add("signature", signature);

            var jsonBody = JsonConvert.SerializeObject(form);
            logger?.LogInformation("ExLinkCashier.QueryDepositStatusAsync - Request body: {jsonBody}", jsonBody);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/recharge/order/status")
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            logger?.LogInformation("ExLinkCashier.QueryDepositStatusAsync - Response: {json}", json);

            var result = JsonConvert.DeserializeObject<DepositStatusResponse>(json);
            if (result?.Success == true && result.Code == 1)
            {
                return result;
            }

            logger?.LogWarning("ExLinkCashier.QueryDepositStatusAsync - Failed: {json}", json);
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ExLinkCashier.QueryDepositStatusAsync - Exception: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Query account balance
    /// API: https://api.exlinked.global/coin/pay/user/query/account-balances
    /// </summary>
    public static async Task<AccountBalanceResponse?> QueryAccountBalanceAsync(
        string uid,
        string secretKey,
        HttpClient client,
        ILogger? logger = null)
    {
        try
        {
            var form = new Dictionary<string, object>
            {
                { "uid", long.Parse(uid) }
            };

            var signature = GenerateSignature(form, secretKey, logger);
            form.Add("signature", signature);

            var jsonBody = JsonConvert.SerializeObject(form);
            logger?.LogInformation("ExLinkCashier.QueryAccountBalanceAsync - Request body: {jsonBody}", jsonBody);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/user/query/account-balances")
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            logger?.LogInformation("ExLinkCashier.QueryAccountBalanceAsync - Response: {json}", json);

            var result = JsonConvert.DeserializeObject<AccountBalanceResponse>(json);
            if (result?.Success == true && result.Code == 1)
            {
                return result;
            }

            logger?.LogWarning("ExLinkCashier.QueryAccountBalanceAsync - Failed: {json}", json);
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ExLinkCashier.QueryAccountBalanceAsync - Exception: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Query collection payment types (for deposits)
    /// API: https://api.exlinked.global/coin/pay/route/query/queryCollectionPaymentType
    /// </summary>
    public static async Task<PaymentTypeResponse?> QueryCollectionPaymentTypesAsync(
        string uid,
        string secretKey,
        string coinName,
        HttpClient client,
        ILogger? logger = null)
    {
        try
        {
            var form = new Dictionary<string, object>
            {
                { "uid", long.Parse(uid) }
            };

            var signature = GenerateSignature(form, secretKey, logger);
            form.Add("signature", signature);
            
            // coinName is added AFTER signature
            form.Add("coinName", coinName);

            var jsonBody = JsonConvert.SerializeObject(form);
            logger?.LogInformation("ExLinkCashier.QueryCollectionPaymentTypesAsync - Request body: {jsonBody}", jsonBody);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/route/query/queryCollectionPaymentType")
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            logger?.LogInformation("ExLinkCashier.QueryCollectionPaymentTypesAsync - Response: {json}", json);

            var result = JsonConvert.DeserializeObject<PaymentTypeResponse>(json);
            if (result?.Success == true && result.Code == 1)
            {
                return result;
            }

            logger?.LogWarning("ExLinkCashier.QueryCollectionPaymentTypesAsync - Failed: {json}", json);
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ExLinkCashier.QueryCollectionPaymentTypesAsync - Exception: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Query payout payment types (for withdrawals)
    /// API: https://api.exlinked.global/coin/pay/route/query/queryPayoutPaymentType
    /// </summary>
    public static async Task<PaymentTypeResponse?> QueryPayoutPaymentTypesAsync(
        string uid,
        string secretKey,
        string coinName,
        HttpClient client,
        ILogger? logger = null)
    {
        try
        {
            var form = new Dictionary<string, object>
            {
                { "uid", long.Parse(uid) }
            };

            var signature = GenerateSignature(form, secretKey, logger);
            form.Add("signature", signature);
            
            // coinName is added AFTER signature
            form.Add("coinName", coinName);

            var jsonBody = JsonConvert.SerializeObject(form);
            logger?.LogInformation("ExLinkCashier.QueryPayoutPaymentTypesAsync - Request body: {jsonBody}", jsonBody);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/route/query/queryPayoutPaymentType")
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            logger?.LogInformation("ExLinkCashier.QueryPayoutPaymentTypesAsync - Response: {json}", json);

            var result = JsonConvert.DeserializeObject<PaymentTypeResponse>(json);
            if (result?.Success == true && result.Code == 1)
            {
                return result;
            }

            logger?.LogWarning("ExLinkCashier.QueryPayoutPaymentTypesAsync - Failed: {json}", json);
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ExLinkCashier.QueryPayoutPaymentTypesAsync - Exception: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Query supported fiat currencies
    /// API: https://api.exlinked.global/coin/pay/proxy/query/supportFiatCoinList
    /// </summary>
    public static async Task<SupportedCurrenciesResponse?> QuerySupportedCurrenciesAsync(
        string uid,
        string secretKey,
        HttpClient client,
        ILogger? logger = null)
    {
        try
        {
            var form = new Dictionary<string, object>
            {
                { "uid", long.Parse(uid) }
            };

            var signature = GenerateSignature(form, secretKey, logger);
            form.Add("signature", signature);

            var jsonBody = JsonConvert.SerializeObject(form);
            logger?.LogInformation("ExLinkCashier.QuerySupportedCurrenciesAsync - Request body: {jsonBody}", jsonBody);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/proxy/query/supportFiatCoinList")
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            logger?.LogInformation("ExLinkCashier.QuerySupportedCurrenciesAsync - Response: {json}", json);

            var result = JsonConvert.DeserializeObject<SupportedCurrenciesResponse>(json);
            if (result?.Success == true && result.Code == 1)
            {
                return result;
            }

            logger?.LogWarning("ExLinkCashier.QuerySupportedCurrenciesAsync - Failed: {json}", json);
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ExLinkCashier.QuerySupportedCurrenciesAsync - Exception: {Message}", ex.Message);
            return null;
        }
    }

    public class BankListResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public List<BankInfo> Data { get; set; } = new();
    }

    public class BankInfo
    {
        [JsonProperty("bankCode")]
        public string BankCode { get; set; } = string.Empty;

        [JsonProperty("bankName")]
        public string BankName { get; set; } = string.Empty;
    }

    public class WithdrawalStatusResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public WithdrawalStatusData Data { get; set; } = new();
    }

    public class WithdrawalStatusData
    {
        [JsonProperty("uid")]
        public long Uid { get; set; }

        [JsonProperty("merchantOrderNo")]
        public string MerchantOrderNo { get; set; } = string.Empty;

        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currencyCoinName")]
        public string CurrencyCoinName { get; set; } = string.Empty;

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("realAmount")]
        public decimal RealAmount { get; set; }

        [JsonProperty("orderAmount")]
        public decimal OrderAmount { get; set; }

        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; } = string.Empty;

        /// <summary>
        /// Get status description
        /// 1=Pending, 2=Processing, 3=Success, 4=Failed, 5=Reversed, 8=Withdrawal Failed, 10=Failed and Refunded
        /// </summary>
        public string GetStatusDescription()
        {
            return Status switch
            {
                1 => "Pending",
                2 => "Processing",
                3 => "Success",
                4 => "Failed",
                5 => "Reversed",
                8 => "Withdrawal Failed",
                10 => "Failed and Refunded",
                _ => $"Unknown ({Status})"
            };
        }
    }

    public class DepositStatusResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public DepositStatusData Data { get; set; } = new();
    }

    public class DepositStatusData
    {
        [JsonProperty("uid")]
        public long Uid { get; set; }

        [JsonProperty("merchantOrderNo")]
        public string MerchantOrderNo { get; set; } = string.Empty;

        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currencyCoinName")]
        public string CurrencyCoinName { get; set; } = string.Empty;

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("realAmount")]
        public decimal RealAmount { get; set; }

        [JsonProperty("orderAmount")]
        public decimal OrderAmount { get; set; }

        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; } = string.Empty;

        /// <summary>
        /// Get status description for deposit orders
        /// 1=Pending, 2=Processing, 3=Success, 4=Failed, 5=Reversed, 8=Withdrawal Failed, 10=Failed and Refunded
        /// </summary>
        public string GetStatusDescription()
        {
            return Status switch
            {
                1 => "Pending",
                2 => "Processing",
                3 => "Success",
                4 => "Failed",
                5 => "Reversed",
                8 => "Withdrawal Failed",
                10 => "Failed and Refunded",
                _ => $"Unknown ({Status})"
            };
        }
    }

    public class AccountBalanceResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public List<AccountBalanceData> Data { get; set; } = new();
    }

    public class AccountBalanceData
    {
        [JsonProperty("type")]
        public int Type { get; set; } // 1=币币, 2=法币

        [JsonProperty("coinName")]
        public string CoinName { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("frozenAmount")]
        public decimal FrozenAmount { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; } = string.Empty;
    }

    public class PaymentTypeResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public List<PaymentTypeData> Data { get; set; } = new();
    }

    public class PaymentTypeData
    {
        [JsonProperty("coinName")]
        public string CoinName { get; set; } = string.Empty;

        [JsonProperty("coinId")]
        public long CoinId { get; set; }

        [JsonProperty("paymentType")]
        public string PaymentType { get; set; } = string.Empty;
    }

    public class SupportedCurrenciesResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public List<CurrencyData> Data { get; set; } = new();
    }

    public class CurrencyData
    {
        [JsonProperty("currencyCoinName")]
        public string CurrencyCoinName { get; set; } = string.Empty;

        [JsonProperty("currencyCoinId")]
        public long CurrencyCoinId { get; set; }

        [JsonProperty("currencyNameEn")]
        public string CurrencyNameEn { get; set; } = string.Empty;

        [JsonProperty("currencyName")]
        public string CurrencyName { get; set; } = string.Empty;

        [JsonProperty("countryNameEn")]
        public string CountryNameEn { get; set; } = string.Empty;

        [JsonProperty("countryName")]
        public string CountryName { get; set; } = string.Empty;

        [JsonProperty("currencySufftCode")]
        public string CurrencySufftCode { get; set; } = string.Empty;
    }
}
