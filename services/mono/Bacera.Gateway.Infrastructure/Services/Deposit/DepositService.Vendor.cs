using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor;
using Bacera.Gateway.Vendor.Bakong;
using Bacera.Gateway.Vendor.BigPay;
using Bacera.Gateway.Vendor.BigPay.Models;
using Bacera.Gateway.Vendor.BipiPay;
using Bacera.Gateway.Vendor.Buy365;
using Bacera.Gateway.Vendor.Buzipay;
using Bacera.Gateway.Vendor.ChinaPay;
using Bacera.Gateway.Vendor.ChipPay;
using Bacera.Gateway.Vendor.EuPayment;
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
using Bacera.Gateway.Vendor.Pay247;
using Bacera.Gateway.Vendor.PaymentAsia;
using Bacera.Gateway.Vendor.PayPal;
using Bacera.Gateway.Vendor.Poli;
using Bacera.Gateway.Vendor.Poli.Models;
using Bacera.Gateway.Vendor.UnionePay;
using Bacera.Gateway.Vendor.UsePay;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Bacera.Gateway.Services;

public partial class DepositService
{
    private async Task<DepositCreatedResponseModel> ProcessChinaPayPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = ChinaPayOptions.FromJson(method.Configuration);
        var user = await userSvc.GetPartyAsync(account.PartyId);

