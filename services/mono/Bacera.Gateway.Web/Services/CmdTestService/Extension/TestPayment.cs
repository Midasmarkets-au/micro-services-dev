using Amazon.Runtime;
using Bacera.Gateway;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.Bakong;
using Bacera.Gateway.Vendor.BigPay;
using Bacera.Gateway.Vendor.BipiPay;
using Bacera.Gateway.Vendor.Buy365;
using Bacera.Gateway.Vendor.ChinaPay;
using Bacera.Gateway.Vendor.EuPayment;
using Bacera.Gateway.Vendor.ExLink.Models;
using Bacera.Gateway.Vendor.ExLinkCashier;
using Bacera.Gateway.Vendor.FivePayVA;
using Bacera.Gateway.Vendor.Help2Pay;
using Bacera.Gateway.Vendor.Long77Pay;
using Bacera.Gateway.Vendor.Monetix;
using Bacera.Gateway.Vendor.OFAPay;
using Bacera.Gateway.Vendor.Pay247;
using Bacera.Gateway.Vendor.TwelveGroup;
using Bacera.Gateway.Vendor.Rivo;
using Bacera.Gateway.Vendor.PaymentAsia;
using Bacera.Gateway.Vendor.PayPal;
using Bacera.Gateway.Vendor.SeaBipiPay;
using Bacera.Gateway.Services.Twilio;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Twilio.Exceptions;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    private async Task TestExlinkVND()
    {
        const string config = """
                              {
                                "Uid": "5588719",
                                "RequestUrl": "https://api.exlinked.global/coin/pay/recharge/order/create",
                                "Currency": "VND",
                                //"ChannelCode": "ScanQRCode",
                                //"BankCode": "AllBanksSupported",
                                "SecretKey": "WLSA6GAaaTLr4NkLZtm0oaP887jwiIAv",
                                //"CallbackSecretKey": "gdVvlCdcMVVIDmleTbxV1U1kyrt5zbr6",
                                "PaymentMethod": 1,
                              }
                              """;
        var client = _httpClientFactory.CreateClient();
        await ExLinkCashier.TestAsync(client, config, _logger);
    }
    
    private async Task TestExlink()
    {
        const string config = """
                              {
                                "uid": 1895970,
                                "payType": 2,
                                "secretKey": "L3FGadjTGnoFUz1VMGdS5oq4P7RWQPXk",
                                "requestUrl": "https://api.exlinked.com/coin/pay/order/pay/checkout/counter",
                                "legacyRequestUrl": "https://api.exlinked.com/coin/pay/order/coin/checkout/counter",
                                "callbackSecretKey": "4Cqv6OlcdinrNiWHJzAwfsLeMLIPj4aG"
                              }
                              """;
        var client = _httpClientFactory.CreateClient();
        await ExLink.TestAsync(client, config, _logger);
    }
    
    private async Task TestExLinkExchangeRate()
    {
        Console.WriteLine("🧪 Testing ExLink Exchange Rate API...");
        Console.WriteLine();
        
        // Test with tenant 10000 (or your test tenant)
        const long testTenantId = 10000;
        
        try
        {
            using var scope = _serviceProvider.CreateTenantScope(testTenantId);
            
            // Verify tenant exists and connection string is available
            var pool = scope.ServiceProvider.GetRequiredService<MyDbContextPool>();
            var connectionString = pool.GetConnectionStringByTenantId(testTenantId);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"❌ ERROR: Tenant {testTenantId} not found in connection string cache.");
                Console.WriteLine($"Available tenants: {string.Join(", ", pool.GetTenantIds())}");
                Console.WriteLine();
                Console.WriteLine("Please use one of the available tenant IDs or verify tenant is configured in core.\"_Tenant\" table.");
                return;
            }
            
            // Explicitly initialize TenantDbContext to ensure connection string is set
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            var paymentMethodSvc = scope.ServiceProvider.GetRequiredService<PaymentMethodService>();
            
            Console.WriteLine($"✅ Initialized tenant scope for tenant: {testTenantId}");
            Console.WriteLine($"✅ Connection string: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
            Console.WriteLine();
            
            // Test currency pairs
            var testPairs = new[]
            {
                (CurrencyTypes.USD, CurrencyTypes.VND, "USD → VND"),
                (CurrencyTypes.USC, CurrencyTypes.VND, "USC → VND"),
                (CurrencyTypes.USD, CurrencyTypes.THB, "USD → THB"),
                (CurrencyTypes.VND, CurrencyTypes.THB, "VND → THB"),
                (CurrencyTypes.USD, CurrencyTypes.PHP, "USD → PHP"),
                (CurrencyTypes.USD, CurrencyTypes.IDR, "USD → IDR"),
                (CurrencyTypes.USD, CurrencyTypes.INR, "USD → INR"),
                (CurrencyTypes.USD, CurrencyTypes.MXN, "USD → MXN"),
                (CurrencyTypes.USD, CurrencyTypes.KRW, "USD → KRW"),
                (CurrencyTypes.USD, CurrencyTypes.JPY, "USD → JPY"),
                (CurrencyTypes.USD, CurrencyTypes.BRL, "USD → BRL")
            };
            
            Console.WriteLine("Testing multiple currency pairs:");
            Console.WriteLine("=====================================");
            
            foreach (var (from, to, description) in testPairs)
            {
                Console.WriteLine($"\n📊 Testing: {description}");
                Console.WriteLine($"   From: {from} ({(int)from})");
                Console.WriteLine($"   To: {to} ({(int)to})");
                
                try
                {
                    var rate = await paymentMethodSvc.GetExLinkCashierExchangeRateAsync(from, to);
                    
                    if (rate > 0)
                    {
                        Console.WriteLine($"   ✅ SUCCESS: Rate = {rate:F6}");
                        Console.WriteLine($"   Example: 1 {from} = {rate:N2} {to}");
                        Console.WriteLine($"   Example: 100 {from} = {(rate * 100):N2} {to}");
                    }
                    else
                    {
                        Console.WriteLine($"   ❌ FAILED: Rate = {rate} (API failed or not found)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ EXCEPTION: {ex.Message}");
                    _logger.LogError(ex, "TestExLinkExchangeRate failed for {From} -> {To}", from, to);
                }
            }
            
            Console.WriteLine("\n=====================================");
            Console.WriteLine("✅ Exchange Rate Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            _logger.LogError(ex, "TestExLinkExchangeRate failed during initialization");
        }
    }
    
    private async Task TestExLinkSupportedCurrencies()
    {
        Console.WriteLine("🧪 Testing ExLink Supported Currencies API...");
        Console.WriteLine();
        
        const long testTenantId = 10000;
        
        try
        {
            using var scope = _serviceProvider.CreateTenantScope(testTenantId);
            
            var pool = scope.ServiceProvider.GetRequiredService<MyDbContextPool>();
            var connectionString = pool.GetConnectionStringByTenantId(testTenantId);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"❌ ERROR: Tenant {testTenantId} not found.");
                Console.WriteLine($"Available tenants: {string.Join(", ", pool.GetTenantIds())}");
                return;
            }
            
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            
            // Get ExLink configuration
            var exLinkMethod = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            
            if (exLinkMethod == null)
            {
                Console.WriteLine("❌ No active ExLink payment method found");
                return;
            }
            
            var options = ExLinkCashierOptions.FromJson(exLinkMethod.Configuration);
            if (string.IsNullOrEmpty(options.Uid) || string.IsNullOrEmpty(options.SecretKey))
            {
                Console.WriteLine("❌ Invalid ExLink configuration");
                return;
            }
            
            Console.WriteLine($"✅ Using ExLink UID: {options.Uid}");
            Console.WriteLine();
            
            // Call supported currencies API
            var form = new Dictionary<string, object>
            {
                { "uid", long.Parse(options.Uid) }
            };
            
            var signature = ExLinkCashier.GenerateSignature(form, options.SecretKey, _logger);
            form.Add("signature", signature);
            
            var jsonBody = JsonConvert.SerializeObject(form);
            Console.WriteLine($"📤 Request: {jsonBody}");
            Console.WriteLine();
            
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/proxy/query/supportFiatCoinList")
            {
                Content = content
            };
            request.Headers.Add("Language", "en_us");
            
            var response = await client.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"📥 Response:");
            Console.WriteLine(responseJson);
            Console.WriteLine();
            
            // Parse and display results
            var result = JsonConvert.DeserializeObject<dynamic>(responseJson);
            if (result?.success == true && result.code == 1)
            {
                Console.WriteLine("✅ Supported Currencies:");
                Console.WriteLine("=====================================");
                
                foreach (var currency in result.data)
                {
                    Console.WriteLine($"  • {currency.currencyCoinName} ({currency.currencyCoinId})");
                    Console.WriteLine($"    - Name: {currency.currencyNameEn} ({currency.currencyName})");
                    Console.WriteLine($"    - Country: {currency.countryNameEn} ({currency.countryName})");
                    Console.WriteLine($"    - Suffix Code: {currency.currencySufftCode}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine($"❌ API returned error: {result?.message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            _logger.LogError(ex, "TestExLinkSupportedCurrencies failed");
        }
    }
    
    private async Task TestExLinkPaymentTypes()
    {
        Console.WriteLine("🧪 Testing ExLink Payment Types API...");
        Console.WriteLine();
        
        const long testTenantId = 10000;
        
        try
        {
            using var scope = _serviceProvider.CreateTenantScope(testTenantId);
            
            var pool = scope.ServiceProvider.GetRequiredService<MyDbContextPool>();
            var connectionString = pool.GetConnectionStringByTenantId(testTenantId);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"❌ ERROR: Tenant {testTenantId} not found.");
                Console.WriteLine($"Available tenants: {string.Join(", ", pool.GetTenantIds())}");
                return;
            }
            
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            
            // Get ExLink configuration
            var exLinkMethod = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            
            if (exLinkMethod == null)
            {
                Console.WriteLine("❌ No active ExLink payment method found");
                return;
            }
            
            var options = ExLinkCashierOptions.FromJson(exLinkMethod.Configuration);
            if (string.IsNullOrEmpty(options.Uid) || string.IsNullOrEmpty(options.SecretKey))
            {
                Console.WriteLine("❌ Invalid ExLink configuration");
                return;
            }
            
            Console.WriteLine($"✅ Using ExLink UID: {options.Uid}");
            Console.WriteLine();
            
            // Test multiple currencies
            var currencies = new[] { "VND", "THB", "PHP", "IDR", "INR", "MXN", "KRW", "JPY", "BRL" };
            
            Console.WriteLine("Testing Payment Types for Multiple Currencies:");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            
            foreach (var coinName in currencies)
            {
                Console.WriteLine($"💰 Currency: {coinName}");
                Console.WriteLine("-----------------------------------");
                
                try
                {
                    // Build request (coinName is NOT included in signature)
                    var form = new Dictionary<string, object>
                    {
                        { "uid", long.Parse(options.Uid) }
                    };
                    
                    var signature = ExLinkCashier.GenerateSignature(form, options.SecretKey, _logger);
                    form.Add("signature", signature);
                    form.Add("coinName", coinName); // Add AFTER signature
                    
                    var jsonBody = JsonConvert.SerializeObject(form);
                    
                    var client = _httpClientFactory.CreateClient();
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/route/query/queryCollectionPaymentType")
                    {
                        Content = content
                    };
                    request.Headers.Add("Language", "en_us");
                    
                    var response = await client.SendAsync(request);
                    var responseJson = await response.Content.ReadAsStringAsync();
                    
                    var result = JsonConvert.DeserializeObject<dynamic>(responseJson);
                    
                    if (result?.success == true && result.code == 1)
                    {
                        if (result.data != null && result.data.Count > 0)
                        {
                            Console.WriteLine($"  ✅ Available Payment Types:");
                            foreach (var paymentType in result.data)
                            {
                                Console.WriteLine($"     - {paymentType.paymentType} (CoinID: {paymentType.coinId})");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"  ⚠️  No payment types configured for {coinName}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  ❌ Error: {result?.message} (code: {result?.code})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ Exception: {ex.Message}");
                }
                
                Console.WriteLine();
            }
            
            Console.WriteLine("=====================================");
            Console.WriteLine("✅ Payment Types Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestExLinkPaymentTypes failed");
        }
    }
    
    private async Task TestExLinkPayoutPaymentTypes()
    {
        Console.WriteLine("🧪 Testing ExLink Payout Payment Types API...");
        Console.WriteLine();
        
        const long testTenantId = 10000;
        
        try
        {
            using var scope = _serviceProvider.CreateTenantScope(testTenantId);
            
            var pool = scope.ServiceProvider.GetRequiredService<MyDbContextPool>();
            var connectionString = pool.GetConnectionStringByTenantId(testTenantId);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"❌ ERROR: Tenant {testTenantId} not found.");
                Console.WriteLine($"Available tenants: {string.Join(", ", pool.GetTenantIds())}");
                return;
            }
            
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            
            // Get ExLink configuration
            var exLinkMethod = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            
            if (exLinkMethod == null)
            {
                Console.WriteLine("❌ No active ExLink payment method found");
                return;
            }
            
            var options = ExLinkCashierOptions.FromJson(exLinkMethod.Configuration);
            if (string.IsNullOrEmpty(options.Uid) || string.IsNullOrEmpty(options.SecretKey))
            {
                Console.WriteLine("❌ Invalid ExLink configuration");
                return;
            }
            
            Console.WriteLine($"✅ Using ExLink UID: {options.Uid}");
            Console.WriteLine();
            
            // Test multiple currencies
            var currencies = new[] { "VND", "THB", "PHP", "IDR", "INR", "MXN", "KRW", "JPY", "BRL" };
            
            Console.WriteLine("Testing Payout Payment Types for Multiple Currencies:");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            
            foreach (var coinName in currencies)
            {
                Console.WriteLine($"💰 Currency: {coinName}");
                Console.WriteLine("-----------------------------------");
                
                try
                {
                    // Build request (coinName is NOT included in signature)
                    var form = new Dictionary<string, object>
                    {
                        { "uid", long.Parse(options.Uid) }
                    };
                    
                    var signature = ExLinkCashier.GenerateSignature(form, options.SecretKey, _logger);
                    form.Add("signature", signature);
                    form.Add("coinName", coinName); // Add AFTER signature
                    
                    var jsonBody = JsonConvert.SerializeObject(form);
                    
                    var client = _httpClientFactory.CreateClient();
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.exlinked.global/coin/pay/route/query/queryPayoutPaymentType")
                    {
                        Content = content
                    };
                    request.Headers.Add("Language", "en_us");
                    
                    var response = await client.SendAsync(request);
                    var responseJson = await response.Content.ReadAsStringAsync();
                    
                    var result = JsonConvert.DeserializeObject<dynamic>(responseJson);
                    
                    if (result?.success == true && result.code == 1)
                    {
                        if (result.data != null && result.data.Count > 0)
                        {
                            Console.WriteLine($"  ✅ Available Payout Payment Types:");
                            foreach (var paymentType in result.data)
                            {
                                Console.WriteLine($"     - {paymentType.paymentType} (CoinID: {paymentType.coinId})");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"  ⚠️  No payout payment types configured for {coinName}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  ❌ Error: {result?.message} (code: {result?.code})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ Exception: {ex.Message}");
                }
                
                Console.WriteLine();
            }
            
            Console.WriteLine("=====================================");
            Console.WriteLine("✅ Payout Payment Types Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestExLinkPayoutPaymentTypes failed");
        }
    }
    
    private async Task TestExLinkWithdrawal()
    {
        Console.WriteLine("🧪 Testing ExLink Withdrawal/Payout API...");
        Console.WriteLine();
        
        const long testTenantId = 10000;
        
        try
        {
            using var scope = _serviceProvider.CreateTenantScope(testTenantId);
            
            var pool = scope.ServiceProvider.GetRequiredService<MyDbContextPool>();
            var connectionString = pool.GetConnectionStringByTenantId(testTenantId);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"❌ ERROR: Tenant {testTenantId} not found.");
                Console.WriteLine($"Available tenants: {string.Join(", ", pool.GetTenantIds())}");
                return;
            }
            
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            
            // Get ExLink configuration
            var exLinkMethod = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            
            if (exLinkMethod == null)
            {
                Console.WriteLine("❌ No active ExLink payment method found");
                return;
            }
            
            var options = ExLinkCashierOptions.FromJson(exLinkMethod.Configuration);
            if (string.IsNullOrEmpty(options.Uid) || string.IsNullOrEmpty(options.SecretKey))
            {
                Console.WriteLine("❌ Invalid ExLink configuration");
                return;
            }
            
            Console.WriteLine($"✅ Using ExLink UID: {options.Uid}");
            Console.WriteLine();
            
            // Use test data from ExLink documentation
            Console.WriteLine("Testing Withdrawal with ExLink Test Data:");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            
            var client = new ExLinkCashier.WithdrawalRequestClient
            {
                PaymentNumber = Payment.GenerateNumber(),
                Amount = 500015m, // Test amount from documentation
                Currency = "INR",
                PaymentType = "BankDirect",
                BankName = "NGAN HANG TMCP CONG THUONG VIET NAM (VIETINBANK)",
                BankCode = "VIETINBANK",
                BankBranchName = "NGAN HANG TMCP A CHAU (ACB)",
                AccountName = "else",
                BankNumber = "6100000000010",
                Memo = "Test withdrawal",
                Options = options,
                Client = _httpClientFactory.CreateClient(),
                Logger = _logger
            };
            
            Console.WriteLine($"📤 Request Details:");
            Console.WriteLine($"   Payment Number: {client.PaymentNumber}");
            Console.WriteLine($"   Amount: {client.Amount} {client.Currency}");
            Console.WriteLine($"   Bank: {client.BankName} ({client.BankCode})");
            Console.WriteLine($"   Branch: {client.BankBranchName}");
            Console.WriteLine($"   Account: {client.BankNumber}");
            Console.WriteLine($"   Account Name: {client.AccountName}");
            Console.WriteLine($"   Payment Type: {client.PaymentType}");
            Console.WriteLine();
            
            var response = await client.RequestAsync();
            
            Console.WriteLine($"📥 Response:");
            Console.WriteLine($"   Success: {response.IsSuccess}");
            Console.WriteLine($"   Message: {response.Message}");
            Console.WriteLine($"   Full Response: {response.ResponseJson}");
            Console.WriteLine();
            
            if (response.IsSuccess)
            {
                Console.WriteLine("✅ Withdrawal request created successfully!");
            }
            else
            {
                Console.WriteLine($"❌ Withdrawal request failed: {response.Message}");
            }
            
            Console.WriteLine("=====================================");
            Console.WriteLine("✅ Withdrawal Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            _logger.LogError(ex, "TestExLinkWithdrawal failed");
        }
    }

    private async Task TestExLinkPayoutBankList()
    {
        try
        {
            Console.WriteLine();
            Console.WriteLine("Testing ExLink Payout Bank List:");
            Console.WriteLine("=====================================");
            Console.WriteLine();

            const long testTenantId = 10000;

            using var scope = _serviceProvider.CreateTenantScope(testTenantId);
            
            var pool = scope.ServiceProvider.GetRequiredService<MyDbContextPool>();
            var connectionString = pool.GetConnectionStringByTenantId(testTenantId);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"❌ ERROR: Tenant {testTenantId} not found.");
                Console.WriteLine($"Available tenants: {string.Join(", ", pool.GetTenantIds())}");
                return;
            }
            
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            
            // Get ExLink configuration
            var exLinkMethod = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            
            if (exLinkMethod == null)
            {
                Console.WriteLine("❌ ERROR: ExLink payment method not found!");
                return;
            }

            var options = ExLinkCashierOptions.FromJson(exLinkMethod.Configuration);
            Console.WriteLine($"✅ Found ExLink config (UID: {options.Uid})");
            Console.WriteLine();

            // Test multiple currencies
            var currencies = new[] { "VND", "THB", "PHP", "IDR", "INR", "MXN", "KRW", "JPY", "BRL" };
            var paymentType = options.PaymentType; // Use configured payment type

            foreach (var coinName in currencies)
            {
                Console.WriteLine($"📋 Querying bank list for {coinName} (PaymentType: {paymentType}):");
                Console.WriteLine("-------------------------------------");

                var result = await ExLinkCashier.QueryPayoutBankListAsync(
                    options.Uid,
                    options.SecretKey,
                    coinName,
                    paymentType,
                    _httpClientFactory.CreateClient(),
                    _logger
                );

                if (result != null && result.Data.Count > 0)
                {
                    Console.WriteLine($"✅ Found {result.Data.Count} banks:");
                    foreach (var bank in result.Data.Take(5)) // Show first 5
                    {
                        Console.WriteLine($"   • {bank.BankCode}: {bank.BankName}");
                    }
                    if (result.Data.Count > 5)
                    {
                        Console.WriteLine($"   ... and {result.Data.Count - 5} more");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️  No banks found or API returned error");
                }
                Console.WriteLine();
            }

            Console.WriteLine("=====================================");
            Console.WriteLine("✅ Bank List Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            _logger.LogError(ex, "TestExLinkPayoutBankList failed");
        }
    }

    private async Task TestExLinkWithdrawalStatus()
    {
        try
        {
            Console.WriteLine();
            Console.WriteLine("Testing ExLink Withdrawal Status Query:");
            Console.WriteLine("=====================================");
            Console.WriteLine();

            const long testTenantId = 10000;

            using var scope = _serviceProvider.CreateTenantScope(testTenantId);
            
            var pool = scope.ServiceProvider.GetRequiredService<MyDbContextPool>();
            var connectionString = pool.GetConnectionStringByTenantId(testTenantId);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"❌ ERROR: Tenant {testTenantId} not found.");
                Console.WriteLine($"Available tenants: {string.Join(", ", pool.GetTenantIds())}");
                return;
            }
            
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            
            // Get ExLink configuration
            var exLinkMethod = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            
            if (exLinkMethod == null)
            {
                Console.WriteLine("❌ ERROR: ExLink payment method not found!");
                return;
            }

            var options = ExLinkCashierOptions.FromJson(exLinkMethod.Configuration);
            Console.WriteLine($"✅ Found ExLink config (UID: {options.Uid})");
            Console.WriteLine();

            // Prompt for merchant order number
            Console.Write("Enter merchantOrderNo to query (or press Enter to use test value): ");
            var merchantOrderNo = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(merchantOrderNo))
            {
                merchantOrderNo = "pm-267f507ee9b"; // Default test value from previous withdrawal test
            }

            Console.WriteLine($"🔍 Querying status for order: {merchantOrderNo}");
            Console.WriteLine();

            var result = await ExLinkCashier.QueryWithdrawalStatusAsync(
                options.Uid,
                options.SecretKey,
                merchantOrderNo,
                _httpClientFactory.CreateClient(),
                _logger
            );

            if (result != null)
            {
                Console.WriteLine($"📥 Order Status:");
                Console.WriteLine($"   UID: {result.Data.Uid}");
                Console.WriteLine($"   Merchant Order No: {result.Data.MerchantOrderNo}");
                Console.WriteLine($"   Record ID: {result.Data.RecordId}");
                Console.WriteLine($"   Currency: {result.Data.CurrencyCoinName}");
                Console.WriteLine($"   Status: {result.Data.Status} ({result.Data.GetStatusDescription()})");
                Console.WriteLine($"   Order Amount: {result.Data.OrderAmount:F2}");
                Console.WriteLine($"   Real Amount: {result.Data.RealAmount:F2}");
                Console.WriteLine($"   Fee: {result.Data.Fee:F2}");
                Console.WriteLine($"   Signature: {result.Data.Signature}");
                Console.WriteLine();

                // Status explanation
                Console.WriteLine("📖 Status Codes:");
                Console.WriteLine("   1 = Pending");
                Console.WriteLine("   2 = Processing");
                Console.WriteLine("   3 = Success");
                Console.WriteLine("   4 = Failed");
                Console.WriteLine("   5 = Reversed");
                Console.WriteLine("   8 = Withdrawal Failed");
                Console.WriteLine("   10 = Failed and Refunded");
            }
            else
            {
                Console.WriteLine($"❌ Failed to query order status - API returned error or order not found");
            }

            Console.WriteLine();
            Console.WriteLine("=====================================");
            Console.WriteLine("✅ Status Query Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            _logger.LogError(ex, "TestExLinkWithdrawalStatus failed");
        }
    }
    
    private async Task TestChinaPay()
    {
        // 1：银行卡 Bank card 2：支付宝（需开通） Alipay (need open) 3：微信（需开通） WeChat (need open)
        const string config = """
                              {
                                "ApiDomain": "https://api.fx.dvbfservice.com/trade",
                                "AppId": "1752657690322",
                                "ServerPublicKey": "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCCU6BZwUHSf7GyUaxnGE9CPTrncFBuN8J4zroVOk/raS2KPPV9W3f4/uq9lEtUtvxKQFubDQyALY4CvwnMVyZrnvh+6YwodUP0LuYbwYa70wRPvr4G/Ee9MwnHQPJpGPS9CEsGGnsdX+iYt+8fJ6DWRfiHv3cjW8kPM8W+R+n7swIDAQAB",
                                "AppPrivateKey": "MIICdQIBADANBgkqhkiG9w0BAQEFAASCAl8wggJbAgEAAoGBALsBJTpj+jQNevOMDCWgxsxP2DlszW7Oxq+LOC+XuAs0n5WGcY+go7zdIQVCKQu3qcQ97GWQZX4+KjVho+EKPysmOCzDDl713So9JS/TI4jP6y0pwVmKS8ojMO5r0CUPH8Bpb5BN8D8YIYyTUIZw5QbUpzhdIEQMciAVhwOrLFsRAgMBAAECgYA8allRrP0nlfdT+bnu6itv1JfhrZINK4arLgo/7f9Kt3ybPS0xHs/nc3V4knBaVcLmIK9f/K+dgo32Yw4j5UCUrMf8mF+yVV2pGaopDzz3mQ7rSESXMaw4bWlnVhEz9Hia23cC99q56LqOqNwPFGlnB/AWEWPNZVuKmNy9J3qrRQJBAPaUj8l34Gh513z4C4mg9fVtgjDLnZhXi3gpEfT3qlWdthGsRgdf8tYUFcT8Pa8LfgqTROfnKlj0p8mfZjYPN1sCQQDCJfXWp2Bx/Xnoudi24j0Unhdpyx7PGmoaWngUhCIaPYFgaM8VWG3HzunPLdLp9h+3MMmkVKVfeK62bu+Wjy8DAkB1L4FuNAR4Yn9gqtmA3PhHXXXkDsCk5+Ymgw4/p3xSGBOxLnhRhO35g64c4evGxuVtzTOQKrJbeFpNXe3Lf0vFAkAXffRjRtl6sYsgx+wbJQDzD7YRjQxrTuzrx2qkQODHgA25lrUIBi0yci0Ebq0ItklcJ0Ee60cfaEDrvjyrYSP1AkBqtT/W/FLhpefEJ3TdNfUoK1cCzMHoI21TB+RJqU1Rrs35Be48T0RUVJQHc/e7QivfSY7mztomcf+sNFM2GgAx",
                                "Version": "1.0.0",
                                "Charset": "utf-8",
                                "PayType": 3,
                                "Symbol": 0,
                                "InterfaceName": "payq.trade.wapurl",
                              }
                              """;
        var client = _httpClientFactory.CreateClient();
        await ChinaPay.TestAsync(client, config, _logger);
    }
    
    private async Task TestPay247()
    {
        await TestPay247CreateOrder();
    }

    private async Task TestPay247Out()
    {
        await TestPay247CreatePayout();
    }
    
    private async Task TestNPay()
    {
        
        const string config = """
                              {
                                "endPoint": "https://swpapi.jy6989.com/UtInRecordApi/orderGateWay",
                                "serverIp": "13.213.62.199",
                                "merchantId": "601401",
                                "callbackSecret": "FCAFC952-D2CC-E9F4-E2B8-3E19F94D2632",
                                "merchantSecret": "10fcac0ce98fc54bcdc1fcf85fe7eaf9"
                              }
                              """;
        await NPay.TestAsync(config, _logger);
    }

    private async Task TestFivePayVA()
    {
        // public string AccountId { get; set; } = string.Empty;
        // public string Password { get; set; } = string.Empty;
        // public string PassPhrase { get; set; } = string.Empty;
        // public string Endpoint { get; set; } = string.Empty;

        // sec1_thebcr_vip
        const string config = """
                              {
                                "endPoint": "https://cn-payment.my5pay.com/f2fOrder/createorder",
                                "secretKey": "wtA4PHr7bF37Ozp5neKLJpal",
                                "merchantId": 53620,
                                "currencyCode": "VND",
                                "callbackDomain": "https://api.bvi.thebcr.com/api/v1"
                              }
                              """;
        await FivePayVA.TestAsync(config, _logger);
    }
    
    private async Task TestPayPal()
    {
        const string config = """
                              {
                                "ClientId": "Ab8ERX3wNfPaz1VZj7MR4_ROE6GlHB5QOVu3LpRtRFooFlnQ5bd6CnRYMpoyCjudb3MPMiMKGjgko3EK",
                                "ClientSecret": "EJgikWgqB0HbMez1CA-p47S6iZw_CXbkwkASOTrz5OCxEuuQgT1TCTFN6EoNVkQymcRCh0ZCt7h1TGaF",
                                "TokenEndPoint": "https://api-m.sandbox.paypal.com/v1/oauth2/token",
                                "CheckOutEndPoint": "https://api-m.sandbox.paypal.com/v2/checkout/orders",
                              }
                              """;

        const string config1 = """
                               {
                                 "Note": "bcrprosperity For BVI use only",
                                 "ClientId": "AUnsp8S-_vWvKfVPG8qMzRNpZc4IxleWXx04LRqUFk9qPUBbuFtKI6-KfKMCUXVEU3k7id6y_CN5LxUC",
                                 "BrandName": "Bacera",
                                 "ClientSecret": "EMHjlmgQvK0MGvurF2yd7rQYUcWizBQsIiDcXEraqxwf_7htf118_k_1bN6RjTYfnvnGjUEGEhabVhMf",
                                 "TokenEndPoint": "https://api-m.paypal.com/v1/oauth2/token",
                                 "CheckOutEndPoint": "https://api-m.paypal.com/v2/checkout/orders"
                               }
                               """;

        const string config2 = """
                               {
                                 "Note": "bcrholdings For BVI use only",
                                 "ClientId": "AWjSC1LpzaSUrpAyaoykWWNErjjXhguDOFTgGQkZDszS2hxh_b6p1zrB0HX0-J78Yn_h_STjE3FiN_bP",
                                 "BrandName": "Bacera",
                                 "ClientSecret": "EKLG-SW1H4jwz96kKQpkMBfLAHwUeya5imWBQl9nPJ2or4nx5X4gFd62S8ID2_ixGXRZeyd5bdnhme5u",
                                 "TokenEndPoint": "https://api-m.paypal.com/v1/oauth2/token",
                                 "CheckOutEndPoint": "https://api-m.paypal.com/v2/checkout/orders"
                               }
                               """;
        var client = _httpClientFactory.CreateClient();
        await PayPal.TestAsync(client, config2, _logger);
    }
    
    private async Task TestCrypto()
    {
        // using var scope = CreateTenantScopeByTenantIdAsync(10000);
        // var depositSvc = scope.ServiceProvider.GetRequiredService<DepositService>();
        // var res = await depositSvc.CreateDepositForAccountAsync(10055, 57130, 2000, new Dictionary<string, object>());
        // _logger.LogInformation("TestCrypto: {res}", res);
    }

    private async Task EnableAccountAccessByGroup()
    {
        using var scope = _serviceProvider.CreateTenantScope(10000);
        var svc = scope.ServiceProvider.GetRequiredService<PaymentMethodService>();
        await svc.EnableAccountAccessByGroupAsync(55322, "AliPay");
    }
    
    private async Task TestBakong()
    {
        const string config = """
                              {
                                "EndPointBase": "https://payment.pa-sys.com",
                                "MerchantSecret": "433a37a6-e227-49b4-8063-951393b0c48c",
                                "MerchantToken": "c8eca877-2762-4a4d-8f26-70fd9c951670",
                                "Currency": "USD",
                                "Network": "DirectDebit"
                              }
                              """;
        await Bakong.TestAsync(config, _logger);
    }

    private async Task TestEuPayment()
    {
        const string config = """
                              {
                                "AccountId": "sec1_thebcr_ftd",
                                "Password": "Vi9UFt99ush",
                                "PassPhrase": "47b3D0a5CDee1eDacBbbebd32F3067DAC",
                                "Endpoint": "https://ts.secure1gateway.com/api/v2/ProcessTx"
                              }
                              """;
        const string ramperConfig = """
                                    {
                                      "AccountId": "sec1_thebcr_ramper",
                                      "Password": "Vi9UFt99ush",
                                      "PassPhrase": "47b3D0a5CDee1eDacBbbebd32F3067DAC",
                                      "Endpoint": "https://ts.secure1gateway.com/api/v2/ProcessTx"
                                    }
                                    """;
        const string testConfig = """
                                  {
                                    "AccountId": "test_mid_cc_direct",
                                    "Password": "Vc16Fd3+P",
                                    "PassPhrase": "4aE1AdAE7336ACDee72E2F56684A7c05C",
                                    "Endpoint": "https://ts.secure1gateway.com/api/v2/ProcessTx"
                                  }
                                  """;
        var client = _httpClientFactory.CreateClient();
        await EuPay.TestAsync(client, testConfig, _logger);
    }

    private async Task TestMonetixPay()
    {
        // projectID: 139462(VND), 139472(KHR)
        // const string config = """
        //                       {
        //                         "Login": "g2_75322",
        //                         "Password": "M2Q3YWVm",
        //                         "SecretKeyForAesEncrypt": "BYw1^%8%ootYS^O@qTdP[%Ibn1n5Qvnk",
        //                         "ProjectId": "139472", 
        //                         "SecretKeyForSignature": "d3500d00a61a2c20a8af66b8699304d230e2cfb005e8f962f9231945f807f7233cac3431134727cad504b08d59a6a82387a0ed4ae8a2b17716869bc80dc470a8",
        //                         "Server": "pay188pay.com/g2",
        //                       }
        //                       """;

        // const string config = """
        //                       {
        //                         "Login": "g2_75322",
        //                         "Password": "M2Q3YWVm",
        //                         "SecretKeyForAesEncrypt": "yrj7ud2tf4SLRSP-vYTkG31FQW3hBe",
        //                         "ProjectId": "139472", 
        //                         "SecretKeyForSignature": "_u1ybt5a",
        //                         "Server": "pay188pay.com/g2",
        //                       }
        //                       """;

        const string config = """
                              {
                                "Login": "g2_75322",
                                "Password": "M2Q3YWVm",
                                "SecretKeyForAesEncrypt": "BYw1^%8%ootYS^O@qTdP[%Ibn1n5Qvnk",
                                "ProjectId": "139472", 
                                "SecretKeyForSignature": "9c52b5617af0e864955cc8247f0f479489765c20b574219cbe39df26ad36c0423539360b0725a0474be0a8d5389564fac589f707e062d0a08f2e61d3904b5b24",
                                "Server": "pay188pay.com/g2",
                              }
                              """;
        await Monetix.TestAsync(config, _logger);
    }

    private async Task TestHelp2Pay()
    {
        // projectID: 139462(VND), 139472(KHR)
        // const string config = """
        //                       {
        //                         "Login": "g2_75322",
        //                         "Password": "M2Q3YWVm",
        //                         "SecretKeyForAesEncrypt": "BYw1^%8%ootYS^O@qTdP[%Ibn1n5Qvnk",
        //                         "ProjectId": "139472", 
        //                         "SecretKeyForSignature": "d3500d00a61a2c20a8af66b8699304d230e2cfb005e8f962f9231945f807f7233cac3431134727cad504b08d59a6a82387a0ed4ae8a2b17716869bc80dc470a8",
        //                         "Server": "pay188pay.com/g2",
        //                       }
        //                       """;

        // const string config = """
        //                       {
        //                         "Login": "g2_75322",
        //                         "Password": "M2Q3YWVm",
        //                         "SecretKeyForAesEncrypt": "yrj7ud2tf4SLRSP-vYTkG31FQW3hBe",
        //                         "ProjectId": "139472", 
        //                         "SecretKeyForSignature": "_u1ybt5a",
        //                         "Server": "pay188pay.com/g2",
        //                       }
        //                       """;

        const string config = """
                              {
                              "EndPoint":"https://api.safepaymentapp.com/MerchantTransfer",
                              "MerchantCode":"T0146",
                              "CallbackDomain": "https://api.bvi.thebcr.com",
                              "SecurityCode":"bfGpDQWntFmGCcIuka3x"
                              }
                              """;
        await Help2Pay.TestAsync(config, _logger);
    }

    private async Task TestBigPay()
    {
        const string config = """
                              {
                                  "TenantId": 10000,
                                  "Endpoint": "https://bcr-api-84cyv.ondigitalocean.app/deposits",
                                  "RedirectPrefix": "https://epay-bqx9v.ondigitalocean.app",
                                  "ApiKey": "53c329f9-7810-48f0-a3ef-04f686176a22",
                                  "CallbackSecretKey": "89a3547e-bb93-4173-8a6e-c354f880aa1f",
                                  "CallbackDomain": "https://api.thebcrsea.com"
                              }
                              """;

        // const string config = """
        //                       {
        //                         "Currency": "THB",
        //                         "MerchantId": "ME2333",
        //                         "Endpoint": "https://bipipay.com/bpgateway/api",
        //                         "MerchantSecret": "bipipaykey",
        //                         "CallbackDomain": "https://api.bvi.thebcr.com"
        //                       }
        // https://epay-bqx9v.ondigitalocean.app/?order={hashId}
        //                       """;
        await BigPay.TestAsync(config, _logger);
    }

    private async Task TestSeaBipiPay()
    {
        const string config = """
                              {
                                "Currency": "THB",
                                "MerchantId": "ME2333",
                                "Endpoint": "https://bipipay.com/bpgateway/api",
                                "MerchantSecret": "bipipaykey",
                                "CallbackDomain": "https://api.bvi.thebcr.com"
                              }
                              """;

        const string newConfig = """
                                 {
                                   "Currency": "CNY",
                                   "MerchantId": "ME2333",
                                   "Endpoint": "https://bipipay.com/bpgateway/api",
                                   "MerchantSecret": "bipipaykey",
                                   "CallbackDomain": "https://api.bvi.thebcr.com"
                                 }
                                 """;

        // const string config = """
        //                       {
        //                         "Currency": "THB",
        //                         "MerchantId": "ME2333",
        //                         "Endpoint": "https://bipipay.com/bpgateway/api",
        //                         "MerchantSecret": "bipipaykey",
        //                         "CallbackDomain": "https://api.bvi.thebcr.com"
        //                       }
        // https://epay-bqx9v.ondigitalocean.app/?order={hashId}
        //                       """;
        var client = _httpClientFactory.CreateClient();
        await SeaBipiPay.TestAsync(client, newConfig, _logger);
    }

    private async Task TestBipiPay()
    {
        const string config = """
                              {
                                "Currency": "CNY",
                                "MerchantId": "ME250405FB",
                                "Endpoint": "https://bipipay.com/gateway/api",
                                "MerchantSecret": "2fd5393050f6be173b29744d413a7fecb55bdf08e81931b3f641aa66b0355ce5",
                                "CallbackDomain": "https://api.bvi.thebcr.com"
                              }
                              """;
        var client = _httpClientFactory.CreateClient();
        await BipiPay.TestAsync(client, config, _logger);
    }

    private async Task TestOFAPay()
    {
        // ServiceName	            paytype internal    REF	scode	    key
        //
        // BCR_CNY_Alipay2alipay	P2P	    CL2	        744815975702	FFDB@14rxH.ZpjU
        // BCR_CNY_P2P	            P2P	    CL6	        744815975703	Qx.wicfcgToRWND
        // BCR_CNY_P2P_Manual	    P2P	    CL1	        744815975704	1XsyJj3Vr9OVOk9
        // BCR_CNY_DinTalk	        P2P	    CL5	        744815975705	lLDiAxAfki8aLAQ
        // BCR_CNY_ECNY	        P2P	    CL4	        744815975706	2#p/.SS90q6AkSy
        // BCR_CNY_Alipay_Inter	QP	    CI3	        744815975707	q!UJTxus0kVfmuf
        const string BCR_CNY_Alipay2alipay = """
                                             {
                                                "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                                "PayType": "P2P",
                                                "MerchantId": "744815975702",
                                                "SecretKey": "FFDB@14rxH.ZpjU",
                                                "Currency": "CNY",
                                                "ProductName": "BCR",
                                                "CallbackDomain": "https://api.bvi.thebcr.com"
                                             }
                                             """;
        const string BCR_CNY_P2P = """
                                   {
                                      "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                      "PayType": "P2P",
                                      "MerchantId": "744815975703",
                                      "SecretKey": "Qx.wicfcgToRWND",
                                      "Currency": "CNY",
                                      "ProductName": "BCR",
                                      "CallbackDomain": "https://api.bvi.thebcr.com"
                                   }
                                   """;

        const string BCR_CNY_P2P_Manual = """
                                          {
                                             "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                             "PayType": "P2P",
                                             "MerchantId": "744815975704",
                                             "SecretKey": "1XsyJj3Vr9OVOk9",
                                             "Currency": "CNY",
                                             "ProductName": "BCR",
                                             "CallbackDomain": "https://api.bvi.thebcr.com"
                                          }
                                          """;

        const string BCR_CNY_DinTalk = """
                                       {
                                          "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                          "PayType": "P2P",
                                          "MerchantId": "744815975705",
                                          "SecretKey": "lLDiAxAfki8aLAQ",
                                          "Currency": "CNY",
                                          "ProductName": "BCR",
                                          "CallbackDomain": "https://api.bvi.thebcr.com"
                                       }
                                       """;

        const string BCR_CNY_ECNY = """
                                    {
                                       "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                       "PayType": "P2P",
                                       "MerchantId": "744815975706",
                                       "SecretKey": "2#p/.SS90q6AkSy",
                                       "Currency": "CNY",
                                       "ProductName": "BCR",
                                       "CallbackDomain": "https://api.bvi.thebcr.com"
                                    }
                                    """;

        const string BCR_CNY_Alipay_Inter = """
                                            {
                                               "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                               "PayType": "QP",
                                               "MerchantId": "744815975707",
                                               "SecretKey": "q!UJTxus0kVfmuf",
                                               "Currency": "CNY",
                                               "ProductName": "BCR",
                                               "CallbackDomain": "https://api.bvi.thebcr.com"
                                            }
                                            """;

        const string BCR_VND_OB = """
                                  {
                                     "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                     "PayType": "VND",
                                     "MerchantId": "744815975708",
                                     "SecretKey": "xD.lMrlb2BFfvQ@",
                                     "Currency": "VND",
                                     "ProductName": "BCR",
                                     "CallbackDomain": "https://api.bvi.thebcr.com"
                                  }
                                  """;
        const string BCR_VND_P2C = """
                                   {
                                      "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                      "PayType": "VND",
                                      "MerchantId": "744815975709",
                                      "SecretKey": "zRFr6gHdQmb7oSw",
                                      "Currency": "VND",
                                      "ProductName": "BCR",
                                      "CallbackDomain": "https://api.bvi.thebcr.com"
                                   }
                                   """;

        const string BCR_VND_MOMO = """
                                    {
                                       "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                       "PayType": "VND",
                                       "MerchantId": "744815975710",
                                       "SecretKey": "LDGlO!x0LXDmGAW",
                                       "Currency": "VND",
                                       "ProductName": "BCR",
                                       "CallbackDomain": "https://api.bvi.thebcr.com",
                                       "RedirectPage": "0"
                                    }
                                    """;

        const string BCR_VND_Zalo = """
                                    {
                                       "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                       "PayType": "VND",
                                       "MerchantId": "744815975711",
                                       "SecretKey": "IAwnz9!5sv5JgWI",
                                       "Currency": "VND",
                                       "ProductName": "BCR",
                                       "CallbackDomain": "https://api.bvi.thebcr.com",
                                        "RedirectPage": "0"
                                    }
                                    """;

        const string BCR_VND_Viettelpay = """
                                          {
                                             "EndPoint": "https://www.jzc899.com/pay/order.aspx",
                                             "PayType": "VND",
                                             "MerchantId": "744815975712",
                                             "SecretKey": "I3dlMimrolMbKmj",
                                             "Currency": "VND",
                                             "ProductName": "BCR",
                                             "CallbackDomain": "https://api.bvi.thebcr.com",
                                             "RedirectPage": "0"
                                          }
                                          """;
        await OFAPay.TestAsync(BCR_CNY_Alipay2alipay, _logger);
    }

    private async Task TestDragonPayAsync()
    {
        var config = """
                     {
                         "callbackDomain": "https://api.bvi.thebcr.com",
                         "network": "DirectDebit",
                         "currency": "PHP",
                         "merchantSecret": "433a37a6-e227-49b4-8063-951393b0c48c",
                         "merchantToken": "c8eca877-2762-4a4d-8f26-70fd9c951670",
                         "endPointBase": "https://payment.pa-sys.com/app/page",
                     }
                     """;

        var client = _httpClientFactory.CreateClient();
        var res = await DragonPayPHP.TestAsync(client, config, _logger);
        _logger.LogInformation("TestDragonPayAsync: {res}", res);
    }

    /// <summary>
    /// Test Long77 Pay Virtual Account (VA) payment with demo/test environment
    /// Usage: dotnet run -- cmd test test-long77-vn-payment
    /// </summary>
    private async Task TestLong77VnPayment()
    {
        Console.WriteLine("🧪 Testing Long77 Pay Virtual Account (VN Payment) with demo environment...");
        Console.WriteLine("📋 Test Configuration:");
        Console.WriteLine("   Test ID: 10000");
        Console.WriteLine("   Test KEY: 8K3A813Oj6izd595453Ct399w4");
        Console.WriteLine("   Demo Endpoint: http://8.219.116.132:7699/demoBNB/");
        Console.WriteLine();

        // Demo/test configuration
        // Correct endpoint: http://8.219.116.132:7699/gateway/bnb/createVA.do
        // (Verified: this endpoint returns JSON responses)
        const string config = """
                              {
                                "MerchantId": "10000",
                                "MerchantSecret": "8K3A813Oj6izd595453Ct399w4",
                                "Endpoint": "http://8.219.116.132:7699/",
                                "CallbackDomain": "http://8.219.116.132:7699/demoBNB",
                                "VAEndpoint": "http://8.219.116.132:7699/gateway/bnb/createVA.do",
                                "VATrackingEndpoint": "http://8.219.116.132:7699/gateway/bnb/paymentDetailsVA.do",
                                "TransferEndpoint": "http://8.219.116.132:7699/gateway/bnb/transferATM.do",
                                "TransferTrackingEndpoint": "http://8.219.116.132:7699/gateway/bnb/transferDetailsATM.do"
                              }
                              """;

        // Production configuration 
        //const string config = """
        //                      {
        //                        "MerchantId": "10216",
        //                        "MerchantSecret": "X0z4r726NZh25dX0d10BX2U0",
        //                        "Endpoint": "https://vi.long77.net/",
        //                        "CallbackDomain": "https://vi.long77.net/demoBNB",
        //                        "VAEndpoint": "https://vi.long77.net/gateway/bnb/createVA.do",
        //                        "VATrackingEndpoint": "https://vi.long77.net/gateway/bnb/paymentDetailsVA.do",
        //                        "TransferEndpoint": "https://vi.long77.net/gateway/bnb/transferATM.do",
        //                        "TransferTrackingEndpoint": "https://vi.long77.net/gateway/bnb/transferDetailsATM.do"
        //                      }
        //                      """;

        try
        {
            var options = Long77PayOptions.FromJson(config);
            options.TenantId = 10000; // Test tenant ID
            // For demo test, use the demo callback URL
            var demoCallbackUrl = "http://8.219.116.132:7699/demoBNB/payNotify.php";

            // Create VA request
            // Note: Based on successful test, customer_name and payee_name can be empty strings
            // Generate partner_order_code in the format: Test + timestamp + random (no dashes)
            // Example from successful test: Test2025111712442054267
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000000, 9999999).ToString();
            var partnerOrderCode = $"Test{timestamp}{random}";
            
            var vaRequest = new Long77PayVARequestModel
            {
                Amount = 1000000, // 1,000,000 VND (matching successful test)
                PaymentNumber = partnerOrderCode, // Use format matching successful test
                ReturnUrl = "http://8.219.116.132:7699/demoBNB/", // Use demo return URL
                //ReturnUrl = "http://localhost:"
                CustomerName = "", // Empty string as per successful test
                PayeeName = "", // Empty string as per successful test
                ExtraData = "" // Empty string as per successful test
            };
            vaRequest.ApplyOptions(options);
            
            // Build form and override notify_url for demo test, then rebuild signature
            var form = new Dictionary<string, string>
            {
                { "partner_id", options.MerchantId },
                { "timestamp", Utils.ToTimestamp(DateTime.UtcNow).ToString() },
                { "random", Guid.NewGuid().ToString() },
                { "partner_order_code", vaRequest.PaymentNumber },
                { "amount", vaRequest.Amount.ToString() },
                { "notify_url", demoCallbackUrl }, // Use demo callback URL
                { "return_url", vaRequest.ReturnUrl },
                { "customer_name", vaRequest.CustomerName },
                { "payee_name", vaRequest.PayeeName },
                { "extra_data", vaRequest.ExtraData }
            };
            
            // Build signature string and sign
            var signatureString = $"{form["partner_id"]}:{form["timestamp"]}:{form["random"]}:" +
                                 $"{form["partner_order_code"]}:{form["amount"]}:{form["customer_name"]}:" +
                                 $"{form["payee_name"]}:{form["notify_url"]}:{form["return_url"]}:" +
                                 $"{form["extra_data"]}:{options.MerchantSecret}";
            var signature = Utils.Md5Hash(signatureString).ToLower();
            form.Add("sign", signature);
            
            var signedForm = form;

            Console.WriteLine("📤 Request Details:");
            Console.WriteLine($"   Amount: {vaRequest.Amount:N0} VND");
            Console.WriteLine($"   Payment Number: {vaRequest.PaymentNumber}");
            Console.WriteLine($"   Customer Name: {(string.IsNullOrEmpty(vaRequest.CustomerName) ? "(empty)" : vaRequest.CustomerName)}");
            Console.WriteLine($"   Payee Name: {(string.IsNullOrEmpty(vaRequest.PayeeName) ? "(empty)" : vaRequest.PayeeName)}");
            Console.WriteLine($"   Return URL: {vaRequest.ReturnUrl}");
            Console.WriteLine($"   Notify URL: {demoCallbackUrl}");
            Console.WriteLine($"   VA Endpoint: {options.VAEndpoint}");
            Console.WriteLine();

            Console.WriteLine("🔐 Signed Request Form:");
            foreach (var kvp in signedForm)
            {
                Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
            }
            Console.WriteLine($"   Signature String: {signatureString}");
            Console.WriteLine();

            // Send request - try multiple endpoint variations if first fails
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(signedForm), Encoding.UTF8, "application/json");
            
            // Use the correct endpoint: http://8.219.116.132:7699/gateway/bnb/createVA.do
            // (Based on successful test, this is the correct path without /demoBNB/ prefix)
            var endpoint = options.VAEndpoint;

            Console.WriteLine($"📡 Sending POST request to: {endpoint}");
            var response = await client.PostAsync(endpoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"📥 Response Status: {response.StatusCode} ({(int)response.StatusCode})");

            Console.WriteLine($"📄 Response Body:");
            Console.WriteLine(responseJson);
            Console.WriteLine();

            if (response.IsSuccessStatusCode)
            {
                // Parse JSON response
                dynamic? obj = null;
                try
                {
                    obj = Utils.JsonDeserializeDynamic(responseJson);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to parse JSON response: {ex.Message}");
                    Console.WriteLine($"   Raw response: {responseJson}");
                    return;
                }

                if (obj == null)
                {
                    Console.WriteLine($"❌ Response is null or empty");
                    return;
                }

                // Try to parse using Long77PayVAResponseModel
                Long77PayVAResponseModel vaResponse;
                try
                {
                    vaResponse = Long77PayVAResponseModel.FromDynamic(obj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to parse response model: {ex.Message}");
                    Console.WriteLine($"   Response type: {obj?.GetType().Name ?? "null"}");
                    Console.WriteLine($"   Try using SelectToken to access properties:");
                    try
                    {
                        // Try manual parsing as fallback
                        var codeToken = ((Newtonsoft.Json.Linq.JObject)obj).SelectToken("code");
                        var msgToken = ((Newtonsoft.Json.Linq.JObject)obj).SelectToken("msg");
                        Console.WriteLine($"   code: {codeToken?.ToString() ?? "null"}");
                        Console.WriteLine($"   msg: {msgToken?.ToString() ?? "null"}");
                    }
                    catch
                    {
                        // Ignore
                    }
                    return;
                }

                if (vaResponse.IsSuccess)
                {
                    Console.WriteLine("✅ SUCCESS! Virtual Account created successfully");
                    Console.WriteLine($"   System Order Code: {vaResponse.SystemOrderCode}");
                    Console.WriteLine($"   Partner Order Code: {vaResponse.PartnerOrderCode}");
                    Console.WriteLine($"   Amount: {vaResponse.Amount:N0} VND");
                    Console.WriteLine($"   Payment ID: {vaResponse.PaymentId ?? "(null)"}");
                    
                    // Detailed payment URL analysis
                    Console.WriteLine();
                    Console.WriteLine("🔗 Payment URL Analysis:");
                    if (!string.IsNullOrEmpty(vaResponse.PaymentUrl))
                    {
                        Console.WriteLine($"   Raw Payment URL from API: {vaResponse.PaymentUrl}");
                        
                        // Check if URL contains placeholder domains
                        if (vaResponse.PaymentUrl.Contains("xxxxx.com") || 
                            vaResponse.PaymentUrl.Contains("placeholder") ||
                            vaResponse.PaymentUrl.Contains("example.com"))
                        {
                            Console.WriteLine("   ⚠️  WARNING: Payment URL contains placeholder domain!");
                            Console.WriteLine("   This suggests the API is using a default/placeholder URL.");
                            Console.WriteLine("   Possible causes:");
                            Console.WriteLine("     1. return_url in request might be incorrect or missing");
                            Console.WriteLine("     2. API configuration issue on Long77 Pay side");
                            Console.WriteLine("     3. Merchant account not properly configured");
                        }
                        
                        // Check if URL is relative
                        if (vaResponse.PaymentUrl.StartsWith("/"))
                        {
                            Console.WriteLine("   ℹ️  Payment URL is relative (starts with /)");
                            var constructedUrl = options.Endpoint.TrimEnd('/') + vaResponse.PaymentUrl;
                            Console.WriteLine($"   Constructed full URL: {constructedUrl}");
                        }
                        
                        // Check if URL is absolute but wrong domain
                        if (vaResponse.PaymentUrl.StartsWith("http://") || vaResponse.PaymentUrl.StartsWith("https://"))
                        {
                            try
                            {
                                var paymentUri = new Uri(vaResponse.PaymentUrl);
                                var endpointUri = new Uri(options.Endpoint);
                                if (paymentUri.Host != endpointUri.Host)
                                {
                                    Console.WriteLine($"   ⚠️  Domain mismatch!");
                                    Console.WriteLine($"      Payment URL domain: {paymentUri.Host}");
                                    Console.WriteLine($"      Expected domain: {endpointUri.Host}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"   ❌ Failed to parse payment URL: {ex.Message}");
                            }
                        }
                        
                        Console.WriteLine();
                        Console.WriteLine("📋 Request Fields Sent to API:");
                        Console.WriteLine($"   return_url: {form["return_url"]}");
                        Console.WriteLine($"   notify_url: {form["notify_url"]}");
                        Console.WriteLine($"   partner_id: {form["partner_id"]}");
                    }
                    else
                    {
                        Console.WriteLine("   ❌ Payment URL is NULL or EMPTY!");
                        Console.WriteLine("   This is unusual - check the API response structure.");
                    }
                    
                    // Show raw response data.payment_url if available
                    try
                    {
                        var paymentUrlToken = ((Newtonsoft.Json.Linq.JObject)obj).SelectToken("data.payment_url");
                        if (paymentUrlToken != null)
                        {
                            Console.WriteLine($"   Raw data.payment_url from JSON: {paymentUrlToken.ToString()}");
                        }
                        else
                        {
                            Console.WriteLine("   ⚠️  data.payment_url not found in response JSON");
                            Console.WriteLine("   Available data fields:");
                            var dataToken = ((Newtonsoft.Json.Linq.JObject)obj).SelectToken("data");
                            if (dataToken != null && dataToken is Newtonsoft.Json.Linq.JObject dataObj)
                            {
                                foreach (var prop in dataObj.Properties())
                                {
                                    Console.WriteLine($"      - {prop.Name}: {prop.Value}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ⚠️  Could not extract raw payment_url: {ex.Message}");
                    }
                    
                    Console.WriteLine();
                    if (vaResponse.BankAccount != null)
                    {
                        Console.WriteLine($"   Bank Account Info:");
                        Console.WriteLine($"      Bank Code: {vaResponse.BankAccount.BankCode}");
                        Console.WriteLine($"      Bank Name: {vaResponse.BankAccount.BankName}");
                        Console.WriteLine($"      Account No: {vaResponse.BankAccount.BankAccountNo}");
                        Console.WriteLine($"      Account Name: {vaResponse.BankAccount.BankAccountName}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ FAILED: {vaResponse.Message ?? "Unknown error"}");
                    Console.WriteLine($"   Code: {vaResponse.Code}");
                }
            }
            else
            {
                Console.WriteLine($"❌ HTTP Request Failed: {response.StatusCode}");
                Console.WriteLine($"   Response: {responseJson}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception occurred: {ex.Message}");
            Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
            _logger.LogError(ex, "TestLong77VnPayment failed");
        }
    }

    /// <summary>
    /// Test Long77 Pay Transfer (ATM) payment with demo/test environment
    /// Usage: dotnet run -- cmd test test-long77-transfer
    /// </summary>
    //private async Task TestLong77Transfer()
    //{
    //    Console.WriteLine("🧪 Testing Long77 Pay Transfer (ATM) with demo environment...");
    //    Console.WriteLine("📋 Test Configuration:");
    //    Console.WriteLine("   Test ID: 10000");
    //    Console.WriteLine("   Test KEY: 8K3A813Oj6izd595453Ct399w4");
    //    Console.WriteLine("   Demo Endpoint: http://8.219.116.132:7699/demoBNB/");
    //    Console.WriteLine();

    //    // Demo/test configuration
    //    const string config = """
    //                          {
    //                            "MerchantId": "10000",
    //                            "MerchantSecret": "8K3A813Oj6izd595453Ct399w4",
    //                            "Endpoint": "http://8.219.116.132:7699/",
    //                            "CallbackDomain": "http://8.219.116.132:7699/demoBNB",
    //                            "VAEndpoint": "http://8.219.116.132:7699/gateway/bnb/createVA.do",
    //                            "VATrackingEndpoint": "http://8.219.116.132:7699/gateway/bnb/paymentDetailsVA.do",
    //                            "TransferEndpoint": "http://8.219.116.132:7699/gateway/bnb/transferATM.do",
    //                            "TransferTrackingEndpoint": "http://8.219.116.132:7699/gateway/bnb/transferDetailsATM.do"
    //                          }
    //                          """;

    //    try
    //    {
    //        var options = Long77PayOptions.FromJson(config);
    //        options.TenantId = 10000; // Test tenant ID
    //        // For demo test, use the demo callback URL
    //        var demoCallbackUrl = "http://8.219.116.132:7699/demoBNB/transferNotify.php";

    //        // Create Transfer request
    //        // Generate partner_order_code in the format: Test + timestamp + random (no dashes)
    //        // Example from successful test: TestBNBdf2025111713313487624
    //        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    //        var random = new Random().Next(1000000, 9999999).ToString();
    //        var partnerOrderCode = $"TestBNBdf{timestamp}{random}";

    //        var transferRequest = new Long77PayTransferRequestModel
    //        {
    //            Amount = 1000000, // 1,000,000 VND (matching successful test)
    //            PaymentNumber = partnerOrderCode, // Use format matching successful test
    //            BankCode = "ABBANK", // From sample: bank: ABBANK
    //            BankAccountNo = "123456123456", // From sample: account no: 123456123456
    //            BankAccountName = "test Name2", // From sample: account name: test Name2
    //            //BankName = "ABBANK", // Use same as BankCode
    //            ExtraData = "" // Empty string
    //        };
    //        transferRequest.ApplyOptions(options);

    //        // Build form and override notify_url for demo test, then rebuild signature
    //        // API expects payee_ prefix for bank account fields and payee_bank_account_type
    //        // Signature format: partner_id:timestamp:random:partner_order_code:amount:payee_bank_code:payee_bank_account_type:payee_bank_account_no:payee_bank_account_name:message:extra_data:partner_secret
    //        var form = new Dictionary<string, string>
    //        {
    //            { "partner_id", options.MerchantId },
    //            { "timestamp", Utils.ToTimestamp(DateTime.UtcNow).ToString() },
    //            { "random", Guid.NewGuid().ToString() },
    //            { "partner_order_code", transferRequest.PaymentNumber },
    //            { "amount", transferRequest.Amount.ToString() },
    //            { "payee_bank_code", transferRequest.BankCode }, // Use payee_ prefix
    //            { "payee_bank_account_type", "account" }, // Required field: "account" or "card"
    //            { "payee_bank_account_no", transferRequest.BankAccountNo }, // Use payee_ prefix
    //            { "payee_bank_account_name", transferRequest.BankAccountName }, // Use payee_ prefix
    //            { "message", "" }, // Message field for signature (can be empty)
    //            { "notify_url", demoCallbackUrl }, // Use demo callback URL
    //            { "extra_data", transferRequest.ExtraData }
    //        };

    //        // Build signature string and sign
    //        // Format: partner_id:timestamp:random:partner_order_code:amount:payee_bank_code:payee_bank_account_type:payee_bank_account_no:payee_bank_account_name:message:extra_data:partner_secret
    //        var signatureString = $"{form["partner_id"]}:{form["timestamp"]}:{form["random"]}:" +
    //                             $"{form["partner_order_code"]}:{form["amount"]}:{form["payee_bank_code"]}:" +
    //                             $"{form["payee_bank_account_type"]}:{form["payee_bank_account_no"]}:" +
    //                             $"{form["payee_bank_account_name"]}:{form["message"]}:{form["extra_data"]}:" +
    //                             $"{options.MerchantSecret}";
    //        var signature = Utils.Md5Hash(signatureString).ToLower();
    //        form.Add("sign", signature);

    //        var signedForm = form;

    //        Console.WriteLine("📤 Request Details:");
    //        Console.WriteLine($"   Amount: {transferRequest.Amount:N0} VND");
    //        Console.WriteLine($"   Payment Number: {transferRequest.PaymentNumber}");
    //        Console.WriteLine($"   Payee Bank Code: {transferRequest.BankCode}");
    //        Console.WriteLine($"   Payee Bank Account Type: account");
    //        Console.WriteLine($"   Payee Bank Account No: {transferRequest.BankAccountNo}");
    //        Console.WriteLine($"   Payee Bank Account Name: {transferRequest.BankAccountName}");
    //        Console.WriteLine($"   Notify URL: {demoCallbackUrl}");
    //        Console.WriteLine($"   Transfer Endpoint: {options.TransferEndpoint}");
    //        Console.WriteLine();

    //        Console.WriteLine("🔐 Signed Request Form:");
    //        foreach (var kvp in signedForm)
    //        {
    //            Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
    //        }
    //        Console.WriteLine($"   Signature String: {signatureString}");
    //        Console.WriteLine();

    //        // Send request
    //        var client = _httpClientFactory.CreateClient();
    //        var content = new StringContent(JsonConvert.SerializeObject(signedForm), Encoding.UTF8, "application/json");

    //        var endpoint = options.TransferEndpoint;

    //        Console.WriteLine($"📡 Sending POST request to: {endpoint}");
    //        var response = await client.PostAsync(endpoint, content);
    //        var responseJson = await response.Content.ReadAsStringAsync();

    //        Console.WriteLine($"📥 Response Status: {response.StatusCode} ({(int)response.StatusCode})");

    //        Console.WriteLine($"📄 Response Body:");
    //        Console.WriteLine(responseJson);
    //        Console.WriteLine();

    //        if (response.IsSuccessStatusCode)
    //        {
    //            // Parse JSON response
    //            dynamic? obj = null;
    //            try
    //            {
    //                obj = Utils.JsonDeserializeDynamic(responseJson);
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine($"❌ Failed to parse JSON response: {ex.Message}");
    //                Console.WriteLine($"   Raw response: {responseJson}");
    //                return;
    //            }

    //            if (obj == null)
    //            {
    //                Console.WriteLine($"❌ Response is null or empty");
    //                return;
    //            }

    //            // Try to parse using Long77PayTransferResponseModel
    //            Long77PayTransferResponseModel transferResponse;
    //            try
    //            {
    //                transferResponse = Long77PayTransferResponseModel.FromDynamic(obj, responseJson);
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine($"❌ Failed to parse response model: {ex.Message}");
    //                Console.WriteLine($"   Response type: {obj?.GetType().Name ?? "null"}");
    //                Console.WriteLine($"   Try using SelectToken to access properties:");
    //                try
    //                {
    //                    // Try manual parsing as fallback
    //                    var codeToken = ((Newtonsoft.Json.Linq.JObject)obj).SelectToken("code");
    //                    var msgToken = ((Newtonsoft.Json.Linq.JObject)obj).SelectToken("msg");
    //                    var dataToken = ((Newtonsoft.Json.Linq.JObject)obj).SelectToken("data");
    //                    Console.WriteLine($"   code: {codeToken?.ToString() ?? "null"}");
    //                    Console.WriteLine($"   msg: {msgToken?.ToString() ?? "null"}");
    //                    if (dataToken != null)
    //                    {
    //                        var partnerOrderCodeToken = ((Newtonsoft.Json.Linq.JObject)dataToken).SelectToken("partner_order_code");
    //                        Console.WriteLine($"   data.partner_order_code: {partnerOrderCodeToken?.ToString() ?? "null"}");
    //                    }
    //                }
    //                catch
    //                {
    //                    // Ignore
    //                }
    //                return;
    //            }

    //            // Show all available fields in response for troubleshooting
    //            Console.WriteLine("📋 Response Analysis:");
    //            try
    //            {
    //                var dataToken = ((Newtonsoft.Json.Linq.JObject)obj).SelectToken("data");
    //                if (dataToken != null && dataToken is Newtonsoft.Json.Linq.JObject dataObj)
    //                {
    //                    Console.WriteLine("   Available data fields:");
    //                    foreach (var prop in dataObj.Properties())
    //                    {
    //                        Console.WriteLine($"      - {prop.Name}: {prop.Value}");
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine($"   ⚠️  Could not parse response data: {ex.Message}");
    //            }

    //            Console.WriteLine();
    //            Console.WriteLine("ℹ️  Note: Transfer (payout/withdrawal) operations do NOT return a payment_url.");
    //            Console.WriteLine("   Payment URLs are only returned by Virtual Account (deposit) operations.");
    //            Console.WriteLine("   To test payment URL, use: test-long77-vn-payment");
    //            Console.WriteLine();

    //            if (transferResponse.IsSuccess)
    //            {
    //                Console.WriteLine("✅ SUCCESS! Transfer initiated successfully");
    //                Console.WriteLine($"   System Order Code: {transferResponse.SystemOrderCode ?? "N/A"}");
    //                Console.WriteLine($"   Partner Order Code: {transferResponse.PartnerOrderCode ?? "N/A"}");
    //                if (transferResponse.Amount.HasValue)
    //                {
    //                    Console.WriteLine($"   Amount: {transferResponse.Amount:N0} VND");
    //                }
    //            }
    //            else
    //            {
    //                Console.WriteLine($"❌ FAILED: {transferResponse.Message ?? "Unknown error"}");
    //                Console.WriteLine($"   Code: {transferResponse.Code}");
    //            }
    //        }
    //        else
    //        {
    //            Console.WriteLine($"❌ HTTP Request Failed: {response.StatusCode}");
    //            Console.WriteLine($"   Response: {responseJson}");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"❌ Exception occurred: {ex.Message}");
    //        Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
    //        _logger.LogError(ex, "TestLong77Transfer failed");
    //    }
    //}

    public static string ComputeSha512Hash(string input)
    {
        using (SHA512 sha512Hash = SHA512.Create())
        {
            byte[] sourceBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);
            string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty).ToLowerInvariant();
            return hash;
        }
    }

    private async Task MigrateNewPaymentServiceAsync(long tid, long startPartyId)
    {
        var tasks = new[] { tid }.Select(async tenantId =>
        {
            using var scope = _serviceProvider.CreateTenantScope(tenantId);
            var con = scope.ServiceProvider.GetRequiredService<TenantDbConnection>();
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            var cache = scope.ServiceProvider.GetRequiredService<IMyCache>();
            var paymentMethodSvc = scope.ServiceProvider.GetRequiredService<PaymentMethodService>();

            await cache.KeyDeleteAsync(CacheKeys.GetPaymentMethodKey(tenantId));
//             try
//             {
//                 await con.ExecuteAsync($"""
//                                         alter table acct."_Payment" drop constraint "_Payment__PaymentMethod_Id_fk";
//                                         """);
//             }
//             catch
//             {
//                 // ignored
//             }
//
//             try
//             {
//                 await con.ExecuteAsync($"""
//                                         alter table acct."_Payment" drop constraint "_payment__paymentmethod_id_fk";
//                                         """);
//             }
//             catch
//             {
//                 // ignored
//             }
//
//
// //             await con.ExecuteAsync($"""
// //                                     delete from acct."_AccountPaymentMethodAccess";
// //                                     delete from acct."_WalletPaymentMethodAccess";
// //                                     delete from acct."_PaymentMethod";
// //                                     alter sequence acct."_PaymentMethod_Id_seq" restart with 1;
// //                                     """);
//
//               await con.ExecuteAsync($"""
//                                       delete from acct."_AccountPaymentMethodAccess";
//                                       delete from acct."_WalletPaymentMethodAccess";
//                                       delete from acct."_PaymentMethod";
//                                       alter sequence acct."_PaymentMethod_Id_seq" restart with 1;
//                                       """);
//
//
//
//             var items = await ctx.PaymentServices.ToListAsync();
//             foreach (var item in items)
//             {
//                 var obj = Utils.JsonDeserializeDynamic(item.Configuration);
//                 var newItem = new PaymentMethod
//                 {
//                     Id = item.Id,
//                     Platform = item.Platform,
//                     CurrencyId = item.CurrencyId,
//                     MethodType = item.CategoryName == "Withdrawal" ? "Withdrawal" : "Deposit",
//                     Percentage = item.Sequence,
//                     InitialValue = item.InitialValue,
//                     MinValue = item.MinValue,
//                     MaxValue = item.MaxValue,
//                     Name = item.Name,
//                     Configuration = JsonConvert.SerializeObject(obj),
//                     CommentCode = item.CommentCode,
//                     IsHighDollarEnabled = item.IsHighDollarEnabled,
//                     IsAutoDepositEnabled = item.IsAutoDepositEnabled,
//                     Group = item.CategoryName,
//                     Status = item.IsActivated == 1 && (item.CanDeposit == 1 || item.CanWithdraw == 1)
//                         ? (short)PaymentMethodStatusTypes.Active
//                         : (short)PaymentMethodStatusTypes.Inactive,
//                 };
//                 ctx.PaymentMethods.Add(newItem);
//                 await ctx.SaveChangesAsync();
//             }
//
//             var maxId = await ctx.PaymentMethods.MaxAsync(x => x.Id);
//
//             await con.ExecuteAsync($"""
//                                     alter table acct."_Payment"
//                                     add constraint "_Payment__PaymentMethod_Id_fk"
//                                         foreign key ("PaymentServiceId") references acct."_PaymentMethod";
//
//                                     alter sequence acct."_PaymentMethod_Id_seq" restart with {maxId + 1};
//                                     """);


            // return;

            
            
            const int size = 100;
            var methods = await paymentMethodSvc.GetMethodsAsync(true);

            var qq = ctx.Parties.Where(x =>
                x.PaymentServiceAccesses.Count != 0 && (x.Accounts.Count != 0 || x.Wallets.Count != 0));
            qq = qq.Where(x => x.Id >= startPartyId);
            // if (tid == 1) qq = qq.Where(x => x.Id > 18123);
            // if (tid == 10004) qq = qq.Where(x => x.Id > 320383);
            // if (tid == 10000) qq = qq.Where(x => x.Id > 17670);

            var query = qq.AsNoTracking()
                // .Where(x => x.Accounts.Any(y => y.Uid == 33197738))
                .Include(x => x.Accounts)
                .Include(x => x.Wallets)
                .Include(x => x.PaymentServiceAccesses)
                .OrderBy(x => x.Id);

            var total = await query.CountAsync();
            using var bar = CreateBar(total);
            var page = 1;
            while (true)
            {
                var parties = await query
                    .Skip((page - 1) * size).Take(size)
                    .ToListAsync();

                foreach (var party in parties)
                {
                    var currentProcess = Process.GetCurrentProcess();

                    // 获取私有内存大小（以字节为单位） 
                    var privateMemorySize = currentProcess.PrivateMemorySize64;

                    // 获取工作集内存大小（以字节为单位）
                    var workingSetMemorySize = currentProcess.WorkingSet64;

                    // 获取虚拟内存大小（以字节为单位）
                    var virtualMemorySize = currentProcess.VirtualMemorySize64;

                    bar.Tick(
                        $"PrivateMemorySize: {privateMemorySize} WorkingSetMemorySize: {workingSetMemorySize} VirtualMemorySize: {virtualMemorySize}");

                    if (party.Accounts.Count > 0)
                    {
                        await con.ExecuteAsync($"""
                                                delete from acct."_AccountPaymentMethodAccess" where "AccountId" in ({string.Join(',', party.Accounts.Select(x => x.Id))});
                                                """);
                    }

                    if (party.Wallets.Count > 0)
                    {
                        await con.ExecuteAsync($"""
                                                delete from acct."_WalletPaymentMethodAccess" where "WalletId" in ({string.Join(',', party.Wallets.Select(x => x.Id))});
                                                """);
                    }

                    foreach (var account in party.Accounts)
                    {
                        var access = party.PaymentServiceAccesses
                            .Where(x => x.CurrencyId == account.CurrencyId && x.FundType == account.FundType)
                            .Select(x => new AccountPaymentMethodAccess
                            {
                                AccountId = account.Id,
                                PaymentMethodId = methods.First(y => y.Id == x.PaymentServiceId).Id,
                                Status = (short)(x.CanDeposit == 1 || x.CanWithdraw == 1
                                    ? PaymentMethodAccessStatusTypes.Active
                                    : PaymentMethodAccessStatusTypes.Inactive),
                                OperatedPartyId = 1
                            });

                        ctx.AccountPaymentMethodAccesses.AddRange(access);
                    }

                    foreach (var wallet in party.Wallets)
                    {
                        var access = party.PaymentServiceAccesses
                            .Where(x => x.CurrencyId == wallet.CurrencyId && x.FundType == wallet.FundType)
                            .Select(x => new WalletPaymentMethodAccess
                            {
                                WalletId = wallet.Id,
                                PaymentMethodId = methods.First(y => y.Id == x.PaymentServiceId).Id,
                                Status = (short)(x.CanDeposit == 1 || x.CanWithdraw == 1
                                    ? PaymentMethodAccessStatusTypes.Active
                                    : PaymentMethodAccessStatusTypes.Inactive),
                                OperatedPartyId = 1
                            });

                        ctx.WalletPaymentMethodAccesses.AddRange(access);
                    }

                    await ctx.SaveChangesAsync();
                }

                if (parties.Count < size)
                    break;


                // Manual GC parties
                page++;
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Test Twilio SMS Verification service
    /// Usage: dotnet run -- cmd test test-twilio-sms
    /// </summary>
    private async Task TestTwilioSms()
    {
        Console.WriteLine("🧪 Testing Twilio SMS Verification Service...");
        Console.WriteLine("=====================================");
        Console.WriteLine();

        try
        {
            var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            var serviceSid = Environment.GetEnvironmentVariable("TWILIO_SERVICE_SID");

            if (string.IsNullOrEmpty(accountSid) || 
                string.IsNullOrEmpty(authToken) || 
                string.IsNullOrEmpty(serviceSid))
            {
                Console.WriteLine("❌ ERROR: Twilio configuration missing!");
                Console.WriteLine();
                Console.WriteLine("Please ensure the following environment variables are set in .env file:");
                Console.WriteLine("  • TWILIO_ACCOUNT_SID");
                Console.WriteLine("  • TWILIO_AUTH_TOKEN");
                Console.WriteLine("  • TWILIO_SERVICE_SID");
                Console.WriteLine();
                Console.WriteLine("Current values:");
                Console.WriteLine($"  TWILIO_ACCOUNT_SID: {(string.IsNullOrEmpty(accountSid) ? "(not set)" : $"{accountSid[..10]}...{accountSid[^4..]}")}");
                Console.WriteLine($"  TWILIO_AUTH_TOKEN: {(string.IsNullOrEmpty(authToken) ? "(not set)" : "********")}");
                Console.WriteLine($"  TWILIO_SERVICE_SID: {(string.IsNullOrEmpty(serviceSid) ? "(not set)" : $"{serviceSid[..10]}...{serviceSid[^4..]}")}");
                return;
            }

            Console.WriteLine("✅ Twilio Configuration:");
            Console.WriteLine($"   Account SID: {accountSid[..10]}...{accountSid[^4..]}");
            Console.WriteLine($"   Service SID: {serviceSid[..10]}...{serviceSid[^4..]}");
            Console.WriteLine($"   Auth Token: ********** (hidden)");
            Console.WriteLine();

            var smsService = new TwilioSmsVerificationService(accountSid, authToken, serviceSid);

            Console.Write("📱 Enter phone number to test (E.164 format, e.g. +8613800138000): ");
            var testPhone = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(testPhone))
            {
                Console.WriteLine("❌ Phone number is required");
                return;
            }

            if (!testPhone.StartsWith("+"))
            {
                Console.WriteLine("⚠️  WARNING: Phone number should start with '+' (E.164 format)");
                Console.Write("Continue anyway? (y/n): ");
                var confirm = Console.ReadLine()?.Trim().ToLower();
                if (confirm != "y")
                {
                    Console.WriteLine("❌ Test cancelled");
                    return;
                }
            }

            // Remove any spaces from phone number (common mistake)
            testPhone = testPhone.Replace(" ", "").Replace("-", "");

            var limitKey = $"test_sms_{DateTime.UtcNow.Ticks}";

            Console.WriteLine();
            Console.WriteLine($"📤 Sending verification code to: {testPhone}");
            Console.WriteLine($"   Limit Key: {limitKey}");
            Console.WriteLine($"   Max requests per minute: 2");
            Console.WriteLine();
            Console.WriteLine("💡 TRIAL ACCOUNT REMINDER:");
            Console.WriteLine("   If using Twilio Trial account, this number must be verified at:");
            Console.WriteLine("   https://console.twilio.com/us1/develop/phone-numbers/manage/verified");
            Console.WriteLine();

            bool success;
            try
            {
                success = await smsService.Verification(testPhone, limitKey);
            }
            catch (Twilio.Exceptions.ApiException ex)
            {
                Console.WriteLine("❌ Twilio API Error:");
                Console.WriteLine($"   Error: {ex.Message}");
                Console.WriteLine();

                if (ex.Message.Contains("unverified"))
                {
                    Console.WriteLine("🔧 SOLUTION:");
                    Console.WriteLine($"   1. Visit: https://console.twilio.com/us1/develop/phone-numbers/manage/verified");
                    Console.WriteLine($"   2. Click 'Verify a phone number'");
                    Console.WriteLine($"   3. Enter: {testPhone}");
                    Console.WriteLine($"   4. Complete the verification process");
                    Console.WriteLine($"   5. Or upgrade to a paid account to send to any number");
                    Console.WriteLine();
                    Console.WriteLine("📝 Verified number format must be EXACTLY: {testPhone}");
                    Console.WriteLine("   (no spaces, no dashes, with + and country code)");
                }
                else if (ex.Message.Contains("Invalid 'To' Phone Number"))
                {
                    Console.WriteLine("🔧 SOLUTION:");
                    Console.WriteLine($"   The phone number format is invalid.");
                    Console.WriteLine($"   Correct E.164 format: +[country code][number]");
                    Console.WriteLine($"   Example for Australia: +61435863042");
                    Console.WriteLine($"   Your input: {testPhone}");
                }

                throw;
            }

            if (success)
            {
                Console.WriteLine("✅ SMS sent successfully!");
                Console.WriteLine("📱 Check your phone for the verification code");
                Console.WriteLine();
                Console.WriteLine("⏰ Code will expire in 10 minutes");
                Console.WriteLine();

                Console.Write("Enter the 6-digit code you received: ");
                var code = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(code))
                {
                    Console.WriteLine("❌ Code is required");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine($"🔍 Verifying code: {code}");
                Console.WriteLine();

                var (isValid, verifiedPhone) = await smsService.VerificationCheck(testPhone, code);

                if (isValid)
                {
                    Console.WriteLine("✅✅✅ VERIFICATION SUCCESS! ✅✅✅");
                    Console.WriteLine($"   Verified Phone: {verifiedPhone}");
                    Console.WriteLine($"   Code: {code}");
                    Console.WriteLine();
                    Console.WriteLine("🎉 Twilio SMS verification is working correctly!");
                }
                else
                {
                    Console.WriteLine("❌ VERIFICATION FAILED!");
                    Console.WriteLine("   Possible reasons:");
                    Console.WriteLine("   • Code is incorrect");
                    Console.WriteLine("   • Code has expired (10 min timeout)");
                    Console.WriteLine("   • Code was already used");
                    Console.WriteLine("   • Phone number mismatch");
                }

                Console.WriteLine();
                Console.WriteLine("📝 Testing rate limiting...");
                Console.WriteLine($"   Attempting to send another SMS with same limit key: {limitKey}");

                var success2 = await smsService.Verification(testPhone, limitKey);
                if (success2)
                {
                    Console.WriteLine("✅ Second SMS sent (1/2 requests used)");
                }

                var success3 = await smsService.Verification(testPhone, limitKey);
                if (!success3)
                {
                    Console.WriteLine("✅ Third SMS blocked by rate limiter (2/2 limit reached)");
                    Console.WriteLine("   Rate limiting is working correctly!");
                }
                else
                {
                    Console.WriteLine("⚠️  Third SMS was sent (rate limiter might not be working)");
                }
            }
            else
            {
                Console.WriteLine("❌ Failed to send SMS!");
                Console.WriteLine();
                Console.WriteLine("   Possible reasons:");
                Console.WriteLine("   • Invalid phone number format");
                Console.WriteLine("   • Twilio API credentials incorrect");
                Console.WriteLine("   • Service SID not valid");
                Console.WriteLine("   • Phone number not verified in Twilio trial account");
                Console.WriteLine("   • Insufficient Twilio balance");
                Console.WriteLine("   • Network connectivity issues");
                Console.WriteLine();
                Console.WriteLine("   Check Twilio Console logs: https://console.twilio.com/monitor/logs");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
            _logger.LogError(ex, "TestTwilioSms failed");
        }

        Console.WriteLine();
        Console.WriteLine("=====================================");
        Console.WriteLine("✅ Twilio SMS Test Complete!");
    }

    // ============================================================================================
    // Pay247
    //
    // Five test entry points (wired via CmdTestService switch):
    //   test-pay247-sig      → TestPay247Signature      (pure unit test; no creds required)
    //   test-pay247-create   → TestPay247CreateOrder    (iterates EVERY active Pay247 Deposit row)
    //   test-pay247-query    → TestPay247QueryOrder     (interactive; prompts for mch_order_no)
    //   test-pay247-balance  → TestPay247Balance        (lists ALL wallets returned by the API)
    //   test-pay247-payout   → TestPay247CreatePayout   (iterates active Withdrawal rows; picks sample bank)
    //
    // Doc: https://docs.pay247.io/api-reference/en/common.md
    // ============================================================================================

    private const long Pay247TestTenantId = 10000;

    /// <summary>
    /// Unit test of the MD5 signer using a synthetic payload. Asserts deterministic output and
    /// that empty/null/sign fields are stripped before sorting.
    /// </summary>
    private Task TestPay247Signature()
    {
        Console.WriteLine("🧪 Testing Pay247 MD5 signer...");
        Console.WriteLine();

        const string secret = "ac6faf8c05ca457bb4edf2a206a1fa40";

        var spec = new Dictionary<string, object?>
        {
            ["mch_id"]       = "EZC1LTGNQY10177",
            ["mch_order_no"] = "pm-2620abc12345",
            ["mch_user_id"]  = "325150104",
            ["currency"]     = "VND",
            ["amount"]       = 50000m,
            ["pay_method"]   = "BANK",
            ["pay_theme"]    = "link",
            ["client_ip"]    = "127.0.0.1",
            ["notify_url"]   = "https://example.com/cb",
            ["return_url"]   = "https://example.com/return",
            ["payer_name"]   = "TEST PAYER",
            ["payer_phone"]  = "0900000000",
            ["payer_email"]  = "test@example.com",
            ["timestamp"]    = 1700000000000L,    // deterministic for repeatability
            ["version"]      = "v3.0",
            ["uuid"]         = "550e8400-e29b-41d4-a716-446655440000",
            ["emptyShouldBeStripped"] = "",
            ["nullShouldBeStripped"]  = null,
        };

        var sig1 = Pay247.GenerateSignature(spec, secret, _logger);
        var sig2 = Pay247.GenerateSignature(spec, secret, _logger);

        Console.WriteLine($"   Computed: {sig1}");
        Console.WriteLine($"   Repeated: {sig2}");
        Console.WriteLine($"   Length:   {sig1.Length} (expected 32 for MD5 hex)");
        Console.WriteLine($"   Deterministic: {(sig1 == sig2 ? "✅ YES" : "❌ NO")}");

        // Sanity: an unrelated tweak must change the signature.
        spec["amount"] = 50001m;
        var sig3 = Pay247.GenerateSignature(spec, secret, _logger);
        Console.WriteLine($"   Tweaked:  {sig3}");
        Console.WriteLine($"   Differs:  {(sig3 != sig1 ? "✅ YES" : "❌ NO")}");

        // Regression: real prod pay-in callback. Pay247 KEEPS empty-string fields (error="")
        // in the callback signature, unlike the request direction. VerifySignatureFromRaw must
        // therefore sign with dropEmpty:false, or this false-mismatches like prod did 2026-06-12.
        Console.WriteLine();
        Console.WriteLine("   Callback verify (real prod payload, error=\"\" must be kept):");
        const string callbackRaw =
            "{\"mch_id\":\"EZC1LTGNQY10177\",\"mch_order_no\":\"pm-26247e15516d\"," +
            "\"order_no\":\"PIIDR1017720260611153338982328\",\"status\":\"CLOSED\"," +
            "\"currency\":\"IDR\",\"pay_method\":\"BANK\",\"amount\":\"9143025.00\"," +
            "\"fee\":\"0.00\",\"error\":\"\",\"created_at\":1781163218000," +
            "\"sign\":\"68a37848a1738ed424f990565df2dc1b\"}";
        var callbackOk = Pay247.VerifySignatureFromRaw(callbackRaw, secret, _logger);
        Console.WriteLine($"   Callback signature valid: {(callbackOk ? "✅ YES" : "❌ NO")}");

        Console.WriteLine();
        Console.WriteLine("=====================================");
        Console.WriteLine("✅ Pay247 Signature Test Complete!");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Iterates every active Pay247 Deposit row for the test tenant and fires one
    /// create-collection-order per row. Currency and PayMethod are read from each row's
    /// <c>Configuration</c> — never hardcoded — so this naturally exercises every
    /// (currency × pay_method) combination ops has enabled.
    /// </summary>
    private async Task TestPay247CreateOrder()
    {
        Console.WriteLine("🧪 Testing Pay247 Create Payin Order...");
        Console.WriteLine();

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(Pay247TestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var methods = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.Pay247)
                .Where(x => x.MethodType == PaymentMethodTypes.Deposit)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .ToListAsync();

            if (methods.Count == 0)
            {
                Console.WriteLine("❌ No active Pay247 Deposit payment method rows for tenant {0}.", Pay247TestTenantId);
                Console.WriteLine("   Seed via the DB script in the Pay247 skill doc before running this test.");
                return;
            }

            Console.WriteLine($"Found {methods.Count} active Pay247 Deposit row(s); creating one test order per row.");
            Console.WriteLine("=====================================");

            foreach (var method in methods)
            {
                var options = Pay247Options.FromJson(method.Configuration);
                options.TenantId = Pay247TestTenantId;

                var (isValid, err) = options.Validate();
                if (!isValid)
                {
                    Console.WriteLine($"⚠ MethodId={method.Id}: invalid config — {err}");
                    continue;
                }

                Console.WriteLine($"📤 MethodId={method.Id}  Currency={options.Currency}  PayMethod={options.PayMethod}");

                var client = new Pay247.RequestClient
                {
                    PaymentNumber = Payment.GenerateNumber(),
                    AccountUid    = 325150104, // test client uid; ignored by Pay247 except for echo
                    Amount        = SuggestedTestAmount(options.Currency),
                    Currency      = options.Currency,
                    PayMethod     = options.PayMethod,
                    ClientIp      = "127.0.0.1",
                    PayerName     = "TEST PAYER",
                    PayerPhone    = "0900000000",
                    PayerEmail    = "test@example.com",
                    ReturnUrl     = "https://example.com/return",
                    Subject       = $"Pay247 test {options.Currency}/{options.PayMethod}",
                    Options       = options,
                    Client        = _httpClientFactory.CreateClient(),
                    Logger        = _logger,
                };

                var response = await client.RequestAsync();
                Console.WriteLine($"   IsSuccess     = {response.IsSuccess}");
                Console.WriteLine($"   Action        = {response.Action}");
                Console.WriteLine($"   PaymentNumber = {response.PaymentNumber}");
                Console.WriteLine($"   Reference     = {response.Reference}");
                Console.WriteLine($"   RedirectUrl   = {response.RedirectUrl}");
                if (!response.IsSuccess)
                    Console.WriteLine($"   ⚠ Message    = {response.Message}");
                Console.WriteLine("-------------------------------------");
            }

            Console.WriteLine("✅ Pay247 Create Order Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            _logger.LogError(ex, "TestPay247CreateOrder failed");
        }
    }

    /// <summary>Prompts for a <c>mch_order_no</c> and queries the Pay247 payin-status endpoint.</summary>
    private async Task TestPay247QueryOrder()
    {
        Console.WriteLine("🧪 Testing Pay247 Query Payin Order...");
        Console.Write("Enter mch_order_no (the PaymentNumber we sent to Pay247): ");
        var orderNo = (Console.ReadLine() ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(orderNo))
        {
            Console.WriteLine("❌ No mch_order_no provided.");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(Pay247TestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var method = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.Pay247)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            if (method == null)
            {
                Console.WriteLine("❌ No active Pay247 payment method row found.");
                return;
            }

            var options = Pay247Options.FromJson(method.Configuration);
            options.TenantId = Pay247TestTenantId;

            var envelope = await Pay247.QueryPayinAsync(
                options, orderNo, _httpClientFactory.CreateClient(), _logger);

            if (envelope == null)
            {
                Console.WriteLine("❌ No / unparseable response.");
                return;
            }

            Console.WriteLine($"   code    = {envelope.Code}");
            Console.WriteLine($"   message = {envelope.Message}");
            Console.WriteLine($"   status  = {(string?)envelope.Data?["status"]}");
            Console.WriteLine($"   amount  = {(string?)envelope.Data?["amount"]}");
            Console.WriteLine($"   actual  = {(string?)envelope.Data?["actual_amount"]}");
            Console.WriteLine($"   data    = {envelope.Data?.ToString(Formatting.None)}");

            Console.WriteLine("✅ Pay247 Query Order Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestPay247QueryOrder failed");
        }
    }

    /// <summary>Lists every wallet in the Pay247 balance response (multi-currency by design).</summary>
    private async Task TestPay247Balance()
    {
        Console.WriteLine("🧪 Testing Pay247 Balance...");
        Console.WriteLine();

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(Pay247TestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var method = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.Pay247)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            if (method == null)
            {
                Console.WriteLine("❌ No active Pay247 payment method row found.");
                return;
            }

            var options = Pay247Options.FromJson(method.Configuration);
            options.TenantId = Pay247TestTenantId;

            var envelope = await Pay247.QueryBalanceAsync(
                options, _httpClientFactory.CreateClient(), _logger);

            if (envelope == null)
            {
                Console.WriteLine("❌ No / unparseable response.");
                return;
            }

            Console.WriteLine($"   code    = {envelope.Code}");
            Console.WriteLine($"   message = {envelope.Message}");

            var wallets = envelope.Data?["wallets"] as Newtonsoft.Json.Linq.JArray;
            if (wallets == null || wallets.Count == 0)
            {
                Console.WriteLine($"   data = {envelope.Data?.ToString(Formatting.None)}");
            }
            else
            {
                Console.WriteLine($"   Wallets ({wallets.Count} currencies):");
                foreach (var item in wallets)
                {
                    Console.WriteLine($"     - {item["currency"]}: balance={item["balance"]}, freeze={item["freeze"]}");
                }
            }

            Console.WriteLine("✅ Pay247 Balance Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestPay247Balance failed");
        }
    }

    /// <summary>
    /// Iterates active Pay247 Withdrawal rows and fires one sandbox disbursement per row.
    /// The bank code is taken from the row's <see cref="Pay247Options.Banks"/>; if empty,
    /// falls back to <see cref="Pay247BankCodes"/>. For non-BANK rows (PHP MAYA) the
    /// "bank_code" equals the method name.
    /// </summary>
    private async Task TestPay247CreatePayout()
    {
        Console.WriteLine("🧪 Testing Pay247 Create Payout Order...");
        Console.WriteLine();

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(Pay247TestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var methods = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.Pay247)
                .Where(x => x.MethodType == PaymentMethodTypes.Withdrawal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .ToListAsync();
            if (methods.Count == 0)
            {
                Console.WriteLine("❌ No active Pay247 Withdrawal payment method rows found.");
                return;
            }

            foreach (var method in methods)
            {
                var options = Pay247Options.FromJson(method.Configuration);
                options.TenantId = Pay247TestTenantId;

                var (isValid, err) = options.Validate();
                if (!isValid)
                {
                    Console.WriteLine($"⚠ MethodId={method.Id}: invalid config — {err}");
                    continue;
                }

                string bankCode, bankName;
                if (!string.Equals(options.PayMethod, "BANK", StringComparison.OrdinalIgnoreCase))
                {
                    // For non-BANK methods (PHP MAYA payout etc.) Pay247 expects bank_code == pay_method.
                    bankCode = options.PayMethod;
                    bankName = options.PayMethod;
                }
                else
                {
                    var banks = options.GetBanksForCurrencyOrSeed(options.Currency);
                    var fromConfig = options.GetBanksForCurrency(options.Currency).Length > 0;
                    if (banks.Count > 0)
                    {
                        var first = banks[0];
                        bankCode = first.Code;
                        bankName = first.Name;
                        Console.WriteLine($"   Bank source: {(fromConfig ? "Configuration.Banks" : "Pay247BankCodes seed")}");
                    }
                    else
                    {
                        Console.Write($"   No banks configured for {options.Currency}. Enter bank_code: ");
                        bankCode = (Console.ReadLine() ?? string.Empty).Trim();
                        Console.Write("   Enter bank name: ");
                        bankName = (Console.ReadLine() ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(bankCode))
                        {
                            Console.WriteLine("⚠ Skipped — no bank_code supplied.");
                            continue;
                        }
                    }
                }

                Console.WriteLine($"📤 MethodId={method.Id}  Currency={options.Currency}  PayMethod={options.PayMethod}  Bank={bankCode} ({bankName})");

                var client = new Pay247.PayoutRequestClient
                {
                    PaymentNumber = Payment.GenerateNumber(),
                    Amount        = SuggestedTestAmount(options.Currency),
                    Currency      = options.Currency,
                    AccountName   = "TEST BENEFICIARY",
                    AccountNo     = "0001234567",
                    BankCode      = bankCode,
                    BankBranch    = string.Empty,
                    Options       = options,
                    Client        = _httpClientFactory.CreateClient(),
                    Logger        = _logger,
                };

                var response = await client.RequestAsync();
                Console.WriteLine($"   IsSuccess = {response.IsSuccess}");
                Console.WriteLine($"   Message   = {response.Message}");
                Console.WriteLine($"   Response  = {response.ResponseJson}");
                Console.WriteLine("-------------------------------------");
            }

            Console.WriteLine("✅ Pay247 Create Payout Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestPay247CreatePayout failed");
        }
    }

    /// <summary>Returns a sane default test amount per currency (above Pay247 minimums, below daily caps).</summary>
    private static decimal SuggestedTestAmount(string currency) => currency.ToUpperInvariant() switch
    {
        "VND" => 50_000m,    // ~2 USD
        "IDR" => 30_000m,    // ~2 USD
        "MYR" => 10m,        // ~2 USD
        "PHP" => 100m,       // ~2 USD
        "THB" => 100m,
        "JPY" => 500m,
        _     => 10m,
    };
    // ============================================================================================
    // Rivo
    //
    // Five test entry points (wired via CmdTestService switch):
    //   test-rivo-sig      → TestRivoSignature      (pure unit test; no creds required)
    //   test-rivo-create   → TestRivoCreateOrder    (iterates EVERY active Rivo row per currency)
    //   test-rivo-query    → TestRivoQueryOrder     (interactive; prompts for tradeNo)
    //   test-rivo-balance  → TestRivoBalance        (lists ALL currencies returned by the API)
    //   test-rivo-payout   → TestRivoCreatePayout   (iterates active rows; picks sample bank by currency)
    //
    // Rivo IP-whitelist gotcha: a 40304 response means our outbound IP isn't whitelisted yet.
    // ============================================================================================

    private const long RivoTestTenantId = 10000;

    /// <summary>
    /// Unit test of the SHA-512 signer using a synthetic payload. Asserts deterministic output and
    /// that empty/null/sign fields are stripped before sorting.
    /// </summary>
    private Task TestRivoSignature()
    {
        Console.WriteLine("🧪 Testing Rivo SHA-512 signer...");
        Console.WriteLine();

        const string secret = "TEST_SECRET_KEY";

        var spec = new Dictionary<string, object?>
        {
            ["mchId"]         = "MCH123456",
            ["tradeNo"]       = "mdm-ABCDEF1234",
            ["amount"]        = 12345.67m,
            ["currency"]      = "VND",
            ["paymentMethod"] = "01",
            ["clientIp"]      = "127.0.0.1",
            ["name"]          = "NGUYEN VAN A",
            ["phone"]         = "0900000000",
            ["email"]         = "test@example.com",
            ["city"]          = " ",
            ["address"]       = " ",
            ["notifyUrl"]     = "https://example.com/cb",
            ["returnUrl"]     = "",
            ["subject"]       = "",
            ["emptyShouldBeStripped"] = "",
            ["nullShouldBeStripped"]  = null,
        };
        // Deterministic stamping for repeatable tests.
        spec["signType"]  = "SHA512";
        spec["version"]   = "1.1";
        spec["timestamp"] = 1700000000000L;
        spec["nonce"]     = "abcd1234abcd1234";

        var sig1 = Rivo.GenerateSignature(spec, secret, _logger);
        var sig2 = Rivo.GenerateSignature(spec, secret, _logger);

        Console.WriteLine($"   Computed: {sig1}");
        Console.WriteLine($"   Repeated: {sig2}");
        Console.WriteLine($"   Length:   {sig1.Length} (expected 128 for SHA-512 hex)");
        Console.WriteLine($"   Deterministic: {(sig1 == sig2 ? "✅ YES" : "❌ NO")}");

        // Sanity: an unrelated tweak must change the signature.
        spec["amount"] = 12345.68m;
        var sig3 = Rivo.GenerateSignature(spec, secret, _logger);
        Console.WriteLine($"   Tweaked:  {sig3}");
        Console.WriteLine($"   Differs:  {(sig3 != sig1 ? "✅ YES" : "❌ NO")}");

        Console.WriteLine();
        Console.WriteLine("=====================================");
        Console.WriteLine("✅ Rivo Signature Test Complete!");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Iterates every active Rivo <c>PaymentMethod</c> row for the test tenant and fires one
    /// create-collection-order per row. Currency and PaymentMethod are read from each row's
    /// <c>Configuration</c> — never hardcoded — so this naturally exercises VND today and
    /// THB/IDR/etc. once they're added.
    /// </summary>
    private async Task TestRivoCreateOrder()
    {
        Console.WriteLine("🧪 Testing Rivo Create Collection Order...");
        Console.WriteLine();

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(RivoTestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var methods = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.Rivo)
                .Where(x => x.MethodType == PaymentMethodTypes.Deposit)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .ToListAsync();

            if (methods.Count == 0)
            {
                Console.WriteLine("❌ No active Rivo Deposit payment method rows for tenant {0}.", RivoTestTenantId);
                Console.WriteLine("   Seed one via the back-office before running this test.");
                return;
            }

            Console.WriteLine($"Found {methods.Count} active Rivo Deposit row(s); creating one test order per row.");
            Console.WriteLine("=====================================");

            foreach (var method in methods)
            {
                var options = RivoOptions.FromJson(method.Configuration);
                options.TenantId = RivoTestTenantId;

                var (isValid, err) = options.Validate();
                if (!isValid)
                {
                    Console.WriteLine($"⚠ MethodId={method.Id}: invalid config — {err}");
                    continue;
                }

                Console.WriteLine($"📤 MethodId={method.Id}  Currency={options.Currency}  PaymentMethod={options.PaymentMethod}");

                var client = new Rivo.RequestClient
                {
                    PaymentNumber = Payment.GenerateNumber(),
                    Amount        = options.Currency.Equals("VND", StringComparison.OrdinalIgnoreCase) ? 50_000m : 100m,
                    Currency      = options.Currency,
                    PaymentMethod = options.PaymentMethod,
                    PayerName     = "TEST PAYER",
                    PayerPhone    = "0900000000",
                    PayerEmail    = "test@example.com",
                    ClientIp      = "127.0.0.1",
                    City          = "Ho Chi Minh",
                    Address       = "District 1",
                    ReturnUrl     = "https://example.com/return",
                    Subject       = $"Rivo test {options.Currency}",
                    Options       = options,
                    Client        = _httpClientFactory.CreateClient(),
                    Logger        = _logger,
                };

                var response = await client.RequestAsync();
                Console.WriteLine($"   IsSuccess     = {response.IsSuccess}");
                Console.WriteLine($"   Action        = {response.Action}");
                Console.WriteLine($"   PaymentNumber = {response.PaymentNumber}");
                Console.WriteLine($"   Reference     = {response.Reference}");
                Console.WriteLine($"   RedirectUrl   = {response.RedirectUrl}");
                Console.WriteLine($"   QrCode        = {(string.IsNullOrEmpty(response.TextForQrCode) ? "(none)" : response.TextForQrCode[..Math.Min(60, response.TextForQrCode.Length)] + "...")}");
                if (!response.IsSuccess)
                {
                    Console.WriteLine($"   ⚠ Message    = {response.Message}");
                    Console.WriteLine("   Hint: 40304 == outbound IP not whitelisted by Rivo merchant ops.");
                }
                Console.WriteLine("-------------------------------------");
            }

            Console.WriteLine("✅ Rivo Create Order Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            _logger.LogError(ex, "TestRivoCreateOrder failed");
        }
    }

    /// <summary>Prompts for a <c>tradeNo</c> and queries the Rivo collection-order status endpoint.</summary>
    private async Task TestRivoQueryOrder()
    {
        Console.WriteLine("🧪 Testing Rivo Query Collection Order...");
        //Console.Write("Enter tradeNo (the PaymentNumber we sent to Rivo): ");
        var tradeNo = "pm-26220ba79222"; // (Console.ReadLine() ?? string.Empty).Trim();
        //Console.Write("Enter outTradeNo (the platform order number): ");
        var outTradeNo = "PAY2605282059870285133434880";  //(Console.ReadLine() ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(tradeNo))
        {
            Console.WriteLine("❌ No tradeNo provided.");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(RivoTestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var method = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.Rivo)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            if (method == null)
            {
                Console.WriteLine("❌ No active Rivo payment method row found.");
                return;
            }

            var options = RivoOptions.FromJson(method.Configuration);
            options.TenantId = RivoTestTenantId;

            var envelope = await Rivo.QueryCollectionOrderAsync(
                options, tradeNo, outTradeNo, _httpClientFactory.CreateClient(), _logger);

            if (envelope == null)
            {
                Console.WriteLine("❌ No / unparseable response.");
                return;
            }

            Console.WriteLine($"   code = {envelope.Code}");
            Console.WriteLine($"   msg  = {envelope.Msg}");
            Console.WriteLine($"   payStatus = {(int?)envelope.Data?["payStatus"]}");
            Console.WriteLine($"   payTime   = {(string?)envelope.Data?["payTime"]}");
            Console.WriteLine($"   data      = {envelope.Data?.ToString(Formatting.None)}");

            Console.WriteLine("✅ Rivo Query Order Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestRivoQueryOrder failed");
        }
    }

    /// <summary>Lists every currency in the Rivo balance response (multi-currency by design).</summary>
    private async Task TestRivoBalance()
    {
        Console.WriteLine("🧪 Testing Rivo Balance...");
        Console.WriteLine();

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(RivoTestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var method = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.Rivo)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            if (method == null)
            {
                Console.WriteLine("❌ No active Rivo payment method row found.");
                return;
            }

            var options = RivoOptions.FromJson(method.Configuration);
            options.TenantId = RivoTestTenantId;

            var envelope = await Rivo.QueryBalanceAsync(
                options, currencyFilter: null, _httpClientFactory.CreateClient(), _logger);

            if (envelope == null)
            {
                Console.WriteLine("❌ No / unparseable response.");
                return;
            }

            Console.WriteLine($"   code = {envelope.Code}");
            Console.WriteLine($"   msg  = {envelope.Msg}");

            var balances = envelope.Data?["balances"] as Newtonsoft.Json.Linq.JArray;
            if (balances == null || balances.Count == 0)
            {
                // Some accounts return a single currency at the data root.
                Console.WriteLine($"   data = {envelope.Data?.ToString(Formatting.None)}");
            }
            else
            {
                Console.WriteLine($"   Balances ({balances.Count} currencies):");
                foreach (var item in balances)
                {
                    Console.WriteLine($"     - {item["currency"]}: balance={item["balance"]}, toSettle={item["toSettle"]}, frozen={item["frozen"]}");
                }
            }

            Console.WriteLine("✅ Rivo Balance Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestRivoBalance failed");
        }
    }

    /// <summary>
    /// Iterates active Rivo rows and fires one sandbox disbursement per row. The bank code is taken
    /// from <see cref="RivoBankCodes"/> for that currency; if the currency has no published table
    /// yet, the test falls back to a CLI prompt.
    /// </summary>
    private async Task TestRivoCreatePayout()
    {
        Console.WriteLine("🧪 Testing Rivo Create Disbursement Order...");
        Console.WriteLine();

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(RivoTestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var methods = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.Rivo)
                .Where(x => x.MethodType == PaymentMethodTypes.Withdrawal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .ToListAsync();
            if (methods.Count == 0)
            {
                Console.WriteLine("❌ No active Rivo Withdrawal payment method rows found.");
                return;
            }

            foreach (var method in methods)
            {
                var options = RivoOptions.FromJson(method.Configuration);
                options.TenantId = RivoTestTenantId;

                var (isValid, err) = options.Validate();
                if (!isValid)
                {
                    Console.WriteLine($"⚠ MethodId={method.Id}: invalid config — {err}");
                    continue;
                }

                // Prefer the per-row override (Configuration.Banks) — that's how the back-office
                // bank dropdown gets its list at runtime. Fall back to the code-side Appendix C
                // seed only when the row hasn't been populated yet.
                var banks = options.GetBanksForCurrencyOrSeed(options.Currency);
                var fromConfig = options.GetBanksForCurrency(options.Currency).Length > 0;

                string bankCode, bankName;
                if (banks.Count > 0)
                {
                    var first = banks[0];
                    bankCode = first.Code;
                    bankName = first.Name;
                }
                else
                {
                    Console.Write($"   No banks configured for {options.Currency} (Configuration.Banks empty, no seed). Enter bankCode: ");
                    bankCode = (Console.ReadLine() ?? string.Empty).Trim();
                    Console.Write("   Enter bankName: ");
                    bankName = (Console.ReadLine() ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(bankCode))
                    {
                        Console.WriteLine("⚠ Skipped — no bankCode supplied.");
                        continue;
                    }
                }

                Console.WriteLine($"📤 MethodId={method.Id}  Currency={options.Currency}  Bank={bankCode} ({bankName})  Source={(fromConfig ? "Configuration.Banks" : "RivoBankCodes seed")}");

                var client = new Rivo.WithdrawalRequestClient
                {
                    PaymentNumber    = Payment.GenerateNumber(),
                    Amount           = options.Currency.Equals("VND", StringComparison.OrdinalIgnoreCase) ? 50_000m : 100m,
                    Currency         = options.Currency,
                    AccountName      = "TEST BENEFICIARY",
                    AccountNoUnified = "0001234567",
                    BankCode         = bankCode,
                    BankName         = bankName,
                    Purpose          = "Rivo sandbox test",
                    Options          = options,
                    Client           = _httpClientFactory.CreateClient(),
                    Logger           = _logger,
                };

                var response = await client.RequestAsync();
                Console.WriteLine($"   IsSuccess = {response.IsSuccess}");
                Console.WriteLine($"   Message   = {response.Message}");
                Console.WriteLine($"   Response  = {response.ResponseJson}");
                Console.WriteLine("-------------------------------------");
            }

            Console.WriteLine("✅ Rivo Create Payout Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestRivoCreatePayout failed");
        }
    }

    // ============================================================================================
    // 12Group (thesolix.com) — Thailand THB gateway
    //
    // Three test entry points (wired via CmdTestService switch):
    //   test-12group-create → TestTwelveGroupCreateOrder    (iterates active 12Group Deposit rows)
    //   test-12group-query  → TestTwelveGroupInquiryDeposit  (inquiry-deposit by trans_id)
    //   test-12group-payout → TestTwelveGroupCreatePayout    (iterates active 12Group Withdrawal rows)
    //
    // No -sig test: 12Group has no request/callback signature (static-JWT header auth only).
    // Remember: real money is required even in the test environment.
    // ============================================================================================

    private const long TwelveGroupTestTenantId = 10000;

    /// <summary>Iterates every active 12Group Deposit row for the test tenant and fires one create-qr-code per row.</summary>
    private async Task TestTwelveGroupCreateOrder()
    {
        Console.WriteLine("🧪 Testing 12Group Create QR Code...");
        Console.WriteLine();

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(TwelveGroupTestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var methods = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.TwelveGroup)
                .Where(x => x.MethodType == PaymentMethodTypes.Deposit)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .ToListAsync();

            if (methods.Count == 0)
            {
                Console.WriteLine("❌ No active 12Group Deposit payment method rows for tenant {0}.", TwelveGroupTestTenantId);
                Console.WriteLine("   Seed one via twelvegroup_paymentmethod_seed.sql before running this test.");
                return;
            }

            Console.WriteLine($"Found {methods.Count} active 12Group Deposit row(s); creating one test QR per row.");
            Console.WriteLine("=====================================");

            foreach (var method in methods)
            {
                var options = TwelveGroupOptions.FromJson(method.Configuration);
                options.TenantId = TwelveGroupTestTenantId;

                var (isValid, err) = options.Validate();
                if (!isValid)
                {
                    Console.WriteLine($"⚠ MethodId={method.Id}: invalid config — {err}");
                    continue;
                }

                var paymentNumber = Payment.GenerateNumber();
                var ref1 = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{Random.Shared.Next(1000, 9999)}";

                Console.WriteLine($"📤 MethodId={method.Id}  Env={options.Env}  Currency={options.Currency}  ref1={ref1}");

                var client = new TwelveGroup.RequestClient
                {
                    PaymentNumber = paymentNumber,
                    Amount        = 300m, // vendor min 300 THB/transaction
                    Ref1          = ref1,
                    Ref3          = "Somchai Rakdee",
                    Ref4          = paymentNumber,
                    MobileNo      = "0000000000",
                    Options       = options,
                    Client        = _httpClientFactory.CreateClient(),
                    Logger        = _logger,
                };

                var response = await client.RequestAsync();
                Console.WriteLine($"   IsSuccess     = {response.IsSuccess}");
                Console.WriteLine($"   Action        = {response.Action}");
                Console.WriteLine($"   PaymentNumber = {response.PaymentNumber}");
                Console.WriteLine($"   trans_id      = {response.Reference}");
                Console.WriteLine($"   RedirectUrl   = {response.RedirectUrl}");
                if (!response.IsSuccess)
                    Console.WriteLine($"   ⚠ Message    = {response.Message}");
                Console.WriteLine("-------------------------------------");
            }

            Console.WriteLine("✅ 12Group Create QR Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            _logger.LogError(ex, "TestTwelveGroupCreateOrder failed");
        }
    }

    /// <summary>Calls inquiry-deposit for a given trans_id and prints status / aml_status / aml_check.</summary>
    private async Task TestTwelveGroupInquiryDeposit()
    {
        Console.WriteLine("🧪 Testing 12Group inquiry-deposit...");
        // Replace with a real trans_id returned by test-12group-create.
        var transId = "a6743daf9af8d4703558cee1c9316b4d";
        if (string.IsNullOrWhiteSpace(transId) || transId == "REPLACE_WITH_TRANS_ID")
        {
            Console.WriteLine("❌ Set transId in TestTwelveGroupInquiryDeposit to a real trans_id first.");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(TwelveGroupTestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var method = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.TwelveGroup)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .FirstOrDefaultAsync();
            if (method == null)
            {
                Console.WriteLine("❌ No active 12Group payment method row found.");
                return;
            }

            var options = TwelveGroupOptions.FromJson(method.Configuration);
            options.TenantId = TwelveGroupTestTenantId;

            var data = await TwelveGroup.InquiryDepositAsync(
                options, transId, _httpClientFactory.CreateClient(), _logger);

            if (data == null)
            {
                Console.WriteLine("❌ No / unparseable response (or data not found).");
                return;
            }

            Console.WriteLine($"   status      = {(string?)data["status"]}");
            Console.WriteLine($"   aml_status  = {(string?)data["aml_status"]}");
            Console.WriteLine($"   aml_check   = {(string?)data["aml_check"]}");
            Console.WriteLine($"   amount      = {(string?)data["amount"]}");
            Console.WriteLine($"   ref4        = {(string?)data["ref4"]}");
            Console.WriteLine("✅ 12Group Inquiry Deposit Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestTwelveGroupInquiryDeposit failed");
        }
    }

    /// <summary>Iterates active 12Group Withdrawal rows and fires one transfer-out per row (sample bank from the seed/config).</summary>
    private async Task TestTwelveGroupCreatePayout()
    {
        Console.WriteLine("🧪 Testing 12Group Transfer Out...");
        Console.WriteLine();

        try
        {
            using var scope = _serviceProvider.CreateTenantScope(TwelveGroupTestTenantId);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var methods = await ctx.PaymentMethods
                .Where(x => x.Platform == (int)PaymentPlatformTypes.TwelveGroup)
                .Where(x => x.MethodType == PaymentMethodTypes.Withdrawal)
                .Where(x => x.Status == (short)PaymentMethodStatusTypes.Active)
                .Where(x => x.DeletedOn == null)
                .ToListAsync();
            if (methods.Count == 0)
            {
                Console.WriteLine("❌ No active 12Group Withdrawal payment method rows found.");
                return;
            }

            foreach (var method in methods)
            {
                var options = TwelveGroupOptions.FromJson(method.Configuration);
                options.TenantId = TwelveGroupTestTenantId;

                var (isValid, err) = options.Validate();
                if (!isValid)
                {
                    Console.WriteLine($"⚠ MethodId={method.Id}: invalid config — {err}");
                    continue;
                }

                var banks = options.GetBanksForCurrencyOrSeed(options.Currency);
                if (banks.Count == 0)
                {
                    Console.WriteLine($"⚠ MethodId={method.Id}: no banks configured/seeded for {options.Currency}; skipping.");
                    continue;
                }

                var bank = banks[0];
                Console.WriteLine($"📤 MethodId={method.Id}  Env={options.Env}  Bank={bank.Code} ({bank.Name})");

                var client = new TwelveGroup.PayoutRequestClient
                {
                    PaymentNumber = Payment.GenerateNumber(),
                    Amount        = 100m,
                    BankCode      = bank.Code,
                    BankNumber    = "9090542086",
                    BankName      = bank.Name,
                    AccountName   = "Tanach Lueangsongchai",
                    MobileNo      = options.DefaultMobileNo,
                    TransactionBy = $"Test_{options.PartnerCode}",
                    Options       = options,
                    Client        = _httpClientFactory.CreateClient(),
                    Logger        = _logger,
                };

                var response = await client.RequestAsync();
                Console.WriteLine($"   IsSuccess = {response.IsSuccess}");
                Console.WriteLine($"   Message   = {response.Message}");
                Console.WriteLine($"   Response  = {response.ResponseJson}");
                Console.WriteLine("-------------------------------------");
            }

            Console.WriteLine("✅ 12Group Transfer Out Test Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            _logger.LogError(ex, "TestTwelveGroupCreatePayout failed");
        }
    }
}
