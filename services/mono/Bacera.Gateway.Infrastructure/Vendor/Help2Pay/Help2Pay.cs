using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor.Help2Pay.Models;
using Bacera.Gateway.Web.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Help2Pay;

public class Help2Pay
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = Help2PayOptions.FromJson(config);
        var obj = new
        {
            currency = (int)CurrencyTypes.INR,
            frontUri = "https://www.google.com",
        };
        var (result, message) = EnsureRequest(obj);
        if (!result) return message;
        var request = Utils.JsonDeserializeDynamic(JsonConvert.SerializeObject(obj));

        var client = new RequestClient
        {
            Amount = 120000,
            AccountUid = 123456789,
            PaymentNumber = Payment.GenerateNumber("mdm-"),
            ReturnUrl = request.frontUri,
            Currency = request.currency,
            Ip = "47.241.6.29",
            Language = "en-us",
            Bank = "",
            Options = options,
            Logger = logger,
        };

        var response = await client.RequestAsync(true);
        return response;
    }

    public class RequestClient
    {
        // public string MerchantCode { get; set; } = null!;
        public CurrencyTypes Currency { get; set; } = CurrencyTypes.Invalid;
        public long AccountUid { get; set; }
        public string PaymentNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow.AddHours(8); // base on UTC+8 Hong Kong
        public string? Bank { get; set; }
        public string Language { get; set; } = LanguageTypes.English;
        public string Ip { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
        public Help2PayOptions Options { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;
        public HttpClient? Client { get; set; }

        // Payer fields — compulsory for certain currencies per spec p11
        public string? PayerAccountName { get; set; }
        public string? PayerAccountNumber { get; set; }
        public string? PayerAccountNameLocal { get; set; }
        public string? PayerPhoneNumber { get; set; }

        private string SignSignature()
            => Utils.Md5Hash($"{Options.MerchantCode}" +
                             $"{PaymentNumber}" +
                             $"{AccountUid}" +
                             $"{Amount:0.00}" +
                             $"{Enum.GetName(Currency)}" +
                             $"{CreatedOn:yyyyMMddHHmmss}" +
                             $"{Options.SecurityCode}" +
                             $"{Ip}");


        public Dictionary<string, string> BuildForm()
        {
            var form = new Dictionary<string, string>
            {
                { "Merchant", Options.MerchantCode },
                { "Currency", Enum.GetName(Currency)! },
                { "Customer", AccountUid.ToString() },
                { "Reference", PaymentNumber },
                { "Amount", Amount.ToString("0.00") },
                // Spec requires 12-hour hh:mm:sstt (e.g. 02:31:42PM); hash uses yyyyMMddHHmmss separately
                { "Datetime", CreatedOn.ToString("yyyy-MM-dd hh:mm:sstt") },
                { "FrontURI", ReturnUrl },
                { "BackURI", Options.CallbackUri },
                { "Bank", Bank ?? "" },
                { "Language", Language },
                { "ClientIP", Ip },
            };

            if (!string.IsNullOrEmpty(PayerAccountName))
                form["PayerAccountName"] = PayerAccountName;
            if (!string.IsNullOrEmpty(PayerAccountNumber))
                form["PayerAccountNumber"] = PayerAccountNumber;
            if (!string.IsNullOrEmpty(PayerAccountNameLocal))
                form["PayerAccountNameLocal"] = PayerAccountNameLocal;
            if (!string.IsNullOrEmpty(PayerPhoneNumber))
                form["PayerPhoneNumber"] = PayerPhoneNumber;

            return form;
        }

        // Per-currency compulsory-field + amount-decimal validation per spec p09/p10/p11.
        // Returns sentinel codes so the FE can localise the message.
        public (bool ok, string code) Validate()
        {
            var currencyCode = Enum.GetName(Currency) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Bank))
                return (false, "__HELP2PAY_BANK_REQUIRED__");

            // Bank whitelist — must be in this PaymentMethod row's configured Banks for this currency.
            // Skipped only if Options.Banks is unpopulated (legacy rows pre-multi-row split).
            if (Options.Banks.Count > 0 && !Options.IsBankSupported(currencyCode, Bank!))
                return (false, "__HELP2PAY_BANK_NOT_SUPPORTED__");

            // PayerAccountName — compulsory for MYR, IDR, KRW, CNY, THB
            if ((Currency is CurrencyTypes.MYR or CurrencyTypes.IDR or CurrencyTypes.KRW
                    or CurrencyTypes.CNY or CurrencyTypes.THB)
                && string.IsNullOrWhiteSpace(PayerAccountName))
                return (false, "__HELP2PAY_PAYER_NAME_REQUIRED__");

            // CNY PayerAccountName must be Chinese (CJK Unified Ideographs)
            if (Currency == CurrencyTypes.CNY
                && !System.Text.RegularExpressions.Regex.IsMatch(
                    PayerAccountName ?? string.Empty,
                    @"^[\p{IsCJKUnifiedIdeographs}\p{IsCJKUnifiedIdeographsExtensionA}]+$"))
                return (false, "__HELP2PAY_PAYER_NAME_MUST_BE_CHINESE__");

            // PayerAccountNumber — compulsory for KRW, BRL, PHP, THB
            if ((Currency is CurrencyTypes.KRW or CurrencyTypes.BRL
                    or CurrencyTypes.PHP or CurrencyTypes.THB)
                && string.IsNullOrWhiteSpace(PayerAccountNumber))
                return (false, "__HELP2PAY_PAYER_NUMBER_REQUIRED__");

            // PayerAccountNameLocal — compulsory for THB
            if (Currency == CurrencyTypes.THB
                && string.IsNullOrWhiteSpace(PayerAccountNameLocal))
                return (false, "__HELP2PAY_PAYER_NAME_LOCAL_REQUIRED__");

            // PayerPhoneNumber — compulsory for INR
            if (Currency == CurrencyTypes.INR
                && string.IsNullOrWhiteSpace(PayerPhoneNumber))
                return (false, "__HELP2PAY_PAYER_PHONE_REQUIRED__");

            // Amount decimal restrictions per spec p09:
            //  - Always whole: VND, IDR, CNY, KRW, INR
            //  - THB only when Bank == PPTP (QR Payment / PromptPay)
            //  - PHP only when Bank in (PAYMAYA, GCASH) — eWallet (Native) transfer types
            //    (also accept legacy "MAYA" in case data uses the spec-doc naming)
            var mustBeWhole =
                Currency is CurrencyTypes.VND or CurrencyTypes.IDR or CurrencyTypes.CNY
                    or CurrencyTypes.KRW or CurrencyTypes.INR
                || (Currency == CurrencyTypes.THB && Bank == "PPTP")
                || (Currency == CurrencyTypes.PHP
                    && (Bank == "PAYMAYA" || Bank == "GCASH" || Bank == "MAYA"));
            if (mustBeWhole && Amount % 1m != 0m)
                return (false, "__HELP2PAY_AMOUNT_DECIMAL_NOT_ALLOWED__");

            return (true, string.Empty);
        }

        public async Task<ResponseModel> SendAsync()
        {
            await Task.Delay(0);
            var form = BuildForm();
            Logger.LogInformation("Help2Pay Request: {form}", form);
            var key = SignSignature();
            Logger.LogInformation("Help2Pay Key: {key}", key);
            form.Add("Key", key);
            // var content = new FormUrlEncodedContent(form);
            // using var client = new HttpClient();
            // var response = await client.PostAsync(Options.EndPoint, content);
            // var body = await response.Content.ReadAsStringAsync();
            // return !response.IsSuccessStatusCode
            //     ? new ResponseModel { IsSuccess = false }
            //     : ResponseModel.FromJson(Options.EndPoint, form, body);
            return ResponseModel.FromJson(Options.EndPoint, form, "");
        }

        public async Task<DepositCreatedResponseModel> RequestAsync(bool isTest = false)
        {
            await Task.Delay(0);

            if (!isTest)
            {
                var (ok, code) = Validate();
                if (!ok)
                {
                    Logger.LogWarning("Help2Pay Request rejected: {code} (Currency={currency}, Bank={bank})",
                        code, Currency, Bank);
                    return DepositCreatedResponseModel.Fail(code, showMessage: true);
                }
            }

            var form = BuildForm();
            Logger.LogInformation("Help2Pay Request: {form}", form);
            var key = SignSignature();
            Logger.LogInformation("Help2Pay Key: {key}", key);
            form.Add("Key", key);
            if (isTest)
            {
                var content = new FormUrlEncodedContent(form);
                var response = await Client!.PostAsync(Options.EndPoint, content);
                var body = await response.Content.ReadAsStringAsync();
            }
            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Form = form,
                Action = PaymentResponseActionTypes.Post,
                EndPoint = Options.EndPoint,
                PaymentNumber = PaymentNumber
            };
        }
    }

    public sealed class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public Dictionary<string, string> Form { get; set; } = new();
        public string RedirectUrl { get; set; } = null!;

        public static ResponseModel FromJson(string endPoint, Dictionary<string, string> form, string body)
            => new()
            {
                IsSuccess = true,
                RedirectUrl = endPoint,
                Form = form,
            };
    }

    public static (bool, string) ValidateCallbackSpec(Dictionary<string, string> spec, string securityCode)
    {
        var requiredKeys = new[] { "Merchant", "Reference", "Customer", "Amount", "Currency", "Status", "Key" };
        var missedKeys = requiredKeys.Where(x => !spec.ContainsKey(x)).ToList();
        if (missedKeys.Count != 0) return (false, $"Missing required keys: {string.Join(", ", missedKeys)}");

        // Callback key formula per spec p15: Merchant+Reference+Customer+Amount+Currency+Status+SecurityCode
        var baseString = spec["Merchant"] + spec["Reference"] + spec["Customer"] + spec["Amount"] + spec["Currency"] + spec["Status"] + securityCode;
        var calculatedKey = Utils.Md5Hash(baseString).ToUpper();
        var result = string.Equals(calculatedKey, spec["Key"], StringComparison.OrdinalIgnoreCase);
        if (!result)
        {
            BcrLog.Slack($"Help2Pay Callback Validate Signature Failed: {JsonConvert.SerializeObject(spec)}");
        }

        return (result, result ? string.Empty : "Invalid key");
    }

    public static (bool, string) EnsureRequest(object obj)
    {
        var request = Utils.JsonDeserializeDynamic(JsonConvert.SerializeObject(obj));
        if (request == null) return (false, "Invalid request object");

        var currency = request.currency != null ? (CurrencyTypes)request.currency : CurrencyTypes.Invalid;
        if (currency == CurrencyTypes.Invalid) return (false, "Invalid currency type");

        if (request.frontUri == null && request.returnUrl == null) return (false, "Invalid returnUrl or frontUri");
        return (true, string.Empty);
    }

    public sealed class RequestSupplement
    {
        [Required] public CurrencyTypes Currency { get; set; }
        [Required] public string FrontUri { get; set; } = null!;
        [Required] public string ReturnUrl { get; set; } = null!;

        public string GetReturnUrl() => string.IsNullOrEmpty(ReturnUrl) ? FrontUri : ReturnUrl;

        public string? GetBank()
        {
            if (!QrCode) return null;
            return Currency switch
            {
                CurrencyTypes.MYR => "5",
                CurrencyTypes.THB => "1",
                CurrencyTypes.VND => "4",
                CurrencyTypes.IDR => "6",
                CurrencyTypes.PHP => "7",
                CurrencyTypes.INR => "2",
                _ => null
            };
        }

        public string ValidationMessage { get; set; } = string.Empty;
        public bool QrCode { get; set; } = false;

        public static bool TryParse(string json, out RequestSupplement request)
        {
            request = new RequestSupplement();
            try
            {
                request = JsonConvert.DeserializeObject<RequestSupplement>(json)!;
                return true;
            }
            catch (Exception e)
            {
                request.ValidationMessage = e.Message;
                return false;
            }
        }
    }
}