        var client = new ChinaPay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            AccountUid = account.Uid,
            PaymentNumber = Payment.GenerateNumber(),
            NativeName = user.GuessNativeName(),
            PhoneNumber = user.PhoneNumberRaw,
            Options = options,
            Client = clientFactory.CreateClient(),
            Logger = logger,
        };

        var response = await client.RequestAsync();
        return response;
    }
    private async Task<DepositCreatedResponseModel> ProcessEuPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = EuPayOptions.FromJson(method.Configuration);
        var user = await userSvc.GetPartyAsync(account.PartyId);

        if (!EuPayRequestModel.TryParse(JsonConvert.SerializeObject(request), out var requestModel))
            return DepositCreatedResponseModel.Fail("Invalid parameters to create EuPayment");

        await cfgSvc.SetAsync(nameof(Party), user.PartyId, ConfigKeys.EuPayDesensitizedRequestData,
            requestModel.GetDesensitizedData());

        var client = new EuPay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            Email = user.EmailRaw,
            Ip = request.GetValueOrDefault("ip") as string ?? string.Empty,
            Request = requestModel,
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            CurrencyId = (CurrencyTypes)account.CurrencyId,
            Options = options,
            Logger = logger,
            Client = clientFactory.CreateClient()
        };

        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessBakongAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = BakongOptions.FromJson(method.Configuration);
        var user = await userSvc.GetPartyAsync(account.PartyId);
        var targetCurrency = request.TryGetValue("currencyId", out var valRaw) && int.TryParse(valRaw, out var val)
            ? (CurrencyTypes)val
            : (CurrencyTypes)method.CurrencyId;
        var client = new Bakong.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            Email = user.EmailRaw!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CurrencyId = targetCurrency,
            Phone = user.PhoneNumberRaw,
            IpAddress = request.GetValueOrDefault("ip") ?? string.Empty,
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            Options = options,
            Logger = logger,
        };

        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessOFAPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = OFAPayOptions.FromJson(method.Configuration);
        var user = await userSvc.GetPartyAsync(account.PartyId);

        var client = new OFAPay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PartyUid = user.Uid,
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            NativeName = user.GuessNativeName(),
            Options = options,
            Logger = logger,
            Client = clientFactory.CreateClient()
        };

        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessMonetixAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = MonetixOptions.FromJson(method.Configuration);
        var user = await userSvc.GetPartyAsync(account.PartyId);
        var targetCurrency = request.TryGetValue("currencyId", out var valRaw) && int.TryParse(valRaw, out var val)
            ? (CurrencyTypes)val
            : (CurrencyTypes)method.CurrencyId;

        var client = new Monetix.RequestClient
        {
            Amount = exchangedAmount,
            PaymentNumber = Payment.GenerateNumber(),
            AccountUid = account.Uid,
            Email = user.EmailRaw,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CurrencyId = targetCurrency,
            Client = clientFactory.CreateClient(),
            Options = options
        };

        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessCryptoAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var crypto = await cryptoSvc.GetRandomCryptoForPaymentAsync();
        if (crypto == null) return DepositCreatedResponseModel.Fail("No available crypto");

        var setting = await cfgSvc.GetAsync<Crypto.Setting>(nameof(Public), 0, ConfigKeys.CryptoSetting);
        var response = new DepositCreatedResponseModel
        {
            IsSuccess = true,
            TextForQrCode = crypto.Address,
            Action = PaymentResponseActionTypes.QrCode,
            PaymentNumber = Payment.GenerateNumber(),
            Message = setting!.PayExpiredTimeInMinutes.ToString(),
            CreatedCbHandler = async deposit =>
            {
                crypto.InUsePaymentId = deposit.PaymentId;
                crypto.Status = (int)CryptoStatusTypes.InUse;
                crypto.UpdatedOn = DateTime.UtcNow;
                deposit.Payment.ReferenceNumber = crypto.Name;
                tenantCtx.Cryptos.Update(crypto);
                await tenantCtx.SaveChangesAsync();

                var hasValidCrypto = await tenantCtx.Cryptos.AnyAsync(x => x.Status == (int)CryptoStatusTypes.Idle);
                if (!hasValidCrypto)
                {
                    const string message = "!!!All Crypto Addresses in use!!!";
                    var notice = MessagePopupDTO.BuildWarning("Crypto", message);
                    await messageSvc.SendPopupToManagerAsync(_tenantId, notice);
                }

                var key = CacheKeys.CryptoWalletKey(crypto.Address);
                var expire = setting.GetPayExpiredTimeInMinutes();
                await cache.SetStringAsync(key, "1", TimeSpan.FromMinutes(expire));
            }
        };

        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessPayPalAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = PayPalOptions.FromJson(method.Configuration);
        var client = new PayPal.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            AccountUid = account.Uid,
            Options = options,
            CurrencyId = (CurrencyTypes)method.CurrencyId,
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            CancelUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            Logger = logger,
            Client = clientFactory.CreateClient()
        };
        var response = await client.RequestAsync();
        var responseModel = (PayPalResponseModel)response.Form!;
        response.CreatedCbHandler = async deposit =>
        {
            deposit.Payment.ReferenceNumber = responseModel.Id;
            tenantCtx.Payments.Update(deposit.Payment);
            await tenantCtx.SaveChangesAsync();
        };
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessWireAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        if (_tenantId == 1) return DepositCreatedResponseModel.Success();

        var prefix = method.Name.ToLower();
        if (!prefix.Contains("em") && !prefix.Contains("hd")) return DepositCreatedResponseModel.Success();

        var user = await userSvc.GetPartyAsync(account.PartyId);
        var instruction = await paymentMethodSvc.GetInstructionAsync(method, user.Language);
        
        var baseUrl = "https://thebcr.com/";
        Uri.TryCreate(request["baseUrl"], UriKind.Absolute, out var uri);
        if (uri != null) baseUrl = "https://" + uri.Host;
        
        var response = new DepositCreatedResponseModel
        {
            IsSuccess = true,
            Action = PaymentResponseActionTypes.Post,
            EndPoint = $"{baseUrl.TrimEnd('/')}/deposit-instruction",
            
            PaymentNumber = Payment.GenerateNumber(),
            Form = new
            {
                logo = method.Logo,
                language = user.Language,
                // logo = "/images/wallet/" + method.Group + ".png",
                instruction,
                paymentInfo = new
                {
                    amount = exchangedAmount,
                    currencyId = method.CurrencyId,
                    createdOn = DateTime.UtcNow,
                }
            }
        };
        return response;
    }

    private static async Task<DepositCreatedResponseModel> ProcessManualAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        // var requested = (ManualPaymentRequestModel)ManualPaymentRequestModel.FromDict(request);
        // var validationResult = await new ManualPaymentModelValidator().ValidateAsync(requested);
        // return !validationResult.IsValid
        //     ? DepositCreatedResponseModel.Fail("Invalid parameters")
        //     :
        await Task.Delay(0);
        return DepositCreatedResponseModel.Success();;
    }

    private async Task<DepositCreatedResponseModel> ProcessChipPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var option = ChipPayOptions.FromJson(method.Configuration);
        var client = new ChipPay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            CurrencyId = (CurrencyTypes)method.CurrencyId,
            Client = clientFactory.CreateClient(),
            Options = option
        };
        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessUnionePayXAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = UnionePayOptions.FromJson(method.Configuration);
        var user = await userSvc.GetPartyAsync(account.PartyId);
        var client = new UnionePay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            Options = options,
            IdNumber = user.IdNumber,
            NativeName = user.GuessNativeName()
        };
        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessExLinkAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = ExLinkOptions.FromJson(method.Configuration);
        var party = await userSvc.GetPartyAsync(account.PartyId);
        var client = new ExLink.RequestClient
        {
            UniqueCode = account.Uid.ToString(),
            PaymentNumber = Payment.GenerateNumber(),
            Amount = RoundUp(exchangedAmount / 100m),
            PayerName = party.GuessNativeName(),
            Client = clientFactory.CreateClient(),
            Options = options
        };

        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessExLinkCashierAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = ExLinkCashierOptions.FromJson(method.Configuration);
        
        var client = new ExLinkCashier.RequestClient
        {
            PaymentNumber = Payment.GenerateNumber(),
            Amount = RoundUp(exchangedAmount / 100m),
            Options = options,
            Client = clientFactory.CreateClient(),
            Logger = logger
        };

        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessNPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        // var user = await userSvc.GetPartyAsync(account.PartyId);
        var nativeName = request.GetValueOrDefault("nativeName");
        if (string.IsNullOrWhiteSpace(nativeName))
        {
            var user = await userSvc.GetPartyAsync(account.PartyId);
            nativeName = user.GuessNativeName();
        }
        var options = NPayOptions.FromJson(method.Configuration);
        var client = new NPay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            NativeName = nativeName,
            AccountUid = account.Uid,
            Options = options,
            Client = clientFactory.CreateClient(),
            Logger = logger
        };
        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessUsePayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = UsePayOptions.FromJson(method.Configuration);
        var client = new UsePay.RequestClient
        {
            TenantId = _tenantId,
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            Options = options,
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            Logger = logger
        };

        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessFivePayF2FAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = FivePayF2FOptions.FromJson(method.Configuration);
        var targetCurrencyId = request.TryGetValue("currencyId", out var valRaw) && int.TryParse(valRaw, out var val)
            ? (CurrencyTypes)val
            : (CurrencyTypes)method.CurrencyId;
        
        var client = new FivePayF2F.RequestClient
        {
            TenantId = _tenantId,
            AccountUid = account.Uid,
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            CurrencyId = targetCurrencyId,
            Options = options,
            Logger = logger
        };

        if (targetCurrencyId == CurrencyTypes.THB)
        {
            var user = await userSvc.GetPartyAsync(account.PartyId);
            client.NativeName = user.GuessNativeName();
        }
        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessFivePayVAAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = FivePayVAOptions.FromJson(method.Configuration);
        var targetCurrencyId = request.TryGetValue("currencyId", out var valRaw) && int.TryParse(valRaw, out var val)
            ? (CurrencyTypes)val
            : (CurrencyTypes)method.CurrencyId;
        var client = new FivePayVA.RequestClient
        {
            TenantId = _tenantId,
            AccountUid = account.Uid,
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            CurrencyId = targetCurrencyId,
            Options = options,
            Logger = logger
        };
        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessGPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var nativeName = request.GetValueOrDefault("nativeName") as string;
        if (string.IsNullOrWhiteSpace(nativeName))
        {
            var user = await userSvc.GetPartyAsync(account.PartyId);
            nativeName = user.GuessNativeName();
        }

        if (string.IsNullOrWhiteSpace(nativeName)) return DepositCreatedResponseModel.Fail("nativeName is required");

        var returnUrl = request.GetValueOrDefault("returnUrl");
        if (string.IsNullOrWhiteSpace(returnUrl)) return DepositCreatedResponseModel.Fail("returnUrl is required");

        var options = GPayOptions.FromJson(method.Configuration);
        var client = new GPay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = returnUrl,
            Ip = request.GetValueOrDefault("ip") ?? string.Empty,
            NativeName = nativeName,
            Options = options,
            Logger = logger,
            Client = clientFactory.CreateClient()
        };

        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessPaymentAsiaRMBAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var user = await userSvc.GetPartyAsync(account.PartyId);
        var options = PaymentAsiaRMBOptions.FromJson(method.Configuration);
        var client = new PaymentAsiaRMB.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            Language = user.Language,
            Email = user.EmailRaw,
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            ReturnFailUrl = request.GetValueOrDefault("returnFailUrl") as string ?? string.Empty,
            NativeName = user.GuessNativeName(),
            RmbOptions = options,
            Logger = logger,
        };
        
        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessDragonPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = DragonPayPHPOptions.FromJson(method.Configuration);
        var user = await userSvc.GetPartyAsync(account.PartyId);

        var client = new DragonPayPHP.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            Email = user.EmailRaw!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.PhoneNumberRaw,
            IpAddress = request.GetValueOrDefault("ip") as string ?? string.Empty,
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            Options = options,
            Logger = logger,
        };

        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessHelp2PayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var targetCurrencyId = request.TryGetValue("currencyId", out var valRaw) && int.TryParse(valRaw, out var val)
            ? (CurrencyTypes)val
            : (CurrencyTypes)method.CurrencyId;
        var options = Help2PayOptions.FromJson(method.Configuration);
        var user = await userSvc.GetPartyAsync(account.PartyId);
        var client = new Help2Pay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            AccountUid = account.Uid,
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            Currency = targetCurrencyId,
            Ip = request.GetValueOrDefault("ip") as string ?? string.Empty,
            Language = user.Language,
            Bank = "",
            Options = options,
            Logger = logger,
        };

        return await client.RequestAsync();
    }

    private async Task<DepositCreatedResponseModel> ProcessUEnjoyAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var kycInfos = UEnjoy.KycInfos.FromDict(request);
        var user = await userSvc.GetPartyAsync(account.PartyId);
        // var userAreaCode = await 
        var option = UEnjoyOptions.FromJson(method.Configuration);
        var client = new UEnjoy.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            Logger = logger,
            KycName = kycInfos.NativeName,
            UserAreaCode = user.CCC,
            UserMobile = kycInfos.UserMobile,
            Options = option,
            Client = clientFactory.CreateClient(),
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
        };

        return await client.RequestAsync();
    }

    private async Task<DepositCreatedResponseModel> ProcessPay247Async(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var targetCurrency = request.TryGetValue("currencyId", out var valRaw) && int.TryParse(valRaw, out var val)
            ? (CurrencyTypes)val
            : (CurrencyTypes)method.CurrencyId;
        var option = Pay247Options.FromJson(method.Configuration);
        var client = new Pay247.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            AccountUid = account.Uid,
            PaymentNumber = Payment.GenerateNumber(),
            CurrencyId = targetCurrency,
            Logger = logger,
            Options = option,
            Client = clientFactory.CreateClient(),
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
        };

        return await client.RequestAsync();
    }

    private async Task<DepositCreatedResponseModel> ProcessBigPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var option = BigPayOptions.FromJson(method.Configuration);
        var client = new BigPay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            AccountId = account.Uid,
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            CancelUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            Options = option,
            Client = clientFactory.CreateClient(),
            Logger = logger
        };

        return await client.RequestAsync();
    }

    private async Task<DepositCreatedResponseModel> ProcessPoliPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var option = PoliOptions.FromJson(method.Configuration);
        var client = new PoliPay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            CurrencyId = (CurrencyTypes)account.CurrencyId,
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            Client = clientFactory.CreateClient(),
            Options = option
        };
        return await client.RequestAsync();
    }

    private async Task<DepositCreatedResponseModel> ProcessBipiPayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var options = BipiPayOptions.FromJson(method.Configuration);
        var nativeName = request.GetValueOrDefault("nativeName");
        if (string.IsNullOrWhiteSpace(nativeName))
        {
            var user = await userSvc.GetPartyAsync(account.PartyId);
            nativeName = user.GuessNativeName();
        }
        // var user = await userService.GetPartyAsync(account.PartyId);
        var client = new BipiPay.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            UsdAmount = 0,
            PaymentNumber = Payment.GenerateNumber(),
            NativeName = nativeName,
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            CurrencyId = (CurrencyTypes)account.CurrencyId,
            Options = options
        };
        var response = await client.RequestAsync();
        return response;
    }

    private async Task<DepositCreatedResponseModel> ProcessLong77PayAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var option = Long77PayOptions.FromJson(method.Configuration);
        option.TenantId = _tenantId;

        // Check if Virtual Account flow is requested (via request parameter or if VAEndpoint is configured)
        var useVA = request.GetValueOrDefault("useVA", "false").ToLower() == "true" ||
                    !string.IsNullOrEmpty(option.VAEndpoint);

        if (useVA && !string.IsNullOrEmpty(option.VAEndpoint))
        {
            // Virtual Account flow
            var vaAmount = RoundUp(exchangedAmount / 100m); // Convert from cents to VND

            // Validate amount range: 50,000 - 4,000,000,000 VND
            if (vaAmount < 50000 || vaAmount > 4000000000)
            {
                return DepositCreatedResponseModel.Fail(
                    $"Amount must be between 50,000 and 4,000,000,000 VND. Current: {vaAmount:N0}");
            }

            // Generate payment number in Long77 Pay format (no hyphens allowed)
            // Format: Deposit{timestamp}{random} - matches test format Test{timestamp}{random}
            // Example: Deposit2025111712442054267
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000000, 9999999).ToString();
            var paymentNumber = $"Deposit{timestamp}{random}";

            var vaRequest = new Long77PayVARequestModel
            {
                Amount = (long)vaAmount,
                PaymentNumber = paymentNumber,
                ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
                CustomerName = request.GetValueOrDefault("customerName") ?? string.Empty,
                PayeeName = request.GetValueOrDefault("payeeName") ?? string.Empty,
                ExtraData = request.GetValueOrDefault("extraData") ?? string.Empty,
                Ip = request.GetValueOrDefault("ip") ?? string.Empty // IP address from request
            };
            vaRequest.ApplyOptions(option);
            
            // Validate request before sending
            var validator = new Long77PayVARequestModelValidator();
            var validationResult = await validator.ValidateAsync(vaRequest);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                logger.LogWarning("Long77Pay VA Request validation failed: {Errors}", errors);
                return DepositCreatedResponseModel.Fail($"Request validation failed: {errors}");
            }

            var signedForm = vaRequest.Sign();
            logger.LogInformation("Long77Pay VA Request - PaymentNumber: {PaymentNumber}, Form: {Form}", paymentNumber, signedForm);
            
            // Log partner_order_code specifically to debug
            if (signedForm.ContainsKey("partner_order_code"))
            {
                logger.LogInformation("Long77Pay VA Request - partner_order_code value: '{Value}'", signedForm["partner_order_code"]);
            }
            else
            {
                logger.LogError("Long77Pay VA Request - partner_order_code is MISSING from form!");
            }

            var content = new StringContent(JsonConvert.SerializeObject(signedForm), Encoding.UTF8, "application/json");
            var response = await clientFactory.CreateClient().PostAsync(option.VAEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Long77Pay VA Request failed: {StatusCode}", response.StatusCode);
                return DepositCreatedResponseModel.Fail("Failed to create virtual account");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            logger.LogInformation("Long77Pay VA Response: {Response}", responseJson);

            var obj = Utils.JsonDeserializeDynamic(responseJson);
            var vaResponse = Long77PayVAResponseModel.FromDynamic(obj);

            if (!vaResponse.IsSuccess)
            {
                return DepositCreatedResponseModel.Fail(vaResponse.Message ?? "Failed to create virtual account");
            }

            // ? Cast dynamic values to string to fix CS1973 error
            var paymentUrl = (string?)vaResponse.PaymentUrl ?? "(null)";
            var systemOrderCode = (string?)vaResponse.SystemOrderCode ?? "(null)";
            var partnerOrderCode = (string?)vaResponse.PartnerOrderCode ?? "(null)";

            // Detailed logging for payment URL troubleshooting
            logger.LogInformation("Long77Pay VA Response Details - PaymentUrl: {PaymentUrl}, SystemOrderCode: {SystemOrderCode}, PartnerOrderCode: {PartnerOrderCode}",
                paymentUrl,
                systemOrderCode,
                partnerOrderCode);

            // Check if payment URL contains placeholder domains (indicates API/config issue)
            if (!string.IsNullOrEmpty(paymentUrl))
            {
                if (paymentUrl.Contains("xxxxx.com") || 
                    paymentUrl.Contains("placeholder") ||
                    paymentUrl.Contains("example.com"))
                {
                    logger.LogWarning("Long77Pay VA Payment URL contains placeholder domain: {PaymentUrl}. " +
                        "This suggests an API configuration issue. Check return_url in request: {ReturnUrl}",
                        paymentUrl, vaRequest.ReturnUrl);
                }
                
                // Log request details for troubleshooting
                logger.LogInformation("Long77Pay VA Request Details - ReturnUrl: {ReturnUrl}, NotifyUrl: {NotifyUrl}, Endpoint: {Endpoint}",
                    vaRequest.ReturnUrl,
                    option.VACallbackUri,
                    option.Endpoint);
            }
            else
            {
                logger.LogWarning("Long77Pay VA Payment URL is NULL or EMPTY in API response");
            }

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = vaResponse.PaymentUrl,
                Reference = vaResponse.SystemOrderCode ?? string.Empty,
                PaymentNumber = vaRequest.PaymentNumber
            };
        }
        else
        {
            // Existing flow (keep for backward compatibility)
            var client = new Long77Pay.RequestClient
            {
                Amount = RoundUp(exchangedAmount / 100m),
                PaymentNumber = Payment.GenerateNumber(),
                ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
                Logger = logger,
                Client = clientFactory.CreateClient(),
                Options = option
            };
            return await client.RequestAsync();
        }
    }

    private async Task<DepositCreatedResponseModel> ProcessLong77PayUsdtAsync(PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        var option = Long77PayOptions.FromJson(method.Configuration);
        var client = new Long77PayUsdt.RequestClient
        {
            Amount = RoundUp(exchangedAmount / 100m),
            PaymentNumber = Payment.GenerateNumber(),
            ReturnUrl = request.GetValueOrDefault("returnUrl") ?? string.Empty,
            TargetAccountUid = account.Uid,
            Client = clientFactory.CreateClient(),
            Options = option
        };
        return await client.RequestAsync();
    }

    private async Task<DepositCreatedResponseModel> ProcessBuzipayAsync(
        PaymentMethod method,
        Account account,
        long exchangedAmount,
        Dictionary<string, string> request)
    {
        var options = BuzipayOptions.FromJson(method.Configuration);
        if (!options.IsValid())
        {
            return DepositCreatedResponseModel.Fail("Invalid Buzipay configuration");
        }
        
        // Get or create Buzipay customer ID for card saving
        var customerService = new Services.Buzipay.BuzipayCustomerService(tenantCtx, logger, clientFactory);
        var customerId = await customerService.GetOrCreateCustomerIdAsync(account.PartyId, options);
        
        // Create request client
        var client = new Vendor.Buzipay.Buzipay.RequestClient
        {
            PaymentNumber = Payment.GenerateNumber(),
            Amount = RoundUp(exchangedAmount / 100m),
            Currency = "USD",  // Map from method.CurrencyId as needed
            Description = $"Deposit to account {account.AccountNumber}",
            CustomerId = customerId,
            Options = options,
            Client = clientFactory.CreateClient(),
            Logger = logger
        };
        
        return await client.RequestAsync();
    }

    private static async Task<DepositCreatedResponseModel> ProcessOcexAsync(
        PaymentMethod method
        , Account account
        , long exchangedAmount
        , Dictionary<string, string> request)
    {
        await Task.Delay(0);
        return DepositCreatedResponseModel.Fail("Not implemented");
    }
}